using UnityEngine;
using System.IO;
using System;
using System.Collections;
using BKKZ.POW01;
public class ReadText : MonoBehaviour {
    private ArrayList m_aArray; // 文本中每行的内容
    /*
     * path：读取文件的路径
     * name：读取文件的名称
     */
    ArrayList fnLoadFile(string sPath, string sName) {
        StreamReader t_sStreamReader = null; // 使用流的形式读取
        //try
        //{
        t_sStreamReader = File.OpenText(sPath + "//" + sName);
        //}
        //catch (Exception ex)
        //{
        //    return null;
        //}
        string t_sLine; // 每行的内容
        ArrayList t_aArrayList = new ArrayList(); // 容器
        while ((t_sLine = t_sStreamReader.ReadLine()) != null) {
            t_aArrayList.Add(t_sLine); // 将每一行的内容存入数组链表容器中
        }
        t_sStreamReader.Close(); // 关闭流

        t_sStreamReader.Dispose(); // 销毁流

        return t_aArrayList; // 将数组链表容器返回
    }
    static ArrayList fnLoadFile(eResBundle eID,string path){

        //BKTools.Assetbundle_path + BKTools.Assetbundle_Name_By_Platform + BKTools.bundles_dir[(int)eID]
        //var ta = BKTools.getBundle(eID).LoadAsset<TextAsset> (path);
        var ta = BKTools.LoadAsset<TextAsset>(eID, path);
		var xmlText = ta.text;
		ArrayList t_aArrayList = new ArrayList(); // 容器
		string[] arrs = xmlText.Split(new string[]{"\r\n","\n"},StringSplitOptions.RemoveEmptyEntries);
		foreach (string str in arrs) {
			t_aArrayList.Add (str);
		}
		return t_aArrayList;

	}
    void Start() {
		//initData();
    }
	static string m_sPath =  "Assets/Res/CSV/";
	public static void initData(Data inst){
		if (Data.has_init)
			return;
//        string m_sPath = Application.dataPath + "/Resources/CSV/";
		ArrayList list;
        list = fnLoadFile(eResBundle.CSV_Skill,m_sPath + "skill.csv");
		AnalyseSkillData(inst,list);
        list = fnLoadFile(eResBundle.CSV_Card,m_sPath + "card.csv");  //卡牌关联的技能数据要从skill中获取，必须先初始化skill
        AnalyseCardData(inst,list);
        list = fnLoadFile(eResBundle.CSV_Buff,m_sPath + "buff.csv");
        AnalyseBuffData(inst,list);
		Data.has_init = true;
	}
    static void AnalyseCardData(Data inst,ArrayList list) {
        string[] str,str_img;
        str_img = new string[list.Count - 1];//头
        for (int x = 1; x < list.Count; x++) {
            //新建信息
            Card_Info info = new Card_Info();
            str = ((string)list[x]).Split(',');
            info.id         = BKTools.ParseInt(str[(int)eCSV_Card.ID]);
            info.name       = str[(int)eCSV_Card.NAME];
            info.img        = str[(int)eCSV_Card.IMG];
            str_img[x - 1]  = info.img;
            info.cost       = BKTools.ParseInt(str[(int)eCSV_Card.COST]);
            info.spd        = BKTools.ParseInt(str[(int)eCSV_Card.SPD]);
            info.mana       = BKTools.ParseInt(str[(int)eCSV_Card.MANA]);
            info.atk        = BKTools.ParseInt(str[(int)eCSV_Card.CT]);
            info.vct        = (eCard_Vocation)BKTools.ParseInt(str[(int)eCSV_Card.VCT]);
            info.rare       = BKTools.ParseInt(str[(int)eCSV_Card.RARE]);
            info.stk        = BKTools.ParseInt(str[(int)eCSV_Card.STK]);
            info.skill01    = BKTools.ParseInt(str[(int)eCSV_Card.skill01]);
            info.skill02    = BKTools.ParseInt(str[(int)eCSV_Card.skill02]);
            info.spellcard  = BKTools.ParseInt(str[(int)eCSV_Card.spellcard]);

            inst.card_data.Add(info.id, info);
        }
        // 初始化需要完整的卡牌数据
        // TODO 也可能只初始化战斗中用到的可拍数据
        Card_Info.initSprite();
    }
    static void AnalyseSkillData(Data inst,ArrayList list) {
        //Data.Inst.skill_data.Add()
        string[] str = ((string)list[0]).Split(',');
//         foreach (string _s in str) {
//             Debug.Log(_s);
//         }
        for (int x = 1; x < list.Count; x++) {
            Skill_Info info = new Skill_Info();
            str = ((string)list[x]).Split(',');

            //保险
            if (str.Length != (int)eCSV_Skill.Max)
                continue;

            info.id                 = BKTools.ParseInt(str[(int)eCSV_Skill.ID]);
            info.name               = str[(int)eCSV_Skill.Name];
            info.my_Type            = (eSkill_Type)BKTools.ParseInt(str[(int)eCSV_Skill.Type]);
            info.describe           = str[(int)eCSV_Skill.描述];
            info.my_Condition       = (eSkill_Condition)BKTools.ParseInt(str[(int)eCSV_Skill.COD]);
            //info.表格中的隐藏列，没什么卵用    = str[(int)eCSV_Skill.COR];
            info.my_Event           = (eSkill_Event)BKTools.ParseInt(str[(int)eCSV_Skill.EVT]);
            info.my_Locator         = (eSkill_Scope_Locator)BKTools.ParseInt(str[(int)eCSV_Skill.STT]);
            //定位坐标
            string[] location = str[(int)eCSV_Skill.UPS].Split(';');
            int coo_x = 0;
            foreach(string _num in location){
                int num = BKTools.ParseInt(_num);
                switch (coo_x++) {
                    case 0:
                        info.my_location.x = num;
                        break;
                    case 1:
                        info.my_location.y = num;
                        break;
                    case 2:
                        info.my_location.z = num;
                        break;
                }
            }
            //定位方式
            string[] scopes = str[(int)eCSV_Skill.EXS].Split(';');
            info.my_Scope = new eSkill_Scope[scopes.Length];
            for (int i=0;i<scopes.Length;i++) {
                info.my_Scope[i] = (eSkill_Scope)BKTools.ParseInt(scopes[i]);
            }


            info.my_Scope_Depth = BKTools.ParseInt(str[(int)eCSV_Skill.SCO]);
            info.target_number = BKTools.ParseInt(str[(int)eCSV_Skill.TAGnum]);
            info.my_select_Solution = (eSkill_Target_SelectSolution)BKTools.ParseInt(str[(int)eCSV_Skill.TAG]);
            info.my_TargetBelong = (eSkill_TargetBelong)BKTools.ParseInt(str[(int)eCSV_Skill.TAGtype]);
            info.skill_damage = BKTools.ParseInt(str[(int)eCSV_Skill.DAM]);
            info.my_Kouka = (eSkill_Kouka)BKTools.ParseInt(str[(int)eCSV_Skill.DAMtype]);

            string[] buffs = str[(int)eCSV_Skill.BuffId].Split(';');
            info.my_buffs = new int[buffs.Length];
            for (int i = 0; i < buffs.Length; i++) {
                info.my_buffs[i] = BKTools.ParseInt(buffs[i]);
            }
            info.cg_display = str[(int)eCSV_Skill.CG_Display];

            int count = Data.Inst.skill_data.Count;
            inst.skill_data.Add(info.id, info);
        }
    }

