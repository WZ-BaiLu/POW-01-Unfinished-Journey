using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using BKKZ.POW01;
namespace BKKZ.POW01 {
    //[SelectionBase]
    [RequireComponent(typeof(PolygonCollider2D))]
    public class ChessContainer : CBuffContain, IComparable<ChessContainer> {
        public eMapGridType terrain_type;
        public int number;
        public int row;
        public int column;
        //public Main Main.Instance;
        public bool move_flag = false;  //可到达
        bool[] fog_flag = new bool[(int)ePlayer.Max];
        public Chess my_chess = null;
        //动画表现用
        public static Material mat_sprite;
        public static Material mat_mesh_diffuse;
        int aniFogState = 0;//Normal=0,Half=1,Lift=2    用于在正式修改动画状态前标记和对比
        public Animator aniFog = null;
        public Animator AniFog{
            get{
                if (aniFog == null) {
                    for (int j = 0; j < transform.childCount; j++) {
                        var child = transform.GetChild(j);
                        if (child.tag.Contains("Fog")) {
                            aniFog = child.GetComponent<Animator>();
                        }
                    }
                }
                return aniFog;
            }
        }
        //ChessContainer Around
        public ChessContainer CCUpper;
        public ChessContainer CCLower;
        public ChessContainer CCUpperRight;
        public ChessContainer CCLowerLeft;
        public ChessContainer CCUpperLeft;
        public ChessContainer CCLowerRight;
        public Action<ChessContainer> MouseDown;
        public Action<ChessContainer> MouseUp;
        // Use this for initialization
        void Start() {
            base.Start();
            //默认对所有人有雾霾
            for (int i = 0; i < fog_flag.Length; i++) {
                fog_flag[i] = true;
            }
            //        Main.Instance = Camera.main.GetComponent<Main>();
            //         if (bg_unmovable==null) {
            //             bg_unmovable = Resources.Load("Prefabs/UI/unmovable_area") as GameObject;
            //         }

            //		UnityEngine.UI.Button btn = GetComponent<UnityEngine.UI.Button>();
            //		if (btn!= null) {
            //			btn.onClick.RemoveAllListeners();
            //			btn.onClick.AddListener(OnMouseDown);
            //		}
        }

        // Update is called once per frame

