using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;
using WindowsInput;

namespace Exceed
{
    public partial class ExceedMain : Form
    {
        string datafilePath = "KeyStore.dat";
        string settingfilePath = "Setting.dat";

        public ExceedMain()
        {
            InitializeComponent();
            m_KeyboardHookManager.KeyDown += m_KeyboardHookManager_KeyDown;
            m_KeyboardHookManager.KeyUp += m_KeyboardHookManager_KeyUp;
            m_MouseHookManager.MouseMove += m_MouseHookManager_MouseMove;
            m_MouseHookManager.MouseWheel += m_MouseHookManager_MouseWheel;
            m_MouseHookManager.MouseDown += m_MouseHookManager_MouseDown;
            m_MouseHookManager.MouseUp += m_MouseHookManager_MouseUp;
            m_KeyboardHookManager.Enabled = true;

            LoadData();
            Exceeder.RunWorkerAsync();
        }


        public bool isRunning = false; // Exceed System is Runnning?
        Stimulus Tick; // ThisTick Keyboard/Mouse Event
        List<Order> ExceedOrders = new List<Order>(); // Order List

        public void Start()
        {
            isRunning = true;
            m_MouseHookManager.Enabled = true;
            m_KeyboardHookManager.Enabled = true;

            label1.Text = "Exceed is running.";
            this.Text = "Exceed : Running";
            button1.Text = "Stop";
            button2.Enabled = false;
            대기중ToolStripMenuItem.Text = "작동중";
            설정ToolStripMenuItem.Enabled = false;
            notifyIcon1.ShowBalloonTip(3, "Exceed", "Exceed is Working", ToolTipIcon.None);
        }

        public void Stop()
        {
            isRunning = false;
            m_MouseHookManager.Enabled = false;
            m_KeyboardHookManager.Enabled = false;

            label1.Text = "Exceed was suspend.";
            this.Text = "Exceed";
            button2.Enabled = true;
            button1.Text = "Run";
            대기중ToolStripMenuItem.Text = "대기중";
            설정ToolStripMenuItem.Enabled = true;
            notifyIcon1.ShowBalloonTip(3, "Exceed", "Exceed is Suspend", ToolTipIcon.None);
        }

        private readonly KeyboardHookListener m_KeyboardHookManager = new KeyboardHookListener(new GlobalHooker());
        private readonly MouseHookListener m_MouseHookManager = new MouseHookListener(new GlobalHooker());

        VirtualKeyCode lastPressedKey;
        bool leftkeyPressing, rightKeyPressing, MiddleKeyPressing;

        #region KeyBoard

        void m_KeyboardHookManager_KeyUp(object sender, KeyEventArgs e)
        {
            lastPressedKey = unchecked((VirtualKeyCode)(-1));
        }

