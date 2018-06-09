using UnityEngine;
using System.Collections;
namespace BKKZ.POW01 {

    public class FrameAni : MonoBehaviour {
        private SpriteRenderer sprite_renderer;
        Sprite[] sprites = new Sprite[43];
        public float FPS = 24f;
        public string path_name;
        int nowFrame = 0;
        float timeCount = 0;
        void Awake() {
            //Application.DontDestroyOnLoad(gameObject);
            //object.DontDestroyOnLoad(gameObject);
            return;
            sprite_renderer = GetComponent<SpriteRenderer>();
            for (int i = 1; i < 44; i++) {
                object o = Resources.Load("Image/turn_frame_ani/" + path_name + "/" + path_name + i.ToString("D4"));
                sprites[i - 1] = Sprite.Create(o as Texture2D, new Rect(0, 0, 1920, 480), new Vector2(0.5f, 0.5f));
                //(o as Texture2D).width
                //(o as Texture2D).height

            }
            sprite_renderer.sprite = sprites[0];
        }

        // Use this for initialization

        void Start() {
        }
        public void reset() {
            transform.position = Vector3.zero;
            nowFrame = 0;
            timeCount = 0;
            Main.Inst.addDancer();
        }
        // Update is called once per frame
        void Update() {
            timeCount += Time.deltaTime;
            nowFrame = Mathf.FloorToInt(timeCount * FPS);
            if (nowFrame >= sprites.Length) {
                Main.Inst.redDancer();
                gameObject.SetActive(false);
                return;
            }
            sprite_renderer.sprite = sprites[nowFrame];
        }
    }
}