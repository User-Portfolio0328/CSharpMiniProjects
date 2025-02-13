using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.HtmlControls;
using System.Windows;
using System.Windows.Forms;
using Transaction_Record.Application.Interfaces;
using Transaction_Record.Application.Services;
using Transaction_Record.Domain.Enums;
using Transaction_Record.Domain.Interfaces;
using Transaction_Record.Presentation.ViewModels;
using WindowsInput.Native;


namespace Transaction_Record.Infrastructure.Services
{
    public class MouseAutomationService : IMouseAutomationService
    {
        #region Properties
        private Point _alterationOrbPosition { get; set; } // 改造石位置
        private Point _augmentationOrbPosition { get; set; } //增幅石位置
        private Point _transmutationOrbPosition { get; set; } //蛻變石位置
        private Point _scouringOrbPosition { get; set; } //重鑄石位置
        private Point _regalOrbPosition { get; set; } //富豪石位置
        private Point _craftItemPosition { get; set; } // 要製作的裝備位置
        private CancellationTokenSource _cancellationTokenSource;
        private ICraftingConditionService _craftingConditionService;
        private bool _isSelecting = false;
        private int _selectionStep = 0;
        #endregion

        #region P/Invoke
        private delegate IntPtr LowLevelKeyBoardProc(int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);
        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyBoardProc lpfn, IntPtr hMod, uint dwTreadId);
        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEX(IntPtr hhk);
        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        [Flags]
        private enum MouseEventFlags : uint
        {
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x008,
            RightUp = 0x0010
        }
        #endregion

        #region Hooks
        private const int WM_KEYDOWN = 0x0100;
        private const int WH_KEYBOARD_LL = 13;
        private IntPtr _hookID = IntPtr.Zero;
        private LowLevelKeyBoardProc _proc;
        #endregion

        public MouseAutomationService(ICraftingConditionService craftingConditionService)
        {
            this._craftingConditionService = craftingConditionService;
            this._proc = this.HookCallback;
            this.HookKeyBoard();
        }

        public void Dispose()
        {
            UnhookWindowsHookEX(this._hookID);
        }

