using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using BKKZ.POW01;
//玩家回合的开始，显示玩家信息
public class EnemyDrawPhase :TurnPhase
{
	void Update(){
        if (!Main.Inst.game_state.CanPhaseRun())
            return;
		//动画播放期间不允许影响流程的操作
		if (!Main.Inst.isStageClear ())
			return;
		//n(*≧▽≦*)n 其实抽排阶段什么事情都不能做
	}
	public override void Start(){
		Debug.Log("抽牌阶段");
		GameObject[] container = null;
		//回合特效标志
		GameObject[] at;	//启动
		GameObject[] af;	//关闭
		if (Main.Inst.turn_player == ePlayer.Player1)
		{
			at = Main.Inst.turn_showgirl_black;
			af = Main.Inst.turn_showgirl_white;
		} else if (Main.Inst.turn_player == ePlayer.Player2)
		{
			at = Main.Inst.turn_showgirl_white;
			af = Main.Inst.turn_showgirl_black;
		} else if (Main.Inst.turn_player >= ePlayer.Enemy1)
		{
			at = Main.Inst.turn_showgirl_white;
			af = Main.Inst.turn_showgirl_black;
		} else
			return;
		foreach (GameObject obj in at)
		{
			obj.SetActive(true);
		};
		foreach (GameObject obj in af)
		{
			obj.SetActive(false);
		};
		Main.Inst.player_1.onDamage(0);
		Main.Inst.player_2.onDamage(0);
		Main.Inst.player_1.FreshDeckRemain();
		Main.Inst.player_2.FreshDeckRemain();
		ResetMonsterAI ();
		StartCoroutine (phaseStart());
	}

	IEnumerator phaseStart(){
		GameObject ani_obj = null;
		if (Main.Inst.turn_player == ePlayer.Player1)
		{
			ani_obj = Main.Inst.turn_kuroko;
		} else if (Main.Inst.turn_player == ePlayer.Player2)
		{
			ani_obj = Main.Inst.turn_shiroi;
		} else if (Main.Inst.turn_player >= ePlayer.Enemy1)
		{
			ani_obj = Main.Inst.turn_shiroi;
		}
		ani_obj.SetActive(true);
		yield return new WaitForSeconds(1f);
		ani_obj.SetActive(false);
		GoNext ();
		yield return null;
	}

	void ResetMonsterAI(){
		foreach (var ai in Main.Inst.dic_ai) {
			if (ai.Value.my_chess.belong == turn_belong)
				ai.Value.Restart ();
		}
	}
}