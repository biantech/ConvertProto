using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using System.Threading;

namespace ConvertProto
{
    public partial class EntityGenerator : Form
    {
        //是否自动生成java文件
        private Boolean generateJava = true;
        
        //控制两个TextBox是否可更改
        private bool javaPackageNameCanEdit = false;
        private bool protobufVersionCanEdit = false;

        //类名
        private string className;

        private FileIOBusiness fileIOBusiness = new FileIOBusiness();

        //输出生成结果
        private List<string> outputMsg = new List<string>();

        public EntityGenerator()
        {
            InitializeComponent();
            StaticInfo.initialDictionary();
            refreshPage();
        }

      
        private string toFirstLetterUpper(string s)
        {
            if (s.Length > 1)
            {
                string res = s.Substring(0, 1).ToUpper() + s.Substring(1, s.Length - 1);
                return res;
            }
            else if(s.Length == 1)
            {
                return s.ToUpper();
            }
            return s;
        }
        

        private void javaPackageNameLockBtn_Click(object sender, EventArgs e)
        {
            if (javaPackageNameCanEdit)
            {
                StaticInfo.javaPackageName = javaPackageNameTextBox.Text; 
                javaPackageNameTextBox.Enabled = false;
                javaPackageNameCanEdit = false;
            }
            else
            {
                javaPackageNameTextBox.Enabled = true;
                javaPackageNameCanEdit = true;
            }
        }

        private void protoVersionLockBtn_Click(object sender, EventArgs e)
        {
            if (protobufVersionCanEdit)
            {
                StaticInfo.protobufVersion = protoVersionTextBox.Text;
                protoVersionTextBox.Enabled = false;
                protobufVersionCanEdit = false;
            }
            else
            {
                protoVersionTextBox.Enabled = true;
                protobufVersionCanEdit = true;
            }
        }

        private void changeOutputPathBtn_Click(object sender, EventArgs e)
        {
            if (outputFloderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                StaticInfo.outputPath = outputFloderBrowserDialog.SelectedPath;
                outputPathLabel.Text = StaticInfo.outputPath;
                StaticInfo.checkoutPath();
            }
        }

        private void changeProgramPathBtn_Click(object sender, EventArgs e)
        {
            if (protoProgramFolderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                StaticInfo.generateorPath = protoProgramFolderBrowserDialog.SelectedPath;
                ProtoProgramPathLabel.Text = StaticInfo.generateorPath;
            }
        }

        private void generateBtn_Click(object sender, EventArgs e)
        {
            GeneratorMsgTextBox.Clear();
            outPutProtoTextBox.Clear();
            outputMsg.Clear();
            string[] aryString = (!String.IsNullOrEmpty(SourceText.Text.Trim())) ? SourceText.Lines : null;
            if (aryString == null) {
                return;
            }
            aryString = aryString.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();  //去除空行
            List<String> inputLines = new List<string>(aryString);
            List<String> outputProtoLines = Convert(inputLines);

            StaticInfo.checkoutPath();
            string pathOutput = StaticInfo.outputPath + StaticInfo.javaPackageName.Replace(".", "/") + "/" + className + ".proto";
            fileIOBusiness.CreateFloder(StaticInfo.outputPath + StaticInfo.javaPackageName.Replace(".", "/") + "/");
            fileIOBusiness.WriteFile(outputProtoLines, pathOutput);
            StaticInfo.checkoutPath2();
            fileIOBusiness.WriteFile(outputProtoLines, StaticInfo.generateorPath+ className + ".proto");
            GenerateJava(pathOutput);
            outputMsg.Add("proto生成成功");
            outPutProtoTextBox.Text = String.Join(Environment.NewLine, outputProtoLines.ToArray());
            GeneratorMsgTextBox.Text = String.Join(Environment.NewLine, outputMsg.ToArray());
        }


