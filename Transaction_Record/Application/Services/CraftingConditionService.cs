using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Transaction_Record.Application.Interfaces;
using Transaction_Record.Domain;
using Transaction_Record.Domain.Enums;
using Transaction_Record.Domain.Interfaces;
using Transaction_Record.Infrastructure;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;

namespace Transaction_Record.Application.Services
{
    public class CraftingConditionService : ICraftingConditionService
    {
        #region Properties
        private readonly ObservableCollection<CraftingCondition> _craftingConditions;
        private readonly IMouseAutomationService _mouseAutomationService;
        private CancellationTokenSource _cancellationTokenSource;
        #endregion

        private readonly Dictionary<AffixSlot, string> _affixSlotMapping = new Dictionary<AffixSlot, string>()
        {
            { AffixSlot.第一條前綴, "Prefix" },
            { AffixSlot.第二條前綴, "Prefix" },
            { AffixSlot.第三條前綴, "Prefix" },
            { AffixSlot.第四條前綴, "Prefix" },
            { AffixSlot.第一條後綴, "Suffix" },
            { AffixSlot.第二條後綴, "Suffix" },
            { AffixSlot.第三條後綴, "Suffix" },
            { AffixSlot.第四條後綴, "Suffix" }
        };

        public CraftingConditionService(ObservableCollection<CraftingCondition> craftingConditions, IMouseAutomationService mouseAutomationService)
        {
            this._craftingConditions = craftingConditions;
            this._mouseAutomationService = mouseAutomationService;
            this._mouseAutomationService.OnStopRequested += this.StopAutomation;
        }

        // 比對在設定條件上與物品的詞綴相符的數量
        private (int, int) AffixMatchingCount(string itemProperty)
        {
            int prefixCount = 0, suffixCount = 0;

            // 以詞綴欄區分詞綴總條目數
            var groupedConditions = this.GroupedCraftingItemConditions();
            var itemAffixList = this.ParseAffixes(itemProperty);

            // 比對設定條件與物品上的詞綴
            foreach (var group in groupedConditions)
            {
                foreach (var itemAffix in itemAffixList)
                {
                    var AffixModifier = this.ConverToAffixModifier(group.Key); // 獲取製作物品條件上的前後綴
                    var conditions = group.Value;

                    // 是否包含關鍵字
                    bool hasKeyword = conditions
                        .Any(condition => itemAffix.keyword
                        .IndexOf(condition.Keyword, StringComparison.OrdinalIgnoreCase) >= 0);

                    //前後綴是否符合
                    bool isAffixModifierMatch = false;

                    // 詞綴階級是否符合
                    bool isTierMatch = false;

                    if (hasKeyword)
                    {
                        //前後綴是否與物品的前後綴相同
                        isAffixModifierMatch = itemAffix.affixModifier == AffixModifier;
                    }

                    if (isAffixModifierMatch)
                    {
                        // 詞綴階級是否大於或等於物品的詞綴階級
                        isTierMatch = conditions.Any(condition => itemAffix.tier <= int.Parse(condition.AffixTier.Replace("T", "")));
                    }

                    // 若 關鍵字 與 前後綴 與 詞綴階級 都相符即符合
                    var matchingAffix = hasKeyword && isAffixModifierMatch && isTierMatch;

                    // 確認是哪一條前(後)綴正確
                    if (matchingAffix)
                    {
                        if (itemAffix.affixModifier == "Prefix")
                        {
                            prefixCount++;
                        }
                        else if (itemAffix.affixModifier == "Suffix")
                        {
                            suffixCount++;
                        }
                    }
                }
            }
            return (prefixCount, suffixCount);
        }

        // 比對物品上的詞綴是否與設定條件相符
        private bool IsAffixMatching(int prefixCount, int suffixCount)
        {
            bool isMatching = prefixCount >= this.GroupedCraftingItemConditions().Count(s => this.ConverToAffixModifier(s.Key) == "Prefix") &&
                   suffixCount >= this.GroupedCraftingItemConditions().Count(s => this.ConverToAffixModifier(s.Key) == "Suffix");

            return isMatching;
        }

