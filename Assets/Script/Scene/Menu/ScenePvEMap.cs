using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BKKZ.POW01.Scene {
    public class ScenePvEMap : MonoBehaviour {

        // Use this for initialization
        void Start() {
            //每次回到选关都重新加载，避免之前修改残留（Load函数都已经加了重置
            SceneLoading.LoadBundle_PvE_Level();
        }

        // Update is called once per frame
        void Update() {

        }
        void OnGUI() {
            ListLevels();
        }
        void ListLevels() {
            string[] scenePaths = SceneLoading.bundle_pve_level.GetAllScenePaths();
            GUILayout.Label("选择关卡：");
            foreach (var item in scenePaths) {
                if (GUILayout.Button(item)) {
                    SceneLoading.GoLevel = item;
                    UnityEngine.SceneManagement.SceneManager.LoadScene("LoadingToPvE");
                }
            }
        }
    }
}