using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace MixCargoController
{
    public static class ParametersPatcher
    {
        /// <summary>
        /// 手动复制粘贴
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="objectId"></param>
        /// <param name="factory"></param>
        /// <param name="copyInserters"></param>
        /// <param name="copyAllSettings"></param>
        /// <param name="__result"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BuildingParameters), "CopyFromFactoryObject")]
        public static void CopyFromFactoryObjectPostfix(ref BuildingParameters __instance, int objectId, PlanetFactory factory, bool copyInserters , bool copyAllSettings, ref bool __result)
        {
            if (objectId > 0)
            {
                EntityData[] entityPool = factory.entityPool;
                int beltId = entityPool[objectId].beltId;
                if (beltId > 0)
                {
                    BeltComponent beltComponent = factory.cargoTraffic.beltPool[beltId];
                    if (beltComponent.id == beltId)
                    {
                        int pathId = beltComponent.segPathId;
                        if(RuntimeData.HasRules(factory.planetId, pathId))
                        {
                            int[] ori = __instance.parameters;
                            __instance.parameters = new int[2 + 2 * RuntimeData.infos[factory.planetId][pathId].cargoLimit.Count];

                            if (ori != null)
                            {
                                Array.Copy(ori, __instance.parameters, ori.Length);
                            }
                            else
                            {
                                __instance.parameters[0] = 0;
                                __instance.parameters[1] = 0;
                            }

                            int i = 1;
                            foreach (var item in RuntimeData.infos[factory.planetId][pathId].cargoLimit)
                            {
                                __instance.parameters[2 * i] = item.Key;
                                __instance.parameters[2*i + 1] = item.Value;
                                i++;
                            }
                            __result = true;
                        }
                    }
                }
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(BuildingParameters), "PasteToFactoryObject")]
        public static void PasteToFactoryObjectPostfix(ref BuildingParameters __instance, int objectId, PlanetFactory factory, ref bool __result)
        {

            EntityData[] entityPool = factory.entityPool;
            if (objectId > 0 && entityPool[objectId].id == objectId)
            {
                int beltId = entityPool[objectId].beltId;
                if (beltId != 0 && __instance.type == BuildingType.Belt )
                {
                    int planetId = factory.planetId;
                    BeltComponent beltComponent = factory.cargoTraffic.beltPool[beltId];
                    if (beltComponent.id == beltId)
                    {
                        int pathId = factory.cargoTraffic.beltPool[beltId].segPathId;
                        if (__instance.parameters != null && __instance.parameters.Length > 2) // 说明源线路有设定复制过来了，则修改目标线路的设定
                        {
                            if (!RuntimeData.infos.ContainsKey(planetId))
                                RuntimeData.infos[planetId] = new System.Collections.Concurrent.ConcurrentDictionary<int, BeltCargoInfo>();

                            RuntimeData.infos[planetId][pathId] = new BeltCargoInfo();

                            for (int i = 2; i + 1 < __instance.parameters.Length; i += 2)
                            {
                                RuntimeData.infos[planetId][pathId].AddOrUpdateLimit(__instance.parameters[i], __instance.parameters[i + 1]);
                            }
                            RecalcCargoCurCount(factory, pathId);
                        }
                        else // 说明复制过来的没有混带数据，那么取消目标线路的混带限制设定
                        {
                            if (RuntimeData.infos.ContainsKey(planetId))
                                RuntimeData.infos[planetId][pathId] = new BeltCargoInfo();
                        }
                        __result = true;
                        UIBeltWindowPatcher.RefreshAll();
                    }
                    
                }
            }
        }

        /// <summary>
        /// 蓝图粘贴等prebuild变为实体时执行
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="parameters"></param>
        /// <param name="factory"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BuildingParameters), "ApplyPrebuildParametersToEntity")]
        public static void ApplyPrebuildParametersToEntity(int entityId, int[] parameters, PlanetFactory factory)
        {
            EntityData[] entityPool = factory.entityPool;
            if (entityId > 0 && entityPool[entityId].id == entityId)
            {
                int beltId = entityPool[entityId].beltId;
                if (beltId != 0 && parameters != null && parameters.Length > 2) 
                {
                    int planetId = factory.planetId;
                    BeltComponent beltComponent = factory.cargoTraffic.beltPool[beltId];
                    if (beltComponent.id == beltId)
                    {
                        int pathId = factory.cargoTraffic.beltPool[beltId].segPathId;
                        if (!RuntimeData.infos.ContainsKey(planetId))
                            RuntimeData.infos[planetId] = new System.Collections.Concurrent.ConcurrentDictionary<int, BeltCargoInfo>();

                        RuntimeData.infos[planetId][pathId] = new BeltCargoInfo();

                        for (int i = 2; i + 1 < parameters.Length; i += 2)
                        {
                            RuntimeData.infos[planetId][pathId].AddOrUpdateLimit(parameters[i], parameters[i + 1]);
                        }
                        RecalcCargoCurCount(factory, pathId);
                    }

                }
            }
        }


        /// <summary>
        /// 蓝图创建过程中，还会有一次复制param会只管param的有限长度（2），要注意patch这个
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="_parameters"></param>
        /// <param name="_paramCount"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BuildingParameters), "ToParamsArray")]
        public static void ToParamsArrayPostfix(ref BuildingParameters __instance, ref int[] _parameters, ref int _paramCount)
        {
            if(__instance.type == BuildingType.Belt)
            {
                if(__instance.parameters != null && __instance.parameters.Length > 2)
                {
                    _paramCount = __instance.parameters.Length;
                    _parameters = new int[_paramCount];
                    Array.Copy(__instance.parameters, _parameters, _paramCount);
                    //for (int i = 0; i < _parameters.Length; i++)
                    //{
                    //    Console.WriteLine(_parameters[i].ToString());
                    //}
                }
            }
        }

        /// <summary>
        /// 蓝图创建过程中，还会有一次复制param会只管param的有限长度（2），要注意patch这个
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="_parameters"></param>
        /// <param name="_paramCount"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BuildingParameters), "FromParamsArray")]
        public static void FromParamsArrayPostfix(ref BuildingParameters __instance, ref int[] _parameters)
        {
            if (__instance.type == BuildingType.Belt)
            {
                if (_parameters != null && _parameters.Length > 2)
                {
                    __instance.parameters = new int[_parameters.Length];
                    Array.Copy(_parameters, __instance.parameters, _parameters.Length);
                }
            }
        }

        public static void RecalcCargoCurCount(PlanetFactory factory, int pathId)
        {
            int planetId = factory.planetId;
            if (RuntimeData.HasRules(planetId, pathId))
            {
                var obj = RuntimeData.infos[planetId][pathId];
                lock (obj)
                {
                    foreach (var key in RuntimeData.infos[planetId][pathId].cargoLimit.Keys)
                    {
                        RuntimeData.infos[planetId][pathId].cargoCount[key] = 0;
                    }
                    int bufferLength = factory.cargoTraffic.pathPool[pathId].bufferLength;
                    if (bufferLength > 5)
                    {
                        var obj2 = factory.cargoTraffic.pathPool[pathId].buffer;
                        lock (obj2)
                        {
                            for (int i = 0; i < bufferLength; i++)
                            {
                                int num = i + 5;
                                if (num >= 0 && num < bufferLength)
                                {
                                    int num2 = (int)obj2[num];
                                    if (num2 > 0)
                                    {
                                        int num3 = num;
                                        if (num2 == 246)
                                        {
                                            num3 += 250 - num2;
                                            int cargoId = (int)(obj2[num3 + 1] - 1 + (obj2[num3 + 2] - 1) * 100) + (int)(obj2[num3 + 3] - 1) * 10000 + (int)(obj2[num3 + 4] - 1) * 1000000;
                                            int itemId = factory.cargoTraffic.container.cargoPool[cargoId].item;
                                            if (RuntimeData.infos[planetId][pathId].cargoCount.ContainsKey(itemId))
                                                RuntimeData.infos[planetId][pathId].cargoCount[itemId]++;
                                            i += 9;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 原本的是否显示已复制，还会判断param[0]是否大于0，这里要覆写那个判断，改成不管param[0]，只要长度>2就说明有混带参数，就要显示复制粘贴成功的提示
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="__result"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BuildingParameters), "CopiedTipText")]
        public static void CopiedTipTextPostfix(ref BuildingParameters __instance, ref string __result)
        {
            if (__instance.type == BuildingType.Belt && __instance.parameters != null && __instance.parameters.Length > 2)
            {
                __result = "设置已复制".Translate();
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(BuildingParameters), "PastedTipText")]
        public static void PastedTipTextPostfix(ref BuildingParameters __instance, ref string __result)
        {
            if (__instance.type == BuildingType.Belt && __instance.parameters != null && __instance.parameters.Length > 2)
            {
                __result = "设置已粘贴".Translate();
            }
        }
    }
}