        // 將要製作物品的需求詞綴做分組 (例如:第一條前綴的一組, 第二條前綴的一組, 第一條後綴的一組....)
        private Dictionary<AffixSlot, List<CraftingCondition>> GroupedCraftingItemConditions()
        {
            return this._craftingConditions
                .GroupBy(condition => condition.AffixType)
                .ToDictionary(group => group.Key, group => group.ToList());
        }

        // 開始進行滑鼠點擊操控
        public async Task ExecuteMouseClickAndCompare(RollType rollType)
        {
            this._cancellationTokenSource = new CancellationTokenSource();
            var token = this._cancellationTokenSource.Token;

            // 複製物品的內容
            await this._mouseAutomationService.MoveMouseToCraftItem();
            await this._mouseAutomationService.CopyCraftItemProperty();
            var itemProperty = System.Windows.Clipboard.GetText();
            var itemRarity = this.GetCraftItemRarity(itemProperty);

            // 若物品不是普通物品就用重鑄石變為普通物品
            if (itemRarity != "Normal")
            {
                await this._mouseAutomationService.ClickScouringOrbOnItemAsync();
            }

            while (!token.IsCancellationRequested)
            {
                await this._mouseAutomationService.CopyCraftItemProperty();
                itemProperty = System.Windows.Clipboard.GetText();
                itemRarity = this.GetCraftItemRarity(itemProperty);
                var AffixCount = this.AffixMatchingCount(itemProperty);
                var isAffixMatching = this.IsAffixMatching(AffixCount.Item1, AffixCount.Item2);

                // 若為普通物品就使用蛻變石
                if (itemRarity == "Normal")
                {
                    await this._mouseAutomationService.ClickTransmutationOrbOnItemAsync();
                }
                // 若為魔法物品 且 且需要使用富豪來做裝 且 需要使用富豪石 就使用富豪石
                else if (itemRarity == "Magic" &&
                    rollType == RollType.改造配增幅配富豪 &&
                    this.GetOrbUse(itemProperty, AffixCount.Item1, AffixCount.Item2) == "Regal Orb")
                {
                    await this._mouseAutomationService.ClickRegalOrbOnItemAsync();
                }
                // 若為魔法物品 且 需要使用增幅石就使用增幅石
                else if (itemRarity == "Magic" &&
                    this.GetOrbUse(itemProperty, AffixCount.Item1, AffixCount.Item2) == "Augmentation Orb")
                {
                    await this._mouseAutomationService.ClickAugmentationOrbOnItemAsync();
                }
                // 若為魔法物品 且 匹配不相同就使用改造石
                else if (itemRarity == "Magic" &&
                    !isAffixMatching)
                {
                    await this._mouseAutomationService.ClickAlterationOrbOnItemAsync();
                }
                // 若為稀有物品 且 匹配不相同就使用重鑄石
                else if (itemRarity == "Rare" &&
                    !isAffixMatching)
                {
                    await this._mouseAutomationService.ClickScouringOrbOnItemAsync();
                }
                // 比對複製的內容是否有目標詞綴, 若沒有的話則等待200毫秒進行下一次點擊循環, 若有的話則完成並結束
                else if (isAffixMatching)
                {
                    System.Windows.MessageBox.Show("完成!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                }
                await Task.Delay(100);
            }
        }

        // 是否需要使用增幅石
        private string GetOrbUse(string itemProperty, int prefixMatchCount, int suffixMatchCount)
        {
            if (this._craftingConditions == null)
            {
                throw new ArgumentException("Condition not yet set");
            }

            //獲取裝備上的詞綴
            var affixes = this.ParseAffixes(itemProperty);
            // 以詞綴欄區分詞綴總條目數與詞綴條件
            var groupedConditions = this._craftingConditions
                .GroupBy(condition => condition.AffixType)
                .ToDictionary(group => group.Key, group => group.ToList());
            bool isAffixMatch = affixes
                .Any(affix => groupedConditions
                .Any(group => affix.affixModifier == this.ConverToAffixModifier(group.Key)));

            // 獲取設定條件上的前綴數量
            int conditionPerfixcount = groupedConditions.Count(group => this.ConverToAffixModifier(group.Key) == "Prefix");
            // 獲取設定條件上的後綴數量
            int conditionSuffixcount = groupedConditions.Count(group => this.ConverToAffixModifier(group.Key) == "Suffix");
            // 是否只有前綴並且超過1個
            bool hasOnlyMultiplePrefixConditions = conditionPerfixcount > 1 && conditionSuffixcount == 0;
            // 是否只有後綴並且超過1個
            bool hasOnlyMultipleSuffix = conditionPerfixcount == 0 && conditionSuffixcount > 1;

            // 若物品上的前後綴總共只有1條 且 正確 且 設定條件上的詞綴欄超過一個，就使用增幅石
            if (affixes.Count == 1 &&
                prefixMatchCount + suffixMatchCount == 1 &&
                groupedConditions.Count > 1)
            {
                return "Augmentation Orb";
            }
            // 若物品上的前後綴總共只有1條 且 需要的前(後)綴在不在裝備上，就使用增幅石
            else if (affixes.Count == 1 &&
                !isAffixMatch)
            {
                return "Augmentation Orb";
            }
            // 若物品上的前後綴總共2條 且 2條都正確 且 設定條件上的詞綴欄超過2個，就使用富豪石
            else if (affixes.Count == 2 &&
                prefixMatchCount + suffixMatchCount == 2 &&
                groupedConditions.Count > 2)
            {
                return "Regal Orb";
            }
            // 若物品上的前後綴總共2條 且 其中1個正確 且 設定條件上的前(後)綴為只有多個前綴或多個後綴，就使用富豪石
            else if (affixes.Count == 2 &&
                prefixMatchCount + suffixMatchCount == 1 &&
                hasOnlyMultiplePrefixConditions &&
                hasOnlyMultipleSuffix)
            {
                return "Regal Orb";
            }
            return "Unknown";
        }

        private string ExtractValueByKeyword(string itemProperties, string regex)
        {
            Match match = Regex.Match(itemProperties, regex);
            string rarity = match.Groups[1].Value;
            return rarity;
        }

        // 解析物品詞綴與詞綴階級的表單
        private List<(string keyword, string affixModifier, int tier)> ParseAffixes(string itemProperties)
        {
            var affixes = new List<(string keyword, string affixModifier, int tier)>();
            // 正規表達獲取裝備上的前後綴
            var prefixRegex = new Regex(@"{ (Prefix) Modifier .*? \(Tier:\s*(\d+)\)(?:\s*—\s*[^}]+)?\s* }([\s\S]+?)(?=\r?\n{|\r?\n*Suffix Modifier|\r?\n(?:--------|$))", RegexOptions.Multiline);
            var suffixRegex = new Regex(@"{ (Suffix) Modifier .*? \(Tier:\s*(\d+)\)(?:\s*—\s*[^}]+)?\s* }([\s\S]+?)(?=\r?\n(?:--------|$))", RegexOptions.Multiline);

            this.MatchAffixes(prefixRegex, itemProperties, affixes);
            this.MatchAffixes(suffixRegex, itemProperties, affixes);

            return affixes;
        }

        // 製作物品詞綴表
        private void MatchAffixes(
            Regex regex,
            string itemProperties,
            List<(string keyword, string affixModifier, int tier)> affixes)
        {
            var matches = regex.Matches(itemProperties);
            foreach (Match match in matches)
            {
                string affixModifier = match.Groups[1].Value;
                int tier = int.Parse(match.Groups[2].Value);
                string keyword = match.Groups[3].Value.Trim();
                affixes.Add((keyword, affixModifier, tier));
            }
        }

        private string ConverToAffixModifier(AffixSlot affixSlot)
        {
            return this._affixSlotMapping.TryGetValue(affixSlot, out string AffixModifier) ? AffixModifier : "Unknow modifier";
        }

        private string GetCraftItemRarity(string itemProperty)
        {
            return this.ExtractValueByKeyword(itemProperty, @"Rarity:\s*(\w+)");
        }

        private void StopAutomation()
        {
            if (this._cancellationTokenSource != null)
            {
                this._cancellationTokenSource.Cancel();
                this._cancellationTokenSource.Dispose();
                this._cancellationTokenSource = null;
            }
        }

        private void RollCraftItemAction(RollType rollType)
        {
            string rollyTypeContain;

            switch (rollType)
            {
                case RollType.改造配幅石:
                    rollyTypeContain = "改造配增幅";
                    break;
                case RollType.改造配增幅配富豪:
                    rollyTypeContain = "改造配增幅配富豪";
                    break;
                default:
                    rollyTypeContain = "未知";
                    break;
            }
        }
    }
}
