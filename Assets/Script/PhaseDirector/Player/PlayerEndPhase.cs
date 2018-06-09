using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using BKKZ.POW01;
public class PlayerEndPhase :TurnPhase
{
	public TurnPhase PvpNextPhase;
	public override void Start(){
		Debug.Log("结束流程");
		TurnEnd_Buff ();

		switch (Main.Inst.lv_ctrl.map_data.my_type) {
		case eMapType.PvE_Solo:
			if (Main.Inst.turn_player == ePlayer.Player1) {
				Main.Inst.turn_player = ePlayer.Enemy1;
			} else {
				Main.Inst.turn_player = ePlayer.Player1;
			}
			GoNext ();
			break;
		case eMapType.PvP_2P:
			if (Main.Inst.turn_player == ePlayer.Player1) {
				Main.Inst.turn_player = ePlayer.Player2;
			} else {
				Main.Inst.turn_player = ePlayer.Player1;
			}
			GoNext (PvpNextPhase);
			break;
		default:
			Debug.LogError ("关卡流程出错，下一回合行动者不明。");
			break;
		}
//		if (Main.Instance.levelinfo.map_data.my_type == eMapType.PvP_2P) {
//			GoNext (PvpNextPhase);
//		} else {
//			GoNext ();
//		}
	} 

	void TurnEnd_Buff(){
		BuffContrllor.Deal_Effect (Main.Inst, true, eBuffEvent.Phase_End);
//		int buffcount = 0, buff_duration_count = 0;
//		foreach (Chess c in Main.Instance.list_chess) {
//			foreach (BKKZ.Buff item in c.my_buffs) {
//				buffcount++;
//				buff_duration_count += item.my_Duration;
//			}
//		}
	}
}