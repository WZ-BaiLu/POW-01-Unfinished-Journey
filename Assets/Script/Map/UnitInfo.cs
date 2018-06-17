using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BKKZ.POW01
{
	[System.Serializable]
	public enum eUnitType{
		Unit = 0,
		Buff,

		Max
	}
	[System.Serializable]
	public class UnitInfo:IComparable<UnitInfo>
	{
		/// <summary>
		/// 编辑器下，每次加载地图资料时重置
		/// 讲道理新建也要
		/// </summary>
		public static int Unit_Count = 0;
        ///初始位置编号
        //[XMLLayout("起始格子：", "BKKZ.POW01.MapEditor, Assembly-CSharp-Editor;Inst.arr_grid_id")]
        public int m_start_grid ;
        /// AI编号(0为默认ai）
        [XMLLayout("类型：")]
        public eUnitType m_unit_type;
        [XMLLayout("ID：", "BKKZ.POW01.Data, Assembly-CSharp;arr_Chess_name","BKKZ.POW01.Data, Assembly-CSharp;arr_Chess_key" )]
        public int m_unit_id ;

        [XMLLayout("出场时机：","BKKZ.POW01.EventList, Assembly-CSharp-Editor;Instance.Arr_ID_String")]
        public int m_launch_event_order = 0;
        [XMLLayout("AI：")]
		public BKKZ.POW01.AI.eAI_Type AI_ID = 0;
        [XMLLayout("ClickID：")]
		public int click_ID = -1;
        [XMLLayout("所属：")]
		public ePlayer m_belong;	//从属
		public Chess Chess{ set; get;}
		public Buff Buff{ set; get;}
		public UnitInfo (int _id)
		{
			m_unit_type = eUnitType.Unit;
			m_unit_id = _id;

		}
		public UnitInfo (int _id,eUnitType _type)
		{
			m_unit_type = _type;
			m_unit_id = _id;
		}


//		public override bool Equals(object obj)
//		{
//			if (obj == null) return false;
//			UnitInfo objAsPart = obj as UnitInfo;
//			if (objAsPart == null) return false;
//			else return Equals(objAsPart);
//		}
//		//排序用，并非判断两个东西一样
//		public bool Equals(UnitInfo other)
//		{
//			if (other == null) return false;
//			return (this.m_launch_event_order.Equals(other.m_launch_event_order));
//		}
		// Default comparer for Part type.
		public int CompareTo(UnitInfo comparePart)
		{
			// A null value means that this object is greater.
			if (comparePart == null)
				return 1;
			else if (m_unit_type == comparePart.m_unit_type)
				return m_launch_event_order.CompareTo (comparePart.m_launch_event_order);
			else
				return m_unit_type.CompareTo (comparePart.m_unit_type);
		}
	}
}

