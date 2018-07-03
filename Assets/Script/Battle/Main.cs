using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using BKKZ.POW01.AI;
namespace BKKZ.POW01 {
    public enum ePlayer {
        None = 0,
        Player1,    //
        Player2,
        Player3,
        Player4,    //4
        Enemy1,     //5
        Enemy2,
        Drama,      //中立/表演
        Max
    }
    /*
     * 鼠标操作
     * 棋子
     * 手牌
     * 场景（空地）
     * 	- 按下拖动
     * 格子
     */
    public enum eMouseDownTarget {
        None = 0,
        Card,
        Chess,
        MapGrid
    }
    //public bool pincerDown(Chess c, ePlayer startteam, ref ArrayList list,ref Chess _partner, ref ArrayList _result_list) {
    public delegate bool dPincer(Chess c, ePlayer startteam, ref ArrayList list, ref Chess _partner, ref ArrayList _result_list, bool isStrike);
    public delegate bool dStriker(Chess c, ePlayer startteam, ref ArrayList list, ref ArrayList _result_list);
    public delegate Chess dGetAroundChess(Chess c);
    public delegate ChessContainer dGetAroundChessContainer(ChessContainer cc);
    public class Main : MonoBehaviour { 
        static Main _inst = null;
        public static Main Inst {
            get {
                return _inst;
            }
        }
        //引用
        public Dictionary<int,ChessContainer> chess_grids;//TODO 全部棋盘格子，包含了多余空位，后期或许能删掉
        //public List<ChessContainer> chess_grids_available;//实际显示的格子
        public GameObject bg_unmovable; //不可移动区域特效
        public GameObject bg_unmovable_start; //不可移动区域特效
        public List<GameObject> list_vfx_unmovable = new List<GameObject>();

        public Vector2 chess_container_size;
        public GameObject[] cardcont_p1;
        public GameObject[] cardcont_p2;
        public PlayerBoard player_1;    //攻方
        public PlayerBoard player_2;    //守方
        public GameObject UI_gameplay;  //游戏界面的UI，方面在表演时隐藏（比如结束界面

        public UIBattleSummary panBattleSummary; //战斗总结界面
        //public CardData card_data_main_old;
        //操作状态
        public Card select_card;
        RaycastHit hit = new RaycastHit();
        //关卡信息
        public LevelController lv_ctrl;
        //卡片信息
        public Image card_info;
        public Image card_info_vct;
        public Image card_info_img;
        public Image card_info_cardface;
        public Sprite card_info_cover_w;
        public Sprite card_info_cover_b;
        public Text card_info_cost;
        public Text card_info_mana;
        public Text card_info_pow;
        public Text card_info_spd;
        //卡片技能信息
        public Text card_info_title_skill1;
        public Text card_info_content_skill1;
        public Text card_info_title_skill2;
        public Text card_info_content_skill2;
        //卡牌文字颜色修改项目
        public Text card_info_title_cost;
        public Color card_info_theme_p1_title;
        public Color card_info_theme_p1_content;
        public Color card_info_theme_p2_title;
        public Color card_info_theme_p2_content;

        //棋子数组
        public Dictionary<long, Chess> dic_chess = new Dictionary<long, Chess>();
        public Dictionary<long, MonsterAI> dic_ai = new Dictionary<long, MonsterAI>();
        public Dictionary<long, MonsterAI>.Enumerator enumerator_ai;
        //回合&阶段标志
        public GameObject turn_showgirl;
        public GameObject[] turn_showgirl_white;    //表示当前回合控制者的一些特效
        public GameObject[] turn_showgirl_black;
        public GameObject turn_kuroko;  //黑方开始图
        public GameObject turn_shiroi;  //白方开始图
        public GameObject win_kuroko;   //黑方胜利图
        public GameObject win_shiroi;   //白方胜利图
        Dictionary<string, int> stage_dancer = new Dictionary<string, int>();    //播放动画的人，TODO 原型阶段用string作为索引，要优化请思考更好的办法
        public ePlayer turn_player = ePlayer.Player1;
        //弃用
        public ePhaseType now_phase_type = ePhaseType.Draw;  //只在phasecontrol中修改，当阶段切换发生时，处理对应阶段的内容
        public TurnPhase now_turnphase;//用对象迭代取代了上面的switch形式
        public TurnPhase explore_phase;//单独存储探索阶段对象的引用
        public bool b_setchess = false; //下过棋了
        public bool b_battbe_phase_pincer_over = false;   //与dea_list共同判断战斗阶段结束(包含强袭）
        public bool b_phase_trigger = true;
        public bool b_game_start = false;
        public GameState game_state = new GameState();
        public bool b_attacked = false; //发生过夹击(回合结束时间多等待一会)

        //操作状态
        //拖屏操作(点击管理)
        public eMouseDownTarget click_target = eMouseDownTarget.None;
        Vector3 v_drag_start;
        Vector3 v_drag_now;
        Vector3 v_drag_map_start;
        bool b_is_mousedown = false;
        bool b_is_drag;
        float f_mousedown_time = 0;
        //棋子操作
        public bool b_moving_chess = false; //控制骑士进行移动
        public Chess moving_chess;
        private int action_chance = 0;
        public int Action_Chance {
            get { return action_chance; }
            set { action_chance = value; }
        }
        //夹击方法数组
        public dPincer[] dPincers = new dPincer[6];
        public dGetAroundChess[] dGetAroundsChess = new dGetAroundChess[6];
        public dGetAroundChessContainer[] dGetChessContainer = new dGetAroundChessContainer[6];
        //设置参数
        //    private float nearest_chess_container_radius = 1;
        //战斗事件
        public ArrayList overturn_list = new ArrayList();
        // Use this for initialization
#if UNITY_EDITOR
        //作弊方式
        public bool is_taboo_skill = false;
#endif

        void Start() {
            init();


            StartCoroutine(GameStart());

        }