        private void generateAllProtoBtn_Click(object sender, EventArgs e)
        {
            string entityName = TValueTextBox.Text;
            GeneratorMsgTextBox.Clear();
            outPutProtoTextBox.Clear();
            outputMsg.Clear();
            if (String.IsNullOrWhiteSpace(entityName))
            {
                outputMsg.Add("TValue输入为空，无法生成全量proto");
                GeneratorMsgTextBox.Text = String.Join(Environment.NewLine, outputMsg.ToArray());
                return;
            }
             // 多层 List / Dictionary 嵌套 判定 
            int hitMulti = Regex.Matches(entityName, @"List\<Dictionary\<").Count +
                                   Regex.Matches(entityName, @"List\<List\<").Count +
                                   Regex.Matches(entityName, @"Dictionary\<Dictionary\<").Count +
                                   Regex.Matches(entityName, @"Dictionary\<List\<").Count +
                                   Regex.Matches(entityName, @"\>\>+").Count;
            if (hitMulti > 0)
            {
                outputMsg.Add("TValue:" + entityName + "：为多层 List / Dictionary 嵌套，请手动生成");
                GeneratorMsgTextBox.Text = String.Join(Environment.NewLine, outputMsg.ToArray());
                return;
            }

            List<String> resLines = GenerateTValueDic(entityName);
            StaticInfo.checkoutPath();
            string pathOutput = StaticInfo.outputPath + StaticInfo.javaPackageName.Replace(".", "/") + "/" + className + ".proto";
            fileIOBusiness.CreateFloder(StaticInfo.outputPath + StaticInfo.javaPackageName.Replace(".", "/") + "/");
            fileIOBusiness.WriteFile(resLines, pathOutput);
            
            GenerateJava(pathOutput);
            outputMsg.Add("proto生成结束");
            outPutProtoTextBox.Text = String.Join(Environment.NewLine, resLines.ToArray());
            GeneratorMsgTextBox.Text = String.Join(Environment.NewLine, outputMsg.ToArray());
        }

        private void generateIncProtoBtn_Click(object sender, EventArgs e)
        {
            string businessName = BusinessNameTextBox.Text;
            string tIncUpdateName = TIncUpdateTextBox.Text;
            string tIncDeleteName = TIncDeleteTextBox.Text;
            GeneratorMsgTextBox.Clear();
            outPutProtoTextBox.Clear();
            outputMsg.Clear();
            if (String.IsNullOrWhiteSpace(businessName))
            {
                outputMsg.Add("BusinessName输入为空，无法生成全量proto");
                GeneratorMsgTextBox.Text = String.Join(Environment.NewLine, outputMsg.ToArray());
                return;
            }
            else if (String.IsNullOrWhiteSpace(tIncDeleteName))
            {
                outputMsg.Add("TIncDelete输入为空，无法生成全量proto");
                GeneratorMsgTextBox.Text = String.Join(Environment.NewLine, outputMsg.ToArray());
                return;
            }
            else if (String.IsNullOrWhiteSpace(tIncUpdateName))
            {
                outputMsg.Add("tTIncUpdate输入为空，无法生成全量proto");
                GeneratorMsgTextBox.Text = String.Join(Environment.NewLine, outputMsg.ToArray());
                return;
            }

            List<String> resLines = GenerateDynamicInfo(businessName, tIncUpdateName, tIncDeleteName);
            StaticInfo.checkoutPath();
            string pathOutput = StaticInfo.outputPath + StaticInfo.javaPackageName.Replace(".", "/") + "/" + className + ".proto";
            fileIOBusiness.CreateFloder(StaticInfo.outputPath + StaticInfo.javaPackageName.Replace(".", "/") + "/");
            fileIOBusiness.WriteFile(resLines, pathOutput);

            GenerateJava(pathOutput);
            outputMsg.Add("proto生成结束");
             
            outPutProtoTextBox.Text = String.Join(Environment.NewLine, resLines.ToArray());
            GeneratorMsgTextBox.Text = String.Join(Environment.NewLine, outputMsg.ToArray());
        }

