using UnityEngine;
using System.Collections;
using BKKZ.POW01;

public class BuffAnimator : MonoBehaviour {
    public CBuffContain owner;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void TimeOut()
    {
        if (owner != null) 
        {
            //owner.vfx_duration.Clear();
            //StartCoroutine(playNext());

            Debug.Log("删除" + owner.belong.ToString() + ":" + gameObject.name);
            owner.b_vfx_running = false;
            owner.playNextVFX();
        }
        Destroy(gameObject, 0.2f);
    }

    IEnumerator playNext()
    {
        yield return new WaitForSeconds(0.1f);
        Debug.Log("删除" + owner.belong.ToString() + ":" + gameObject.name);
        owner.b_vfx_running = false;
        owner.playNextVFX();
        Destroy(gameObject, 0.2f);
        yield return null;
    }
}
