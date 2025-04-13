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
            LocalizationModule.RegisterTranslation("MCC设置物品说明", "On this conveyor belt, the maximum quantity of cargos for different items will be constrained below the value you entered. When a certain item's cargo quantity reaches the upper limit, the sorter will no longer be able to place that kind of item on this conveyor belt.\n1-4 piled items will always be counted as one cargo.", "在这条传送带上，不同物品的货物数量上限将被约束在玩家输入的数值以下。当某种货物达到设定的上限时，分拣器将无法继续向这条传送带上放置该物品。\n堆叠起来的货物算1个。", "On this conveyor belt, the maximum quantity of cargos for different items will be constrained below the value you entered. When a certain item's cargo quantity reaches the upper limit, the sorter will no longer be able to place that kind of item on this conveyor belt.\n1-4 piled items will always be counted as one cargo.");

            LocalizationModule.RegisterTranslation("MCC重复物品提示", "Cannot select duplicate items", "不能选择重复的物品", "Cannot select duplicate items");
        }
    }
}
