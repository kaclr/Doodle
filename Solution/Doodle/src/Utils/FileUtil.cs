using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Doodle
{
    public static class FileUtil
    {
        public static bool Exists(string path)
        {
            return PathUtil.GetPathState(path) == PathState.File;
        }

        public static bool TryCreateFile(string path, string content)
        {
            if (Exists(path))
            {
                return false;
            }

            var sw = File.CreateText(path);
            sw.Write(content);
            sw.Close();
            return true;
        }

        public static string ComputeSHA1(string path)
        {
            if (!File.Exists(path)) throw new NotFileException(path, nameof(path));

            string hashSHA1 = string.Empty;
            //检查文件是否存在，如果文件存在则进行计算，否则返回空值

            using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                //计算文件的SHA1值
                System.Security.Cryptography.SHA1 calculator = System.Security.Cryptography.SHA1.Create();
                Byte[] buffer = calculator.ComputeHash(fs);
                calculator.Clear();
                //将字节数组转换成十六进制的字符串形式
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < buffer.Length; i++)
                {
                    stringBuilder.Append(buffer[i].ToString("x2"));
                }
                hashSHA1 = stringBuilder.ToString();
            }//关闭文件流

            return hashSHA1;
        }
    }
}
