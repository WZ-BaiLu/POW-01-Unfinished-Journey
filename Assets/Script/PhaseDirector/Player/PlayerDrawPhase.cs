using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using BKKZ.POW01;
//玩家回合的开始，显示玩家信息
public class PlayerDrawPhase :TurnPhase
{

	PlayerBoard _turnplayer = null;
	void Update(){
		//动画播放期间不允许影响流程的操作
		if (!Main.Inst.isStageClear ())
			return;
		//n(*≧▽≦*)n 其实抽排阶段什么事情都不能做
	}
	public override void Start(){
		Debug.Log("抽牌阶段");
		GameObject[] container = null;
        //玩家回合开始，回合计数加一    回合数
        Main.Inst.lv_ctrl.now_turn++;
        Main.Inst.lv_ctrl.total_turn++;

        //触发回合切换事件
        TurnPhase newPhase = null;
        Main.Inst.CheckAndInsertPhase(ref newPhase, nextPhaseDefault, eEventTrigger.Reach_turn,null);
        if (newPhase != null)
            nextPhaseDefault = newPhase;

		//预留PVE和PVP两种情况的处理
		switch(Main.Inst.lv_ctrl.map_data.my_type) {
		case eMapType.PvE_Mult:
			_turnplayer = Main.Inst.player_1;
			container = Main.Inst.cardcont_p1;
			break;
		case eMapType.PvE_Solo:
			_turnplayer = Main.Inst.player_1;
			container = Main.Inst.cardcont_p1;
			//没有特殊处理
			break;
		case eMapType.PvP_2P:
			//根据回合归属方重新排布操作者的界面
			//！！联网版制作前该功能仅用于单客户端双人对战
			//切换界面
			if (Main.Inst.turn_player == ePlayer.Player1)
			{
				_turnplayer = Main.Inst.player_1;
				container = Main.Inst.cardcont_p1;
				Main.Inst.player_1.setBorderOn();
				Main.Inst.player_2.setBorderOff();
			} else if (Main.Inst.turn_player == ePlayer.Player2)
			{
				_turnplayer = Main.Inst.player_2;
				container = Main.Inst.cardcont_p2;
				Main.Inst.player_1.setBorderOff();
				Main.Inst.player_2.setBorderOn();
			} else
			{
				Debug.Log("回合开始时发生不明原因，回合进攻玩家不明");
				return;
			}
			break;
		default:
			break;
		}

		Main.Inst.clearCardInfo();


		Main.Inst.b_attacked = false;
		Main.Inst.b_moving_chess = false;
		if (_turnplayer.hand_cards.Count < 5)
		{
			if (!_turnplayer.orenotan_draw())
			{
				Debug.Log("牌堆没牌了");
				//return;
			}
			//removeCard((Card)_turnplayer.hand_cards[0]);
		}
		Main.Inst.b_setchess = false;
		Main.Inst.setHand(_turnplayer.hand_cards, container);
		//clickCardContainer(container[0].GetComponent<CardContainer>());
//		Main.Instance.b_phase_trigger = true;//d
//		Main.Instance.now_phase++;//d


		GameObject[] at;	//启动
		GameObject[] af;	//关闭
		if (_turnplayer.ID == ePlayer.Player1)
		{
			at = Main.Inst.turn_showgirl_black;
			af = Main.Inst.turn_showgirl_white;
		} else if (_turnplayer.ID == ePlayer.Player2)
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

		StartCoroutine (phaseStart());
	}

	IEnumerator phaseStart(){
		GameObject ani_obj;
		if (_turnplayer.ID == ePlayer.Player1)
		{
			ani_obj = Main.Inst.turn_kuroko;
		} else
		{
			ani_obj = Main.Inst.turn_shiroi;
		}
		ani_obj.SetActive(true);
		yield return new WaitForSeconds(1f);
		ani_obj.SetActive(false);
		GoNext ();
		yield return null;
	}

}