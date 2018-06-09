using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;
namespace BKKZ.POW01 {
    [RequireComponent(typeof(PolygonCollider2D))]
    public class Card : MonoBehaviour {
        public int card_id;
        public Image img;
        public Image img_vct;
        public Text txt_pow;
        public Text txt_mana;
        public Text txt_cost;
        public Text txt_spd;
        public GameObject vfx_select;
        public ePlayer owner;
        public Action<Card> MouseDown;
        public Action<Card> MouseUp;
        private bool inited = false;
        // Use this for initialization
        void Start() {
            if (!inited) {
                init(ePlayer.None);
            }
            //        UnityEngine.UI.Button btn = GetComponent<UnityEngine.UI.Button>();
            //        if (btn!= null) {
            //            btn.onClick.RemoveAllListeners();
            //            btn.onClick.AddListener(cardClick);
            //        }

            //vfx_select = transform.GetChild(0).gameObject.GetComponent<Image>();
        }
        void OnMouseDown() {
            //Debug.Log("天哪你真高");
            //      	Main.Instance.clickCardContainer(transform.parent.gameObject.GetComponent<CardContainer>());
            //		Main.Instance.MouseDownOnCard(this);
            MouseDown(this);
        }
        void OnMouseUp() {
            //Debug.Log ("up card");
            MouseUp(this);
        }
        public void init(ePlayer _owner) {
            owner = _owner;
            //img = GetComponent<Image>();
            inited = true;
            //COST SPD MANA POW
            Card_Info _info = Data.Inst.card_data[card_id];
            //         txt_pow.text = Main.Instance.card_data.data[card_id][3].ToString();
            //         txt_mana.text = Main.Instance.card_data.data[card_id][2].ToString();
            //         txt_cost.text = Main.Instance.card_data.data[card_id][0].ToString();
            //         txt_spd.text = Main.Instance.card_data.data[card_id][1].ToString();
            //        img_vct.sprite = Main.Instance.card_data.knights_vocation_type[Main.Instance.card_data.data[card_id][4]];
            txt_pow.text = _info.atk.ToString();
            txt_mana.text = _info.mana.ToString();
            txt_cost.text = _info.cost.ToString();
            txt_spd.text = _info.spd.ToString();
            img_vct.sprite = Card_Info.dic_vocation_sprite[_info.vct];
        }
        // Update is called once per frame
        void Update() {

        }
        public void OnClick() {

        }

        public void setMaskOn() {
            //vfx_select.color = new Color(1f, 154f/255f, 0, 128f/255f);
            vfx_select.SetActive(true);
        }
        public void setMaskOff() {
            //vfx_select.color = new Color(1f, 154f/255f, 0, 0);
            vfx_select.SetActive(false);
        }
    }

}
