using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BKKZ.POW01
{
    public enum eBuffContainerType
    {
        Error = 0,
        Chess,
        ChessContainer,

        Max
    }
    public abstract class CBuffContain :MonoBehaviour,IBuffContainer
    {
        public ePlayer belong = ePlayer.None;
        public ePlayer owner = ePlayer.None;
		public int click_id = -1;//作为关卡点击事件的触发参数
        public bool b_vfx_running = false;
        public List<Buff> my_buffs = new List<Buff>();
        protected List<BuffAdder> list_buffs_add = new List<BuffAdder>();
        public int index_buff_vfx = 0;

        public List<GameObject> list_vfx = new List<GameObject>();

		public void Start(){
		}

        public void playNextVFX()
        {
            if (b_vfx_running || my_buffs.Count==0)
                return;
            if (index_buff_vfx >= my_buffs.Count)
            {
                index_buff_vfx = 0;
            }
            if (my_buffs[index_buff_vfx].my_buff_info.effect == eBuff_Effect.Halo)
            {
                index_buff_vfx++;
                return;
            }
            GameObject obj = BKTools.addVFX(PrefabPath.VFX_Buff_Prefix + my_buffs[index_buff_vfx].my_buff_info.duration_vfx + ".prefab");
            if (obj)
            {
                obj.transform.SetParent(transform, true);
                //my_buffs[index_buff_vfx].vfx_duration.Add(obj);   暂时仅对halo有效
                obj.transform.localPosition = Buff_Info.vfx_off_buff;
                BuffAnimator ba = obj.GetComponent<BuffAnimator>();
                ba.owner = this;
                Debug.Log("增加" + belong.ToString() + ":" + ba.gameObject.name);
                b_vfx_running = true;
            }
            index_buff_vfx++;
        }


        virtual public void AddBuff(BuffAdder adder)
        {
            //异步添加
            list_buffs_add.Add(adder);
            Main.Inst.addDancer("cc_buff_adder");
        }
        //     public void RemoveBuff(int buff_id) {
        //         if (my_buffs.ContainsKey(buff_id)) {
        //             my_buffs.Remove(buff_id);
        //         }
        //     }
        public eBuffContainerType getContainerType()
        {
            return eBuffContainerType.ChessContainer;
        }
        public abstract ChessContainer getContainer();
        public List<Buff> getBuffList()
        {
            return my_buffs;
        }
        public GameObject getGameObject()
        {
            return gameObject;
        }
    }
    //八福拥有者
    public interface IBuffContainer
    {
        ChessContainer getContainer();
        void AddBuff(BuffAdder buff_adder);
        List<Buff> getBuffList();
        eBuffContainerType getContainerType();
        GameObject getGameObject();
    }
    public class BuffAdder
    {
        public ePlayer from;
        public int id;
        public BuffAdder(ePlayer _from, int _id)
        {
            from = _from;
            id = _id;
        }
    }
    public class Buff : ILocation_Scope
    {
        public Chess owner_ch;
        public ChessContainer owner_chc;
        public IBuffContainer owner;
        public ePlayer stand_side = ePlayer.None;  //站边，这个buff由哪一方发动
        public int my_Duration;
        public List<GameObject> vfx_duration =  new List<GameObject>();
        public Buff_Info my_buff_info;

        //实现Ilocation_Scope
        public eSkill_Scope[] iLS_Scope
        {
            get
            {
                return my_buff_info.my_Scope;
            }
        }
        public eSkill_Scope_Locator iLS_Locater
        {
            get
            {
                return my_buff_info.my_Locator;
            }
        }
        public Point3 iLS_Point
        {
            get
            {
                return my_buff_info.my_location;
            }
        }
        public int iLS_Depth
        {
            get
            {
                return my_buff_info.my_Scope_Depth;
            }
        }
    }
    public class BuffContrllor
    {
        public static void Deal_Effect(Main main, bool run_delete, eBuffEvent buff_event)
        {
            foreach (KeyValuePair<long,Chess> data in Main.Inst.dic_chess) {
				Chess c = data.Value;
                dealBuff(c, run_delete, buff_event);
            }
            foreach (var item in main.chess_grids)
            {
                dealBuff(item.Value, run_delete, buff_event);
            }
        }
        /* 单个效果的触发 可以直接通过analyse执行效果
         * 这里主要提供群体触发，机会回合切换等时间。（加入有什么效果的事件是其他技能发动时，也可以加，呜噗噗噗噗噗）
         */
        public static void dealBuff(IBuffContainer container, bool run_delete, eBuffEvent buff_event)
        {
            List<Buff> dead_list = new List<Buff>();   //删除标记
            List<Buff> dick = container.getBuffList();
            foreach (Buff buff in dick)
            {
                analyseBuff_Effect(buff, buff_event);


                // 删除检查&标记
                if (run_delete)
                {
                    buff.my_Duration--;
                    if (buff.my_Duration <= 0)
                    {
                        dead_list.Add(buff);
                        analyseBuff_Effect(buff, eBuffEvent.Buff_Remove);

                        // TODO 删除特效
                        for (int i = buff.vfx_duration.Count - 1; i >= 0; i-- )
                        {
                            UnityEngine.Object.Destroy(buff.vfx_duration[i]);
                        }
                    }
                }
            }

            //执行删除
            foreach (Buff i in dead_list)
            {
                if (container.getBuffList().Contains(i))
                    container.getBuffList().Remove(i);
            }
        }
        /*已经通过接口实现*/
        // 更新为加载事件统一处理
        public static void addBuff(ref List<BuffAdder> list_adder, CBuffContain container,Chess ch,ChessContainer chc,string dancer_key)
        {
//             {
//                 Buff new_buff = new Buff();
//                 //new_buff.my_buff_info.id = buff_id;
//                 new_buff.my_buff_info = Data.Inst.buff_data[buff_id];
//                 c.my_buffs.Add(new_buff);
//             }
            BuffAdder[] adds = new BuffAdder[list_adder.Count];
            list_adder.CopyTo(adds);
            list_adder.Clear();

            List<Buff> my_buffs = container.getBuffList();
            //异步添加BUFF
            foreach (BuffAdder buff_id in adds)
            {
                //做法修改后不再需要判断重复
                //             if (my_buffs.ContainsKey(buff_id))
                //             {
                //                 Debug.Log("加buff失败，找不到buff，ID：" + buff_id);
                //                 return;
                //             } else if (!my_buffs.ContainsKey(buff_id))
                {
                    Buff new_buff = new Buff();
                    new_buff.stand_side = buff_id.from;
                    new_buff.my_buff_info = Data.Inst.buff_data[buff_id.id];
                    new_buff.my_Duration = new_buff.my_buff_info.duration;
                    my_buffs.Add(new_buff);
                    new_buff.owner_ch = ch;
                    new_buff.owner_chc = chc;
                    new_buff.owner = container;

                    BuffContrllor.analyseBuff_Effect(new_buff, eBuffEvent.Buff_Add);
                    BK_AnimEvts av = BKTools.addVFX_Dancer(new_buff.my_buff_info.start_vfx);
                    if(av)
                        av.transform.position = container.getContainer().transform.position;
                    // 持续特效
					if (new_buff.my_buff_info.effect == eBuff_Effect.Halo) {
						GameObject obj = BKTools.addVFX (PrefabPath.VFX_Buff_Prefix + new_buff.my_buff_info.duration_vfx + ".prefab");
						if (obj) {
							obj.transform.SetParent (container.transform, true);
							new_buff.vfx_duration.Add (obj);   //暂时仅对halo有效
							obj.transform.localPosition = Vector3.forward * 0.2f;
						}
					} else {
						container.playNextVFX ();
					}
                }
                Main.Inst.redDancer(dancer_key);
            }
        }
        public static void analyseBuff_Effect(Buff buff, eBuffEvent buff_event)
        {

            if (buff.my_buff_info.my_event != buff_event)
            {
                if (buff.my_buff_info.my_event == eBuffEvent.Buff_Exist
                    && (buff_event == eBuffEvent.Buff_Add
                        || buff_event == eBuffEvent.Buff_Remove)
                    )
                {
                    //如果是持续效果，则在增加和删除时触发效果
                } else if (buff.my_buff_info.my_event == eBuffEvent.Phase_Prepare
                    && buff_event == eBuffEvent.Buff_Add) 
                {
                    //如果是每回合造成一次效果的，会在添加时触发效果
                } else
                {
                    return;
                }
            }
            // continue
            switch (buff.my_buff_info.effect)
            {
                case eBuff_Effect.DOT:
                    if (buff.owner_ch != null)
                    {
                        DamageInfo new_dmg = new DamageInfo(buff.my_buff_info.values[0]);
                        buff.owner_ch.on_damage(new_dmg);
                    }
                    break;
                case eBuff_Effect.Move_BAN:
                    //无
                    //经过无数坑踩过后，决定这边在添加时做逻辑处理，仅显示表现
//                     if (buff.owner_chc != null && (buff_event == eBuffEvent.Buff_Add/* || buff_event == eBuffEvent.Phase_Prepare*/))
//                     {
//                         BK_AnimEvts av = BKTools.addVFX(buff.owner_chc.Main.Instance.bg_unmovable_start);
//                         av.transform.position = buff.owner_chc.transform.position + new Vector3(0, 0, -0.8f);
//                     }
//                     if (buff.owner_chc != null)
//                     {
//                         if (buff_event == eBuffEvent.Buff_Add)
//                         {
//                             switch (buff.my_buff_info.my_TargetBelong)
//                             {
//                                 case eSkill_TargetBelong.Both_Player:
//                                     buff.owner_chc.setMoveBan(ePlayer.Max);
//                                     break;
//                                 case eSkill_TargetBelong.Opponent:
//                                     ePlayer target = ePlayer.Player1;
//                                     if (buff.stand_side == ePlayer.Player1) 
//                                     {
//                                         target = ePlayer.Player2;
//                                     }
//                                     buff.owner_chc.setMoveBan(target);
//                                     break;
//                                 case eSkill_TargetBelong.Teammate:
//                                     buff.owner_chc.setMoveBan(ePlayer.Max);
//                                     break;
//                                 default:
//                                     break;
//                             }
//                             
//                         } else if (buff_event == eBuffEvent.Buff_Remove)
//                         {
//                             buff.owner_chc.setMoveBan(ePlayer.None);
//                         }
//                     }
                    break;
                case eBuff_Effect.Attack_BAN:
                    break;
                case eBuff_Effect.Halo:
				List<ChessContainer> list = BKTools.GetSkillScope(buff, buff.owner.getContainer());
                    foreach (ChessContainer cc in list) 
                    {
                        if (cc.my_chess!=null) 
                        {
                            //检查目标类型是否符合
                            if (BKTools.IsTargetFit(buff.my_buff_info.my_TargetBelong, cc.my_chess.belong, buff.stand_side))
                            {
                                foreach (int id in buff.my_buff_info.values)
                                {
                                    cc.my_chess.AddBuff(new BuffAdder(buff.stand_side, id));
                                    Debug.Log("哎呀~~~又加了一个 card_id:" + cc.my_chess.attribute.card_id);
                                }
                            }
                        }
                        if ((int)buff.my_buff_info.my_TargetBelong > 2)    // 3、4、5地面效果
                        {
                            foreach (int id in buff.my_buff_info.values)
                            {
                                cc.AddBuff(new BuffAdder(buff.stand_side, id));
                                Debug.Log("哎呀~~~又加了一个格子_id:" + cc.number);
                            }
//                                 switch (buff.my_buff_info.my_TargetBelong)
//                                 {
//                                     case eSkill_TargetBelong.地面中立:
//                                         cc.B_unmovable = true;
//                                         break;
//                                     default:
//                                         Debug.Log("warning 填写的BUFF 目标类型尚未实现，填你妈逼。  buff id : " + buff.my_buff_info.id);
//                                         break;
//                                 }

                        }
                    }
                    break;
                default:
                    Debug.Log("buff效果ID错误。buff id-" + buff.my_buff_info.id);
                    break;
            }
        }

        //检查是否含有特定效果
        public static bool ContainEffect(IBuffContainer c, eBuff_Effect id)
        {

            foreach (Buff item in c.getBuffList())
            {
                if (item.my_buff_info.effect == id)
                {
                    return true;
                }
            }
            return false;
        }
        public static List<Buff> getBuff_by_ID(IBuffContainer c, eBuff_Effect id)
        {
            List<Buff> res = new List<Buff>();
            foreach (Buff item in c.getBuffList())
            {
                if (item.my_buff_info.effect == id)
                {
                    res.Add(item);
                }
            }
            return res;
        }
        public static void removeAll(CBuffContain c)
        {
            c.my_buffs.Clear();
        }
    }
}