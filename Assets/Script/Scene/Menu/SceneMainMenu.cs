using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMainMenu : MonoBehaviour {
    public string scene_level_name = "Level1";
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	}
    public void ClickLevel(int level_id){
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene_level_name);
    }
    private void OnGUI() {

        //GUILayout.BeginArea(new Rect(100, 100, 1600, 900));
        SimpleGUI();
        //GUILayout.EndArea();
    }
    public GUIStyle style;
    void SimpleGUI() {
        
        // GUILayout.BeginArea(new Rect(10, 10, 100, 100));
        {
            if(GUILayout.Button("PvE Level")){
                UnityEngine.SceneManagement.SceneManager.LoadScene("PvE_Map");
            }
            if(GUILayout.Button("PvP")){
                UnityEngine.SceneManagement.SceneManager.LoadScene("PvP");
            }
            if(GUILayout.Button("DeckBuild")){
                UnityEngine.SceneManagement.SceneManager.LoadScene("DeckBuild");
            }
            if(GUILayout.Button("Setting")){
                UnityEngine.SceneManagement.SceneManager.LoadScene("Setting");
            }
            if(GUILayout.Button("Aboat")){
                UnityEngine.SceneManagement.SceneManager.LoadScene("Aboat");
            }
            if(GUILayout.Button("Quit")){
                Application.Quit();
            }
        }
        //GUILayout.EndArea();
    }
}
