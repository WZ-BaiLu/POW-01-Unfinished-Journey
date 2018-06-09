using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Unstopable : MonoBehaviour {
    public float HPS = 1;
    private float nowHue = 0;
    public Text refText;
    public float delay = 10;
	// Use this for initialization
	void Start () {
// 	    if (refText==null) {
//             refText = GetComponent<Text>();
// 	    }
	    
	}
	
	// Update is called once per frame
    void Update() {
        if (delay <= 0) {
            refText.color = BKKZ.POW01.BKTools.HSVtoRGB(nowHue, 1, 1);
            nowHue += Time.deltaTime * HPS;
            if (nowHue > 360) {
                nowHue -= 360;
            }
        } else {
            delay -= Time.deltaTime;
            if (delay<=0) {
                refText.gameObject.SetActive(true);
            }
        }
    }
}
