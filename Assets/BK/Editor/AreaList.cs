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
    //AreaList编辑器特殊情况，这个列表真的只有编辑功能
    public class AreaList:ScriptableObject
    {
        [SerializeField]
        public List<bool> datalist = null;//每个区域ID是否处于显示中，只提供选择编辑不提供排序等，因为现在的操作对象不是真的
        public SerializedObject s_inst;
        public ReorderableList m_GUIList;
        public static int now_area_index = 0;
        public static string[] arr_string_id;
//        public static SerializedObject editing_event;
        public bool isInit = false;
        public Action ResetArea;
        List<float> heights;
        int expandedLines = 3;

        public void NewElement(){
            
//            new_e.event_id = datalist [datalist.Count-1].event_id + 1;    //默认命名已经放到构造函数里了
            datalist.Add (false);
            MapEditor.my_map_data.list_area_grid.Add(new AreaInfo());
//            SyncStringData ();
        }
        public void SyncStringData() {
            arr_string_id = new string[datalist.Count];
            for (int i = 0; i < datalist.Count; i++) {
                arr_string_id[i] = i.ToString();
            }
            //            ResetUnit ();//刷新顺序跟initScene冲突
        }
        //以下大部分为绘制内容，显示逻辑初始化
        public void initData(ScriptableObject target, Action ResetUnitView) {
            if (isInit)
                return;
            isInit = true;
            datalist = new List<bool>();
            //对其数量
            for (int i = 0; i < MapEditor.my_map_data.list_area_grid.Count; i++) {
                datalist.Add(false);
            }
            //至少显示一个
            if (datalist.Count > 0)
                datalist[0] = true;

            SyncStringData();
            //s_inst = new SerializedObject(target);
            m_GUIList = new ReorderableList(datalist, typeof(List<bool>), false, true, true, true);
            ResetArea = ResetUnitView;
            // Initialize a temporary list of element heights to be used later on in the draw function
            heights = new List<float>(datalist.Count);

            m_GUIList.index = 0;
            m_GUIList.drawElementCallback += _EventList_DrawElementCallback;
            //TODO 暂时不让排序
            m_GUIList.onReorderCallback = (ReorderableList list) => {
                AreaInfo temp = MapEditor.my_map_data.list_area_grid[now_area_index];
                MapEditor.my_map_data.list_area_grid.RemoveAt(now_area_index);
                MapEditor.my_map_data.list_area_grid.Insert(list.index, temp);
                //不刷新
                now_area_index = list.index;
                //s_inst.ApplyModifiedProperties();
                //Debug.Log("出现list.index与实际长度不符，当前："+list.index);
            };
            m_GUIList.drawHeaderCallback = (Rect rect) =>
            {  
                EditorGUI.LabelField(rect, "Area List");
            };
            m_GUIList.onChangedCallback = (ReorderableList list) =>
            {
                //NOTE: When reordering elements in ReorderableList, elements are not moved, but data is swapped between them.
                // So if you keep addres of element 0 ex: data = list[0], after reordering element 0 with 1, data will contain the elemnt1 data.
                // Keeping a reference to MapLayer in TileChunks is useless

                //s_inst.ApplyModifiedProperties(); // apply adding and removing changes
                //s_inst.Update();
//                SyncStringData();
                ResetArea ();
//                MyAutoTileMap.SaveMap();
//                MyAutoTileMap.LoadMap();
            };
            m_GUIList.onSelectCallback = (ReorderableList list) => {
                now_area_index = m_GUIList.index;
//                editing_event = new UnityEditor.SerializedObject(datalist[now_area_index]);
                ResetArea();
//                Debug.Log (datalist [list.index] + "/" + datalist.Count);
//                Debug.Log(m_GUIList.serializedProperty.GetArrayElementAtIndex(list.index));
//                Debug.Log(m_GUIList.serializedProperty.CountInProperty());
//                Debug.Log(m_GUIList.serializedProperty.GetArrayElementAtIndex(list.index).FindPropertyRelative ("event_id"));
            };
            m_GUIList.onAddCallback = (ReorderableList list) =>
            {
                list.index = datalist.Count;
                NewElement();
                //s_inst.ApplyModifiedProperties(); 
                now_area_index = m_GUIList.index;
                ResetArea();
                SyncStringData();
            };
            m_GUIList.onRemoveCallback = (ReorderableList list) => {
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
                SyncStringData();
                MapEditor.my_map_data.list_area_grid.RemoveAt(list.index);
                //s_inst.ApplyModifiedProperties();
                now_area_index = m_GUIList.index;
                ResetArea();
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
            //s_inst.Update();
            m_GUIList.DoLayoutList();
            //s_inst.ApplyModifiedProperties();
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

            //SerializedProperty itemData = m_GUIList.serializedProperty.GetArrayElementAtIndex(index);

            rect.y += 3;
            rect.height = EditorGUIUtility.singleLineHeight-1;
            //float elemY;
            float elemX = rect.x;float elemWidth = 10; 
            //编号
            EditorGUI.LabelField (new Rect(elemX,rect.y,elemWidth,rect.height),index.ToString());

            elemX += elemWidth; elemWidth = 20;
            EditorGUI.BeginChangeCheck();
            datalist[index] = EditorGUI.Toggle(new Rect(elemX, rect.y, elemWidth, rect.height), datalist[index]);
            EditorGUI.EndChangeCheck();
            if (GUI.changed)
                ResetArea();
            //elemX += elemWidth; elemWidth = 250;
            //EditorGUI.PropertyField (new Rect(elemX,rect.y,elemWidth,rect.height)
                //, itemData.FindPropertyRelative ("event_id"), GUIContent.none);
        }
        void resizeHeightList(){
            float[] floats = heights.ToArray();
            Array.Resize(ref floats, datalist.Count);
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

