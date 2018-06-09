#define DMG_WAVE
#define CHESS_ID_SOL_1
//#define CHESS_ID_SOL_2
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using BKKZ.POW01;
using BKKZ.POW01.AI;
using System.Collections.Generic;

public enum eChessState {
    Normal = 0, // 总结了一下自己的代码，发现这个状态只是用来表示没有进行另外两个状态的操作
    Deading,    //不开始下一轮连击
    Overturn    //会发动攻击
}
///游戏中不应该访问card_info，需要从attribute中访问实时值
public class ChessAttribute {
    public Chess my_chess = null;
    public Card_Info base_attr = null;
    public int card_id;
    public string Name { get { return base_attr.name; } }
    public int Max_mana { get { return base_attr.mana; } }
    public int mana;
    public int Cost { get { return base_attr.cost; } }
    public int Spd {
        get {
            if (my_chess != null) {
                if (BuffContrllor.ContainEffect(my_chess, eBuff_Effect.Move_BAN))
                    return 0;
            }
            return base_attr.spd;
        }
    }
    public eCard_Vocation Vocation { get { return base_attr.vct; } }
    public int Rare { get { return base_attr.rare; } }
    public int Stk { get { return base_attr.stk; } }
    public int Pow { get { return base_attr.atk; } }
    public Skill_Info[] skill = new Skill_Info[3];
    public int GetSklDmg(Skill_Info skill) {
        if (skill.skill_damage == -1) {
            return Pow;
        } else {
            return skill.skill_damage;
        }
    }
    //	public 


    /*
    //id
    name
    //img
    //cost
    //spd
    //mana
    //ct
    vct
    ria
    stk
    skill01
    skill02
    spellcard
     */
}
[SelectionBase]
[RequireComponent(typeof(PolygonCollider2D))]
public class Chess : CBuffContain {
    public ChessContainer container;
    //序号生成器
    static Dictionary<int, int> ShoukanNumCountByEventID = new Dictionary<int, int>();//哪个事件召唤了几次
    public static int GetSequenceInEvent(int _shoukan_event_id) {
        if (!ShoukanNumCountByEventID.ContainsKey(_shoukan_event_id)) {
            ShoukanNumCountByEventID.Add(_shoukan_event_id, 0);
        }
        return ShoukanNumCountByEventID[_shoukan_event_id]++;
    }
    //public Main Main.Instance;
    //属性
    public bool B_movable {
        get {
            return BuffContrllor.ContainEffect(this, eBuff_Effect.Move_BAN);
        }
    }

    public ChessAttribute attribute = new ChessAttribute();
    public int shoukan_event_sequence = 1;
    long _chess_id = -1;
    //卡片ID+归属+事件+序号
    //方案1 long存储 补6位
    //倒数第6归属        1
    //倒数3、4、5事件     4
    //最后2位是顺序       2   玩家召唤序号默认为0

    ///方案2 
    /// 按位算，int有32位 
    /// 预留05位十进制数卡片ID   需要17位   131072(大关2 小关1 怪物种类2)也可以不那么排嘛
    /// 归属预留8个十进制数      需要3位    还好目前归属就7种
    /// 假设一个事件最多召唤32只   需要5位
    ///                       剩余7 128事件
    public static long genarateChessID(int card_id,int belong,int shoukan_event,int shoukan_event_sequence) {
        long res;
#if CHESS_ID_SOL_1
        res = (long)card_id * 10000000 + belong * 1000000 + shoukan_event * 100 + shoukan_event_sequence;
#if UNITY_EDITOR
        if (shoukan_event >= 10000) {
            Debug.LogError("事件数量超出棋子列表统计范围");
        }
        if (shoukan_event_sequence >= 100) {
            Debug.LogError("单事件召唤数量超出棋子列表统计范围");
        }
        if (card_id >= Math.Pow(10,12)) {
            Debug.Log("卡片ID太长，超出棋子列表统计范围");
        }
#endif
#endif
#if CHESS_ID_SOL_2
                _chess_id = attribute.card_id << 15 + (int)belong << 12 + shoukan_event << 5 + shoukan_event_sequence;
#if UNITY_EDITOR
                if (attribute.card_id >= Math.Pow(2,19)) {
                    Debug.LogError("事件数量超出棋子列表统计范围(最大524288)");
                }
                if (shoukan_event_sequence >= Math.Pow(2,4)) {
                    Debug.LogError("单事件召唤数量超出棋子列表统计范围");
                }
                if (shoukan_event>int.MaxValue/100000) {
                    Debug.Log("卡片ID太长，超出棋子列表统计范围");
                }
#endif
#endif
        return res;
    }
    public long ChessID {
        get {
        if (_chess_id == -1) {
            _chess_id = genarateChessID(attribute.card_id, (int)belong, shoukan_event, shoukan_event_sequence);
        }
        return _chess_id;
    }
    }

