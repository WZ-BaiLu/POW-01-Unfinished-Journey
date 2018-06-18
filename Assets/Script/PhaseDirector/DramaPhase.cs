using UnityEngine;
using BKKZ.POW01.AI;
using System;
using System.Collections;
using System.Collections.Generic;
namespace BKKZ.POW01 {
    public class DramaPhase : TurnPhase {
        public DramaScript drama_script;
        bool analya_finish = true;
        // Use this for initialization
        override public void Start() {
            Debug.Log("剧情表演");
        }

        // Update is called once per frame
        void Update() {
            //动画播放期间不允许影响流程的操作
            if (!Main.Inst.isStageClear() || !analya_finish)
                return;
            RunDrama();
        }
        public void RunDrama(){

            DramaSection section = drama_script.getNext();
            if (section == null) {
                GoNext();
            } else
                AnalysisSection(section);
        }

        void AnalysisSection(DramaSection section) {
            analya_finish = false;
            switch (section.my_type) {
            case eDramaOperationType.Move:
                break;
            case eDramaOperationType.ShouKan:
                StartCoroutine(corShoukan(section));
                break;
            case eDramaOperationType.Speak:
                PerformSpeak(section);
                break;
            case eDramaOperationType.Skill:
                break;
            case eDramaOperationType.Switch:
                LogicModifySwitch(section);
                break;
            case eDramaOperationType.Variable:
                LogicModifyVariable(section);
                break;
            case eDramaOperationType.FogOn:
                StartCoroutine(corFogCover(section));
                break;
            case eDramaOperationType.FogOff:
                StartCoroutine(corFogLift(section));
                break;
            case eDramaOperationType.Win:
                Main.Inst.GameOver(ePlayer.Player2);
                break;
            case eDramaOperationType.Lose:
                break;
            case eDramaOperationType.ExploreStart:
                StartCoroutine(corExploreStart(section));
                break;
            case eDramaOperationType.BattleStart:
                StartCoroutine(corBattleStart(section));
                break;
            case eDramaOperationType.ShowArea:
                StartCoroutine(corShowArea(section));
                break;
            case  eDramaOperationType.HideArea:
                StartCoroutine(corHideArea(section));
                break;
            case eDramaOperationType.ExplorContinue:
                StartCoroutine(corExploreContinue(section));
                break;
            default:
                Debug.LogError("剧本数据出错");
                break;
            }
        }
        void PerformSpeak(DramaSection section){
            UILevelMain.Inst.ShowDialoge(section.manfenzuowen,DialogeCallback);
        }
        void DialogeCallback(){
            analya_finish = true;
        }
        //修改全局开关和变量
        void LogicModifySwitch(DramaSection section) {
            switch (section.op_boolean) {
            case eDOType_boolean.与:
                Main.Inst.lv_ctrl.global_switch[section.variable_index] = Main.Inst.lv_ctrl.global_switch[section.variable_index] && section.v_boolean;
                break;
            case eDOType_boolean.或:
                Main.Inst.lv_ctrl.global_switch[section.variable_index] = Main.Inst.lv_ctrl.global_switch[section.variable_index] || section.v_boolean;
                break;
            case eDOType_boolean.赋值:
                Main.Inst.lv_ctrl.global_switch[section.variable_index] = section.v_boolean;
                break;
            default:
                break;
            }

        }
        //修改全局开关和变量
        void LogicModifyVariable(DramaSection section) {
            switch (section.op_int) {
            case eDOType_int.乘:
                Main.Inst.lv_ctrl.global_variable[section.variable_index] *= section.v_int;
                break;
            case eDOType_int.减:
                Main.Inst.lv_ctrl.global_variable[section.variable_index] -= section.v_int;
                break;
            case eDOType_int.加:
                Main.Inst.lv_ctrl.global_variable[section.variable_index] += section.v_int;
                break;
            case eDOType_int.赋值:
                Main.Inst.lv_ctrl.global_variable[section.variable_index] = section.v_int;
                break;
            case eDOType_int.除:
                Main.Inst.lv_ctrl.global_variable[section.variable_index] /= section.v_int;
                break;
            case eDOType_int.平方:
                Main.Inst.lv_ctrl.global_variable[section.variable_index] = (int)Mathf.Pow(Main.Inst.lv_ctrl.global_variable[section.variable_index], section.v_int);
                break;
            }

        }
        IEnumerator corShowArea(DramaSection section) {
            //开放活动区域
            foreach (var item in Main.Inst.lv_ctrl.map_data.list_area_grid[section.area_id].list) {
                //驱散迷雾
                BKTools.FogLift(item, 1, GameRule.Default_PvE_Fog_Lift_Range, GameRule.ePlayerIndex);
                //记录已探索
                Main.Inst.lv_ctrl.AreaExplored.Add(item);
            }
            analya_finish = true;
            yield return null;
        }
        IEnumerator corHideArea(DramaSection section) {
            //开放活动区域
            foreach (var item in Main.Inst.lv_ctrl.map_data.list_area_grid[section.area_id].list) {
                //驱散迷雾
                BKTools.FogCover(item, 0, GameRule.ePlayerIndex);
                //记录已探索
                if(Main.Inst.lv_ctrl.AreaExplored.Contains(item))
                    Main.Inst.lv_ctrl.AreaExplored.Remove(item);
            }
            analya_finish = true;
            yield return null;
        }
        //探索开始
        IEnumerator corExploreStart(DramaSection section) {
            //开房活动区域
            foreach (var item in Main.Inst.lv_ctrl.map_data.list_area_grid[section.area_id].list) {
                //驱散迷雾
                BKTools.FogLift(item, 1, GameRule.Default_PvE_Fog_Lift_Range, GameRule.ePlayerIndex);
                //记录已探索
                Main.Inst.lv_ctrl.AreaExplored.Add(item);
            }
            //召唤王
            int temp_lord_id = 1001;
            ePlayer belong = section.belong;
            ChessContainer grid = Main.Inst.chess_grids[section.to_grid_id];

            //创建棋子
            Main.Inst.moving_chess = Main.Inst.now_turnphase.setChess(temp_lord_id, ePlayer.Player1, grid, section.my_argu_AI_ID, false, GameRule.PlayerChessEventID,0);
            //设置镜头跟踪方式
            Camera.main.GetComponent<ExBaCamera>().SetFollowTarget(Main.Inst.moving_chess);
            analya_finish = true;
            yield return null;
        }
        static string KEY_corBattleStart = "drama-battlestart";
        IEnumerator corBattleStart(DramaSection section) {
            Main.Inst.addDancer(KEY_corBattleStart);
            Main.Inst.lv_ctrl.now_turn = 0;   // 重新计算回合数
            //存储探索进度
            Main.Inst.lv_ctrl.ExplorePos = Main.Inst.moving_chess.container.number;
            // 遮盖已开放区域
            foreach (var item in Main.Inst.lv_ctrl.AreaExplored) {
                BKTools.FogCover(item,1,GameRule.ePlayerIndex);
            }
            yield return new WaitUntil(BKTools.ISCWithArgu(KEY_corBattleStart));
            //隐藏探索骑士
            //Main.Inst.moving_chess.container.removeChess();
            //Main.Inst.moving_chess.gameObject.SetActive(false);
            // 开放战斗区域
            foreach (var item in Main.Inst.lv_ctrl.map_data.list_area_grid[section.area_id].list) {
                Main.Inst.lv_ctrl.AreaBattle.Add(item);
                BKTools.FogLift(item, 1, GameRule.Default_PvE_Fog_Lift_Range, GameRule.ePlayerIndex);
            }
            // 召唤玩家默认阵容
            //TODO 从牌组设置读取
            Main.Inst.moving_chess.container.removeChess();
            Main.Inst.chess_grids[section.to_grid_id].appendChess(Main.Inst.moving_chess);
            //setChessPlayer(1001, ePlayer.Player1, Main.Inst.chess_grids[section.to_grid_id]);
            Debug.Log("草拟吗，到底能不能玩断点啦!");
            // 切换阶段 （不能使用插入，不然会使得当前不止一个阶段对象响应事件——已经造成过召唤剧情成功生成未执行时被认为已经被杀光的错误
            AppendPhase(Instantiate(BKTools.getBundleObject(eResBundle.Prefabs, PrefabPath.PlayerDrawPhase), Main.Inst.NodePhaseControler.transform).GetComponent<TurnPhase>());
            Main.Inst.redDancer(KEY_corBattleStart);
            //召唤怪物阵容 - 由原来的事件机制完成


            //设置镜头跟踪方式
            Camera.main.GetComponent<ExBaCamera>().SetFollowTarget(section.area_id);
            analya_finish = true;
        }
        const string KEYExCont = "corExploreContinue";
        IEnumerator corExploreContinue(DramaSection section) {
            Main.Inst.addDancer(KEYExCont);
            // 展示战斗成果
            Main.Inst.panBattleSummary.Show();
            yield return new WaitUntil(BKTools.ISCWithArgu(KEYExCont));
            //开放活动区域 TODO 关闭战斗区域特效
            foreach (var item in Main.Inst.lv_ctrl.AreaBattle) {
                //驱散迷雾
                BKTools.FogCover(item, 1, GameRule.ePlayerIndex);
            }
            //恢复探索区域
            yield return new WaitUntil(BKTools.ISCWithArgu(KEYExCont));
            //清理战斗人员
            foreach (var chess in Main.Inst.dic_chess) {
                chess.Value.container.removeChess();
                chess.Value.gameObject.SetActive(false);
            }
            // 召回探索骑士
            Main.Inst.moving_chess = Main.Inst.now_turnphase.setChess(1001, ePlayer.Player1, Main.Inst.chess_grids[Main.Inst.lv_ctrl.ExplorePos], section.my_argu_AI_ID, false, GameRule.PlayerChessEventID,0);
            Main.Inst.b_moving_chess = true;
            //开放活动区域
            foreach (var item in Main.Inst.lv_ctrl.AreaExplored) {
                //驱散迷雾
                BKTools.FogLift(item, 1, GameRule.Default_PvE_Fog_Lift_Range, GameRule.ePlayerIndex);
            }
            yield return new WaitUntil(BKTools.ISCWithArgu(KEYExCont));
            // 切换阶段 —— 被清理后自动接上探索
            if (Main.Inst.now_turnphase.nextPhaseDefault.gameObject.scene.name != null)
                Destroy(Main.Inst.now_turnphase.nextPhaseDefault.gameObject);
            Main.Inst.now_turnphase.nextPhaseDefault = Main.Inst.explore_phase;
            analya_finish = true;
            Main.Inst.redDancer(KEYExCont);
        }
        IEnumerator corShoukan(DramaSection section) {
            //        int id = int.Parse (script.my_argu [0]);
            //        ePlayer belong = (ePlayer)int.Parse (script.my_argu [1]);
            //        ChessContainer grid = Main.Instance.chess_grids [int.Parse (script.my_argu [2])];
            int id = section.chess_id;
            ePlayer belong = section.belong;
            ChessContainer grid = Main.Inst.chess_grids[section.to_grid_id];

            //创建棋子
            Main.Inst.addDancer("shoukan-" + id);//召唤操作会直接开始清理流程，我还没加标记呢，草拟吗的
            int shoukan_sequence = Chess.GetSequenceInEvent(section.from_event);
            Main.Inst.now_turnphase.setChess(id, belong, grid, section.my_argu_AI_ID, true, section.from_event,shoukan_sequence);
            yield return new WaitForSeconds(GameRule.Shoukan_Duration);
            Main.Inst.redDancer("shoukan-" + id);
            analya_finish = true;
        }
        IEnumerator corMove(DramaSection section) {
            //移动者 TODO 解析要改成后面多4位的形式 暂不修改
            int id = section.chess_id;
            Chess ch = Main.Inst.dic_chess[id];
            StartMoveChess(ch);
            Main.Inst.addDancer("move-" + id);
            yield return new WaitForSeconds(GameRule.Moveing_Duration);
            Main.Inst.redDancer("move-" + id);

            //移动方式
            eDOType_Move movetype = section.move_type;
            bool is_force_move = false;
            ChessContainer grid = null;
            Point3 locater = section.move_argu;
            switch (movetype) {
            case eDOType_Move.Force_Move_ABS:
                is_force_move = true;
                grid = Main.Inst.chess_grids[locater.x];
                break;
            case eDOType_Move.Force_Move_REL:
                is_force_move = true;
                grid = BKTools.LocateChessGrid(ch.container, locater);
                break;
            case eDOType_Move.Normal_Move_ABS:
                grid = Main.Inst.chess_grids[locater.x];
                break;
            case eDOType_Move.Norma_lMove_REL:
                grid = BKTools.LocateChessGrid(ch.container, locater);
                break;
            default:
                Debug.LogError("剧情移动方式错误");
                break;
            }
            moveChess(ch, grid, false);
            yield return null;
        }
        IEnumerator corFogLift(DramaSection script) {
            BKTools.FogLift(script.to_grid_id, script.v_int + GameRule.Default_PvE_Fog_Lift_Range, GameRule.Default_PvE_Fog_Lift_Range, GameRule.ePlayerIndex);
            yield return new WaitForSeconds(1f);
            analya_finish = true;
            yield return null;
        }
        //TODO 迷雾目标一般是玩家，这个设定未能在编辑器中设置
        IEnumerator corFogCover(DramaSection script) {
            BKTools.FogCover(script.to_grid_id, script.v_int, GameRule.ePlayerIndex);
            yield return new WaitForSeconds(1f);
            analya_finish = true;
            yield return null;
        }
        IEnumerator corSpeak(DramaSection script) {
            yield return null;
        }
        IEnumerator corSkill(DramaSection script) {
            yield return null;
        }


        /// <summary>
        /// 要求两边都是实例，不能是prefab
        /// 经过一次实验，这个功能但对对dramaPhase使用，因为dramaphase默认没有next
        /// 如果是插入形式，会使正常流程的下一步被null替代
        /// </summary>
        /// <param name="_toPhase">会被自动设置成为激活，等待流程激活</param>
        public void AppendPhase(TurnPhase _toPhase) {
            if (_toPhase == null) {
                return;
            }
            _toPhase.gameObject.SetActive(false);
            //TODO 保险，如果对非实例进行nextPhaseDefault的修改，就报错
            if (gameObject.scene.name == null) {
                Debug.LogError("不应该对预制体进行这步操作");
            }
            if (nextPhaseDefault != null && nextPhaseDefault.gameObject.scene.name == null) {
                Debug.LogError("不应该对预制体进行这步操作");
                return;
            }

            //TurnPhase _temp = nextPhaseDefault;
            //if (nextPhaseDefault != null)
            //_toPhase.nextPhaseDefault = _temp;

            //接入链表
            nextPhaseDefault = _toPhase;
        }
    }

}