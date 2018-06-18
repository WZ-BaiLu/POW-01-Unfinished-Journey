using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILevelMain : MonoBehaviour {
    //对话
    public RectTransform PanelDialoge;
    public Text textDialoge;

    //回调
    private Action DialogeCallback;

    static UILevelMain instance = null;
    public static UILevelMain Inst {
        get {
            return instance;
        }
    }
	// Use this for initialization
	void Start () {
        instance = this;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ShowDialoge(string dialoge,Action callback){
        textDialoge.text = dialoge;
        PanelDialoge.gameObject.SetActive(true);
        DialogeCallback = callback;
    }
    public void HideDialoge(){
        PanelDialoge.gameObject.SetActive(false);
        DialogeCallback();
    }
}
