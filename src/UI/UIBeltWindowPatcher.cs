using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WinAPI;

namespace MixCargoController
{
    public static class UIBeltWindowPatcher
    {
        public static List<UICargoSetting> settings;
        public static GameObject settingsObj = null;
        public static void Init()
        {
            if (settingsObj == null)
            {
                GameObject beltWindow = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Belt Window");
                beltWindow.GetComponent<RectTransform>().sizeDelta = new Vector2(450, 284);
                settingsObj = new GameObject("cargo-settings");
                settingsObj.transform.SetParent(beltWindow.transform);
                settingsObj.transform.localScale = Vector3.one;
                settingsObj.transform.localPosition = new Vector3(-30, 60, 0);

                settings = new List<UICargoSetting>();
                for (int i = 0; i < 5; i++)
                {
                    UICargoSetting setting = new UICargoSetting(i);
                    settings.Add(setting);
                }
            }
        }

        public static void RefreshAll(UIBeltWindow window = null)
        {
            if(window == null)
                window = UIRoot.instance?.uiGame?.beltWindow;
            if(window?.traffic?.planet != null)
            {
                int planetId = window.traffic.factory.planetId;
                int pathId = GetCurCargoPathId(window);
                if(pathId != -1 && planetId > 0)
                {
                    if(RuntimeData.infos.ContainsKey(planetId) && RuntimeData.infos[planetId].ContainsKey(pathId))
                    {
                        BeltCargoInfo info = RuntimeData.infos[planetId][pathId];
                        int s = 0;
                        int maxs = 4;
                        foreach (var pair in info.cargoLimit)
                        {
                            if (s > maxs)
                                break;

                            int itemId = pair.Key;
                            int limit = pair.Value;
                            settings[s].obj.SetActive(true);
                            settings[s].SetItem(itemId, limit);
                            s++;
                        }
                        for (int i = s; i < settings.Count; i++)
                        {
                            settings[i].SetItem(0, 0);
                            if (i == info.cargoLimit.Count)
                            {
                                settings[i].obj.SetActive(true);
                            }
                            else
                            {
                                settings[i].obj.SetActive(false);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < settings.Count; i++)
                        {
                            settings[i].SetItem(0, 0);
                            if (i == 0)
                            {
                                settings[i].obj.SetActive(true);
                            }
                            else
                            {
                                settings[i].obj.SetActive(false);
                            }
                        }

                    }
                }
                RecalcCargoCurCount(window);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIBeltWindow), "_OnUpdate")]
        public static void UpdatePostfix(ref UIBeltWindow __instance)
        {
            int planetId = __instance.traffic.factory.planetId;
            int pathId = GetCurCargoPathId(__instance);
            if (pathId != -1 && planetId > 0)
            {
                if (RuntimeData.infos.ContainsKey(planetId) && RuntimeData.infos[planetId].ContainsKey(pathId))
                {
                    BeltCargoInfo info = RuntimeData.infos[planetId][pathId];
                    foreach (var pair in info.cargoCount)
                    {
                        int itemId = pair.Key;
                        int count = pair.Value;
                        int limit = info.cargoLimit.ContainsKey(itemId) ? info.cargoLimit[itemId] : 0;

                        for (int i = 0; i < settings.Count; i++)
                        {
                            if (settings[i].itemId == itemId)
                            {
                                settings[i].SetCurCount(count, limit);
                                break;
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIBeltWindow), "_OnOpen")]
        public static void OnOpenPostfix(ref UIBeltWindow __instance)
        {
            RefreshAll(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIBeltWindow), "OnBeltIdChange")]
        public static void OnBeltIdChangePostfix(ref UIBeltWindow __instance)
        {
            RefreshAll(__instance);
        }

        public static int GetCurCargoPathId(UIBeltWindow window = null)
        {
            if(window == null)
                window = UIRoot.instance?.uiGame?.beltWindow;
            if(window != null)
            {
                if(window.beltId != 0 && window.factory != null)
                {
                    BeltComponent beltComponent = window.traffic.beltPool[window.beltId];
                    if(beltComponent.id == window.beltId)
                    {
                        return beltComponent.segPathId;
                    }
                }
            }
            return -1;
        }

        public static UIBeltWindow GetCurWindow()
        {
            return UIRoot.instance?.uiGame?.beltWindow;
        }

        public static void RecalcCargoCurCount(UIBeltWindow window = null)
        {
            if (window == null)
                window = UIRoot.instance?.uiGame?.beltWindow;
            if (window != null)
            {
                int planetId = window.traffic.factory.planetId;
                int pathId = GetCurCargoPathId(window);
                if (RuntimeData.infos.ContainsKey(planetId) && RuntimeData.infos[planetId].ContainsKey(pathId))
                {
                    var obj = RuntimeData.infos[planetId][pathId];
                    lock (obj)
                    {
                        foreach (var key in RuntimeData.infos[planetId][pathId].cargoLimit.Keys)
                        {
                            RuntimeData.infos[planetId][pathId].cargoCount[key] = 0;
                        }
                        int bufferLength = window.traffic.pathPool[pathId].bufferLength;
                        if (bufferLength > 5)
                        {
                            var obj2 = window.traffic.pathPool[pathId].buffer;
                            lock (obj2)
                            {
                                for (int i = 0; i < bufferLength; i++)
                                {
                                    int num = i + 5;
                                    if(num >= 0 && num < bufferLength)
                                    {
                                        int num2 = (int)obj2[num];
                                        if (num2 > 0)
                                        {
                                            int num3 = num;
                                            if (num2 == 246)
                                            {
                                                num3 += 250 - num2;
                                                int cargoId = (int)(obj2[num3 + 1] - 1 + (obj2[num3 + 2] - 1) * 100) + (int)(obj2[num3 + 3] - 1) * 10000 + (int)(obj2[num3 + 4] - 1) * 1000000;
                                                int itemId = window.traffic.container.cargoPool[cargoId].item;
                                                if (RuntimeData.infos[planetId][pathId].cargoCount.ContainsKey(itemId))
                                                    RuntimeData.infos[planetId][pathId].cargoCount[itemId]++;
                                                i += 9;
                                            }
                                            //else
                                            //{
                                            //    num3 += (int)(246 - obj2[num - 4]);
                                            //}
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