    static void AnalyseBuffData(Data inst,ArrayList list) {
        string[] str;
        for (int x = 1; x < list.Count; x++) {

            Buff_Info info = new Buff_Info();
            str = ((string)list[x]).Split(',');

            //保险
            if (str.Length != (int)eCSV_Buff.Max)
                continue;

            info.id = BKTools.ParseInt( str[(int)eCSV_Buff.ID]);
            info.name = str[(int)eCSV_Buff.Name];
            info.describe = str[(int)eCSV_Buff.说明];
            info.my_event = (eBuffEvent)BKTools.ParseInt(str[(int)eCSV_Buff.事件]);
            info.effect = (eBuff_Effect)BKTools.ParseInt( str[(int)eCSV_Buff.Effect]);
            //数值
            string[] values =  str[(int)eCSV_Buff.Value].Split(';');
            info.values = BKTools.ParseInt(values);

            info.duration = BKTools.ParseInt( str[(int)eCSV_Buff.Round]);
            info.my_Locator = (eSkill_Scope_Locator)BKTools.ParseInt(str[(int)eCSV_Buff.起点]);
            //定位坐标
            string[] location = str[(int)eCSV_Buff.起点参数].Split(';');
            int coo_x = 0;
            foreach (string _num in location) {
                int num = BKTools.ParseInt(_num);
                switch (coo_x++) {
                    case 0:
                        info.my_location.x = num;
                        break;
                    case 1:
                        info.my_location.y = num;
                        break;
                    case 2:
                        info.my_location.z = num;
                        break;
                }
            }
            //定位方式
            string[] scopes = str[(int)eCSV_Buff.范围图形].Split(';');
            info.my_Scope = new eSkill_Scope[scopes.Length];
            for (int i = 0; i < scopes.Length; i++) {
                info.my_Scope[i] = (eSkill_Scope)BKTools.ParseInt(scopes[i]);
            }
            info.my_Scope_Depth = BKTools.ParseInt(str[(int)eCSV_Buff.范围大小]);
            info.my_TargetBelong = (eSkill_TargetBelong)BKTools.ParseInt(str[(int)eCSV_Buff.目标类型]);
            info.start_vfx = Buff_Info.vfx_buff_dir + str[(int)eCSV_Buff.开始特效];
            info.duration_vfx = Buff_Info.vfx_buff_dir + str[(int)eCSV_Buff.持续特效];
            

            inst.buff_data.Add(info.id, info);
        }
    }

}
