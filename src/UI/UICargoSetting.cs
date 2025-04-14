using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MixCargoController
{
    public class UICargoSetting
    {
        public int index;
        public int itemId;
        public GameObject obj;
        public InputField valueInput;
        public Text infoText;
        public Image icon;
        public static Sprite oriEmptySprite = null;
        public static float width = 150;
        public static float height = 36;

        public UICargoSetting(int index, GameObject parentObj)
        {
            if(oriEmptySprite == null)
            {
                oriEmptySprite = Resources.Load<Sprite>("ui/textures/sprites/icons/icon-unselected");
            }

            obj = new GameObject("setting");
            obj.transform.localScale = Vector3.one;
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, height);
            obj.transform.SetParent(parentObj.transform, false);
            obj.SetActive(false);
            obj.SetActive(true);
            //obj.transform.localPosition = new Vector3(0, -38 * index, 0);

            this.index = index;
            GameObject ori = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Belt Window/item-sign");
            GameObject iconObj = GameObject.Instantiate(ori);
            iconObj.name = "icon";
            iconObj.transform.SetParent(obj.transform);
            iconObj.transform.localScale = Vector3.one;
            iconObj.GetComponent<RectTransform>().sizeDelta = new Vector2(36, 36);
            iconObj.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(30, 0, 0f);
            iconObj.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0.5f);
            iconObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.5f);
            if (UIRoot.instance?.uiGame?.beltWindow != null)
            {
                iconObj.GetComponent<UIButton>().onClick -= UIRoot.instance.uiGame.beltWindow.OnTagSelectButtonClick;
                iconObj.GetComponent<UIButton>().onRightClick -= UIRoot.instance.uiGame.beltWindow.OnTagSelectButtonRightClick;
                iconObj.GetComponent<UIButton>().onClick += OnIconClick;
                iconObj.GetComponent<UIButton>().onRightClick += OnIconRightClick;

            }
            else
            {
                Debug.LogError("null");
            }
            iconObj.GetComponent<UIButton>().tips.tipTitle = "MCC设置物品标题";
            iconObj.GetComponent<UIButton>().tips.tipText = "MCC设置物品说明";
            iconObj.GetComponent<UIButton>().tips.width = 226;
            icon = iconObj.GetComponent<Image>();

            GameObject infoTextObj = iconObj.transform.Find("icon-tag-label").gameObject;
            infoTextObj.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0.5f);
            infoTextObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.5f);
            infoTextObj.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
            infoTextObj.transform.SetParent(obj.transform, true);
            GameObject.Destroy(infoTextObj.GetComponent<Localizer>());
            infoText = infoTextObj.GetComponent<Text>();    
            infoText.alignment = TextAnchor.MiddleLeft;
            infoTextObj.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(115, 0, 0);
            infoText.text = "";

            GameObject oriInputFieldObj = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Blueprint Browser/inspector-group/Scroll View/Viewport/Content/group-1/input-short-text"); if (oriInputFieldObj == null)
                oriInputFieldObj = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Blueprint Browser/inspector-group/BP-panel-scroll(Clone)/Viewport/pane/group-1/input-short-text");
            if (oriInputFieldObj == null)
            {
                Debug.LogError("Error when init oriInputField because some other mods has changed the Blueprint Browser UI. Please contant jinxOAO.");
            }

            GameObject ratioInputObj = GameObject.Instantiate(oriInputFieldObj, obj.transform);
            ratioInputObj.name = "input";
            ratioInputObj.GetComponent<RectTransform>().sizeDelta = new Vector2(45, 30);
            ratioInputObj.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.5f);
            ratioInputObj.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0.5f);
            ratioInputObj.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
            ratioInputObj.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(60, 0, 0);
            ratioInputObj.GetComponent<InputField>().characterLimit = 5;
            ratioInputObj.GetComponent<InputField>().transition = Selectable.Transition.None; // 要不然鼠标不在上面时颜色会很浅，刚打开容易找不到，不够明显
            ratioInputObj.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);
            ratioInputObj.transform.Find("value-text").GetComponent<Text>().color = Color.white;
            ratioInputObj.transform.Find("value-text").GetComponent<Text>().fontSize = 14;
            ratioInputObj.GetComponent<InputField>().onEndEdit.RemoveAllListeners();
            ratioInputObj.GetComponent<InputField>().onEndEdit.AddListener((x) => SaveThisSetting());
            ratioInputObj.SetActive(false);
            ratioInputObj.SetActive(true);
            valueInput = ratioInputObj.GetComponent<InputField>();

            SetItem(0, 0);
        }

        public void OnIconClick(int i)
        {
            UIBeltWindow window = UIBeltWindowPatcher.GetCurWindow();
            if(window != null)
            {
                UIItemPicker.Popup(window.windowTrans.anchoredPosition + new Vector2(-300f, 235f), new Action<ItemProto>(OnItemSelectedReturn));
            }
        }

        public void OnIconRightClick(int i)
        {
            RemoveThisSetting();
        }


        public void OnItemSelectedReturn(ItemProto itemProto)
        {
            if(itemProto != null)
            {
                int id = itemProto.ID;
                if (id != this.itemId) // 检测是不是重复设置的物品，如果重复则设置无效
                {
                    UIBeltWindow window = UIBeltWindowPatcher.GetCurWindow();
                    if (window?.traffic != null)
                    {
                        int planetId = window.traffic.factory.planetId;
                        int segPathId = UIBeltWindowPatcher.GetCurCargoPathId(window);
                        if (RuntimeData.infos.ContainsKey(planetId))
                        {
                            if (RuntimeData.infos[planetId].ContainsKey(segPathId)) // 物品不能重复执行
                            {
                                if (RuntimeData.infos[planetId][segPathId].cargoLimit.ContainsKey(id))
                                {
                                    UIRealtimeTip.Popup("MCC重复物品提示".Translate());
                                    return;
                                }
                            }
                        }
                    }
                }
                if (this.itemId != 0 && this.itemId != id)
                {
                    RemoveThisSetting();
                }
                SetItem(itemProto.ID, -1);
                SaveThisSetting();
            }
            else
            {
                RemoveThisSetting();
            }
        }

        public void SetItem(int itemId, int limitInput)
        {
            if(itemId != 0 && LDB.items.Select(itemId) != null)
            {
                this.itemId = itemId;
                icon.sprite = LDB.items.Select(itemId).iconSprite;
                if(limitInput >= 0)
                    valueInput.text = limitInput.ToString();
            }
            else
            {
                this.itemId = 0;
                icon.sprite = oriEmptySprite;
                valueInput.text = "0";
                infoText.text = "";
            }
        }

        public void RemoveThisSetting()
        {
            UIBeltWindow window = UIBeltWindowPatcher.GetCurWindow();
            if(window?.traffic?.factory != null)
            {
                int planetId = window.traffic.factory.planetId;
                int segPathId = UIBeltWindowPatcher.GetCurCargoPathId(window);
                if(RuntimeData.infos.ContainsKey(planetId))
                {
                    if (RuntimeData.infos[planetId].ContainsKey(segPathId))
                    {
                        RuntimeData.infos[planetId][segPathId].RemoveLimit(itemId);
                    }
                }
            }
            this.itemId = 0;
            icon.sprite = oriEmptySprite;
            valueInput.text = "0";
            infoText.text = "";

            UIBeltWindowPatcher.RefreshAll();
        }
        public void SaveThisSetting()
        {
            if(itemId == 0)
            {
                return;
            }

            int value = 0;
            try
            {
                value = Convert.ToInt32(valueInput.text);
                if (value < 0)
                    value = 0;
            }
            catch (Exception)
            {
                value = 0;
            }
            valueInput.text = value.ToString();

            UIBeltWindow window = UIBeltWindowPatcher.GetCurWindow();
            if (window?.traffic?.factory != null)
            {
                int planetId = window.traffic.factory.planetId;
                int segPathId = UIBeltWindowPatcher.GetCurCargoPathId(window);
                if (!RuntimeData.infos.ContainsKey(planetId))
                {
                    var data = RuntimeData.infos;
                    RuntimeData.infos.AddOrUpdate(planetId, new ConcurrentDictionary<int, BeltCargoInfo>(), (x, y) => new ConcurrentDictionary<int, BeltCargoInfo>());
                }
                if (!RuntimeData.infos[planetId].ContainsKey(segPathId))
                {
                    RuntimeData.infos[planetId].AddOrUpdate(segPathId, new BeltCargoInfo(), (x, y) => new BeltCargoInfo());
                }
                RuntimeData.infos[planetId][segPathId].AddOrUpdateLimit(itemId, value);

                UIBeltWindowPatcher.RefreshAll();
            }

        }

        public void SetCurCount(int curCount) 
        {
            infoText.text = curCount.ToString();// + "/" + limitCount.ToString();
        }
    }
}
