using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;
namespace BKKZ.POW01{
	/// <summary>
	/// 实际运行时，从数据文件解析后的数据
	/// 理论上编辑界面跟此处各有一份。中间隔着一个地图文件
	/// </summary>
	public class LevelController : MonoBehaviour {
        public static AssetBundle BattleSceneBundle = null;
        public LevelMapData map_data;
        public GameObject chess_board;
        public List<ChessContainer> list_grid = new List<ChessContainer>();
        public bool isCreated = false;
		//关卡信息_运行时数据
		//public Dictionary<int,List<UnitInfo>> map_data.mydic_grid_unit;//运行用
		//public List<EventInfo> map_data.list_event_data = null;
        public int total_turn = 0;  //总回合数
		public int now_turn = 0;    //当前回合数，随玩家回合开始+1
                                    //TODO 战斗中回合（战斗）
                                    //TODO 累计战斗回合（战斗）
                                    //TODO 累计步数（探索）
        public bool[] global_switch = new bool[21];//从1取值，保证负数表示的无效值与有效值一一对应
		public int[] global_variable = new int[21];
        //探索模式&战斗模式区域（下方两个区域可以用以来回切换）
        public List<int> AreaExplored = new List<int>();//已探索区域
        public List<int> AreaBattle = new List<int>();//战斗用
        public int ExplorePos = -1; //切换模式时，SL探索模式的位置
		/// <summary>
		/// 触发起点的处理放在具体位置
		/// </summary>
		/// <returns>The IDs,which event condition ready.</returns>
		/// <param name="trigger">Trigger.</param>
		/// <param name="int_argu">骑士站立、贴近事件为格子ID.	点击事件为click_ID</param>
		public int[] isNowEventConditionReady(eEventTrigger trigger, int int_argu, Chess chess_argu){
			List<int> events_ok = new List<int> ();
//			foreach (var _evt in event_list) {
            for (int checking_event_number = 0; checking_event_number < map_data.list_event_data.Count; checking_event_number++) {
				var _evt = map_data.list_event_data [checking_event_number];
				//判断触发时机是否符合
				if (trigger != _evt.condition.trigger_evt)
					continue;
                //前置事件判断
                if (_evt.condition.pre_event_id >= 0 && map_data.list_event_data[_evt.condition.pre_event_id].triggered_times < _evt.condition.pre_event_launchtimes)
                    continue;
				//触发阶段判断
                if (_evt.condition.launch_phase != ePhaseType.All && Main.Inst.now_turnphase!=null &&_evt.condition.launch_phase !=  Main.Inst.now_turnphase.myType) 
					continue;
				//触发次数判断
				if (!(_evt.condition.trig_chance==0 || _evt.condition.trig_chance>_evt.triggered_times))
					continue;
				//全局开关1判断
                if (_evt.condition.gs_1 > 0 && !global_switch [_evt.condition.gs_1])
					continue;
				//全局开关2判断
                if (_evt.condition.gs_2 > 0 && !global_switch[_evt.condition.gs_2])
					continue;
				//触发参数对照
				switch (_evt.condition.trigger_evt) {
				case eEventTrigger.Reach_turn:
					if (_evt.condition.trig_argu != now_turn)
						continue;
					break;
				case eEventTrigger.Click:
					if(_evt.condition.trig_argu != int_argu)
						continue;
					break;
				case eEventTrigger.Kights_Stand:
                    //棋子对应的玩家有没有资格触发
                    if (!_evt.condition.players_who_can[(int)chess_argu.belong])
                        continue;
                    //查看棋子站的格子是否属于某个区域
                    bool match = false;
                    foreach (var item in Main.Inst.lv_ctrl.map_data.list_area_grid[_evt.condition.trig_argu].list) {
                        if (item == chess_argu.container.number) {
                            match = true;
                            break;
                        }
                    }
                    if (!match) continue;
                    break;
                case eEventTrigger.Monter_Dead:
					if (_evt.condition.trig_argu != int_argu)
						continue;
					break;
				case eEventTrigger.Phase_Change:
					//不需要额外判断
				default:
					break;
				}

				//全局变量判断
				if (_evt.condition.glo_v>0) {
					switch (_evt.condition.glo_v_op) {
					case eCompareOP.大于:
						if(!(global_variable [_evt.condition.glo_v] > _evt.condition.glo_v_compare))
							continue;
						break;
					case eCompareOP.大于等于:
						if(!(global_variable [_evt.condition.glo_v] >= _evt.condition.glo_v_compare))
							continue;
						break;
					case eCompareOP.小于:
						if(!(global_variable [_evt.condition.glo_v] < _evt.condition.glo_v_compare))
							continue;
						break;
					case eCompareOP.小于等于:
						if(!(global_variable [_evt.condition.glo_v] <= _evt.condition.glo_v_compare))
							continue;
						break;
					case eCompareOP.等于:
						if(!(global_variable [_evt.condition.glo_v] == _evt.condition.glo_v_compare))
							continue;
						break;
					case eCompareOP.不等于:
						if(!(global_variable [_evt.condition.glo_v] != _evt.condition.glo_v_compare))
							continue;
							break;
					default:
						Debug.Log ("关卡事件触发条件，变量对比方式错误");
						break;
					}
				}
				//携带道具判断
				if (!(_evt.condition.with_item > 0 && true))
					Debug.Log ("携带道具判断未完成");
				//携带角色判断
				if (!(_evt.condition.with_char > 0 && true))
                    Debug.Log ("携带角色判断未完成");
				//怪物死亡判断
				//目标事件ID和自身ID相同时，表示所有怪物死光
				//其他表示指定的
				bool monster_dead = true;
                if (_evt.condition.EventMonter_Dead >= 0) {
                    //将召唤
                    foreach (var coming_event in events_ok) {
                        foreach (var unit in map_data.list_unit_data) {
                            //有召唤发生，且需要怪物全死
                            if (unit.m_launch_event_order == coming_event && _evt.condition.EventMonter_Dead == checking_event_number) {
                                monster_dead = false;
                                break;
                            }
                            //有召唤发生，且怪物的召唤事件ID与需要死的怪物批次相同
                            else if (unit.m_launch_event_order == coming_event && coming_event == _evt.condition.EventMonter_Dead) {
                                monster_dead = false;
                                break;
                            }
                            if (!monster_dead) break;
                        }
                        if (!monster_dead) break;
                    }
                    //已召唤
                    foreach (KeyValuePair<long, Chess> item in Main.Inst.dic_chess) {
                        Chess ch = item.Value;
                        //-1是玩家召唤，但玩家打翻的还会算
                        //                  if (ch.shoukan_event > 0 && ch.shoukan_event <= now_event_order)

                        //怪物全死有效
                        if (_evt.condition.EventMonter_Dead == checking_event_number) {
                            if (ch.belong < ePlayer.Player1 || ch.belong > ePlayer.Player4) {
                                monster_dead = false;
                                break;
                            }
                        } else {
                            //指定批次怪物死亡有效
                            if ((ch.belong < ePlayer.Player1 || ch.belong > ePlayer.Player4) && ch.shoukan_event == _evt.condition.EventMonter_Dead) {
                                monster_dead = false;
                                break;
                            }
                        }
                    }
                }
				if (!monster_dead)
					continue;
                events_ok.Add(checking_event_number);

				//_evt.triggered_times++;   //

			}


			return events_ok.ToArray ();
			//胜利条件由事件决定
//			if (event_list.Count == now_event_order) {
//				Main.Instance.GameOver (ePlayer.Player2);
//				return false;
//			}
//			EventInfo evt = event_list[now_event_order];
//
//			if (!isFirstRun && Main.Instance.now_turnphase.myType != evt.launch_phase
//			 && evt.launch_phase != ePhaseType.All)
//				return false;
//			//当前0表示所有之前事件对应的怪物死亡
//			if (evt.condition.trigger == eEventTrigger.Phase_Change && evt.condition.EventMonter_Dead == 0) {
//				foreach(KeyValuePair<int,Chess> item in Main.Instance.dic_chess){
//					Chess ch = item.Value;
//					//-1是玩家召唤，但玩家打翻的还会算
////					if (ch.shoukan_event > 0 && ch.shoukan_event <= now_event_order)
//					if ((ch.belong< ePlayer.Player1 || ch.belong> ePlayer.Player4) && ch.shoukan_event <= now_event_order)
//						return false;
//				}
//				return true;
//			}
//			Debug.Log ("其他条件未完成");
//			return false;
		}
		// Use this for initialization
		void Start () {
			//map_data = LevelMapData.LoadData (Application.dataPath + "/Resources/Level/" + Level + ".map");
//			if (Level == "1") {
				
//			}
			//Debug.Log(JsonConvert.SerializeObject());

			init ();
			//loadMap (Level);  //因为数据帮到scene文件上了，所以不必要加载

            SceneManager.LoadScene("battle_Level", LoadSceneMode.Additive);
		}
		void init(){
			//map_data.list_event_data = new List<EventInfo> ();//不是编辑器EventList真好  //ScriptableObject做法使用自带的List不需要新建备用

			//初始化关卡变量
			for (int i = 1; i < 21; i++) {
				global_switch [i] = false;
				global_variable [i] = 0;
			}
		}
		void loadMap(string level){
			//检查文件夹
			if (!Directory.Exists (LevelMapData.LevelDir)) {
				Debug.Log ("找不到地图文件");
				return;
			}
            //加载文件
            //string fp = LevelMapData.LevelDir+level+".asset"; //TODO 关卡内加载数据的方式还没对
            //LevelMapData map = LevelMapData.LoadScriptableObject (fp);
            LevelMapData map = LevelMapData.Bundle_PvE_Level_Data.LoadAsset<LevelMapData>(level.ToString());
            //测试
            if(map.list_area_grid.Count>0) {
                string str = "Bundle中的area_grid数据:\r\n";
                foreach (var item in map.list_area_grid[0].list) {
                    str += item + ",";
                }
                Debug.Log(str);
            }

            //检查数据
            if (map == null) {
				Debug.Log (TextUnit.BKME_DataBroken);
				return;
			}
            //三大数据
            map_data = map;
            //mydic_grid_unit = LevelMapData.ParseUnitDataToDic(map.my_unit_info_key,map.my_unit_info);
            //map_data.mydic_grid_unit = JsonConvert.DeserializeObject<Dictionary<int,List<UnitInfo>>>(map.json_grid_unit);
            //LevelMapData.ParseEventDataToDic(event_list,map.my_event,map.event_script);
            //map_data.list_event_data = JsonConvert.DeserializeObject<List<EventInfo>>(map.my_event_json);
            //数据安全检测
			//if (map_data.mydic_grid_unit==null ||
			//	event_list.Count==0) {
            //    Debug.Log(TextUnit.BKME_DataBroken);
			//	Debug.Log("地图单位/事件数据异常");
			//	return;
			//}
		}
		// Update is called once per frame
		void Update () {
		
		}
	}
}