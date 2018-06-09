using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
namespace BKKZ.POW01 {
    /// <summary>
    /// 基本是PlayerMainPhase的改版，玩家自由行动
    /// </summary>
    public class ExplorePhase : TurnPhase {
        public int StepCount = 0;
        bool is_walking = false;

        Vector3 PreGridPosition {
            get {
                return new Vector3(pre_grid.transform.position.x, pre_grid.transform.position.y,
                    -1 * target_grid.transform.parent.localScale.z);
            }
        }
        Vector3 NextGridPosition {
            get {
                return new Vector3(next_grid.transform.position.x, next_grid.transform.position.y,
                    -1 * target_grid.transform.parent.localScale.z);
            }
        }
        ChessContainer pre_grid = null;
        ChessContainer next_grid = null;
        ChessContainer target_grid = null;

        int max_grid_dis = 0;
        Dictionary<int, int> chess_grids_dist = null;
        void initData() {
            if (chess_grids_dist == null) {
                chess_grids_dist = new Dictionary<int, int>();
                foreach (var item in Main.Inst.chess_grids) {
                    chess_grids_dist.Add(item.Key, 0);
                }
            }
        }
        public override void Start() {
            Debug.Log("探索流程中");
            initData();
            StartCoroutine(phaseExplore());
        }
        IEnumerator phaseExplore() {
            //默认一直在移动
            Main.Inst.b_moving_chess = true;
            //TODO 关闭手牌
            //TODO 关闭血量
            //TODO 开启牌组详情等
            yield return null;
        }
        void Update() {
            if (!Main.Inst.game_state.CanPhaseRun())
                return;
            //动画播放期间不允许影响流程的操作
            if (!Main.Inst.isStageClear())
                return;
            WalkToTargetGrid();
        }

        /// <summary>
        /// 探索模式中没有行动次数限制，故去掉了行动机会的判断
        /// 不用开始，直接移动
        /// </summary>
        /// <param name="c">C.</param>
        override public void StartMoveChessPlayer(Chess c) {
            //StartMoveChess(c);
        }

        //走棋 操作响应
        override public void moveChessPlayer(ChessContainer targetContainer) {
            //没有行动次数限制
            if (Main.Inst.moving_chess == null) 
                return;
            //未探索出的区域不能在这个模式下进入
            if (!Main.Inst.lv_ctrl.AreaExplored.Contains(targetContainer.number))
                return;


            FindPathTo(targetContainer.number);
            if (chess_grids_dist[Main.Inst.moving_chess.container.number] > 0) {
                target_grid = targetContainer;
                is_walking = true;
                //move_countdown = move_duration;
                move_interval_countdown = float.Epsilon;
                MoveStep();
            }
            //moveChess(Main.Inst.moving_chess, targetContainer, false);
        }
        [SerializeField]
        float move_interval = 1;
        [SerializeField]
        float move_duration = 1;
        float move_countdown = 0;
        float move_interval_countdown = 0;
        [SerializeField]
        float move_event_time = 0.5f;
        void WalkToTargetGrid() {
            if (!is_walking) return;
            if (move_interval_countdown >0) {
                move_interval_countdown -= Time.deltaTime;
                if (move_interval_countdown <= 0)
                    move_countdown = move_duration;
            }
            if (move_countdown>0) {
                move_countdown -= Time.deltaTime;
                Main.Inst.moving_chess.transform.position = Vector3.Lerp(NextGridPosition, PreGridPosition, move_countdown / move_duration);
                if(move_countdown<=0) {
                    move_interval_countdown = move_interval;
                    //下一步
                    //target_grid = Main.Inst.chess_grids
                    MoveStep();
                }

                if(move_countdown < move_event_time && move_countdown + Time.deltaTime> move_event_time) {
                    Main.Inst.moving_chess.container.removeChess();


                    //target_grid.appendChess(Main.Inst.moving_chess);  //以下是这个语句的改写
                    {
                        next_grid.my_chess = Main.Inst.moving_chess;
                        Main.Inst.moving_chess.transform.parent = next_grid.transform;
                        //my_chess.transform.localPosition = Vector3.back;
                        //my_chess.transform.localScale = Vector3.one;
                        Main.Inst.moving_chess.container = next_grid;
                    }



                    //隐患 不知道这个会不会导致移动流程被破坏
                    CheckMoveEvent(Main.Inst.moving_chess, nextPhaseDefault);
                }
            }
        }
        //逻辑部分，时间表现在外面
        void MoveStep() {
            //结束条件
            if (next_grid == target_grid) {
                is_walking = false;
                return;
            }
            ChessContainer nearest = null;
            int lastdist =999;
            foreach (var item in Main.Inst.dGetChessContainer) {
                ChessContainer _cc = item(Main.Inst.moving_chess.container);
                if (_cc == null) continue;
                if (chess_grids_dist[_cc.number] == -1) continue;
                if (lastdist == 999 || chess_grids_dist[_cc.number] < lastdist) {
                    lastdist = chess_grids_dist[_cc.number];
                    nearest = _cc;
                }
            }
            StepCount++;
            pre_grid = Main.Inst.moving_chess.container;
            next_grid = nearest;
        }
        void FindPathTo(int grid_number) {
            List<int> keys = new List<int>();
            foreach (var item in chess_grids_dist) {
                keys.Add(item.Key);
            }
            foreach (var item in keys) {
                chess_grids_dist[item] = 0;
            }

            max_grid_dis = 0;
            chess_grids_dist[grid_number] = -1;
            DistDFS_Explored(grid_number,1);
            chess_grids_dist[grid_number] = 0;
        }
        