    public eChessState my_state = eChessState.Normal;
    //Buff用
    //     public List<Buff> my_buffs = new List<Buff>();
    //     private List<BuffAdder> list_buffs_add = new List<BuffAdder>();
    //     public int index_buff_vfx = 0;
    //     public bool b_vfx_running = false;
    //prefab
    public Sprite img_cover_black;
    public Sprite img_cover_white;
    public Sprite img_bg_black;
    public Sprite img_bg_white;
    public DamageText dam_text;
    public CureText cure_text;

    public GameObject[] hitEffects1;
    public GameObject[] hitEffects2;
    public GameObject[] cureEffects;
    public GameObject[] deadEffects1;
    public GameObject[] overturnEffects1;
    //引用
    public Image img_hp;
    public Image img_vct;
    public float length_img_hp_max = 0.749f;
    public GameObject bg_move;
    //成员
    private static List<GameObject> list_movable_area = new List<GameObject>();

    //流程控制
    public static ArrayList driving_skill_list = new ArrayList();
    //public static bool is_doing_shoukan = false;    //隐患 目前一次只召唤一个，所以只用boolean表示，如果后期有技能一次性召唤多个单位。应该像↑一样，用list
    //召唤技能和攻击改用外部驱动，不用内部检测。且攻击的触发从召唤技能中调用，不需要单独记录
    //战斗追加数据
    ///攻击方向筛选
    public eDirection attack_Direction = eDirection.All;
    public List<TDL_Item> TDL = new List<TDL_Item>();
    //运行中的使用信息
    public int shoukan_event;	//-1是玩家召唤
    public void ShouKanSkillAndPincer(){
        Main.Inst.b_battbe_phase_pincer_over = false;
        StartCoroutine(ShoukanCoroutine());
    }
    public void ShouKanPincer(){
        //上场大杀四方
        if (!BuffContrllor.ContainEffect(this, eBuff_Effect.Attack_BAN))
            CheckPincer(0);
    }
    IEnumerator ShoukanCoroutine() {
        
        foreach (Skill_Info skill in attribute.skill) {
            //上场发动术式
            if (skill != null) {
                if (skill.my_Event == eSkill_Event.Ritual && Check_Skill_Condition(skill)) {
                    SkillStart(skill);
                }
            }
        }
        yield return new WaitUntil(Main.Inst.isStageClear);
        ShouKanPincer();
        yield return null;
    }
    //设置血量
    public void setHP(int newHp) {
        if (newHp < 0) {
            newHp = 0;
        } else if (newHp > attribute.Max_mana) {
            newHp = attribute.Max_mana;
        }
        attribute.mana = newHp;
        Rect rect = img_hp.rectTransform.rect;
        img_hp.rectTransform.sizeDelta = new Vector2(length_img_hp_max * attribute.mana / attribute.Max_mana, rect.height);
    }
    //初始化数据
    public void initData() {
        //老数据
        /*
        int[] data = Main.Instance.card_data_main_old.data[attribute.card_id];
        //COST SPD MANA POW VCT
        attribute.cost = data[0];
        attribute.spd = data[1];
        attribute.mana = data[2];
        attribute.max_mana = data[2];
        attribute.pow = data[3];
         */

        //         attribute.skill = new Skill_Info();
        //         attribute.skill.id = 10001;
        //         attribute.skill.my_Type = eSkill_Type.Ritual;
        //         attribute.skill.my_Event = eSkill_Event.Ritual;
        //         attribute.skill.my_Kouka = eSkill_Kouka.DirectDamage;
        //         attribute.skill.my_TargetBelong = eSkill_TargetBelong.Opponent;
        //         attribute.skill.my_Locator = eSkill_Scope_Locator.Chess_Location;
        //         attribute.skill.my_location = new Point3(0,0,0);
        //         attribute.skill.my_Scope = new eSkill_Scope[]{eSkill_Scope.Circle};
        //         attribute.skill.my_Scope_Depth = 2;

#if UNITY_EDITOR
        if (!Data.Inst.card_data.ContainsKey(attribute.card_id))
            return;
#endif
        //attribute采用属性get，方面计算丢个chess的引用进去计算buff
        attribute.my_chess = this;

        Card_Info my_card_info = Data.Inst.card_data[attribute.card_id];
        attribute.base_attr = my_card_info;
        //        attribute.card_id = my_card_info.id;
        //        attribute.name = my_card_info.name;
        //        attribute.cost = my_card_info.cost;
        //        attribute.spd = my_card_info.spd;
        attribute.mana = my_card_info.mana; //单独属性 = =，直接修改，不需要复杂运算
                                            //        attribute.max_mana = my_card_info.mana;
                                            //        attribute.vocation = my_card_info.vct;
                                            //        attribute.ria = my_card_info.ria;
                                            //        attribute.stk = my_card_info.stk;
                                            //        attribute.pow = my_card_info.ct;
        if (my_card_info.skill01 != -1)
            attribute.skill[0] = Data.Inst.skill_data[my_card_info.skill01];
        if (my_card_info.skill02 != -1)
            attribute.skill[1] = Data.Inst.skill_data[my_card_info.skill02];
        if (my_card_info.spellcard != -1)
            attribute.skill[2] = Data.Inst.skill_data[my_card_info.spellcard];
        setHP(attribute.Max_mana);
    }
    //棋子图片
    public void initImage() {
        Sprite sp_cover;
        Sprite sp_bg;
        if (belong == ePlayer.Player1) {
            sp_cover = img_cover_black;
            sp_bg = img_bg_black;
        } else {
            sp_cover = img_cover_white;
            sp_bg = img_bg_white;
        }
        //封面图
        transform.Find("cover").GetComponent<SpriteRenderer>().sprite = sp_cover;
        //守望先锋图
        transform.Find("bg").GetComponent<SpriteRenderer>().sprite = sp_bg;

#if UNITY_EDITOR
        if (!Card_Info.dic_id_chess_sprite.ContainsKey(attribute.card_id))
            return;
#endif

        //职业图
        img_vct.sprite = Card_Info.dic_vocation_sprite[Data.Inst.card_data[attribute.card_id].vct];
        //棋子图
        transform.Find("charactor_img").GetComponent<SpriteRenderer>().sprite = Card_Info.dic_id_chess_sprite[attribute.card_id];
    }
    // Update is called once per frame
    void Update() {
        if (list_buffs_add.Count > 0) {
            //             BuffAdder[] adds = new BuffAdder[list_buffs_add.Count];
            //             list_buffs_add.CopyTo(adds);
            //             list_buffs_add.Clear();
            //             //异步添加BUFF
            //             foreach (BuffAdder buff_id in adds)
            //             {
            //                 //做法修改后不再需要判断重复
            //     //             if (my_buffs.ContainsKey(buff_id))
            //     //             {
            //     //                 Debug.Log("加buff失败，找不到buff，ID：" + buff_id);
            //     //                 return;
            //     //             } else if (!my_buffs.ContainsKey(buff_id))
            //                 {
            //                     Buff new_buff = new Buff();
            //                     new_buff.stand_side = buff_id.from;
            //                     new_buff.my_buff_info = Data.Inst.buff_data[buff_id.id];
            //                     new_buff.my_Duration = new_buff.my_buff_info.duration;
            //                     my_buffs.Add(new_buff);
            //                     new_buff.owner_ch = this;
            //                     new_buff.owner = this;
            // 
            //                     BuffContrllor.analyseBuff_Effect(new_buff, eBuffEvent.Buff_Add);
            //                 }
            //                 Main.Instance.redDancer("chess_buff_adder");
            //             }
            BuffContrllor.addBuff(ref list_buffs_add, this, this, null, "chess_buff_adder");
        }

        //轮流播放特效
        playNextVFX();

        //依次执行ToDoList，完成AI行动
        DealTDList();
    }
    //解析和执行ToDoList行动序列
    public void DealTDList() {
        if (TDL.Count == 0)
            return;
        if (!Main.Inst.isStageClear())
            return;
        TDL_Item item = TDL[0];
        Main.Inst.addDancer(item.type.ToString());
        switch (item.type) {
        case eTDL_Item_Type.TDL_Attack:
            StartCoroutine(corTDL_Attack(item));
            break;
        case eTDL_Item_Type.TDL_Move:
            StartCoroutine(corTDL_Move(item));
            break;
        case eTDL_Item_Type.TDL_Skill:
            StartCoroutine(corTDL_Skill(item));
            break;
        case eTDL_Item_Type.TDL_Strike:
            StartCoroutine(corTDL_Strike(item));
            break;
        default:
            break;
        }
    }
    IEnumerator corTDL_Attack(TDL_Item item) {
        CheckPincer(0);
        //等待夹击结束
        yield return new WaitUntil(BKTools.ISCWithArgu(item.type.ToString()));
        Main.Inst.redDancer(item.type.ToString());
        TDL.RemoveAt(0);
        yield return null;
    }
    /// <summary>
    /// TDL移动 协线程.
    /// TDL相关参数需要移动目的地ID
    /// </summary>
    /// <returns>The TD l move.</returns>
    /// <param name="item">Item.</param>
    IEnumerator corTDL_Move(TDL_Item item) {
        Main.Inst.now_turnphase.StartMoveChess(this);
        //		Debug.Log ("移动开始" + Time.time);
        yield return new WaitForSeconds(GameRule.Moveing_Duration);
        Main.Inst.now_turnphase.moveChess(this, Main.Inst.chess_grids[item.argu_grid_id], item.argu_trigger_strike);
        //		Debug.Log ("移动" + Time.time);
        //万一有执行强袭就要等待
        yield return new WaitUntil(BKTools.ISCWithArgu(item.type.ToString()));
        //		Debug.Log ("移动结束" + Time.time);
        Main.Inst.redDancer(item.type.ToString());
        TDL.RemoveAt(0);
        yield return null;
    }
    /// <summary>
    /// TDL技能/攻击 协线程.
    /// TDL相关参数需要技能信息、方向限制
    /// </summary>
    IEnumerator corTDL_Skill(TDL_Item item) {
        if (item.argu_Direction != eDirection.None)
            attack_Direction = item.argu_Direction;
        SkillStart(item.argu_skill_info);
        //		Debug.Log ("技能开始" + Time.time);
        yield return new WaitUntil(BKTools.ISCWithArgu(item.type.ToString()));
        //		Debug.Log ("技能结束" + Time.time);
        Main.Inst.redDancer(item.type.ToString());
        TDL.RemoveAt(0);
        yield return null;
    }
    IEnumerator corTDL_Strike(TDL_Item item) {
        CheckStrike(0);
        yield return new WaitUntil(BKTools.ISCWithArgu(item.type.ToString()));
        Main.Inst.redDancer(item.type.ToString());
        TDL.RemoveAt(0);
        yield return null;
    }
    public void CheckStrike(float delay) {
        //保险
        if (belong != Main.Inst.turn_player)
            return;
        //正文
        Main.Inst.b_battbe_phase_pincer_over = false;
        StartCoroutine(CheckLineCoroutine(delay, true));
        my_state = eChessState.Normal;
    }
    //检查夹击
    public void CheckPincer(float delay) {
        //保险
        if (belong != Main.Inst.turn_player)
            return;
        //正文
        Main.Inst.b_battbe_phase_pincer_over = false;
        StartCoroutine(CheckLineCoroutine(delay, false));
        my_state = eChessState.Normal;
    }
    IEnumerator CheckLineCoroutine(float delay, bool isStrike) {
        Main.Inst.addDancer("checkline");
        yield return new WaitForSeconds(delay);
        ArrayList search_list = new ArrayList();
        ArrayList result_list = new ArrayList();
        bool check = false;
        Chess partner = null;
        Main.Inst.b_battbe_phase_pincer_over = true;//偷懒 如果有夹击会自动取消
        for (int i = 0; i < 6; i++) {
            check = Main.Inst.dPincers[i](this, belong, ref search_list, ref partner, ref result_list, isStrike);
            if (isStrike) {
                partner = this;
            }


            if (search_list.Count > 0 && check)
                if (!BuffContrllor.ContainEffect(partner, eBuff_Effect.Attack_BAN))
                    StartCoroutine(play_pincer_attack(search_list, partner));
            search_list.Clear();
            check = false;
        }
        //         if (result_list.Count>0){
        //             StartCoroutine(OppoList(result_list, partner));
        //             yield return new WaitForSeconds(1);
        //         }
        search_list.Clear();
        result_list.Clear();
        //夹击结束标志，必须等待夹击完成
        //Main.Instance.battbe_phase_pincer_over = true;
        Main.Inst.redDancer("checkline");
        yield return null;
    }

