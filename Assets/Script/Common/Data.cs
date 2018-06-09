#define BATTLE_SCENE_TEST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace BKKZ.POW01 {
    public class Data {
        private static Data instance;
        private string str_input_data;
        public static int DAMAGE_WAVE_MIN = -5;
        public static int DAMAGE_WAVE_MAX = 4;
        public static bool DEAD_ENDING_TURN = true; //1翻棋 2击飞
        public static float DELAY_AFTER_SKILL = 1f;
        public static bool MOVE_COST_HANDCARD = true;

        public static int[][] card_data_old;
        public static bool has_init = false;
        public Dictionary<int, Skill_Info> skill_data = new Dictionary<int, Skill_Info>();
        public Dictionary<int, Card_Info> card_data = new Dictionary<int, Card_Info>();
        public Dictionary<int, BKKZ.POW01.Buff_Info> buff_data = new Dictionary<int, BKKZ.POW01.Buff_Info>();
        //编辑器展示用
#if UNITY_EDITOR
        public static string[] arr_Chess_name;
        public static int[] arr_Chess_key;
#endif
        //关联
        public static string MAIN_CANVAS = "Canvas_Nanahara";

        public static Data Inst {
            get {
                if (instance == null) {
                    instance = new Data();
                    ReadText.initData(instance);

#if UNITY_EDITOR
                    //编辑器 显示用
                    arr_Chess_name = (from card_info in instance.card_data
                                      select card_info.Value.name).ToArray();
                    arr_Chess_key = (from card_info in instance.card_data
                                     select card_info.Key).ToArray();
#endif
                }
                return instance;
            }
        }
        public void setData(string _s_input) {
            str_input_data = _s_input;

            string[] arr_s = str_input_data.Split(';');
            string[] system_set = arr_s[0].Split(',');
            if (system_set.Length < 3) {
                Console.WriteLine("数据不正确!");
                return;
            }
            DAMAGE_WAVE_MIN = int.Parse(system_set[0]);
            DAMAGE_WAVE_MAX = int.Parse(system_set[1]);
            //DEAD_ENDING_TURN = int.Parse(system_set[2]);
            if (int.Parse(system_set[2]) == 1) {
                DEAD_ENDING_TURN = true;
            } else {
                DEAD_ENDING_TURN = false;
            }

            card_data_old = new int[arr_s.Length - 2][];    // warning 最后一个分号会造成空行。
            string[] _sp;
            for (int i = 1; i < arr_s.Length; i++) {
                _sp = arr_s[i].Split(',');
                if (_sp[0] == "") {
                    break;
                }
                card_data_old[i - 1] = new int[_sp.Length];
                for (int j = 0; j < _sp.Length; j++) {
                    card_data_old[i - 1][j] = int.Parse(_sp[j]);
                }

                //card_data[i-1] = arr_s[i].Split(',');
            }
        }
        public void CastData(ref int[][] data) {
            if (card_data_old == null) {
                #region 
                data = new int[16][];
                //COST SPD MANA POW VCT
                data[0] = new int[5] { 5, 2, 150, 25, 0 };
                data[1] = new int[5] { 3, 2, 100, 25, 2 };
                data[2] = new int[5] { 6, 3, 150, 25, 0 };
                data[3] = new int[5] { 6, 2, 150, 50, 0 };
                data[4] = new int[5] { 4, 3, 100, 25, 2 };
                data[5] = new int[5] { 4, 3, 100, 25, 2 };
                data[6] = new int[5] { 4, 3, 50, 25, 2 };
                data[7] = new int[5] { 5, 2, 100, 50, 1 };
                data[8] = new int[5] { 6, 1, 100, 100, 1 };
                data[9] = new int[5] { 3, 1, 100, 25, 2 };
                data[10] = new int[5] { 3, 2, 50, 25, 2 };
                data[11] = new int[5] { 5, 1, 50, 50, 1 };
                data[12] = new int[5] { 5, 2, 150, 50, 0 };
                data[13] = new int[5] { 6, 3, 150, 50, 0 };
                data[14] = new int[5] { 3, 2, 50, 25, 2 };
                data[15] = new int[5] { 4, 1, 150, 25, 0 };
            }
            #endregion
        else {
                data = new int[card_data_old.Length][];
                for (int i = 0; i < card_data_old.Length; i++) {
                    data[i] = (int[])card_data_old[i].Clone();
                }
            }
        }
    }

}