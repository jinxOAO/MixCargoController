using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixCargoController
{
    public class PathChangePatcher
    {
        /// <summary>
        /// 新创建的传送带path删除所有设定
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="__result"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CargoTraffic), "NewCargoPath")]
        public static void OnNewCargoPathCreated(ref CargoTraffic __instance, ref CargoPath __result)
        {
            int id = __result.id;
            int planetId = __instance.factory.planetId;
            if(RuntimeData.infos.ContainsKey(planetId))
            {
                RuntimeData.infos[planetId].TryRemove(id, out _);
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(CargoTraffic), "RemoveCargoPath")]
        public static void OnNewCargoPathCreated(ref CargoTraffic __instance, int id)
        {
            int planetId = __instance.factory.planetId;
            if (RuntimeData.infos.ContainsKey(planetId))
            {
                RuntimeData.infos[planetId].TryRemove(id, out _);
            }
        }
    }
}
