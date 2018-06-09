using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BKKZ.POW01 {
    public enum eLoadingDirection {
        Menu2Battle,
        Battle2Menu
    }
    public class SceneLoading:MonoBehaviour {
        public static string GoLevel = "Level1";
        public static eLoadingDirection direction = eLoadingDirection.Menu2Battle;
        public static AssetBundle bundle_pve_level = null;
        //包含重置功能，每次进选关界面调用可以防止读到打完一局的关卡
        public static void LoadBundle_PvE_Level() {
            if (bundle_pve_level != null)
                bundle_pve_level.Unload(true);
            if (bundle_pve_level == null)
                bundle_pve_level = AssetBundle.LoadFromFile(LevelBundleDir);
        }
        public static string LevelBundleDir {
            get {
                return BKTools.Assetbundle_path + BKTools.Assetbundle_Name_By_Platform + "pve_levels";
            }
        }
        //加载基础内容（包含重置
        public static void LoadBattleSceneContent() {
            //通用资源
            string abpath = string.Empty;
            foreach (var item in BKTools.bundles_dir) {
                abpath = BKTools.Assetbundle_path + BKTools.Assetbundle_Name_By_Platform + item;
                if (!BKTools.dic_battle_scene_content.ContainsKey(item)) {
                    BKTools.dic_battle_scene_content.Add(item, null);
                }
                if (BKTools.dic_battle_scene_content.ContainsKey(item)) {
                    if (BKTools.dic_battle_scene_content[item] != null) {
                        BKTools.dic_battle_scene_content[item].Unload(true);
                    }
                    if (BKTools.dic_battle_scene_content[item] == null) {
                        BKTools.dic_battle_scene_content[item] = AssetBundle.LoadFromFile(abpath);
                        BKTools.dic_battle_scene_content[item].LoadAllAssets();
                    }
                }
            }
            //场景资源，不能LoadAsset
            if (LevelController.BattleSceneBundle != null)
                LevelController.BattleSceneBundle.Unload(true);
            if (LevelController.BattleSceneBundle == null)
                LevelController.BattleSceneBundle = AssetBundle.LoadFromFile(BKTools.BattleSceneBundleDir);
        }
        public static void UnLoadBattleSceneContent() {
            foreach (var item in BKTools.dic_battle_scene_content) {
                if (item.Value!=null) {
                    item.Value.Unload(true);
                    BKTools.dic_battle_scene_content.Remove(item.Key);
                }
            }
            //因为不能跟其他有LoadAsset调用的放进同一个批量处理里，单独写
            if (LevelController.BattleSceneBundle != null)
                LevelController.BattleSceneBundle.Unload(true);
        }
    }
}