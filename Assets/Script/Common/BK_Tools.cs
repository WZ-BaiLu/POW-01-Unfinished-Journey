using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BKKZ.POW01 {
    public enum eDirection {
        None = -1,
        UpperLeft = 0,
        LowerRight,
        Upper,
        Lower,
        UpperRight,
        LowerLeft,

        All	///一般默认状态
	}
    [System.Serializable]
    public class Point3 {

        public Point3() {
            x = 0;
            y = 0;
            z = 0;
        }
        public Point3(int _x, int _y, int _z) {
            x = _x;
            y = _y;
            z = _z;
        }
        public int x;
        public int y;
        public int z;
    }
    public enum eGameState {
        Uninit,
        Start,
        Explore,
        Battle,
        Ending,
        WaitReturn
    }
    public class GameState {
        eGameState _now = eGameState.Uninit;
        public eGameState Now {
            get {
                return _now;
            }
        }
        public void Set(eGameState new_state) {
            _now = new_state;
        }
        public bool CanPhaseRun() {
            if (Main.Inst.game_state.Now == eGameState.Uninit)
                return false;
            if (Main.Inst.game_state.Now == eGameState.WaitReturn)
                return false;
            return true;
        }
    }
    public class DeadInfo {
        public Chess killer;
        public Chess turn_to;
    }
    public class DamageInfo {
        public Chess source_actor;
        public Chess source_partner;
        public int dmg; //唯一必要
        public DamageInfo(int _dmg) {
            dmg = _dmg;
        }
        public DamageInfo setActor(Chess _c) {
            source_actor = _c;
            return this;
        }
        public DamageInfo setPartner(Chess _c) {
            source_partner = _c;
            return this;
        }
        public DamageInfo setDMG(int _dmg) {
            dmg = _dmg;
            return this;
        }

    }






    public interface ILocation_Scope {
        eSkill_Scope[] iLS_Scope { get; }
        eSkill_Scope_Locator iLS_Locater { get; }
        Point3 iLS_Point { get; }
        int iLS_Depth { get; }
    }
    public enum eResBundle {
        BaseContent = 0,
        Ef, //历史遗留
        Image,  //历史遗留
        Prefabs,
        PvE_Level_Data, //关卡数据
        CSV_Buff,
        CSV_Card,
        CSV_Skill,
        PvE_Level   //关卡
    }
    public class BKTools {
        public static float AngularByDirection(eDirection dir) {
            switch (dir) {
            case eDirection.LowerRight:
                return 240;
            case eDirection.UpperLeft:
                return 60;
            case eDirection.Lower:
                return 180;
            case eDirection.Upper:
                return 0;
            case eDirection.LowerLeft:
                return 120;
            case eDirection.UpperRight:
                return 300;
            default:
                Debug.Log("Something wrong! Getting unkown dirction of chesscontainner");
                return 0;
            }
        }

        public static Color HSVtoRGB(float h, float s, float v) {
            int i;
            float f, p, q, t;
            float r, g, b;
            if (s == 0) {
                // achromatic (grey)  
                r = v;
                g = v;
                b = v;
                return new Color(r, g, b);
            }
            h /= 60;            // sector 0 to 5  
            i = (int)Mathf.Floor(h);
            f = h - i;          // factorial part of h  
            p = v * (1 - s);
            q = v * (1 - s * f);
            t = v * (1 - s * (1 - f));
            switch (i) {
            case 0:
                r = v;
                g = t;
                b = p;
                break;
            case 1:
                r = q;
                g = v;
                b = p;
                break;
            case 2:
                r = p;
                g = v;
                b = t;
                break;
            case 3:
                r = p;
                g = q;
                b = v;
                break;
            case 4:
                r = t;
                g = p;
                b = v;
                break;
            default:        // case 5:  
                r = v;
                g = p;
                b = q;
                break;
            }
            return new Color(r, g, b);
        }
        public static Vector2 chess_container_size = new Vector2(2.26f, 2.58f);//棋盘格子大小

        /// <summary>
        /// without last '/'
        /// </summary>
        public static string Assetbundle_path {
            get {
                return
#if UNITY_ANDROID
                "jar:file://" + Application.dataPath + "!/assets";  
#elif UNITY_IPHONE
                Application.dataPath + "/Raw";  
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
                Application.dataPath;
#else
                string.Empty;  
#endif
            }
        }
        public static string Assetbundle_Name_By_Platform {
            get {
                return
                "/../AssetBundles/" +
#if UNITY_ANDROID
                "Android/";  
#elif UNITY_IPHONE
                "iOS/";  
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
                "StandaloneWindows/";
#else
                string.Empty;  
#endif
            }
        }
        //可以被LoadAllAssets的Bundle【Scene除外
        public static string[] bundles_dir = new string[] { "/temp/base_content_bundle", "/temp/ef", "/temp/image", "/temp/prefabs", "pve_level_data", "csv_buff", "csv_card", "csv_skill" };
        public static string BattleSceneBundleDir {
            get { return Application.dataPath + "/../AssetBundles/" + BKTools.Assetbundle_Name_By_Platform + "battle_level"; }
        }
        public static Dictionary<string, AssetBundle> dic_battle_scene_content = new Dictionary<string, AssetBundle>();
        public static Dictionary<string, AssetBundle> BundleDic { get { return dic_battle_scene_content; } }
        public static AssetBundle getBundle(eResBundle eID) {
            return BundleDic[bundles_dir[(int)eID]];
        }
        public static GameObject getBundleObject(eResBundle bundle_id,string asset_path) {
            return getBundle(bundle_id).LoadAsset<GameObject>(asset_path);
        }

        public static int ParseInt(string str) {
            if (str == "") {
                return -1;
            } else {
                return int.Parse(str);
            }
        }
        public static int[] ParseInt(string[] str) {
            int[] res = new int[str.Length];
            for (int i = 0; i < str.Length; i++) {
                if (str[i] == "") {
                    res[i] = -1;
                } else {
                    res[i] = int.Parse(str[i]);
                }
            }
            return res;
        }

        public static GameObject addVFX(string obj) {
            return addVFX(BKTools.getBundleObject(eResBundle.Prefabs, obj));
        }
        public static GameObject addVFX(GameObject obj) {
            if (obj == null)
                return null;
            GameObject vfx = GameObject.Instantiate(obj);
            return vfx;
        }
        public static BK_AnimEvts addVFX_Dancer(string obj) {
            return addVFX_Dancer(BKTools.getBundleObject(eResBundle.Prefabs, obj));
        }
        public static BK_AnimEvts addVFX_Dancer(GameObject obj) {
            if (obj == null)
                return null;
            GameObject vfx = GameObject.Instantiate(obj);
            BK_AnimEvts aes = vfx.GetComponent<BK_AnimEvts>();
            if (aes == null) {
                return null;
            }
            //Camera.main.GetComponent<Main>().addDancer(aes.key);
            aes.Start_Add_Dancer();
            return aes;
        }
        public static ChessContainer LocateChessGrid(ChessContainer now_cc, Point3 relativePosition) {
            ChessContainer cc;
            cc = now_cc;
            for (int x = 0; x < Mathf.Abs(relativePosition.x); x++) {
                if (relativePosition.x > 0) {
                    cc = cc.CCLowerRight;//Main.Instance.getRightLowerChessContainer(cc);
                } else {
                    cc = cc.CCUpperLeft;//Main.Instance.getLeftUpperChessContainer(cc);
                }
            }
            for (int y = 0; y < Mathf.Abs(relativePosition.y); y++) {
                if (relativePosition.y > 0) {
                    cc = cc.CCLower;//Main.Instance.getLowerChessContainer(cc);
                } else {
                    cc = cc.CCUpper;//Main.Instance.getUpperChessContainer(cc);
                }
            }
            for (int z = 0; z < Mathf.Abs(relativePosition.z); z++) {
                if (relativePosition.z > 0) {
                    cc = cc.CCLowerLeft;
                } else {
                    cc = cc.CCUpperRight;
                }
            }
            return cc;
        }
        public static List<ChessContainer> GetSkillScope(ILocation_Scope skill, ChessContainer now_cc) {
            return GetSkillScope(skill, now_cc, eDirection.All);
        }
        public static List<ChessContainer> GetSkillScope(ILocation_Scope skill, ChessContainer now_cc, eDirection direction_limit) {
            List<ChessContainer> list = new List<ChessContainer>();
            ChessContainer cc;
            switch (skill.iLS_Locater) {
                case eSkill_Scope_Locator.Board_Location:
                    //cc = Main.Instance
                    cc = Main.Inst.chess_grids[getChessContainer(skill.iLS_Point.x, skill.iLS_Point.y)];
                    break;
                case eSkill_Scope_Locator.Chess_Location:
                    cc = now_cc;
                    break;
                case eSkill_Scope_Locator.Related_Location:
                    //Debug.Log("谁让你填相对坐标的，没人告诉你功能没做好吗! 技能ID" + skill.id);
                    cc = LocateChessGrid(now_cc, skill.iLS_Point);
                    //                for (int x = 0; x < Mathf.Abs(skill.iLS_Point.x); x++) {
                    //                    if (skill.iLS_Point.x > 0) {
                    //                        cc = Main.Instance.getRightLowerChessContainer(cc);
                    //                    } else {
                    //                        cc = Main.Instance.getLeftUpperChessContainer(cc);
                    //                    }
                    //                }
                    //                for (int y = 0; y < Mathf.Abs(skill.iLS_Point.y); y++) {
                    //                    if (skill.iLS_Point.y > 0) {
                    //                        cc = Main.Instance.getLowerChessContainer(cc);
                    //                    } else {
                    //                        cc = Main.Instance.getUpperChessContainer(cc);
                    //                    }
                    //                }
                    //                for (int z = 0; z < Mathf.Abs(skill.iLS_Point.z); z++) {
                    //                    if (skill.iLS_Point.z > 0) {
                    //                        cc = Main.Instance.getLeftLowerChessContainer(cc);
                    //                    } else {
                    //                        cc = Main.Instance.getRightUpperChessContainer(cc);
                    //                    }
                    //                }
                    break;
                default:
                    Debug.Log("神特么default。技能起点信息错误!");
                    return null;
            }
            list.Add(cc);

            foreach (eSkill_Scope _scope in skill.iLS_Scope) {
                switch (_scope) {
                    case eSkill_Scope.Circle:             //  一圈
                        //cc.search_around(ref list, 0, skill.iLS_Depth);
                        list = getAroundGrid(skill.iLS_Depth,list);
                        break;
                    case eSkill_Scope.Up:                 //  上
                    case eSkill_Scope.Down:               //  下
                    case eSkill_Scope.RightUp:            //  右上
                    case eSkill_Scope.LeftDown:           //  左下
                    case eSkill_Scope.LeftUp:             //  左上
                    case eSkill_Scope.RightDown:          //  右下
                        int index = (int)_scope - 1;
                        ChessContainer _cc = cc;
                        for (int i = 0; i < skill.iLS_Depth; i++) {
                            // 限制攻击范围
                            if (direction_limit != eDirection.All && i != (int)direction_limit)
                                continue;
                            _cc = Main.Inst.dGetChessContainer[index](_cc);
                            if (_cc != null) {
                                list.Add(_cc);
                            } else {
                                break;
                            }
                        }
                        break;
                    default:
                        Debug.Log("技能范围，形状参数错误。");
                        break;
                }
            }
            return list;
        }



        public static List<ChessContainer> getArrivableGrid(Chess c) {
            List<ChessContainer> res_list = new List<ChessContainer>();
            List<ChessContainer> search_list = new List<ChessContainer>();
            search_list.Add(c.container);
            search_list = getAroundGrid(c.attribute.Spd, search_list);
            foreach (var item in search_list) {
                if (item.my_chess == null && GameRule.judgePassable(c, item)) {
                    res_list.Add(item);
                }
            }
            return res_list;
        }


        /// <summary>
        /// 统一的搜索周围棋盘格方法，默认不进行可移动检测
        /// 默认的使用方式需要在seartchlist中包含起点，这会造成范围包含起点，对于技能而言是能接受的。这点在移动中单独去除
        /// 这个判断不包含迷雾
        /// </summary>
        /// <returns>The around grid.</returns>
        /// <param name="level_remain">Level remain.</param>
        /// <param name="searchlist">Searchlist.</param>
        public static List<ChessContainer> getAroundGrid(int level_remain, List<ChessContainer> searchlist) {
            return getAroundGrid(level_remain,searchlist,false,true,null);
        }
        /// <summary>
        /// 统一的搜索周围棋盘格方法
        /// </summary>
        /// <returns>The around grid.</returns>
        /// <param name="level_remain">Level remain.</param>
        /// <param name="searchlist">Searchlist.</param>
        /// <param name="list_depth">保存对应结果的深度，以最开始的level_remain计算，其结果是里搜索边界的距离.</param>
        /// <param name="check_available">If set to <c>true</c> 会排除空格、障碍的格子.</param>
        /// <param name="ignore_chess">If set to <c>false</c> 会排除有棋子的格子.</param>
        public static List<ChessContainer> getAroundGrid(int level_remain, List<ChessContainer> searchlist, bool check_available,bool ignore_chess, List<int>list_depth) {
            if (level_remain == 0)
                return searchlist;
            List<ChessContainer> list_this_time = new List<ChessContainer>();
            foreach (var item in searchlist) {
                foreach (var dGet in Main.Inst.dGetChessContainer) {
                    var new_grid = dGet(item);
                    if (new_grid == null)
                        continue;
                    if (searchlist.Contains(new_grid))
                        continue;
                    //TODO 可站立、可移动检测，可能会做飞行、游泳等特色，目前只有间隙不可用
                    if (check_available && (new_grid.terrain_type == eMapGridType.Gap || new_grid.terrain_type == eMapGridType.Unvailable))
                        continue;
                    //上方有棋子时
                    if (!ignore_chess && new_grid.my_chess!=null)
                        continue;
                    
                    //					if (new_grid.my_chess != null)
                    //						continue;
                    list_this_time.Add(new_grid);
                    //深度记录
                    //if (list_depth != null)
                        //list_depth.Add(level_remain);
                }
            }
            foreach (var item in list_this_time) {
                if (!searchlist.Contains(item)) {
                    searchlist.Add(item);
                    //深度记录
                    if (list_depth != null)
                        list_depth.Add(level_remain);
                }
                //else{
                    //深度更新(深度好像没必要更新，越更新越大)
                    //if (list_depth != null) {
                    //    int index = searchlist.FindIndex(x => x == item)
                    //    list_depth[index] = level_remain;
                    //}
                //}
            }
            level_remain--;
            return getAroundGrid(level_remain, searchlist,check_available,ignore_chess,list_depth);
        }
        public static bool IsTargetFit(eSkill_TargetBelong target_side, ePlayer target, ePlayer self_side) {
            switch (target_side) {
                case eSkill_TargetBelong.All:
                    return true;
                case eSkill_TargetBelong.Both_Player:
                    return target == ePlayer.Player1 || target == ePlayer.Player2;
                case eSkill_TargetBelong.Opponent:
                    return target != self_side && target != ePlayer.None;
                case eSkill_TargetBelong.Teammate:
                    return target == self_side;
                case eSkill_TargetBelong.Scene:
                    return target == ePlayer.None;
                default:
                    Debug.Log("技能目标错误");
                    break;
            }
            return false;
        }
        //过时了
        public static int getChessContainer(int column, int row) {
            Debug.LogWarning("调用了过时的方法");
            return Main.chess_count_line[column] + row;
        }

        public static System.Func<bool> ISCWithArgu(string[] str) {
            System.Func<bool> res = () => Main.Inst.isStageClear(str);
            return res;
        }
        public static System.Func<bool> ISCWithArgu(string str) {
            System.Func<bool> res = () => Main.Inst.isStageClear(new string[] { str });
            //			System.Func<bool> res = Main.Instance.isStageClear;
            return res;
        }
        /// <summary>
        /// Fogs the lift.
        /// </summary>
        /// <param name="center_grid_id">Center grid identifier.</param>
        /// <param name="range">Range. 0包含起点，1就是周围一圈</param>
        /// <param name="visible_border">Visible border.</param>
        /// <param name="px">P1 P2 P3 P4 Enemy1 E2.</param>
        public static void FogLift(int center_grid_id,int range,int visible_border,int[] px){
            List<ChessContainer> list = new List<ChessContainer>();
            list.Add(Main.Inst.chess_grids[center_grid_id]);
            //.FogLift()
            List<int> list_depth = new List<int>();
            list_depth.Add(range+1);
            getAroundGrid(range, list, false, true, list_depth);
            for (int i = 0; i < list.Count; i++) {
                var item = list[i];
                //除了边境都完全驱散
                item.FogLift(list_depth[i] > visible_border, px);
                //yield return new WaitForSeconds(0.1f);
            }
        }
        public static void FogCover(int center_grid_id, int range, int[] px) {
            List<ChessContainer> list = new List<ChessContainer>();
            list.Add(Main.Inst.chess_grids[center_grid_id]);
            getAroundGrid(range, list, false, true, null);
            for (int i = 0; i < list.Count; i++) {
                var item = list[i];
                //除了边境都完全驱散
                item.FogCover(px);
                //yield return new WaitForSeconds(0.1f);
            }
        }
    }

    public enum E_XMLLayoutType {
        Horizontal = 0,
        Vertical,
    }
    /// <summary>
    /// XML快速编辑器生成工具
    /// 按照想在编辑器显示的顺序排布变量
    /// 多重分组用/斜杠表示层级【弃用】
    /// 关联数据使用字符串反射，类名部分单独用分号隔开
    /// </summary>
    public class XMLLayoutAttribute : System.Attribute {
        string _sheet = null;
        //int _sequence_number; //通过排序来实现
        string _displayname;

        public string Displayname {
            get {
                return _displayname;
            }
        }

        string _intergrate_key = null;   //某某类的
        public string IntergrateKey { get { return _intergrate_key; } }
        //string[] _intergrate_key_member = null;  
        //public string[] IntergrateKeyMember { get { return _intergrate_key_member; } }

        string _intergrate_value = null;   //某某类的
        public string IntergrateValue { get { return _intergrate_value; } }
        //string[] _intergrate_value_member = null;  //
        //public string[] IntergrateValueMember { get { return _intergrate_value_member; } }


        public XMLLayoutAttribute(string displayname) {
            _displayname = displayname;
            //_sequence_number = sequence_number;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:BKKZ.POW01.XMLLayoutAttribute"/> class.
        /// , Assembly-CSharp
        /// , Assembly-CSharp-Editor
        /// </summary>
        /// <param name="displayname">Displayname.</param>
        /// <param name="str_key">String key.</param>
        public XMLLayoutAttribute(string displayname, string str_key) {
            _displayname = displayname;
            _intergrate_key = str_key;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:BKKZ.POW01.XMLLayoutAttribute"/> class.
        /// , Assembly-CSharp
        /// , Assembly-CSharp-Editor
        /// </summary>
        /// <param name="displayname">Displayname.</param>
        /// <param name="str_sky">String sky.</param>
        /// <param name="str_value">String value.</param>
        public XMLLayoutAttribute(string displayname, string str_sky, string str_value) {
            _displayname = displayname;
            _intergrate_key = str_sky;
            _intergrate_value = str_value;
        }
        //SerializedObject so = new SerializedObject(_unit);
        //EditorGUILayout.PropertyField(so.FindProperty("m_unit_type"));
        //so.ApplyModifiedProperties();
    }


#if XML_GROUP_ENABLE
    //[System.AttributeUsage(System.AttributeTargets.All,
                   //AllowMultiple = true)]
    public class XMLLayoutGroupAttribute : System.Attribute {
        //string _group = null;
        string _group_name = null;
        E_XMLLayoutType _layout_type = E_XMLLayoutType.Horizontal;
        public XMLLayoutGroupAttribute(string group_name, E_XMLLayoutType layout_type) {
            _group_name = group_name;
            _layout_type = layout_type;
        }
        public string GroupName{
            get{return _group_name;}
        }
        public E_XMLLayoutType LayoutType {
            get { return _layout_type; }
        }
    }
#endif
}