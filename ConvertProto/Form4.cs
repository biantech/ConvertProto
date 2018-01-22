using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConvertProto
{
    public partial class Form4 : Form
    {
        public List<String> lines = new List<string>();
        public string javaPackageName = "com.ctrip.hotel.search.internalentity.pb";
        private string outputFinalFloder = @"D:\proto\";
        private string outputFloder = @"F:\pb\";
        public Form4()
        {
            InitializeComponent();
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            
            string className = ClassNameBox.Text;
            string businessName = BusinessNameBox.Text;

            if (String.IsNullOrWhiteSpace(className) || String.IsNullOrWhiteSpace(businessName)) {
                return;
            }

            FileIOBusiness fileIO = new FileIOBusiness();
            fileIO.CreateFloder(outputFinalFloder + businessName);
           
            lines.Clear();
            addHead("PBDynamicDataV2For" + className);
            lines.Add("import \""+className+".proto\";");
            lines.Add("");

            lines.Add("message DynamicDataV2For" + className);
            lines.Add("{");
            lines.Add("repeated "+className+" EntityList = 1;");
            lines.Add("optional string StartDate = 2;");
            lines.Add("repeated int64 EntityDateMapList = 3;");
            lines.Add("}");
            fileIO.WriteFile(lines, outputFinalFloder + businessName + @"\"+"DynamicDataV2For" + className + ".proto");
            fileIO.WriteFile(lines, outputFloder+"output1.proto");
            lines.Clear();

            addHead("PBDictionaryForDynamicDataV2For" + className);
            lines.Add("import \"DynamicDataV2For" + className + ".proto\";");
            lines.Add("");

            lines.Add("message DictionaryForDynamicDataV2For" + className);
            lines.Add("{");
            lines.Add("repeated KVForDictionaryForDynamicDataV2For" + className + " KeyValuePair = 1;");
            lines.Add("}");

            lines.Add("message KVForDictionaryForDynamicDataV2For" + className);
            lines.Add("{");
            lines.Add("optional int32 Key = 1;");
            lines.Add("optional DynamicDataV2For" + className + " Value = 2;");
            lines.Add("}");

            fileIO.WriteFile(lines, outputFinalFloder  + businessName + @"\"+"DictionaryForDynamicDataV2For" + className + ".proto");
            fileIO.WriteFile(lines, outputFloder+"output2.proto");
            lines.Clear();

            addHead("PBDynamicIncreaseInfoFor" + businessName);
            lines.Add("import \"DynamicDataV2For" + className + ".proto\";");
            lines.Add("import \"DictionaryForListDateTime.proto\";");
            lines.Add("");

            lines.Add("message KVForDicAddOrUpdateInfoFor" + businessName);
            lines.Add("{");
            lines.Add("optional int32 Key = 1;");
            lines.Add("optional DynamicDataV2For" + className + " Value = 2;");
            lines.Add("}");

            lines.Add("message DynamicIncreaseInfoFor" + businessName);
            lines.Add("{");
            lines.Add("repeated KVForDictionaryForListDateTime DicDeleteInfo = 1;");
            lines.Add("repeated KVForDicAddOrUpdateInfoFor" + businessName + " DicAddOrUpdateInfo = 2;");
            lines.Add("}");
            fileIO.WriteFile(lines, outputFinalFloder + businessName + @"\" + "DynamicIncreaseInfoFor" + className + ".proto");
            fileIO.WriteFile(lines, outputFloder + "output3.proto");
            lines.Clear();
        }

        private void addHead(string outClassName) {
            lines.Add("syntax = \"proto2\";");
            lines.Add("package tmp;");
            lines.Add("");

            lines.Add("option java_package = \"" + javaPackageName +"\";");
            lines.Add("option java_outer_classname = \"" + outClassName + "\";");

        }

        private void button3_Click(object sender, EventArgs e)
        {
            string className = ClassNameBox1.Text;
            string businessName = BusinessNameBox1.Text;

            if (String.IsNullOrWhiteSpace(className) || String.IsNullOrWhiteSpace(businessName))
            {
                return;
            }
            FileIOBusiness fileIO = new FileIOBusiness();
            fileIO.CreateFloder(outputFinalFloder + @"\"+businessName);

            lines.Clear();
            addHead("PBDictionaryFor" + className + "List");
            lines.Add("import \"" + className + ".proto\";");
            lines.Add("");


            lines.Add("message DictionaryFor" + className + "List");
            lines.Add("{");
            lines.Add("repeated KVForDictionaryFor" + className + "List KeyValuePair = 1;");
            lines.Add("}");

            lines.Add("message KVForDictionaryFor" + className + "List");
            lines.Add("{");
            lines.Add("optional int32 Key = 1;");
            lines.Add("repeated " + className + " Value = 2;");
            lines.Add("}");

            fileIO.WriteFile(lines, outputFinalFloder + businessName + @"\" + "DictionaryFor" + className + "List.proto");
            fileIO.WriteFile(lines, outputFloder + "output1.proto");
            
            lines.Clear();
            addHead("PBDynamicIncreaseInfoFor" + businessName);
            lines.Add("import \"" + className + ".proto\";");
            lines.Add("import \"DictionaryForListInt.proto\";");
            lines.Add("");

            lines.Add("message KVForDicAddOrUpdateInfoFor" + businessName);
            lines.Add("{");
            lines.Add("optional int32 Key = 1;");
            lines.Add("repeated " + className + " Value = 2;");
            lines.Add("}");

            lines.Add("message DynamicIncreaseInfoFor" + businessName);
            lines.Add("{");
            lines.Add("repeated KVForDictionaryForListInt DicDeleteInfo = 1;");
            lines.Add("repeated KVForDicAddOrUpdateInfoFor" + businessName + " DicAddOrUpdateInfo = 2;");
            lines.Add("}");

            fileIO.WriteFile(lines, outputFinalFloder + businessName + @"\" + "DynamicIncreaseInfoFor" + businessName + ".proto");
            fileIO.WriteFile(lines, outputFloder + "output2.proto");

        }

        
    }
}
