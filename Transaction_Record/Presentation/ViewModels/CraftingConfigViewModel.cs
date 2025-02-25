using MaterialDesignThemes.Wpf.Converters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.HtmlControls;
using System.Windows;
using System.Windows.Input;
using Transaction_Record.Application.Interfaces;
using Transaction_Record.Application.Services;
using Transaction_Record.Domain;
using Transaction_Record.Domain.Enums;
using Transaction_Record.Domain.Interfaces;
using Transaction_Record.Infrastructure;
using Transaction_Record.Infrastructure.Services;
using Transaction_Record.Presentation.Commands;

namespace Transaction_Record.Presentation.ViewModels
{
    public class CraftingConfigViewModel : BaseViewModel
    {
        #region Properties
        private RollType _rollType;
        public RollType RollType
        {
            get => this._rollType;
            set
            {
                this._rollType = value;
                this.OnPropertyChanged(nameof(this.RollType));
            }
        }

        private string _keyword;
        public string Keyword
        {
            get => this._keyword;
            set
            {
                this._keyword = value;
                this.OnPropertyChanged(nameof(this.Keyword));
            }
        }

        private AffixSlot _affixType;
        public AffixSlot AffixType
        {
            get => this._affixType;
            set
            {
                this._affixType = value;
                this.OnPropertyChanged(nameof(this.AffixType));
            }
        }

        private string _affixTier;
        public string AffixTier
        {
            get => this._affixTier;
            set
            {
                this._affixTier = value;
                this.OnPropertyChanged(nameof(this.AffixTier));
            }
        }

        public ObservableCollection<AffixSlot> AffixTypes { get; set; }
        public ObservableCollection<string> AffixTiers { get; set; }
        public ObservableCollection<RollType> RollTypes { get; set; }
        public ObservableCollection<CraftingCondition> CraftingConditions { get; set; }
        private readonly ICraftingConfigRepository _craftingConfigRepository;
        private readonly ICraftingConditionService _craftingConditionService;
        private readonly IMessageBoxService _messageBoxService;

        #endregion

        #region Commands
        public ICommand AddConditionCommand => new RelayCommand(this.AddCondition);
        public ICommand RemoveConditionCommand => new RelayCommand<CraftingCondition>(this.RemoveCondition);
        #endregion

        public CraftingConfigViewModel(
            IServiceProvider serviceProvider,
            ICraftingConditionService craftingConditionService,
            ICraftingConfigRepository craftingConfigRepository,
            IMouseAutomationService mouseAutomationService,
            IMessageBoxService messageBoxService,
            ObservableCollection<CraftingCondition> craftingCondition) : base(mouseAutomationService)
        {
            this.CraftingConditions = craftingCondition;
            this.AffixTypes = new ObservableCollection<AffixSlot>((AffixSlot[])Enum.GetValues(typeof(AffixSlot)));
            this.AffixTiers = new ObservableCollection<string>
            {
                "T1",
                "T2",
                "T3",
                "T4",
                "T5",
                "T6",
                "T7",
                "T8",
                "T9",
                "T10",
                "T11",
                "T12",
                "T13",
                "T14",
                "T15",
                "T16",
                "T17",
                "T18",
                "T19"
            };
            this.RollTypes = new ObservableCollection<RollType>((RollType[])Enum.GetValues(typeof(RollType)));
            this._craftingConfigRepository = craftingConfigRepository;
            this._craftingConditionService = craftingConditionService;
            this._messageBoxService = messageBoxService;
            this.LoadConditions();
        }

        private void AddCondition(object parameter)
        {
            if (string.IsNullOrWhiteSpace(this.Keyword))
            {
                return;
            }

            var craftingCondition = new CraftingCondition
            {
                Keyword = this.Keyword,
                AffixType = this.AffixType,
                AffixTier = this.AffixTier
            };
            this.CraftingConditions.Add(craftingCondition);
            this.SaveConfig();

            this.Keyword = string.Empty;
            this.AffixType = craftingCondition.AffixType;
            this.AffixTier = craftingCondition.AffixTier;
        }

        private void RemoveCondition(CraftingCondition craftingCondition)
        {
            if (craftingCondition == null)
            {
                return;
            }

            this.CraftingConditions.Remove(craftingCondition);
            this.SaveConfig();
        }

        private void SaveConfig()
        {
            this._craftingConfigRepository.SaveCondition(this.CraftingConditions);
        }

        private void LoadConditions()
        {
            var savedConditions = this._craftingConfigRepository.LoadCondition();

            if (savedConditions != null && savedConditions.Any())
            {
                this.CraftingConditions.Clear();
                foreach (var condition in savedConditions)
                {
                    this.CraftingConditions.Add(condition);
                }
            }
        }

        protected override void OnPositionSelected(int step)
        {
            string contain;

            switch (step)
            {
                case -1:
                    contain = "取消腳本自動化!";
                    break;
                case 0:
                    contain = "請按 F2 來選擇位置, 請先選擇改造石位置";
                    break;
                case 1:
                    contain = "請設置蛻變石位置";
                    break;
                case 2:
                    contain = "請設置增幅石位置";
                    break;
                case 3:
                    contain = "請設置重鑄石位置";
                    break;
                case 4:
                    contain = "請設置富豪石位置";
                    break;
                case 5:
                    contain = "請設置要製作的物品位置";
                    break;
                case 6:
                    contain = "開始製作!";
                    break;
                default:
                    contain = "未知";
                    break;
            }

            this._messageBoxService.ShowMessage(contain, "提示");

            if (step == 6)
            {
                this._craftingConditionService.ExecuteMouseClickAndCompare(this._rollType);
            }
        }
    }
}