        void m_KeyboardHookManager_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12)
            {
                if (!isRunning) Start();
                else Stop();
            }

#if DEBUG
            if (e.KeyCode == Keys.F11)
                InputSimulator.SimulateKeyDown(VirtualKeyCode.LSHIFT);
#endif
            lastPressedKey = (VirtualKeyCode)e.KeyData;
            if (ExceedOrders.Find(x =>
                (x.blockKey == true) &&
                (x.condition.PressingKey == (VirtualKeyCode)e.KeyData)
                ) != null) e.SuppressKeyPress = true;
        }

        #endregion

        #region Mouse

        Point LeftMouseLocation = new Point();

        void m_MouseHookManager_MouseMove(object sender, MouseEventArgs e)
        {
            if (LeftMouseLocation.X > e.X)
            {
                Tick.XPosDecreased = true;
            }
            else if (LeftMouseLocation.X < e.X)
            {
                Tick.XPosIncreased = true;
            }

            if (LeftMouseLocation.Y > e.Y)
            {
                Tick.YPosDecreased = true;
            }
            else if (LeftMouseLocation.Y < e.Y)
            {
                Tick.YPosIncreased = true;
            }
            LeftMouseLocation = e.Location;
        }

        void m_MouseHookManager_MouseUp(object sender, MouseEventArgs e)
        {
            leftkeyPressing = rightKeyPressing = MiddleKeyPressing = false;
        }

        void m_MouseHookManager_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                leftkeyPressing = true;
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                rightKeyPressing = true;
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                MiddleKeyPressing = true;
            }
        }

        void m_MouseHookManager_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                Tick.WheelIncreased = true;
            }
            else if (e.Delta < 0)
            {
                Tick.WheelDecreased = true;
            }
        }


        #endregion

        Stimulus TickedStimuls;
        int Delay = 33;

        private void Exceeder_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (isRunning)
                {
                    TickedStimuls = Tick;

                    if (lastPressedKey != unchecked((VirtualKeyCode)(-1))) TickedStimuls.PressingKey = lastPressedKey;
                    if (leftkeyPressing) TickedStimuls.LBDown = true;
                    if (rightKeyPressing) TickedStimuls.RBDown = true;
                    if (MiddleKeyPressing) TickedStimuls.MBDown = true;

#if DEBUG
                    //Console.WriteLine(TickedStimuls.ToString());
#endif

                    foreach (var item in ExceedOrders)
                    {
                        item.CheackOrder(TickedStimuls);
                    }
                    Tick.Init();
                    Thread.Sleep(Delay);
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.isExitToHide && this.Visible)
            {
                HIDE();
                e.Cancel = true; ;
            }
            else
            {
                foreach (var item in ExceedOrders)
                {
                    item.Release();
                    InputSimulator.SimulateKeyUp(item.reaction.PressingKey);
                }

                SaveData();
            }
        }

        bool isExitToHide;

        private void button2_Click(object sender, EventArgs e)
        {
            OpenSetting();
        }

        void OpenSetting()
        {
            var frm = new ExceedSetting(ExceedOrders);
            frm.isAlwaysOnTop = this.TopMost;
            frm.isExitToHide = this.isExitToHide;
            var await = frm.ShowDialog();
            this.TopMost = frm.isAlwaysOnTop;
            this.isExitToHide = frm.isExitToHide;
            this.ExceedOrders = frm.myOrders.ToList();

        }

        private bool LoadData()
        {
            try
            {
                using (System.IO.Stream ReadStream = new FileStream(datafilePath, FileMode.Open))
                {
                    BinaryFormatter binFormatter = new BinaryFormatter();
                    ExceedOrders = binFormatter.Deserialize(ReadStream) as List<Order>;
                }
                using (System.IO.Stream ReadStream = new FileStream(settingfilePath, FileMode.Open))
                {
                    var binFormatter = new StreamReader(ReadStream);
                    this.TopMost = bool.Parse(binFormatter.ReadLine());
                    this.isExitToHide = bool.Parse(binFormatter.ReadLine());
                    binFormatter.Close();
                }
                return true;
            }
            catch
            {
                return false;
            }

        }

        private void SaveData()
        {
            using (var WriteStream = new FileStream(datafilePath, FileMode.Create))
            {
                var binFormatter = new BinaryFormatter();
                binFormatter.Serialize(WriteStream, ExceedOrders);
            }
            using (var WriteStream = new FileStream(settingfilePath, FileMode.Create))
            {
                var binFormatter = new StreamWriter(WriteStream);
                binFormatter.WriteLine(this.TopMost);
                binFormatter.WriteLine(this.isExitToHide);
                binFormatter.Close();
            }
        }

        private void button1_MouseDown(object sender, MouseEventArgs e)
        {
            if (isRunning) Stop();
            else Start();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.Visible == false) this.Show();
        }

        private void 설정ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenSetting();
        }

        private void 종료ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Hide();
            this.Close();
        }

        private void 보이기ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (보이기ToolStripMenuItem.Text == "보이기") this.Show();
            else
            {
                HIDE();
            }
        }

        void HIDE()
        {
            this.Hide();
            보이기ToolStripMenuItem.Text = "보이기";
            notifyIcon1.ShowBalloonTip(3, "Exceed", "Hey! I'm on the tray!", ToolTipIcon.None);
        }

        private void ExceedMain_Shown(object sender, EventArgs e)
        {
            보이기ToolStripMenuItem.Text = "숨기기";
        }

        private void 시작ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (시작ToolStripMenuItem.Text == "시작")
            {
                this.Start();
                시작ToolStripMenuItem.Text = "정지";
            }
            else
            {

                this.Stop();
                시작ToolStripMenuItem.Text = "시작";
            }
        }

    }


    [Serializable()]
    public class Order
    {
        public Stimulus condition;
        public Stimulus reaction;
        public bool blockKey;

        public Order(Stimulus condition, Stimulus reaction, bool isBlockKey)
        {
            this.condition = condition;
            this.reaction = reaction;
            this.blockKey = isBlockKey;
        }

        public void CheackOrder(Stimulus tickStimlus)
        {
            if (condition == tickStimlus)
            {
                reaction.Press();
            }
            else
            {
                reaction.UnPress();
            }
        }

        public void Release()
        {
            reaction.UnPress();
        }
    }



    [Serializable()]
    public struct Stimulus
    {
        public bool XPosIncreased, XPosDecreased;
        public bool YPosIncreased, YPosDecreased;
        public bool WheelIncreased, WheelDecreased;
        public bool LBDown, RBDown, MBDown;
        const int MouseSensivity = 1;

        public VirtualKeyCode PressingKey;

        /// <summary>
        /// 모든 변수를 초기 상태로 되돌립니다.
        /// </summary>
        public void Init()
        {
            XPosIncreased = XPosDecreased =
            YPosIncreased = YPosDecreased =
            WheelIncreased = WheelDecreased =
            LBDown = RBDown = MBDown = false;

            PressingKey = unchecked((VirtualKeyCode)(-1));
        }

        bool isPressing;


        /// <summary>
        /// 설정된 Stimulus(읽을 줄 모른다)를 입력합니다.
        /// </summary>
        public void Press()
        {
            {
                if (PressingKey != unchecked((VirtualKeyCode)(-1)))
                {
                    InputSimulator.SimulateKeyDown(PressingKey);
                }
                else if (LBDown)
                {
                    if (!isPressing)
                    {
                        Win32.mouse_event(Win32.LBDOWN, 0, 0, 0, 0);
                    }
                }
                else if (RBDown)
                {
                    if (!isPressing)
                    {
                        Win32.mouse_event(Win32.RBDOWN, 0, 0, 0, 0);
                    }
                }
                else if (MBDown)
                {
                    if (!isPressing)
                    {
                        Win32.mouse_event(Win32.MBDOWN, 0, 0, 0, 0);
                    }
                }
                else
                {
                    var mPos = Win32.GetCursorPosition();

                    if (XPosIncreased)
                    {
                        MoveMouse(mPos.X + MouseSensivity, mPos.Y);
                    }
                    else if (XPosDecreased)
                    {
                        MoveMouse(mPos.X - MouseSensivity, mPos.Y);
                    }
                    else if (YPosIncreased)
                    {
                        MoveMouse(mPos.X, mPos.Y + MouseSensivity);
                    }
                    else if (YPosDecreased)
                    {
                        MoveMouse(mPos.X, mPos.Y - MouseSensivity);
                    }
                    else if (WheelIncreased)
                    {
                        Win32.mouse_event(Win32.WHEEL, 0, 0, 120, 0);
                    }
                    else if (WheelDecreased)
                    {
                        Win32.mouse_event(Win32.WHEEL, 0, 0, unchecked((uint)(-120)), 0);
                    }

                }
                isPressing = true;
            }
        }


        void MoveMouse(int x, int y)
        {
            Win32.POINT p = new Win32.POINT();
            p.x = Convert.ToInt16(x);
            p.y = Convert.ToInt16(y);

            Win32.SetCursorPos(p.x, p.y);
        }

        public void UnPress()
        {
            if (isPressing)
            {
                InputSimulator.SimulateKeyUp(PressingKey);
                if (LBDown)
                {
                    Win32.mouse_event(Win32.LBUP, 0, 0, 0, 0);
                }
                else if (RBDown)
                {
                    Win32.mouse_event(Win32.RBUP, 0, 0, 0, 0);
                }
                else if (MBDown)
                {
                    Win32.mouse_event(Win32.MBUP, 0, 0, 0, 0);
                }
                isPressing = false;
            }
        }

        public static bool IsNullOrEmpty(Stimulus obj)
        {
            if (obj == null)
            {
                return true;
            }

            if ((obj.XPosIncreased == false)
            && (obj.XPosDecreased == false)
            && (obj.YPosIncreased == false)
            && (obj.YPosDecreased == false)
            && (obj.WheelIncreased == false)
            && (obj.WheelDecreased == false)
            && !obj.LBDown && !obj.RBDown && !obj.MBDown
            && (obj.PressingKey == unchecked((VirtualKeyCode)(-1)))) return true;

            return false;
        }

        #region API Methods

        //const int WM_KEYDOWN = 0x100;
        //const int WM_KEYUP = 0x101;
        //const int WM_CHAR = 0x105;
        //const int WM_SYSKEYDOWN = 0x104;
        //const int WM_SYSKEYUP = 0x105;

        //[DllImport("user32.dll", CharSet = CharSet.Auto)]
        //static extern int PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);


        //[DllImport("user32", SetLastError = true)]
        //public static extern IntPtr GetForegroundWindow();

        #endregion

        #region Opeartor

        static bool IsEqual(Stimulus a, Stimulus b)
        {
            if ((a.XPosIncreased && b.XPosIncreased)
            || (a.XPosDecreased && b.XPosDecreased)
            || (a.YPosIncreased && b.YPosIncreased)
            || (a.YPosDecreased && b.YPosDecreased)
            || (a.WheelIncreased && b.WheelIncreased)
            || (a.WheelDecreased && b.WheelDecreased)
            || (a.LBDown && b.LBDown)
            || (a.RBDown && b.RBDown)
            || (a.MBDown && b.MBDown)
            || (a.PressingKey == b.PressingKey && a.PressingKey != unchecked((VirtualKeyCode)(-1)) && b.PressingKey != unchecked((VirtualKeyCode)(-1)))) return true;
            return false;
        }

        public static bool operator ==(Stimulus a, Stimulus b)
        {
            return IsEqual(a, b);
        }


        public static bool operator !=(Stimulus a, Stimulus b)
        {
            return !IsEqual(a, b);
        }


        public override string ToString()
        {
            var str = "";

            if (this.XPosIncreased) str += "X Increase ";
            if (this.XPosDecreased) str += "X Decrease ";
            if (this.YPosIncreased) str += "Y Increase ";
            if (this.YPosDecreased) str += "Y Decrease ";
            if (this.WheelIncreased) str += "W Increase ";
            if (this.WheelDecreased) str += "W Decrease ";
            if (this.LBDown) str += "Left Click ";
            if (this.RBDown) str += "Right Click ";
            if (this.MBDown) str += "Middle Click ";
            if (this.PressingKey != unchecked((VirtualKeyCode)(-1))) str += PressingKey.ToString();

            return str;
        }

        #endregion
    }

    public class Win32
    {
        [DllImport("User32.Dll")]
        public static extern long SetCursorPos(int x, int y);

        [DllImport("User32.Dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref POINT point);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        public static Point GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            //bool success = User32.GetCursorPos(out lpPoint);
            // if (!success)

            return new Point(lpPoint.x, lpPoint.y);
        }

        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        public const uint LBDOWN = 0x00000002; // 왼쪽 마우스 버튼 눌림
        public const uint LBUP = 0x00000004; // 왼쪽 마우스 버튼 떼어짐
        public const uint RBDOWN = 0x00000008; // 오른쪽 마우스 버튼 눌림
        public const uint RBUP = 0x000000010; // 오른쪽 마우스 버튼 떼어짐
        public const uint MBDOWN = 0x00000020; // 휠 버튼 눌림
        public const uint MBUP = 0x000000040; // 휠 버튼 떼어짐
        public const uint WHEEL = 0x00000800; //휠 스크롤
    }
}
