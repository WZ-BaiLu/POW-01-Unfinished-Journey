using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace BKKZ.POW01.AI
{
	/// <summary>
	/// AI的待选计划方案
	/// 包含了：
	/// 	走到哪里
	/// 	用什么技能攻击
	/// 	能否击杀
	/// 	走到这个位置后会不会被击杀
	/// 	TODO 以及其他帮助判断行动优劣的信息
	/// </summary>
	public class Operation:System.IComparable<Operation>
	{
		public int iWouldKillTarget = 0;
		public bool bMayBeKilled = false;
		public int targetGridIndex = -1;
		public int skill_id = -1;
		public Skill_Info skill = null;
		public List<Chess> attack_target = new List<Chess>();
		public eDirection direcion = eDirection.All;
		public int exDamage = -1;
		public int exInjure = -1;

		// 仿火纹底板
		// 能打死时打人
		// 不会被打死时优先打人
		// 会被打死时逃跑
		// 怪物攻击以主动技能为主
		public int CompareTo(Operation comparePart)
		{
			// A null value means that this object is greater.
			if (comparePart == null)
				return 1;

			if (iWouldKillTarget>0 || comparePart.iWouldKillTarget>0) {

				if (iWouldKillTarget == comparePart.iWouldKillTarget) { 
					// 都能杀人比预期受伤
					//[!]攻击性AI可以优先判断攻击 TODO
					return exInjure.CompareTo (comparePart.exInjure);
				} else {
					return iWouldKillTarget.CompareTo (comparePart.iWouldKillTarget);
				}
			}
			if (bMayBeKilled && comparePart.bMayBeKilled) {
				//都会死比其他
				// 当前做法是比输出
			}else if (bMayBeKilled){
				return -1;
			}else if (comparePart.bMayBeKilled){
				return 1;
			}

			// 打不死人也不会被打死时比输出
			return exDamage.CompareTo (comparePart.exDamage);
		}
	}
}