//#define TEST_EDITOR
#define DEBUG_DATA_CHECK
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace BKKZ.POW01{
    public enum eMapDataState{
		Loading =0,
		Load,
		Uncreated,
		Dirt,
		Saved
	}
	public enum eBrushState{
		Grid = 0,
		Unit,
		Event,
		GlobalSwitch,
        Area,       //区域，用于探索时开房区域和战斗时限制区域
		Max
	}
    public class MapEditor : EditorWindow {
        static MapEditor instance = null;
        public static MapEditor Inst {
            get {
                if (instance == null)
                    ShowWindow();
                return instance;
            }
        }
        public static string LevelDir {
            get { return "Assets/Res/PvELevel/"; }
        }
        //public GUIContent titleContent = new GUIContent("BK地图编辑器");
        public bool isInit = false;
        //编辑器多选
        public ChessContainer SelectGrid;
        //坑
        //		SceneView scene_view = null;	//避免多次使用getwindow<sceneview>
        //编辑器显示内容
        eMapDataState my_mapDateState = eMapDataState.Uncreated;
        string[] strMapDataState;// = new string[]{TextUnit.BKME_State_Loading, TextUnit.BKME_State_Load, TextUnit.BKME_State_Uncreated, TextUnit.BKME_State_Dirt, TextUnit.BKME_State_Save};
                                 // 显示⬆️+数据⬇️
                                 //		SerializedObject s_this;
        public EventList my_eventlist;
        public UnitList my_unitlist;//鉴于实际编写中使用字典形式，对应到ReorderableList的结构多一步操作，故此变量只做显示
        public AreaList my_arealist;
        //		ReorderableList gui_eventlist;

        public static LevelMapData my_map_data = null;
        public LevelController lv_ctrl = null;

        //  <编辑状态数据>
        public string[] arr_grid_id;
        static int max_grid_number = 0;
        public int SelectGridId {
            get {
                if (SelectGrid != null)
                    return SelectGrid.number;
                else
                    return 0;
            }
        }
        public string my_mapData_Source;//检测地图来源，用于判断修改和新建
        //public Dictionary<int, List<UnitInfo>> mydic_grid_unit;//编辑用，存储时进行翻译
        string map_rename = "";
        int[] map_resize;
        //string[] _map_resize_s;//感觉可以保证效率
        //笔刷
        string[] editor_state;  //编辑层（名字）
        eBrushState my_editor_state = eBrushState.Unit; //编辑层（格子、单位、buff）
        GUIContent[] brush_btn;
        string[] brush_name;	//笔刷名字
        string[] str_area_brush_name;//区域笔刷名字
        int my_brush_select = 0;//笔刷位置  -- 
#if TEST_EDITOR
		//临时测试
		string myString = "Hello World";
		bool groupEnabled;
		bool myBool = true;
		float myFloat = 1.23f;
		bool toggle1 = false;
		bool toggle2 = false;
#endif

        //滚动位置记录
        Vector2 scrollPos = new Vector2();
        void OnGUI() {
            if (!isInit) {
                GUILayout.Label("Loading");
                return;
            }
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width));
            // The actual window code goes here
            //TODO 显示选择地形的类型
            //关卡状态
            GUI_MapInfo();
            //笔刷工具
            if (my_map_data != null) {
                GUILayout.BeginHorizontal();
                GUILayout.Label("编辑层：");
                my_editor_state = (eBrushState)GUILayout.Toolbar((int)my_editor_state, editor_state);
                GUILayout.EndHorizontal();
                switch (my_editor_state) {
                case eBrushState.Grid:
                    GUI_GridBrush();
                    break;
                case eBrushState.Unit:
                    //TODO 包装个显示卡片信息的
                    //if (!my_map_data.mydic_grid_unit.ContainsKey(grid_id))
                    //    my_map_data.mydic_grid_unit.Add(grid_id, new List<UnitInfo>());
                    //List<UnitInfo> list = my_map_data.mydic_grid_unit[grid_id];
                    GUI_UnitEdit();
                    break;
                case eBrushState.Event:
                    GUI_EventEdit();
                    break;
                case eBrushState.GlobalSwitch:
                    GUI_GlobalSwitch();
                    break;
                case eBrushState.Area:
                    GUI_AreaEdit();
                    break;
                default:
                    Debug.Log("编辑器笔刷状态错误");
                    break;
                }
            }
            //编辑对象信息
            GUI_select_info();
            //鼠标事件
            UpdateMouseInputs();
#if TEST_EDITOR
			GUILayout.Label ("Base Settings", EditorStyles.boldLabel);
			myString = EditorGUILayout.TextField ("Text Field", myString);

			groupEnabled = EditorGUILayout.BeginToggleGroup ("Optional Settings", groupEnabled);
			myBool = EditorGUILayout.Toggle ("Toggle", myBool);
			myFloat = EditorGUILayout.Slider ("Slider", myFloat, -3, 3);
			EditorGUILayout.EndToggleGroup();

			toggle1 = EditorGUILayout.Toggle ("group1", toggle1);
			if(toggle1)
				GUILayout.Label ("  艾玛");
			toggle2 = EditorGUILayout.Toggle ("group", toggle2);
			if (toggle2)
			 	GUILayout.Label ("  爱马仕");
#endif
            //EditorUtility.SetDirty(this);
            EditorGUILayout.EndScrollView();
        }
        //自定义托管事件，为了取消笔刷的选择
        void SceneGUI(SceneView sceneView) {
            //编辑单位或Buff不允许多选
            if (my_editor_state == eBrushState.Unit) {
                if (Selection.objects.Length > 1) {
                    Object obj = Selection.objects[0];
                    Selection.objects = new Object[1] { obj };
                }
                return;
            }
            Brush_in_Scene();
        }
        void Brush_in_Scene() {
            if (my_editor_state != eBrushState.Area && my_editor_state != eBrushState.Grid)
                return;
            if (my_editor_state == eBrushState.Grid && my_brush_select == 0)
                return;
            //SenceGUI 减少非必要事件响应
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlID);
            EventType currentEventType = Event.current.GetTypeForControl(controlID);
            bool skip = false;
            int saveControl = GUIUtility.hotControl;

            if (currentEventType == EventType.Layout) { skip = true; } else if (currentEventType == EventType.ScrollWheel) { skip = true; }

            if (!skip) {
                //自制鼠标事件
                //				Rect rSceneView = new Rect( 0, 0, Screen.width, Screen.height );
                Rect rSceneView = SceneView.currentDrawingSceneView.position;
                //				Debug.Log (SceneView.currentDrawingSceneView.position);
                rSceneView.x = 0; rSceneView.y = 0;
                //				string str = "rSceneView="+rSceneView.ToString()+"\r\n";
                if (rSceneView.Contains(Event.current.mousePosition)) {
                    //					str += "鼠标位置="+Event.current.mousePosition;
                    //					Debug.Log (str);
                    //					return;
                    UpdateMouseInputs();
                    //抛弃选择
                    if (m_isMouseLeftDown)
                        Selection.objects = new Object[1];
                    //笔刷涂抹
                    Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                    //					Plane hPlane = new Plane (Vector3.forward, Vector3.zero);		
                    //					float distance = 0; 
                    if (m_isMouseRight || m_isMouseLeft) {
                        //						Debug.Log (Event.current.mousePosition);
                        //						Debug.Log(GetWindow<SceneView> ().position);
                        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
                        if (hit.collider != null) {
                            ChessContainer grid = hit.collider.GetComponent<ChessContainer>();
                            if (grid != null) {

                                switch (my_editor_state) {
                                case eBrushState.Grid:
                                    //修改地形
                                    int select_grid_type = my_brush_select - 1;
                                    my_map_data.my_grid_t_type[grid.number] = (eMapGridType)select_grid_type;
                                    //                              grid.GetComponent<SpriteRenderer> ().color = grid_color [select_grid_type];
                                    setGridDisplay(grid);
                                    break;
                                case eBrushState.Area:
                                    if (my_map_data.list_area_grid.Count <= AreaList.now_area_index || my_map_data.list_area_grid[AreaList.now_area_index].list == null)
                                        break;
                                    if (my_brush_select == 0 && my_map_data.list_area_grid[AreaList.now_area_index].list.Contains(grid.number))
                                        my_map_data.list_area_grid[AreaList.now_area_index].list.Remove(grid.number);
                                    else if (my_brush_select == 1 && !my_map_data.list_area_grid[AreaList.now_area_index].list.Contains(grid.number))
                                        my_map_data.list_area_grid[AreaList.now_area_index].list.Add(grid.number);
                                    ResetAreaView();
                                    break;
                                default:
                                    break;
                                }

                            }
                        }
                    }
                }
            }
        }
        void GUI_select_info() {
            if (my_map_data == null)
                return;

            if (SelectGridId == -1)
                GUILayout.Label("当前格子:无效");
            else
                GUILayout.Label("当前格子:" + SelectGridId);


            //GUILayout.Label (select_1.name);


            switch (my_editor_state) {
            case eBrushState.Grid:
                if (SelectGrid == null) {
                    break;
                }
                GUILayout.Label("地形:" + SelectGrid.terrain_type.ToString());
                break;
            case eBrushState.Unit:
                //GUILayout.Label (select_unit.my_card_info.name + "(id:" + select_unit.my_card_info.id + ")");
                break;
            case eBrushState.Event:
                //TODO 放到替换笔刷的地方吧
                //GUILayout.Label(select_buff.my_buffs.name+"(id:"+select_unit.my_card_info.id+")");
                break;
            case eBrushState.GlobalSwitch:
                break;
            case eBrushState.Area:
                break;
            default:
                Debug.Log("笔刷状态错误");
                break;
            }
        }
        //		bool b_fresh = false;
        void GUI_MapInfo() {
            //地图状态
            GUILayout.Label(TextUnit.BKME_DataState + strMapDataState[(int)my_mapDateState]);

            //编辑中的地图ScriptableObject对象
            my_map_data = EditorGUILayout.ObjectField(my_map_data, typeof(LevelMapData), false) as LevelMapData;
            if (GUI.changed) {
                InitAfterGetData(false);
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("关卡（" + (my_map_data != null ? my_map_data.mapName : TextUnit.BKME_State_Uncreated) + "）");
            //Debug.Log (levelname_rename);
            map_rename = GUILayout.TextField(map_rename);
            //Debug.Log (levelname_rename);
            //				text = GUI.TextField(..., text);
            //				text = Regex.Replace(text, @"[^a-zA-Z0-9 ]", "");
            GUILayout.EndHorizontal();

            //尺寸
            //			EditorGUILayout.BeginHorizontal();
            GUILayout.BeginHorizontal();
            if (my_map_data != null) {
                GUILayout.Label("尺寸:(" + my_map_data.my_size[0] + " x " + my_map_data.my_size[1] + ")");
                //				_map_resize_s[0]=GUILayout.TextField (_map_resize_s[0]);
                map_resize[0] = EditorGUILayout.IntField(map_resize[0]);
                GUILayout.Label("x");
                //				_map_resize_s[1]=GUILayout.TextField (_map_resize_s[1]);
                map_resize[1] = EditorGUILayout.IntField(map_resize[1]);
                if (my_map_data.my_size[0] != map_resize[0] || my_map_data.my_size[1] != map_resize[1]) {
                    if (GUILayout.Button("修改")) {
                        bool confirm = true;
                        //边界检测
                        if (Resize_Check()) {
                            confirm = EditorUtility.DisplayDialog(TextUnit.Dialog_Title_Common, TextUnit.Dialog_ResizeMap, TextUnit.Dialog_OK_Common, TextUnit.Btn_Cancel);
                        }
                        if (confirm) {
                            EditorApplication.delayCall += Resize_Map;
                        }
                    }
                }
            }
            GUILayout.EndHorizontal();
            //文件操作
            GUILayout.BeginHorizontal();
            {
                //创建 ();
                if (GUILayout.Button(TextUnit.Btn_Create)) {
                    //GUI.Button ();
                    CreateMapDate();
                }
                //加载
                //if (GUILayout.Button (TextUnit.Btn_Load)) {
                //	EditorApplication.delayCall += UI_LoadMapDate;
                //}
                if (GUILayout.Button(TextUnit.Btn_Save)) {
                    SaveData();
                }
                //关闭
                /*if (GUILayout.Button(TextUnit.Btn_Close)) {
                    //SaveMapDate ();
                    ClearScene();
                    SaveData();
                    my_map_data = null;
                }*/
            }
            GUILayout.EndHorizontal();

#if DEBUG_DATA_CHECK
            Debug_Btn_1();
#endif
        }
        //让距离够近的格子自动对齐，关联移动关系
        void RelateAllGrids() {
            ChessContainer grid;
            Vector3 checkpos = Vector3.zero; ;
            foreach (var item in lv_ctrl.list_grid) {
                //以这个格子为中心，搜索上方【三】个方向上应该有东西的地方，是否有grid
                RelateGrid(item, eDirection.UpperRight, eDirection.Upper, eDirection.UpperLeft);
            }
            //标记修改
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
        void RelateGrid(ChessContainer item, params eDirection[] dirs) {
            Vector3 checkpos = new Vector3(0, BKTools.chess_container_size.y, 0) * item.transform.lossyScale.x;
            Vector3 off;
            ChessContainer grid;
            foreach (var dir in dirs) {
                off = Quaternion.Euler(0, 0, BKTools.AngularByDirection(dir)) * checkpos;
                Ray ray = new Ray(item.transform.position + off, Vector3.forward);
                //						Debug.Log (Event.current.mousePosition);
                //						Debug.Log(GetWindow<SceneView> ().position);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
                if (hit.collider != null) {
                    grid = hit.collider.GetComponent<ChessContainer>();
                    if (grid != null)
                        item.RelateTo(grid, dir);
                }
            }
        }
        void SaveData() {
            EditorUtility.SetDirty(my_map_data);
            AssetDatabase.SaveAssets();
            //foreach (var item in my_map_data.list_area_grid) {
            //item.list_ser = item.list.ToArray();
            //}
        }
#if DEBUG_DATA_CHECK
        void Debug_Btn_1() {
            if (GUILayout.Button("Debug")) {
                EditorApplication.delayCall += Debug_1;
            }
        }
        /// <summary>
        /// 地图编辑器内数据调试
        /// </summary>
		static void Debug_1() {
#if UNITY_EDITOR
            Debug.Log("测试中，不用担心代码再正式游戏中执行");
#endif
            //			Debug.Log(my_eventlist.s_inst.FindProperty ("datalist").isArray);
            //			Debug.Log (my_eventlist.m_GUIList.serializedProperty.isArray);
            //			Debug.Log (my_eventlist.m_GUIList.serializedProperty == my_eventlist.s_inst.FindProperty ("datalist"));
            //			return;
            //			EventInfo _info = new EventInfo ();
            //			SerializedObject _s =  new UnityEditor.SerializedObject(this);
            ////			SerializedProperty ppt = _s.FindProperty ("event_id");
            //			SerializedProperty ppt = _s.FindProperty ("brush_state");
            //			Debug.Log (ppt);
            //			return;


            //			DramaScript script = new DramaScript ();
            EventInfo _event = new EventInfo();
            DramaSection section = new DramaSection();
            _event.drama_script.Add(section);
            section = new DramaSection();
            _event.drama_script.Add(section);

            Debug.Log(JsonUtility.ToJson(_event));
            Debug.Log(JsonUtility.ToJson(_event.drama_script));

            //Debug.Log(JsonConvert.SerializeObject(_event));
            //Debug.Log(JsonConvert.SerializeObject(_event.drama_script));
            //			DramaScript _copy = JsonUtility.FromJson<DramaScript> (JsonUtility.ToJson(_event.drama_script));
            return;


            if (my_map_data == null) {
                Debug.Log("地图数据不存在");
                return;
            }
            Debug.Log(JsonUtility.ToJson(my_map_data));

            //			Debug.Log (JsonUtility.ToJson (my_eventlist.datalist[0]));
            //CombineLvData ();
        }
#endif
        void GUI_GridBrush() {
            //GUILayout.Label ("笔刷");

            GUILayout.Label("拓展棋盘:");
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {
                    //第一行
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("↖")) {
                        ExtendGroupGroup(eDirection.UpperLeft);
                    }
                    if (GUILayout.Button("↑")) {
                        ExtendGroupGroup(eDirection.Upper);
                    }
                    if (GUILayout.Button("↗")) {
                        ExtendGroupGroup(eDirection.UpperRight);
                    }
                    GUILayout.EndHorizontal();
                    //第二行
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("↙")) {
                        ExtendGroupGroup(eDirection.LowerLeft);
                    }
                    if (GUILayout.Button("↓")) {
                        ExtendGroupGroup(eDirection.Lower);
                    }
                    if (GUILayout.Button("↘")) {
                        ExtendGroupGroup(eDirection.LowerRight);
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                {
                    if (GUILayout.Button("Del")) {
                        ChessContainer cc;
                        foreach (var item in Selection.gameObjects) {
                            cc = item.GetComponent<ChessContainer>();
                            lv_ctrl.list_grid.Remove(cc);
                            DestroyImmediate(cc.gameObject);
                        }
                    }
                    if (GUILayout.Button("关联")) {
                        foreach (var item in Selection.gameObjects) {
                            ChessContainer cc = item.GetComponent<ChessContainer>();
                            if (cc != null)
                                RelateGrid(cc, eDirection.UpperLeft, eDirection.LowerRight, eDirection.Upper, eDirection.Lower, eDirection.UpperRight, eDirection.LowerLeft);
                        }
                    }
                    if (GUILayout.Button("对齐")) {
                        foreach (var item in Selection.gameObjects) {
                            ChessContainer cc = item.GetComponent<ChessContainer>();
                            if (cc != null)
                                foreach (var dir in ChessContainer.AlignOrder) {
                                    if (cc.AlignGrid(dir))
                                        break;
                                }
                        }

                    }
                }
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                {
                    //矫正关联
                    if (GUILayout.Button("关联临近格子")) {
                        RelateAllGrids();
                    }
                    //矫正关联
                    if (GUILayout.Button("检查格子ID")) {
                        foreach (var item in lv_ctrl.list_grid) {
                            foreach (var target in lv_ctrl.list_grid) {
                                if (item.number == target.number && item != target)
                                    item.number = max_grid_number++;
                            }
                        }
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            //过时的 
            my_brush_select = GUILayout.Toolbar(my_brush_select, brush_name);
            if (GUI.changed && my_brush_select != 0) {
                ChessContainer obj;
                foreach (var item in Selection.objects) {
                    obj = ((GameObject)item).GetComponent<ChessContainer>();
                    if (obj != null) {
                        obj.terrain_type = (eMapGridType)(my_brush_select - 1);
                        setGridDisplay(obj, obj.terrain_type);
                    }
                }
            }
        }
        void ExtendGroupGroup(eDirection dir) {
            List<GameObject> list = new List<GameObject>();
            foreach (var item in Selection.gameObjects) {
                ChessContainer base_grid = item.GetComponent<ChessContainer>();
                list.Add(ExtendGrid(base_grid, dir).gameObject);
            }
            //操作后统一数据
            Selection.objects = list.ToArray();
            GridsChanged();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
        ///最好不要单独调用
        ///比如Selection会出问题
        ChessContainer ExtendGrid(ChessContainer base_grid, eDirection dir) {
            ChessContainer new_grid = base_grid.GetAround(dir);
            if (new_grid == null) {
                new_grid = Instantiate(BKTools.getBundleObject(eResBundle.Prefabs, PrefabPath.ChessGrid)).GetComponent<ChessContainer>();
                new_grid.number = max_grid_number++;
                lv_ctrl.list_grid.Add(new_grid);
                new_grid.transform.SetParent(lv_ctrl.chess_board.transform, true);
                new_grid.transform.localScale = base_grid.transform.localScale;
                base_grid.RelateTo(new_grid, dir);
                int reverse_dir = (int)dir;
                reverse_dir = (reverse_dir / 2) * 2 + (1 - reverse_dir % 2);
                new_grid.AlignGrid((eDirection)reverse_dir);
                RelateGrid(new_grid, ChessContainer.AlignOrder);
                return new_grid;
            } else {
                return ExtendGrid(new_grid, dir);
            }
        }
        void GridsChanged() {
            arr_grid_id = (from grid in lv_ctrl.list_grid
                    select grid.number.ToString()).ToArray();
        }
        void ResetUnitView() {
            ClearUnitView();
            foreach (var unit in my_map_data.list_unit_data) {
                if (unit.m_launch_event_order == EventList.now_event_order) {
                    ShowUnitOnGrid(unit, unit.m_start_grid);
                }
            }
        }
        void ClearUnitView() {
            GameObject[] oldBoard = GameObject.FindGameObjectsWithTag("Chess");
            foreach (GameObject _obj in oldBoard) {
                DestroyImmediate(_obj);
            }
        }

        void ResetAreaView() {
            ClearAreaView();

            GameObject t = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BK/Editor/Prefabs/chess_grid_area_mark.prefab");
            //多区域同时显示
            for (int i = 0; i < my_arealist.datalist.Count; i++) {
                if (!my_arealist.datalist[i])
                    continue;
                foreach (var item in my_map_data.list_area_grid[i].list) {
                    GameObject obj = Instantiate(t, lv_ctrl.list_grid[item].transform);
                    obj.transform.localPosition = Vector3.back;
                }
            }
        }
        void ClearAreaView() {
            GameObject[] oldAreaMark = GameObject.FindGameObjectsWithTag("Editor_Area_Mark");
            foreach (GameObject _obj in oldAreaMark) {
                DestroyImmediate(_obj);
            }
        }
        void ShowUnitOnGrid(UnitInfo unit, int grid_number) {
            if (my_map_data == null)
                return;
            //计算位置
            Vector3 new_pos = Vector3.zero;
            //             Vector3 new_pos = getPosition(Vector3.zero, grid_number / my_map_data.my_size[1], grid_number % my_map_data.my_size[0]);
            //             bool b_odd = (grid_number / my_map_data.my_size[1]) % 2 == 0;//奇数列不加
            //             new_pos.y += b_odd ? 0 : BKTools.chess_container_size.y / 2;
            foreach (var item in lv_ctrl.list_grid) {
                if (item.number == grid_number)
                    new_pos = item.transform.position;
            }
            //创建棋子
            Chess newchess = Instantiate(BKTools.getBundleObject(eResBundle.Prefabs,PrefabPath.Chess)).GetComponent<Chess>();
            newchess.attribute.card_id = unit.m_unit_id;
            newchess.transform.parent = lv_ctrl.chess_board.transform;
            newchess.transform.position = Vector3.back + new_pos;
            newchess.transform.localScale = Vector3.one;
            //			newchess.GetComponent<Chess>().refMain = this;
            //同步棋子信息
            //初始化数据
            newchess.initData();
            //初始化图片
            newchess.initImage();

            newchess.container = lv_ctrl.list_grid[grid_number];
        }
        void GUI_UnitEdit() {
            //if (my_unitlist.datalist != list) {
            //	my_unitlist = new UnitList ();
            //	my_unitlist.initData ();
            //}
            //			GUILayout.Label ("单位(ID,类型,出现事件)");
            my_brush_select = 0;
            //list.Sort ();
            //my_map_data.list_unit_data.Sort(); 
            if (SelectGridId != UnitList.now_grid_for)
                my_unitlist.initData();
            my_unitlist.OnInspectorGUI();
            if (my_unitlist.datalist.Count <= 0)
                return;
            UnitInfo _unit = my_map_data.list_unit_data[UnitList.now_unit_order];

            /*  2017年11月18日09:27:33 全面改用XML系统
			//格子上单位详情
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.BeginVertical ();
					//骑士/buff
					GUILayout.Label("类型：\t");
					_unit.m_unit_type = (eUnitType)EditorGUILayout.EnumPopup (_unit.m_unit_type);
                    //SerializedObject so = new SerializedObject(_unit);
                    //EditorGUILayout.PropertyField(so.FindProperty("m_unit_type"));
                    //so.ApplyModifiedProperties();
					//出场事件
					GUILayout.Label("出场时机");
					_unit.m_launch_event_order = EditorGUILayout.Popup (_unit.m_launch_event_order, EventList.Instance.Arr_ID_String);
				EditorGUILayout.EndVertical ();

				EditorGUILayout.BeginVertical ();
					//对应ID
					if (_unit.m_unit_type== eUnitType.Unit) {
						GUILayout.Label ("ID：\t");
                //var prop = typeof(Data).GetField("arr_Chess_name");   TODO 这边的代码将汇总到XML编辑器中
                //string[] names = (string[])prop.GetValue(null);
                //int t_id = EditorGUILayout.IntPopup(_unit.m_unit_id, names, Data.arr_Chess_key);
						int t_id = EditorGUILayout.IntPopup (_unit.m_unit_id, Data.arr_Chess_name, Data.arr_Chess_key);
						if (_unit.m_unit_id != t_id) {
							_unit.m_unit_id = t_id;
							ResetUnitView ();
						}

						//AI类型
						GUILayout.Label ("AI：\t");
						_unit.AI_ID = (BKKZ.POW01.AI.eAI_Type)EditorGUILayout.EnumPopup (_unit.AI_ID);
						//点击事件ID
						GUILayout.Label ("ClickID：\t");
						_unit.click_ID = EditorGUILayout.IntField (_unit.click_ID);
					}
				EditorGUILayout.EndVertical ();
			EditorGUILayout.EndHorizontal ();
            */
            if (Tools.AnalyseXMLEditor(_unit))
                ResetUnitView();
            //			EditorGUI.PropertyField (new Rect(elemX,rect.y,elemWidth,rect.height)
            //				, itemData.FindPropertyRelative ("m_unit_type"), GUIContent.none);
            //			string[] arr_Chess_name = (from card_info in Data.Inst.card_data
            //				select card_info.Value.name).ToArray();
            //			int[] arr_Chess_key = (from card_info in Data.Inst.card_data
            //				select card_info.Key).ToArray();
            //			itemData.FindPropertyRelative ("m_unit_id").intValue = EditorGUI.IntPopup (new Rect (elemX, rect.y, elemWidth, rect.height)
            //				, itemData.FindPropertyRelative ("m_unit_id").intValue, arr_Chess_name, arr_Chess_key);


            //			string[] arr_event_name = EventList.Instance.arr_id_str;
            //			itemData.FindPropertyRelative ("m_launch_event_order").intValue = EditorGUI.Popup (new Rect (elemX, rect.y, elemWidth, rect.height)
            //				, itemData.FindPropertyRelative ("m_launch_event_order").intValue, arr_event_name);
        }
        void GUI_GlobalSwitch() {
            EditorGUILayout.LabelField("全局开关");
            for (int i = 1; i < my_map_data.global_switch.Length; i++) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(i + ":\t");
                my_map_data.global_switch[i] = EditorGUILayout.TextField(my_map_data.global_switch[i]);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.LabelField("全局变量");
            for (int i = 1; i < my_map_data.global_variable.Length; i++) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(i + ":\t");
                my_map_data.global_variable[i] = EditorGUILayout.TextField(my_map_data.global_variable[i]);
                EditorGUILayout.EndHorizontal();
            }
        }
        //剧本编辑器 手选各自标记
        public static bool bSelectSenceGrid = false;
        public static System.Action<int> dSelectCallback;
        void GUI_EventEdit() {
            EventInfo edit_event = my_map_data.list_event_data[EventList.now_event_order];
            //			GUILayout.Label ("Buff");
            my_brush_select = 0;
            //			gui_eventlist.DoLayoutList ();
            my_eventlist.OnInspectorGUI();
            // 根据当前选择的事件，单独绘制这个事件的编辑

            // EditorGUILayout.PropertyField(EventList.Instance.m_GUIList.serializedProperty.GetArrayElementAtIndex(EventList.now_event_order),true);
            //			return;
            //			bool check = EditorGUILayout.Toggle ("全局开关", edit_event.condition.gs_1 != -1);
            //			if (check && edit_event.condition.gs_1 == -1)
            //				edit_event.condition.gs_1 = 0;
            //EditorGUILayout.BeginVertical(GUILayout.Width(250));

            Tools.CheckBox("前置事件", ref edit_event.condition.pre_event_id, my_eventlist.Arr_ID_String, 0, -1);
            if (edit_event.condition.pre_event_id >= 0)
                edit_event.condition.pre_event_launchtimes = EditorGUILayout.IntField("\t|-触发几次", edit_event.condition.pre_event_launchtimes);


            edit_event.condition.trigger_evt = (eEventTrigger)EditorGUILayout.EnumPopup("触发时机", edit_event.condition.trigger_evt);
            edit_event.condition.launch_phase = (ePhaseType)EditorGUILayout.EnumPopup("阶段限制", edit_event.condition.launch_phase);
            switch (edit_event.condition.trigger_evt) {
            case eEventTrigger.Kights_Stand:
                //站立/贴近的格子ID
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("站到 " + edit_event.condition.trig_argu + "区域时\t\t");
                //				bool check = EditorGUILayout.Toggle ("格子", edit_event.condition.trig_argu >= 0);
                //				if (check) {
                if (edit_event.condition.trig_argu < 0)
                    edit_event.condition.trig_argu = Mathf.Abs(edit_event.condition.trig_argu);
                edit_event.condition.trig_argu = EditorGUILayout.Popup("区域", edit_event.condition.trig_argu, AreaList.arr_string_id);
                //				} else {
                //					if (edit_event.condition.trig_argu > 0) {
                //						edit_event.condition.trig_argu = -Mathf.Abs (edit_event.condition.trig_argu);
                //					} else if (edit_event.condition.trig_argu == 0)
                //						edit_event.condition.trig_argu = -1;
                //				}
                EditorGUILayout.EndHorizontal();
                GUILayout.Label("谁的骑士可以触发这个事件:");
                GUIStyle style = new GUIStyle(GUI.skin.box);
                Color _bak = GUI.backgroundColor;
                GUI.backgroundColor = new Color(1, 1, 1);
                EditorGUILayout.BeginVertical(style); {
                    for (int i = 1; i < edit_event.condition.players_who_can.Length; i++) {
                        edit_event.condition.players_who_can[i] = EditorGUILayout.Toggle(((ePlayer)i).ToString(), edit_event.condition.players_who_can[i]);
                    }
                }
                EditorGUILayout.EndVertical();
                break;
            case eEventTrigger.Monter_Dead:
                edit_event.condition.trig_argu = EditorGUILayout.IntField("怪物ID", edit_event.condition.trig_argu);
                break;
            case eEventTrigger.Reach_turn:
                edit_event.condition.trig_argu = EditorGUILayout.IntField("触发回合", edit_event.condition.trig_argu);
                break;
            case eEventTrigger.Click:
                edit_event.condition.trig_argu = EditorGUILayout.IntField("点击ID", edit_event.condition.trig_argu);
                break;
            case eEventTrigger.Phase_Change:
                EditorGUILayout.LabelField("（回合切换时触发可以用于点击交互）");
                EditorGUILayout.LabelField("（由点击功能直接修改全局变量来触发）");
                break;
            default:
                break;
            }
            //trig_changc
            edit_event.condition.trig_chance = EditorGUILayout.IntField("触发次数（0无限）", edit_event.condition.trig_chance);
            Tools.CheckBox("开关1", ref edit_event.condition.gs_1, my_map_data.global_switch);
            Tools.CheckBox("开关2", ref edit_event.condition.gs_2, my_map_data.global_switch);
            if (Tools.CheckBox("变量", ref edit_event.condition.glo_v, my_map_data.global_variable)) {
                EditorGUILayout.BeginHorizontal();
                edit_event.condition.glo_v_op = (eCompareOP)EditorGUILayout.EnumPopup("\t\t", edit_event.condition.glo_v_op);
                edit_event.condition.glo_v_compare = EditorGUILayout.IntField(edit_event.condition.glo_v_compare);
                EditorGUILayout.EndHorizontal();
            }

            //			Tools.CheckBox ("独立开关", ref edit_event.condition.self_s,LevelMapData.self_switch);	//脑补了一下独立开关并用不上
            Tools.CheckBox("携带道具", ref edit_event.condition.with_item);//TODO 将来使用intpopup，从道具数据拿
            Tools.CheckBox("携带队员", ref edit_event.condition.with_char);//TODO 将来使用intpopup，从角色数据拿
            Tools.CheckBox("某事件的怪物死光", ref edit_event.condition.EventMonter_Dead, EventList.Instance.Arr_ID_String, 0, EventCondition.DEFAULT_EventMonster_Dead_UnableValue);
            GUILayout.Label("（用于刷新波次）");
            //			EditorGUILayout.Popup()
            //			SerializedProperty prop = EventList.Instance.m_GUIList.serializedProperty.GetArrayElementAtIndex(EventList.now_event_order);
            //			EditorGUILayout.PropertyField();
            //事件内容——剧情表演
            GUI_DramaList(edit_event);


            //EditorGUILayout.EndVertical ();

        }
        void GUI_DramaList(EventInfo edit_event) {

            GUILayout.Label("———————事件———————");
            if (edit_event.drama_script != null) {
                for (int i = 0; i < edit_event.drama_script.section_list.Count; i++) {
                    var section = edit_event.drama_script.section_list[i];
                    EditorGUILayout.BeginHorizontal();
                    //style
                    GUIStyle g_style = new GUIStyle(GUI.skin.button);
                    g_style.alignment = TextAnchor.MiddleLeft;
                    if (GUILayout.Button(DramaSectionWindows.Drama2String(section), g_style, GUILayout.Width(position.width - 100))) {
                        DramaSectionWindows.ShowWindow(i, section);
                    }
                    if (GUILayout.Button("↑"))
                        EventList.Now_Event.drama_script.MoveUp(i);
                    if (GUILayout.Button("↓"))
                        EventList.Now_Event.drama_script.MoveDown(i);
                    if (GUILayout.Button("delete")) {
                        EventList.Now_Event.drama_script.Remove(section);
                        //						Close ();
                        DramaSectionWindows.section = null;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            //			GUIStyle style = new GUIStyle ();
            //			style.alignment = TextAnchor.UpperRight;
            var menu = new GenericMenu();
            //草，只有单位这个是不用在这里写的

            //			menu.AddItem (new GUIContent ("Move"), 
            //				false, () => {
            //					DramaSection _ds = new DramaSection ();
            //					_ds.my_type = eDramaOperationType.Move;
            //					_ds.chess_id = 1001;	//TODO 当前事件对应的棋子列表
            //					_ds.to_grid_id = 0;		//TODO 当前选择的格子
            //					edit_event.drama_script.Add (_ds);
            //				}
            //			);
            for (int i = (int)eDramaOperationType.Move; i < (int)eDramaOperationType.Max; i++) {
                eDramaOperationType _et = (eDramaOperationType)i;
                menu.AddItem(new GUIContent(_et.ToString()),
                    false, () => {
                        DramaSection _ds = new DramaSection();
                        _ds.from_event = edit_event.getOrder();
                        _ds.my_type = _et;
                        edit_event.drama_script.Add(_ds);
                    }
                );
            }



            if (EditorGUILayout.DropdownButton(new GUIContent("Add"), FocusType.Keyboard, GUILayout.Width(50))) {
                //				edit_event.drama_script.Add ();
                menu.ShowAsContext();
            }
            //编辑内容绘制
            DramaSectionWindows.OnGUI();

        }
        void GUI_AreaEdit() {

            my_brush_select = GUILayout.Toolbar(my_brush_select, str_area_brush_name);
            //my_brush_select = 0;
            my_arealist.OnInspectorGUI();

            GUILayout.Label("点击场景地块进行编辑");
        }
        /// <summary>
        /// 2018年6月7日01:03:05 加了一层保险，如果选中的新对象不是grid，则不改变SelectGrid的指向。保证需要格子信息的功能正确
        /// </summary>
        void OnSelectionChange() {
            ChessContainer new_select = null;
            if (Selection.gameObjects.Length > 0)
                new_select = Selection.gameObjects[0].GetComponent<ChessContainer>();
            if (new_select == null || new_select == SelectGrid)
                return;
            SelectGrid = new_select;
			///编辑地图事件条件时，如果需要编辑格子的数据（变量开关），则把选定值加入事件条件数据
			if (bSelectSenceGrid) {
				if (SelectGrid != null) {
//					my_eventlist.datalist [EventList.now_event_order].condition.trig_argu = select_grid.number;
					dSelectCallback(SelectGrid.number);
					bSelectSenceGrid = false;
				}
			}
			Repaint();
		}
		void setGridDisplay(ChessContainer grid){
            setGridDisplay(grid, eMapGridType.Normal);
		}
        void setGridDisplay(ChessContainer grid, eMapGridType type) {
            Undo.RecordObject(grid, "Modify Grid");
            grid.terrain_type = type;
            grid.GetComponent<SpriteRenderer>().color = LevelMapData.Grid_Color[(int)grid.terrain_type];
        }
        void OnInspectorUpdate(){
			if (!isInit)
				return;
			//文字过滤
			map_rename  = Regex.Replace(map_rename, @"[^a-zA-Z0-9\-]", "");
//			_map_resize_s[0] = Regex.Replace(_map_resize_s[0], @"\D", "");
//			_map_resize_s[1] = Regex.Replace(_map_resize_s[1], @"\D", "");
//			map_resize[0] = int.Parse ("0"+_map_resize_s [0]);
//			map_resize[1] = int.Parse ("0"+_map_resize_s [1]);

			Repaint ();
		}
//		float t_time = 0;
		void Update(){
            //			init ();
            //			b_fresh = !b_fresh;
            //			t_time += Time.deltaTime;
            //			levelname_rename = t_time + "";

            CheckNowScene();
        }
        UnityEngine.SceneManagement.Scene nowscene;
        void CheckNowScene() {
            if (nowscene != UnityEngine.SceneManagement.SceneManager.GetActiveScene())
                nowscene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            else
                return;
            LevelController lc = GameObject.FindObjectOfType<LevelController>();
            if (lc != null) {
                this.lv_ctrl = lc;
                my_map_data = lc.map_data;
                InitAfterGetData(false);
            }
        }
		//创建地图信息
		void CreateMapDate(){
			if (my_map_data != null) {
				string content = "当前正在编辑关卡，";
				bool f_return = true;
				if (my_mapDateState == eMapDataState.Dirt)
					content += "是否放弃当前地图";
				else if (my_map_data.mapName == map_rename)
					content += "文件名重复，请另外保存";
				else
					f_return = false;
				if (f_return) {
					if (my_mapData_Source == null) {
						if (!EditorUtility.DisplayDialog (TextUnit.BKKZ_MapEditor_Title, content, TextUnit.Dialog_Drop_Common, TextUnit.Btn_Cancel))
							return;
					}
					else {
						EditorUtility.DisplayDialog (TextUnit.BKKZ_MapEditor_Title, content, TextUnit.Dialog_OK_Common);
						return;
					}
				}
			}
			if (!Regex.IsMatch (map_rename, @"[a-zA-Z0-9]")) {
				//if (levelname_rename == "") {//需要判断是否有文字
				//Debug.Log ("地图文件名无效");
                EditorUtility.DisplayDialog(TextUnit.BKKZ_MapEditor_Title, "地图文件名无效", TextUnit.Dialog_OK_Common);
                return;
			}
            if (System.IO.File.Exists("Assets/Res/PvELevel/" + map_rename + ".asset")) {
                EditorUtility.DisplayDialog(TextUnit.BKKZ_MapEditor_Title, "地图名重复", TextUnit.Dialog_OK_Common);
                return;
            }
			my_map_data = ScriptableObject.CreateInstance<LevelMapData>();

            AssetDatabase.CreateAsset(my_map_data, "Assets/Res/PvELevel/"+map_rename+".asset");
            AssetDatabase.SaveAssets();
            //EditorUtility.FocusProjectWindow();
            //Selection.activeObject = my_map_data;

            UnitInfo.Unit_Count = 0;
            Debug.Log("创建成功:" + my_map_data.mapName);
            InitAfterGetData(true);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
		//加载地图信息（用作修改）
		void UI_LoadMapDate(){
			//检查文件夹
			if (!Directory.Exists (LevelDir)) {
				EditorUtility.DisplayDialog(TextUnit.Dialog_Title_Common,TextUnit.Dialog_LevelDirectoryUncreated,TextUnit.Dialog_OK_Common);
			}
			//加载文件
			string fp = EditorUtility.OpenFilePanel (TextUnit.Dialog_OpenLevelFile, LevelDir, TextUnit.BKME_FlExt);
			LevelMapData map = LevelMapData.LoadScriptableObject(fp);
			//检查数据
			if (map == null) {
				Debug.Log (TextUnit.BKME_DataBroken);
				return;
			}
			//加载——编辑器数据初始化ReorderableList特例
			//if (!ReadUnitDateToDic (map.json_grid_unit) ||
			//	!ReadEventDataToList (map.my_event_json)) {
			//	EditorUtility.DisplayDialog (TextUnit.Dialog_Title_Common,"数据异常，加载结束",TextUnit.Dialog_OK_Common);
			//	return;
			//}
            //Arealist特例
            //my_arealist.datalist.Clear();
            //foreach (var item in map.list_area_grid) {
            //    my_arealist.datalist.Add(false);
            //}
            //my_arealist.SyncStringData();

			
			my_map_data = map;

			//初始化场景
			InitNewScene ();
			InitMapName (map.mapName);
			//显示骑士
			ResetUnitView ();
			my_mapData_Source = fp;
			my_mapDateState = eMapDataState.Load;
			map_resize [0] = my_map_data.my_size [0];
			map_resize [1] = my_map_data.my_size [1];
            //初始化各编辑器的数据
            my_eventlist.initData(my_map_data);
            //my_unitlist.initData();
            //			_map_resize_s [0] = map_resize [0].ToString ();
            //			_map_resize_s [1] = map_resize [1].ToString ();
            //Debug.Log("我有了");
        }
        void InitAfterGetData(bool isNew) {
            //出具查错（因为代码变化，老的文件中可能没有新的结构，会报错
            if(my_map_data.list_event_data==null)
                my_map_data.list_event_data = new List<EventInfo>();
            if (my_map_data.list_area_grid == null)
                my_map_data.list_area_grid = new List<AreaInfo>();
            if (my_map_data.list_unit_data == null)
                my_map_data.list_unit_data = new List<UnitInfo>();
            //单位和区域下一个编号计算
            if (isNew) {
                //初始化场景
                InitNewScene();
            } else {
                lv_ctrl.chess_board = GameObject.FindGameObjectWithTag("ChessBoard");
            }
            InitMapName(isNew ? map_rename : my_map_data.mapName);
            //初始化各编辑器的数据
            my_unitlist.initData();
            my_eventlist.initData(my_map_data);
            my_arealist.initData(my_arealist, ResetAreaView);
            lv_ctrl = GameObject.FindObjectsOfType<LevelController>()[0];

            //获取最大的grid.number。保证新建的grid.number不同
            foreach (var item in lv_ctrl.list_grid) {
                if (item.number > max_grid_number)
                    max_grid_number = item.number + 1;
            }
            GridsChanged();
            //显示骑士
            ResetUnitView();
            my_mapDateState = eMapDataState.Load;
            map_resize[0] = my_map_data.my_size[0];
            map_resize[1] = my_map_data.my_size[1];
        }
        //保存地图信息
        void SaveMapDate(){
			if (!Regex.IsMatch (map_rename, @"[a-zA-Z0-9]")) {
				//if (levelname_rename == "") {//需要判断是否有文字
				Debug.Log ("地图文件名无效");
				return;
			}
			if (my_map_data == null) {
				EditorUtility.DisplayDialog(TextUnit.Dialog_Title_Common,TextUnit.Dialog_LevelDataUncreated,TextUnit.Dialog_OK_Common);
				Debug.Log ("未建立地图");
				return;
			}
			//检查关卡目录
			if (!Directory.Exists(LevelDir)){
				//Debug.Log("如果还没有，那就创造一个");
				Directory.CreateDirectory (LevelDir);
			}
			string to_Path = LevelDir + map_rename + TextUnit.BKME_FldotExt;
			if(my_mapData_Source == to_Path){
				//整合数据
				CombineLvData();
				//保存文件
				//File.WriteAllText (to_Path,JsonConvert.SerializeObject (my_map_data));
			}
			//修改名字的时候
			else  {
				//文件重复时
				if (File.Exists (to_Path)) {
					//重复时确定是否覆盖
					if (EditorUtility.DisplayDialog (TextUnit.Dialog_Title_Common, TextUnit.Dialog_FileOverlay, TextUnit.Btn_Yes, TextUnit.Btn_Cancel)) {
						File.Delete (to_Path);
						Debug.Log("判定为成功");
					} else {
						return;
					}
				}
				//整合数据
				CombineLvData();
				//保存文件
                //File.WriteAllText (to_Path,JsonConvert.SerializeObject (my_map_data));
				//2017年01月02日01:22:16
				//修改为另存操作
				//没重复时直接改名
				//File.Move (my_mapData_Source, to_Path);
			}
			#if DEBUG_DATA_CHECK
			Debug.Log(JsonUtility.ToJson(my_map_data));
			#endif
			//Debug.Log("我有了");

			my_mapData_Source = to_Path;
			my_mapDateState = eMapDataState.Saved;
		}
		//整合数据
        //过期的 2018年5月31日19:20:28
		void CombineLvData(){
			//修改data内文件名
			my_map_data.mapName = map_rename;
            //dictionnary转成数组
            //my_map_data.my_unit_info = ParseUnitDateToString(mydic_grid_unit,ref my_map_data); //2017年11月07日07:02:54 unitlist用jsonConvert处理
            //my_map_data.json_grid_unit = JsonConvert.SerializeObject(my_map_data.list_unit_data);
            //SaveEventDataJSON (ref my_map_data.my_event,ref my_map_data.event_script,my_eventlist.datalist);
            //my_map_data.my_event_json = JsonConvert.SerializeObject(my_map_data.list_event_data);
		}
		//解析事件数据
		string[] ParseEventDataToString(List<EventInfo> list){
			//位置num,单位类型,角色ID,
			List<string> event_data = new List<string >();
			foreach (EventInfo _event in list) {
				_event.drama_script.json = JsonUtility.ToJson (_event.drama_script.section_list.ToArray());

				#if UNITY_EDITOR
				string str = JsonUtility.ToJson (_event);
				Debug.Log (str);
				event_data.Add (str);
				#else
				res.Add (JsonUtility.ToJson (_event));
				#endif
			}
//			Debug.Log (res.ToArray());
			return event_data.ToArray();
		}
		//事件、剧本数据保存
		void SaveEventDataJSON(ref string[] evt,ref string[] scp,List<EventInfo> list){
			List<string> event_data = new List<string >();
			List<string> script_data = new List<string >();
			foreach (EventInfo _event in list) {
				string str = JsonUtility.ToJson (_event);
				event_data.Add (str);
				//script 多一步JSON化
				//str = JsonConvert.SerializeObject (_event.drama_script);
				script_data.Add (str);
			}
			evt = event_data.ToArray ();
			scp = script_data.ToArray ();
			//			Debug.Log (res.ToArray());
		}
		//解析事件数据
		bool ReadEventDataToList(string event_json){
			if (event_json == null)
				return false;
            my_map_data.list_event_data.Clear ();

            //my_map_data.list_event_data = JsonConvert.DeserializeObject<List<EventInfo>>(event_json);
            if (my_map_data.list_event_data == null)
				return false;
			my_eventlist.IsNeedDefault ();
//			my_eventlist.SyncStringData ();
			return true;
		}
		bool ReadUnitDateToDic(string arr){
			if (arr == null)
				return false;
			UnitInfo.Unit_Count = 0;
            //mydic_grid_unit = LevelMapData.ParseUnitDataToDic(key,arr);   //2017年11月07日07:09:15 过气了
            //my_map_data.list_unit_data = JsonConvert.DeserializeObject<List<UnitInfo>>(arr);
			if (my_map_data.list_unit_data == null)
				return false;
			return true;
		}
		//设定地图名显示
		void InitMapName(string name){
			if(my_map_data!=null)
				my_map_data.mapName = name;
			map_rename = name;
		}
		//Scene窗口内容
		void ClearScene(){
			GameObject[] oldBoard = GameObject.FindGameObjectsWithTag ("ChessBoard");
			foreach (GameObject _obj in oldBoard) {
				DestroyImmediate (_obj);
			}
		}
        void InitNewScene(){
			if (my_map_data == null)
				return;
            //ClearScene ();
            lv_ctrl.chess_board = Instantiate (BKTools.getBundleObject(eResBundle.Prefabs, PrefabPath.ChessBoard));
            lv_ctrl.list_grid = new List<ChessContainer> ();
			//重新建立棋盘
			bool b_odd = true;
			Vector2 anchor = new Vector2 ();
			int size_count = 0;
			for (int x = 0; x < my_map_data.my_size[0]; x++) {
				for (int y = 0; y < my_map_data.my_size[1]; y++) {
					ChessContainer new_grid = Instantiate (BKTools.getBundleObject(eResBundle.Prefabs,PrefabPath.ChessGrid)).GetComponent<ChessContainer>();
					new_grid.number = size_count++;
					new_grid.row = y;
					new_grid.column = x;
					new_grid.transform.SetParent (lv_ctrl.chess_board.transform);
					//位置
//					Vector2 new_pos = new Vector2();
//					new_pos.x = anchor.x + chess_container_size.x * x;
//					new_pos.y= anchor.y+(b_odd?0:chess_container_size.y/2) +y * chess_container_size.y;
					Vector3 new_pos = getPosition(anchor,x,y);
					new_pos.y += b_odd?0: BKTools.chess_container_size.y/2;
					new_grid.transform.localPosition = new_pos;
					new_grid.transform.localScale = Vector3.one;
					//外观
					setGridDisplay(new_grid);

                    //关联
                    lv_ctrl.list_grid.Add(new_grid);
				}
				b_odd = !b_odd;
			}
		}
		public Vector3 getPosition(Vector3 anchor,int x,int y){
			Vector3 new_pos = new Vector3();
			new_pos.x = anchor.x + BKTools.chess_container_size.x * x;
			new_pos.y= anchor.y +y * BKTools.chess_container_size.y;
			return new_pos;
		}
		//是否有东西超出边界
		bool Resize_Check(){
			foreach (var item in my_map_data.list_unit_data) {
				if (item.m_start_grid / my_map_data.my_size [1] > map_resize [1])
					return true;
				else if (item.m_start_grid % my_map_data.my_size [1] > map_resize [0])
					return true;
			}
			return false;
		}
		void Resize_Map(){
			eMapGridType[] new_grid_info=new eMapGridType[map_resize[0]*map_resize[1]];
			int[] new_grid_godir= new int[new_grid_info.Length];
			//暂时不实现
			//int[] new_unit_info = new int[my_map_data.my_unit_info.Length];
			//初始化新数据
			int all_dir_clear = eGridGoDir.Upper | eGridGoDir.Lower | eGridGoDir.UpperRight | eGridGoDir.LowerLeft | eGridGoDir.UpperLeft | eGridGoDir.LowerRight;
			for (int i = 0; i < new_grid_info.Length; i++) {
				new_grid_info [i] = eMapGridType.Unvailable;
				new_grid_godir [i] = all_dir_clear;
			}
            //翻译过程
            /*
			int[] old = new int[2]{0,0};
			int new_p = 0;
			for (int n = 0; n < my_map_data.my_grid_godir.Length; n++) {
				//定位
				old[0] = n / my_map_data.my_size[1];
				old[1] = n % my_map_data.my_size[1];
				//旧图超出新图尺寸的部分抛弃
				if (map_resize [0] < my_map_data.my_size [0] && old [0] >= map_resize [0]) {
					if(my_map_data.list_unit_data.ContainsKey(n))   //后面还用到的话，需要用for把所有List<UnitInfo>的所有单位检查一次。【但是现在的计划是改变地图编辑的方式，不会用到粗暴裁剪了】2018年6月1日23:27:44
						my_map_data.list_unit_data.Remove(n);
					continue;
				}
				if (map_resize [1] < my_map_data.my_size [1] && old [1] >= map_resize [1]) {
					if(my_map_data.list_unit_data.ContainsKey(n))
						my_map_data.list_unit_data.Remove(n);
					continue;
				}
				new_p = old [0] * map_resize [1] + old [1];
				new_grid_info [new_p] = 
					my_map_data.my_grid_t_type [n];
				if (new_p != n && my_map_data.list_unit_data.ContainsKey(n)) {
					if (!my_map_data.list_unit_data.ContainsKey (new_p))
						my_map_data.list_unit_data.Add (new_p, my_map_data.list_unit_data [n]);
					else
						my_map_data.list_unit_data [new_p] = my_map_data.list_unit_data [n];
					my_map_data.list_unit_data.Remove (n);
				}
			}
            */
            //赋值
            my_map_data.my_grid_godir = new_grid_godir;
			my_map_data.my_grid_t_type = new_grid_info;
			my_map_data.my_size[0] = map_resize[0];
			my_map_data.my_size[1] = map_resize[1];
			//重置地图
			InitNewScene ();
			ResetUnitView ();
		}

		//显示编辑器
		[MenuItem ("Window/MapEditor %e")]
		public static void  ShowWindow () {
            if (instance != null) return;
			var window = EditorWindow.GetWindow(typeof(MapEditor));
			((MapEditor)window).init ();
		}
        [MenuItem("Window/TestFn %t")]
        public static void TestFnSC(){

            //DramaPhase empty_phase = Resources.Load<DramaPhase>(PrefabPath.DramaPhase);
            //empty_phase.drama_script = new DramaScript();

            //DramaPhase empty_phase2 = Resources.Load<DramaPhase>(PrefabPath.DramaPhase);

            //int card_id = 130000;
            //int belong = 7;
            //int shoukan_event = 127;
            //int shoukan_event_sequence = 15;

            //uint res = (uint)(card_id << 15) + (belong << 12) + (shoukan_event << 5) + shoukan_event_sequence;



            //Debug.Log(1<<2);
            //Debug.Log(res);
            //card_id = (res >> 15);
            //Debug.Log("card_id = " +card_id.ToString());
            //belong = (res >> 12) & 7;
            //Debug.Log("belong = " + belong.ToString());
            //shoukan_event = (res >> 5) & 127;
            //Debug.Log("shoukan_event = " + shoukan_event.ToString());
        }
        void Awake() {
            //init(true);
        }
        //保证编译后编辑器不用重开
        private void OnEnable() {
            init(true);
        }
        private void OnDestroy() {
            SceneView.onSceneGUIDelegate -= SceneGUI;
        }
        public void init(){
		    init (false);
		}
		/// <summary>
		/// 无视现状强制初始化
		/// </summary>
		/// <param name="force_init">If set to <c>true</c> force init.</param>
		public void init(bool force_init){
			if (isInit && !force_init)
				return;
            //场景事件托管
            SceneView.onSceneGUIDelegate += SceneGUI;
            //地图数据
            my_map_data = null;
			instance = this;
			titleContent = new GUIContent(TextUnit.BKKZ_MapEditor_Title);
			//ClearScene ();
			strMapDataState = new string[]{TextUnit.BKME_State_Loading, TextUnit.BKME_State_Load, TextUnit.BKME_State_Uncreated, TextUnit.BKME_State_Dirt, TextUnit.BKME_State_Save};
			my_brush_select = 0;
			brush_name = new string[]{"选择","边界","缺口","普通","山","河","城","林"};
            str_area_brush_name = new string[] { "删除", "添加" };
//			my_brush_state = eBrushState.Grid;
			editor_state = new string[]{"地形","单位","事件","全局事件","区域"};
//				brush_btn = new GUIContent[brush_name.Length];
//				for (int i = 0; i < brush_name.Length; i++) {
//					brush_btn [i].text = brush_name [i];
//					//brush_btn[i].image = 
//				}
			map_rename = "";
			map_resize = new int[]{ 32, 32 };



			//界面初始化
			my_eventlist = EventList.Instance;
			my_eventlist.ResetUnit = ResetUnitView;
//			Debug.Log ("绑定过resetunit功能");
			my_unitlist = new UnitList ();
			//my_unitlist.initData (my_unitlist,new List<UnitInfo>());
            my_unitlist.ResetUnit = ResetUnitView;

            my_arealist = new AreaList();

//				my_eventlist.Add (new EventInfo ());
//				s_this = new UnityEditor.SerializedObject(this);

			//事件界面
//				gui_eventlist = new ReorderableList(s_this,
//					s_this.FindProperty("my_eventlist"),
//						true, true, true, true);
//				SerializedProperty ppt = s_this.FindProperty ("my_eventlist");//map_rename
//				SerializedProperty ppt2 = s_this.FindProperty ("map_rename");//my_brush_select
//				SerializedProperty ppt3 = s_this.FindProperty ("my_brush_select");//
//				Debug.Log (ppt);
//				Debug.Log (ppt2);
//				Debug.Log (ppt3);
			#if RESTORE
			gui_eventlist.drawElementCallback += _EventList_DrawElementCallback;
			gui_eventlist.drawHeaderCallback = (Rect rect) =>
			{  
				EditorGUI.LabelField(rect, "Event List");
			};
			gui_eventlist.onChangedCallback = (ReorderableList list) =>
			{
				//NOTE: When reordering elements in ReorderableList, elements are not moved, but data is swapped between them.
				// So if you keep addres of element 0 ex: data = list[0], after reordering element 0 with 1, data will contain the elemnt1 data.
				// Keeping a reference to MapLayer in TileChunks is useless
				s_this.ApplyModifiedProperties(); // apply adding and removing changes
				//				MyAutoTileMap.SaveMap();
				//				MyAutoTileMap.LoadMap();
			};
			#endif


//				_map_resize_s = new string[2];
//				_map_resize_s [0] = map_resize[0].ToString();
//				_map_resize_s [1] = map_resize[1].ToString();
			my_mapDateState = eMapDataState.Uncreated;

			//坑
//				scene_view = GetWindow<SceneView> ();
			//手写吧，不要算了
//				GameObject pre_grid = Resources.Load<GameObject> ("Prefabs/chess_grid");
//				Rect _rect = pre_grid.GetComponent<SpriteRenderer> ().sprite.rect;
//				float radius = _rect.width / 2 - 2;
			//((MapEdtior)window).chess_container_size = new Vector2 (1.5f*radius,Mathf.Pow(3,0.5f)*radius)/100f;
			
			//TODO 后期对初始化过的地图进行细分
//				if (map_data == null)
//					mapDateState = eMapDataState.Uncreated;	//仅当没有数据时强制定义为创建，后续根据逻辑处理修改
			#if DEBUG_DATA_CHECK
//				mydic_grid_unit.Add(0,new List<int>());
//				mydic_grid_unit[0].Add(39);
//				mydic_grid_unit[0].Add(3939);
//				mydic_grid_unit[0].Add(39);
//				mydic_grid_unit.Add(1,new List<int>());
//				mydic_grid_unit[1].Add(39);
//				mydic_grid_unit[1].Add(3939);
//				mydic_grid_unit[1].Add(39);
			#endif 


			isInit = true;
			Debug.Log ("欢迎使用穆风牌地图编辑器");
		}
		#if RESTORE
		//绘制
		private void _EventList_DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
		{
			SerializedProperty itemData = gui_eventlist.serializedProperty.GetArrayElementAtIndex(index);

			//			rect.y += 2;
			//			rect.height = EditorGUIUtility.singleLineHeight;
			//			EditorGUI.PropertyField(rect, itemData, GUIContent.none);

			EditorGUILayout.PropertyField (itemData.FindPropertyRelative ("event_id"));
			EditorGUILayout.PropertyField (itemData.FindPropertyRelative ("condition"));
			EditorGUILayout.PropertyField (itemData.FindPropertyRelative ("description"));
		}
		#endif
		//自制鼠标操作信息获取
		bool m_isMouseLeft;
		bool m_isMouseRight;
		//bool m_isMouseMiddle;
		bool m_isMouseLeftDown;
		bool m_isMouseRightDown;
		//bool m_isMouseMiddleDown;

		void UpdateMouseInputs()
		{
			m_isMouseLeftDown = false;
			m_isMouseRightDown = false;
			//m_isMouseMiddleDown = false;

			if( Event.current.isMouse )
			{
				m_isMouseLeftDown = ( Event.current.type == EventType.MouseDown && Event.current.button == 0);
				m_isMouseRightDown = ( Event.current.type == EventType.MouseDown && Event.current.button == 1);
				//m_isMouseMiddleDown = ( Event.current.type == EventType.MouseDown && Event.current.button == 1);
				m_isMouseLeft = m_isMouseLeftDown || ( Event.current.type == EventType.MouseDrag && Event.current.button == 0);
				m_isMouseRight = m_isMouseRightDown || ( Event.current.type == EventType.MouseDrag && Event.current.button == 1);
				//m_isMouseMiddle = m_isMouseMiddleDown || ( Event.current.type == EventType.MouseDrag && Event.current.button == 2);
			}
		}
	}

}