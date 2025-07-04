using HarmonyLib;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MixCargoController
{
    public static class BeltCargoCountLogic
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CargoTraffic), "TryPickItem", new Type[] { typeof(int), typeof(int), typeof(int), typeof(int[]), typeof(byte), typeof(byte) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out })]
        [HarmonyPatch(typeof(CargoTraffic), "TryPickItem", new Type[] { typeof(int), typeof(int), typeof(int), typeof(byte), typeof(byte) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out})]
        public static void PickFromPostfix(ref CargoTraffic __instance, int beltId, int __result)
        {
            if (__result > 0)
            {
                if (RuntimeData.infos.ContainsKey(__instance.factory.planetId))
                {
                    int segPathId = __instance.beltPool[beltId].segPathId;
                    if (RuntimeData.infos[__instance.factory.planetId].ContainsKey(segPathId))
                    {
                        if(RuntimeData.infos[__instance.factory.planetId][segPathId].cargoCount.ContainsKey(__result))
                        {
                            var obj = RuntimeData.infos[__instance.factory.planetId][segPathId];
                            lock (obj)
                            {
                                RuntimeData.infos[__instance.factory.planetId][segPathId].cargoCount[__result]--;
                            }
                        }
                    }
                }
            }
        }


        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(CargoTraffic), "TryPickItemAtRear")]
        //public static void PickFromRearPostfix(ref CargoTraffic __instance, int beltId, int __result)
        //{
        //    if (__result > 0)
        //    {
        //        if (RuntimeData.infos.ContainsKey(__instance.factory.planetId))
        //        {
        //            int segPathId = __instance.beltPool[beltId].segPathId;
        //            if (RuntimeData.infos[__instance.factory.planetId].ContainsKey(segPathId))
        //            {
        //                if (RuntimeData.infos[__instance.factory.planetId][segPathId].cargoCount.ContainsKey(__result))
        //                {
        //                    var obj = RuntimeData.infos[__instance.factory.planetId][segPathId];
        //                    lock (obj)
        //                    {
        //                        RuntimeData.infos[__instance.factory.planetId][segPathId].cargoCount[__result]--;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}


        /// <summary>
        /// 爪子放置到传送带上，且物品有堆叠才会调用
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CargoTraffic), "TryInsertItem")]
        public static bool TryInsertSimplePrefix(ref CargoTraffic __instance, int beltId, int offset, int itemId, byte itemCount, byte itemInc, ref bool __result)
        {
            if (RuntimeData.infos.ContainsKey(__instance.factory.planetId))
            {
                int segPathId = __instance.beltPool[beltId].segPathId;
                if (RuntimeData.infos[__instance.factory.planetId].ContainsKey(segPathId))
                {
                    if (!RuntimeData.infos[__instance.factory.planetId][segPathId].enabled) // 未启用
                        return true;

                    if (RuntimeData.infos[__instance.factory.planetId][segPathId].CanInsert(itemId))
                    {
                        __result = __instance.pathPool[__instance.beltPool[beltId].segPathId].TryInsertItem(__instance.beltPool[beltId].pivotOnPath + offset, itemId, itemCount, itemInc);
                        if(__result)
                        {
                            var obj = RuntimeData.infos[__instance.factory.planetId][segPathId];
                            lock(obj)
                            {
                                RuntimeData.infos[__instance.factory.planetId][segPathId].cargoCount[itemId]++;
                            }
                        }
                        return false;
                    }
                    else
                    {
                        __result = false;
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 爪子放置到传送带上，且物品有堆叠才会调用
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="beltId"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CargoTraffic), "TryInsertItemToBeltWithStackIncreasement")]
        public static bool TryInsertCanStackPrefix(ref CargoTraffic __instance, int beltId, int offset, int itemId, int maxStack, ref int itemCount, ref int itemInc)
        {
            if (RuntimeData.infos.ContainsKey(__instance.factory.planetId))
            {
                int segPathId = __instance.beltPool[beltId].segPathId;
                int planetId = __instance.factory.planetId;
                if (RuntimeData.infos[planetId].ContainsKey(segPathId))
                {
                    if (!RuntimeData.infos[planetId][segPathId].enabled) // 未启用
                        return true;

                    bool canAddNewCargo = RuntimeData.infos[planetId][segPathId].CanInsert(itemId);
                    __instance.pathPool[__instance.beltPool[beltId].segPathId].CargoPathTryInsertStackPatch(__instance.beltPool[beltId].pivotOnPath + offset, itemId, maxStack, ref itemCount, ref itemInc, planetId, segPathId, canAddNewCargo);
                    return false;
                }
            }

            return true;
        }


        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(CargoTraffic), "TryInsertItemAtHead")]
        //public static bool TryInsertAtHeadSimplePrefix(ref CargoTraffic __instance, int beltId, int itemId, byte stack, byte inc, ref bool __result)
        //{
        //    if (RuntimeData.infos.ContainsKey(__instance.factory.planetId))
        //    {
        //        int segPathId = __instance.beltPool[beltId].segPathId;
        //        if (RuntimeData.infos[__instance.factory.planetId].ContainsKey(segPathId))
        //        {
        //            if (!RuntimeData.infos[__instance.factory.planetId][segPathId].enabled) // 未启用
        //                return true;

        //            if (RuntimeData.infos[__instance.factory.planetId][segPathId].CanInsert(itemId))
        //            {
        //                __result = __instance.pathPool[__instance.beltPool[beltId].segPathId].TryInsertItemAtHeadAndFillBlank(itemId, stack, inc);
        //                if (__result)
        //                {
        //                    var obj = RuntimeData.infos[__instance.factory.planetId][segPathId];
        //                    lock (obj)
        //                    {
        //                        RuntimeData.infos[__instance.factory.planetId][segPathId].cargoCount[itemId]++;
        //                    }
        //                }
        //                return false;
        //            }
        //            else
        //            {
        //                __result = false;
        //                return false;
        //            }
        //        }
        //    }
        //    return true;
        //}


        public static void CargoPathTryInsertStackPatch(this CargoPath _this, int index, int itemId, int maxStack, ref int count, ref int inc, int planetId, int segPathId, bool canAddNewCargo)
        {
            byte[] obj = _this.buffer;
            lock (obj)
            {
                int num = index + 5;
                if (num >= 0 && num < _this.bufferLength)
                {
                    int num2 = (int)_this.buffer[num];
                    if (num2 > 0)
                    {
                        int num3 = num;
                        if (num2 >= 246)
                        {
                            num3 += 250 - num2;
                        }
                        else
                        {
                            num3 += (int)(246 - _this.buffer[num - 4]);
                        }
                        int cargoId = (int)(_this.buffer[num3 + 1] - 1 + (_this.buffer[num3 + 2] - 1) * 100) + (int)(_this.buffer[num3 + 3] - 1) * 10000 + (int)(_this.buffer[num3 + 4] - 1) * 1000000;
                        _this.cargoContainer.AddItemStackToCargo(cargoId, itemId, maxStack, ref count, ref inc);
                    }
                }
                if (count == 0)
                {
                    return;
                }
                int num4 = index - 4;
                if (num4 >= 0 && num4 < _this.bufferLength)
                {
                    int num5 = (int)_this.buffer[num4];
                    if (num5 > 0)
                    {
                        int num6 = num4;
                        if (num5 >= 246)
                        {
                            num6 += 250 - num5;
                        }
                        else
                        {
                            num6 += (int)(246 - _this.buffer[num4 - 4]);
                        }
                        int cargoId2 = (int)(_this.buffer[num6 + 1] - 1 + (_this.buffer[num6 + 2] - 1) * 100) + (int)(_this.buffer[num6 + 3] - 1) * 10000 + (int)(_this.buffer[num6 + 4] - 1) * 1000000;
                        _this.cargoContainer.AddItemStackToCargo(cargoId2, itemId, maxStack, ref count, ref inc);
                    }
                }
                if (count == 0 || !canAddNewCargo) // 这里有修改，如果因为限制数量已满，可以现有货物上堆叠（上方的逻辑），但是堆完还有剩余需要插入，那么需要根据canAddNewCargo决定能否继续执行插入
                {
                    return;
                }
                int num7 = index - 5;
                if (index < 4)
                {
                    return;
                }
                if (num >= _this.bufferLength)
                {
                    return;
                }
                bool flag2 = false;
                while (index > num7)
                {
                    if (_this.buffer[num] == 0)
                    {
                        flag2 = true;
                        break;
                    }
                    index--;
                    num--;
                }
                if (!flag2)
                {
                    return;
                }
                if (num + 6 < _this.bufferLength)
                {
                    if (_this.buffer[++num] != 0)
                    {
                        index = num - 1 - 5;
                    }
                    else if (_this.buffer[++num] != 0)
                    {
                        index = num - 1 - 5;
                    }
                    else if (_this.buffer[++num] != 0)
                    {
                        index = num - 1 - 5;
                    }
                    else if (_this.buffer[++num] != 0)
                    {
                        index = num - 1 - 5;
                    }
                    else if (_this.buffer[++num] != 0)
                    {
                        index = num - 1 - 5;
                    }
                }
                if (index < 4)
                {
                    return;
                }
                int num8 = index + 5;
                int num9 = index - 4;
                if (_this.buffer[num9] == 0 && (!_this.closed || num9 >= 10))
                {
                    int num10 = count;
                    int num11 = inc;
                    if (count > maxStack)
                    {
                        num10 = maxStack;
                        num11 = inc / count;
                        int num12 = inc - num11 * count;
                        count -= num10;
                        num12 -= count;
                        num11 = ((num12 > 0) ? (num11 * num10 + num12) : (num11 * num10));
                        inc -= num11;
                    }
                    else
                    {
                        count = 0;
                        inc = 0;
                    }
                    _this.InsertItemDirect(index, itemId, (byte)num10, (byte)num11);
                    var objmcc = RuntimeData.infos[planetId][segPathId];
                    lock (objmcc)
                    {
                        RuntimeData.infos[planetId][segPathId].cargoCount[itemId]++;
                    }
                    return;
                }
                int num13 = num8 - 2880;
                if (num13 < 0)
                {
                    num13 = 0;
                }
                int num14 = 0;
                int num15 = 0;
                bool flag3 = false;
                bool flag4 = false;
                for (int i = num8; i >= num13; i--)
                {
                    if (_this.buffer[i] == 0)
                    {
                        num15++;
                        if (!flag3)
                        {
                            num14++;
                        }
                        if (num14 == 10 && (!_this.closed || i >= 10))
                        {
                            int num16 = count;
                            int num17 = inc;
                            if (count > maxStack)
                            {
                                num16 = maxStack;
                                num17 = inc / count;
                                int num18 = inc - num17 * count;
                                count -= num16;
                                num18 -= count;
                                num17 = ((num18 > 0) ? (num17 * num16 + num18) : (num17 * num16));
                                inc -= num17;
                            }
                            else
                            {
                                count = 0;
                                inc = 0;
                            }
                            _this.InsertItemDirect(index, itemId, (byte)num16, (byte)num17);
                            var objmcc = RuntimeData.infos[planetId][segPathId];
                            lock (objmcc)
                            {
                                RuntimeData.infos[planetId][segPathId].cargoCount[itemId]++;
                            }
                            return;
                        }
                        if (num15 == 10 && (!_this.closed || i >= 10))
                        {
                            flag4 = true;
                            break;
                        }
                    }
                    else
                    {
                        flag3 = true;
                        if (num14 < 1)
                        {
                            return;
                        }
                        if (_this.buffer[i] == 255)
                        {
                            i -= 9;
                        }
                    }
                }
                if (_this.closed && !flag4 && num15 >= 10 && num15 < 20 && num8 < 2880)
                {
                    num15 -= 10;
                    if (num14 > 10)
                    {
                        num14 = 10;
                    }
                    int num19 = _this.bufferLength - 1;
                    while (num19 > num8 && num19 > _this.bufferLength + num8 - 2880)
                    {
                        if (_this.buffer[num19] == 0)
                        {
                            num15++;
                        }
                        else if (_this.buffer[num19] == 255)
                        {
                            num19 -= 9;
                        }
                        if (num15 >= 10)
                        {
                            if (num14 == 10)
                            {
                                int num20 = count;
                                int num21 = inc;
                                if (count > maxStack)
                                {
                                    num20 = maxStack;
                                    num21 = inc / count;
                                    int num22 = inc - num21 * count;
                                    count -= num20;
                                    num22 -= count;
                                    num21 = ((num22 > 0) ? (num21 * num20 + num22) : (num21 * num20));
                                    inc -= num21;
                                }
                                else
                                {
                                    count = 0;
                                    inc = 0;
                                }
                                _this.InsertItemDirect(index, itemId, (byte)num20, (byte)num21);
                                var objmcc = RuntimeData.infos[planetId][segPathId];
                                lock (objmcc)
                                {
                                    RuntimeData.infos[planetId][segPathId].cargoCount[itemId]++;
                                }
                                return;
                            }
                            flag4 = true;
                            break;
                        }
                        else
                        {
                            num19--;
                        }
                    }
                }
                if (flag4)
                {
                    int num23 = 10 - num14;
                    int num24 = num8 - num14 + 1;
                    for (int j = num9; j >= num13; j--)
                    {
                        if (_this.buffer[j] == 246)
                        {
                            int num25 = 0;
                            int num26 = j - 1;
                            while (num26 >= num13 && num25 < num23 && _this.buffer[num26] == 0)
                            {
                                num25++;
                                num26--;
                            }
                            if (num25 > 0)
                            {
                                Array.Copy(_this.buffer, j, _this.buffer, j - num25, num24 - j);
                                num23 -= num25;
                                num24 -= num25;
                                j -= num25;
                            }
                        }
                    }
                    if (num23 == 0)
                    {
                        int num27 = count;
                        int num28 = inc;
                        if (count > maxStack)
                        {
                            num27 = maxStack;
                            num28 = inc / count;
                            int num29 = inc - num28 * count;
                            count -= num27;
                            num29 -= count;
                            num28 = ((num29 > 0) ? (num28 * num27 + num29) : (num28 * num27));
                            inc -= num28;
                        }
                        else
                        {
                            count = 0;
                            inc = 0;
                        }
                        _this.InsertItemDirect(index, itemId, (byte)num27, (byte)num28);
                        var objmcc = RuntimeData.infos[planetId][segPathId];
                        lock (objmcc)
                        {
                            RuntimeData.infos[planetId][segPathId].cargoCount[itemId]++;
                        }
                        return;
                    }
                    Assert.CannotBeReached("断言失败：插入货物逻辑有误");
                }
            }
        }



        [HarmonyPrefix]
        [HarmonyPatch(typeof(CargoTraffic), "CargoPathsGameTickSync")]
        public static bool CargoPathsGameTickSyncPrefix(ref CargoTraffic __instance)
        {
            PerformanceMonitor.BeginSample(ECpuWorkEntry.Belt);
            for (int i = 1; i < __instance.pathCursor; i++)
            {
                if (__instance.pathPool[i] != null && __instance.pathPool[i].id == i)
                {
                    if (__instance.pathPool[i].outputPath == null || __instance.pathPool[i].outputPath.id == i) // 没有输出口的带子不考虑计数问题，环路（输出是自己）也不考虑
                        __instance.pathPool[i].Update();
                    else
                        __instance.pathPool[i].CargoPathUpdateMCC(__instance.planet.id);
                }
            }
            PerformanceMonitor.EndSample(ECpuWorkEntry.Belt);
            return false;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(CargoTraffic), "CargoPathsGameTickAsync")]
        public static bool CargoPathsGameTickAsyncPrefix(ref CargoTraffic __instance, int _usedThreadCnt, int _curThreadIdx, int _minimumMissionCnt)
        {
            int num;
            int num2;
            if (WorkerThreadExecutor.CalculateMissionIndex(1, __instance.pathCursor - 1, _usedThreadCnt, _curThreadIdx, _minimumMissionCnt, out num, out num2))
            {
                for (int i = num; i < num2; i++)
                {
                    if (__instance.pathPool[i] != null && __instance.pathPool[i].id == i)
                    {
                        if (__instance.pathPool[i].outputPath == null || __instance.pathPool[i].outputPath.id == i) // 没有输出口的带子不考虑计数问题，环路（输出是自己）也不考虑
                            __instance.pathPool[i].Update();
                        else
                            __instance.pathPool[i].CargoPathUpdateMCC(__instance.planet.id);
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// 传送带直接接入另一条传送带的计数逻辑，
        /// </summary>
        public static void CargoPathUpdateMCC(this CargoPath __instance, int planetId)
        {
            byte[] obj;
            if (__instance.outputPath != null)
            {
                int outputPathId = __instance.outputPath.id;
                int num;
                if (__instance.outputPath.chunkCount == 1)
                {
                    num = __instance.outputPath.chunks[2];
                    __instance.outputChunk = 0;
                }
                else
                {
                    int num2 = __instance.outputPath.chunkCount - 1;
                    if (__instance.outputChunk > num2)
                    {
                        __instance.outputChunk = num2;
                    }
                    int num3 = 0;
                    for (; ; )
                    {
                        if (__instance.outputIndex < __instance.outputPath.chunks[__instance.outputChunk * 3])
                        {
                            num2 = __instance.outputChunk - 1;
                            __instance.outputChunk = (num3 + num2) / 2;
                        }
                        else
                        {
                            if (__instance.outputIndex < __instance.outputPath.chunks[__instance.outputChunk * 3] + __instance.outputPath.chunks[__instance.outputChunk * 3 + 1])
                            {
                                break;
                            }
                            num3 = __instance.outputChunk + 1;
                            __instance.outputChunk = (num3 + num2) / 2;
                        }
                    }
                    num = __instance.outputPath.chunks[__instance.outputChunk * 3 + 2];
                }
                byte[] array = (__instance.id > __instance.outputPath.id) ? __instance.buffer : __instance.outputPath.buffer;
                obj = ((__instance.id < __instance.outputPath.id) ? __instance.buffer : __instance.outputPath.buffer);
                lock (obj)
                {
                    byte[] obj2 = array;
                    lock (obj2)
                    {
                        int num4 = __instance.bufferLength - 5 - 1;
                        if (__instance.buffer[num4] == 250)
                        {
                            int cargoId = (int)(__instance.buffer[num4 + 1] - 1 + (__instance.buffer[num4 + 2] - 1) * 100) + (int)(__instance.buffer[num4 + 3] - 1) * 10000 + (int)(__instance.buffer[num4 + 4] - 1) * 1000000;
                            ///////////////////////////////////////////////////////下面有更改
                            int itemId = __instance.cargoContainer.cargoPool[cargoId].item;
                            bool outHasRule = RuntimeData.HasRules(planetId, outputPathId);
                            bool thisHasRule = RuntimeData.HasRules(planetId, __instance.id); bool canTry;
                            if (outHasRule)
                                canTry = RuntimeData.infos[planetId][outputPathId].CanInsert(itemId);
                            else
                                canTry = true;

                            if (__instance.closed)
                            {
                                if (canTry)
                                {
                                    if (__instance.outputPath.TryInsertCargoNoSqueeze(__instance.outputIndex, cargoId))
                                    {
                                        Array.Clear(__instance.buffer, num4 - 4, 10);
                                        __instance.updateLen = __instance.bufferLength;
                                        if (outHasRule) // 如果out有rule，又执行了插入，说明肯定是有这个item的设定
                                        {
                                            var info = RuntimeData.infos[planetId][outputPathId];
                                            lock (info)
                                            {
                                                RuntimeData.infos[planetId][outputPathId].cargoCount[itemId]++;
                                            }
                                        }
                                        if (thisHasRule && RuntimeData.infos[planetId][__instance.id].cargoLimit.ContainsKey(itemId))
                                        {
                                            var info = RuntimeData.infos[planetId][__instance.id];
                                            lock (info)
                                            {
                                                RuntimeData.infos[planetId][__instance.id].cargoCount[itemId]--;
                                            }
                                        }
                                    }
                                }
                            }
                            else if (canTry)
                            {
                                if (__instance.outputPath.TryInsertCargo((__instance.lastUpdateFrameOdd == __instance.outputPath.lastUpdateFrameOdd) ? __instance.outputIndex : ((__instance.outputIndex + num > __instance.outputPath.bufferLength - 6) ? (__instance.outputPath.bufferLength - 6) : (__instance.outputIndex + num)), cargoId))
                                {
                                    Array.Clear(__instance.buffer, num4 - 4, 10);
                                    __instance.updateLen = __instance.bufferLength;

                                    if (outHasRule) // 如果out有rule，又执行了插入，说明肯定是有这个item的设定
                                    {
                                        var info = RuntimeData.infos[planetId][outputPathId];
                                        lock (info)
                                        {
                                            RuntimeData.infos[planetId][outputPathId].cargoCount[itemId]++;
                                        }
                                    }
                                    if (thisHasRule && RuntimeData.infos[planetId][__instance.id].cargoLimit.ContainsKey(itemId))
                                    {
                                        var info = RuntimeData.infos[planetId][__instance.id];
                                        lock (info)
                                        {
                                            RuntimeData.infos[planetId][__instance.id].cargoCount[itemId]--;
                                        }
                                    }
                                }
                            }
                            ///////////////////////////////////////////////////////
                        }
                        goto IL_292;
                    }
                }
            }
            if (__instance.bufferLength <= 10)
            {
                return;
            }
        IL_292:
            obj = __instance.buffer;
            lock (obj)
            {
                __instance.lastUpdateFrameOdd = ((GameMain.gameTick & 1L) == 1L);
                int num5 = __instance.updateLen - 1;
                while (num5 >= 0 && __instance.buffer[num5] != 0)
                {
                    __instance.updateLen--;
                    num5--;
                }
                if (__instance.updateLen != 0)
                {
                    int num6 = __instance.updateLen;
                    for (int i = __instance.chunkCount - 1; i >= 0; i--)
                    {
                        int num7 = __instance.chunks[i * 3];
                        int num8 = __instance.chunks[i * 3 + 2];
                        if (num7 < num6)
                        {
                            if (__instance.buffer[num7] != 0)
                            {
                                int j = num7 - 5;
                                while (j < num7 + 4)
                                {
                                    if (j >= 0 && __instance.buffer[j] == 250)
                                    {
                                        if (j < num7)
                                        {
                                            num7 = j + 5 + 1;
                                            break;
                                        }
                                        num7 = j - 4;
                                        break;
                                    }
                                    else
                                    {
                                        j++;
                                    }
                                }
                            }
                            int k = 0;
                            while (k < num8)
                            {
                                int num9 = num6 - num7;
                                if (num9 < 10)
                                {
                                    k = ((num8 < num9) ? num8 : num9);
                                    break;
                                }
                                int num10 = 0;
                                for (int l = 0; l < num8 - k; l++)
                                {
                                    int num11 = num6 - 1 - l;
                                    if (__instance.buffer[num11] != 0)
                                    {
                                        break;
                                    }
                                    num10++;
                                }
                                if (num10 > 0)
                                {
                                    Array.Copy(__instance.buffer, num7, __instance.buffer, num7 + num10, num9 - num10);
                                    Array.Clear(__instance.buffer, num7, num10);
                                    k += num10;
                                }
                                int num12 = num6 - 1;
                                while (num12 >= 0 && __instance.buffer[num12] != 0)
                                {
                                    num6--;
                                    num12--;
                                }
                            }
                            int num13 = num7 + ((k == 0) ? 1 : k);
                            if (num6 > num13)
                            {
                                num6 = num13;
                            }
                        }
                    }
                }
            }
            return;
        }

    }
}
