using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CureText : MonoBehaviour {
    float time_count = 0;
    public float duration;
    public Vector3 speed;
    public float scale_start;
    public float scale_end;
    float scale_time_count = 0;
    public float scale_duration;
    public float drag = 1;

    public Text text;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        if (time_count > duration) {
            Destroy(gameObject);
        }
        Anim();

        time_count += Time.deltaTime;
        
    }
    void Anim() {


        if (scale_time_count <= scale_duration) {
            transform.localScale = Vector3.Lerp(Vector3.one * scale_start, Vector3.one * scale_end, time_count / scale_duration);
            scale_time_count += Time.deltaTime;
        } else {
            transform.localScale = Vector3.one * scale_end;
        }
        transform.position += speed;
        //transform.localScale = Vector3.one * 5;

        //减速
        speed *= drag;
    }
}
