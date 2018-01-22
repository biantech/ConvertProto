using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertProto
{
    public static class StaticInfo
    {
        //cmd路径
        public static string cmdPath = @"C:\Windows\System32\cmd.exe";
        //输出目录
        public static string outputPath = @"F:/pb/out/";

        public static void checkoutPath()
        {
            if (String.IsNullOrWhiteSpace(outputPath))
            {
                outputPath = @"F:/pb/out/";
            }
            if (outputPath[outputPath.Count() - 1] != '/' && outputPath[StaticInfo.outputPath.Count() - 1] != '\\')
            {
                outputPath += '/';
            }
        }



        //java 包名
        public static string javaPackageName = "com.ctrip.hotel.search.internalentity.pb";
        //protobuf exe程序位置
        public static string generateorPath = @"F:/pb/";
        public static void checkoutPath2()
        {
            if (String.IsNullOrWhiteSpace(outputPath))
            {
                generateorPath = @"F:/pb/";
            }
            if (outputPath[outputPath.Count() - 1] != '/' && outputPath[StaticInfo.outputPath.Count() - 1] != '\\')
            {
                outputPath += '/';
            }
        }
        //ptotobuf 版本号
        public static string protobufVersion = "2.6.1";

        //转换变量名对应proto变量名字典
        public static Dictionary<string, string> paraDic = new Dictionary<string, string>();

        /// <summary>
        /// 初始化变量名对照字典 
        /// </summary>
        public static void initialDictionary()
        {
            //清理
            paraDic.Clear();

            paraDic.Add("int", "int32");
            paraDic.Add("Int16", "int32");
            paraDic.Add("Int32", "int32");
            paraDic.Add("byte", "int32");
            paraDic.Add("short", "int32");

            paraDic.Add("long", "int64");
            paraDic.Add("Int64", "int64");

            paraDic.Add("string", "string");
            paraDic.Add("String", "string");
            paraDic.Add("DateTime", "string");

            paraDic.Add("bool", "bool");
            paraDic.Add("Boolean", "bool");
            paraDic.Add("double", "double");
            paraDic.Add("Double", "double");
            paraDic.Add("float", "float");
            paraDic.Add("decimal", "decimal");
            paraDic.Add("Decimal", "decimal");
        }


        /// <summary>
        /// 获取对应的proto变量名 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string getParaName(string name)
        {
            if (paraDic.ContainsKey(name))
            {
                return paraDic[name];
            }
            return name;
        }

        /// <summary>
        /// 获取对应的proto变量名 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string getParaNameAndImport(string name, ref List<String> importLines)
        {
            if (paraDic.ContainsKey(name))
            {
                return paraDic[name];
            }
            importLines.Add("import \"" + name + ".proto\";");
            return name;
        }

    }
}