    IEnumerator play_pincer_attack(ArrayList _list, Chess partner) {
        Main.Inst.addDancer("play_pincer");
        ArrayList list = new ArrayList();
        list.AddRange(_list);
        if (list.Count <= 0) {
            yield return null;
        }
        //第一波
        Main.Inst.b_battbe_phase_pincer_over = false;
        Main.Inst.b_attacked = true;
        Debug.Log("夹击开始");
        foreach (Chess c in list) {
            //c.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            if (c.belong == Main.Inst.turn_player) {
                continue;
            }
            //FX
            foreach (GameObject obj in hitEffects1) {
                GameObject _o = (GameObject)GameObject.Instantiate(obj, c.transform.position + new Vector3(0, 0, -1f), c.transform.rotation);
                _o.transform.parent = c.transform;
            }
        }
        yield return new WaitForSeconds(0.25f);
        //第二波
        foreach (Chess c in list) {
            //             if (c.controller == Main.Instance.turn_player) {
            //                 continue;
            //             }

            //伤害计算
            int damage = get_attack() + partner.get_attack();
            DamageInfo _info = new DamageInfo(damage);
            _info.setActor(this);
            _info.setPartner(partner);
            damage_and_Display(c, _info);
        }
        Main.Inst.b_battbe_phase_pincer_over = true;
        Main.Inst.redDancer("play_pincer");
        Debug.Log("夹击结束");
        yield return null;
    }
    public void damage_and_Display(Chess c, DamageInfo _info) {
        //chess c (target)
        //FX
        foreach (GameObject obj in hitEffects2) {
            GameObject _o = (GameObject)GameObject.Instantiate(obj, c.transform.position + new Vector3(0, 0, -1f), c.transform.rotation);
            _o.transform.parent = c.transform;
        }
        c.on_damage(_info);
        //伤害跳字
        DamageText dmgtext = (GameObject.Instantiate(dam_text)).GetComponent<DamageText>();
        dmgtext.text.text = _info.dmg.ToString();
        //dmgtext.transform.parent = GameObject.Find("Canvas").transform;
        dmgtext.transform.SetParent(GameObject.Find(Data.MAIN_CANVAS).transform);
        dmgtext.text.transform.position = c.transform.position;
        dmgtext.text.transform.localPosition = new Vector3(dmgtext.text.transform.localPosition.x, dmgtext.text.transform.localPosition.y, 0);
    }
    public void cure_and_Display(Chess c, DamageInfo _info) {
        //chess c (target)
        //FX
        foreach (GameObject obj in cureEffects) {
            GameObject _o = (GameObject)GameObject.Instantiate(obj);
            //_o.transform.parent = c.transform;
            _o.transform.SetParent(c.transform, false);
        }
        c.on_cure(_info);
        //伤害跳字
        CureText dmgtext = (GameObject.Instantiate(cure_text)).GetComponent<CureText>();
        dmgtext.text.text = _info.dmg.ToString();
        //dmgtext.transform.parent = GameObject.Find("Canvas").transform;
        dmgtext.transform.SetParent(GameObject.Find(Data.MAIN_CANVAS).transform);
        dmgtext.text.transform.position = c.transform.position;
        dmgtext.text.transform.localPosition = new Vector3(dmgtext.text.transform.localPosition.x, dmgtext.text.transform.localPosition.y, 0);
    }
    public int get_attack() {
#if DMG_WAVE
        return get_attack_value() + UnityEngine.Random.Range(Data.DAMAGE_WAVE_MIN, Data.DAMAGE_WAVE_MAX);
#else
        return get_attack_value();
#endif
    }
    public int get_attack_value() {
        return attribute.Pow;
    }
    public void on_cure(DamageInfo _info) {
        setHP(attribute.mana + _info.dmg);
    }
    public void on_damage(DamageInfo _info) {
        setHP(attribute.mana - _info.dmg);
        if (attribute.mana == 0) {
            //罗严塔尔技能触发点

            //开始死亡
            DeadInfo dead_info = new DeadInfo();
            dead_info.killer = _info.source_actor;
            dead_info.turn_to = _info.source_actor;
            StartCoroutine(playDead(dead_info));
        }
    }
    IEnumerator playDead(DeadInfo dead_info) {
        Main.Inst.addDancer("playDead");
        DeadLogicBefore(dead_info);
        yield return new WaitForSeconds(0.5f);
        //FX
        foreach (GameObject obj in deadEffects1) {
            GameObject _o = Instantiate(obj, transform.position + new Vector3(0, 0, -1f), transform.rotation);
            _o.transform.parent = transform;
        }
        //FX
        foreach (GameObject obj in overturnEffects1) {
            GameObject _o = Instantiate(obj, transform.position + new Vector3(0, 0, -1f), transform.rotation);
            _o.transform.parent = transform;
        }
        DeadLogicAfter(dead_info);

        Main.Inst.redDancer("playDead");
        yield return null;
    }
    public void DeadLogicBefore(DeadInfo dead_info) {
        belong = dead_info.turn_to.belong;
        BuffContrllor.removeAll(this);
        if (Main.Inst.lv_ctrl.map_data.my_type == eMapType.PvP_2P) {
            Main.Inst.overturn_list.Add(this);
            my_state = eChessState.Deading;
        }
    }
    public void DeadLogicAfter(DeadInfo dead_info) {

        //if (Data.DEAD_ENDING_TURN) {
        //PVP反面，其他都删除【设计】PVE场景中NPC的阵营转变由事件处理
        if(Main.Inst.lv_ctrl.map_data.my_type == eMapType.PvP_2P) { 
            setHP(attribute.Max_mana);
            initImage();
            my_state = eChessState.Overturn;
            if (Main.Inst.lv_ctrl.map_data.my_type == eMapType.PvP_2P)
                Main.Inst.DamageOnOppoPlayer(attribute.Cost);
        } else {
            //坑 或许逻辑有问题
            my_state = eChessState.Deading;
            //container.clearMoveFlag();
            //Destroy(gameObject);
            remove();
        }
    }

    public void show_movable_area() {
        List<ChessContainer> result_list = new List<ChessContainer>();
        //TODO 等BK_Tools中getAroundGrid函数重写后替换
        //container.search_around(ref result_list, 0, attribute.spd);
        result_list.Add(container);
        BKTools.getAroundGrid(attribute.Spd, result_list, true, false, null);
        result_list.Remove(container);

#region 清理不在战斗区域的标记
        List<ChessContainer> not_in_battle_area = new List<ChessContainer>();
        foreach (var item in result_list) {
            if (!Main.Inst.lv_ctrl.AreaBattle.Contains(item.number))
                not_in_battle_area.Add(item);
        }
        foreach (var grid in not_in_battle_area) {
            result_list.Remove(grid);
        }
#endregion

        foreach (ChessContainer cc in result_list) {
            if (cc.isMoveBan(owner))
                continue;
            //可移动区域-面片色块
            GameObject obj = Instantiate(bg_move, cc.transform.position, cc.transform.rotation);
            list_movable_area.Add(obj);
            cc.setMoveFlag_On();
            obj.transform.parent = transform;
            Vector3 new_pos = obj.transform.localPosition;
            new_pos.z = 0.3f;
            obj.transform.localPosition = new_pos;
        }
    }
    public static void clear_Moveable_Area() {
        foreach (GameObject go in list_movable_area) {
            Destroy(go);
        }
    }
    public void remove() {
        Main.Inst.dic_chess.Remove(ChessID);
        Destroy(gameObject);
    }
    //================================================
    //技能相关
    public void SkillStart(Skill_Info skill) {
        StartCoroutine(SkillCoroutine(skill));

    }
    public bool Check_Skill_Condition(Skill_Info skill) {
#if UNITY_EDITOR
        if (Main.Inst.is_taboo_skill) {
            return false;
        }
#endif
        switch (skill.my_Condition) {
        case eSkill_Condition.None:
            return true;
        default:
            return false;
        }
    }
    /// <summary>
    /// Skills the coroutine.
    /// 自动解除攻击方向限制
    /// </summary>
    /// <param name="skill">Skill.</param>
    static string[] _STR_Skill_Coroutine = null;
    static string[] STR_Skill_Coroutine {
        get {
            if (_STR_Skill_Coroutine == null)
                _STR_Skill_Coroutine = new String[] { "skill_coroutine", eTDL_Item_Type.TDL_Skill.ToString() };
            return _STR_Skill_Coroutine;
        }
    }
    IEnumerator SkillCoroutine(Skill_Info skill) {
        Main.Inst.addDancer(STR_Skill_Coroutine[0]);
        driving_skill_list.Add(this);
        //技能发动提示
        Ritual_Display(skill);
        yield return new WaitForSeconds(1);
        //播放表现
        float delay = CG_Display(skill);
        yield return new WaitUntil(BKTools.ISCWithArgu(STR_Skill_Coroutine));

        //ArrayList scope_list = getSkillScope(skill);
        List<ChessContainer> scope_list = BKTools.GetSkillScope(skill, container, attack_Direction);
        //展示攻击范围
        for (int i = scope_list.Count - 1; i >= 0; i--) {
            ChessContainer cc = (ChessContainer)scope_list[i];
            BK_AnimEvts vfx = BKTools.addVFX_Dancer(Main.Inst.GetComponent<VFX_Dictionary>().Skill_Scope_Mark);
            vfx.transform.SetParent(cc.transform, false);
        }
        yield return new WaitUntil(BKTools.ISCWithArgu(STR_Skill_Coroutine));

        //筛选攻击目标
        //有效性筛选
        for (int i = scope_list.Count - 1; i >= 0; i--) {
            ChessContainer cc = (ChessContainer)scope_list[i];
            if (cc.my_chess == null || !BKTools.IsTargetFit(skill.my_TargetBelong, cc.my_chess.belong, belong)) {
                scope_list.RemoveAt(i);
                continue;
            }
        }
        //目标数量有限时，选择合适的目标（大部分时候为随机）
        //info.target_number
        if (skill.target_number != 0) {
            List<ChessContainer> list_copy = new List<ChessContainer>();
            foreach (var item in scope_list) {
                list_copy.Add(item);
            }
            scope_list.Clear();
            for (int i = 0; i < skill.target_number && list_copy.Count > 0; i++) {
                int index = UnityEngine.Random.Range(0, list_copy.Count);
                scope_list.Add(list_copy[index]);
                list_copy.Remove(list_copy[index]);
            }
        }


        //展示攻击目标
        for (int i = scope_list.Count - 1; i >= 0; i--) {
            ChessContainer cc = (ChessContainer)scope_list[i];
            BK_AnimEvts vfx = BKTools.addVFX_Dancer(Main.Inst.GetComponent<VFX_Dictionary>().Skill_Target_Mark);
            vfx.transform.SetParent(cc.transform, false);
        }
        yield return new WaitUntil(BKTools.ISCWithArgu(STR_Skill_Coroutine));

        //计算效果
        Main.Inst.b_attacked = true;
        AnalyseSkillKouKa(skill, scope_list);
        //计算结果后等待
        yield return new WaitForSeconds(Data.DELAY_AFTER_SKILL);
        driving_skill_list.Remove(this);
        attack_Direction = eDirection.All;
        Main.Inst.redDancer(STR_Skill_Coroutine[0]);
        yield return null;
    }
    public void Ritual_Display(Skill_Info skill) {

        switch (skill.my_Type) {
        case eSkill_Type.Ritual:
            //GameObject obj = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/FX/coat_ef"));
            BK_AnimEvts obj = BKTools.addVFX_Dancer(PrefabPath.VFX_Ritual);
            if (obj)
                obj.transform.SetParent(transform, false);
            break;
        default:
            break;
        }
    }
    public float CG_Display(Skill_Info skill) {
        BKTools.addVFX_Dancer(PrefabPath.VFX_CG_Display + skill.cg_display + ".prefab");
        return 1.5f;
        switch (skill.id) {
        case 10001:
            GameObject.Instantiate(Resources.Load("Prefabs/SkillDisplay/Gunfire_ef"));
            return 1.5f;
        case 10004:
            //GameObject.Instantiate(Resources.Load("Prefabs/SkillDisplay/CG_ef_001"));
            BKTools.addVFX_Dancer("Assets/Res/Prefabs/SkillDisplay/CG_ef_001");
            return 1.5f;
        default:
            return 0;
        }
    }
    public void AnalyseSkillKouKa(Skill_Info skill, List<ChessContainer> scope_list) {
        //分析效果
        switch (skill.my_Kouka) {
        case eSkill_Kouka.DirectDamage:
            foreach (ChessContainer cc in scope_list) {
                if (cc.my_chess != null) {
                    Chess chess = cc.my_chess;

                    //伤害计算
                    DamageInfo _info = new DamageInfo(attribute.GetSklDmg(skill));
                    int damage = get_attack();
                    _info.setActor(this);
                    _info.setPartner(null);
                    //伤害表现
                    damage_and_Display(chess, _info);
                }
            }

            break;
        case eSkill_Kouka.Cure:
            foreach (ChessContainer cc in scope_list) {
                if (cc.my_chess != null) {
                    Chess chess = cc.my_chess;
                    //伤害计算
                    DamageInfo _info = new DamageInfo(attribute.GetSklDmg(skill));
                    int damage = get_attack();
                    _info.setActor(this);
                    _info.setPartner(null);
                    //伤害表现
                    cure_and_Display(chess, _info);
                }
            }
            break;
        case eSkill_Kouka.Buff:
            foreach (ChessContainer cc in scope_list) {
                if (cc.my_chess != null) {
                    foreach (int buff_id in skill.my_buffs) {
                        cc.my_chess.AddBuff(new BuffAdder(belong, buff_id));
                    }
                }
            }
            break;
        default:
            Debug.Log("错误 08021337 ，技能效果类型未实现。技能ID:" + skill.id);
            break;
        }
    }

    override public void AddBuff(BuffAdder adder) {
        //异步添加
        list_buffs_add.Add(adder);
        Main.Inst.addDancer("chess_buff_adder");
    }


    //↑技能===================================================
    //↓BuffContain接口实现====================================
    //    public void AddBuff(BuffAdder buff_adder)
    //    {
    //        //需要异步添加
    //        list_buffs_add.Add(buff_adder);
    //        Main.Instance.addDancer("chess_buff_adder");
    //    }
    // //     public void RemoveBuff(int buff_id)
    // //     {
    // //         if (my_buffs.ContainsKey(buff_id))
    // //         {
    // //             my_buffs.Remove(buff_id);
    // //         } else
    // //         {
    // //             Debug.Log("删除BUFF失败。找不到buff，ID：" + buff_id);
    // //         }
    // //     }
    //     public List<Buff> getBuffList()
    //     {
    //         return my_buffs;
    //     }
    //     public eBuffContainerType getContainerType()
    //     {
    //         return eBuffContainerType.Chess;
    //     }
    //     public Main Main.Instance { get { return Main.Instance; } }
    public override ChessContainer getContainer() {
        return container;
    }
    public Action<Chess> MouseDown;
    public Action<Chess> MouseUp;
    void OnMouseDown() {
        //		Debug.Log ("click");
        MouseDown(this);
    }
    void OnMouseUp() {
        MouseUp(this);
    }

    //占用特定位置时，被执行踢开
    public void KickAway() {
        ChessContainer toGrid = null;
        for (int i = 0; i < 10; i++) {
            foreach (var getGrid in Main.Inst.dGetChessContainer) {
                toGrid = getGrid(container);
                //TODO 有可能提到活动区域外，再说
                if (toGrid != null && toGrid.my_chess == null)
                    break;
            }
            //			if(container.CCUpperLeft.my_chess == null)
        }

        if (toGrid != null && toGrid.my_chess == null) {
            container.removeChess();
            toGrid.appendChess(this);
        }
    }
    //     public GameObject getGameObject()
    //     {
    //         return gameObject;
    //     }
    //     public void playNextVFX()
    //     {
    //         if (index_buff_vfx>=my_buffs.Count) 
    //         {
    //             index_buff_vfx = 0;
    //         }
    //         GameObject obj = BKTools.addVFX(my_buffs[index_buff_vfx].my_buff_info.duration_vfx);
    //         if (obj)
    //         {
    //             obj.transform.SetParent(transform, true);
    //             my_buffs[index_buff_vfx].vfx_duration.Add(obj);
    //             obj.transform.localPosition = Vector3.forward * 0.2f;
    //             BuffAnimator ba = obj.GetComponent<BuffAnimator>();
    //             ba.owner = this;
    //             b_vfx_running = true;
    //         }
    //         index_buff_vfx++;
    //     }
}