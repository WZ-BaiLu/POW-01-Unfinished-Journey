using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using BKKZ.POW01;

public class PlayerMainPhase1 :TurnPhase
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
	}


	override public void StartMoveChessPlayer(Chess c){
		if (Main.Inst.Action_Chance == 0 )
		{
			return;
		}
		StartMoveChess (c);
	}

	//走棋
	override public void moveChessPlayer(ChessContainer targetContainer)
	{
		if (Main.Inst.moving_chess == null || Main.Inst.Action_Chance == 0 )
			return;
        //非战斗区域不能进入
        if (!Main.Inst.lv_ctrl.AreaBattle.Contains(targetContainer.number))
            return;

        if (!targetContainer.move_flag) return;

		moveChess (Main.Inst.moving_chess, targetContainer, true);
		//流程标志
		Main.Inst.Action_Chance--;
		GoNext ();
	}
	//下棋 
	//下棋成功后前往主要阶段2
    override public void setChessPlayer(int card_id,ePlayer card_belong,ChessContainer grid)
	{
        if (Main.Inst.select_card == null || Main.Inst.Action_Chance == 0) 
            return;
        //非战斗区域不能进入
        if (!Main.Inst.lv_ctrl.AreaBattle.Contains(grid.number))
            return;
        
		setChess (card_id, card_belong, grid,BKKZ.POW01.AI.eAI_Type.Default,true, GameRule.PlayerChessEventID,0);
		//流程标志
		Main.Inst.Action_Chance--;
		GoNext ();
	}
}
