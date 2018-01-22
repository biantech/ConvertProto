using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConvertProto
{
    
    public partial class Form3 : Form
    {
        public List<String> outPutLines = new List<string>();
        public List<String> tmpLines = new List<string>();
        public List<String> importLines = new List<string>();

        private string outputFloder = @"F:\pb\";
        public Form3()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //获取输入
            string dicDeleteInfoTxt = TextBox1.Text;
            string dicAddOrUpdateInfoTxt = TextBox2.Text;
            string className = TextBox3.Text;

            outPutLines.Clear();
            importLines.Clear();
            tmpLines.Clear();
            //生成dicAddOrUpdateInfo的dictionary
            generateDicAddOrUpdateInfo(dicAddOrUpdateInfoTxt,className);

            tmpLines.Clear();
            //生成DynamicIncreaseInfo
            generateDynamicIncreaseInfo(dicAddOrUpdateInfoTxt,dicDeleteInfoTxt,className);

            List<String> finalLines = new List<string>();
            //初始化头部
            finalLines.Add("syntax = \"proto2\";");
            finalLines.Add("package tmp;");
            finalLines.Add("");
            finalLines.Add("option java_package = \"com.ctrip.hotel.search.internalentity.pb\";");
            finalLines.Add("option java_outer_classname = \"PBDynamicIncreaseInfoFor" + className + "\";");
            finalLines.Add("");
            finalLines.AddRange(importLines);
            finalLines.AddRange(outPutLines);

            string pathOutput = outputFloder+ "DynamicIncreaseInfoFor" + className + ".proto";
            FileIOBusiness fileIOBusiness = new FileIOBusiness();
            fileIOBusiness.WriteFile(finalLines, pathOutput);
            pathOutput = outputFloder + "output.proto";

            fileIOBusiness.WriteFile(finalLines, pathOutput);
        }

        private void  generateDicAddOrUpdateInfo(string dicAddOrUpdateInfoTxt,string className)
        {
            if (String.IsNullOrWhiteSpace(dicAddOrUpdateInfoTxt))
            {
                return;
            }
            
            tmpLines.Add("optional int32 Key = 1;");
            string value = dicAddOrUpdateInfoTxt;
            if (value == "int" || value == "short" || value == "byte" || value == "Int32")
            {
                tmpLines.Add("optional int32 Value = 2;");
            }
            else if (value == "float")
            {
                tmpLines.Add("optional float Value = 2;");
            }
            else if (value == "double")
            {
                tmpLines.Add("optional double Value = 2;");
            }
            else if (value == "string" || value == "String")
            {
                tmpLines.Add("optional string Value = 2;");
            }
            else if (value == "bool" || value == "Boolean")
            {
                tmpLines.Add("optional bool Value = 2;");
            }
            else
            {
                tmpLines.Add("optional " + value + " Value = 2;");
                importLines.Add("import \"" + value + ".proto\";");
            }

            outPutLines.Add("message KVForDicAddOrUpdateInfoFor" + className);
            outPutLines.Add("{");
            outPutLines.AddRange(tmpLines);
            outPutLines.Add("}");
            
        }

        private void generateDynamicIncreaseInfo(string dicAddOrUpdateInfoTxt, string dicDeleteInfoTxt,string className) {
            if (String.IsNullOrWhiteSpace(dicAddOrUpdateInfoTxt) || String.IsNullOrWhiteSpace(dicDeleteInfoTxt))
            {
                return;
            }
            
            if (!dicDeleteInfoTxt.Contains("List"))
            {
                if (dicDeleteInfoTxt == "int" || dicDeleteInfoTxt == "short" || dicDeleteInfoTxt == "byte" || dicDeleteInfoTxt == "Int32")
                {
                    tmpLines.Add("optional int32 DicDeleteInfo = 1;");
                }
                else if (dicDeleteInfoTxt == "float")
                {
                    tmpLines.Add("optional float DicDeleteInfo = 1;");
                }
                else if (dicDeleteInfoTxt == "double")
                {
                    tmpLines.Add("optional double DicDeleteInfo = 1;");
                }
                else if (dicDeleteInfoTxt == "string" || dicDeleteInfoTxt == "String")
                {
                    tmpLines.Add("optional string DicDeleteInfo = 1;");
                }
                else if (dicDeleteInfoTxt == "bool" || dicDeleteInfoTxt == "Boolean")
                {
                    tmpLines.Add("optional bool DicDeleteInfo = 1;");
                }
                else
                {
                    tmpLines.Add("optional " + dicDeleteInfoTxt + " DicDeleteInfo = 1;");
                    importLines.Add("import \"" + dicDeleteInfoTxt + ".proto\";");
                }
            }
            else if (dicDeleteInfoTxt.Contains("List") && Regex.Matches(dicDeleteInfoTxt, "List<(.*?)>").Count == 1)
            {
                string s = dicDeleteInfoTxt.Replace("List<", "").Replace(">", "");
                if (s == "int" || s == "short" || s == "byte" || s == "Int32")
                {
                    tmpLines.Add("repeated int32 DicDeleteInfo = 1;");
                }
                else if (s == "float")
                {
                    tmpLines.Add("repeated float DicDeleteInfo = 1;");
                }
                else if (s == "double")
                {
                    tmpLines.Add("repeated double DicDeleteInfo = 1;");
                }
                else if (s == "string" || s == "String")
                {
                    tmpLines.Add("repeated string DicDeleteInfo = 1;");
                }
                else if (s == "bool" || s == "Boolean")
                {
                    tmpLines.Add("repeated bool DicDeleteInfo = 1;");
                }
                else
                {
                    tmpLines.Add("repeated " + dicDeleteInfoTxt + " DicDeleteInfo = 1;");
                    importLines.Add("import \"" + s + ".proto\";");
                }
            }

            tmpLines.Add("repeated KVForDicAddOrUpdateInfoFor" + className + " DicAddOrUpdateInfo = 2;");

            outPutLines.Add("message DynamicIncreaseInfoFor" + className);
            outPutLines.Add("{");
            outPutLines.AddRange(tmpLines);
            outPutLines.Add("}");
        }

        
    }
}
