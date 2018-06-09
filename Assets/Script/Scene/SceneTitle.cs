using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 初始化
/// 检查更新
/// 进入主菜单
/// </summary>
public class SceneTitle : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		GoToMainLevel();
	}
    void GoToMainLevel(){
        if (Input.anyKeyDown) {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }
    }
}
