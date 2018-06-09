using UnityEngine;
using System.Collections;

public class BtnToggleUI : MonoBehaviour {
    public GameObject refObj;
	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public void Click() {
        refObj.SetActive(!refObj.active);
    }
}
