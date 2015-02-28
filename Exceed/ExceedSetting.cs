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
    public partial class ExceedSetting : Form
    {
        public List<Order> myOrders = new List<Order>();
        List<Order> backupOrders;
        public bool isAlwaysOnTop, isExitToHide;

        public ExceedSetting(List<Order> orders)
        {
            InitializeComponent();
            myOrders = orders.ToList();
            backupOrders = orders;
            ResizeColumns();
            foreach (var item in myOrders)
            {
                AddItemtoList(item);
            }
        }

        void AddItemtoList(Order order)
        {
            var item = new ListViewItem(order.condition.ToString());
            item.SubItems.Add(order.reaction.ToString());

            this.listView1.Items.Add(item);
        }

        void ResizeColumns()
        {
            listView1.Columns[0].Width = listView1.Width / 2 - 2;
            listView1.Columns[1].Width = listView1.Width / 2 - 2;
        }

        private void listView1_Resize(object sender, EventArgs e)
        {
            ResizeColumns();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            var frm = new NewOrder();
            frm.ShowDialog();
            if(frm.isOrderGenerated)
            {
                var newOrder = frm.returnOrder;
                AddItemtoList(newOrder);
                myOrders.Add(newOrder);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            isAlwaysOnTop = checkBox1.Checked;
            isExitToHide = checkBox2.Checked;
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            myOrders = backupOrders;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                myOrders.RemoveAt(item.Index);
                item.Remove();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var frm = new KeySelect();
            frm.ShowDialog();
        }

        private void ExceedSetting_Shown(object sender, EventArgs e)
        {
            this.checkBox1.Checked = this.isAlwaysOnTop;
            this.checkBox2.Checked = this.isExitToHide;
        }

    }
}
