using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using WinAPI;

namespace MixCargoController
{
    public static class UIBeltWindowPatcher
    {
        public static List<UICargoSetting> settings;
        public static GameObject settingContentsObj = null;
        public static void Init()
        {
            if (settingContentsObj == null)
            {
                GameObject beltWindow = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Belt Window");
                beltWindow.GetComponent<RectTransform>().sizeDelta = new Vector2(470, 284);
                //settingContentsObj = new GameObject("cargo-settings");
                //settingContentsObj.transform.SetParent(beltWindow.transform);
                //settingContentsObj.transform.localScale = Vector3.one;
                //settingContentsObj.transform.localPosition = new Vector3(-30, 60, 0);

                // 混带设定标题
                GameObject oriTextObj = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Belt Window/item-sign/icon-tag-label");
                GameObject titleObj = GameObject.Instantiate(oriTextObj, beltWindow.transform);
                titleObj.name = "setting-title";
                titleObj.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1f);
                titleObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1f);
                titleObj.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                titleObj.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(186, -71, 0);
                titleObj.GetComponent<Localizer>().stringKey = "MCC混带设置";
                titleObj.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

                // scrollview设定
                GameObject oriScrollViewGroup = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Blueprint Browser/view-group");
                GameObject scrollViewGroup = GameObject.Instantiate(oriScrollViewGroup, beltWindow.transform);
                scrollViewGroup.name = "settings-view-group";
                scrollViewGroup.GetComponent<Image>().color = new Color(0.038f, 0.049f, 0.066f, 0.285f);
                scrollViewGroup.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(185, -83);
                scrollViewGroup.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 170);
                settingContentsObj = scrollViewGroup.transform.Find("Scroll View/Viewport/Content").gameObject;
                while (settingContentsObj.transform.childCount > 0) // 移除所有子对象
                {
                    GameObject.DestroyImmediate(settingContentsObj.transform.GetChild(settingContentsObj.transform.childCount - 1).gameObject);
                }
                GridLayoutGroup gridLayoutGroup = settingContentsObj.AddComponent<GridLayoutGroup>();
                ContentSizeFitter contentSizeFitter = settingContentsObj.AddComponent<ContentSizeFitter>();
                gridLayoutGroup.cellSize = new Vector2(UICargoSetting.width, UICargoSetting.height);
                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                scrollViewGroup.transform.Find("Scroll View/Viewport").GetComponent<RectTransform>().sizeDelta = new Vector2(0, -2); 
                Vector3 oriLocalPosition = scrollViewGroup.transform.Find("Scroll View/Viewport").transform.localPosition;
                scrollViewGroup.transform.Find("Scroll View/Viewport").transform.localPosition = new Vector3(oriLocalPosition.x, oriLocalPosition.y - 2, oriLocalPosition.z);

                settings = new List<UICargoSetting>();
                //for (int i = 0; i < 5; i++)
                //{
                //    UICargoSetting setting = new UICargoSetting(i, settingContentsObj);
                //    settings.Add(setting);
                //}

