using UnityEngine;
using UnityEngine.UI;
//using System.Runtime.InteropServices;
using System.Collections;
//[StructLayout(LayoutKind.Sequential,CharSet=CharSet.Auto)]
namespace BKKZ.POW01 {
    public class BtnCommon : MonoBehaviour {
        public InputField input;
        // Use this for initialization
        void Start() {
            if (input != null) {
                input.text = "4,-5, 1;5, 2, 150, 25, 0;3, 2, 100, 25, 2;6, 3, 150, 25, 0;6, 2, 150, 50, 0;4, 3, 100, 25, 2;4, 3, 100, 25, 2;4, 3, 50, 25, 2;5, 2, 100, 50, 1;6, 1, 100, 100, 1;3, 1, 100, 25, 2;3, 2, 50, 25, 2;5, 1, 50, 50, 1;5, 2, 150, 50, 0;6, 3, 150, 50, 0;3, 2, 50, 25, 2;4, 1, 150, 25, 0;";
            }
        }

        // Update is called once per frame
        void Update() {

        }
        public void loadLevel(string level) {
            if (level == "battle") {
                Data.Inst.setData(input.text);
            }
            Application.LoadLevel(level);
        }
        public void openFile() {
            //OpenFileDialog ofd = new OpenFileDialog();
        }
        public void loadLevel(int level) {
            Application.LoadLevel(level);
        }
        public void ToggleObject(GameObject refObj) {
            refObj.SetActive(!refObj.active);
        }

        public void readCopyboard(InputField input) {
            TextEditor te = new TextEditor();
            te.OnFocus();
            te.Paste();
            string s = te.content.text;
            input.text = s;
        }
    }
}