using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BKKZ.POW01
{
    public enum eCSV_Buff
    {
        ID = 0,
        Name,
        说明,
        事件,
        Effect,
        Value,		//效果数值-半角分号分割多项数值
        //伤害性效果时就是伤害值
        //叠加BUFF的效果就是BUFF_ID
        Round,
        起点,		//起点定位 0：以骑士自身为起点 1：以固定位置为起点 2：以相对位置为起点
        起点参数,	//定位参数 若起点类型为1，则在此直接填入固定坐标
        范围图形,	//技能释放方向  0：释放者周围  1：骑士上方  2：骑士下方  3：骑士右上  4：骑士左下  5：骑士左上  6：骑士右下  7：正三角  8：倒三角  9：固定坐标  10：骑士左侧  11：骑士右侧  12：骑士正上  13：骑士正下
        范围大小,	//范围格子数目  例如为1时，对于以骑士周围为方向的技能范围为骑士四周一圈，对于方向为骑士左方的技能，范围为骑士左侧一格
        目标类型,   //形式和技能一样
        开始特效,
        持续特效,
        Max
    }
    public enum eBuffDamageType
    {
        DMG = 0,    //：伤害型
        Cure = 1,   //：恢复型
        None = 2,   //：均不是
    }
    public enum eBuff_Effect
    {
        None = 0,
        DOT,
        Move_BAN = 100,     //禁止移动
        Attack_BAN = 101,   //禁止攻击
        Halo = 200,     //光环，每次准备阶段给范围内目标加BUFF
        //AreaMoveBAN,        //区域封锁
    }
    public enum eBuffEvent
    {
        Phase_Draw = 0,
        Phase_Prepare,  //不出意外，每回合的效果计算从这里开始，所以回合性效果，持续时间以这个来定
        Phase_Main1,
        Phase_Battle,
        Phase_Main2,
        Phase_End,
        Buff_Add,   //添加Buff时
        Buff_Remove,//删除Buff时
        Buff_Exist, //Buff存在既有效，开始和删除时各执行一次效果（add,remove)
    }
    public class Buff_Info
    {
        public static string vfx_buff_dir = "Prefabs/FX/Buff/";
        public static Vector3 vfx_off_buff = Vector3.zero;//new Vector3(1f, 0.6f, 0);
        public int id;
        public string name;
        public string describe;
        public eBuffEvent my_event;
        public eBuff_Effect effect = eBuff_Effect.None;
        public int[] values;    //
        public int duration;
        public eSkill_Scope_Locator my_Locator;                    // STT
        public Point3 my_location = new Point3();                  // UPS
        public eSkill_Scope[] my_Scope;                            // EXS
        public int my_Scope_Depth;                                 // SCO
        public eSkill_TargetBelong my_TargetBelong;                // 目标类型
        public string start_vfx;
        public string duration_vfx;
    }
}