using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Exceed
{
    public partial class NewOrder : Form
    {
        public bool isOrderGenerated = false;
        public Order returnOrder;
        public Stimulus myCondition, myReaction;

        public NewOrder()
        {
            InitializeComponent();
            myCondition.Init();
            myReaction.Init();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var frm = new KeySelect();
            frm.ShowDialog();
            if (!Stimulus.IsNullOrEmpty(frm.myStimul))
            {
                myCondition = frm.myStimul;
                this.button1.Text = myCondition.ToString();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var frm = new KeySelect();
            frm.ShowDialog();
            if (!Stimulus.IsNullOrEmpty(frm.myStimul))
            {
                myReaction = frm.myStimul;
                this.button2.Text = myReaction.ToString();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!Stimulus.IsNullOrEmpty(myReaction) && !Stimulus.IsNullOrEmpty(myCondition))
            {
                this.isOrderGenerated = true;
                returnOrder = new Order(myCondition, myReaction, checkBox1.Checked);
                this.Close();
            }
            else
            {
                MessageBox.Show("Nope. (._. )");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
