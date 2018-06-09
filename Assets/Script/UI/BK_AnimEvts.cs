using UnityEngine;
using System.Collections;
namespace BKKZ.POW01 {
    public class BK_AnimEvts : MonoBehaviour {
        public bool addTrigerOnce = false;
        public bool removeTrigerOnce = false;
        public string key;  //动画名，proto阶段用于debug
                            // Use this for initialization
        void Start() {
            //         if (key!="") {
            //             addTrigerOnce = true;
            //             Camera.main.GetComponent<Main>().addDancer(key);
            //         }
            if (addTrigerOnce == false) {
                Debug.Log(gameObject.name + "没加过");
            }
        }

        // Update is called once per frame
        void Update() {

        }

        public void Start_Add_Dancer() {
            addDancer();
        }
        public void End_Reduce_Dancer_Delete() {
            reduceDancer();
            Destroy(gameObject);
        }
        public void End_And_Delete() {
            Destroy(gameObject);
        }
        void OnDestroy() {
            reduceDancer();
            Debug.Log("哎呀~~被删掉啦~~ :" + key + "/" + gameObject.name);
        }

        void addDancer() {
            if (addTrigerOnce) {
                return;
            }
            addTrigerOnce = true;
            Main.Inst.addDancer(key);
        }
        public void reduceDancer() {
            if (removeTrigerOnce) {
                return;
            }
            Main.Inst.redDancer(key);
            removeTrigerOnce = true;
        }
    }
}
