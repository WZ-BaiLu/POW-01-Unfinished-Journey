using UnityEngine;
using System.Collections;

public class Light_Beat : MonoBehaviour
{
    public float min_intensity = 1f;
    public float max_intensity = 1f;
    public float beatinterval = 1f;
    private bool incrase = false;
    private float timecount = 0;
    private Light light;

    public GameObject lookat;
	// Use this for initialization
	void Start () {
        light = GetComponent<Light>();
	}
	
	// Update is called once per frame
	void Update () {
        if (incrase)
            light.intensity = Mathf.Lerp(max_intensity, min_intensity, timecount / beatinterval);
        else
            light.intensity = Mathf.Lerp(min_intensity, max_intensity, timecount / beatinterval);
        if (timecount > beatinterval)
        {
            incrase = !incrase;
            timecount = 0;
        }
        timecount += Time.deltaTime;

        transform.LookAt(lookat.transform);
	}
}
