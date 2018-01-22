using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertProto
{
    public class FileIOBusiness
    {
        ///<summary>
        ///逐行读文件
        ///</summary>
        ///
        public List<String> ReadFile(string filePath)
        {
            String line = String.Empty;
            List<string> lines = new List<string>();
            if (!System.IO.File.Exists(filePath))
            {
                return new List<string>();
            }
            System.IO.StreamReader file = new System.IO.StreamReader(filePath);
            while ((line = file.ReadLine()) != null)
            {

                lines.Add(line);
            }
            file.Close();
            return lines;
        }

        ///<summary>
        ///写文件(覆盖)
        ///</summary>
        ///
        public void WriteFile(List<String> lines, string fileName)
        {

            if (!System.IO.File.Exists(fileName))
            {
                using (System.IO.FileStream fs = System.IO.File.Create(fileName))
                {
                    fs.Close();
                }
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName))
                {
                    foreach (string line in lines)
                    {
                        file.WriteLine(line);
                    }
                }
            }
            else
            {
                try
                {
                    System.IO.File.Delete(fileName);
                }
                catch (System.IO.IOException e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
                try
                {
                    using (System.IO.FileStream fs = System.IO.File.Create(fileName))
                    {

                    }

                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName))
                    {
                        foreach (string line in lines)
                        {
                            file.WriteLine(line);
                        }
                    }
                }
                catch (System.IO.IOException e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }

            }
        }
        ///<summary>
        ///创建文件夹
        ///</summary>
        ///
        public void CreateFloder(string floderName)
        {
            if (floderName[floderName.Count() - 1] == '/')
            {
                floderName = floderName.Substring(0, floderName.Count() - 1);
            }
            try
            {
                if (!System.IO.Directory.Exists(floderName))
                {
                    System.IO.Directory.CreateDirectory(floderName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
