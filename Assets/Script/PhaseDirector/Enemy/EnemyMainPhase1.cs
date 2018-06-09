using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using BKKZ.POW01;

public class EnemyMainPhase1 :TurnPhase
{
	public override void Start(){
		Debug.Log("主流1");
		StartCoroutine (phaseMain1());
	}
	IEnumerator phaseMain1()
	{
		//准备阶段处理
		BuffContrllor.Deal_Effect(Main.Inst, false, eBuffEvent.Phase_Main1);
		yield return new WaitUntil(Main.Inst.isStageClear);
		yield return null;
	}
	void Update(){
        if (!Main.Inst.game_state.CanPhaseRun())
            return;
		//动画播放期间不允许影响流程的操作
		if (!Main.Inst.isStageClear ())
			return;	
		if (CheckActionEnd ())
			GoNext ();
		TakingAction ();
	}
	bool CheckActionEnd(){
		//行动结束条件1 -- 怪物死光
		bool hasMonster = false;
		foreach (var item in Main.Inst.dic_chess) {
			if (item.Value.belong >= ePlayer.Enemy1 && item.Value.belong <= ePlayer.Enemy2) {
				hasMonster = true;
				break;
			}
		}
		if (!hasMonster)
			return true;

		//条件2 -- 怪物行动结束，具体看怪物的ai结果
		foreach (var item in Main.Inst.dic_ai) {
			if (item.Value.my_chess.belong >= ePlayer.Enemy1 && item.Value.my_chess.belong <= ePlayer.Enemy2) {
				//只要还有一只怪能动，他们就没有结束
				if (item.Value.getActivable ())
					return false;
			}
		}
		return true;
	}
	void TakingAction(){
//		foreach (var item in Main.Instance.dic_ai) {
//			if (item.Value.getActivable ()) {
//				item.Value.Active ();
//				break;
//			}
//		}
//		Dictionary<int,BKKZ.POW01.AI.MonsterAI>.Enumerator enumor = Main.Instance.dic_ai.GetEnumerator();
//		bool allAiEnd = false;
//		while (!allAiEnd) {
//			if (enumor.MoveNext ()) {
//				KeyValuePair<int,BKKZ.POW01.AI.MonsterAI> item = enumor.Current;
//				if (item.Value.getActivable ()) {
//					item.Value.Active ();
//					break;
//				}
//			} else {
//				enumor.Dispose ();
//			}
//		}
		//通过遍历器，每次执行一个AI
		foreach (var item in Main.Inst.dic_chess) {
			if (item.Value.TDL.Count > 0)
				return;
		}
//		int index = Main.Instance.enumerator_ai.Current.Key;
//		if (Main.Instance.dic_chess.ContainsKey(index) && Main.Instance.dic_chess [index].todolist.Count > 0)
//			return;
		//每次AI执行会向棋子的TodoList添加一次行动指令，依次执行完毕后才会有下一个棋子的AI判断
		//TODO 可以删掉试试
		for (int i = 0; i < 1 && Main.Inst.dic_ai.Count > 0; i++) {
			if (Main.Inst.enumerator_ai.MoveNext ()) {
                KeyValuePair<long,BKKZ.POW01.AI.MonsterAI> item = Main.Inst.enumerator_ai.Current;

				if (item.Value.my_chess.belong < ePlayer.Enemy1 || item.Value.my_chess.belong > ePlayer.Enemy2)
					continue;
					
				if (item.Value.getActivable ()) {
					//如果是连续行动的AI则多执行一次
					bool cont_act = false;
					do {
						cont_act = item.Value.Active ();
					} while(cont_act);
				}
			} else {
				Main.Inst.enumerator_ai = Main.Inst.dic_ai.GetEnumerator();
				i--;
			}
		}
	}
}