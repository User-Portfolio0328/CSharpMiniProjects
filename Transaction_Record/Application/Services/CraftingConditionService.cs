using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Transaction_Record.Application.Interfaces;
using Transaction_Record.Domain;
using Transaction_Record.Domain.Enums;
using Transaction_Record.Infrastructure;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;

namespace Transaction_Record.Application.Services
{
    public class CraftingConditionService : ICraftingConditionService
    {
        private readonly ObservableCollection<CraftingCondition> _craftingConditions;
        private readonly Dictionary<string, string> _tierMapping = new Dictionary<string, string>()
        {
            { "T1", "Tier: 1" },
            { "T2", "Tier: 2" },
            { "T3", "Tier: 3" },
            { "T4", "Tier: 4" },
            { "T5", "Tier: 5" },
            { "T6", "Tier: 6" },
            { "T7", "Tier: 7" },
            { "T8", "Tier: 8" },
            { "T9", "Tier: 9" },
            { "T10", "Tier: 10" },
            { "T11", "Tier: 11" },
            { "T12", "Tier: 12" },
            { "T13", "Tier: 13" },
            { "T14", "Tier: 14" },
            { "T15", "Tier: 15" },
            { "T16", "Tier: 16" },
            { "T17", "Tier: 17" },
            { "T18", "Tier: 18" },
            { "T19", "Tier: 19" }
        };

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

        public CraftingConditionService(ObservableCollection<CraftingCondition> craftingConditions)
        {
            this._craftingConditions = craftingConditions;
        }

        // 比對詞綴是否符合設定
        public bool IsAffixMatching(string itemProperty)
        {
            int prefixCount = 0, suffixCount = 0;

            // 以詞綴欄區分詞綴總條目數
            var groupedConditions = this._craftingConditions
                .GroupBy(condition => condition.AffixType)
                .ToDictionary(group => group.Key, group => group.ToList());
            var itemAffixList = this.ParseAffixes(itemProperty);

            foreach (var group in groupedConditions)
            {
                foreach (var itemAffix in itemAffixList)
                {
                    var AffixModifier = this.ConverToAffixModifier(group.Key); //製作物品條件上的前後綴
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

                    var matchingAffix = hasKeyword && isAffixModifierMatch && isTierMatch;

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
            return prefixCount >= groupedConditions.Count(s => this.ConverToAffixModifier(s.Key) == "Prefix") &&
                   suffixCount >= groupedConditions.Count(s => this.ConverToAffixModifier(s.Key) == "Suffix");
        }

        // 解析物品詞綴與詞綴階級的表單
        private List<(string keyword, string affixModifier, int tier)> ParseAffixes(string itemProperties)
        {
            var affixes = new List<(string keyword, string affixModifier, int tier)>();
            // 正規表達匹配的前後綴
            var prefixRegex = new Regex(@"{ (Prefix) Modifier .*? \(Tier:\s*(\d+)\)(?:\s*—\s*[^}]+)?\s* }([\s\S]+?)(?=\r?\n{|\r?\n*Suffix Modifier|\r?\n(?:--------|$))", RegexOptions.Multiline);
            var suffixRegex = new Regex(@"{ (Suffix) Modifier .*? \(Tier:\s*(\d+)\)(?:\s*—\s*[^}]+)?\s* }([\s\S]+?)(?=\r?\n(?:--------|$))", RegexOptions.Multiline);
            
            
            string test1 = prefixRegex.ToString();
            string test = suffixRegex.ToString();

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

        // 是否需要使用增幅石
        public bool NeedUseAugmentationOrb(string itemProperties)
        {
            var affixes = this.ParseAffixes(itemProperties);

            // 以詞綴欄區分詞綴總條目數
            var groupedConditions = this._craftingConditions
                .GroupBy(condition => condition.AffixType)
                .ToDictionary(group => group.Key, group => group.ToList());

            // 若物品只有一條詞綴且設定上的詞綴欄超過一個
            if (affixes.Count == 1 && groupedConditions.Count > 1)
            {
                return true;
            }
            return false;
        }

        public string ExtractValueByKeyword(string itemProperties, string regex)
        {
            Match match = Regex.Match(itemProperties, regex);
            string rarity = match.Groups[1].Value;
            return rarity;
        }
    }
}
