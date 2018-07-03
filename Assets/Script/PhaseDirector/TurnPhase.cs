using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BKKZ.POW01;
/// 抽牌、准备、主流
public enum ePhaseType{
	Draw = 0,
	Prepare,
	Main1,
	Battle,
	Main2,
	End,
	Drama,
    Explore,
	All
}
public class TurnPhase : MonoBehaviour
{
	public ePhaseType myType;
	public ePlayer turn_belong;	//阶段行动方
	public static string PhaseControllerNameInScene = "PhaseController";
	public TurnPhase nextPhaseDefault;

	// Use this for initialization
	virtual public void Start ()
	{
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.K)) {
			Debug.Log (gameObject.name);
		}
		if (Input.GetKeyDown (KeyCode.N)) {
			GoNext();
		}

	}
    virtual public void GoNext(){
		GoNext (nextPhaseDefault);
	}
	virtual public void GoNext(TurnPhase toPhase){

        //保险，剧情表演完全结束前不会进入下一个阶段，而玩家的任意操作进入下一个阶会因为行动机会没用完而回到主流1
        if (turn_belong == ePlayer.Drama) {
            DramaPhase drama = (DramaPhase)this;
            if (drama != null && !drama.drama_script.IsOver)
                return;
        }

        TurnPhase insert_phase = null;
        if (toPhase != null && toPhase.gameObject.scene.name == null) {
            toPhase = Instantiate(toPhase, Main.Inst.NodePhaseControler.transform);
            toPhase.gameObject.SetActive(false);
        }
        Main.Inst.CheckAndInsertPhase(ref insert_phase, toPhase,eEventTrigger.Phase_Change,null);

		if (insert_phase!=null) {
			insert_phase.gameObject.SetActive(true);
            Main.Inst.now_turnphase = insert_phase;
		}
		else {
            if (toPhase != null) {
                toPhase.gameObject.SetActive(true);
                Main.Inst.now_turnphase = toPhase;
            }

            //如果没有下一步，也就结束了
		}
		Destroy (gameObject);
	}

	virtual public void StartMoveChess(Chess c){
		//if (Main.Inst.Action_Chance>0)
		//{
			//TODO overlay
			//当被施加不可移动BUFF时跳出
			if (BuffContrllor.ContainEffect(c, eBuff_Effect.Move_BAN)) 
				return;

			Main.Inst.setCardInfo(c.attribute.card_id, c);
			if (c.belong != Main.Inst.turn_player)
				return;

			Chess.clear_Moveable_Area();
			c.show_movable_area();
			Main.Inst.b_moving_chess = true;
			Main.Inst.moving_chess = c;
			Main.Inst.showUnmovableArea(Main.Inst.turn_player);
		//}
	}

	//走棋
	virtual public void moveChess(Chess moving_chess,ChessContainer targetContainer,bool trigger_strike)
	{
		/*  移动耗牌
        if (Data.MOVE_COST_HANDCARD)
        {
            CardContainer cdc;
            GameObject[] cdcs;
            ArrayList hand;
            if (turn_player == ePlayer.Player1)
            {
                cdc = cardcont_p1[0].GetComponent<CardContainer>();
                cdcs = cardcont_p1;
                hand = player_1.hand_cards;
            } else
            {
                cdc = cardcont_p2[0].GetComponent<CardContainer>();
                cdcs = cardcont_p2;
                hand = player_2.hand_cards;
            }
            if (cdc.gameObject.transform.childCount <= 0)
            {
                return;
            }
            Card card = cdc.gameObject.transform.GetChild(0).gameObject.GetComponent<Card>();
            if (!judgeSelectCard(card))
                return;
            removeCard(card);
            setHand(hand, cdcs);
        }*/

		moving_chess.container.removeChess();
		targetContainer.appendChess(moving_chess);

        //驱散迷雾 随设计改变而无效
        //BKTools.FogLift(moving_chess.container.number, moving_chess.attribute.spd + GameRule.Default_PvE_Fog_Lift_Range, GameRule.Default_PvE_Fog_Lift_Range, new int[] { (int)moving_chess.belong });
		//攻击
		if (trigger_strike && !BuffContrllor.ContainEffect (moving_chess, eBuff_Effect.Attack_BAN))
			moving_chess.CheckStrike (0);
		//卡牌信息显示
		Main.Inst.clearCardInfo();
		//TODO 流程控制更新
		//		Main.Instance.b_setchess = true;
		//		Main.Instance.now_phase = ePhase.Battle;
		//		Main.Instance.b_phase_trigger = true;//从战斗阶段往主要阶段2

		//清除移动操作暴击
		Main.Inst.b_moving_chess = false;//应该已经没用了
		Chess.clear_Moveable_Area();
		//清理所有移动标记(上面谁写的，都是错别字！
		foreach (var item in Main.Inst.chess_grids)
		{
			item.Value.clearMoveFlag();
		}

        CheckMoveEvent(moving_chess, nextPhaseDefault);
	}
    //在战斗中是必然切换阶段的，在探索时不一定
    virtual public void CheckMoveEvent(Chess chess_argu,TurnPhase toPhase){
        TurnPhase insert_phase = null;
        if (nextPhaseDefault != null && nextPhaseDefault.gameObject.scene.name == null) {
            nextPhaseDefault = Instantiate(nextPhaseDefault, Main.Inst.NodePhaseControler.transform);
            nextPhaseDefault.gameObject.SetActive(false);
        }
        Main.Inst.CheckAndInsertPhase(ref insert_phase, nextPhaseDefault, eEventTrigger.Kights_Stand, chess_argu);

        if (insert_phase != null) {
            insert_phase.gameObject.SetActive(true);
            Main.Inst.now_turnphase = insert_phase;

            //为探索模式特设，不销毁，只禁用
            if (myType == ePhaseType.Explore)
                gameObject.SetActive(false);
            else
                StartCoroutine(DestroyDelay(gameObject));
        } 
        /* 
         * 移动事件本身不处理插入事件意外的工作【隐患】之前工作是正常的
        else {
            if (toPhase != null) {
                toPhase.gameObject.SetActive(true);
                Main.Inst.now_turnphase = toPhase;
            }

            //如果没有下一步，也就结束了
        }
        */
    }
    //避免移动|召唤操作提前删除阶段
    IEnumerator DestroyDelay(GameObject obj) {
        yield return new WaitUntil(Main.Inst.isStageClear);
        if(gameObject != null)
            Destroy(gameObject);
    }
	//下棋 
    virtual public Chess setChess(int card_id,ePlayer card_belong,ChessContainer t_grid,BKKZ.POW01.AI.eAI_Type ai_type, bool trigger_shokan_skill, int shoukan_event, int event_sequence){
		//TODO 后期加入召唤参数，如果是强制召唤才能把当前格子上的骑士顶开
		if(t_grid.my_chess!=null)
			t_grid.my_chess.KickAway();

        //分支 pve玩家召唤时有可能是第二次召唤
        bool need_new = true;
        long chessid = 0;
        if (Main.Inst.lv_ctrl.map_data.my_type == eMapType.PvE_Mult || Main.Inst.lv_ctrl.map_data.my_type == eMapType.PvE_Solo) {
            chessid = Chess.genarateChessID(card_id, (int)card_belong, shoukan_event, event_sequence);
            need_new = !Main.Inst.dic_chess.ContainsKey(chessid);
        }
        Chess the_chess = null;
        if (need_new) {
            //创建棋子
            the_chess = Instantiate(BKTools.LoadAsset<GameObject>(eResBundle.Prefabs, PrefabPath.Chess)).GetComponent<Chess>();
            the_chess.attribute.card_id = card_id;
            the_chess.belong = card_belong;
            the_chess.owner = card_belong;
            the_chess.MouseDown = Main.Inst.MouseDownOnChess;
            the_chess.MouseUp = Main.Inst.MouseUpOnChess;
            //召唤信息——对应事件——玩家为-1
            the_chess.shoukan_event = shoukan_event;
            //事件ID确认后可以获得事件序号
            the_chess.shoukan_event_sequence = event_sequence;
            //初始化数据
            the_chess.initData();
            //初始化图片
            the_chess.initImage();

            Main.Inst.dic_chess.Add(the_chess.ChessID, the_chess);

            //创建AI
            if (card_belong < ePlayer.Player1 || card_belong > ePlayer.Player4)
                BKKZ.POW01.AI.MonsterAI.createMonsterAI(the_chess.ChessID, ai_type);
        } else {
            the_chess = Main.Inst.dic_chess[chessid];
            the_chess.gameObject.SetActive(true);
        }
        t_grid.appendChess(the_chess);
        //召唤技能
        if (trigger_shokan_skill)
            the_chess.ShouKanSkillAndPincer();

        //驱散迷雾
        //BKTools.FogLift(newchess.container.number, newchess.attribute.spd + GameRule.Default_PvE_Fog_Lift_Range, GameRule.Default_PvE_Fog_Lift_Range, new int[] { (int)newchess.belong });

        //		Main.Instance.b_setchess = true;
        //		Main.Instance.now_phase = ePhase.Battle;
        //		Main.Instance.b_phase_trigger = true;//从战斗阶段往主要阶段2

        // 只有主流会耗卡
        if (Main.Inst.now_turnphase.myType == ePhaseType.Main1)
            UseHandCard();

        if(Main.Inst.now_turnphase.myType != ePhaseType.Drama)
            CheckMoveEvent(the_chess,nextPhaseDefault);
        return the_chess;
	}
    public void UseHandCard() {
        //为本地多人保留
        GameObject[] cardcont;
        List<Card> handcards;
        if (Main.Inst.turn_player == ePlayer.Player1) {
            cardcont = Main.Inst.cardcont_p1;
            handcards = Main.Inst.player_1.hand_cards;
        } else {
            cardcont = Main.Inst.cardcont_p2;
            handcards = Main.Inst.player_2.hand_cards;
        }
        //end 为本地多人保留
        //去掉手牌
        Main.Inst.removeCard(Main.Inst.select_card);
        Main.Inst.clearCardInfo();
        Main.Inst.select_card = null;
        //对齐手牌
        Main.Inst.setHand(handcards, cardcont);
    }
    //走棋 操作响应
	virtual public void moveChessPlayer(ChessContainer targetContainer){

	}
	//下棋
	virtual public void setChessPlayer(int card_id,ePlayer card_belong,ChessContainer grid){

	}
	//开始走棋
	virtual public void StartMoveChessPlayer(Chess c){

	}
    //选牌逻辑
    virtual public void SelectCard(Card card){
        if (card == null) {
            return;
        }
        if (Main.Inst.turn_player == card.owner) {
            Main.Inst.select_card = card;
            //改为主流才能标记选卡召唤的逻辑
            if (myType == ePhaseType.Main1) {
                Main.Inst.moving_chess = null;
                Main.Inst.b_moving_chess = false;
            }
            Main.Inst.setCardInfo(card.card_id, null);
        }
        //告诉点击管理，点了张卡
        Main.Inst.click_target = eMouseDownTarget.Card;
    }
}

