﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace Exceed
{
    public partial class KeySelect : Form
    {
        public Stimulus myStimul;
        string[] mouseStimulsText = {
                                              "Left Click",
                                              "Right Click",
                                              "Middle Click",
                                              "X Increase",
                                              "X Decrease",
                                              "Y Increase",
                                              "Y Decrease",
                                              "W Increase",
                                              "W Decrease"
                                          };
        WindowsInput.VirtualKeyCode[] CodeEnums = (WindowsInput.VirtualKeyCode[])Enum.GetValues(typeof(WindowsInput.VirtualKeyCode));
        public KeySelect()
        {
            InitializeComponent();
            foreach (var item in CodeEnums)
            {
                this.comboBox1.Items.Add(item);
            }
            foreach (var item in mouseStimulsText)
            {
                this.comboBox3.Items.Add(item);
            }
            myStimul = new Stimulus();
            myStimul.Init();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                if (string.IsNullOrWhiteSpace(comboBox1.Text))
                {
                    MessageBox.Show("Cant (._. )");
                    return;
                }
                myStimul.PressingKey = (WindowsInput.VirtualKeyCode)Enum.Parse(typeof(WindowsInput.VirtualKeyCode), comboBox1.Text);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(comboBox3.Text))
                {
                    MessageBox.Show("Cant (._. )");
                    return;
                }
                switch (comboBox3.Text)
                {
                    case "X Increase":
                        myStimul.XPosIncreased = true;
                        break;
                    case "X Decrease":
                        myStimul.XPosDecreased = true;
                        break;
                    case "Y Increase":
                        myStimul.YPosIncreased = true;
                        break;
                    case "Y Decrease":
                        myStimul.YPosDecreased = true;
                        break;
                    case "W Increase":
                        myStimul.WheelIncreased = true;
                        break;
                    case "W Decrease":
                        myStimul.WheelDecreased = true;
                        break;
                    case "Left Click":
                        myStimul.LBDown = true;
                        break;
                    case "Right Click":
                        myStimul.RBDown = true;
                        break;
                    case "Middle Click":
                        myStimul.MBDown = true;
                        break;
                    default:
                        MessageBox.Show("Cant (._. )");
                        return;
                }
                if (string.IsNullOrWhiteSpace(textBox1.Text))
                {
                    MessageBox.Show("Cant (._. )");
                    return;
                }
                myStimul.MouseSensivity = int.Parse(textBox1.Text);
            }
            
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            myStimul.Init();
            this.Close();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if(radioButton1.Checked)
            {
                comboBox1.Enabled = true;
                comboBox3.Enabled = false;
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                comboBox1.Enabled = false;
                comboBox3.Enabled = true;
            }
        }

        private void textBox1_KeyPress_1(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsDigit(e.KeyChar)) && e.KeyChar != (int)Keys.Back)
            {
                e.Handled = true;
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox3.Text)
            {
                case "X Increase":
                case "X Decrease":
                case "Y Increase":
                case "Y Decrease":
                case "W Increase":
                case "W Decrease":
                    textBox1.Enabled = true;
                    break;
                case "Left Click":
                case "Right Click":
                case "Middle Click":
                    textBox1.Enabled = false;
                    break;
            }
        }

        private void comboBox1_KeyDown(object sender, KeyEventArgs e)
        {
            this.SuspendLayout();
            comboBox1.DropDownStyle = ComboBoxStyle.DropDown;
            comboBox1.Text = ((WindowsInput.VirtualKeyCode)(e.KeyData)).ToString();
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            this.ResumeLayout();
#if DEBUG
            Console.WriteLine(string.Format("{0} = {1}, {2}", (int)(e.KeyData), (WindowsInput.VirtualKeyCode)(e.KeyData), comboBox1.Text));
#endif
            myStimul.PressingKey = (WindowsInput.VirtualKeyCode)Enum.Parse(typeof(WindowsInput.VirtualKeyCode), comboBox1.Text);
        }


    }
}
