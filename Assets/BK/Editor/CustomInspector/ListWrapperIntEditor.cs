using UnityEditor;
using UnityEngine;
namespace BKKZ.POW01 {
    [CustomEditor(typeof(AreaInfo))]
    public class ListWrapperIntEditor : Editor {

        public override void OnInspectorGUI() {
            //base.OnInspectorGUI();
            EditorGUILayout.LabelField("这是一个Int数组");
        }
    }
}