using MaterialDesignThemes.Wpf.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.HtmlControls;
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
        private readonly IMouseAutomationService _mouseAutomationService;

        #endregion

        #region Commands
        public ICommand AddConditionCommand => new RelayCommand(this.AddCondition);
        public ICommand RemoveConditionCommand => new RelayCommand<CraftingCondition>(this.RemoveCondition);
        #endregion

        public CraftingConfigViewModel(
            ICraftingConditionService craftingConditionService,
            ICraftingConfigRepository craftingConfigRepository,
            IMouseAutomationService mouseAutomationService)
        {
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
            this.CraftingConditions = new ObservableCollection<CraftingCondition>();
            this._craftingConfigRepository = craftingConfigRepository;
            this._craftingConditionService = craftingConditionService;
            this._mouseAutomationService = mouseAutomationService;
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
    }
}
