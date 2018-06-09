using UnityEngine;
using UnityEngine.UI;
using System.Collections;
namespace BKKZ.POW01 {
    public class CardContainer : MonoBehaviour {
        RectTransform rect_tran;
        public ePlayer owner;
        public Rect std_pos;
        //public Outline outline;
        // Use this for initialization
        void Start() {
            //outline = transform.GetChild()GetComponent<Outline>();
            rect_tran = GetComponent<RectTransform>();
            std_pos = rect_tran.rect;
        }

        // Update is called once per frame
        void Update() {

        }
        public Card setOutlineOn() {
            if (transform.childCount <= 0)
                return null;
            Card obj = transform.GetChild(0).gameObject.GetComponent<Card>();
            if (obj != null) {
                obj.setMaskOn();
                return obj;
            }
            return null;
        }
        public Card setOutlineOff() {
            if (transform.childCount <= 0)
                return null;
            Card obj = transform.GetChild(0).gameObject.GetComponent<Card>();
            if (obj != null) {
                obj.setMaskOff();
                return obj;
            }
            return null;
        }
    }
}