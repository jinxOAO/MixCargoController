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
            int totalCount = r.ReadInt32();
            for (int i = 0; i < totalCount; i++)
            {
                int planetId = r.ReadInt32();
                int count = r.ReadInt32();
                infos[planetId] = new ConcurrentDictionary<int, BeltCargoInfo>();
                for(int j = 0; j < count; j++)
                {
                    int segPathId = r.ReadInt32();
                    BeltCargoInfo info = new BeltCargoInfo();
                    infos[planetId][segPathId] = info;
                    info.Import(r);
                }
            }
        }

        public static void Export(BinaryWriter w) 
        {
            w.Write(infos.Count);
            foreach (var planetInfoDatas in infos) 
            {
                w.Write(planetInfoDatas.Key);
                w.Write(planetInfoDatas.Value.Count);
                foreach(var pair in planetInfoDatas.Value)
                {
                    w.Write(pair.Key);
                    pair.Value.Export(w);
                }
            }
        }

        public static void IntoOtherSave()
        {
            Init();
        }

        //public static void RemoveAllUnboundData()
        //{
        //    foreach (var planetInfoDatas in infos)
        //    {
        //        int planetId = planetInfoDatas.Key;
        //        PlanetFactory factory = GameMain.galaxy.PlanetById(planetId)?.factory;
        //        if(factory == null)
        //        {
        //            infos.TryRemove(planetId, out _);
        //        }
        //        else
        //        {
        //            foreach (var item in planetInfoDatas.Value)
        //            {
        //                int segPathId = item.Key;
        //            }
        //        }
        //    }
        //}


    }
}
