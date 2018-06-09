using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace BKKZ.POW01 {
    //记录关卡地图数据
    /// <summary>
    /// 通过性在GameRule中获得
    /// </summary>
    public enum eMapGridType {
        ///黑色，没有地面，边界不可跨越
        Unvailable = 0,
        ///缺口，可以飞过（后期用九宫格自动填充算法在gap位为
        Gap,
        ///灰色
        Normal,
        ///棕色
        Moutain,
        ///淡蓝色
        River,
        ///砖墙纹理
        Castle,
        ///绿色
        Forest,

        Max
    }
    public class eGridGoDir {
        public const int Upper = 1;
        public const int Lower = 1 << 1;
        public const int UpperRight = 1 << 2;
        public const int LowerLeft = 1 << 3;
        public const int UpperLeft = 1 << 4;
        public const int LowerRight = 1 << 5;
    }
    public enum eMapType {
        PvE_Solo = 0,
        PvE_Mult,
        PvP_2P,
    }
    /// <summary>
    /// 用于JSON解析
    /// </summary>
    public class LevelMapData:ScriptableObject {
        public static string[] dicGridColor = new string[(int)eMapGridType.Max];
        private static AssetBundle _bundle_pve_leve_data = null;
        public static AssetBundle Bundle_PvE_Level_Data {
            get {
                if (_bundle_pve_leve_data == null)
                    _bundle_pve_leve_data = AssetBundle.LoadFromFile(LevelBundleDir);
                return _bundle_pve_leve_data;
            }
        }
        public static string LevelBundleDir {
            get {
                return BKTools.Assetbundle_path + BKTools.Assetbundle_Name_By_Platform + "pve_level_data";
                //Application.dataPath + "/../AssetBundles/" + BKTools.assetbundle_name_by_platform + "csv_skill"
            }
        }
        public static string LevelDir {
            get {
#if UNITY_EDITOR
                return Application.dataPath + "/Res/PvELevel/";
#else
                return "";
#endif
            }
        }
        public static Color[] grid_color;
        public static Color[] Grid_Color {
            get {
                if (grid_color == null) {
                    grid_color = new Color[7];
                    grid_color[0] = new Color(1, 1, 1, 0.1f);// 边界
                    grid_color[1] = new Color(0.5f, 0.5f, 1, 0.5f);// 缺口
                    grid_color[2] = new Color(1, 1, 1, 1);// 普通
                    grid_color[3] = new Color(1, 0.62f, 0, 1);// 山
                    grid_color[4] = new Color(0, 0.5f, 1, 1);// 河
                    grid_color[5] = new Color(1, 0, 0, 0.5f);// 城
                    grid_color[6] = new Color(0, 0.83f, 0, 1);// 林
                }
                return grid_color;
            }
        }
        //测试部分

        //数据部分
        public eMapType my_type = eMapType.PvE_Solo;
		public string mapName;
        ///先攻方
		public ePlayer offensive = ePlayer.Player1;
		//与resize有关的数据
		public int[] my_size;
		public eMapGridType[] my_grid_t_type;//地形数据（为了方便使用json，放弃多维数组做法，将所有数据存到同一行）
		public int[] my_grid_godir;
		public int[] my_unit_info_key;	//用于生成mydic_grid_unit<int,unity_info>的key，下面是value
		public string json_grid_unit;	//单位数据(计划数据：位置序号、出场方式、单位ID)
		public string my_event_json;
        public List<EventInfo> list_event_data = new List<EventInfo>();
        public List<UnitInfo> list_unit_data = new List<UnitInfo>();//编辑用，存储时进行翻译
        public List<AreaInfo> list_area_grid = new  List<AreaInfo>();//每个事件对应的区域<格子,事件ID>，该事件是否有操作该格子
        //public string[] event_script;   //2017年11月03日18:24:06 暂时未起效，对应代码做防呆处理
		//以上
		/*
		 * 位置序号：在地图尺寸变化时，超出范围的进行自动约束而非删除
		 * 出场方式：一直存在，按事件序号出场
		 * 单位ID：参见卡牌表格（提供接口，特定编号从服务器获得数据）
		 */
		//起点
		public int[] start_point = new int[2];
		///全局开关 仿RPG Maker设置
		public string[] global_switch = new string[21];
		///全局变量 仿RPG Maker设置
		public string[] global_variable = new string[21];
		///独立开关 仿RPG Maker设置 考虑后觉得用不到
		public static readonly string[] self_switch = {"A","B","C","D"};
        //过时 似乎没什么用 数据
        public static LevelMapData LoadScriptableObject(string name) {
            
            return null;
        }
		public LevelMapData(){
			init_Grid (5,5);
			for (int i = 1; i < global_switch.Length; i++) {
				global_switch [i] = "开关"+(i);
			}
			for (int i = 1; i < global_variable.Length; i++) {
				global_variable [i] = "变量"+(i);
			}
//			my_unit_info = new string[1];
		}
		public LevelMapData(int _row_length){
			init_Grid (_row_length,_row_length);
//			my_unit_info = new string[1];
		}
        //过时了
		void init_Grid(int _row,int _column){
			my_size = new int[]{_row,_column};
			my_grid_t_type = new eMapGridType[_row*_column];
			my_grid_godir = new int[my_grid_t_type.Length];
			int all_dir_clear = eGridGoDir.Upper | eGridGoDir.Lower | eGridGoDir.UpperRight | eGridGoDir.LowerLeft | eGridGoDir.UpperLeft | eGridGoDir.LowerRight;
			for(int x=0;x<my_grid_t_type.Length;x++){
				my_grid_t_type[x] = eMapGridType.Normal;
				my_grid_godir [x] = all_dir_clear;
			}
		}
        /// <summary>
        /// 过时了
        /// </summary>
        /// <returns>The unit data to dic.</returns>
        /// <param name="key">Key.</param>
        /// <param name="arr">Arr.</param>
		public static List<UnitInfo> ParseUnitDataToDic(int[] key,string arr){
			List<UnitInfo> dic = new List<UnitInfo> ();
			for (int i = 0; i < arr.Length; i ++) {
				//if(!dic.ContainsKey(key[i]))
				//	dic.Add(key[i],new ListWrapperUnitinfo());
                ////dic [key [i]].Add (JsonConvert.DeserializeObject<UnitInfo>(arr[i]));
				UnitInfo.Unit_Count++;
			}
			return dic;
		}
        /// <summary>
        /// 过时了
        /// </summary>
        /// <returns><c>true</c>, if event data to dic was parsed, <c>false</c> otherwise.</returns>
        /// <param name="list">List.</param>
        /// <param name="event_arr">Event arr.</param>
        /// <param name="script_arr">Script arr.</param>
		public static bool ParseEventDataToDic(List<EventInfo> list,string[] event_arr,string[] script_arr){
//			int data_length = 3;
//			if (arr.Length % data_length != 0) {
//				Debug.Log ("地图数据错误（文件内数据转换成编辑器数据时）");
//				return false;
//			}
//			for (int i = 0; i < arr.Length; i += data_length) {
//				EventInfo.event_count++;
//				EventInfo info = new EventInfo ();
//				info.event_id = arr [i];
////				info.condition = int.Parse (arr [i + 1]);
//				info.description = arr [i + 2];
			for (int i = 0; i < event_arr.Length; i ++) {
				EventInfo _evt = JsonUtility.FromJson<EventInfo> (event_arr [i]);
                //_evt.drama_script = JsonConvert.DeserializeObject<DramaScript> (script_arr [i]);
				EventInfo.event_count++;
				list.Add (_evt);
                //2017年11月07日06:16:15 整理代码时发现并没有从json读取Section信息的必要，后续找到了再加
				//DramaSection[] arr_section = JsonUtility.FromJson<DramaSection[]>( _evt.drama_script.json);
				//if (arr_section != null && arr_section.Length > 0) {
				//	foreach (var item in arr_section) {
				//		if (item == null)
				//			continue;
				//		_evt.drama_script.Add (item);
				//	}
				//}
			}
			return true;
		}

	}
}