        void DistDFS_Explored(int now_pos,int nowdis) {
            foreach (var item in Main.Inst.dGetChessContainer) {
                ChessContainer cc = item(Main.Inst.chess_grids[now_pos]);

                if (cc == null)
                    continue;
                if (chess_grids_dist[cc.number] == -1)
                    continue;
                if (nowdis < chess_grids_dist[cc.number] || chess_grids_dist[cc.number] == 0) {
                    if (GameRule.judgePassable(Main.Inst.moving_chess, cc)) {
                        chess_grids_dist[cc.number] = nowdis;
                        if (nowdis > max_grid_dis)
                            max_grid_dis = nowdis;
                    } else {
                        chess_grids_dist[cc.number] = -1;
                    }
                    if(chess_grids_dist[cc.number] != -1)
                        DistDFS_Explored(cc.number, chess_grids_dist[cc.number] + 1);
                }
            }
            return;
        }
        //走棋 执行
        override public void moveChess(Chess moving_chess, ChessContainer targetContainer, bool trigger_strike) {

            moving_chess.container.removeChess();
            targetContainer.appendChess(moving_chess);
            CheckMoveEvent(moving_chess, nextPhaseDefault);
        }
        //下棋 探索模式没有召唤截断操作
        override public void setChessPlayer(int card_id, ePlayer card_belong, ChessContainer grid) {
            return;
            if (Main.Inst.select_card == null || Main.Inst.moving_chess!=null) {
                return;
            }
            setChess(card_id, card_belong, grid, AI.eAI_Type.Default, false, GameRule.PlayerChessEventID,0);
            Debug.Log("从explore召唤");
            return;
        }

        //探索阶段不会像战斗模式一样每次操作都切换阶段，只在检测事件后切换
        override public void GoNext(TurnPhase toPhase) {
            TurnPhase insert_phase = null;

            Main.Inst.CheckAndInsertPhase(ref insert_phase, toPhase, eEventTrigger.Phase_Change,null);

            if (insert_phase != null) {
                insert_phase.gameObject.SetActive(true);
                Main.Inst.now_turnphase = insert_phase;
                Destroy(gameObject);
            }
        }
        override public void SelectCard(Card card) {
            if (card == null) {
                return;
            }
            if (Main.Inst.turn_player == card.owner) {
                Main.Inst.select_card = card; 
                //Main.Inst.moving_chess = null;//探索模式下不取消移动准备
                Main.Inst.setCardInfo(card.card_id, null);
            }
            //告诉点击管理，点了张卡
            Main.Inst.click_target = eMouseDownTarget.Card;
        }
        //绘制距离
        void OnDrawGizmos(){
            if (chess_grids_dist.Count > 0) {
                foreach (var item in chess_grids_dist) {
                    Gizmos.color = BKTools.HSVtoRGB(1.0f*item.Key/max_grid_dis, 1, 1);;
                    //Gizmos.DrawLine(p1, p2);
                    //Gizmos.DrawSphere(p2, 0.1f);
                }
            }
        }
    }
}