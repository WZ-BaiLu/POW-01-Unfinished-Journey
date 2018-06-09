using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BKKZ.POW01 {
    [System.Serializable]
    public class AreaInfo {
        public static string Area_Name_Prefix = "区域";
        public static int Area_Count = 0;
        public string area_name = string.Empty;
        public List<int> list = new List<int>();
        public AreaInfo() {
            area_name = Area_Name_Prefix + Area_Count++;
        }
    }
}