using UnityEngine;
using System.Collections;
namespace BKKZ.POW01 {
    public class Start_Scene_Ani : MonoBehaviour {
        public ChessContainer[] chess_container_list;

        private bool is_show = true;
        public float time_interval;
        private float time_count = 0;
        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {
            foreach (ChessContainer cc in chess_container_list) {
                if (is_show) {
                    cc.GetComponent<SpriteRenderer>().material = ChessContainer.mat_mesh_diffuse;
                } else {
                    cc.GetComponent<SpriteRenderer>().material = ChessContainer.mat_sprite;
                }
            }
            time_count += Time.deltaTime;
            if (time_count > time_interval) {
                is_show = !is_show;
                time_count = 0;
            }

        }
    }
}