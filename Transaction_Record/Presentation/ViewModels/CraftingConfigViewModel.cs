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
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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

        public ObservableCollection<CraftingCondition> CraftingConditions { get; set; }
        public ObservableCollection<AffixSlot> AffixTypes { get; set; }
        public ObservableCollection<string> AffixTiers { get; set; }
        public ObservableCollection<RollType> RollTypes { get; set; }
        private readonly ICraftingConditionService _craftingConditionService;
        private readonly IMessageBoxService _messageBoxService;
        private readonly ISelectionDataProviderService _selectionDataProviderService;


        #endregion

        #region Commands
        public ICommand AddConditionCommand => new RelayCommand(this.AddCondition);
        public ICommand RemoveConditionCommand => new RelayCommand<CraftingCondition>(this.DeleteCondition);
        #endregion

        public CraftingConfigViewModel(
            ObservableCollection<CraftingCondition> craftingConditions,
            ICraftingConditionService craftingConditionService,
            IMouseAutomationService mouseAutomationService,
            ISelectionDataProviderService selectionDataProviderService,
            IMessageBoxService messageBoxService) : base(mouseAutomationService)
        {
            this._selectionDataProviderService = selectionDataProviderService;
            this.AffixTypes = selectionDataProviderService.GetAffixType();
            this.AffixTiers = selectionDataProviderService.GetAffixTier();
            this.RollTypes = selectionDataProviderService.GetRollType();
            this.CraftingConditions = craftingConditions;
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
            try
            {
                var craftingCondition = new CraftingCondition
                {
                    Keyword = this.Keyword,
                    AffixType = this.AffixType,
                    AffixTier = this.AffixTier
                };

                this._craftingConditionService.AddCondition(craftingCondition);
                this.LoadConditions();

                this.Keyword = string.Empty;
                this.AffixType = craftingCondition.AffixType;
                this.AffixTier = craftingCondition.AffixTier;
            }
            catch (Exception ex)
            {
                this._messageBoxService.ShowMessage($"新增條件失敗：{ex.Message}", "錯誤", MessageBoxImage.Error);
            }
        }

        private void DeleteCondition(CraftingCondition craftingCondition)
        {
            if (craftingCondition == null)
            {
                return;
            }

            this._craftingConditionService.DeleteCondition(craftingCondition.Id);
            this.LoadConditions();
        }

        private void LoadConditions()
        {
            var savedConditions = this._craftingConditionService.LoadCondition();
            this.CraftingConditions.Clear();

            foreach (var condition in savedConditions)
            {
                this.CraftingConditions.Add(condition);
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