        /// <summary>
        /// 转换entity方法
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        private List<String> Convert(List<String> lines)
        {
            List<String> outPutLines = new List<string>();

            className = "";
            int isEnum = -1;
            List<String> importList = new List<string>();
            List<String> paraList = new List<string>();
            List<String> extraContentList = new List<string>();
            MatchCollection m;

            //遍历lines
            for (int i = 0; i < lines.Count(); ++i)
            {
                string line = lines[i];
                if (line.Trim().IndexOf("//") == 0)
                {
                    continue;
                }
                m = Regex.Matches(line, @"public\s*class\s*(\w*)\{?");
                if (m.Count == 1)
                {
                    className = m[0].Groups[1].Value;
                    isEnum = 0;
                    continue;
                }
                m = Regex.Matches(line, @"public\s*enum\s*(\w*)\{?");
                if (m.Count == 1)
                {
                   className = m[0].Groups[1].Value;
                   isEnum = 1;
                   continue;
                }
               
                //第一行不可能是变量，跳过
                if (i == 0)
                {
                    continue;
                }

                //分类讨论
                if (isEnum == 0)
                {
                    m = Regex.Matches(lines[i - 1], @"\[ProtoMember\((\w*?)\)\]");
                    if (m.Count != 1)
                    {
                        continue;
                    }
                    
                    int field = 0;          //生成的变量的field值
                    Int32.TryParse(m[0].Groups[1].Value, out field);
                    if (field == 0)
                    {
                        continue;
                    }

                    /*------------判断变量类型--------------- */
                    string type, paraName;
                    //声明形如 public int a;或 public int a = 3;
                    m = Regex.Matches(line, @"public\s*(\w*)\s*(\w*)");
                    if (m.Count != 1)
                    {
                        continue;
                    }
                    if (Regex.Matches(line, @"public\s*List").Count == 0 && Regex.Matches(line, @"public\s*Dictionary").Count == 0 && Regex.Matches(line, @"public\s*HashSet").Count == 0 ) {
                        type = m[0].Groups[1].Value;
                        paraName = m[0].Groups[2].Value;
                        paraList.Add("optional " + StaticInfo.getParaNameAndImport(type,ref importList) + " " + paraName + " = " + field.ToString() + ";");
                        continue;
                    }
                    //检查是否为多层嵌套
                    int hitMulti = Regex.Matches(line, @"public\s*List\<Dictionary\<").Count +
                                   Regex.Matches(line, @"public\s*List\<List\<").Count +
                                   Regex.Matches(line, @"public\s*Dictionary\<Dictionary\<").Count +
                                   Regex.Matches(line, @"public\s*Dictionary\<List\<").Count +
                                   Regex.Matches(line, @"\>\>\s*").Count;
                    if (hitMulti > 0)
                    {
                        outputMsg.Add(line + "：为多层List/Dictionary嵌套，请手工处理；");
                        continue;
                    }
                    //处理一层List
                    m = Regex.Matches(line, @"public\s*List\<(\w*)\>\s*(\w*)");
                    if (m.Count == 1)
                    {
                        type = m[0].Groups[1].Value;
                        paraName = m[0].Groups[2].Value;
                        paraList.Add("repeated " + StaticInfo.getParaNameAndImport(type, ref importList) + " " + paraName + " = " + field.ToString() + ";");
                        continue;
                    }

                    //处理一层HashSet
                    m = Regex.Matches(line, @"public\s*HashSet\<(\w*)\>\s*(\w*)");
                    if (m.Count == 1)
                    {
                        type = m[0].Groups[1].Value;
                        paraName = m[0].Groups[2].Value;
                        paraList.Add("repeated " + StaticInfo.getParaNameAndImport(type, ref importList) + " " + paraName + " = " + field.ToString() + ";");
                        continue;
                    }
                    //Dictionary处理
                    m = Regex.Matches(line, @"public\s*Dictionary\<(\w*),(\w*)\>\s*(\w*)");
                    if (m.Count == 1)
                    {
                        //处理一层List
                        string keyPara = StaticInfo.getParaNameAndImport(m[0].Groups[1].Value,ref importList);
                        string valuePara = StaticInfo.getParaNameAndImport(m[0].Groups[2].Value,ref importList);
                        paraName = m[0].Groups[3].Value;
                        extraContentList.Add("");
                        if (valuePara == "int32")
                        {
                            extraContentList.Add("message KVForDictionaryForInt");
                            paraList.Add("repeated KVForDictionaryForInt " + paraName + " = " + field.ToString() + ";");
                        }
                        else if (valuePara == "int64")
                        {
                            extraContentList.Add("message KVForDictionaryForInt");
                            paraList.Add("repeated KVForDictionaryForLong " + paraName + " = " + field.ToString() + ";");
                        }
                        else {
                            extraContentList.Add("message KVForDictionaryFor" + toFirstLetterUpper(valuePara));
                            paraList.Add("repeated KVForDictionaryFor" + toFirstLetterUpper(valuePara) + " = " + field.ToString() + ";");
                        }
                        extraContentList.Add("{");
                        extraContentList.Add("optional " + keyPara + " Key = 1;");
                        extraContentList.Add("optional " + valuePara + " Value = 2;");
                        extraContentList.Add("}");
                        continue;
                    }
                }
                if (isEnum == 1)
                {
                    if (lines[i - 1].Contains("[ProtoEnum]"))
                    {
                        if (line.Contains(","))
                        {
                            paraList.Add(line.Replace(",", ";"));
                        }
                        else
                        {
                            paraList.Add(line + ";");
                        }
                        continue;
                    }
                }
            }

            //生成outlines
            outPutLines.Add("syntax = \"proto2\";");
            outPutLines.Add("package tmp;");
            outPutLines.Add("");
            outPutLines.Add("option java_package = \"" + StaticInfo.javaPackageName + "\";");
            outPutLines.Add("option java_outer_classname = \"PB" + className + "\";");
            outPutLines.Add("");

            if (isEnum == 0)
            {
                outPutLines.AddRange(importList);
                outPutLines.Add("");
                outPutLines.Add("message " + className);
                outPutLines.Add("{");
                outPutLines.AddRange(paraList);
                outPutLines.Add("}");
                outPutLines.AddRange(extraContentList);
                outPutLines.Add("");
            }
            else if (isEnum == 1)
            {
                outPutLines.Add("enum " + className);
                outPutLines.Add("");
                outPutLines.Add("{");
                outPutLines.AddRange(paraList);
                outPutLines.Add("}");
                outPutLines.Add("");
            }
            return outPutLines;
        }


