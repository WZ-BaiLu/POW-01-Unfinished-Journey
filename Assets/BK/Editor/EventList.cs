using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BKKZ.POW01
{
    /*
     * 每次地图数据变化需要初始化
     */
    public class EventList:ScriptableObject
    {
        static EventList instance = null;
        public static EventList Instance{
            get{
                if (instance == null)
                    instance = new EventList ();
                
                return instance;
            }
        }
        //public List<EventInfo> list_event_data = null;
        public SerializedObject s_inst;
        public ReorderableList m_GUIList;
        public static int now_event_order = 0;
        public static EventInfo Now_Event{
            get{
                if (MapEditor.my_map_data.list_event_data.Count> now_event_order)
                    return MapEditor.my_map_data.list_event_data [now_event_order];
                else
                    return null;
            }
        }
//        public static SerializedObject editing_event;
        private string[] arr_id_str;
        public string[] Arr_ID_String{
            get{

                for(int i=0;i< MapEditor.my_map_data.list_event_data.Count;i++){
                    MapEditor.my_map_data.list_event_data[i].setOrder(i);
                }
                arr_id_str = new string[MapEditor.my_map_data.list_event_data.Count];
                for(int i=0;i< MapEditor.my_map_data.list_event_data.Count;i++){
                    arr_id_str[i] = MapEditor.my_map_data.list_event_data[i].event_id;
                }
                return arr_id_str;
            }
        }
        public bool isInit = false;
        public Action ResetUnit;
        List<float> heights;
        int expandedLines = 3;

        //没用到
        public EventInfo getEventbyID(string  id){
            foreach (EventInfo e in MapEditor.my_map_data.list_event_data) {
                if (e.event_id == id) {
                    return e;
                }
            }
            return null;
        }

        public void NewElement(){
            EventInfo.event_count++;
            EventInfo new_e = new EventInfo ();
            //            new_e.event_id = datalist [datalist.Count-1].event_id + 1;    //默认命名已经放到构造函数里了
            MapEditor.my_map_data.list_event_data.Insert (now_event_order+1,new_e);
//            SyncStringData ();
        }
        /// <para>判断当前地图是否没有任何事件</para>  
        /// <param name="no param">    sd </param>
        public void IsNeedDefault(){
            if (MapEditor.my_map_data.list_event_data.Count <= 0) {
                EventInfo.event_count++;
                EventInfo new_event = new EventInfo ();
                new_event.event_id = "事件1";
//                new_event.condition = 0;
                new_event.description = "检查完毕 游戏启动";
                MapEditor.my_map_data.list_event_data.Add (new_event);
            }
        }
        //专门为了单位编辑界面多出的两个值进行同步
        public void SyncStringData(){
            for(int i=0;i< MapEditor.my_map_data.list_event_data.Count;i++){
                MapEditor.my_map_data.list_event_data[i].setOrder(i);
            }
            arr_id_str = new string[MapEditor.my_map_data.list_event_data.Count];
            for(int i=0;i< MapEditor.my_map_data.list_event_data.Count;i++){
                arr_id_str[i] = MapEditor.my_map_data.list_event_data[i].event_id;
            }
//            ResetUnit ();//刷新顺序跟initScene冲突
        }
        //重置数据(出厂设置，加载新地图时应有所筛选，比如是否需要默认数据）
        //2018年5月31日23:41:46 调用这个函数的地方并没有实质作用
        public void ResetData(){
            MapEditor.my_map_data.list_event_data.Clear ();
            IsNeedDefault ();//默认数据
//            SyncStringData ();
            ResetUnit ();
        }
        public void OnEventSelectChange(){
            DramaSectionWindows.ShowWindow(now_event_order, MapEditor.my_map_data.list_event_data[now_event_order].drama_script.section_list.Count>0 ? 
                                                            MapEditor.my_map_data.list_event_data[now_event_order].drama_script.section_list[0] : null);
            ResetUnit();
        }
        //以下大部分为绘制内容，显示逻辑初始化
        public void initData(ScriptableObject target){
            //if (isInit)
                //return;
            isInit = true;
            s_inst = new SerializedObject (target);
//            Debug.Log((s_inst.FindProperty ("datalist").isArray).ToString());
//            Debug.Log ("属性" + s_inst.FindProperty ("datalist"));
//            Debug.Log ("长度" + s_inst.FindProperty ("datalist").arraySize);
            m_GUIList = new ReorderableList(s_inst, //TODO 需要改变serializedObj对象
                s_inst.FindProperty("list_event_data"),
                true, true, true, true);

            IsNeedDefault();//默认数据
            //ResetUnit();    //重置场景上的单位图标
            // Initialize a temporary list of element heights to be used later on in the draw function
            heights = new List<float>(s_inst.FindProperty("list_event_data").arraySize);
            m_GUIList.index = 0;
            m_GUIList.drawElementCallback += _EventList_DrawElementCallback;
            m_GUIList.onReorderCallback = (ReorderableList list) => {
                //把棋子从原来的事件序号对应到新的时间序号
                foreach (var unit_item in MapEditor.my_map_data.list_unit_data) {
                    if (unit_item.m_launch_event_order > now_event_order
                        && unit_item.m_launch_event_order > list.index)
                        continue;
                    if (unit_item.m_launch_event_order < now_event_order
                        && unit_item.m_launch_event_order < list.index)
                        continue;

                    if(unit_item.m_launch_event_order == now_event_order){
                        unit_item.m_launch_event_order = list.index;
                        continue;
                    }
                    //从下向上拖
                    if(now_event_order>list.index)
                        unit_item.m_launch_event_order++;
                    else if(now_event_order<list.index)
                        unit_item.m_launch_event_order--;
                }
                //不刷新
                now_event_order = list.index;
                s_inst.ApplyModifiedProperties();
                //Debug.Log("出现list.index与实际长度不符，当前："+list.index);
            };
            m_GUIList.drawHeaderCallback = (Rect rect) =>
            {  
                EditorGUI.LabelField(rect, "Event List");
            };
            m_GUIList.onChangedCallback = (ReorderableList list) =>
            {
                //NOTE: When reordering elements in ReorderableList, elements are not moved, but data is swapped between them.
                // So if you keep addres of element 0 ex: data = list[0], after reordering element 0 with 1, data will contain the elemnt1 data.
                // Keeping a reference to MapLayer in TileChunks is useless

                s_inst.ApplyModifiedProperties(); // apply adding and removing changes
                s_inst.Update();
//                SyncStringData();
                //ResetUnit ();
//                MyAutoTileMap.SaveMap();
//                MyAutoTileMap.LoadMap();
            };
            m_GUIList.onSelectCallback = (ReorderableList list) => {
                now_event_order = m_GUIList.index;
//                editing_event = new UnityEditor.SerializedObject(datalist[now_event_order]);
                OnEventSelectChange();
//                Debug.Log (datalist [list.index] + "/" + datalist.Count);
//                Debug.Log(m_GUIList.serializedProperty.GetArrayElementAtIndex(list.index));
//                Debug.Log(m_GUIList.serializedProperty.CountInProperty());
//                Debug.Log(m_GUIList.serializedProperty.GetArrayElementAtIndex(list.index).FindPropertyRelative ("event_id"));
            };
            m_GUIList.onAddCallback = (ReorderableList list) =>
            {
                NewElement();
                s_inst.ApplyModifiedProperties(); 
                list.index = ++now_event_order;
                OnEventSelectChange();
//                SyncStringData();
            };
            m_GUIList.onRemoveCallback = (ReorderableList list) => {
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
                IsNeedDefault ();
//                SyncStringData();
                s_inst.ApplyModifiedProperties();
                now_event_order = m_GUIList.index;
                OnEventSelectChange();
            };
            // Adjust heights based on whether or not an element is selected.
            m_GUIList.elementHeightCallback = ( index ) => {
                //Repaint();
                float height = 0;

                try {
                    height = heights[index];
                } catch ( ArgumentOutOfRangeException e ) {
                    Debug.LogWarning(e.Message);
                } finally {
                    resizeHeightList();
                }

                return height;
            };


            // Set the color of the selected list item
            m_GUIList.drawElementBackgroundCallback = ( rect, index, active, focused ) => {
                try{
                    rect.height = heights[index];
                }catch ( ArgumentOutOfRangeException e ) {
                    Debug.LogWarning(e.Message);
                } finally {
                    resizeHeightList();
                }
                rect.height += 1;//+1 is to give each element a bit of padding on the bottom
                Texture2D tex = new Texture2D(1, 1);
                if ( active ){
                    tex.SetPixel(0, 0, new Color(0.1f, 0.33f, 1f, 0.33f));
                    tex.Apply();
                    GUI.DrawTexture(rect, tex as Texture);
                }
            };
        }
        public void OnInspectorGUI()
        {
            if (!isInit) {
                Debug.Log ("未初始化");
                return;
            }
            s_inst.Update();
            m_GUIList.DoLayoutList();
            s_inst.ApplyModifiedProperties();
//            SyncStringData ();
        }
        private void _EventList_DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            // Manage the height of this element
            bool foldout = isActive;
            float height = EditorGUIUtility.singleLineHeight * 1.25f; // multiply by 1.25 to give each property a little breathing room
//            if ( foldout ) {
//                height = EditorGUIUtility.singleLineHeight * expandedLines + 2; // +2 is to give each element a bit of padding on the bottom
//            }

            // Manage heights of each element
            /// TODO: heights should really based on the GetPropertyHeight of property type, rather
            /// than some random function parameter that we input, but I can't get GetPropertyHeight
            /// to be properly here... at least for custom property drawers.
            try {
                heights[index] = height;
            } catch ( ArgumentOutOfRangeException e ) {
                Debug.LogWarning(e.Message);
            } finally {
                resizeHeightList ();
            }

            SerializedProperty itemData = m_GUIList.serializedProperty.GetArrayElementAtIndex(index);

            rect.y += 3;
            rect.height = EditorGUIUtility.singleLineHeight-1;
//            EditorGUI.PropertyField(rect, itemData, GUIContent.none);
//            return;
            float elemY;
            float elemX = rect.x;float elemWidth = 30; 
            //编号
            EditorGUI.LabelField (new Rect(elemX,rect.y,elemWidth,rect.height),"编号");
            elemX += elemWidth; elemWidth = 250;
            EditorGUI.PropertyField (new Rect(elemX,rect.y,elemWidth,rect.height)
                , itemData.FindPropertyRelative ("event_id"), GUIContent.none);
            //条件
//            elemX += elemWidth; elemWidth = 30;
//            EditorGUI.LabelField (new Rect(elemX,rect.y,elemWidth,rect.height),"条件");
//            elemX += elemWidth; elemWidth = 100;
//            EditorGUI.PropertyField (new Rect(elemX,rect.y,elemWidth,rect.height*3)
//                , itemData.FindPropertyRelative ("condition"), GUIContent.none);
            
//            if (foldout) {
//                //描述
//                elemX = rect.x;elemWidth = 60;
//                elemY = rect.y + EditorGUIUtility.singleLineHeight + 2;
//                EditorGUI.LabelField (new Rect(elemX,elemY,elemWidth,rect.height),"触发阶段");
//                elemX += elemWidth; elemWidth = 40;
//                EditorGUI.PropertyField (new Rect(elemX,elemY,elemWidth,rect.height)
//                    , itemData.FindPropertyRelative ("launch_phase"), GUIContent.none);
//                //描述
//                elemX = rect.x;
//                elemWidth = rect.width;
//                elemY = rect.y + (EditorGUIUtility.singleLineHeight + 2)*2;
//                EditorGUI.PropertyField (new Rect (elemX, elemY, elemWidth, rect.height)
//                    , itemData.FindPropertyRelative ("description"), GUIContent.none);
//            }
        }
        void resizeHeightList(){
            float[] floats = heights.ToArray();
            Array.Resize(ref floats, m_GUIList.serializedProperty.arraySize);
//            heights = floats.ToList();

//            for (int i = 0; i < floats.Length; i++) {
//                if(heights.Contains(i))
//                    heights[i] = floats[i];
//                else 
//                    heights.Add(floats [i]);
//            }
            heights.Clear();
            heights.AddRange (floats);
        }
    }
}

