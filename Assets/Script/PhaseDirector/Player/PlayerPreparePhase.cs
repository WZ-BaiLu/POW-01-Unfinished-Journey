using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using BKKZ.POW01;

public class PlayerPreparePhase :TurnPhase
{
	public override void Start(){
		Debug.Log("准备阶段");
		StartCoroutine (phasePrepare());
	
		//获得行动机会
		Main.Inst.Action_Chance = GameRule.Default_PvE_Action_Chance;
	}
	IEnumerator phasePrepare()
	{
        //玩家单位所在地重新驱散一次迷雾 2017年11月25日00:59:19 骑士失去主动驱散迷雾的能力
        //foreach (var item in Main.Inst.dic_chess) {
        //    if (item.Value.belong>ePlayer.None && item.Value.belong<=ePlayer.Player4) {
        //        BKTools.FogLift(item.Value.container.number, item.Value.attribute.spd+GameRule.Default_PvE_Fog_Lift_Range, GameRule.Default_PvE_Fog_Lift_Range,new int[]{(int)item.Value.belong});
        //    }
        //}

        //准备阶段处理
        BuffContrllor.Deal_Effect(Main.Inst, false, eBuffEvent.Phase_Prepare);
		yield return new WaitUntil(Main.Inst.isStageClear);
		GoNext ();
		yield return null;
	}
}