        public static int[] chess_count_line = new int[] { 0, 4, 9, 15, 22, 28, 33 };
        void init() {
            if (_inst == null)
                _inst = this;





            //初始化
            dPincers[0] = pincerUpperLeft;
            dPincers[1] = pincerLowerRight;
            dPincers[2] = pincerUpper;
            dPincers[3] = pincerLower;
            dPincers[4] = pincerUpperRight;
            dPincers[5] = pincerLowerLeft;
            dGetAroundsChess[0] = getUpperLeftChess;
            dGetAroundsChess[1] = getLowerRightChess;
            dGetAroundsChess[2] = getUpperChess;
            dGetAroundsChess[3] = getLowerChess;
            dGetAroundsChess[4] = getUpperRightChess;
            dGetAroundsChess[5] = getLowerLeftChess;
            dGetChessContainer[0] = getUpperLeftChessContainer;
            dGetChessContainer[1] = getLowerRightChessContainer;
            dGetChessContainer[2] = getUpperChessContainer;
            dGetChessContainer[3] = getLowerChessContainer;
            dGetChessContainer[4] = getUpperRightChessContainer;
            dGetChessContainer[5] = getLowerLeftChessContainer;


            //加载地图数据
            lv_ctrl = GameObject.FindGameObjectWithTag("LevelCtrl").GetComponent<LevelController>();
            lv_ctrl.map_data = Instantiate(lv_ctrl.map_data);
            //chess_border = Instantiate(Resources.Load<GameObject>("Prefabs/chesscontainer"));
            initMapHH();
            //        //棋盘位置数据
            //        for (int i = 0; i < 37; i++)
            //        {
            //            ChessContainer cc = chess_grids[i];
            //            cc.number = i;
            //
            //            if (i < chess_count_line[1])
            //            {
            //                cc.column = 0;
            //                cc.row = i;
            //            } else if (i < chess_count_line[2])
            //            {
            //                cc.column = 1;
            //                cc.row = i - chess_count_line[1];
            //            } else if (i < chess_count_line[3])
            //            {
            //                cc.column = 2;
            //                cc.row = i - chess_count_line[2];
            //            } else if (i < chess_count_line[4])
            //            {
            //                cc.column = 3;
            //                cc.row = i - chess_count_line[3];
            //            } else if (i < chess_count_line[5])
            //            {
            //                cc.column = 4;
            //                cc.row = i - chess_count_line[4];
            //            } else if (i < chess_count_line[6])
            //            {
            //                cc.column = 5;
            //                cc.row = i - chess_count_line[5];
            //            } else
            //            {
            //                cc.column = 6;
            //                cc.row = i - chess_count_line[6];
            //            }
            //            Vector3 new_pos = cc.transform.position;
            //            new_pos.x = (-3 + cc.column) * chess_container_size.x;
            //            float base_y = 0;
            //            switch (cc.column)
            //            {
            //                case 0:
            //                case 6:
            //                    base_y = 1.5f * chess_container_size.y;
            //                    break;
            //                case 1:
            //                case 5:
            //                    base_y = 2f * chess_container_size.y;
            //                    break;
            //                case 2:
            //                case 4:
            //                    base_y = 2.5f * chess_container_size.y;
            //                    break;
            //                case 3:
            //                    base_y = 3f * chess_container_size.y;
            //                    break;
            //                default:
            //                    Debug.Log("神特么，棋盘位置能有0~6以外的列号");
            //                    break;
            //            }
            //            new_pos.y = base_y - cc.row * chess_container_size.y;
            //            cc.transform.localPosition = new_pos;
            //        }
        }
        //在火华编辑模式下生成游戏中场景的方式
        void initMapHH() {
            if (lv_ctrl == null)
                return;
            //棋盘由已有数据转换成字典
            chess_grids = new Dictionary<int, ChessContainer>();
            foreach (var item in FindObjectsOfType<LevelController>()[0].list_grid) {
                chess_grids.Add(item.number,item);
            }
            //重新建立棋盘
            bool b_odd = true;
            Vector2 anchor = new Vector2();
            int size_count = 0;
            foreach (var item in chess_grids) {
                ChessContainer new_grid = item.Value;
                //new_grid.number  从编辑时决定
                //new_grid.row      火华编辑模式将不再使用行列方式
                //new_grid.column
                //new_grid.terrain_type 编辑时决定
                //new_grid.transform.SetParent(chess_border.transform);
                //位置
                //外观
                setGridDisplay(new_grid);

                //关联
                new_grid.MouseDown = MouseDownOnGrid;   //托管点击格子的事件
                new_grid.MouseUp = MouseUpOnGrid;

                //战争迷雾
                int[] players = new int[(int)ePlayer.Max];
                for (int i = 0; i < players.Length; i++)
                    players[i] = i;
                new_grid.FogCover(players);

                //if (levelinfo.map_data.my_grid_t_type[new_grid.number] != eMapGridType.Unvailable)
                //chess_grids_available.Add(new_grid);
            }
        }
        //过时的
        void initMap() {
            if (lv_ctrl == null)
                return;
            //		ClearScene ();

            chess_grids = new Dictionary<int, ChessContainer>();
            //重新建立棋盘
            bool b_odd = true;
            Vector2 anchor = new Vector2();
            int size_count = 0;
            for (int x = 0; x < lv_ctrl.map_data.my_size[0]; x++) {
                for (int y = 0; y < lv_ctrl.map_data.my_size[1]; y++) {
                    ChessContainer new_grid = Instantiate(BKTools.LoadAsset<GameObject>(eResBundle.Prefabs,PrefabPath.ChessGrid)).GetComponent<ChessContainer>();
                    new_grid.number = size_count++;
                    new_grid.row = y;
                    new_grid.column = x;
                    new_grid.terrain_type = lv_ctrl.map_data.my_grid_t_type[new_grid.number];
                    new_grid.transform.SetParent(lv_ctrl.chess_board.transform);
                    //位置
                    //					Vector2 new_pos = new Vector2();
                    //					new_pos.x = anchor.x + chess_container_size.x * x;
                    //					new_pos.y= anchor.y+(b_odd?0:chess_container_size.y/2) +y * chess_container_size.y;
                    Vector3 new_pos = getPosition(anchor, x, y);
                    new_pos.y += b_odd ? 0 : chess_container_size.y / 2;
                    new_grid.transform.localPosition = new_pos;
                    new_grid.transform.localScale = Vector3.one;
                    //外观
                    setGridDisplay(new_grid);

                    //关联
                    new_grid.MouseDown = MouseDownOnGrid;   //托管点击格子的事件
                    new_grid.MouseUp = MouseUpOnGrid;
                    chess_grids.Add(-1,new_grid);

                    //战争迷雾
                    int[] players = new int[(int)ePlayer.Max];
                    for (int i = 0; i < players.Length; i++)
                        players[i] = i;
                    new_grid.FogCover(players);

                    //if (levelinfo.map_data.my_grid_t_type[new_grid.number] != eMapGridType.Unvailable)
                        //chess_grids_available.Add(new_grid);
                }
                b_odd = !b_odd;
            }

            int relation = 0;
            foreach (var item in chess_grids) {
                ChessContainer new_grid = item.Value;
                b_odd = new_grid.number / lv_ctrl.map_data.my_size[1] % 2 == 0;
                /*
                     * 特例
                     * 1、最左列不用和左边关联
                     * 2、最右列不用和右边关联
                     * 3、单双数右上右下不同
                     * 4、上下边界判断
                     * 
                     * 奇数列位置偏低
                     */
                //左上关联
                if (new_grid.column != 0) {
                    relation = new_grid.number - lv_ctrl.map_data.my_size[1];
                    //单双列分别判断
                    if (b_odd) {
                        chess_grids[relation].CCLowerRight = new_grid;
                        new_grid.CCUpperLeft = chess_grids[relation];
                    } else if (new_grid.row < (lv_ctrl.map_data.my_size[1] - 1)) {
                        relation += 1;
                        chess_grids[relation].CCLowerRight = new_grid;
                        new_grid.CCUpperLeft = chess_grids[relation];
                    }
                }
                //左下关联
                if (new_grid.column != 0) {
                    relation = new_grid.number - lv_ctrl.map_data.my_size[1];
                    //单双列分别判断
                    if (!b_odd) {
                        chess_grids[relation].CCUpperRight = new_grid;
                        new_grid.CCLowerLeft = chess_grids[relation];
                    } else if (new_grid.row >= 1) {
                        relation -= 1;
                        chess_grids[relation].CCUpperRight = new_grid;
                        new_grid.CCLowerLeft = chess_grids[relation];
                    }
                }
                //上关联
                if (new_grid.row < lv_ctrl.map_data.my_size[1] - 1) {
                    relation = new_grid.number + 1;
                    chess_grids[relation].CCLower = new_grid;
                    new_grid.CCUpper = chess_grids[relation];
                }
                //下关联
                if (new_grid.row >= 1) {
                    relation = new_grid.number - 1;
                    chess_grids[relation].CCUpper = new_grid;
                    new_grid.CCLower = chess_grids[relation];
                }
                /*右侧关联似乎是多余的，从左往右的话
                //右上关联
                if(new_grid.column!=lvc.map_data.my_size[0]-1){
                    relation = new_grid.number + lvc.map_data.my_size [1];
                    //单双列分别判断
                    if (b_odd) {
                        chess_grids [relation].CCLowerLeft = new_grid;
                        new_grid.CCUpperRight = chess_grids [relation];
                    } else {
                        if (new_grid.row < (lvc.map_data.my_size [1]-1)) {
                            relation += 1;
                            chess_grids [relation].CCLowerLeft = new_grid;
                            new_grid.CCUpperRight = chess_grids [relation];
                        }
                    }
                }
                //右下关联
                if(new_grid.column!=lvc.map_data.my_size[0]-1){
                    relation = new_grid.number + lvc.map_data.my_size [1];
                    //单双列分别判断
                    if (!b_odd) {
                        chess_grids [relation].CCLowerLeft = new_grid;
                        new_grid.CCUpperRight = chess_grids [relation];
                    } else {
                        relation -= 1;
                        if (new_grid.row > 1) {
                            chess_grids [relation].CCLowerRight = new_grid;
                            new_grid.CCUpperRight = chess_grids [relation];
                        }
                    }
                }
                */
            }
            //		ResetUnitView ();
        }
        //匹配地形资源
        void setGridDisplay(ChessContainer grid) {
            grid.GetComponent<SpriteRenderer>().color = LevelMapData.Grid_Color[(int)grid.terrain_type];
        }
        public Vector3 getPosition(Vector3 anchor, int x, int y) {
            Vector3 new_pos = new Vector3();
            new_pos.x = anchor.x + chess_container_size.x * x;
            new_pos.y = anchor.y + y * chess_container_size.y;
            return new_pos;
        }
        IEnumerator GameStart() {
            yield return new WaitForSeconds(0.5f);
            //洗牌
            player_1.deck_cards = initDeck(shuffle(), ePlayer.Player1);
            player_2.deck_cards = initDeck(shuffle(), ePlayer.Player2);
            //发牌
            initHand();

            clearSelectCard();//可以单独修改 select_card
                              //clickCardContainer(cardcont_p1[0].GetComponent<CardContainer>());
                              //生成游戏流程
            GenerateTurnPhase();

            b_game_start = true;
            game_state.Set(eGameState.Start);
            yield return null;
        }
        ///回合对象容器
        GameObject _nodePhaseControler = null;
        /// <summary>
        /// 回合对象Hierarchy容器
        /// </summary>
        public GameObject NodePhaseControler {
            get {
                if (_nodePhaseControler == null) {
                    _nodePhaseControler = GameObject.Find(TurnPhase.PhaseControllerNameInScene);
                    if (_nodePhaseControler == null) {
                        _nodePhaseControler = new GameObject(TurnPhase.PhaseControllerNameInScene);
                    }
                }
                return _nodePhaseControler;
            }
        }