        // 啟動滑鼠選擇模式
        private void StartSelection(object parameter = null)
        {
            this._isSelecting = true;
            this._selectionStep = 0;
            System.Windows.MessageBox.Show("請按 F2 來選擇位置, 請先選擇改造石位置", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // 選擇需要左右鍵滑鼠的位置
        private void SelectPosition()
        {
            if (!this._isSelecting)
            {
                return;
            }

            Point mousePosition = GetMousePosition();

            if (this._selectionStep == 0)
            {
                this._alterationOrbPosition = mousePosition;
                this._selectionStep++;
                System.Windows.MessageBox.Show("請設置蛻變石位置", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (_selectionStep == 1)
            {
                this._transmutationOrbPosition = mousePosition;
                this._selectionStep++;
                System.Windows.MessageBox.Show("請設置增幅石位置", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (_selectionStep == 2)
            {
                this._augmentationOrbPosition = mousePosition;
                this._selectionStep++;
                System.Windows.MessageBox.Show("請設置重鑄石位置", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (_selectionStep == 3)
            {
                this._scouringOrbPosition = mousePosition;
                this._selectionStep++;
                System.Windows.MessageBox.Show("請設置要製作的物品位置", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (_selectionStep == 4)
            {
                this._craftItemPosition = mousePosition;
                this._selectionStep++;
                this._isSelecting = false;
                this.ExecuteMouseClickAndCompare();
            }
        }

        private void StopAutomation() 
        {
            if (this._cancellationTokenSource != null) 
            {
                this._cancellationTokenSource.Cancel();
                System.Windows.MessageBox.Show("取消腳本自動化!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private static Point GetMousePosition()
        {
            return new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
        }

        // 開始進行滑鼠點擊操控
        private async void ExecuteMouseClickAndCompare()
        {
            this._cancellationTokenSource = new CancellationTokenSource();
            var token = this._cancellationTokenSource.Token;

            // 複製物品的內容
            await MoveCursorSmoothly((int)this._craftItemPosition.X, (int)this._craftItemPosition.Y);
            await Task.Delay(50);
            await this.CopyCraftItemProperty();
            string itemProperty = System.Windows.Clipboard.GetText();
            
            //若物品不是普通物品就用重鑄石變為普通物品
            if (this.GetCraftItemRarity(itemProperty) != "Normal")
            {
                await this.RightClickOnOrbAsync(this._scouringOrbPosition);
                await this.LeftClickOnCraftItemAsync();
            }

            while (!token.IsCancellationRequested)
            {
                await this.CopyCraftItemProperty();
                itemProperty = System.Windows.Clipboard.GetText();

                var test = this.GetCraftItemRarity(itemProperty);
                if (this.GetCraftItemRarity(itemProperty) == "Normal")
                {
                    await this.RightClickOnOrbAsync(this._transmutationOrbPosition);
                    await this.LeftClickOnCraftItemAsync();
                }
                else if (this.GetCraftItemRarity(itemProperty) == "Magic" &&
                    !this._craftingConditionService.IsAffixMatching(itemProperty) &&
                    this._craftingConditionService.NeedUseAugmentationOrb(itemProperty))
                {
                    await this.RightClickOnOrbAsync(this._augmentationOrbPosition);
                    await this.LeftClickOnCraftItemAsync();
                }
                else if (this.GetCraftItemRarity(itemProperty) == "Magic" && 
                    !this._craftingConditionService.IsAffixMatching(itemProperty))
                {
                    await this.RightClickOnOrbAsync(this._alterationOrbPosition);
                    await this.LeftClickOnCraftItemAsync();
                }
                // 比對複製的內容是否有目標詞綴, 若沒有的話則等待200毫秒進行下一次點擊循環, 若有的話則完成並結束
                else if (this._craftingConditionService.IsAffixMatching(itemProperty))
                {
                    System.Windows.MessageBox.Show("完成!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                }
                else
                {
                    await Task.Delay(200);
                }
            }
        }

        private static async Task MoveCursorSmoothly(int targetX, int targetY)
        {
            int steps = 20;
            GetCursorPos(out POINT currentPos);

            int startX = currentPos.X;
            int startY = currentPos.Y;

            for (int i = 0; i < steps; i++)
            {
                int newX = startX + (targetX - startX) * i / steps;
                int newY = startY + (targetY - startY) * i / steps;
                SetCursorPos(newX, newY);
                await Task.Delay(10);
            }
        }
        private static void SimulateMouseClick(int x, int y, MouseEventFlags flags)
        {
            SetCursorPos(x, y);
            mouse_event((uint)flags, 0, 0, 0, UIntPtr.Zero);
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (vkCode == 0x70) // F1按鍵
                    {

                        this.StartSelection();

                    }
                    else if (vkCode == 0x71) // F2按鍵
                    {
                        this.SelectPosition();
                    }
                    else if (vkCode == 0x72) // F3按鍵
                    {
                        this.StopAutomation();
                    }
                });
            }
            return CallNextHookEx(this._hookID, nCode, wParam, lParam);
        }

        private void HookKeyBoard()
        {
            this._hookID = SetHook(this._proc);
        }

        private static IntPtr SetHook(LowLevelKeyBoardProc proc)
        {
            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private async Task CopyCraftItemProperty() 
        {
            // 使用Ctrl + C 複製物品屬性內容
            SendKeys.SendWait("^%c");
            await Task.Delay(100);
        }

        // 使用石
        private async Task RightClickOnOrbAsync(Point orbPosition)
        {
            // 移動到改造石上並點擊右鍵
            await MoveCursorSmoothly((int)orbPosition.X, (int)orbPosition.Y);
            await Task.Delay(50);
            SimulateMouseClick((int)orbPosition.X, (int)orbPosition.Y, MouseEventFlags.RightDown);
            await Task.Delay(50);
            SimulateMouseClick((int)orbPosition.X, (int)orbPosition.Y, MouseEventFlags.RightUp);
            await Task.Delay(250);
        }

        // 點擊要製作的物品
        private async Task LeftClickOnCraftItemAsync() 
        {
            // 移動到要製作的物品上並點擊左鍵
            await MoveCursorSmoothly((int)this._craftItemPosition.X, (int)this._craftItemPosition.Y);
            await Task.Delay(50);
            SimulateMouseClick((int)this._craftItemPosition.X, (int)this._craftItemPosition.Y, MouseEventFlags.LeftDown);
            await Task.Delay(50);
            SimulateMouseClick((int)this._craftItemPosition.X, (int)this._craftItemPosition.Y, MouseEventFlags.LeftUp);
            await Task.Delay(250);
        }
        private string GetCraftItemRarity(string itemProperty) 
        {
            return this._craftingConditionService.ExtractValueByKeyword(itemProperty, @"Rarity:\s*(\w+)");
        }
    }
}