        void Update() {

            if (list_buffs_add.Count > 0) {
                //             BuffAdder[] adds = new BuffAdder[list_buffs_add.Count];
                //             list_buffs_add.CopyTo(adds);
                //             list_buffs_add.Clear();
                //             Main.Instance.redDancer("cc_buff_adder");
                // 
                //             //异步添加BUFF
                //             foreach (BuffAdder buff_id in adds)
                //             {
                //                 {
                //                     Buff new_buff = new Buff();
                //                     new_buff.stand_side = buff_id.from;
                //                     new_buff.my_buff_info = Data.Inst.buff_data[buff_id.id];
                //                     new_buff.my_Duration = new_buff.my_buff_info.duration;
                //                     my_buffs.Add(new_buff);
                //                     new_buff.owner_chc = this;
                //                     new_buff.owner = this;
                // 
                //                     BuffContrllor.analyseBuff_Effect(new_buff, eBuffEvent.Buff_Add);
                //                 }
                //             }
                BuffContrllor.addBuff(ref list_buffs_add, this, null, this, "cc_buff_adder");
            }
            playNextVFX();
        }
        //重复了，已替换完
        //public void search_around(ref List<ChessContainer> list, int depth, int d_max) {
        //    if (depth >= d_max) {
        //        return;
        //    }
        //    foreach (dGetAroundChessContainer dget in Main.Inst.dGetChessContainer) {
        //        ChessContainer t_cc = dget(this);
        //        if (t_cc) {
        //            t_cc.search_around(ref list, depth + 1, d_max);
        //            if (!list.Contains(t_cc)) {
        //                list.Add(t_cc);
        //            }
        //        }
        //    }
        //}
        public void setMoveFlag_On() {
            if (my_chess != null) {
                return;
            }
            move_flag = true;
        }
        public void clearMoveFlag() {
            move_flag = false;
        }
        //一次操作，互相绑定
        public void appendChess(Chess c) {
            my_chess = c;
            my_chess.transform.parent = transform;
            my_chess.transform.localPosition = Vector3.back;
            my_chess.transform.localScale = Vector3.one;
            if (c != null) {
                c.container = this;
            }
        }
        public void removeChess() {
            if (my_chess != null) {
                my_chess.container = null;
            }
            my_chess = null;
        }
        public bool isMoveBan(ePlayer runner) {
            foreach (Buff buff in my_buffs) {
                if (buff.my_buff_info.effect == eBuff_Effect.Move_BAN) {
                    if (BKTools.IsTargetFit(buff.my_buff_info.my_TargetBelong, runner, buff.stand_side))
                        return true;
                }
            }
            return false;
        }
        //清理迷雾
        static Color half_alpha = new Color(0, 0, 0, 0.5f);
        const string KEYFogLift = "FogLift";
        public void FogLift(bool totally, int[] target_players) {
            if (AniFog == null) return;
            if (aniFogState > (totally?1:0)) return;
            aniFogState = totally ? 2 : 1;
            //清理标记
            foreach (var player in target_players) {
                //非全部清除时，是可见但不可移动的
                fog_flag[player] = false || !totally;
            }

            //清理sprite TODO 对多人版本时，不直接操作sprite，用脚本控制sprite对不同玩家的可见性
            bool res = true;
            for (int i = 0; i < target_players.Length; i++) {
                if (target_players[i] == (int)ePlayer.Player1)
                    res = false;
            }
            if (res) return;//避免对当前客户端意外的玩家的画面上的迷雾产生影响
            StartCoroutine(corFogLift(totally));
        }
        //改用animation_clip实现
        IEnumerator corFogLift(bool totally) {
            Main.Inst.addDancer(KEYFogLift);
            yield return new WaitForSeconds(UnityEngine.Random.Range(0, GameRule.FogRandomMax));
            AniFog.SetInteger("State",aniFogState);
            yield return new WaitForSeconds(1);
            //AniFog.gameObject.SetActive(!totally);
            Main.Inst.redDancer(KEYFogLift);
            yield return null;
        }
        //升起迷雾
        //static Vector3 fog_position = new Vector3(0, 0, -2);  //淘汰咯，有动画就行了
        public void FogCover(int[] target_players) {
            if (AniFog == null) {
                //增加sprite
                //aniFog = Instantiate(Resources.Load<GameObject>(PrefabPath.ChessGridFog), transform).GetComponent<Animator>();
                //aniFog = Instantiate(BKTools.getBundleObject(eResBundle.Prefabs,PrefabPath.ChessGridFog), transform).GetComponent<Animator>();
                aniFog = Instantiate(BKTools.LoadAsset<GameObject>(eResBundle.Prefabs, PrefabPath.ChessGridFog), transform).GetComponent<Animator>();
            }
            if (aniFogState <= 0) return;
            aniFogState = 0;
            //清理标记
            //Fog_flag = true;
            foreach (var player in target_players) {
                fog_flag[player] = true;
            }
            StartCoroutine(corFogCover());

        }
        IEnumerator corFogCover() {
            Main.Inst.addDancer(KEYFogLift);
            yield return new WaitForSeconds(UnityEngine.Random.Range(0, GameRule.FogRandomMax));
            //AniFog.gameObject.SetActive(true);
            AniFog.SetInteger("State",aniFogState);
            yield return new WaitForSeconds(1);
            //obj.GetComponent<Animator>().SetActive(false);
            Main.Inst.redDancer(KEYFogLift);
            yield return null;
        }
        //↓BuffContain接口实现====================================
        public override ChessContainer getContainer() {
            return this;
        }
        //    public void AddBuff(BuffAdder adder)
        //    {
        //        //异步添加
        //        list_buffs_add.Add(adder);
        //        Main.Instance.addDancer("cc_buff_adder");
        //    }
        public void RelateTo(ChessContainer target,eDirection dir) {
            ReverseCheckRelation(dir);
            switch (dir) {
            case eDirection.LowerRight:
                CCLowerRight = target;
                target.CCUpperLeft = this;
                break;
            case eDirection.UpperLeft:
                CCUpperLeft = target;
                target.CCLowerRight = this;
                break;
            case eDirection.Lower:
                CCLower = target;
                target.CCUpper = this;
                break;
            case eDirection.Upper:
                CCUpper = target;
                target.CCLower = this;
                break;
            case eDirection.LowerLeft:
                CCLowerLeft = target;
                target.CCUpperRight = this;
                break;
            case eDirection.UpperRight:
                CCUpperRight = target;
                target.CCLowerLeft = this;
                break;
            default:
                Debug.Log("Something wrong! Getting unkown dirction of chesscontainner");
                return;
            }
        }
        /// <summary>
        /// MapEditor自动关联出现BUG，仅搜索上方3个时，有缺少的棋子都鬼使神差地关联了某个特定棋子。
        /// 由于自动清空会额外造成特殊关联被删除，采用反向清理【你是我的右下角，所以你的左上角如果关联了东西，那一定是有错误
        /// 2018年6月5日02:08:17 没起效
        /// <para  dir>My direction</para>
        /// </summary>
        public void ReverseCheckRelation(eDirection dir) {
            switch (dir) {
            case eDirection.LowerRight:
                if (CCLowerRight != null)
                    CCLowerRight.CCUpperLeft = null;
                break;
            case eDirection.UpperLeft:
                if (CCUpperLeft != null)
                    CCUpperLeft.CCLowerRight = null;
                break;
            case eDirection.Lower:
                if (CCLower != null)
                    CCLower.CCUpper = null;
                break;
            case eDirection.Upper:
                if (CCUpper != null)
                    CCUpper.CCLower = null;
                break;
            case eDirection.LowerLeft:
                if (CCLowerLeft != null)
                    CCLowerLeft.CCUpperRight = null;
                break;
            case eDirection.UpperRight:
                if (CCUpperRight != null)
                    CCUpperRight.CCLowerLeft = null;
                break;
            default:
                Debug.Log("Something wrong! Getting unkown dirction of chesscontainner");
                return;
            }
        }
        //获得周围的格子 对应关系参考BK_Tools
        public ChessContainer GetAround(eDirection dir) {
            return GetAround((int)dir);
        }
        public ChessContainer GetAround(int dir) {
            switch (dir) {
            case 0:
                return CCUpperLeft;
            case 1:
                return CCLowerRight;
            case 2:
                return CCUpper;
            case 3:
                return CCLower;
            case 4:
                return CCUpperRight;
            case 5:
                return CCLowerLeft;
            default:
                Debug.Log("Something Wrong! Get unkown dirction of chesscontainner");
                return null;
            }
        }
        void OnMouseDown() {
            //		Debug.Log ("click");
            MouseDown(this);
        }
        void OnMouseUp() {
            MouseUp(this);
        }
        //	public override bool Equals(object obj)
        //	{
        //		if (obj == null) return false;
        //		ChessContainer objAsPart = obj as ChessContainer;
        //		if (objAsPart == null) return false;
        //		else return Equals(objAsPart);
        //	}
        //	//排序用，并非判断两个东西一样
        //	public bool Equals(ChessContainer other)
        //	{
        //		if (other == null) return false;
        //		return (this.number.Equals(other.number));
        //	}
        // Default comparer for Part type.
        public int CompareTo(ChessContainer comparePart) {
            // A null value means that this object is greater.
            if (comparePart == null)
                return 1;

            else
                return this.number.CompareTo(comparePart.number);
        }
        //用求标记这个格子可以去的方向
        void OnDrawGizmosSelected() {
            if (CCUpper != null){
                DrawGizmosToward(Color.green,Color.green,transform.position, CCUpper.transform.position);
            }
            if(CCLower != null){
                DrawGizmosToward(Color.green,Color.green,transform.position, CCLower.transform.position);
            }
            if(CCUpperRight != null){
                DrawGizmosToward(Color.green,Color.green,transform.position, CCUpperRight.transform.position);
            }
            if(CCLowerLeft != null){
                DrawGizmosToward(Color.green,Color.green,transform.position, CCLowerLeft.transform.position);
            }
            if(CCUpperLeft != null){
                DrawGizmosToward(Color.green,Color.green,transform.position, CCUpperLeft.transform.position);
            }
            if(CCLowerRight != null){
                DrawGizmosToward(Color.green,Color.green,transform.position, CCLowerRight.transform.position);
            }

        }
        //关联专用
        void DrawGizmosToward(Color c1, Color c2, Vector3 pstart, Vector3 pend) {
            Gizmos.color = c1;
            Vector3 p1 = Vector3.MoveTowards(pstart, pend, (BKTools.chess_container_size.y - 0.2f) * transform.lossyScale.y / 2);
            Vector3 p2 = Vector3.MoveTowards(pend, pstart, (BKTools.chess_container_size.y - 0.2f) * transform.lossyScale.y / 2);
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawSphere(p2, 0.1f);
        }
        public static eDirection[] AlignOrder = new eDirection[] { eDirection.Lower ,eDirection.LowerLeft, eDirection.UpperLeft, eDirection.Upper,eDirection.UpperRight,eDirection.LowerRight};
#if UNITY_EDITOR
        /// <summary>
        /// 以这个方向为标准对齐
        /// </summary>
        /// <param name="dir"></param>
        public bool AlignGrid(eDirection dir) {
            ChessContainer target = GetAround(dir);
            if (target != null) {
                Vector3 checkpos = Vector3.zero; ;
                checkpos = new Vector3(0, BKTools.chess_container_size.y, 0) * transform.lossyScale.x;
                int reverse_dir = (int)dir;
                reverse_dir = (reverse_dir / 2) * 2 + (1 - reverse_dir % 2);
                checkpos = Quaternion.Euler(0, 0, BKTools.AngularByDirection((eDirection)reverse_dir)) * checkpos;
                transform.position = target.transform.position + checkpos;
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                return true;
            }
            return false;
        }
#endif
    }
}