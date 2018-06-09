using UnityEditor;
using UnityEngine;
using System.Collections;
namespace BKKZ.POW01 {
    public class Tools {

        public static bool CheckBox(string title, ref int data) {
            return CheckBox(title, ref data, false, null, 1, -1);
        }
        /// <summary>
        /// 带poplist
        /// </summary>
        /// <returns><c>true</c>, if box was checked, <c>false</c> otherwise.</returns>
        /// <param name="title">Title.</param>
        /// <param name="data">Data.</param>
        /// <param name="popitem">Popitem.</param>
        public static bool CheckBox(string title, ref int data, string[] popitem) {
            return CheckBox(title, ref data, true, popitem, 1, -1);
        }

        public static bool CheckBox(string title, ref int data, string[] popitem, int default_value, int unable_value) {
            return CheckBox(title, ref data, true, popitem, default_value, unable_value);
        }


        /// <summary>
        /// 快速生成带勾选方式的编辑界面
        /// 目前的存储方式使得>=0会被认为起效，而取消时被设成负值。故如果值为0，会用到unable_value
        /// 根据不同的值类型，如全局开关0-19和携带物品ID四五位数，提供不同的默认值
        /// </summary>
        /// <returns><c>true</c>, if box was checked, <c>false</c> otherwise.</returns>
        /// <param name="title">Title.</param>
        /// <param name="data">Data.</param>
        /// <param name="isPop">If set to <c>true</c> is pop.</param>
        /// <param name="popitem">Popitem.</param>
        /// <param name="default_value">默认值，被勾选成有效时显示谁.</param>
        /// <param name="unable_value">表示无效的值.</param>
        static bool CheckBox(string title, ref int data, bool isPop, string[] popitem, int default_value, int unable_value) {
            EditorGUILayout.BeginHorizontal();
            bool check = EditorGUILayout.Toggle(title, data >= 0);
            if (check) {
                if (data == unable_value)
                    data = default_value;
                if (data < 0)
                    data = Mathf.Abs(data);
                if (isPop)
                    data = EditorGUILayout.Popup(data, popitem);
                else
                    data = EditorGUILayout.IntField(data);
            } else {
                if (data > 0)
                    data = -Mathf.Abs(data);
                if (data == default_value)
                    data = unable_value;
            }

            EditorGUILayout.EndHorizontal();
            return check;
        }
        //public static void XMLQuickEditor(Object obj){
        //    System.Type type = obj.GetType();
        //    var fields = type.GetFields();
        //    foreach (var item in fields) {
        //        //object[] objs = item.GetCustomAttributes(typeof(SerializeField),false);
        //        var att = item.GetCustomAttributes(typeof(XMLLayoutAttribute), false);
        //        if (att.Length > 0) {
        //            XMLLayoutAttribute xml = att[0] as XMLLayoutAttribute;
        //            item.GetValue(obj);
        //            //Debug.Log(item.Name);
        //        }
        //        //objs as SerializeFieldAttribute;
        //    }
        //}
        /// <summary>
        /// 分析XML自动编辑器属性生成编辑器，如果有修改返回true
        /// </summary>
        /// <returns><c>true</c>, if XML Editor was modified, <c>false</c> otherwise.</returns>
        /// <param name="obj">Object.</param>
        public static bool AnalyseXMLEditor(System.Object obj) {
            var fields = obj.GetType().GetFields();
            return GUIMemb(fields, obj);
        }
        public static bool GUIMemb(System.Reflection.FieldInfo[] memb, System.Object obj) {
#if XML_GROUP_ENABLE
            System.Collections.Generic.List<XMLLayoutGroupAttribute> groupStack = new System.Collections.Generic.List<XMLLayoutGroupAttribute>();
#endif
            foreach (var item in memb) {

                //分组暂时弃用 2017年11月02日12:08:16
                var att = item.GetCustomAttributes(typeof(XMLLayoutAttribute), false);
                if (att.Length == 0) continue;

                XMLLayoutAttribute xml = att[0] as XMLLayoutAttribute;
                //分组信息  2017年11月02日17:47:20 放弃分组功能
                //XMLLayoutGroupAttribute[] item_groups = (XMLLayoutGroupAttribute[])item.GetCustomAttributes(typeof(XMLLayoutGroupAttribute), false);
                //if (item_groups.Length > 0) {
                //    //增加分组
                //    foreach (var g in item_groups) {
                //        if (!CheckXMLGroupsStackContain(g.GroupName, groupStack)) {
                //            switch (g.LayoutType) {
                //                case E_XMLLayoutType.Horizontal:
                //                    EditorGUILayout.BeginHorizontal();
                //                    Debug.Log("新分组，横-" + g.GroupName);
                //                    break;
                //                case E_XMLLayoutType.Vertical:
                //                    EditorGUILayout.BeginVertical();
                //                    Debug.Log("新分组，竖-" + g.GroupName);
                //                    break;
                //                default:
                //                    break;
                //            }
                //            groupStack.Add(g);
                //        }
                //    }
                //    CheckXMLGroupsEnd(groupStack,item_groups);
                //}


                object res = item.GetValue(obj);
                string displayname = xml.Displayname;

                //EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(displayname/*, GUILayout.MaxWidth(50)*/);
                //枚举
                if (res.GetType().IsEnum)
                    item.SetValue(obj, EditorGUILayout.EnumPopup((System.Enum)res));
                //字符串
                if (res is string)
                    item.SetValue(obj, EditorGUILayout.TextField((string)res));
                //整型
                if (res is int) {
                    if(xml.IntergrateKey == null)
                        item.SetValue(obj, EditorGUILayout.IntField((int)res));
                    else{
                        if(xml.IntergrateValue == null)
                            //设计之初是为EventList服务，虽然指定的序号是int，但必须通过文字描述才好认。
                            //2018年6月7日02:05:38 准备给GridID用的时候就将就一下吧
                            item.SetValue(obj, EditorGUILayout.Popup((int)res,GetArrByReflaction<string>(xml.IntergrateKey)));
                        else
                            item.SetValue(obj, EditorGUILayout.IntPopup((int)res, GetArrByReflaction<string>(xml.IntergrateKey)
                                                                                ,GetArrByReflaction<int>(xml.IntergrateValue)));
                    }
                }
                //浮点型
                if (res is float)
                    item.SetValue(obj, EditorGUILayout.FloatField((float)res));

                //整形关联数字


                //EditorGUILayout.EndHorizontal();
            }
            return GUI.changed;
            //CheckXMLGroupsEnd(groupStack, new XMLLayoutGroupAttribute[]{});
        }
        static string AssemlyNextfix = ",Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
        static string EditorAssemlyNextfix = ",Assembly-CSharp-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
        //,Assembly-CSharp-Editor
        //,Assembly-CSharp
        public static T[] GetArrByReflaction<T>(string path){
            
            //System.Type.GetType();
            var members = path.Split(';');
            System.Type type = System.Type.GetType(members[0] + AssemlyNextfix);
            if(type==null)
                type = System.Type.GetType(members[0] + EditorAssemlyNextfix);
            members = members[1].Split('.');

            return DSMemberValue<T>(type, null, members);

            //System.Reflection.MemberInfo member = null;
            //for (int i = 0; i < members.Length; i++) {
            //    if (member==null) {
            //        member = type.GetMember(members[i])[0];
            //    }else
            //    {
            //        member = member.GetType().GetMember(members[i])[0];
            //    }
            //}
            //switch(member.MemberType){
            //    case System.Reflection.MemberTypes.Field:
            //        var field = (System.Reflection.FieldInfo)member;
            //        var res = (System.Array)field.GetValue(null);

            //        return (T[])res;
            //        break;
            //    case System.Reflection.MemberTypes.Property:
            //        var prop = (System.Reflection.PropertyInfo)member;
            //        var res1 = (System.Array)prop.GetValue(null,null);

            //        return (T[])res1;
            //        break;
            //    default:
            //        return null;
            //        break;
            //}
        }
        static T[] DSMemberValue<T>(System.Type src, object obj,string[] names){
            if (names.Length == 0)
                return (T[])(System.Array)obj;


            string[] subarray = new string[names.Length - 1];
            System.Array.Copy(names, 1, subarray, 0, subarray.Length);

            System.Reflection.MemberInfo member;
            if (obj != null)
                member = obj.GetType().GetMember(names[0])[0];
            else
                member = src.GetMember(names[0])[0];


            switch (member.MemberType) {
                case System.Reflection.MemberTypes.Field:
                    var field = (System.Reflection.FieldInfo)member;
                    var res = field.GetValue(obj);

                    return DSMemberValue<T>(src, res, subarray);
                case System.Reflection.MemberTypes.Property:
                    var prop = (System.Reflection.PropertyInfo)member;
                    var res1 = prop.GetValue(obj, null);

                    return DSMemberValue<T>(src, res1, subarray);
                default:
                    return null;
            }


        }
#if XML_GROUP_ENABLE
        //要求分组不重名
        static bool CheckXMLGroupsStackContain(string group_name,System.Collections.Generic.List<XMLLayoutGroupAttribute> groupStack){
            foreach (var item in groupStack) {
                if (item.GroupName == group_name) {
                    return true;
                }
            }
            return false;
        }
        //要求分组不重名
        static bool CheckXMLGroupsStackContain(string group_name, XMLLayoutGroupAttribute[] groupStack) {
            foreach (var item in groupStack) {
                if (item.GroupName == group_name) {
                    return true;
                }
            }
            return false;
        }
        //分组结束判断
        static void CheckXMLGroupsEnd(System.Collections.Generic.List<XMLLayoutGroupAttribute> groupStack,XMLLayoutGroupAttribute[] groups){

            //结束分组
            for (int i = groupStack.Count - 1; i >= 0; i--) {
                if (!CheckXMLGroupsStackContain(groupStack[i].GroupName, groups)) {
                    switch (groupStack[i].LayoutType) {
                        case E_XMLLayoutType.Horizontal:
                            EditorGUILayout.EndHorizontal();
                            Debug.Log("结束分组，横-" + groupStack[i].GroupName);
                            break;
                        case E_XMLLayoutType.Vertical:
                            EditorGUILayout.EndHorizontal();
                            Debug.Log("结束分组，横-" + groupStack[i].GroupName);
                            break;
                        default:
                            break;
                    }
                    groupStack.RemoveAt(i);
                }
            }
        }
#endif
	}
}