        private List<String> GenerateTValueDic(string entityName)
        {
            //判定类型
            /* 1 : 实体 Entity，
             * 2 : 一层 List<T>
             * 3 : 一层 Dictionary<T,T>
             */
            int type = 0;
            string fileName = "";
            List<String> outPutLines = new List<string>();
            List<String> importLines = new List<string>();
            MatchCollection m_List = Regex.Matches(entityName, @"\s*?List\<(\w*)\>\s*?");
            MatchCollection m_Hashset = Regex.Matches(entityName, @"\s*?HashSet\<(\w*)\>\s*?");
            MatchCollection m_Dictionary = Regex.Matches(entityName, @"\s*?Dictionary\<(\w*),(\w*)\>\s*?");
            if (m_List.Count == 1)
            {
                fileName = toFirstLetterUpper(m_List[0].Groups[1].Value) + "List";
                StaticInfo.getParaNameAndImport(m_List[0].Groups[1].Value,ref importLines);
                type = 2;
            }
            else if (m_Hashset.Count == 1)
            {
                fileName = toFirstLetterUpper(m_Hashset[0].Groups[1].Value) + "List";
                StaticInfo.getParaNameAndImport(m_Hashset[0].Groups[1].Value, ref importLines);
                type = 4;
            }
            else if (m_Dictionary.Count == 1)
            {
                StaticInfo.getParaNameAndImport(m_Dictionary[0].Groups[1].Value, ref importLines);
                StaticInfo.getParaNameAndImport(m_Dictionary[0].Groups[2].Value, ref importLines);
                fileName = "DictionaryFor" + toFirstLetterUpper(m_Dictionary[0].Groups[2].Value);
                type = 3;
            }
            else
            {
                fileName = toFirstLetterUpper(entityName);
                StaticInfo.getParaNameAndImport(entityName, ref importLines);
                type = 1;
            }
            className = "DictionaryFor" + fileName;
            //初始化头部
            outPutLines.Add("syntax = \"proto2\";");
            outPutLines.Add("package tmp;");
            outPutLines.Add("");
            outPutLines.Add("option java_package = \"" + StaticInfo.javaPackageName + "\";");
            outPutLines.Add("option java_outer_classname = \"PBDictionaryFor" + fileName + "\";");
            outPutLines.Add("");
            if (importLines.Count > 0)
            {
                outPutLines.AddRange(importLines.Distinct());
                outPutLines.Add("");
            }
            outPutLines.Add("message DictionaryFor" + fileName);
            outPutLines.Add("{");
            outPutLines.Add("repeated KVForDictionaryFor" + fileName + " KeyValuePair = 1;");
            outPutLines.Add("}");
            outPutLines.Add("");
            outPutLines.Add("message KVForDictionaryFor" + fileName);
            outPutLines.Add("{");

            outPutLines.Add("optional int32 Key = 1;");
            //Entity与EntityList较为类似，放在一起生成
            if (type == 1)
            {
                outPutLines.Add("optional " + StaticInfo.getParaName(entityName) + " Value = 2;");
            }
            else if (type == 2)
            {
                outPutLines.Add("repeated " + StaticInfo.getParaName(m_List[0].Groups[1].Value) + " Value = 2;");
            }
            else if (type == 4)
            {
                outPutLines.Add("repeated " + StaticInfo.getParaName(m_Hashset[0].Groups[1].Value) + " Value = 2;");
            }
            else if (type == 3)
            {
                outPutLines.Add("repeated KVFor" + fileName + " Value = 2;");
                outPutLines.Add("}");
                outPutLines.Add("");
                outPutLines.Add("message KVFor" + fileName);
                outPutLines.Add("{");
                outPutLines.Add("optional " + StaticInfo.getParaName(m_Dictionary[0].Groups[1].Value) + " Key = 1;");
                outPutLines.Add("optional " + StaticInfo.getParaName(m_Dictionary[0].Groups[2].Value) + " Value = 2;");
            }
            outPutLines.Add("}");
            return outPutLines;
        }


