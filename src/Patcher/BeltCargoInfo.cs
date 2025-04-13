using System;
using System.Collections.Generic;
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
            cargoLimit[itemId] = limit;
            cargoCount[itemId] = 0;
            RefreshItemLimit();
        }

        public void RemoveLimit(int itemId)
        {
            if(cargoLimit.ContainsKey(itemId))
                cargoLimit.Remove(itemId);
            if(cargoCount.ContainsKey(itemId))
                cargoCount.Remove(itemId);
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

            int totalCargoCount = 0;
        }

        public bool CanInsert(int itemId)
        {
            if (!enabled) return true;
            return cargoLimit.ContainsKey(itemId) && cargoCount.ContainsKey(itemId) && cargoCount[itemId] < cargoLimit[itemId];
        }

        public static void RecalcCargos()
        {

        }
    }
}