                //填充线路按钮
                GameObject oriButtonObj = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Belt Window/state/reverse-button");
                GameObject fillTheBeltLineBtnObj = GameObject.Instantiate(oriButtonObj, beltWindow.transform);
                fillTheBeltLineBtnObj.name = "fill-button";
                fillTheBeltLineBtnObj.transform.localScale = Vector3.one;
                fillTheBeltLineBtnObj.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                fillTheBeltLineBtnObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                fillTheBeltLineBtnObj.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(274, -71);
                fillTheBeltLineBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector3(60, 20);
                GameObject.DestroyImmediate(fillTheBeltLineBtnObj.GetComponent<UIButton>());
                fillTheBeltLineBtnObj.AddComponent<UIButton>();
                fillTheBeltLineBtnObj.GetComponent<UIButton>().onClick += (x) => { FillTheBeltPath(); };
                fillTheBeltLineBtnObj.GetComponent<UIButton>().tips.corner = 3;
                fillTheBeltLineBtnObj.GetComponent<UIButton>().tips.width = 225;
                fillTheBeltLineBtnObj.GetComponent<UIButton>().tips.tipTitle = "MCC填满传送带线路";
                fillTheBeltLineBtnObj.GetComponent<UIButton>().tips.tipText = "MCC填满传送带线路说明";
                fillTheBeltLineBtnObj.GetComponent<UIButton>().audios = oriButtonObj.GetComponent<UIButton>().audios;
                fillTheBeltLineBtnObj.GetComponent<UIButton>().transitions = new UIButton.Transition[2];
                fillTheBeltLineBtnObj.GetComponent<UIButton>().transitions[0] = new UIButton.Transition();
                fillTheBeltLineBtnObj.GetComponent<UIButton>().transitions[0].target = fillTheBeltLineBtnObj.GetComponent<Image>();
                fillTheBeltLineBtnObj.GetComponent<UIButton>().transitions[0].damp = 1;
                fillTheBeltLineBtnObj.GetComponent<UIButton>().transitions[0].mouseoverSize = 1;
                fillTheBeltLineBtnObj.GetComponent<UIButton>().transitions[0].pressedSize = 1;
                fillTheBeltLineBtnObj.GetComponent<UIButton>().transitions[0].highlightSizeMultiplier = 1;
                fillTheBeltLineBtnObj.GetComponent<UIButton>().transitions[0].highlightColorMultiplier = 1;
                fillTheBeltLineBtnObj.GetComponent<UIButton>().transitions[0].highlightAlphaMultiplier = 1;
                fillTheBeltLineBtnObj.GetComponent<UIButton>().transitions[0].mouseoverColor = new Color(0.2093f, 0.7724f, 0.9057f, 1);
                fillTheBeltLineBtnObj.GetComponent<UIButton>().transitions[0].normalColor = new Color(0.2199f, 0.6281f, 0.7642f, 0.7451f);
                fillTheBeltLineBtnObj.GetComponent<UIButton>().transitions[0].pressedColor = new Color(0.1149f, 0.6604f, 0.8396f, 1);
                fillTheBeltLineBtnObj.GetComponent<UIButton>().transitions[0].highlightColorOverride = new Color(0, 0, 0, 0);
                fillTheBeltLineBtnObj.GetComponent<UIButton>().transitions[0].disabledColor = new Color(1, 1, 1, 0.0431f);
                fillTheBeltLineBtnObj.GetComponent<UIButton>().transitions[1] = new UIButton.Transition();
                fillTheBeltLineBtnObj.GetComponent<UIButton>().transitions[1].target = fillTheBeltLineBtnObj.transform.Find("text").GetComponent<Text>();
                fillTheBeltLineBtnObj.GetComponent<UIButton>().transitions[1].damp = 1;
                fillTheBeltLineBtnObj.GetComponent<UIButton>().transitions[1].mouseoverSize = 1;
                fillTheBeltLineBtnObj.GetComponent<UIButton>().transitions[1].pressedSize = 1;
                fillTheBeltLineBtnObj.GetComponent<UIButton>().transitions[1].highlightSizeMultiplier = 1;
                fillTheBeltLineBtnObj.GetComponent<UIButton>().transitions[1].highlightColorMultiplier = 1;
                fillTheBeltLineBtnObj.GetComponent<UIButton>().transitions[1].highlightAlphaMultiplier = 1;
                fillTheBeltLineBtnObj.GetComponent<UIButton>().transitions[1].mouseoverColor = Color.white;
                fillTheBeltLineBtnObj.GetComponent<UIButton>().transitions[1].normalColor = Color.white;
                fillTheBeltLineBtnObj.GetComponent<UIButton>().transitions[1].pressedColor = Color.white;
                fillTheBeltLineBtnObj.GetComponent<UIButton>().transitions[1].highlightColorOverride = new Color(0, 0, 0, 0);
                fillTheBeltLineBtnObj.GetComponent<UIButton>().transitions[1].disabledColor = new Color(1, 1, 1, 0.1353f);
                fillTheBeltLineBtnObj.transform.Find("text").GetComponent<Localizer>().stringKey = "MCC填满传送带线路";
                fillTheBeltLineBtnObj.transform.Find("text").GetComponent<Text>().text = "MCC填满传送带线路".Translate();

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
                        foreach (var pair in info.cargoLimit)
                        {
                            if (s >= settings.Count)
                            {
                                settings.Add(new UICargoSetting(s, settingContentsObj));
                            }

                            int itemId = pair.Key;
                            int limit = pair.Value;
                            settings[s].obj.SetActive(true);
                            settings[s].SetItem(itemId, limit);
                            s++;
                        }
                        // 额外的空行
                        if(s >= settings.Count)
                        {
                            settings.Add(new UICargoSetting(s, settingContentsObj));
                        }
                        settings[s].SetItem(0, 0);
                        settings[s].obj.SetActive(true);

                        // 其他行如果有则隐藏
                        for (int i = s + 1; i < settings.Count; i++)
                        {
                            settings[i].SetItem(0, 0);
                            settings[i].obj.SetActive(false);
                        }
                    }
                    else
                    {
                        if (settings.Count == 0)
                        {
                            settings.Add(new UICargoSetting(0, settingContentsObj));
                        }
                        settings[0].SetItem(0, 0);
                        settings[0].obj.SetActive(true);
                        for (int i = 1; i < settings.Count; i++)
                        {
                            settings[i].SetItem(0, 0);
                            settings[i].obj.SetActive(true);
                            settings[i].obj.SetActive(false);
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
                        //int limit = info.cargoLimit.ContainsKey(itemId) ? info.cargoLimit[itemId] : 0;

                        for (int i = 0; i < settings.Count; i++)
                        {
                            if (settings[i].itemId == itemId)
                            {
                                settings[i].SetCurCount(count);
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
        [HarmonyPatch(typeof(UIBeltWindow), "OnTakeBackPointerUp")]
        public static void RecalcCargoCountAfterPlayerTakePutBeltItems(ref UIBeltWindow __instance)
        {
            RecalcCargoCurCount(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIBeltWindow), "OnBeltIdChange")]
        [HarmonyPatch(typeof(UIBeltWindow), "OnReverseButtonClick")]
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
            if (window?.traffic?.factory != null)
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

        /// <summary>
        /// 按照当前比例修改限制值，以尽可能填满传送带（但不超出）
        /// </summary>
        public static void FillTheBeltPath()
        {
            UIBeltWindow window = GetCurWindow();
            if (window?.traffic?.factory != null)
            {
                int planetId = window.traffic.factory.planetId;
                int pathId = GetCurCargoPathId(window);
                int bufferLength = window.traffic.pathPool[pathId].bufferLength;
                int cargoMaxCount = bufferLength / 10;
                if (RuntimeData.HasRules(planetId, pathId))
                {
                    int total = 0;
                    BeltCargoInfo info = RuntimeData.infos[planetId][pathId];
                    List<int> items = new List<int>();
                    foreach (var pair in info.cargoLimit)
                    {
                        items.Add(pair.Key);
                        total += pair.Value;
                    }
                    if (total != 0)
                    {
                        foreach (var item in items)
                        {
                            info.cargoLimit[item] = (int)(info.cargoLimit[item] * 1.0 / total * cargoMaxCount);
                        }
                        RefreshAll(window);
                    }
                }
            }
        }
    }
}
