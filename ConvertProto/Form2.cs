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
    public partial class Form2 : Form
    {
        private string javaPakageName = StaticInfo.javaPackageName;
        private string outputFloder = StaticInfo.outputPath;
        public Form2()
        {
            InitializeComponent();
            StaticInfo.checkoutPath();
            outputFloder = StaticInfo.outputPath;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            List<String> outPutLines = new List<string>();
            List<String> tmpLines = new List<string>();
            List<String> importLines = new List<string>();
            string key = KeyBox.Text;
            string className = ClassNameBox.Text;
            string value = ValueBox.Text;
            string pathOutput = outputFloder+"DictionaryFor" + className + ".proto";
            
            if (String.IsNullOrWhiteSpace(className)) {
                className = "" + value;
                ClassNameBox.Text = className;
            }

            //初始化头部
            outPutLines.Add("syntax = \"proto2\";");
            outPutLines.Add("package tmp;");
            outPutLines.Add("");
            outPutLines.Add("option java_package = \""+javaPakageName+"\";");
            outPutLines.Add("option java_outer_classname = \"PBDictionaryFor" + className + "\";");
            outPutLines.Add("");

            tmpLines.Add("optional " + StaticInfo.getParaNameAndImport(key,ref importLines) + " Key = 1;");
            if (Regex.Matches(value, @"\s*?List\<(\w*)\>\s*?").Count == 1 && Regex.Matches(value, ">").Count == 1)
            {
                tmpLines.Add("repeated " + StaticInfo.getParaNameAndImport(Regex.Matches(value, @"\s*?List\<(\w*)\>\s*?")[0].Groups[1].Value, ref importLines) + " Value = 2;");
                
            }
            else
            {
                tmpLines.Add("optional " + StaticInfo.getParaNameAndImport(value, ref importLines) + " Value = 2;");
            }
            outPutLines.AddRange(importLines);

            outPutLines.Add("message DictionaryFor" + className);
            outPutLines.Add("{");
            outPutLines.Add("repeated KVForDictionaryFor" + className +" KeyValuePair = 1;");
            outPutLines.Add("}");

            outPutLines.Add("message KVForDictionaryFor" + className);
            outPutLines.Add("{");
            outPutLines.AddRange(tmpLines);
            outPutLines.Add("}");

            FileIOBusiness fileIOBusiness = new FileIOBusiness();
            fileIOBusiness.WriteFile(outPutLines, pathOutput);
            pathOutput = outputFloder+"output.proto";

            fileIOBusiness.WriteFile(outPutLines, pathOutput);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            List<String> outPutLines = new List<string>();
            List<String> tmpLines = new List<string>();
            List<String> importLines = new List<string>();

            string className = ClassNameBox.Text;
            string pathOutput = outputFloder + className + "List.proto";

            //初始化头部
            outPutLines.Add("syntax = \"proto2\";");
            outPutLines.Add("package tmp;");
            outPutLines.Add("");
            outPutLines.Add("option java_package = \"" + javaPakageName + "\";");
            outPutLines.Add("option java_outer_classname = \"PB" + className + "List\";");
            outPutLines.Add("");
            outPutLines.Add("import \""+className+".proto\";");

            outPutLines.Add("message " + className +"List");
            outPutLines.Add("{");
            outPutLines.Add("repeated "+className+" Element = 1;");
            outPutLines.Add("}");

            FileIOBusiness fileIOBusiness = new FileIOBusiness();
            fileIOBusiness.WriteFile(outPutLines, pathOutput);
        }

    }
}