        private List<String> GenerateDynamicInfo(string businessName, string tIncUpdateName, string tIncDeleteName)
        {
            List<String> outPutLines = new List<string>();
            List<String> tmpLines = new List<string>();
            List<String> importLines = new List<string>();
            
            int updateType = checkType(tIncUpdateName);
            int delType = checkType(tIncDeleteName);
            //tIncDeleteName 共 4 种合法输入
            if (delType == 4)
            {
                outputMsg.Add("TIncDelete类型为多层嵌套，请手动生成proto！");
                return outPutLines;
            }
            else if (delType == -1)
            {
                outputMsg.Add("TIncDelete类型为无法识别，请手动生成proto！");
                return outPutLines;
            }
            //TIncUpdate 共 3 种合法输入
            if (updateType == 4 || updateType == 5)
            {
                outputMsg.Add("TIncUpdate类型为多层嵌套，请手动生成proto！");
                return outPutLines;
            }
            else if (updateType == -1)
            {
                outputMsg.Add("TIncUpdate类型为无法识别，请手动生成proto！");
                return outPutLines; 
            }

            #region tIncDeleteName 处理
            string tIncDeleteParaName = "";
            if (delType == 1)
            {
                tIncDeleteParaName = StaticInfo.getParaNameAndImport(tIncDeleteName,ref importLines);
            }
            else if (delType == 2)
            {
                MatchCollection m = Regex.Matches(tIncDeleteName,@"\s*?List\<(\w*)\>\s*?");
                tIncDeleteParaName = StaticInfo.getParaNameAndImport(m[0].Groups[1].Value,ref importLines);
            }
            else if (delType == 3)
            {
                MatchCollection m = Regex.Matches(tIncDeleteName, @"\s*?Dictionary\<(\w*),(\w*)\>\s*?");
                tmpLines.Add("message KVForDictionaryFor" + toFirstLetterUpper(m[0].Groups[2].Value));
                tmpLines.Add("{");
                tmpLines.Add("optional " + StaticInfo.getParaNameAndImport(m[0].Groups[1].Value, ref importLines) + " Key = 1;");
                tmpLines.Add("optional " + StaticInfo.getParaNameAndImport(m[0].Groups[2].Value, ref importLines) + " Value = 2;");
                tmpLines.Add("}");
                tmpLines.Add("");
                tIncDeleteParaName = "KVForDictionaryFor" + toFirstLetterUpper(m[0].Groups[2].Value);
            }
            else if (delType == 5)
            {
                MatchCollection m = Regex.Matches(tIncDeleteName,  @"\s*?Dictionary\s*?\<(\w*)\s*?,\s*?List\<\s*?(\w*)\s*?\>\s*?\>\s*?");
                tmpLines.Add("message KVForDictionaryFor"+toFirstLetterUpper(m[0].Groups[2].Value) + "List");
                tmpLines.Add("{");
                tmpLines.Add("optional " + StaticInfo.getParaNameAndImport(m[0].Groups[1].Value, ref importLines) + " Key = 1;");
                tmpLines.Add("repeated " + StaticInfo.getParaNameAndImport(m[0].Groups[2].Value, ref importLines) + " Value = 2;");
                tmpLines.Add("}");
                tmpLines.Add("");
                tIncDeleteParaName = "KVForDictionaryFor"+toFirstLetterUpper(m[0].Groups[2].Value) + "List";
            }

            #endregion
            #region  tIncUpdateName 处理
            //3种情况
            tmpLines.Add("message KVForDicAddOrUpdateInfoFor" + toFirstLetterUpper(businessName));
            tmpLines.Add("{");
            tmpLines.Add("optional int32 Key = 1;");
            if (updateType == 1)
            {
                tmpLines.Add("optional " + StaticInfo.getParaNameAndImport(tIncUpdateName, ref importLines) + " Value = 2;");
                tmpLines.Add("}");
                tmpLines.Add("");
            }
            else if(updateType == 2)
            {
                MatchCollection m = Regex.Matches(tIncUpdateName , @"\s*?List\<(\w*)\>\s*?");
                tmpLines.Add("repeated " + StaticInfo.getParaNameAndImport(m[0].Groups[1].Value, ref importLines) + " Value = 2;");
                tmpLines.Add("}");
                tmpLines.Add("");
            }
            else if (updateType == 6)
            {
                MatchCollection m = Regex.Matches(tIncUpdateName, @"\s*?HashSet\<(\w*)\>\s*?");
                tmpLines.Add("repeated " + StaticInfo.getParaNameAndImport(m[0].Groups[1].Value, ref importLines) + " Value = 2;");
                tmpLines.Add("}");
                tmpLines.Add("");
            }
            else if(updateType == 3)
            {
                MatchCollection m1 = Regex.Matches(tIncUpdateName, @"\s*?Dictionary\<(\w*),(\w*)\>\s*?");
                MatchCollection m2 = Regex.Matches(tIncDeleteName, @"\s*?Dictionary\<(\w*),(\w*)\>\s*?");
                tmpLines.Add("repeated " + "KVForDictionaryFor" + m1[0].Groups[2].Value + " Value = 2;");
                tmpLines.Add("}");
                tmpLines.Add("");
                
                tmpLines.Add("message KVForDictionaryFor" + toFirstLetterUpper(m1[0].Groups[2].Value));
                tmpLines.Add("{");
                tmpLines.Add("optional " + StaticInfo.getParaNameAndImport(m1[0].Groups[1].Value, ref importLines) + " Key = 1;");
                tmpLines.Add("optional " + StaticInfo.getParaNameAndImport(m1[0].Groups[2].Value, ref importLines) + " Value = 2;");
                tmpLines.Add("}");
                tmpLines.Add("");
                
            }
            #endregion

            //初始化头部
            outPutLines.Add("syntax = \"proto2\";");
            outPutLines.Add("package tmp;");
            outPutLines.Add("");
            outPutLines.Add("option java_package = \"" + StaticInfo.javaPackageName + "\";");
            outPutLines.Add("option java_outer_classname = \"PBDynamicIncreaseInfoFor" + toFirstLetterUpper(businessName) + "\";");
            outPutLines.Add("");
            className = "DynamicIncreaseInfoFor" + toFirstLetterUpper(businessName);
            if (importLines.Count > 0)
            {
                outPutLines.AddRange(importLines.Distinct());
                outPutLines.Add("");
            }
            outPutLines.Add("message DynamicIncreaseInfoFor" + toFirstLetterUpper(businessName));
            outPutLines.Add("{");
            if (delType == 1)
            {
                outPutLines.Add("optional " + tIncDeleteParaName + " DicDeleteInfo = 1;");
            }
            else if (delType == 2 || delType == 3 || delType == 5)
            {
                outPutLines.Add("repeated " + tIncDeleteParaName + " DicDeleteInfo = 1;");
            }
            outPutLines.Add("repeated KVForDicAddOrUpdateInfoFor" + toFirstLetterUpper(businessName) + " DicAddOrUpdateInfo = 2;");
            outPutLines.Add("}");
            outPutLines.Add("");
            outPutLines.AddRange(tmpLines);
            return outPutLines;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paraName"></param>
        /// <returns>
        /// -1 : 不可判定 ； 
        /// 1 ： 一般实体类 
        /// 2：List<T> 
        /// 3.Dictionary<key,value>
        /// 4: 多层嵌套 
        /// 5:(特殊)Dictionary<T,List<T>> </returns>
        private int checkType(string paraName)
        {
            int hitMulti = Regex.Matches(paraName, @"List\<Dictionary\<").Count +
                                  Regex.Matches(paraName, @"List\<List\<").Count +
                                  Regex.Matches(paraName, @"Dictionary\<Dictionary\<").Count +
                                  Regex.Matches(paraName, @"Dictionary\<List\<").Count +
                                  Regex.Matches(paraName, @"\>\>+").Count;
            if (hitMulti > 0)
            {
                if (Regex.Matches(paraName, @"\s*?Dictionary\s*?\<(\w*)\s*?,\s*?List\<\s*?(\w*)\s*?\>\s*?\>\s*?").Count == 1)
                {
                    return 5;
                }
                return 4;
            }
            if (Regex.Matches(paraName, @"\s*?List\<(\w*)\>\s*?").Count == 1)
            {
                return 2;
            }
            else if (Regex.Matches(paraName, @"\s*?Dictionary\<(\w*),(\w*)\>\s*?").Count == 1)
            {
                return 3;
            }
            if (Regex.Matches(paraName, @"\s*?HashSet\<(\w*)\>\s*?").Count == 1)
            {
                return 6;
            }
            else if (Regex.Matches(paraName, @"^[\u4E00-\u9FA5A-Za-z0-9_]+").Count == 1)
            {
                return 1;
            }
            return -1;
        }
        public string RunCmd(string cmd)
        {
            string output = "";
            cmd = cmd.Trim().TrimEnd('&') + "&exit";//说明：不管命令是否成功均执行exit命令，否则当调用ReadToEnd()方法时，会处于假死状态
            using (Process p = new Process())
            {
                p.StartInfo.FileName = StaticInfo.cmdPath;
                p.StartInfo.UseShellExecute = false;        //是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true;   //接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;  //由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true;   //重定向标准错误输出
                p.StartInfo.CreateNoWindow = true;          //不显示程序窗口
                p.Start();//启动程序

                //向cmd窗口写入命令
                p.StandardInput.WriteLine(cmd);
                p.StandardInput.AutoFlush = true;

                //获取cmd窗口的输出信息
                output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();//等待程序执行完退出进程
                p.Close();
            }
            return output;
        }

        private void GenerateJava(string sourceFileName) {
            //如果要生成java
            if (AutoGenerateJavaCheckBox.Enabled)
            {
                //查找对应目录下所有import的proto文件
                //判断proto生成的时候是否有报错信息
                if (outputMsg.Count > 0)
                {
                    outputMsg.Add("proto生成出错，暂停java生成");
                }
                else
                {

                    if (StaticInfo.outputPath[StaticInfo.outputPath.Count() - 1] != '/' && StaticInfo.generateorPath[StaticInfo.generateorPath.Count() - 1] != '\\')
                    {
                        StaticInfo.outputPath += '/';
                    }
                    if (StaticInfo.generateorPath[StaticInfo.generateorPath.Count() - 1] != '/' && StaticInfo.generateorPath[StaticInfo.generateorPath.Count() - 1] != '\\')
                    {
                        StaticInfo.generateorPath += '/';
                    }
                    //生成相对应的cmd 命令
                    string cmdRequest = StaticInfo.generateorPath + "protoc" + StaticInfo.protobufVersion + "  " + sourceFileName + "  --proto_path=" + StaticInfo.generateorPath + " --java_out=" + StaticInfo.outputPath;
                    string cmdRes = RunCmd(cmdRequest);
                }
            }
        }

        private void RefreshBtn_Click(object sender, EventArgs e)
        {
            refreshPage();
        }

        private void refreshPage()
        {
            outputPathLabel.Text = StaticInfo.outputPath;
            javaPackageNameTextBox.Text = StaticInfo.javaPackageName;
            ProtoProgramPathLabel.Text = StaticInfo.generateorPath;
            protoVersionTextBox.Text = StaticInfo.protobufVersion;
            if (generateJava)
            {
                AutoGenerateJavaCheckBox.Checked = true;
            }
            else
            {
                AutoGenerateJavaCheckBox.Checked = false;
            }
            if (javaPackageNameCanEdit)
            {
                javaPackageNameTextBox.Enabled = true;
            }
            else
            {
                javaPackageNameTextBox.Enabled = false;
            }
            if (protobufVersionCanEdit)
            {
                protoVersionTextBox.Enabled = true;
            }
            else
            {
                protoVersionTextBox.Enabled = false;
            }

        }

        private void tableLayoutPanel10_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, tableLayoutPanel10.ClientRectangle,
            Color.Black, 1, ButtonBorderStyle.Solid, //左边
            Color.Black, 1, ButtonBorderStyle.Solid, //上边
            Color.Black, 1, ButtonBorderStyle.Solid, //右边
            Color.Black, 1, ButtonBorderStyle.Solid);//底边
        }

        private void tableLayoutPanel8_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, tableLayoutPanel8.ClientRectangle,
           Color.Black, 1, ButtonBorderStyle.Solid, //左边
           Color.Black, 1, ButtonBorderStyle.Solid, //上边
           Color.Black, 1, ButtonBorderStyle.Solid, //右边
           Color.Black, 1, ButtonBorderStyle.Solid);//底边
        }

        private void tableLayoutPanel7_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, tableLayoutPanel7.ClientRectangle,
           Color.Black, 1, ButtonBorderStyle.Solid, //左边
           Color.Black, 1, ButtonBorderStyle.Solid, //上边
           Color.Black, 1, ButtonBorderStyle.Solid, //右边
           Color.Black, 1, ButtonBorderStyle.Solid);//底边
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void ExtraToolBtn_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Show();
        }

           
    }
}
