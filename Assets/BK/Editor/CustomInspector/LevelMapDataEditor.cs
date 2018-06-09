using UnityEditor;
using UnityEngine;
namespace BKKZ.POW01 {
    [CustomEditor(typeof(LevelMapData))]
    public class LevelMapDataEditor : Editor {
        LevelMapData mapdata;
        string str = string.Empty;
        void OnEnable() {
            mapdata = target as LevelMapData;
        }
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            return;
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("list_area_grid"));
            for (int i = 0; i < mapdata.list_area_grid.Count; i++) {
                EditorGUILayout.BeginHorizontal();
                str = string.Empty;

                var item = mapdata.list_area_grid[i].list;
                foreach (var grid in item) {
                    str += grid + ",";
                }
                EditorGUILayout.LabelField("区域" + i);
                EditorGUILayout.LabelField(str);
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}