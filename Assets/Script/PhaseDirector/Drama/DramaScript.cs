using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using BKKZ.POW01;
using BKKZ.POW01.AI;


public enum eDramaOperationType{
    None = -1,
	ShouKan,    	//ID,所属,位置（被占用顶开目标
	Move,			//骑士ID,移动方式,坐标X（绝对位置移动时，写作grid_id）,Y,Z
	Speak,			//骑士ID,对话内容
	Skill,			//骑士ID,技能ID
	Variable,		//修改变量
	Switch,			//修改开关
    FogOff,         //关闭烟雾
    FogOn,          //打开烟雾
    Win,            //玩家胜利
    Lose,           //玩家失败
    ExploreStart,   //关卡开始,指定探索范围
    BattleStart,    //战斗开始,指定有效战斗范围
    ExplorContinue, //战斗结束,继续探索
    ShowArea,       //添加可探索区域
    HideArea,       //减少可探索区域
    Max
}
public enum eDOType_boolean{
	赋值 = 0,
	与,
	或,
}
public enum eDOType_int{
	赋值,
	加,
	减,
	乘,
	除,
	平方,
}
public enum eDOType_Move{
	Normal_Move_ABS = 0,
	Force_Move_ABS,
	Norma_lMove_REL,
	Force_Move_REL,
}
[System.Serializable]
public class DramaSection{
    public int from_event = -1; //同时在运行时和编辑时都有存，但编辑时没有存召唤section
	public eDramaOperationType my_type;
	public int chess_id = -1;
	public ePlayer belong = ePlayer.Drama;
    public int area_id;
	public int to_grid_id = 0;
	public int skill_id = 0;
	public string manfenzuowen;
	///开关和变量公用索引
	public int variable_index = 0;//开关和变量公用索引
	public bool v_boolean = true;
	public eDOType_boolean op_boolean = eDOType_boolean.赋值;
	public int v_int = 0; 
	public eDOType_int op_int = eDOType_int.赋值;
	public eAI_Type my_argu_AI_ID = eAI_Type.Default;
	public eDOType_Move move_type = eDOType_Move.Force_Move_ABS;
	public Point3 move_argu = new Point3();

	#if UNITY_EDITOR
	public void OnEditorGUI(){
		my_type = (eDramaOperationType)EditorGUILayout.EnumPopup ("类型",my_type);


		string res = "";
		string chess_name = "未选定";
		string skill_name = "未选定";
		switch (my_type) {
		case eDramaOperationType.ShouKan:
//			if(Data.Inst.card_data.ContainsKey(chess_id))
//				chess_name = Data.Inst.card_data [chess_id].name;
			//这个
//			my_argu_AI_ID = (eAI_Type)EditorGUILayout.EnumPopup ("AI",my_argu_AI_ID);
			EditorGUILayout.LabelField("召唤信息在单位中编辑");
			break;
		case eDramaOperationType.Move:
			if(Data.Inst.card_data.ContainsKey(chess_id))
				chess_name = Data.Inst.card_data [chess_id].name;
//			res += "移动 “" + chess_id + ":" + chess_name + "” 至 " + to_grid_id + " 格子";
			//全骑士名字选单（因为存在前一个事件召唤的骑士，加条件判断）
			//目标格子
			break;
		case eDramaOperationType.Skill:
			if(Data.Inst.skill_data.ContainsKey(skill_id))
				skill_name = Data.Inst.skill_data [skill_id].name;
//			res += "由 “" + chess_id + ":" + chess_name + "” 使用 “" + skill_id + "号技能"+ skill_name +" ”";
			//全骑士名字选单（因为存在前一个事件召唤的骑士，加条件判断）
			//目标格子
			break;
		case eDramaOperationType.Speak:
			if(Data.Inst.card_data.ContainsKey(chess_id))
				chess_name = Data.Inst.card_data [chess_id].name;
			res += "由 “" + chess_id + ":" + chess_name + "” 说 “" + manfenzuowen +" ”";
			break;
		default:
			break;
		}
	}
    #endif
}
//剧本从属于阶段
[System.Serializable]
public class DramaScript
{
//	public string json;
//	public IEnumerator<DramaSection> Enumor {
//		get { 
//			if (enumor == null) {
//				enumor = (IEnumerator<DramaSection>)arr_section.GetEnumerator ();
//			}
//			return enumor;
//		}
//		set{ enumor = value;}
//	}
	public DramaScript(){
	}

	public List<DramaSection> section_list = new List<DramaSection>();
    public string json;//2017年11月03日21:14:31 没发现什么用，暂时放弃
	public void Add(DramaSection section){
		if (section == null) {
			Debug.Log ("出现无效数据 根源显示属于TODO项");
			return;
		}
		section_list.Add (section);
	}
	public void Remove(DramaSection _section){
		section_list.Remove (_section);
	}
	public bool MoveUp(int _index){
		if (_index == 0)
			return false;
		DramaSection _moved = section_list [_index - 1];
//		DramaSection _moving = section_list [_index];
		section_list.Remove (_moved);
//		section_list.Remove (_moving);
//		section_list.Insert (_index - 1, _moving);
		section_list.Insert (_index, _moved);
		//		section_list[_index - 1] = section_list
		return true;
	}
	public bool MoveDown(int _index){
		if (_index == section_list.Count-1)
			return false;
//		DramaSection _moved = section_list [_index + 1];
		DramaSection _moving = section_list [_index];
//		section_list.Remove (_moved);
		section_list.Remove (_moving);
		section_list.Insert (_index + 1, _moving);
//		section_list.Insert (_index, _moved);
		//		section_list[_index - 1] = section_list
		return true;
	}
	/// <summary>
    /// 创建每个事件点的剧本
    /// 包含召唤
    /// </summary>
    /// <returns>The generate.</returns>
    /// <param name="level_info">Level info.用于访问地图、单位、事件数据</param>
    /// <param name="event_order">Event order.当前触发的事件序号</param>
	public static DramaScript Generate(LevelController level_info,int event_order){
		DramaScript new_script = new DramaScript ();

		// 后期会有多种事件，目前只制作召唤
		int grid_id = -1;
		foreach (var unit in level_info.map_data.list_unit_data) {
			grid_id = unit.m_start_grid;
			if(unit.m_launch_event_order == event_order){
				DramaSection section = new DramaSection ();
                section.from_event = event_order;
				section.my_type = eDramaOperationType.ShouKan;
				section.chess_id = unit.m_unit_id;
                section.belong = unit.m_belong;
				section.to_grid_id = grid_id;
				section.my_argu_AI_ID = unit.AI_ID;
				new_script.section_list.Add (section);
			}
		}
        // 说好的其他事件  2017年11月11日14:34:24
        EventInfo info = level_info.map_data.list_event_data[event_order];
        foreach (var item in info.drama_script.section_list) {
            new_script.section_list.Add(item);
        }

        return new_script;
	}
	//返回是否结束
	public DramaSection getNext(){
		//用新的判断，因为数组没法删啊
		if (section_list.Count <= 0)
			return null;
		else {
			DramaSection res = section_list[0];
			section_list.RemoveAt (0);
			//返回的剧本在PlotShowPhase中分析执行，请务必添加演员等待
			return res;
		}
	}
	public bool IsOver {
        get { return section_list.Count == 0; }
    }
}