using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BKKZ.POW01 {
    public enum eCameraFollow{
        Stuck = 0,
        Chess,
        Area,
    }
    public class ExBaCamera : MonoBehaviour {
        [SerializeField]
        float follow_speed = 1;
        [SerializeField]
        float slow_down_range = 1;
        [SerializeField]
        Vector3 BoardOff = Vector3.left;
        static float BoardZ = 0;
        Vector3 TargetPos {
            get{
                if (follow_type == eCameraFollow.Chess && m_follow_chess !=null) {
                    return new Vector3(BoardPos.x - m_follow_chess.transform.position.x,BoardPos.y - m_follow_chess.transform.position.y,BoardZ) + BoardOff;
                }else{
                    return m_follow_pos + BoardOff;
                }
            }
        }
        Vector3 BoardPos{
            get{
                return Main.Inst.lv_ctrl.chess_board.transform.position;
            }
            set {
                Main.Inst.lv_ctrl.chess_board.transform.position = value;
            }
        }

        float Speed{
            get{
                return follow_speed * Vector3.Distance(TargetPos,BoardPos) / slow_down_range;
            }
        }

        eCameraFollow follow_type = eCameraFollow.Chess;
        Chess m_follow_chess = null;
        Vector3 m_follow_pos = new Vector3(0,0,BoardZ);

        // Update is called once per frame
        void Update() {
            KeyboardInput();
            switch (follow_type){
            case eCameraFollow.Stuck:
                break;
            case eCameraFollow.Chess:
            case eCameraFollow.Area:
                FollowCamera();
                break;
            }
        }

        //TODO 临时镜头恢复功能
        void KeyboardInput(){
            if(Input.GetKeyDown(KeyCode.C) && Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.LeftControl)){
                if (m_follow_chess!=null)
                {
                    follow_type = eCameraFollow.Chess;
                }
            }

            if (Input.GetKeyDown(KeyCode.A) && Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.LeftControl)) {
                follow_type = eCameraFollow.Area;
            }

        }

        public void CameraStuck(){
            follow_type = eCameraFollow.Stuck;
        }
        public void SetFollowTarget(Chess cs) {
            m_follow_chess = cs;
            follow_type = eCameraFollow.Chess;
        }
        public void SetFollowTarget(int area_id) {
            Vector3 t = Vector3.zero;
            foreach (var item in Main.Inst.lv_ctrl.map_data.list_area_grid[area_id].list) {
                t += Main.Inst.chess_grids[item].transform.position;
            }
            t /= Main.Inst.lv_ctrl.map_data.list_area_grid[area_id].list.Count;
            t.z = BoardZ;
            m_follow_pos = BoardPos - t;
            follow_type = eCameraFollow.Area;
        }
        //其实移动的是板子
        void FollowCamera() {
            BoardPos = Vector3.MoveTowards(BoardPos, TargetPos, Speed*Time.deltaTime);
        }
    }
}