using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixCargoController
{
    public class BeltCargoInfo
    {
        public bool enabled;
        //public int planetId;
        //public int pathIndex;
        //public Dictionary<int, int> cargoRatio;
        public Dictionary<int, int> cargoLimit;
        public Dictionary<int, int> cargoCount;
        public BeltCargoInfo() 
        {
            enabled = false;
            //planetId = 0;
            //pathIndex = 0;
            //cargoRatio = new Dictionary<int, int>();
            cargoLimit = new Dictionary<int, int>();
            cargoCount = new Dictionary<int, int>();
        }

        public void AddOrUpdateLimit(int itemId, int limit)
        {
            lock (cargoLimit)
            {
                cargoLimit[itemId] = limit;
            }
            lock (cargoCount)
            {
                cargoCount[itemId] = 0;
            }
            RefreshItemLimit();
        }

        public void RemoveLimit(int itemId)
        {
            lock (cargoLimit)
            {
                if (cargoLimit.ContainsKey(itemId))
                    cargoLimit.Remove(itemId);
            }
            lock (cargoCount)
            {
                if (cargoCount.ContainsKey(itemId))
                    cargoCount.Remove(itemId);
            }
            RefreshItemLimit();
        }

        public void RefreshItemLimit()
        {
            enabled = false;
            foreach (var pair in cargoLimit)
            {
                if (pair.Value > 0)
                {
                    enabled = true;
                    return;
                }
            }
        }

        public bool CanInsert(int itemId)
        {
            if (!enabled) return true;
            return cargoLimit.ContainsKey(itemId) && cargoCount.ContainsKey(itemId) && cargoCount[itemId] < cargoLimit[itemId];
        }

        public void Export(BinaryWriter w)
        {
            w.Write(cargoLimit.Count);
            foreach (var item in cargoLimit)
            {
                w.Write(item.Key);
                w.Write(item.Value);
            }
            w.Write(cargoCount.Count);
            foreach(var item in cargoCount)
            {
                w.Write(item.Key);
                w.Write(item.Value);
            }
        }

        public void Import(BinaryReader r)
        {
            int count1 = r.ReadInt32();
            for (int i = 0; i < count1; i++)
            {
                cargoLimit[r.ReadInt32()] = r.ReadInt32();
            }
            int count2 = r.ReadInt32();
            for (int i = 0; i < count2; i++)
            {
                cargoCount[r.ReadInt32()] = r.ReadInt32();
            }
            RefreshItemLimit();
        }

    }
}
