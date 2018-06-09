using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace BKKZ.POW01.AI
{
	public enum eTDL_Item_Type{
		TDL_None = -1,
		TDL_Move,
		TDL_Strike,
		TDL_Attack,
		TDL_Skill,
	}
	public class TDL_Item
	{
		/// 行动类型
		public eTDL_Item_Type type = eTDL_Item_Type.TDL_None;
		///移动的目标格子ID
		public int argu_grid_id = -1;
		/// 攻击方向（短期不一定用得上）
		public eDirection argu_Direction = eDirection.None;
		/// 技能信息实例
		/// 通常存在chess.attribute
		/// 获取方式 Data.Inst.skill_data[my_card_info.skill01]
		public Skill_Info argu_skill_info = null;
		/// 移动是否造成强袭和夹击
		public bool argu_trigger_strike = false;

	}
	public class ChessTDList
	{
		List<TDL_Item> list;
		ChessTDList(){
			list = new List<TDL_Item> ();
		}
	}
}

