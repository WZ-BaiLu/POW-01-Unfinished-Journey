using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace BKKZ.POW01{
	public class DramaSectionWindows : EditorWindow
	{
//		public bool isInit = false;
		public static int index;
		public static DramaSection section;

		///显示编辑器 保留代码，其实作为数据传输用
//		[MenuItem ("Window/MapEditor %e")]
		public static void  ShowWindow (int _index,DramaSection _section) {
//			var window = DramaSectionWindows.GetWindow<DramaSectionWindows>();
			index = _index;
			section = _section;
//			((MapEdtior)window).init ();
		}
		public static void OnGUI () {
			GUIStyle style = new GUIStyle (GUI.skin.box);
			Color _bak = GUI.backgroundColor;
			GUI.backgroundColor = new Color (1,1,1);
//			GUI.Box ();
			EditorGUILayout.BeginVertical(style);
//			style.onNormal.;

			if (section==null) {
				GUILayout.Label ("暂无编辑对象，你可以……");
				GUILayout.Label ("跟我……");
				GUILayout.Label ("互相瞅来瞅去");
                GUILayout.Label ("瞅你咋地");
                EditorGUILayout.EndVertical();
				return;
			}
//			EditorGUILayout.BeginVertical (/*GUILayout.ExpandHeight(true),GUILayout.ExpandWidth(true)*/);
//			section.OnEditorGUI ();
			OnSectionGUI(section);
			EditorGUILayout.EndVertical ();
//			EditorGUILayout.BeginHorizontal (/*GUILayout.ExpandHeight(true),GUILayout.ExpandWidth(true)*/);
//			if (GUILayout.Button ("↑"))
//				if (EventList.Now_Event.drama_script.MoveUp (index))
//					index--;
//			if (GUILayout.Button ("↓"))
//				if (EventList.Now_Event.drama_script.MoveDown (index))
//					index++;
//			if (GUILayout.Button ("delete")) {
//				EventList.Now_Event.drama_script.Remove (section);
//				Close ();
//			}
//			EditorGUILayout.EndHorizontal ();
		}
//		void OnEnable()
//		{
//			init (true);
//		}
//		public void init(){
//			init (false);
//		}
//		public void init(bool force_init){
//			if (isInit && !force_init)
//				return;
//			instance = this;
//		}

        //剧情项 简介
		public static string Drama2String(DramaSection section){
			string res = "";
			string chess_name = "未选定";
			string skill_name = "未选定";
			string variable_name = "未选定";
			if(Data.Inst.card_data.ContainsKey(section.chess_id))
				chess_name = Data.Inst.card_data [section.chess_id].name;
			if(Data.Inst.skill_data.ContainsKey(section.skill_id))
				skill_name = Data.Inst.skill_data [section.skill_id].name;
            switch (section.my_type) {
            case eDramaOperationType.ShouKan:
                res += "召唤 “" + section.chess_id + ":" + chess_name + "” 于 " + section.to_grid_id + " 格子";
                break;
            case eDramaOperationType.Move:
                res += "移动 “" + section.chess_id + ":" + chess_name + "” 至 " + section.to_grid_id + " 格子";
                break;
            case eDramaOperationType.Skill:
                res += "由 “" + section.chess_id + ":" + chess_name + "” 使用 “" + section.skill_id + "号技能" + skill_name + " ”";
                break;
            case eDramaOperationType.Speak:
                res += "由 “" + section.chess_id + ":" + chess_name + "” 说 “" + section.manfenzuowen + " ”";
                break;
            case eDramaOperationType.Variable:
                variable_name = section.variable_index + " - " + MapEditor.my_map_data.global_variable[section.variable_index];
                switch (section.op_int) {
                case eDOType_int.赋值:
                    res = variable_name + " = " + section.v_int;
                    break;
                case eDOType_int.加:
                    res = variable_name + " + " + section.v_int;
                    break;
                case eDOType_int.减:
                    res = variable_name + " - " + section.v_int;
                    break;
                case eDOType_int.乘:
                    res = variable_name + " * " + section.v_int;
                    break;
                case eDOType_int.除:
                    res = variable_name + " / " + section.v_int;
                    break;
                case eDOType_int.平方:
                    res = variable_name + " ^ " + section.v_int;
                    break;
                default:
                    break;
                }
                break;
            case eDramaOperationType.Switch:
                variable_name = MapEditor.my_map_data.global_switch[section.variable_index];
                switch (section.op_boolean) {
                case eDOType_boolean.赋值:
                    res = variable_name + " = " + section.v_boolean;
                    break;
                case eDOType_boolean.与:
                    res = variable_name + " && " + section.v_boolean;
                    break;
                case eDOType_boolean.或:
                    res = variable_name + " || " + section.v_boolean;
                    break;
                }
                break;
            case eDramaOperationType.FogOn:
                res = section.to_grid_id + "号格子，产生迷雾 " + section.v_int + "格";
                break;
            case eDramaOperationType.FogOff:
                res = section.to_grid_id + "号格子，驱散迷雾 " + section.v_int + "格";
                break;
            case eDramaOperationType.Win:
                res = "玩家获得胜利";
                break;
            case eDramaOperationType.Lose:
                res = "过关失败";
                break;
            case eDramaOperationType.ExploreStart:
                res = "游戏开始，解锁 " + section.area_id + " 号区域,在" + section.to_grid_id + "召唤默认骑士";
                break;
            case eDramaOperationType.ShowArea:
                res = "开放区域 " + section.area_id + " 号区域";
                break;
            case eDramaOperationType.HideArea:
                res = "封锁区域 " + section.area_id + " 号区域";
                break;
            case eDramaOperationType.BattleStart:
                res = "在区域" + section.area_id + "中展开战斗，起点为" +section.to_grid_id;
                break;
            case eDramaOperationType.ExplorContinue:
                res = "恢复探索模式";
                break;
			default:
				break;
			}



			return res;
		}
		//编辑窗
		public static void OnSectionGUI(DramaSection section){
            //			section.my_type = (eDramaOperationType)EditorGUILayout.EnumPopup ("类型",section.my_type);

            //			string res = "";
            //			string chess_name = "未选定";
            //			string skill_name = "未选定";
            switch (section.my_type) {
            case eDramaOperationType.ShouKan:
                //				if(Data.Inst.card_data.ContainsKey(section.chess_id))
                //					chess_name = Data.Inst.card_data [section.chess_id].name;
                //这个
                //				my_argu_AI_ID = (eAI_Type)EditorGUILayout.EnumPopup ("AI",my_argu_AI_ID);
                EditorGUILayout.LabelField("召唤信息在单位中编辑");
                break;
            case eDramaOperationType.Move:
                //				if (!Data.Inst.card_data.ContainsKey (section.chess_id)) {
                //					section.chess_id = Data.Inst.card_data.First().Key;	//TODO 很神奇的，加了Linq后能够是用first，理论上应该从数据源避免，但
                //				}
                //
                //				chess_name = Data.Inst.card_data [section.chess_id].name;
                //				section.chess_id = EditorGUILayout.IntPopup(
                //					section.chess_id,
                //					(from card in Data.Inst.card_data select card.Value.name).ToArray(),
                //					(from card in Data.Inst.card_data select card.Key).ToArray()
                //				);
                //			res += "移动 “" + section.chess_id + ":" + chess_name + "” 至 " + section.to_grid_id + " 格子";
                //全骑士名字选单（因为存在前一个事件召唤的骑士，加条件判断）
                EditorGUILayout.BeginHorizontal();
                gui_chess_id("将");
                gui_grid_select("移动至");
                EditorGUILayout.EndHorizontal();
                //目标格子
                break;
            case eDramaOperationType.Skill:
                gui_chess_id("使用者");
                gui_skill_id("使用技能");
                break;
            case eDramaOperationType.Speak:
                gui_chess_id("发言者");
                gui_speak("说");
                break;
            case eDramaOperationType.Variable:
                EditorGUILayout.BeginHorizontal();
                section.variable_index = EditorGUILayout.Popup(section.variable_index, MapEditor.my_map_data.global_variable);
                section.op_int = (eDOType_int)EditorGUILayout.EnumPopup(section.op_int);
                section.v_int = EditorGUILayout.IntField(section.v_int);
                EditorGUILayout.EndHorizontal();
                break;
            case eDramaOperationType.FogOn:
                gui_grid_select("笼罩格子");
                section.v_int = EditorGUILayout.IntField(section.v_int);
                EditorGUILayout.LabelField("圈迷雾");
                break;
            case eDramaOperationType.FogOff:
                gui_grid_select("驱散格子");
                section.v_int = EditorGUILayout.IntField(section.v_int);
                EditorGUILayout.LabelField("圈迷雾");
                break;
			case eDramaOperationType.Switch:
				EditorGUILayout.BeginHorizontal ();
				section.variable_index = EditorGUILayout.Popup (section.variable_index, MapEditor.my_map_data.global_switch);
				section.op_boolean = (eDOType_boolean)EditorGUILayout.EnumPopup (section.op_boolean);
				section.v_boolean = EditorGUILayout.Toggle (section.v_boolean);
				EditorGUILayout.EndHorizontal ();
				break;
            case eDramaOperationType.ExploreStart:
                gui_grid_select("起点格子");
                section.area_id = EditorGUILayout.Popup("解锁区域",section.area_id,AreaList.arr_string_id);
                GUILayout.Label("//TODO要转用卡组配置中的信息");
                section.chess_id = EditorGUILayout.IntPopup("默认骑士",section.chess_id, Data.arr_Chess_name, Data.arr_Chess_key);

                break;
            case eDramaOperationType.ShowArea:
                section.area_id = EditorGUILayout.Popup("开放区域", section.area_id, AreaList.arr_string_id);
                break;
            case eDramaOperationType.HideArea:
                section.area_id = EditorGUILayout.Popup("封锁区域", section.area_id, AreaList.arr_string_id);
                break;
            case eDramaOperationType.BattleStart:
                section.area_id = EditorGUILayout.Popup("战斗区域", section.area_id, AreaList.arr_string_id);
                gui_grid_select("战斗起点");
                break;
            case eDramaOperationType.ExplorContinue:
                GUILayout.Label("你有病啊，继续探索不需要修改。");
                break;
			default:
				break;
			}
		}
		//chess_id单独编辑
		static void gui_chess_id(string title){
			if (!Data.Inst.card_data.ContainsKey (section.chess_id)) {
				section.chess_id = Data.Inst.card_data.First().Key;	//TODO 很神奇的，加了Linq后能够是用first，理论上应该从数据源避免，但
			}
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label (title);
			section.chess_id = EditorGUILayout.IntPopup(
				section.chess_id,
				(from card in Data.Inst.card_data select card.Key + " - " +card.Value.name).ToArray(),
				(from card in Data.Inst.card_data select card.Key).ToArray()
			);
			EditorGUILayout.EndHorizontal ();
		}
		static void gui_skill_id(string title){
			if (!Data.Inst.skill_data.ContainsKey (section.skill_id)) {
				section.skill_id = Data.Inst.skill_data.First().Key;	//TODO 很神奇的，加了Linq后能够是用first，理论上应该从数据源避免，但
			}
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label (title);
			section.skill_id = EditorGUILayout.IntPopup(
				section.skill_id,
				(from skill in Data.Inst.skill_data select skill.Value.name).ToArray(),
				(from skill in Data.Inst.skill_data select skill.Key).ToArray()
			);
			EditorGUILayout.EndHorizontal ();
		}
		static void gui_speak(string title){
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label (title);
			section.manfenzuowen = EditorGUILayout.TextField (section.manfenzuowen);
			EditorGUILayout.EndHorizontal ();
		}

		static void gui_grid_select(string title){
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label (title + section.to_grid_id);
			//				bool check = EditorGUILayout.Toggle ("格子", edit_event.condition.trig_argu >= 0);
			//				if (check) {
            var obj = EditorGUILayout.ObjectField (MapEditor.Inst.SelectGrid, typeof(ChessContainer), true);
			section.to_grid_id = (obj as ChessContainer).number;
			if (GUILayout.Button ((MapEditor.bSelectSenceGrid ? "点击场景制定格子" : "选取"))) {
				MapEditor.bSelectSenceGrid = !MapEditor.bSelectSenceGrid;
				MapEditor.dSelectCallback = (int id) => {
					section.to_grid_id = id;
				};
			}
			//				} else {
			//					if (edit_event.condition.trig_argu > 0) {
			//						edit_event.condition.trig_argu = -Mathf.Abs (edit_event.condition.trig_argu);
			//					} else if (edit_event.condition.trig_argu == 0)
			//						edit_event.condition.trig_argu = -1;
			//				}
			EditorGUILayout.EndHorizontal ();
		}
	}

}