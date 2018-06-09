using UnityEngine;
using System.Collections;

public class BtnEnd : MonoBehaviour {
	// Use this for initialization
	void Start () {
        UnityEngine.UI.Button btn = GetComponent<UnityEngine.UI.Button>();
        if (btn != null) {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(onClick);
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    void onClick() {
        BKKZ.POW01.Main.Inst.DamageOnOppoPlayer(150);
    }
}
