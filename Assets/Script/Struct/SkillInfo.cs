using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BKKZ.POW01;
public enum eCSV_Skill {
    ID,
    Name,
    Type,	//技能类型 0：术式 1：纹章 2：咒术 3：魔法 4：荣耀
    描述,
    COD,	//触发条件 0：无条件 1：有条件
    EVT,	//触发事件 0：上场时
    STT,	//起点定位 0：以骑士自身为起点 1：以固定位置为起点 2：以相对位置为起点
    UPS,	//定位参数 若起点类型为1，则在此直接填入固定坐标
    EXS,	//技能释放方向  0：释放者周围  1：骑士上方  2：骑士下方  3：骑士右上  4：骑士左下  5：骑士左上  6：骑士右下  7：正三角  8：倒三角  9：固定坐标  10：骑士左侧  11：骑士右侧  12：骑士正上  13：骑士正下
    SCO,	//范围格子数目  例如为1时，对于以骑士周围为方向的技能范围为骑士四周一圈，对于方向为骑士左方的技能，范围为骑士左侧一格
    TAGnum,	//此技能最大攻击目标数量  0：满足条件的全体  1~10：在满足条件的单位中选择对应数量的目标
    TAG,	//目标选择 0：随机（自动） 1：玩家选择
    TAGtype,//目标类型 0：敌方骑士 1：我方骑士 2：敌我不分
    DAMtype,//效果类型 0：直接伤害型 1：恢复型 2：buff型 3：护盾型 4：光环型
    DAM,	//技能所造成的伤害
    BuffId, //buff效果
    CG_Display,//CG展示

    Max
}
//技能事件、条件、效果原型ID
public enum eSkill_ID {
    Reinhard = 0,       //	0	莱因哈特                    致死免疫
    Reuenthal,          //	1	罗严塔尔
    Siegfried,          //	2	吉格飞(Siegfried Kircheis)
    Heinessen,          //	3	海尼森
    Teliu,              //	4	特留你希特
    Diva_Zi,            //	5	宇宙歌姬·子
    Diva_Shu,           //	6	宇宙歌姬·书
    Diva_Zero,          //	7	宇宙歌姬·零
    Diva_Wei,           //	8	宇宙歌姬·惟
    Diva_Chu,           //	9	宇宙歌姬·楚
    MahouShoujou_Matoka,//	10	圆
    MahouShoujou_Homura,//	11	焰
    MahouShoujou_Sakura,//	12	杏子
    MahouShoujou_Sayaka,//	13	沙耶香
    MahouShoujou_Mami,  //	14	麻美
}
public enum eSkill_Type {
    Ritual = 0,         //  术式
    Emblem,             //  纹章
    Curse,              //  诅咒
    Magic,              //  魔法
    Honour,             //  荣耀
}
public enum eSkill_Event {
    Ritual = 0,         //  术式，上场发动
    Emblem,             //  纹章，满足条件触发
    Curse,              //  诅咒，翻转时发动的技能
    Honour,             //  荣耀，王化时激活的技能
}
public enum eSkill_Condition {
    None = 0,               //  无条件释放
    Max,
}
public enum eSkill_Kouka {
    DirectDamage = 0,       //  直接伤害
    Cure,                   //1：恢复型
    Buff,                   //2：buff型
    Shield,                 //3：护盾型 
    Halo,                   //4：光环型 
}
/* 
 * 技能攻击区域的具体位置根据 
 *  1、类型
 *  2、大小（当类型是固定ID时，这里记录多个格子ID）
 *  3、起点
 *  4、打击范围有效性(80随机）
 *  （相关）打击次数
 *  一个技能具体攻击区域可能有多条数据共同定位
 */
///技能范围类型
///以此为遍历标准，缺少0的情况自行+-1计算
public enum eSkill_Scope {
    Circle = 0,           //  一圈
    LeftUp,             //  左上
    RightDown,          //  右下
    Up,                 //  上
    Down,               //  下
    RightUp,            //  右上
    LeftDown,           //  左下
    //Triangle,           //  正三角，上尖    不要了，用过上述7种来运算
    //Invert_Triangle,    //  倒三角，下尖
    Fixed_Position,     //  固定位置
    //尼玛，哪来的正左正右
}
//技能范围起点
public enum eSkill_Scope_Locator {
    Chess_Location = 0, //  棋子位置
    Board_Location,     //  棋盘位置
    Related_Location,   //  相对位置
}
//目标势力
public enum eSkill_TargetBelong {
    Opponent = 0,   //对手
    Teammate,       //1：队友
    Both_Player,    //2：打双方玩家
    地面我方,       //3：地面-我方
    地面敌方,       //4：地面-敌方
    地面中立,       //5：地面-中立
    Scene,          //6：场景中的中立物品
    None,           //7: 无人深空
    All,            //全都打
}
//目标指定方式
public enum eSkill_Target_SelectSolution {
    Auto = 0,   //自动
    Human,  //手动
}
public class Skill_Info : ILocation_Scope {

    //public bool isReuenthal;//致死免疫    原型——奥斯卡·冯·罗严塔尔
    //     public Dictionary<eSkillID, String> dic_Skill_Event = new Dictionary<eSkillID, String>();       //会直接放置到各个重复逻辑中，文字仅作描述
    //     public Dictionary<eSkillID, String> dic_Skill_Condition = new Dictionary<eSkillID, String>();   //常规条件，Event发生时，进一步判断
    //     public Dictionary<eSkillID, String> dic_Skill_Kouka = new Dictionary<eSkillID, String>();       //效果

    public int id;                                             // ID
    public string name;                                        // Name
    public eSkill_Type my_Type;                                // Type
    public string describe;                                    // 描述
    public eSkill_Condition my_Condition;                      // COD
    //表格中的隐藏列，没什么卵用                               	   // COR
    public eSkill_Event my_Event;                              // EVT
    public eSkill_Scope_Locator my_Locator;                    // STT
    public Point3 my_location = new Point3();                  // UPS
    public eSkill_Scope[] my_Scope;                            // EXS
    public int my_Scope_Depth;                                 // SCO
    public int target_number;                                  // TAGnum
    public eSkill_Target_SelectSolution my_select_Solution;    // TAG
    public eSkill_TargetBelong my_TargetBelong;                // TAGtype
    public eSkill_Kouka my_Kouka;                              // DAMtype
    public int skill_damage;                                   // DAM
    public int[] my_buffs;                                     // Buffs
    public string cg_display;                               //上场CG
    //实现Ilocation_Scope
    public eSkill_Scope[] iLS_Scope {
        get {
            return my_Scope;
        }
    }
    public eSkill_Scope_Locator iLS_Locater {
        get {
            return my_Locator;
        }
    }
    public Point3 iLS_Point {
        get {
            return my_location;
        }
    }
    public int iLS_Depth {
        get {
            return my_Scope_Depth;
        }
    }

    public static string getSkillTypeName(eSkill_Type type){
        switch(type){
            case eSkill_Type.Curse:
                return "咒术";
            case eSkill_Type.Emblem:
                return "纹章";
            case eSkill_Type.Honour:
                return "荣耀";
            case eSkill_Type.Magic:
                return "膜法";
            case eSkill_Type.Ritual:
                return "术式";
            default:
                Debug.Log("获取技能类型名称时，参数错误。参数：" + type.ToString());
                return "ERROR";
        }
    }
}