        /// <summary>
        /// 生成游戏最开始的回合
        /// 通常是召唤怪物
        /// </summary>
        public void GenerateTurnPhase() {


            TurnPhase toPhase = Instantiate(BKTools.LoadAsset<GameObject>(eResBundle.Prefabs, PrefabPath.ExplorerPhase),NodePhaseControler.transform).GetComponent<TurnPhase>();
            toPhase.gameObject.SetActive(false);
            explore_phase = toPhase;
            CheckAndInsertPhase(ref now_turnphase, toPhase,eEventTrigger.Phase_Change,null);


            if (now_turnphase != null) {
                //now_turnphase.nextPhaseDefault = toPhase;     //在insert中自动处理
                now_turnphase.gameObject.SetActive(true);

            } else {
                //now_turnphase = Instantiate (toPhase, NodePhaseControler.transform);
                //当没有插入事件时，激活下一个时间
                toPhase.gameObject.SetActive(true);
            }
        }
        /// 在正常的回合流程中插入剧情表演
        /// 在需要插入时，当前阶段phase已经结束
        public void CheckAndInsertPhase(ref TurnPhase nowPhase, TurnPhase toPhase, eEventTrigger event_trigger,Chess chess_argu) {
            DramaPhase last_new_phase = null;
            DramaPhase first_new_phase = null;
            //事件播放（最开始的召唤
            //      if (levelinfo.isNowEventConditionReady (true)) {
            int[] evt_id = lv_ctrl.isNowEventConditionReady(event_trigger, -1, chess_argu);
            foreach (var id in evt_id) {
                DramaPhase phase = Instantiate(BKTools.LoadAsset<GameObject>(eResBundle.Prefabs, PrefabPath.DramaPhase), NodePhaseControler.transform).GetComponent<DramaPhase>();
                DramaScript script = DramaScript.Generate(lv_ctrl, id);
                phase.drama_script = script;
                if (first_new_phase == null)
                    first_new_phase = phase;
                if (last_new_phase != null) {
                    last_new_phase.AppendPhase(phase);
                }
                last_new_phase = phase;
                //事件触发次数最后增加
                lv_ctrl.map_data.list_event_data[id].triggered_times++;
            }

            if (first_new_phase != null) nowPhase = first_new_phase;

            //最后要把原本的接上去
            if (last_new_phase != null)
                last_new_phase.AppendPhase(toPhase);
            //没有插入事件时，tophase直接由外层创建，这边不用处理


            //原本是通过now_turnphase判断是否有插入剧情事件，没有则直接生成默认的下一个阶段。这个处理交给具体的地方
            //if (now_turnphase != null) {
            //    now_turnphase.nextPhaseDefault = next_phase_prefab;
            //} else {
            //    now_turnphase = Instantiate(next_phase_prefab, NodePhaseControler.transform);
            //}
        }
        public void GameOver(ePlayer lusir) {
            b_game_start = false;
            game_state.Set(eGameState.WaitReturn);
            //         player_1.gameObject.SetActive(false);
            //         player_2.gameObject.SetActive(false);
            UI_gameplay.SetActive(false);

            GameObject showgirl;
            if (lusir == ePlayer.Player2) {
                //showgirl = 
                showgirl = win_kuroko;
            } else {
                showgirl = win_shiroi;
            }
            showgirl.GetComponent<Animator>().SetTrigger("win_event");
        }
        // Update is called once per frame
        void Update() {
            if (game_state.Now == eGameState.Uninit)
                return;
            if (game_state.Now == eGameState.WaitReturn) {
                if (Input.anyKeyDown) {
                    SceneLoading.GoLevel = (lv_ctrl.map_data.my_type == eMapType.PvE_Solo)?"PvE_Map":"PvP";
                    UnityEngine.SceneManagement.SceneManager.LoadScene("LoadingToPvE");
                }
            }
            //流程逻辑(转移到了TurnPhase中）
            //		switch(turn_player)
            //		{
            //		case ePlayer.Player1:
            //		case ePlayer.Player2:
            //		case ePlayer.Player3:
            //		case ePlayer.Player4:
            //			PhaseControl ();
            //			break;
            //		case ePlayer.Enemy1:
            //		case ePlayer.Enemy2:
            //			break;
            //		}

            //交互操作
            if (Input.GetMouseButtonUp(0))
                clickUp();
            dragFunction();
            if (Input.GetMouseButtonDown(0))
                //点击对象类型的判断在托管事件中完成了
                ClickDown();


            testFunction();
        }
        //拖拽处理
        void dragFunction() {
            if (!b_is_mousedown)
                return;


            //获得当前拖拽差
            v_drag_now = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (!b_is_drag) {
                if ((v_drag_start - v_drag_now).magnitude > 1) {
                    //发生拖拽时，同样显示操作辅助（类似于单击棋子移动和单击卡牌放置
                    b_is_drag = true;
                    switch (click_target) {
                    case eMouseDownTarget.MapGrid:
                        //拖拽地图
                        break;
                    case eMouseDownTarget.Card:
                        //TODO 拖拽卡牌
                        break;
                    case eMouseDownTarget.Chess:
                        click_target = eMouseDownTarget.None;
                        //TODO 拖拽棋子
                        break;
                    }
                } else {
                    f_mousedown_time += Time.deltaTime;
                    if (f_mousedown_time > 0.25f && click_target == eMouseDownTarget.Chess) {
                        //					Debug.Log ("长按");
                        //					StartMoveChess(moving_chess);
                        now_turnphase.StartMoveChessPlayer(moving_chess);
                        b_is_drag = true;
                    }
                    return;
                }
            }
            switch (click_target) {
            //		case eMouseDownTarget.MapGrid:
            //			//
            //			break;
            case eMouseDownTarget.Card:
                //TODO 拖拽卡牌
                break;
            case eMouseDownTarget.Chess:
                //TODO 拖拽棋子
                break;
            default:
                //拖拽地图
                lv_ctrl.chess_board.transform.position = v_drag_map_start + (v_drag_now - v_drag_start);
                break;
            }
        }
        bool dragEnd() {
            if (!b_is_drag) {
                return false;
            }
            if (false) {
                //TODO 如果拖拽结果停留在界面上，则这次拖拽无效
                //恢复拖拽前，比如地图位置
                lv_ctrl.chess_board.transform.position = v_drag_map_start;
            } else {
                switch (click_target) {
                case eMouseDownTarget.MapGrid:
                    //拖拽地图
                    lv_ctrl.chess_board.transform.position = v_drag_map_start + (v_drag_now - v_drag_start);
                    break;
                case eMouseDownTarget.Card:
                //TODO 拖拽卡牌
                case eMouseDownTarget.Chess:
                    //TODO 拖拽棋子
                    //				if (b_is_drag) {
                    //					Chess.clear_Moveable_Area ();
                    //					b_moving_chess = false;
                    //				}
                    if (b_is_drag) {
                        //拖拽

                        Ray ray = new Ray(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward);
                        //					Plane hPlane = new Plane (Vector3.forward, Vector3.zero);		
                        //					float distance = 0;
                        RaycastHit2D _hit = Physics2D.Raycast(ray.origin, ray.direction);
                        //绕原路获取目标格子
                        ChessContainer grid = null;
                        if (_hit.collider != null)
                            grid = _hit.collider.GetComponent<ChessContainer>();

                        if (grid == null) {
                            click_target = eMouseDownTarget.None;
                            return false;
                        }
                        switch (click_target) {
                        case eMouseDownTarget.Card:
                            now_turnphase.setChessPlayer(select_card.card_id, turn_player, grid);
                            //						setChess(grid);
                            break;
                        case eMouseDownTarget.Chess:
                            now_turnphase.moveChessPlayer(grid);
                            break;
                        case eMouseDownTarget.MapGrid:
                            //并没有事情
                            break;
                        }
                    }
                    break;
                }
            }
            click_target = eMouseDownTarget.None;
            b_is_drag = false;
            //拖拽后，镜头跟踪失效
            Camera.main.GetComponent<ExBaCamera>().CameraStuck();
            return true;
        }
        void ClickDown() {
            b_is_mousedown = true;
            f_mousedown_time = 0;
            b_is_drag = false;
            v_drag_start = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //TODO 拖拽地图
            v_drag_map_start = lv_ctrl.chess_board.transform.position;
            //TODO 拖拽卡牌
            //TODO 拖拽棋子
        }
        public void clickUp() {
            //		Debug.Log ("clickUP");
            b_is_mousedown = false;
            if (dragEnd())
                return;
            if (click_target != eMouseDownTarget.Chess && !b_is_drag)
                //不论发生什么，点击时关闭移动区域显示
                Chess.clear_Moveable_Area();
            //		    foreach (Chess c in list_chess)
            //		    {
            //		        c.clear_Moveable_Area();
            //		        hideUnmovableArea();
            //		    }

            //此处原本有移动棋子和放置棋子的处理，搬迁到MouseUpOnGrid中
            //我又搬回来啦，因为MouseUp事件的锅2017年01月19日00:07:40
            //又搬走啦，dragend里 2017年01月19日00:23:35
        }
        void MouseDownOnGrid(ChessContainer grid) {
            //应当没有功能
            //		Debug.Log ("clickGrid");
            click_target = eMouseDownTarget.MapGrid;
        }
        void MouseUpOnGrid(ChessContainer grid) {
            //		Debug.Log ("MouseUpGrid");
            if (!b_is_drag) {
                //点击
                //点到格子时——1、所点方格，有移动标记
                if (b_moving_chess) {
                    now_turnphase.moveChessPlayer(grid);
                    return; //TODO 不能直接点击另一个棋子开始另一个棋子的移动流程，而必须是取消操作可能引起反感。
                }
                //点到格子时——3、点到冇棋子的，放置棋子
                if (select_card != null)
                    now_turnphase.setChessPlayer(select_card.card_id, turn_player, grid);
                //			setChess(grid);
            }
            /*

            */
        }
        public void testFunction() {
#if UNITY_EDITOR
            //测试功能
            if (Input.GetKey(KeyCode.LeftControl)) {
                if (Input.GetKeyDown(KeyCode.T)) {
                    DamageOnOppoPlayer(10);
                }
                if (Input.GetKeyDown(KeyCode.S)) {
                    //切换技能开关
                    is_taboo_skill = !is_taboo_skill;
                }
                if (Input.GetKeyDown(KeyCode.A)) {
                    string str = "";
                    str += "true && true || true = " + (true && true || true).ToString() + "\n";
                    str += "true && true || false = " + (true && true || false).ToString() + "\n";
                    str += "true && false || true = " + (true && false || true).ToString() + "\n";
                    str += "true && false || false = " + (true && false || false).ToString() + "\n";
                    str += "false && true || true = " + (false && true || true).ToString() + "\n";
                    str += "false && true || false = " + (false && true || false).ToString() + "\n";
                    str += "false && false || true = " + (false && false || true).ToString() + "\n";
                    str += "false && false || false = " + (false && false || false).ToString() + "\n";
                    Debug.Log(str);
                }
            }
            //         if (Input.GetKeyDown(KeyCode.E)) {
            //             Debug.Log((int)eSkillID.Reinhard);
            //             Debug.Log((int)eSkillID.Reuenthal);
            //             Debug.Log((int)eSkillID.Siegfried);
            //             Debug.Log((int)eSkillID.Heinessen);
            //             Debug.Log((int)eSkillID.Teliu);
            //             Debug.Log((int)eSkillID.Diva_Zi);
            //             Debug.Log((int)eSkillID.Diva_Zero);
            //             Debug.Log((int)eSkillID.Diva_Shu);
            //             Debug.Log((int)eSkillID.Diva_Wei);
            //             Debug.Log((int)eSkillID.Diva_Chu);
            //             Debug.Log((int)eSkillID.MahouShoujou_Matoka);
            //             Debug.Log((int)eSkillID.Quan_Pin_Shang_Hai);
            //             Debug.Log((int)eSkillID.MahouShoujou_Homura);
            //             Debug.Log((int)eSkillID.MahouShoujou_Sakura);
            //             Debug.Log((int)eSkillID.MahouShoujou_Sayaka);
            //             Debug.Log((int)eSkillID.MahouShoujou_Mami);
            //         }
            //         if (Input.GetKeyDown(KeyCode.E) && Input.GetKey(KeyCode.LeftAlt)) {
            //             Debug.Log(eSkillID.MahouShoujou_Matoka == eSkillID.Quan_Pin_Shang_Hai);
            //         }
            //Debug.Log(HSVtoRGB(120, 1, 1));

            //Debug.Log(Random.Range(0, 100).ToString("D4"));

            //         if (Input.GetKeyDown(KeyCode.T)) {
            //         } else if (Input.GetKeyDown(KeyCode.K)) {
            //             player_1.setBorderOn();
            //             player_2.setBorderOff();
            //         } else if (Input.GetKeyDown(KeyCode.W)) {
            //             player_2.setBorderOn();
            //             player_1.setBorderOff();
            //         }
#endif
        }
        void PhaseControl() {
            if (!b_phase_trigger)
                return;
            if (!isStageClear())
                return; //等待所有动画播放完毕
                        //stage_dancer = 0;
            b_phase_trigger = false;
            PlayerBoard _turnplayer;
            GameObject[] container;
            //切换界面
            if (turn_player == ePlayer.Player1) {
                _turnplayer = player_1;
                container = cardcont_p1;
                player_1.setBorderOn();
                player_2.setBorderOff();
            } else if (turn_player == ePlayer.Player2) {
                _turnplayer = player_2;
                container = cardcont_p2;
                player_1.setBorderOff();
                player_2.setBorderOn();
            } else {
                Debug.Log("回合开始时发生不明原因，回合进攻玩家不明");
                return;
            }
            switch (now_phase_type) {
            case ePhaseType.Draw:
                clearCardInfo();
                //            TurnStart(turn_player);
                b_attacked = false;
                b_moving_chess = false;
                if (_turnplayer.hand_cards.Count < 5) {
                    if (!_turnplayer.orenotan_draw()) {
                        Debug.Log("牌堆没牌了");
                        //return;
                    }
                    //removeCard((Card)_turnplayer.hand_cards[0]);
                }
                b_setchess = false;
                setHand(_turnplayer.hand_cards, container);
                //clickCardContainer(container[0].GetComponent<CardContainer>());
                b_phase_trigger = true;
                now_phase_type++;
                break;
            case ePhaseType.Prepare:
                Debug.Log("准备阶段");
                //            StartCoroutine(phasePrepare());
                break;
            case ePhaseType.Main1:
                Debug.Log("主流1");
                Action_Chance = 1;
                //now_phase++;
                break;
            case ePhaseType.Battle:
                //                 foreach (Chess c in list_chess) {
                //                     if (c.my_state == eChessState.Deading || c.my_state == eChessState.Overturn) {
                //                         b_phase_trigger = true;
                //                         break;
                //                     }
                //                 }
                if (b_battbe_phase_pincer_over && overturn_list.Count == 0 && Chess.driving_skill_list.Count == 0) {
                    now_phase_type++;
                }
                b_phase_trigger = true;
                break;
            case ePhaseType.Main2:
                addDancer("Main2");
                if (b_attacked) {
                    reduceDancerLater("Main2", 0.5f);
                } else {
                    reduceDancerLater("Main2", 0.25f);
                }
                now_phase_type++;
                //TODO 正式应该点击按钮结束
                b_phase_trigger = true;
                break;
            case ePhaseType.End:
                b_phase_trigger = true;
                now_phase_type = ePhaseType.Draw;
                switch (lv_ctrl.map_data.my_type) {
                case eMapType.PvE_Solo:
                    if (turn_player == ePlayer.Player1) {
                        turn_player = ePlayer.Enemy1;
                    } else {
                        turn_player = ePlayer.Player1;
                    }
                    break;
                case eMapType.PvP_2P:
                    if (turn_player == ePlayer.Player1) {
                        turn_player = ePlayer.Player2;
                    } else {
                        turn_player = ePlayer.Player1;
                    }
                    break;
                }
                TurnEnd_Buff();
                break;
            }
        }
        void TurnEnd_Buff() {
            BuffContrllor.Deal_Effect(this, true, eBuffEvent.Phase_End);
            int buffcount = 0, buff_duration_count = 0;
            foreach (KeyValuePair<long, Chess> data in dic_chess) {
                Chess c = data.Value;
                foreach (Buff item in c.my_buffs) {
                    buffcount++;
                    buff_duration_count += item.my_Duration;
                }
            }
        }
        //夹击连锁
        public void PincerChain() {
            if (overturn_list.Count == 0)
                return;
            if (!isStageClear())
                return;
            //bool attack = false;
            foreach (Chess c in overturn_list) {
                if (c.my_state != eChessState.Overturn)
                    //attack = true;
                    return;
            }
            for (int i = overturn_list.Count - 1; i >= 0; i--) {
                Chess c = (Chess)overturn_list[i];
                if (!BuffContrllor.ContainEffect(c, eBuff_Effect.Attack_BAN))
                    c.CheckPincer(1);
                overturn_list.Remove(c);
            }
        }
        //从手牌去掉一张卡
        public void removeCard(Card c) {
            if (c == null)
                return;
            if (c.owner == ePlayer.Player1) {
                player_1.hand_cards.Remove(c);
            } else {
                player_2.hand_cards.Remove(c);
            }
            Destroy(c.gameObject);
        }
        //除去手牌的选中特效
        void clearOnesideChess(GameObject[] container) {
            foreach (GameObject obj in container) {
                //             GameObject child = obj.transform.GetChild(0).gameObject;
                //             child.GetComponent<Image>()
                obj.GetComponent<CardContainer>().setOutlineOff();
            }

        }
        public void clearSelectCard() {
            clearOnesideChess(cardcont_p1);
            clearOnesideChess(cardcont_p2);
        }
        public bool judgeSelectCard(Card card) {
            if (card == null) {
                return false;
            }
            if (turn_player == card.owner) {
                return true;
            }
            return false;
        }

