using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using BKKZ.POW01;
public class PlayerBattlePhase :TurnPhase
{
	public TurnPhase MainPhase;
	public override void Start(){
		Debug.Log("战流");
	} 
	void Update(){
        if (!Main.Inst.game_state.CanPhaseRun())
            return;
        if (!Main.Inst.isStageClear())
            return;
		if (Main.Inst.b_battbe_phase_pincer_over && Main.Inst.overturn_list.Count == 0 && Chess.driving_skill_list.Count == 0) {
			if(Main.Inst.Action_Chance==0)
				GoNext ();
			else 
				GoNext(MainPhase);
		}
		else
			Main.Inst.PincerChain();
	}
}