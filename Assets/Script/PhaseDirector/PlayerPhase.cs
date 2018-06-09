using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using BKKZ.POW01;

public class PlayerPhase
{
	//接受操作
	//播放动画
	//处理流程功能
	public void Draw(PlayerBoard _turnplayer){
		//切换界面
		GameObject[] container = null;
		switch(Main.Inst.lv_ctrl.map_data.my_type){
		case eMapType.PvE_Solo:
		//PvE下仅一人，不做显示处理
			break;
		case eMapType.PvE_Mult:
		//PvP 正式游戏的单个客户端只显示一个人的数据
			break;
		case eMapType.PvP_2P:
		//PvP下的处理
			if (_turnplayer.ID == ePlayer.Player1) {
				container = Main.Inst.cardcont_p1;
				Main.Inst.player_1.setBorderOn ();
				Main.Inst.player_2.setBorderOff ();
			} else {
				container = Main.Inst.cardcont_p2;
				Main.Inst.player_1.setBorderOff ();
				Main.Inst.player_2.setBorderOn ();
			}
			Main.Inst.setHand (_turnplayer.hand_cards, container);
			break;
		}
		//回合开始秀
//		Main.Instance.TurnStart(_turnplayer.ID);

		//清空操作标记
		Main.Inst.b_attacked = false;
		Main.Inst.b_moving_chess = false;

		//玩家抽牌逻辑
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

		Main.Inst.b_phase_trigger = true;
		Main.Inst.now_phase_type++;
	}
}
////玩家回合的开始，显示玩家信息
//public class PlayerDrawPhase :TurnPhase
//{
//
//	Player _turnplayer = null;
//	void Update(){
//		//动画播放期间不允许影响流程的操作
//		if (!Main.Instance.isStageClear ())
//			return;
//		//n(*≧▽≦*)n 其实抽排阶段什么事情都不能做
//	}
//	void Start(){
//		base.Start();
//		Debug.Log("抽牌阶段");
//		GameObject[] container = null;
//
//
//		//预留PVE和PVP两种情况的处理
//		switch(Main.Instance.levelinfo.map_data.my_type) {
//		case eMapType.PvE_Mult:
//			_turnplayer = Main.Instance.player_1;
//			container = Main.Instance.cardcont_p1;
//			break;
//		case eMapType.PvE_Solo:
//			_turnplayer = Main.Instance.player_1;
//			container = Main.Instance.cardcont_p1;
//			//没有特殊处理
//			break;
//		case eMapType.PvP_2P:
//			//根据回合归属方重新排布操作者的界面
//			//！！联网版制作前该功能仅用于单客户端双人对战
//			//切换界面
//			if (Main.Instance.turn_player == ePlayer.Player1)
//			{
//				_turnplayer = Main.Instance.player_1;
//				container = Main.Instance.cardcont_p1;
//				Main.Instance.player_1.setBorderOn();
//				Main.Instance.player_2.setBorderOff();
//			} else if (Main.Instance.turn_player == ePlayer.Player2)
//			{
//				_turnplayer = Main.Instance.player_2;
//				container = Main.Instance.cardcont_p2;
//				Main.Instance.player_1.setBorderOff();
//				Main.Instance.player_2.setBorderOn();
//			} else
//			{
//				Debug.Log("回合开始时发生不明原因，回合进攻玩家不明");
//				return;
//			}
//			break;
//		default:
//			break;
//		}
//
//		Main.Instance.clearCardInfo();
//
//
//		Main.Instance.b_attacked = false;
//		Main.Instance.b_moving_chess = false;
//		if (_turnplayer.hand_cards.Count < 5)
//		{
//			if (!_turnplayer.orenotan_draw())
//			{
//				Debug.Log("牌堆没牌了");
//				//return;
//			}
//			//removeCard((Card)_turnplayer.hand_cards[0]);
//		}
//		Main.Instance.b_setchess = false;
//		Main.Instance.setHand(_turnplayer.hand_cards, container);
//		//clickCardContainer(container[0].GetComponent<CardContainer>());
////		Main.Instance.b_phase_trigger = true;//d
////		Main.Instance.now_phase++;//d
//
//
//		GameObject[] at;	//启动
//		GameObject[] af;	//关闭
//		if (_turnplayer.ID == ePlayer.Player1)
//		{
//			at = Main.Instance.turn_showgirl_black;
//			af = Main.Instance.turn_showgirl_white;
//		} else if (_turnplayer.ID == ePlayer.Player2)
//		{
//			at = Main.Instance.turn_showgirl_white;
//			af = Main.Instance.turn_showgirl_black;
//		} else
//			return;
//		foreach (GameObject obj in at)
//		{
//			obj.SetActive(true);
//		};
//		foreach (GameObject obj in af)
//		{
//			obj.SetActive(false);
//		};
//		Main.Instance.player_1.onDamage(0);
//		Main.Instance.player_2.onDamage(0);
//		Main.Instance.player_1.FreshDeckRemain();
//		Main.Instance.player_2.FreshDeckRemain();
//
//		StartCoroutine (phaseStart());
//	}
//
//	IEnumerator phaseStart(){
//		GameObject ani_obj;
//		if (_turnplayer.ID == ePlayer.Player1)
//		{
//			ani_obj = Main.Instance.turn_kuroko;
//		} else
//		{
//			ani_obj = Main.Instance.turn_shiroi;
//		}
//		ani_obj.SetActive(true);
//		yield return new WaitForSeconds(1f);
//		ani_obj.SetActive(false);
//		GoNext ();
//		yield return null;
//	}
//
//}
//public class PlayerPreparePhase :TurnPhase
//{
//	void Start(){
//		base.Start();
//		Debug.Log("准备阶段");
//		StartCoroutine (phasePrepare());
//	
//		//获得行动机会
//		Main.Instance.Action_Chance = GameRule.Default_PvE_Action_Chance;
//	}
//	IEnumerator phasePrepare()
//	{
//		//准备阶段处理
//		BuffContrllor.Deal_Effect(Main.Instance, false, eBuffEvent.Phase_Prepare);
//		yield return new WaitUntil(Main.Instance.isStageClear);
//		GoNext ();
//		yield return null;
//	}
//}
//
//
//public class PlayerMainPhase1 :TurnPhase
//{
//	void Start(){
//		base.Start();
//		Debug.Log("主流1");
//		StartCoroutine (phaseMain1());
//	}
//	IEnumerator phaseMain1()
//	{
//		//准备阶段处理
//		BuffContrllor.Deal_Effect(Main.Instance, false, eBuffEvent.Phase_Main1);
//		yield return new WaitUntil(Main.Instance.isStageClear);
//		yield return null;
//	}
//	void Update(){
//		//动画播放期间不允许影响流程的操作
//		if (!Main.Instance.isStageClear ())
//			return;	
//	}
//
//
//	public void StartMoveChess(Chess c){
//		if (Main.Instance.Action_Chance>0)
//		{
//			Debug.Log("//TODO overlay");
//			//当被施加不可移动BUFF时跳出
//			if (BuffContrllor.ContainEffect(c, eBuff_Effect.Move_BAN)) 
//				return;
//
//			Main.Instance.setCardInfo(c.attribute.card_id, c);
//			if (c.controller != Main.Instance.turn_player)
//				return;
//
//			Chess.clear_Moveable_Area();
//			c.show_movable_area();
//			Main.Instance.b_moving_chess = true;
//			Main.Instance.moving_chess = c;
//			Main.Instance.showUnmovableArea(Main.Instance.turn_player);
//		}
//	}
//
//	//走棋
//	public void moveChess(ChessContainer targetContainer)
//	{
//		/*  移动耗牌
//        if (Data.MOVE_COST_HANDCARD)
//        {
//            CardContainer cdc;
//            GameObject[] cdcs;
//            ArrayList hand;
//            if (turn_player == ePlayer.Player1)
//            {
//                cdc = cardcont_p1[0].GetComponent<CardContainer>();
//                cdcs = cardcont_p1;
//                hand = player_1.hand_cards;
//            } else
//            {
//                cdc = cardcont_p2[0].GetComponent<CardContainer>();
//                cdcs = cardcont_p2;
//                hand = player_2.hand_cards;
//            }
//            if (cdc.gameObject.transform.childCount <= 0)
//            {
//                return;
//            }
//            Card card = cdc.gameObject.transform.GetChild(0).gameObject.GetComponent<Card>();
//            if (!judgeSelectCard(card))
//                return;
//            removeCard(card);
//            setHand(hand, cdcs);
//        }*/
//
//		Main.Instance.moving_chess.transform.parent = targetContainer.transform;
//		Main.Instance.moving_chess.transform.localPosition = Vector3.back;
//		Main.Instance.moving_chess.container.removeChess();
//		targetContainer.addChess(Main.Instance.moving_chess);
//
//		//攻击
//		if (!BuffContrllor.ContainEffect(Main.Instance.moving_chess, eBuff_Effect.Attack_BAN))
//			Main.Instance.moving_chess.CheckStrike(0);
//		//卡牌信息显示
//		Main.Instance.clearCardInfo();
//		//流程标志
//		Main.Instance.Action_Chance--;
//		//TODO 流程控制更新
////		Main.Instance.b_setchess = true;
////		Main.Instance.now_phase = ePhase.Battle;
////		Main.Instance.b_phase_trigger = true;//从战斗阶段往主要阶段2
//
//		//清除移动操作暴击
//		Main.Instance.b_moving_chess = false;
//		Chess.clear_Moveable_Area();
//		//清理所有移动标记(上面谁写的，都是错别字！
//		foreach (ChessContainer obj in Main.Instance.chess_grids_available)
//		{
//			obj.clearMoveFlag();
//		}
//	}
//	//下棋 
//	//下棋成功后前往主要阶段2
//	public void setChess(ChessContainer nearest)
//	{
//		if (Main.Instance.select_card == null
////		|| Main.Instance.b_setchess 
//		|| Main.Instance.Action_Chance == 0 
//		|| Main.Instance.now_phase != ePhase.Main1)
//		{
//			return;
//		}
//		GameObject[] cardcont;
//		List<Card> handcards;
//		if (Main.Instance.turn_player == ePlayer.Player1)
//		{
//			cardcont = Main.Instance.cardcont_p1;
//			handcards = Main.Instance.player_1.hand_cards;
//
//			//下棋后自动选择对方的第一张卡作为棋子 改到了回合开始阶段
//			//clickCardContainer(cardcont_p2[0].GetComponent<CardContainer>());
//		} else
//		{
//			//setHand(hand_p2, cardcnt_p2);
//			cardcont = Main.Instance.cardcont_p2;
//			handcards = Main.Instance.player_2.hand_cards;
//
//			//clickCardContainer(cardcont_p1[0].GetComponent<CardContainer>());
//		}
//		//创建棋子
//		Chess newchess = ((GameObject)GameObject.Instantiate(Resources.Load("Prefabs/chess"))).GetComponent<Chess>();
//		newchess.attribute.card_id = Main.Instance.select_card.card_id;
//		newchess.transform.parent = nearest.transform;
//		newchess.transform.localPosition = Vector3.back;
//		newchess.transform.localScale = Vector3.one;
//		nearest.addChess(newchess);
//		newchess.controller = Main.Instance.turn_player;
//		newchess.owner = Main.Instance.turn_player;
//		newchess.MouseDown = Main.Instance.MouseDownOnChess;
//		newchess.MouseUp = Main.Instance.MouseUpOnChess;
//		//初始化数据
//		newchess.initData();
//		//初始化图片
//		newchess.initImage();
//
//		//流程标志
//		Main.Instance.Action_Chance--;
//
////		Main.Instance.b_setchess = true;
////		Main.Instance.now_phase = ePhase.Battle;
////		Main.Instance.b_phase_trigger = true;//从战斗阶段往主要阶段2
//
//		Main.Instance.list_chess.Add(newchess);
//
//		//去掉手牌
//		Main.Instance.removeCard(Main.Instance.select_card);
//		Main.Instance.clearCardInfo();
//		Main.Instance.select_card = null;
//		//对齐手牌
//		Main.Instance.setHand(handcards, cardcont);
//	}
//}
//
//public class PlayerBattlePhase :TurnPhase
//{
//	public TurnPhase MainPhase;
//	void Start(){
//		base.Start();
//		Debug.Log("战流");
//	} 
//	void Update(){
//		if (Main.Instance.b_battbe_phase_pincer_over && Main.Instance.dead_list.Count == 0 && Chess.driving_skill_list.Count == 0 && !Chess.is_doing_shoukan) {
//			if(Main.Instance.Action_Chance==0)
//				GoNext ();
//			else 
//				GoNext(MainPhase);
//		}
//	}
//}
//
//public class PlayerEndPhase :TurnPhase
//{
//	void Start(){
//		base.Start();
//		Debug.Log("结束流程");
//
//		GoNext ();
//		TurnEnd_Buff ();
//	} 
//
//	void TurnEnd_Buff(){
//		BKKZ.BuffContrllor.Deal_Effect (Main.Instance, true, eBuffEvent.Phase_End);
////		int buffcount = 0, buff_duration_count = 0;
////		foreach (Chess c in Main.Instance.list_chess) {
////			foreach (BKKZ.Buff item in c.my_buffs) {
////				buffcount++;
////				buff_duration_count += item.my_Duration;
////			}
////		}
//	}
//}