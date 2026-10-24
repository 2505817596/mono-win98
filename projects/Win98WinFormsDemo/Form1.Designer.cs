namespace Win98WinFormsDemo
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsbHello = new System.Windows.Forms.ToolStripButton();
            this.tsbClear = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.gbInput = new System.Windows.Forms.GroupBox();
            this.labelName = new System.Windows.Forms.Label();
            this.textName = new System.Windows.Forms.TextBox();
            this.buttonSay = new System.Windows.Forms.Button();
            this.checkUpper = new System.Windows.Forms.CheckBox();
            this.radioA = new System.Windows.Forms.RadioButton();
            this.radioB = new System.Windows.Forms.RadioButton();
            this.comboColor = new System.Windows.Forms.ComboBox();
            this.gbLists = new System.Windows.Forms.GroupBox();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabTree = new System.Windows.Forms.TabPage();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.listView1 = new System.Windows.Forms.ListView();
            this.colName = new System.Windows.Forms.ColumnHeader();
            this.colValue = new System.Windows.Forms.ColumnHeader();
            this.tabInputs = new System.Windows.Forms.TabPage();
            this.labelNum = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.labelDate = new System.Windows.Forms.Label();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.colItem = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colQty = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.clearListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.gbInput.SuspendLayout();
            this.gbLists.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabTree.SuspendLayout();
            this.tabInputs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            //
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.fileToolStripMenuItem, this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(784, 24);
            this.menuStrip1.TabIndex = 0;
            //
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Text = "&File";
            //
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            //
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Text = "&Help";
            //
            this.aboutToolStripMenuItem.Text = "&About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            //
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.tsbHello, this.tsbClear});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(784, 25);
            this.toolStrip1.TabIndex = 1;
            //
            this.tsbHello.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbHello.Text = "Say Hello";
            this.tsbHello.Click += new System.EventHandler(this.buttonSay_Click);
            //
            this.tsbClear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbClear.Text = "Clear";
            this.tsbClear.Click += new System.EventHandler(this.tsbClear_Click);
            //
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.statusLabel });
            this.statusStrip1.Location = new System.Drawing.Point(0, 539);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(784, 22);
            this.statusStrip1.TabIndex = 2;
            //
            this.statusLabel.Text = "Ready";
            //
            this.gbInput.Controls.Add(this.labelName);
            this.gbInput.Controls.Add(this.textName);
            this.gbInput.Controls.Add(this.buttonSay);
            this.gbInput.Controls.Add(this.checkUpper);
            this.gbInput.Controls.Add(this.radioA);
            this.gbInput.Controls.Add(this.radioB);
            this.gbInput.Controls.Add(this.comboColor);
            this.gbInput.Location = new System.Drawing.Point(12, 55);
            this.gbInput.Name = "gbInput";
            this.gbInput.Size = new System.Drawing.Size(360, 150);
            this.gbInput.TabIndex = 3;
            this.gbInput.TabStop = false;
            this.gbInput.Text = "Input controls";
            //
            this.labelName.Location = new System.Drawing.Point(12, 25);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(45, 20);
            this.labelName.Text = "Name:";
            //
            this.textName.Location = new System.Drawing.Point(60, 22);
            this.textName.Name = "textName";
            this.textName.Size = new System.Drawing.Size(220, 20);
            this.textName.TabIndex = 0;
            this.textName.Text = "World";
            //
            this.buttonSay.Location = new System.Drawing.Point(60, 52);
            this.buttonSay.Name = "buttonSay";
            this.buttonSay.Size = new System.Drawing.Size(100, 26);
            this.buttonSay.TabIndex = 1;
            this.buttonSay.Text = "Say Hello";
            this.buttonSay.UseVisualStyleBackColor = true;
            this.buttonSay.Click += new System.EventHandler(this.buttonSay_Click);
            //
            this.checkUpper.Location = new System.Drawing.Point(180, 55);
            this.checkUpper.Name = "checkUpper";
            this.checkUpper.Size = new System.Drawing.Size(120, 20);
            this.checkUpper.TabIndex = 2;
            this.checkUpper.Text = "UPPERCASE";
            //
            this.radioA.Location = new System.Drawing.Point(60, 85);
            this.radioA.Name = "radioA";
            this.radioA.Size = new System.Drawing.Size(100, 20);
            this.radioA.TabIndex = 3;
            this.radioA.Text = "Option A";
            //
            this.radioB.Checked = true;
            this.radioB.Location = new System.Drawing.Point(170, 85);
            this.radioB.Name = "radioB";
            this.radioB.Size = new System.Drawing.Size(100, 20);
            this.radioB.TabIndex = 4;
            this.radioB.TabStop = true;
            this.radioB.Text = "Option B";
            //
            this.comboColor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboColor.Items.AddRange(new object[] { "Default", "Light Yellow", "Light Blue", "Light Green" });
            this.comboColor.Location = new System.Drawing.Point(60, 115);
            this.comboColor.Name = "comboColor";
            this.comboColor.Size = new System.Drawing.Size(220, 21);
            this.comboColor.TabIndex = 5;
            this.comboColor.SelectedIndexChanged += new System.EventHandler(this.comboColor_SelectedIndexChanged);
            //
            this.gbLists.Controls.Add(this.listBox1);
            this.gbLists.Controls.Add(this.checkedListBox1);
            this.gbLists.Location = new System.Drawing.Point(12, 211);
            this.gbLists.Name = "gbLists";
            this.gbLists.Size = new System.Drawing.Size(360, 175);
            this.gbLists.TabIndex = 4;
            this.gbLists.TabStop = false;
            this.gbLists.Text = "Lists";
            //
            this.listBox1.ContextMenuStrip = this.contextMenuStrip1;
            this.listBox1.Items.AddRange(new object[] { "Alpha", "Bravo", "Charlie", "Delta", "Echo" });
            this.listBox1.Location = new System.Drawing.Point(12, 22);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(160, 134);
            this.listBox1.TabIndex = 0;
            //
            this.checkedListBox1.Items.AddRange(new object[] { "Apples", "Oranges", "Bananas", "Grapes" });
            this.checkedListBox1.Location = new System.Drawing.Point(185, 22);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.Size = new System.Drawing.Size(160, 134);
            this.checkedListBox1.TabIndex = 1;
            //
            this.tabControl1.Controls.Add(this.tabTree);
            this.tabControl1.Controls.Add(this.tabInputs);
            this.tabControl1.Location = new System.Drawing.Point(384, 55);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(388, 235);
            this.tabControl1.TabIndex = 5;
            //
            this.tabTree.Controls.Add(this.treeView1);
            this.tabTree.Controls.Add(this.listView1);
            this.tabTree.Location = new System.Drawing.Point(4, 22);
            this.tabTree.Name = "tabTree";
            this.tabTree.Padding = new System.Windows.Forms.Padding(3);
            this.tabTree.Size = new System.Drawing.Size(380, 209);
            this.tabTree.TabIndex = 0;
            this.tabTree.Text = "Tree / List";
            this.tabTree.UseVisualStyleBackColor = true;
            //
            this.treeView1.Location = new System.Drawing.Point(8, 8);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(170, 193);
            this.treeView1.TabIndex = 0;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            //
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { this.colName, this.colValue });
            this.listView1.FullRowSelect = true;
            this.listView1.Location = new System.Drawing.Point(185, 8);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(185, 193);
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            //
            this.colName.Text = "Name";
            this.colName.Width = 90;
            //
            this.colValue.Text = "Value";
            this.colValue.Width = 80;
            //
            this.tabInputs.Controls.Add(this.labelNum);
            this.tabInputs.Controls.Add(this.numericUpDown1);
            this.tabInputs.Controls.Add(this.labelDate);
            this.tabInputs.Controls.Add(this.dateTimePicker1);
            this.tabInputs.Controls.Add(this.progressBar1);
            this.tabInputs.Controls.Add(this.trackBar1);
            this.tabInputs.Controls.Add(this.pictureBox1);
            this.tabInputs.Location = new System.Drawing.Point(4, 22);
            this.tabInputs.Name = "tabInputs";
            this.tabInputs.Padding = new System.Windows.Forms.Padding(3);
            this.tabInputs.Size = new System.Drawing.Size(380, 209);
            this.tabInputs.TabIndex = 1;
            this.tabInputs.Text = "More controls";
            this.tabInputs.UseVisualStyleBackColor = true;
            //
            this.labelNum.Location = new System.Drawing.Point(10, 15);
            this.labelNum.Name = "labelNum";
            this.labelNum.Size = new System.Drawing.Size(80, 20);
            this.labelNum.Text = "Number:";
            //
            this.numericUpDown1.Location = new System.Drawing.Point(95, 13);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(80, 20);
            this.numericUpDown1.TabIndex = 0;
            this.numericUpDown1.Value = new decimal(new int[] { 25, 0, 0, 0 });
            //
            this.labelDate.Location = new System.Drawing.Point(190, 15);
            this.labelDate.Name = "labelDate";
            this.labelDate.Size = new System.Drawing.Size(40, 20);
            this.labelDate.Text = "Date:";
            //
            this.dateTimePicker1.Location = new System.Drawing.Point(230, 13);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(140, 20);
            this.dateTimePicker1.TabIndex = 1;
            //
            this.progressBar1.Location = new System.Drawing.Point(10, 45);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(360, 22);
            this.progressBar1.TabIndex = 2;
            //
            this.trackBar1.Location = new System.Drawing.Point(10, 73);
            this.trackBar1.Maximum = 100;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(360, 45);
            this.trackBar1.TabIndex = 3;
            this.trackBar1.TickFrequency = 10;
            this.trackBar1.Value = 25;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            //
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(10, 125);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(160, 70);
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            //
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { this.colItem, this.colQty });
            this.dataGridView1.Location = new System.Drawing.Point(12, 395);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(760, 130);
            this.dataGridView1.TabIndex = 6;
            //
            this.colItem.HeaderText = "Item";
            this.colItem.Name = "colItem";
            //
            this.colQty.HeaderText = "Quantity";
            this.colQty.Name = "colQty";
            //
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.clearListToolStripMenuItem });
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            //
            this.clearListToolStripMenuItem.Text = "Clear list";
            this.clearListToolStripMenuItem.Click += new System.EventHandler(this.clearListToolStripMenuItem_Click);
            //
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            //
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.gbLists);
            this.Controls.Add(this.gbInput);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Win98 WinForms Demo";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.gbInput.ResumeLayout(false);
            this.gbInput.PerformLayout();
            this.gbLists.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabTree.ResumeLayout(false);
            this.tabInputs.ResumeLayout(false);
            this.tabInputs.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbHello;
        private System.Windows.Forms.ToolStripButton tsbClear;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.GroupBox gbInput;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.TextBox textName;
        private System.Windows.Forms.Button buttonSay;
        private System.Windows.Forms.CheckBox checkUpper;
        private System.Windows.Forms.RadioButton radioA;
        private System.Windows.Forms.RadioButton radioB;
        private System.Windows.Forms.ComboBox comboColor;
        private System.Windows.Forms.GroupBox gbLists;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabTree;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colValue;
        private System.Windows.Forms.TabPage tabInputs;
        private System.Windows.Forms.Label labelNum;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label labelDate;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn colItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn colQty;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem clearListToolStripMenuItem;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Timer timer1;
    }
}
