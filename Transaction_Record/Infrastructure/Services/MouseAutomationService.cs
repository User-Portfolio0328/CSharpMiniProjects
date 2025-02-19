using MahApps.Metro.Controls;
using System;
using System.Diagnostics;
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
        private bool _isSelecting = false;
        private int _selectionStep = 0;
        public event Action<int> PositionSelected;
        public event Action OnStopRequested;
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

        public MouseAutomationService()
        {
            this._proc = this.HookCallback;
            this.HookKeyBoard();
            this._cancellationTokenSource = new CancellationTokenSource();
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
            this.PositionSelected?.Invoke(this._selectionStep);
            this._selectionStep++;
        }

        // 選擇需要左右鍵滑鼠的位置
        private void SelectPosition()
        {
            if (!this._isSelecting)
            {
                return;
            }

            if (this.PositionSelected == null)
            {
                throw new InvalidOperationException("尚未訂閱事件，無法紀錄");
            }

            Point mousePosition = GetMousePosition();

            switch (this._selectionStep)
            {
                case 1:
                    this._alterationOrbPosition = mousePosition;
                    break;
                case 2:
                    this._transmutationOrbPosition = mousePosition;
                    break;
                case 3:
                    this._augmentationOrbPosition = mousePosition;
                    break;
                case 4:
                    this._scouringOrbPosition = mousePosition;
                    break;
                case 5:
                    this._craftItemPosition = mousePosition;
                    this._isSelecting = false;
                    break;
            }

            this.PositionSelected?.Invoke(this._selectionStep);
            this._selectionStep++;
        }

        private void StopAutomation()
        {
            if (this._cancellationTokenSource != null)
            {
                this.OnStopRequested?.Invoke();
                this._cancellationTokenSource.Cancel();
                this._selectionStep = -1;
                this.PositionSelected?.Invoke(this._selectionStep);
            }
        }

        private static Point GetMousePosition()
        {
            return new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
        }

        private static async Task MoveMouseSmoothly(int targetX, int targetY)
        {
            int steps = 10;
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

        // 使用通貨石頭
        private async Task RightClickOnOrbAsync(Point orbPosition)
        {
            // 移動到改造石上並點擊右鍵
            await MoveMouseSmoothly((int)orbPosition.X, (int)orbPosition.Y);
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
            await this.MoveMouseToCraftItem();
            SimulateMouseClick((int)this._craftItemPosition.X, (int)this._craftItemPosition.Y, MouseEventFlags.LeftDown);
            await Task.Delay(50);
            SimulateMouseClick((int)this._craftItemPosition.X, (int)this._craftItemPosition.Y, MouseEventFlags.LeftUp);
            await Task.Delay(250);
        }

        // 複製要製作的物品屬性
        public async Task CopyCraftItemProperty()
        {
            // 使用Ctrl + C 複製物品屬性內容
            SendKeys.SendWait("^%c");
            await Task.Delay(100);
        }

        // 移動到要製作的物品上
        public async Task MoveMouseToCraftItem() 
        {
            await MoveMouseSmoothly((int)this._craftItemPosition.X, (int)this._craftItemPosition.Y);
            await Task.Delay(50);
        }

        // 在製作物品上使用改造石
        public async Task ClickAlterationOrbOnItemAsync()
        {
            await this.RightClickOnOrbAsync(this._alterationOrbPosition);
            await this.LeftClickOnCraftItemAsync();
        }

        // 在製作物品上使用增幅石
        public async Task ClickAugmentationOrbOnItemAsync()
        {
            await this.RightClickOnOrbAsync(this._augmentationOrbPosition);
            await this.LeftClickOnCraftItemAsync();
        }

        // 在製作物品上使用蛻變石
        public async Task ClickTransmutationOrbOnItemAsync()
        {
            await this.RightClickOnOrbAsync(this._transmutationOrbPosition);
            await this.LeftClickOnCraftItemAsync();
        }

        // 在製作物品上使用重鑄石
        public async Task ClickScouringOrbOnItemAsync()
        {
            await this.RightClickOnOrbAsync(this._scouringOrbPosition);
            await this.LeftClickOnCraftItemAsync();
        }

        // 在製作物品上使用富豪石
        public async Task ClickRegalOrbOnItemAsync()
        {
            await this.RightClickOnOrbAsync(this._regalOrbPosition);
            await this.LeftClickOnCraftItemAsync();
        }
    }
}
