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

    }
}
