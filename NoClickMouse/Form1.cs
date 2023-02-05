using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;

namespace NoClickMouse
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ghk = new KeyHandler(Keys.Pause, this, 0);
            ghk.Register();
            ghk2 = new KeyHandler(Keys.PrintScreen, this, 1);
            ghk2.Register();
            ghk3 = new KeyHandler(Keys.Insert, this, 2);
            ghk3.Register();
            ghk4 = new KeyHandler(Keys.Escape, this, 3);
            ghk4.Register();
            timer1.Interval = 100;
            //timer2.Interval = 1000;
            button1.BackColor = Color.OrangeRed;
            button1.Text = "app OFF";
            labelActType.Text = "app OFF";
            textBox1.Text = "1000";
        }


        //This is a replacement for Cursor.Position in WinForms
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;


        Point LastPosition;
        bool notClicked = true;//not already clicked//prevent clicks after the first click
        bool activeApp = false;
        private KeyHandler ghk;
        private KeyHandler ghk2;
        private KeyHandler ghk3;
        private KeyHandler ghk4;
        string[] clickType = new string[] { "Left", "Double", "Right", "Select" };//which function is active
        public static int activeType = 0;
        public static bool selection = false;

        //This simulates a left mouse click
        public static void LeftMouseClick(int xpos, int ypos)
        {
            SetCursorPos(xpos, ypos);
            mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
        }

        public static void RightMouseClick(int xpos, int ypos)
        {
            SetCursorPos(xpos, ypos);
            mouse_event(MOUSEEVENTF_RIGHTDOWN, xpos, ypos, 0, 0);
            mouse_event(MOUSEEVENTF_RIGHTUP, xpos, ypos, 0, 0);
        }

        public static void LeftMouseDown(int xpos, int ypos)
        {
            SetCursorPos(xpos, ypos);
            mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
        }

        public static void LeftMouseUp(int xpos, int ypos)
        {
            SetCursorPos(xpos, ypos);
            mouse_event(MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            HandleOnOff();
        }

        private bool positionEquals(Point x, Point y)
        {
            return x.Equals(y);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            labelActType.Text = clickType[activeType];

            if (!positionEquals(LastPosition, Cursor.Position))//if mouse is moving
            {
                notClicked = true;//not already clicked
                timer2.Stop();//new

            }

            if (positionEquals(LastPosition, Cursor.Position) && notClicked)//click countdown timer start if mouse is still and hasnt alread issued a click on that position
            {
                timer2.Start();
            }

            LastPosition = Cursor.Position;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (clickType[activeType] == "Left")
            {
                LeftMouseClick(Cursor.Position.X, Cursor.Position.Y);

            }
            else if (clickType[activeType] == "Double")// double click
            {
                LeftMouseClick(Cursor.Position.X, Cursor.Position.Y);
                Thread.Sleep(50);
                LeftMouseClick(Cursor.Position.X, Cursor.Position.Y);
            }
            else if (clickType[activeType] == "Right")
            {
                RightMouseClick(Cursor.Position.X, Cursor.Position.Y);
            }

            notClicked = false;
            timer2.Stop();
        }

        private void HandleLeftMouse()//switch between left click and double click
        {
            activeType++;
            if (activeType > 1)
            {
                activeType = 0;
            }
        }

        private void HandleRightMouse()//switch to rigth click
        {
            activeType = 2;
        }

        private void HandleSelection()//select text
        {
            if (selection == false)
            {
                activeType = 3;//selection//to disable other click functionality
                LeftMouseDown(Cursor.Position.X, Cursor.Position.Y);
                selection = true;
            }
            else
            {
                LeftMouseUp(Cursor.Position.X, Cursor.Position.Y);
                activeType = 0;//return fuctionality to left click
                selection = false;
            }
        }
        private void HandleOnOff()//switch to rigth click
        {
            if (!activeApp)
            {
                timer1.Start();
                activeApp = true;
                button1.Text = "app ON";
                button1.BackColor = Color.Green;
                timer2.Interval = int.Parse(textBox1.Text);
            }
            else
            {
                timer1.Stop();
                activeApp = false;
                button1.Text = "app OFF";
                button1.BackColor = Color.OrangeRed;
                labelActType.Text = "app OFF";
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312)
            {    // Trap WM_HOTKEY

                int id = m.WParam.ToInt32();

                if (id != 2)
                {
                    selection = false;
                }

                if (id == 0)//pause
                {
                    HandleRightMouse();//rightclick
                }
                else if (id == 1)//printscr
                {
                    HandleLeftMouse();//leftclick//doubleclick
                }
                else if (id == 2)//insert
                {
                    HandleSelection();//select text or scroll
                }
                else if (id == 3)//Escape// on/off app
                {
                    HandleOnOff();
                }
                //MessageBox.Show(string.Format("Hotkey #{0} pressed", id));
            }
            base.WndProc(ref m);
        }
    }

    public static class Constants
    {
        //windows message id for hotkey
        public const int WM_HOTKEY_MSG_ID = 0x0312;// for registering hotkeys
    }
    public class KeyHandler
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private int key;
        private IntPtr hWnd;
        private int id;

        public KeyHandler(Keys key, Form form, int _id)//added int _id
        {
            this.key = (int)key;
            this.hWnd = form.Handle;
            id = _id;//added//to register keys via numbers
        }

        public override int GetHashCode()
        {
            return key ^ hWnd.ToInt32();
        }

        public bool Register()
        {
            return RegisterHotKey(hWnd, id, 0, key);
        }

        public bool Unregiser()
        {
            return UnregisterHotKey(hWnd, id);
        }
    }
}

