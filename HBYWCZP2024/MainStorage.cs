using HBYWCZP2024.Models;
using LinxUniverse.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HBYWCZP2024
{
    public static class MainStorage
    {
        public static SaveManager<WcsSaves> SaveManager = new();
        public static WcsSaves Saves => SaveManager.SaveFile;
        public static void Init()
        {
            SaveManager.Load();
        }
    }
}