        public void selectCard(Card card) {
            if (card == null) {
                return;
            }
            if (turn_player == card.owner) {
                select_card = card;
                setCardInfo(card.card_id, null);
            }
            //告诉点击管理，点了张卡
            click_target = eMouseDownTarget.Card;
        }
        //必要的，通过位置选卡(遗忘在历史中吧)
        public void clickCardContainer(CardContainer cc) {
            if (cc.transform.childCount <= 0) {
                Debug.Log("正常情况下你已经输了_" + cc.owner);
                return;
            }
            if (!judgeSelectCard(cc.gameObject.transform.GetChild(0).gameObject.GetComponent<Card>()))
                return;
            clearSelectCard();
            selectCard(cc.setOutlineOn());

        }
        public void MouseDownOnChess(Chess c) {
            //		Debug.Log ("down chess");
            click_target = eMouseDownTarget.Chess;
            moving_chess = c;
        }
        public void MouseUpOnChess(Chess c) {
            //		Debug.Log ("up chess");
            if (!b_is_drag)
                //点到格子时——2、点到有棋子的，显示移动范围
                //			StartMoveChess(c);
                now_turnphase.StartMoveChessPlayer(moving_chess);
            else
                Chess.clear_Moveable_Area();
        }
        public void StartMoveChess(Chess c) {
            if (Action_Chance > 0) {
                Debug.Log("//TODO overlay");
                //当被施加不可移动BUFF时跳出
                if (BuffContrllor.ContainEffect(c, eBuff_Effect.Move_BAN))
                    return;

                setCardInfo(c.attribute.card_id, c);
                if (c.belong != turn_player)
                    return;

                Chess.clear_Moveable_Area();
                c.show_movable_area();
                b_moving_chess = true;
                moving_chess = c;

                showUnmovableArea(turn_player);
            }
        }
        //直接判断卡片
        void MouseDownOnCard(Card c) {
            //		Debug.Log ("click card");
            //单机版检测，确保没有点到别人的卡
            if (!judgeSelectCard(c))
                return;
            clearSelectCard();
            c.setMaskOn();
            //选中卡牌的逻辑
            now_turnphase.SelectCard(c);
            //selectCard(c);


        }
        //避免没有取消操作，造成选了卡牌后不能拖动地图找地方放
        void MouseUpOnCard(Card c) {
            //避免操作丢向丢失（unity的机制，哪里按下弹起也归哪
            if (b_is_drag) return;

            click_target = eMouseDownTarget.None;
        }
        bool checkContainerDownLimit(Chess c) {
            if (c.container.column == 0 || c.container.column == 6)
                if (c.container.row == 3)
                    return false;
            if (c.container.column == 1 || c.container.column == 5)
                if (c.container.row == 4)
                    return false;
            if (c.container.column == 2 || c.container.column == 4)
                if (c.container.row == 5)
                    return false;
            if (c.container.row == 6)
                return false;
            return true;
        }
        // 寻找各个方向的格子
        //本来想写到chesscontainer的方法里，但那样好像每个对象都会多一个数组，虽然不多，但本着两个好处的原则，少修改一个代码
        //久写法的追回请找git
        public ChessContainer getUpperChessContainer(ChessContainer cc) {
            return cc.CCUpper;
        }
        public ChessContainer getLowerChessContainer(ChessContainer cc) {
            return cc.CCLower;
        }
        public ChessContainer getUpperRightChessContainer(ChessContainer cc) {
            return cc.CCUpperRight;
        }
        public ChessContainer getLowerRightChessContainer(ChessContainer cc) {
            return cc.CCLowerRight;
        }
        public ChessContainer getUpperLeftChessContainer(ChessContainer cc) {
            return cc.CCUpperLeft;
        }
        public ChessContainer getLowerLeftChessContainer(ChessContainer cc) {
            return cc.CCLowerLeft;
        }
        // 寻找各个方向的下一个chess
        Chess getUpperChess(Chess c) {
            ChessContainer cc = getUpperChessContainer(c.container);
            if (cc != null) {
                if (cc.my_chess != null)
                    return cc.my_chess;
                else
                    return null;
            }
            return null;
        }
        //不检测列内超标，在调用方法前判断
        Chess getLowerChess(Chess c) {
            ChessContainer cc = getLowerChessContainer(c.container);
            if (cc != null) {
                if (cc.my_chess != null)
                    return cc.my_chess;
                else
                    return null;
            }
            return null;
        }
        Chess getUpperRightChess(Chess c) {
            ChessContainer cc = getUpperRightChessContainer(c.container);
            if (cc == null)
                return null;
            if (cc.my_chess != null)
                return cc.my_chess;
            else
                return null;
        }
        Chess getLowerRightChess(Chess c) {
            ChessContainer cc = getLowerRightChessContainer(c.container);
            if (cc == null)
                return null;
            if (cc.my_chess != null)
                return cc.my_chess;
            else
                return null;
        }
        Chess getUpperLeftChess(Chess c) {
            ChessContainer cc = getUpperLeftChessContainer(c.container);
            if (cc == null)
                return null;
            if (cc.my_chess != null)
                return cc.my_chess;
            else
                return null;
        }
        Chess getLowerLeftChess(Chess c) {
            ChessContainer cc = getLowerLeftChessContainer(c.container);
            int off = 4;
            if (cc == null)
                return null;
            if (cc.my_chess != null)
                return cc.my_chess;
            else
                return null;
        }
        bool strikeJudge(Chess c, ePlayer startteam, ref ArrayList list, ref Chess _partner, ref ArrayList _result_list) {
            if (c.belong == startteam) {
                _partner = c;
                _result_list.AddRange(list);
                return true;
            } else {
                list.Add(c);
            }
            return false;
        }
        // 判断夹击pincer
        bool pincerJudge(Chess c, ePlayer startteam, ref ArrayList list, ref Chess _partner, ref ArrayList _result_list) {
            if (c.belong == startteam) {
                _partner = c;
                _result_list.AddRange(list);
                return true;
            } else {
                list.Add(c);
            }
            return false;
        }
        public bool pincerUpperRight(Chess c, ePlayer startteam, ref ArrayList list, ref Chess _partner, ref ArrayList _result_list, bool isStrike) {
            //        if (c.container.column == 6)
            //            return false || isStrike;
            //        if (c.container.row == 0 && c.container.column > 2)
            //            return false || isStrike;
            if (c.container.CCUpperRight == null)
                return false || isStrike;
            Chess nextC = getUpperRightChess(c);
            if (nextC == null)
                return false;
            if (pincerJudge(nextC, startteam, ref list, ref _partner, ref _result_list)) {
                return true;
            }
            return pincerUpperRight(nextC, startteam, ref list, ref _partner, ref _result_list, isStrike);
        }
        public bool pincerLowerRight(Chess c, ePlayer startteam, ref ArrayList list, ref Chess _partner, ref ArrayList _result_list, bool isStrike) {
            //        if (c.container.column == 6)
            //            return false || isStrike;
            //        if (!checkContainerDownLimit(c) && c.container.column > 2)
            //            return false || isStrike;
            if (c.container.CCLowerRight == null)
                return false || isStrike;
            Chess nextC = getLowerRightChess(c);
            if (nextC == null)
                return false;
            if (pincerJudge(nextC, startteam, ref list, ref _partner, ref _result_list)) {
                return true;
            }
            return pincerLowerRight(nextC, startteam, ref list, ref _partner, ref _result_list, isStrike);
        }
        public bool pincerUpperLeft(Chess c, ePlayer startteam, ref ArrayList list, ref Chess _partner, ref ArrayList _result_list, bool isStrike) {
            //        if (c.container.column == 0)
            //            return false || isStrike;
            //        if (c.container.row == 0 && c.container.column < 4)
            //            return false || isStrike;
            if (c.container.CCUpperLeft == null)
                return false || isStrike;
            Chess nextC = getUpperLeftChess(c);
            if (nextC == null)
                return false;
            if (pincerJudge(nextC, startteam, ref list, ref _partner, ref _result_list)) {
                return true;
            }
            return pincerUpperLeft(nextC, startteam, ref list, ref _partner, ref _result_list, isStrike);
        }
        public bool pincerLowerLeft(Chess c, ePlayer startteam, ref ArrayList list, ref Chess _partner, ref ArrayList _result_list, bool isStrike) {
            //        if (c.container.column == 0)
            //            return false || isStrike;
            //        if (!checkContainerDownLimit(c) && c.container.column < 4)
            //            return false || isStrike;
            if (c.container.CCLowerLeft == null)
                return false || isStrike;
            Chess nextC = getLowerLeftChess(c);
            if (nextC == null)
                return false;
            if (pincerJudge(nextC, startteam, ref list, ref _partner, ref _result_list)) {
                return true;
            }
            return pincerLowerLeft(nextC, startteam, ref list, ref _partner, ref _result_list, isStrike);
        }
        public bool pincerUpper(Chess c, ePlayer startteam, ref ArrayList list, ref Chess _partner, ref ArrayList _result_list, bool isStrike) {
            //        if (c.container.row == 0)
            //            return false || isStrike;
            if (c.container.CCUpper == null)
                return false || isStrike;

            Chess nextC = getUpperChess(c);
            if (nextC == null)
                return false;
            if (pincerJudge(nextC, startteam, ref list, ref _partner, ref _result_list)) {
                //隔着自己人是否攻击
                if (list.Count > 0)
                    return true;
                else
                    return false;
            }
            return pincerUpper(nextC, startteam, ref list, ref _partner, ref _result_list, isStrike);
        }
        public bool pincerLower(Chess c, ePlayer startteam, ref ArrayList list, ref Chess _partner, ref ArrayList _result_list, bool isStrike) {
            //        if (!checkContainerDownLimit(c))
            //            return false || isStrike;
            if (c.container.CCLower == null)
                return false || isStrike;

            Chess nextC = getLowerChess(c);
            if (nextC == null)
                return false;
            if (pincerJudge(nextC, startteam, ref list, ref _partner, ref _result_list)) {
                return true;
            }
            return pincerLower(nextC, startteam, ref list, ref _partner, ref _result_list, isStrike);
        }
        //返回一个长度20的数组，包含0~14和其中不重复的额外5个数字
        public int[] shuffle() {
            int[] deck = new int[20];
            int[] deck_c;
            int _i = 0;
            foreach (KeyValuePair<int, Card_Info> item in Data.Inst.card_data) {
                deck[_i] = item.Key;
                _i++;
                if (_i >= deck.Length) {
                    break;
                }
            }
            deck_c = (int[])deck.Clone();
            for (int i = 0; i < deck.Length - _i; i++) {
                int index = Random.Range(0, _i);
                if (deck_c[index] != -1) {
                    deck[_i + i] = deck_c[index];
                    deck_c[index] = -1;
                } else {
                    if (i >= _i) {
                        deck_c = (int[])deck.Clone();
                    }
                    i--;
                    continue;
                }
            }
            //洗牌
            for (int i = 0; i < 500; i++) {
                int index1 = Random.Range(0, deck.Length);
                int index2 = Random.Range(0, deck.Length);
                int c;
                c = deck[index2];
                deck[index2] = deck[index1];
                deck[index1] = c;
            }
            return deck;
        }
        public static string CardsControllerNameInScene = "CardsController";

