using UnityEngine;
//using UnityEditor.EditorGUIUtility;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
namespace BKKZ.POW01 {
    public class PlayerBoard : MonoBehaviour {
        public ePlayer ID;
        public Camera camera_fx;
        public Camera camera_ui;
        public Image SANBAR;
        public Image SANBAR_far;
        public Text txt_SAN;
        public Text txt_SAN_far;
        public Text txt_Deck_Remain;
        public Text txt_Deck_Remain_far;
        //手牌
        public List<Card> deck_cards; //CARD
                                      //public ArrayList deck_p2;
        public List<Card> hand_cards; //CARD
                                      //public ArrayList hand_p2;
        public float sb_default_hight = 20f;
        public float sb_default_width = 200f;
        public float sb_default_hight_far = 20f;
        public float sb_default_width_far = 200f;
        public GameObject fx_damage;
        //attribute
        public Vector3 turn_Position = new Vector3(240.3f, -153.3f, 0);
        public Vector3 oppoturn_Position = new Vector3(240.3f, -353.3f, 0);
        public int SAN = 100;   //血量
        int SAN_MAX = 100;      //总血量

        //VFX
        public GameObject fx_prefab;
        static GameObject fx_shining_hand = null;
        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }
        public void onDamage(int damage) {
            SAN -= damage;
            if (SAN <= 0) {
                SAN = 0;
                Main.Inst.GameOver(ID);
                //StartCoroutine(GameoverCorotine(ID));
            }
            //界面处理
            float hprate = 1f * SAN / SAN_MAX;
            if (Main.Inst.turn_player == ID) {
                SANBAR.rectTransform.sizeDelta = new UnityEngine.Vector2(sb_default_width * hprate, sb_default_hight);
                txt_SAN.text = SAN.ToString();
            } else {
                SANBAR_far.rectTransform.sizeDelta = new UnityEngine.Vector2(sb_default_width_far * hprate, sb_default_hight_far);
                txt_SAN_far.text = SAN.ToString();
            }

            //Color c = Main.HSVtoRGB(hprate * 120, 1, 1);
            //SANBAR.color = c;

            if (damage > 0) {
                //FX
                GameObject _o = (GameObject)GameObject.Instantiate(fx_damage, transform.position + new Vector3(0, 0, -1f), transform.rotation);
                //_o.transform.parent = transform;
                Vector3 fxpos;
                if (Main.Inst.turn_player == ID) {
                    // fxpos = camera_ui.ScreenToWorldPoint(SANBAR.transform.position/* - new Vector3(sb_default_width * hprate / Screen.height * 1080, 0, 0)*/);
                    fxpos = SANBAR.transform.position - new Vector3(sb_default_width * hprate / 100 * (1920f / 800), 0, 0);
                } else {
                    // fxpos = camera_ui.ScreenToWorldPoint(SANBAR_far.transform.position/*+ new Vector3(sb_default_width_far * hprate / Screen.height * 1080, 0, 0)*/);
                    fxpos = SANBAR_far.transform.position - new Vector3(sb_default_width_far * hprate / 100 * (1920f / 800), 0, 0);
                }
                fxpos.z = -1;
                _o.transform.position = fxpos;
            }

        }
        IEnumerator GameoverCorotine(ePlayer lusir) {
            Main.Inst.GameOver(lusir);

            float duration = 0.5f;
            int step = 10;
            GameObject showgirl;
            if (lusir == ePlayer.Player2) {
                //showgirl = 
                showgirl = Main.Inst.win_kuroko;
            } else {
                showgirl = Main.Inst.win_shiroi;
            }
            for (int i = 0; i < step; i++) {
                showgirl.transform.Rotate(Vector3.right * 90 / step);
                //Debug.Log("翻转" + i.ToString());
                yield return new WaitForSeconds(duration / step);
            }
            Debug.Log("翻转结束");
            yield return null;
        }
        public void setBorderOn() {
            GetComponent<RectTransform>().localPosition = turn_Position;
        }
        public void setBorderOff() {
            GetComponent<RectTransform>().localPosition = oppoturn_Position;
        }
        public void FreshDeckRemain() {
            if (ID == Main.Inst.turn_player) {
                txt_Deck_Remain.text = deck_cards.Count.ToString();
            } else {
                txt_Deck_Remain_far.text = deck_cards.Count.ToString();
            }
        }
        public bool orenotan_draw() {
            if (deck_cards.Count <= 0) {
                return false;
            }
            Card c;
            //获得一张卡
            c = (Card)deck_cards[0];
            deck_cards.RemoveAt(0);

            hand_cards.Add(c);
            if (fx_shining_hand != null) {
                Destroy(fx_shining_hand);
            }
            fx_shining_hand = (GameObject)GameObject.Instantiate(fx_prefab);
            fx_shining_hand.transform.parent = transform;
            FreshDeckRemain();

            return true;
        }
    }
}