using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BKKZ.POW01
{
	public enum eCompareOP{
		大于 = 0,
		大于等于,
		小于,
		小于等于,
		等于,
		不等于,
	}
	public enum eEventTrigger{
		Phase_Change = 0,
		Kights_Stand,
		Click,		//点击功能还是通过关卡事件实现，参数是click id通过关卡编辑获得（计划加进BuffContainer）
		Monter_Dead,//特定怪物，需要参数
		Reach_turn,//到达第X回合
	}
	[System.Serializable]
	/// <summary>
	/// 所有参数为-1时表示未起效
	/// </summary>
	public class EventCondition{
        public int pre_event_id = -1;
        public int pre_event_launchtimes = 1;
		///触发次数
		public int trig_chance = 1;
		///全局开关20个
		public int gs_1 = -1;
		public int gs_2 = -1;
		///全局变量20个 （仅支持int）
		public int glo_v = -1;
		public int glo_v_compare = 0;
		public eCompareOP glo_v_op = eCompareOP.大于等于;
		///独立开关4个
		public int self_s = -1;
		///道具存在
		public int with_item = -1;
		///角色存在
		public int with_char = -1;
		///批次怪物，需要参数
		/// TODO 编辑器中设默认值，用poplist形式，指定对应的事件
		/// TODO 默认值0，表示当前事件之前的事件生成怪物死亡（因为最少也是死第
		public const int DEFAULT_EventMonster_Dead_UnableValue = -9999;
		///目标事件ID和自身ID相同时，表示所有怪物死光，毕竟这个事件还没有发生不可能有对的上的怪物
		public int EventMonter_Dead = DEFAULT_EventMonster_Dead_UnableValue;//
		///触发事件：骑士接触、骑士站立、点击、阶段变化、怪物死亡
		public eEventTrigger trigger_evt = eEventTrigger.Phase_Change;
		/// 触发阶段限制
		public ePhaseType launch_phase = ePhaseType.All;
		///触发事件参数
		public int trig_argu = -1;
        ///触发方限制 玩家1-4 怪物1-2 剧情
        public bool[] players_who_can = { false, false, false, false, false, false, false, false};
	}

	[System.Serializable]
	public class EventInfo
	{
		private int order;	//确实是仅在编辑时会用到的值，查看是否会存储（肯定不会啊，我又没读）
		public static int event_count = 0;//用来保证编辑过程中事件名字不混淆 例如：一直建到“事件39”后删掉2个，新建出“事件38”，但实际上38的时间点更靠后。
		public string event_id = "";//改成字符串，方便查看
		///TODO null的可能还有召唤事件，后期统一
		public DramaScript drama_script = new DramaScript ();
		public EventCondition condition = null;
        public int triggered_times;
		public string description="";
		public EventInfo (){
			event_id = TextUnit.BKME_Event_Id_Prefix + event_count;
		}
		public void setOrder(int or){
			order = or;
		}
		public int getOrder(){
			return order;
		}
	}
}