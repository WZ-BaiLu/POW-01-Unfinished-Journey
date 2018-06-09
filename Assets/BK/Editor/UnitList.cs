using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace BKKZ.POW01
{
    /*
     * 建立和Eventlist不同的机制
     * 仅用作编辑显示
     * 会筛选出和当前格子相关的单位
     * 并关联到正确的关卡数据中
     */
	public class UnitList:ScriptableObject
	{
		public List<int> datalist = null;
		UnitInfo[] compare_list = null;
		//public SerializedObject s_inst;
		public ReorderableList m_GUIList;
		public static int now_unit_order = 0;
        public static int now_grid_for = -1;
//		public string[] arr_id_str;
		public bool isInit = false;
		public Action ResetUnit;
		List<float> heights;
		static int expandedLines = 2;

		public void NewElement(){
			NewElement (0,eUnitType.Unit);
		}
		public void NewElement(int id,eUnitType _type){
			m_GUIList.index = datalist.Count;

			UnitInfo new_unit = new UnitInfo (id,_type);
			switch (_type) {
			case eUnitType.Unit:
				if(!Data.Inst.card_data.ContainsKey(id))
					new_unit.m_unit_id = Data.Inst.card_data.First().Key;
				//默认取第一个卡片
				//			new_e.event_id = datalist [datalist.Count-1].event_id + 1;
				break;
			case eUnitType.Buff:
				if(!Data.Inst.buff_data.ContainsKey(id))
					new_unit.m_unit_id = Data.Inst.buff_data.First().Key;
				break;
			default:
				break;
			}
			new_unit.click_ID = UnitInfo.Unit_Count++;
            new_unit.m_start_grid = MapEditor.Inst.SelectGridId;
			MapEditor.my_map_data.list_unit_data.Add (new_unit);
            datalist.Add(MapEditor.my_map_data.list_unit_data.Count - 1);
			new_unit.m_launch_event_order = EventList.now_event_order;

			//s_inst.ApplyModifiedProperties(); 
			now_unit_order = m_GUIList.index;
		}
		//判断是否为空
		public void IsNeedDefault(){
//			if (datalist.Count <= 0) {
//				UnitInfo new_event = new UnitInfo (0);
////				new_event.event_id = 0;
////				new_event.condition = 0;
////				new_event.description = "检查完毕 游戏启动";
//				datalist.Add (new_event);
//			}
		}
		//专门为了单位编辑界面多出的两个值进行同步
		public void SyncStringData(){
			//s_inst.Update ();
			//s_inst.ApplyModifiedProperties ();
			ResetUnit ();
//			for(int i=0;i<datalist.Count;i++){
//				datalist[i].setOrder(i);
//			}
//			arr_id_str = new string[datalist.Count];
//			for(int i=0;i<datalist.Count;i++){
//				arr_id_str[i] = datalist[i].event_id.ToString();
//			}
		}
		//重置数据(出厂设置，加载新地图时应有所筛选，比如是否需要默认数据）
		public void ResetData(ScriptableObject target){
//			datalist.Clear ();
			initData();
//			IsNeedDefault ();//默认数据
			SyncStringData ();
		}
		//排序
		void Resort(){
		}


        public List<int> SelectIndexByGridID(List<UnitInfo> srclist) {
            List<int> list = new List<int>();
            if (MapEditor.Inst.SelectGridId == -1)
                return list;
            for (int i = 0; i < srclist.Count; i++) {
                var item = srclist[i];
                if (item.m_start_grid == MapEditor.Inst.SelectGridId)
                    list.Add(i);
            } 

            return list;
        }
		//以下大部分为绘制内容，显示逻辑初始化
		public void initData(){
//			if (isInit)
//				return;
			isInit = true;
			now_unit_order = 0;
            //经过筛选的地图数据指向
            datalist = SelectIndexByGridID(MapEditor.my_map_data.list_unit_data);
            now_grid_for = MapEditor.Inst.SelectGridId;

            //s_inst = new UnityEditor.SerializedObject (this);
			m_GUIList = new ReorderableList(datalist,
				typeof(List<int>),
				true, true, true, true);
			m_GUIList.draggable = false;
			// Initialize a temporary list of element heights to be used later on in the draw function
			heights = new List<float>(datalist.Count);
			m_GUIList.drawElementCallback += _EventList_DrawElementCallback;
            m_GUIList.index = 0;
			m_GUIList.onReorderCallback = (ReorderableList list) => {
                //交换两个单位的所属事件
				//int t_order = compare_list[list.index].m_launch_event_order;
				//compare_list[list.index].m_launch_event_order = compare_list[now_unit_order].m_launch_event_order;
				//compare_list[now_unit_order].m_launch_event_order = t_order;

				now_unit_order = m_GUIList.index;
				Resort();
			};
			m_GUIList.onAddDropdownCallback = (Rect buttonRect, ReorderableList l) => {  
				var menu = new GenericMenu();
//				var guids = AssetDatabase.FindAssets("", new[]{"Assets/Prefabs/Mobs"});
//				foreach (var guid in guids) {
//					var path = AssetDatabase.GUIDToAssetPath(guid);
//					menu.AddItem(new GUIContent("Mobs/" + Path.GetFileNameWithoutExtension(path)), 
//						false, clickHandler, 
//						new WaveCreationParams() {Type = MobWave.WaveType.Mobs, Path = path});
//				}
//				guids = AssetDatabase.FindAssets("", new[]{"Assets/Prefabs/Bosses"});
//				foreach (var guid in guids) {
//					var path = AssetDatabase.GUIDToAssetPath(guid);
//					menu.AddItem(new GUIContent("Bosses/" + Path.GetFileNameWithoutExtension(path)), 
//						false, clickHandler, 
//						new WaveCreationParams() {Type = MobWave.WaveType.Boss, Path = path});
				//				}
				foreach (var item in Data.Inst.card_data) {
					menu.AddItem(new GUIContent("Unit/" + item.Value.name), 
						false,()=>{NewElement(item.Value.id,eUnitType.Unit);});
				}
				foreach (var item in Data.Inst.buff_data) {
					menu.AddItem(new GUIContent("Buff/" + item.Value.name), 
						false,()=>{NewElement(item.Value.id,eUnitType.Buff);});
				}

				menu.ShowAsContext();
			};
			m_GUIList.drawHeaderCallback = (Rect rect) =>
			{  
				EditorGUI.LabelField(rect, "Unit List");
			};
			m_GUIList.onChangedCallback = (ReorderableList list) =>
			{
				//NOTE: When reordering elements in ReorderableList, elements are not moved, but data is swapped between them.
				// So if you keep addres of element 0 ex: data = list[0], after reordering element 0 with 1, data will contain the elemnt1 data.
				// Keeping a reference to MapLayer in TileChunks is useless

				//s_inst.ApplyModifiedProperties(); // apply adding and removing changes
				//s_inst.Update();
				SyncStringData();
//				MyAutoTileMap.SaveMap();
//				MyAutoTileMap.LoadMap();
			};
			m_GUIList.onSelectCallback = (ReorderableList list) => {
//				Debug.Log (datalist [list.index] + "/" + datalist.Count);
//				Debug.Log(m_GUIList.serializedProperty.GetArrayElementAtIndex(list.index));
//				Debug.Log(m_GUIList.serializedProperty.CountInProperty());
//				Debug.Log(m_GUIList.serializedProperty.GetArrayElementAtIndex(list.index).FindPropertyRelative ("event_id"));
				now_unit_order = m_GUIList.index;
//				compare_list = new UnitInfo[datalist.Count];
//				for (int i = 0; i < datalist.Count; i++) {
//					compare_list[datalist.Count-i-1] = datalist[i];
//				}
//				Debug.Log("compare修改了，改了，了");
//				string str = "compare：";
//				foreach (var item in compare_list) {
//					str += item.m_launch_event_order +"/";
//				}
//				Debug.Log(str);
//				str = "datalist：";
//				foreach (var item in datalist) {
//					str += item.m_launch_event_order +"/";
//				}
//				Debug.Log(str);
			};
			m_GUIList.onAddCallback = (ReorderableList list) =>
			{
				NewElement();
			};
			m_GUIList.onRemoveCallback = (ReorderableList list) => {
                //TODO 可能出问题
                MapEditor.my_map_data.list_unit_data.RemoveAt(datalist[m_GUIList.index]);
				datalist.RemoveAt(m_GUIList.index);
				ReorderableList.defaultBehaviours.DoRemoveButton(list);


				IsNeedDefault ();
				SyncStringData();
				//s_inst.ApplyModifiedProperties();
				now_unit_order = m_GUIList.index;
				ResetUnit();
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
			//s_inst.Update();
			m_GUIList.DoLayoutList();
			//s_inst.ApplyModifiedProperties();
		}
		private void _EventList_DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
		{
			// Manage the height of this element
//			bool foldout = isActive;
			float height = EditorGUIUtility.singleLineHeight * 1.25f; // multiply by 1.25 to give each property a little breathing room
//			if ( foldout ) {
//				height = EditorGUIUtility.singleLineHeight * expandedLines + 2; // +2 is to give each element a bit of padding on the bottom
//			}

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

			//SerializedProperty itemData = m_GUIList.serializedProperty.GetArrayElementAtIndex(index);
			UnitInfo unit = MapEditor.my_map_data.list_unit_data[datalist[index]];
			rect.y += 1;
			rect.height = EditorGUIUtility.singleLineHeight-1;
//			EditorGUI.PropertyField(rect, itemData, GUIContent.none);
//			return;
			float elemY;
//			float elemX = rect.x;float elemWidth = 30; 
			float elemX = rect.x;float elemWidth = 165; 
			//编号
//			EditorGUI.LabelField (new Rect(elemX,rect.y,elemWidth,rect.height),"类型");
//			elemX += elemWidth; elemWidth = 40;
//			EditorGUI.PropertyField (new Rect(elemX,rect.y,elemWidth,rect.height)
//				, itemData.FindPropertyRelative ("m_unit_type"), GUIContent.none);
			switch (unit.m_unit_type) {
			case eUnitType.Unit:
				EditorGUI.LabelField (new Rect(elemX,rect.y,elemWidth,rect.height),unit.m_unit_type.ToString()+" - "+Data.Inst.card_data[unit.m_unit_id].name);
				break;
			case eUnitType.Buff:
				EditorGUI.LabelField (new Rect(elemX,rect.y,elemWidth,rect.height),unit.m_unit_type.ToString()+" - "+Data.Inst.buff_data[unit.m_unit_id].name);
				break;
			default:
				break;
			}

//			elemX += elemWidth; elemWidth = 95;
//			EditorGUI.LabelField (new Rect(elemX,rect.y,elemWidth,rect.height)
//								, Data.Inst.card_data[datalist[index].m_unit_id].name);
			//条件
//			elemX += elemWidth; elemWidth = 15;
//			EditorGUI.LabelField (new Rect(elemX,rect.y,elemWidth,rect.height),"怪物名:");
//			elemX += elemWidth; elemWidth = 80;
//			EditorGUI.PropertyField (new Rect(elemX,rect.y,elemWidth,rect.height)
//				, itemData.FindPropertyRelative ("m_unit_id"), GUIContent.none);
//			EditorGUI.LabelField (new Rect(elemX,rect.y,elemWidth,rect.height)
//				, Data.Inst.card_data[datalist[index].m_unit_id].name);
//			string[] arr_Chess_name = (from card_info in Data.Inst.card_data
//								select card_info.Value.name).ToArray();
//			int[] arr_Chess_key = (from card_info in Data.Inst.card_data
//									select card_info.Key).ToArray();
//			itemData.FindPropertyRelative ("m_unit_id").intValue = EditorGUI.IntPopup (new Rect (elemX, rect.y, elemWidth, rect.height)
//				, itemData.FindPropertyRelative ("m_unit_id").intValue, arr_Chess_name, arr_Chess_key);


			elemX += elemWidth; elemWidth = 55;
			EditorGUI.LabelField (new Rect(elemX,rect.y,elemWidth,rect.height),"出现时机:");
			elemX += elemWidth; elemWidth = 150;
//			string[] arr_event_name = EventList.Instance.arr_id_str;
//			itemData.FindPropertyRelative ("m_launch_event_order").intValue = EditorGUI.Popup (new Rect (elemX, rect.y, elemWidth, rect.height)
//				, itemData.FindPropertyRelative ("m_launch_event_order").intValue, arr_event_name);

			EditorGUI.LabelField (new Rect(elemX,rect.y,elemWidth,rect.height)
				, EventList.Instance.Arr_ID_String[unit.m_launch_event_order]);


//			if (foldout) {
//				//AI类型
//				elemX = rect.x;
//				elemWidth = 30;
//				elemY = rect.y + EditorGUIUtility.singleLineHeight + 0;//2
//				EditorGUI.LabelField (new Rect(elemX,elemY,elemWidth,rect.height),"AI");
//				elemX += elemWidth; elemWidth = 40;
//				EditorGUI.PropertyField (new Rect(elemX,elemY,elemWidth,rect.height)
//					, itemData.FindPropertyRelative ("AI_ID"), GUIContent.none);
//			}
		}
		void resizeHeightList(){
			float[] floats = heights.ToArray();
			Array.Resize(ref floats, datalist.Count);
//			heights = floats.ToList();

//			for (int i = 0; i < floats.Length; i++) {
//				if(heights.Contains(i))
//					heights[i] = floats[i];
//				else 
//					heights.Add(floats [i]);
//			}
			heights.Clear();
			heights.AddRange (floats);
		}
	}
}