        //TODO 卡牌数据应该在服务器，
        public List<Card> initDeck(int[] number, ePlayer owner) {
            GameObject parent = null;
            parent = GameObject.Find(CardsControllerNameInScene);
            if (parent == null)
                parent = new GameObject(CardsControllerNameInScene);

            List<Card> list = new List<Card>();
            for (int i = 0; i < 20; i++) {
                GameObject obj = Instantiate(BKTools.LoadAsset<GameObject>(eResBundle.Prefabs, PrefabPath.NeoCard), parent.transform);
                Card c = obj.GetComponent<Card>();
                c.card_id = number[i];
                c.MouseDown = MouseDownOnCard;
                c.MouseUp = MouseUpOnCard;
                c.init(owner);
                c.img.sprite = Card_Info.dic_id_hand_sprite[c.card_id];
                list.Add(c);
                //临时隐藏操作
                //			c.GetComponent<RectTransform>().SetParent(card_info.rectTransform);
                c.GetComponent<RectTransform>().position = new Vector3(998, 998);   //卡牌大小
            }
            return list;
        }
        //    将手牌添加到界面上
        public void setHand(List<Card> handcards, GameObject[] container) {
            Card c;
            for (int i = 0; i < handcards.Count; i++) {
                c = (Card)handcards[i];
                c.GetComponent<RectTransform>().SetParent(container[i].GetComponent<Image>().rectTransform);
                c.GetComponent<RectTransform>().localPosition = Vector3.zero;
                c.GetComponent<RectTransform>().localScale = Vector3.one;
                //RectTransform rect = container[i].GetComponent<Image>().rectTransform;
                c.GetComponent<RectTransform>().rect.Set(0, 0, 1000, 750);  //卡牌大小
            }
        }
        public void initHand() {
            player_1.hand_cards = new List<Card>();
            player_2.hand_cards = new List<Card>();
            for (int i = 0; i < 4; i++) {

                if (!player_1.orenotan_draw()) {
                    Debug.Log("牌堆没牌了");
                    //return;
                }

                if (!player_2.orenotan_draw()) {
                    Debug.Log("牌堆没牌了");
                    //return;
                }
            }
            //         for (int i = 0; i < 5; i++) {
            //             c = (Card)player_1.hand_cards[i];
            //             c.transform.parent = cardcnt_p1[i].transform;
            //             c.GetComponent<Image>().transform.localPosition = Vector3.zero;
            // 
            // 
            //             c = (Card)player_2.hand_cards[i];
            //             c.transform.parent = cardcnt_p2[i].transform;
            //             c.GetComponent<Image>().transform.localPosition = Vector3.zero;
            //         }
            setHand(player_1.hand_cards, cardcont_p1);
            setHand(player_2.hand_cards, cardcont_p2);
        }
        public void addDancer(string key) {
            if (stage_dancer.ContainsKey(key)) {
                stage_dancer[key]++;
            } else
                stage_dancer.Add(key, 1);
        }
        public void addDancer() {
            addDancer("default");
        }
        public void redDancer(string key) {
            if (stage_dancer.ContainsKey(key)) {
                stage_dancer[key]--;
            } else {
                Debug.Log("删除不存在的舞者，ID：" + key);
                //stage_dancer.Add(key, 1);
            }
        }
        public void redDancer() {
            redDancer("default");
        }
        public bool isStageClear() {
            int sum = 0;
            foreach (KeyValuePair<string, int> item in stage_dancer) {
                sum += item.Value;
            }
            return sum <= 0;
        }
        /// <summary>
        /// 忽略部分表演
        /// </summary>
        /// <returns><c>true</c>, if stage clear was ised, <c>false</c> otherwise.</returns>
        /// <param name="key">Key.</param>
        public bool isStageClear(string[] key) {
            int sum = 0;
            bool skip = false;
            foreach (KeyValuePair<string, int> item in stage_dancer) {
                skip = false;
                //			Debug.Log ("检测演员" + item.Key + ":" + item.Value);
                foreach (var str in key) {
                    if (item.Key == str) {
                        //Debug.Log("忽略");
                        skip = true;
                        break;
                    }
                }
                if (!skip)
                    sum += item.Value;
            }
            return sum <= 0;
        }
        public void reduceDancerLater(string key, float delay) {
            StartCoroutine(reduceDanceCorotine(key, delay));
        }
        public void reduceDancerLater(float delay) {
            reduceDancerLater("default", delay);
        }
        IEnumerator reduceDanceCorotine(string key, float delay) {
            yield return new WaitForSeconds(delay);
            redDancer(key);
            yield return null;
        }

