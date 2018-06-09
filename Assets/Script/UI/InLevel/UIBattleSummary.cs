using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BKKZ.POW01 {
    public class UIBattleSummary : MonoBehaviour {
        public const string KEY = "UIBatSum";
        public void Show(){
            gameObject.SetActive(true);
            Main.Inst.addDancer(KEY);
        }
        public void ClickContinue(){
            Main.Inst.redDancer(KEY);
            gameObject.SetActive(false);
        }
    }
}