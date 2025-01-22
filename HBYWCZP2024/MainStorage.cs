using HBYWCZP2024.Models;
using LinxUniverse.Utils;
using Org.BouncyCastle.Bcpg.Sig;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024
{
    public static class MainStorage
    {
        public static SaveManager<WcsSaves> SaveManager = new();
        public static WcsSaves Saves => SaveManager.SaveFile;
        public static Map<string, string> ErrorTable = [];
        public static void Init()
        {
            SaveManager.Load();
            
            try
            {
                if (File.Exists("ErrorTable.csv"))
                {
                    var lines = File.ReadAllLines("ErrorTable.csv");
                    ErrorTable = lines.Map(line =>
                    {
                        var parts = line.Split('\t');
                        return (parts[0], parts[1]);
                    }).ToMap();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("初始化error表出错");
                
            }
        }

        public static string GetError(string code)
        {
            if (ErrorTable.ContainsKey(code))
            {
                return $"{ErrorTable[code]}: {code}";
            }
            return $"未知错误: {code}";
        }
    }
}
