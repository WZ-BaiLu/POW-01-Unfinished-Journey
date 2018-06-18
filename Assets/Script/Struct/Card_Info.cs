using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BKKZ.POW01 {
    public enum eCard_Vocation {
        Protect = 0,  //盾骑 pro
        Attack,     //枪骑 
        Angel,      //天使 ang
        Magic,      //红魔 mgc
        Ghost,      //死灵 ght
        Mico,       //巫女 mco
        Max
    }


    public enum eCSV_Card {
        ID = 0,
        NAME,
        IMG,        //  图片名
        COST,
        SPD,
        MANA,
        CT,         //  攻击力
        VCT,        //  职业
        RARE,        //  稀有度
        STK,        //  强袭距离
        skill01,    //  技能1
        skill02,    //  技能2
        spellcard,  //  特殊效果

    }
    public class Card_Info {
        public static string card_path = "Assets/Res/Image/card_cg/ifo/";  //大图
        public static Dictionary<int, Sprite> dic_id_card_sprite = new Dictionary<int, Sprite>();
        public static string hand_path = "Assets/Res/Image/card_cg/";      //手牌图
        public static Dictionary<int, Sprite> dic_id_hand_sprite = new Dictionary<int, Sprite>();
        public static string chess_path = "Assets/Res/Image/chess/chess_icon/";     //棋子
        public static Dictionary<int, Sprite> dic_id_chess_sprite = new Dictionary<int, Sprite>();
        public static string vocation_path = "Assets/Res/Image/UI/";     //职业
        public static Dictionary<eCard_Vocation, Sprite> dic_vocation_sprite = new Dictionary<eCard_Vocation, Sprite>();
        public static bool is_sprite_init = false;


        public int id;
        public string name;
        public string img;
        public int cost;
        public int spd;
        public int mana;
        public int atk;
        public eCard_Vocation vct;
        public int rare;
        public int stk;
        public int skill01;
        public int skill02;
        public int spellcard;

        public static void initSprite() {
            if (is_sprite_init)
                return;
            Sprite sprite;
            object o;
            Texture2D t2d;
            foreach (KeyValuePair<int, Card_Info> _info in Data.Inst.card_data) {
                //卡牌详情图
                o = BKTools.LoadAsset<UnityEngine.Object>(eResBundle.Image, card_path + _info.Value.img + ".png");
                t2d = o as Texture2D;
                sprite = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), new Vector2(0.5f, 0.5f));
                dic_id_card_sprite.Add(_info.Key, sprite);
                //手牌图
                o = BKTools.LoadAsset<UnityEngine.Object>(eResBundle.Image, hand_path + _info.Value.img + ".png");
                t2d = o as Texture2D;
                sprite = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), new Vector2(0.5f, 0.5f));
                dic_id_hand_sprite.Add(_info.Key, sprite);
                //棋子图
                o = BKTools.LoadAsset<UnityEngine.Object>(eResBundle.Image, chess_path + _info.Value.img + ".png");
                t2d = o as Texture2D;
                sprite = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), new Vector2(0.5f, 0.5f));
                dic_id_chess_sprite.Add(_info.Key, sprite);
            }

            string[] imgs = new string[3];
            imgs[0] = "ion_por";
            imgs[1] = "ion_atc";
            imgs[2] = "ion_ang";
            //手动初始化职业图标
            for (int i = 0; i < (int)eCard_Vocation.Max; i++) {
                o = BKTools.LoadAsset<UnityEngine.Object>(eResBundle.Image, vocation_path + imgs[i % imgs.Length] + ".png");
                t2d = o as Texture2D;
                sprite = Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), new Vector2(0.5f, 0.5f));
                dic_vocation_sprite.Add((eCard_Vocation)i, sprite);
            }
        }
    }
}