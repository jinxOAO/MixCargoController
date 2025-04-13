using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixCargoController
{
    public class RuntimeData
    {
        public static ConcurrentDictionary<int, ConcurrentDictionary<int, BeltCargoInfo>> infos;

        public static void Init()
        {
            infos = new ConcurrentDictionary<int, ConcurrentDictionary<int, BeltCargoInfo>>();
        }

        public static void Import(BinaryReader r)
        {
            Init();
        }

        public static void Export(BinaryWriter w) 
        {
            
        }

        public static void IntoOtherSave()
        {
            Init();
        }
    }
}
