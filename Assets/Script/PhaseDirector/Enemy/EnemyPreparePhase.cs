using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using BKKZ.POW01;

public class EnemyPreparePhase :TurnPhase
{
	public override void Start(){
		Debug.Log("准备阶段");
		StartCoroutine (phasePrepare());
	
		//获得行动机会
//		Main.Instance.Action_Chance = GameRule.Default_PvE_Action_Chance;
	}
	IEnumerator phasePrepare()
	{
		//准备阶段处理
		BuffContrllor.Deal_Effect(Main.Inst, false, eBuffEvent.Phase_Prepare);
		yield return new WaitUntil(Main.Inst.isStageClear);
		GoNext ();
		yield return null;
	}
}

