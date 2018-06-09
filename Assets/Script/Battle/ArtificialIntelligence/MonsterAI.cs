using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace BKKZ.POW01.AI
{
	public enum eAI_Type{
		Default = 0,
	}
	public class MonsterAI
	{
		/// <summary>
		/// 操控的棋子
		/// </summary>
		public Chess my_chess;
		public long my_id;
		/// <summary>
		/// 行动机会
		/// </summary>
		protected int _active_chance;
		public int ActiveChance{get { return _active_chance;}}
		protected int _default_active_chance;

		MonsterAI(){
			_default_active_chance = 1;
			_active_chance = _default_active_chance;
		}
		/// <summary>
		/// Creates the monster A.
		/// </summary>
		/// <returns>The monster A.</returns>
		/// <param name="chess_id">已经采用4位从属+序号的格式</param>
		/// <param name="type">在eAI_Type中列出的ai类型.</param>
        public static MonsterAI createMonsterAI(long chess_id,eAI_Type type){
			MonsterAI ai = null;
			switch (type) {
			case eAI_Type.Default:
			default:
				ai = new MonsterAI ();
				break;
			}

			ai.my_id = chess_id;
			ai.my_chess = Main.Inst.dic_chess[chess_id];
			if (Main.Inst.dic_ai.ContainsKey (chess_id))
				Debug.LogError ("重复的ID");
			Main.Inst.dic_ai.Add (chess_id, ai);
			//每次更新dic_ai，更换遍历器
			Main.Inst.enumerator_ai = Main.Inst.dic_ai.GetEnumerator();
			if (ai.my_chess.belong >= ePlayer.Player1 && ai.my_chess.belong <= ePlayer.Player4)
				ai.BanActive ();
			return ai;
		}
		//重置行动状态——回合开始时、其他情况
		public void Restart(){
			_active_chance = _default_active_chance;
		}
		public void	 BanActive(){
			_active_chance = 0;
		}
		/// <summary>
		/// 行动
		/// 执行AI逻辑，如果是连续行动就返回true
		/// 其他情况即便能行动多次也按每人一步的来
		/// </summary>
		public bool Active(){
			if (_active_chance <= 0)
				return false;
			//AI优先度在Operation类的排序方法中实现
//			bool find_would_skill = false;	//暂时想不出适用情况，先留着拓展思路
//			bool find_may_dead = false;		//暂时想不出适用情况，先留着拓展思路

			//List<ChessContainer> list =  BKTools.getArrivableGrid (my_chess);
			List<Operation> op_list = generateOperations();
			op_list.Sort ();
			op_list.Reverse ();
//			Debug.Log (op_list [0]);

			//解析opration成TDL item，两者不能省略，毕竟tdl_item没有排序意义
			if (op_list.Count == 0) {
				_active_chance--;
				return false;
			}
			Operation op = op_list[0];
			if (my_chess.container.number != op.targetGridIndex) {
				//移动（如果位置不变则不移动）
				TDL_Item tdl_move = new TDL_Item ();
				tdl_move.type = eTDL_Item_Type.TDL_Move;
				tdl_move.argu_grid_id = op.targetGridIndex;
				my_chess.TDL.Add (tdl_move);
			}

			TDL_Item tdl_atk = new TDL_Item ();
			tdl_atk.type = eTDL_Item_Type.TDL_Skill;
			tdl_atk.argu_Direction = op.direcion;
			tdl_atk.argu_skill_info = op.skill;
//			tdl_atk.
			my_chess.TDL.Add (tdl_atk);
			_active_chance--;
			return false;
		}
		public bool getActivable(){
			return ActiveChance>0;
		}

		public List<Operation> generateOperations() {
			Chess c = my_chess;
			List<ChessContainer> res_list = new List<ChessContainer> ();
			List<ChessContainer> search_list = new List<ChessContainer> ();
			search_list.Add (c.container);
			search_list = BKTools.getAroundGrid (c.attribute.Spd, search_list);
            res_list.Add(c.container);//原地不动也是一种选择
			foreach (var item in search_list) {
                //TODO 判断条件需要完善
				if (item.my_chess==null && GameRule.judgePassable(c,item)) {
					res_list.Add (item);
				}
			}
            
			//return res_list;
			List<Operation> op_list = new List<Operation>();
			//检索所有移动选项
			foreach (var grid in res_list) {
				foreach (var skill in my_chess.attribute.skill) {
					if (skill == null)
						continue;
					//根据攻击生成行动方案
					switch (skill.my_select_Solution) {
					case eSkill_Target_SelectSolution.Auto:
						//随机攻击型
						//按照可能击杀的数量和总体伤害计算
						OpBySkill(ref op_list,skill,eDirection.All,grid);
						break;
					case eSkill_Target_SelectSolution.Human:
                        //制定攻击目标
						//方向限制型
						//按照可能击杀的数量和总体伤害计算
						//每个方向进行一次选择
                        for (eDirection i = eDirection.UpperLeft; i < eDirection.All; i++) {
							OpBySkill(ref op_list,skill,i,grid);
						}
						break;
					default:
						break;
					}
				}
				// 罗列攻击手段
				// 按攻击手段获得每一个格子上能打到的目标
				// 根据目标的血量和攻击手段获得攻击性相关行动数据
				//
				// 对产生的攻击方案，判断对方反击时会造成的伤害预期获得防御相关的行动数据
				// ps:上面这个运算需要一个整体的将来战局模拟数据
			}
			return op_list;
		}
		/// <summary>
		/// Ops the by skill.
		/// </summary>
		/// <param name="op_list">Op list.</param>
		/// <param name="skill">Skill.</param>
		/// <param name="dir_limit">Dir limit.</param>
		/// <param name="simGrid">假定棋子站在这里发起攻击.</param>
		public void OpBySkill(ref List<Operation> op_list,Skill_Info skill,eDirection dir_limit,ChessContainer simGrid){

			List<ChessContainer> attack_grids = BKTools.GetSkillScope (skill, simGrid);
			//筛选攻击目标
			//有效性筛选
			for (int i = attack_grids.Count - 1; i >= 0; i--)
			{
				ChessContainer cc = (ChessContainer)attack_grids[i];
				if (cc.my_chess == null || !BKTools.IsTargetFit(skill.my_TargetBelong, cc.my_chess.belong, my_chess.belong))
				{
					attack_grids.RemoveAt(i);
					continue;
				}
			}
			//数量筛选
			//目标数量有限时，选择合适的目标（大部分时候为随机）
			//TODO 这边的随机选择也是不严谨的， [!]注意operation中的目标不会被用于生成ToDoListItem，不影响实际执行
			if (skill.target_number != 0)
			{
				List<ChessContainer> list_copy = new List<ChessContainer>();
				foreach (var item in attack_grids) {
					list_copy.Add (item);
				}
				attack_grids.Clear();
				for (int i = 0; i < skill.target_number && list_copy.Count > 0; i++)
				{
					int index = UnityEngine.Random.Range(0, list_copy.Count);
					attack_grids.Add(list_copy[index]);
					list_copy.Remove(list_copy[index]);
				}
			}

			Operation op = new Operation ();

			op.targetGridIndex = simGrid.number;
			op.direcion = dir_limit;
			op.skill_id = skill.id;
			op.skill = skill;
			op.attack_target.AddRange(
				from grid in attack_grids
//				where grid.my_chess != null
				select grid.my_chess);

			foreach (var target in attack_grids) {
				//评估行动价值
				if (skill.skill_damage > target.my_chess.attribute.mana)
					op.iWouldKillTarget ++;
//				else
//					op.iWouldKillTarget = false;
				op.bMayBeKilled = false;
				op.exDamage += my_chess.attribute.GetSklDmg(skill);
				// TODO 此处使用了非正式的受伤判断，可能要很久很久的将来才能补正
				op.exInjure = target.my_chess.attribute.Pow;
			}
			op_list.Add (op);
		}
	}

}