        public void DamageOnOppoPlayer(int damage) {
            PlayerBoard player;
            if (turn_player == ePlayer.Player1) {
                player = player_2;
            } else {
                player = player_1;
            }

            player.onDamage(damage);
        }

        public void DamageOnTurnPlayer(int damage) {
            PlayerBoard player;
            if (turn_player == ePlayer.Player2) {
                player = player_2;
            } else {
                player = player_1;
            }

            player.onDamage(damage);
        }


        //显示卡片/棋子详情
        public void setCardInfo(int number, Chess chess) {
            card_info.gameObject.SetActive(true);

            //card_info_img.sprite = card_data_main_old.card_info_sprites[number];
            card_info_img.sprite = Card_Info.dic_id_card_sprite[number];

            //COST SPD MANA POW VCT
            card_info_cost.text = Data.Inst.card_data[number].cost.ToString();
            card_info_spd.text = Data.Inst.card_data[number].spd.ToString();
            if (chess != null) {
                card_info_mana.text = chess.attribute.mana.ToString();
            } else {
                card_info_mana.text = Data.Inst.card_data[number].mana.ToString();
            }
            card_info_pow.text = Data.Inst.card_data[number].atk.ToString();
            card_info_vct.sprite = Card_Info.dic_vocation_sprite[Data.Inst.card_data[number].vct];
            bool isBlack = false;
            if (chess != null) {
                if (chess.belong == ePlayer.Player1) {
                    isBlack = true;
                }
            } else if (turn_player == ePlayer.Player1) {
                isBlack = true;
            }

            Color themeColor;
            Color contentColor;
            if (isBlack) {
                card_info_cardface.sprite = card_info_cover_b;
                themeColor = card_info_theme_p1_title;
                contentColor = card_info_theme_p1_content;
            } else {
                card_info_cardface.sprite = card_info_cover_w;
                themeColor = card_info_theme_p2_title;
                contentColor = card_info_theme_p2_content;
            }

            Skill_Info[] _skills = new Skill_Info[2];
            if (Data.Inst.skill_data.ContainsKey(Data.Inst.card_data[number].skill01)) {
                _skills[0] = Data.Inst.skill_data[Data.Inst.card_data[number].skill01];
            }
            if (Data.Inst.skill_data.ContainsKey(Data.Inst.card_data[number].skill02)) {
                _skills[1] = Data.Inst.skill_data[Data.Inst.card_data[number].skill02];
            }

            card_info_title_cost.color = contentColor;
            //         card_info_title_skill1.color = themeColor;
            //         card_info_title_skill2.color = themeColor;
            //         card_info_content_skill1.color = contentColor;
            //         card_info_content_skill2.color = contentColor;
            //         card_info_title_skill1.gameObject.SetActive(_skills[0] != null);
            //         card_info_title_skill2.gameObject.SetActive(_skills[1] != null);
            //         card_info_content_skill1.gameObject.SetActive(_skills[0] != null);
            //         card_info_content_skill2.gameObject.SetActive(_skills[1] != null);

            Text[] title = new Text[] { card_info_title_skill1, card_info_title_skill2 };
            Text[] content = new Text[] { card_info_content_skill1, card_info_content_skill2 };
            for (int i = 0; i < 2; i++) {
                if (_skills[i] != null) {
                    title[i].text = Skill_Info.getSkillTypeName(_skills[i].my_Type);
                    content[i].text = _skills[i].describe;
                }
                title[i].color = themeColor;
                content[i].color = contentColor;
                title[i].gameObject.SetActive(_skills[i] != null);
                content[i].gameObject.SetActive(_skills[i] != null);
            }
        }
        public void clearCardInfo() {
            card_info.gameObject.SetActive(false);
        }
        public void showUnmovableArea(ePlayer runner) {
            foreach (var item in chess_grids) {
                ChessContainer cc = item.Value;
                if (cc.isMoveBan(runner)) {
                    GameObject obj = Instantiate(bg_unmovable);
                    obj.transform.SetParent(cc.transform);
                    obj.transform.localPosition = Vector3.zero + new Vector3(0, 0, -0.8f);
                    list_vfx_unmovable.Add(obj);
                }
            }
        }
        public void hideUnmovableArea() {
            foreach (GameObject obj in list_vfx_unmovable) {
                Destroy(obj.gameObject);
            }
            list_vfx_unmovable.Clear();
        }
    }
}