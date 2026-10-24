using System;
using System.Drawing;
using System.Windows.Forms;

namespace Win98WinFormsDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            toolTip1.SetToolTip(textName, "Type your name here");
            toolTip1.SetToolTip(buttonSay, "Shows a greeting");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TreeNode root = treeView1.Nodes.Add("Root");
            TreeNode child1 = root.Nodes.Add("Child 1");
            child1.Nodes.Add("Leaf A");
            child1.Nodes.Add("Leaf B");
            root.Nodes.Add("Child 2");
            root.ExpandAll();

            AddListRow("Alpha", "1");
            AddListRow("Bravo", "2");
            AddListRow("Charlie", "3");

            dataGridView1.Rows.Add("Keyboard", "2");
            dataGridView1.Rows.Add("Mouse", "1");
            dataGridView1.Rows.Add("Monitor", "3");

            progressBar1.Value = trackBar1.Value;

            Bitmap bmp = new Bitmap(pictureBox1.Width - 2, pictureBox1.Height - 2);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    int c = (y * 255) / Math.Max(1, bmp.Height - 1);
                    using (Pen p = new Pen(Color.FromArgb(255 - c, 80, c)))
                        g.DrawLine(p, 0, y, bmp.Width, y);
                }
                g.DrawString("GDI+", new Font("Arial", 12, FontStyle.Bold), Brushes.White, 4, 4);
            }
            pictureBox1.Image = bmp;

            Text = "Win98 WinForms Demo - " + Environment.OSVersion.VersionString;
            statusLabel.Text = "Loaded " + Environment.OSVersion.VersionString;
        }

        private void AddListRow(string name, string value)
        {
            ListViewItem item = new ListViewItem(name);
            item.SubItems.Add(value);
            listView1.Items.Add(item);
        }

        private string BuildGreeting()
        {
            string name = textName.Text;
            if (string.IsNullOrEmpty(name))
                name = "World";
            string greeting = "Hello, " + name + "!";
            if (checkUpper.Checked)
                greeting = greeting.ToUpper();
            return greeting;
        }

        private void buttonSay_Click(object sender, EventArgs e)
        {
            string greeting = BuildGreeting();
            statusLabel.Text = greeting;
            MessageBox.Show(this, greeting, "Greeting", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void tsbClear_Click(object sender, EventArgs e)
        {
            textName.Text = "";
            listBox1.Items.Clear();
            checkedListBox1.Items.Clear();
            listView1.Items.Clear();
            dataGridView1.Rows.Clear();
            statusLabel.Text = "Cleared";
        }

        private void checkUpper_CheckedChanged(object sender, EventArgs e)
        {
            statusLabel.Text = BuildGreeting();
        }

        private void comboColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboColor.SelectedIndex)
            {
                case 1: BackColor = Color.LightYellow; break;
                case 2: BackColor = Color.LightBlue; break;
                case 3: BackColor = Color.LightGreen; break;
                default: BackColor = SystemColors.Control; break;
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            statusLabel.Text = "Tree: " + e.Node.Text;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
                statusLabel.Text = "List: " + listView1.SelectedItems[0].Text;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            progressBar1.Value = trackBar1.Value;
            statusLabel.Text = "Value: " + trackBar1.Value;
        }

        private void clearListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            statusLabel.Text = "List cleared";
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this,
                "Mono WinForms demo running on " + Environment.OSVersion.VersionString,
                "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (statusLabel.Text.StartsWith("Time:") || statusLabel.Text == "Ready" || statusLabel.Text == "Loaded " + Environment.OSVersion.VersionString)
                statusLabel.Text = "Time: " + DateTime.Now.ToString("HH:mm:ss");
        }
    }
}
