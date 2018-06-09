using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BKKZ.POW01 {
    public class LoadingToPvELevel : SceneLoading {
        // Use this for initialization
        void Start() {
            switch (direction) {
            case eLoadingDirection.Menu2Battle:
                LoadBattleSceneContent();
                break;
            case eLoadingDirection.Battle2Menu:
                UnLoadBattleSceneContent();
                break;
            default:
                break;
            }
            StartCoroutine(corGoAnotherScene());
        }

        IEnumerator corGoAnotherScene() {
            UnityEngine.SceneManagement.SceneManager.LoadScene(GoLevel);
            yield return null;
        }
    }
}