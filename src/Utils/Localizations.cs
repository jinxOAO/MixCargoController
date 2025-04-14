using CommonAPI.Systems.ModLocalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixCargoController
{
    public class Localizations
    {
        public static void AddLocalizations()
        {
            LocalizationModule.RegisterTranslation("MCC设置物品标题", "Select item", "选择物品", "Select item");
            LocalizationModule.RegisterTranslation("MCC设置物品说明", "After selecting at least one item, only the selected items can be placed on this conveyor belt path, and their <color=#FD965EC0>maximum cargo quantity on this belt path will be limited</color> below the value you entered. When an item's cargo quantity reaches the upper limit, the sorter will no longer be able to place that kind of item on this conveyor belt.\n1-4 piled items will always be counted as one cargo.", "选则至少一个物品后，只有所选物品才可以被放置在这条传送带上，且它们在传送带上的<color=#FD965EC0>货物数量上限将被限制</color>在玩家输入的数值以下。当某种货物达到设定的上限时，分拣器将无法继续向这条传送带上放置该物品。\n堆叠起来的货物算1个。\n\n当前，只有分拣器放入和取出的货物会被计算，若其他手段对传送带的货物产生了影响，可以通过点击传送带打开UI来手动纠正传送带上的货物数", "After selecting at least one item, only the selected items can be placed on this conveyor belt path, and their <color=#FD965EC0>maximum cargo quantity on this belt path will be limited</color> below the value you entered. When an item's cargo quantity reaches the upper limit, the sorter will no longer be able to place that kind of item on this conveyor belt.\n1-4 piled items will always be counted as one cargo.");

            LocalizationModule.RegisterTranslation("MCC重复物品提示", "Cannot select duplicate items", "不能选择重复的物品", "Cannot select duplicate items");
            LocalizationModule.RegisterTranslation("MCC混带设置", "Mix-Cargo Setting", "混带设置", "Mix-Cargo Setting");
        }
    }
}
