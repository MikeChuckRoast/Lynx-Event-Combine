namespace Lynx_Event_Combine
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            databasePathText = new TextBox();
            label1 = new Label();
            chooseDirButton = new Button();
            eventListBox = new ListBox();
            label2 = new Label();
            label3 = new Label();
            mainEventComboBox = new ComboBox();
            combineButton = new Button();
            splitLifButton = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            reloadButton = new Button();
            tableLayoutPanel3 = new TableLayoutPanel();
            removeGenderCheckBox = new CheckBox();
            tableLayoutPanel4 = new TableLayoutPanel();
            run3200mButton = new Button();
            run1600mButton = new Button();
            run800mButton = new Button();
            relay800mButton = new Button();
            relay400mButton = new Button();
            relay200mButton = new Button();
            relay100mButton = new Button();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            tableLayoutPanel4.SuspendLayout();
            SuspendLayout();
            // 
            // databasePathText
            // 
            databasePathText.Dock = DockStyle.Top;
            databasePathText.Location = new Point(3, 3);
            databasePathText.Name = "databasePathText";
            databasePathText.Size = new Size(351, 23);
            databasePathText.TabIndex = 0;
            databasePathText.TextChanged += databasePathText_TextChanged;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new Point(3, 3);
            label1.Name = "label1";
            label1.Size = new Size(109, 15);
            label1.TabIndex = 1;
            label1.Text = "Lynx event file path";
            // 
            // chooseDirButton
            // 
            chooseDirButton.Location = new Point(360, 3);
            chooseDirButton.Name = "chooseDirButton";
            chooseDirButton.Size = new Size(75, 23);
            chooseDirButton.TabIndex = 2;
            chooseDirButton.Text = "Choose...";
            chooseDirButton.UseVisualStyleBackColor = true;
            chooseDirButton.Click += chooseDirButton_Click;
            // 
            // eventListBox
            // 
            eventListBox.Dock = DockStyle.Fill;
            eventListBox.FormattingEnabled = true;
            eventListBox.ItemHeight = 15;
            eventListBox.Location = new Point(3, 226);
            eventListBox.Name = "eventListBox";
            eventListBox.SelectionMode = SelectionMode.MultiSimple;
            eventListBox.Size = new Size(519, 178);
            eventListBox.TabIndex = 3;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Location = new Point(3, 205);
            label2.Name = "label2";
            label2.Size = new Size(121, 15);
            label2.TabIndex = 4;
            label2.Text = "Choose events to add";
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Left;
            label3.AutoSize = true;
            label3.Location = new Point(3, 104);
            label3.Name = "label3";
            label3.Size = new Size(316, 15);
            label3.TabIndex = 6;
            label3.Text = "Main event (other event entries will be added to this event)";
            // 
            // mainEventComboBox
            // 
            mainEventComboBox.Dock = DockStyle.Fill;
            mainEventComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            mainEventComboBox.FormattingEnabled = true;
            mainEventComboBox.Location = new Point(3, 125);
            mainEventComboBox.Name = "mainEventComboBox";
            mainEventComboBox.Size = new Size(519, 23);
            mainEventComboBox.TabIndex = 7;
            // 
            // combineButton
            // 
            combineButton.Anchor = AnchorStyles.None;
            combineButton.Location = new Point(92, 5);
            combineButton.Name = "combineButton";
            combineButton.Size = new Size(75, 23);
            combineButton.TabIndex = 8;
            combineButton.Text = "Combine";
            combineButton.UseVisualStyleBackColor = true;
            combineButton.Click += combineButton_Click;
            // 
            // splitLifButton
            // 
            splitLifButton.Anchor = AnchorStyles.None;
            splitLifButton.Location = new Point(351, 5);
            splitLifButton.Name = "splitLifButton";
            splitLifButton.Size = new Size(75, 23);
            splitLifButton.TabIndex = 9;
            splitLifButton.Text = "Split LIF file";
            splitLifButton.UseVisualStyleBackColor = true;
            splitLifButton.Click += splitLifButton_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 1);
            tableLayoutPanel1.Controls.Add(label2, 0, 6);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel3, 0, 7);
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(eventListBox, 0, 7);
            tableLayoutPanel1.Controls.Add(removeGenderCheckBox, 0, 5);
            tableLayoutPanel1.Controls.Add(mainEventComboBox, 0, 4);
            tableLayoutPanel1.Controls.Add(label3, 0, 3);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel4, 0, 2);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 9;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 21F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 21F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 21F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel1.Size = new Size(525, 447);
            tableLayoutPanel1.TabIndex = 10;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 3;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel2.Controls.Add(databasePathText, 0, 0);
            tableLayoutPanel2.Controls.Add(chooseDirButton, 1, 0);
            tableLayoutPanel2.Controls.Add(reloadButton, 2, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(3, 24);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle());
            tableLayoutPanel2.Size = new Size(519, 34);
            tableLayoutPanel2.TabIndex = 11;
            // 
            // reloadButton
            // 
            reloadButton.Location = new Point(441, 3);
            reloadButton.Name = "reloadButton";
            reloadButton.Size = new Size(75, 23);
            reloadButton.TabIndex = 3;
            reloadButton.Text = "Reload";
            reloadButton.UseVisualStyleBackColor = true;
            reloadButton.Click += reloadButton_Click;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 2;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.Controls.Add(splitLifButton, 1, 0);
            tableLayoutPanel3.Controls.Add(combineButton, 0, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(3, 410);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel3.Size = new Size(519, 34);
            tableLayoutPanel3.TabIndex = 11;
            // 
            // removeGenderCheckBox
            // 
            removeGenderCheckBox.Anchor = AnchorStyles.Left;
            removeGenderCheckBox.AutoSize = true;
            removeGenderCheckBox.Checked = true;
            removeGenderCheckBox.CheckState = CheckState.Checked;
            removeGenderCheckBox.Location = new Point(3, 172);
            removeGenderCheckBox.Name = "removeGenderCheckBox";
            removeGenderCheckBox.Size = new Size(216, 19);
            removeGenderCheckBox.TabIndex = 11;
            removeGenderCheckBox.Text = "Remove gendered name from event";
            removeGenderCheckBox.UseVisualStyleBackColor = true;
            removeGenderCheckBox.CheckedChanged += removeGenderCheckBox_CheckedChanged;
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.ColumnCount = 7;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.2857141F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.2857141F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.2857141F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.2857141F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.2857141F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.2857141F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.2857141F));
            tableLayoutPanel4.Controls.Add(run3200mButton, 6, 0);
            tableLayoutPanel4.Controls.Add(run1600mButton, 5, 0);
            tableLayoutPanel4.Controls.Add(run800mButton, 4, 0);
            tableLayoutPanel4.Controls.Add(relay800mButton, 3, 0);
            tableLayoutPanel4.Controls.Add(relay400mButton, 2, 0);
            tableLayoutPanel4.Controls.Add(relay200mButton, 1, 0);
            tableLayoutPanel4.Controls.Add(relay100mButton, 0, 0);
            tableLayoutPanel4.Dock = DockStyle.Fill;
            tableLayoutPanel4.Location = new Point(3, 64);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 1;
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.Size = new Size(519, 34);
            tableLayoutPanel4.TabIndex = 12;
            // 
            // run3200mButton
            // 
            run3200mButton.Dock = DockStyle.Fill;
            run3200mButton.FlatStyle = FlatStyle.Flat;
            run3200mButton.Location = new Point(447, 3);
            run3200mButton.Name = "run3200mButton";
            run3200mButton.Size = new Size(69, 28);
            run3200mButton.TabIndex = 6;
            run3200mButton.Text = "3200m";
            run3200mButton.UseVisualStyleBackColor = true;
            run3200mButton.Click += run3200mButton_Click;
            // 
            // run1600mButton
            // 
            run1600mButton.Dock = DockStyle.Fill;
            run1600mButton.FlatStyle = FlatStyle.Flat;
            run1600mButton.Location = new Point(373, 3);
            run1600mButton.Name = "run1600mButton";
            run1600mButton.Size = new Size(68, 28);
            run1600mButton.TabIndex = 5;
            run1600mButton.Text = "1600m";
            run1600mButton.UseVisualStyleBackColor = true;
            run1600mButton.Click += run1600mButton_Click;
            // 
            // run800mButton
            // 
            run800mButton.Dock = DockStyle.Fill;
            run800mButton.FlatStyle = FlatStyle.Flat;
            run800mButton.Location = new Point(299, 3);
            run800mButton.Name = "run800mButton";
            run800mButton.Size = new Size(68, 28);
            run800mButton.TabIndex = 4;
            run800mButton.Text = "800m";
            run800mButton.UseVisualStyleBackColor = true;
            run800mButton.Click += run800mButton_Click;
            // 
            // relay800mButton
            // 
            relay800mButton.Dock = DockStyle.Fill;
            relay800mButton.FlatStyle = FlatStyle.Flat;
            relay800mButton.Location = new Point(225, 3);
            relay800mButton.Name = "relay800mButton";
            relay800mButton.Size = new Size(68, 28);
            relay800mButton.TabIndex = 3;
            relay800mButton.Text = "4x800";
            relay800mButton.UseVisualStyleBackColor = true;
            relay800mButton.Click += relay800mButton_Click;
            // 
            // relay400mButton
            // 
            relay400mButton.Dock = DockStyle.Fill;
            relay400mButton.FlatStyle = FlatStyle.Flat;
            relay400mButton.Location = new Point(151, 3);
            relay400mButton.Name = "relay400mButton";
            relay400mButton.Size = new Size(68, 28);
            relay400mButton.TabIndex = 2;
            relay400mButton.Text = "4x400";
            relay400mButton.UseVisualStyleBackColor = true;
            relay400mButton.Click += relay400mButton_Click;
            // 
            // relay200mButton
            // 
            relay200mButton.Dock = DockStyle.Fill;
            relay200mButton.FlatStyle = FlatStyle.Flat;
            relay200mButton.Location = new Point(77, 3);
            relay200mButton.Name = "relay200mButton";
            relay200mButton.Size = new Size(68, 28);
            relay200mButton.TabIndex = 1;
            relay200mButton.Text = "4x200";
            relay200mButton.UseVisualStyleBackColor = true;
            relay200mButton.Click += relay200mButton_Click;
            // 
            // relay100mButton
            // 
            relay100mButton.Dock = DockStyle.Fill;
            relay100mButton.FlatStyle = FlatStyle.Flat;
            relay100mButton.Location = new Point(3, 3);
            relay100mButton.Name = "relay100mButton";
            relay100mButton.Size = new Size(68, 28);
            relay100mButton.TabIndex = 0;
            relay100mButton.Text = "4x100";
            relay100mButton.UseVisualStyleBackColor = true;
            relay100mButton.Click += relay100mButton_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(525, 447);
            Controls.Add(tableLayoutPanel1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(348, 325);
            Name = "Form1";
            Text = "Lynx Event Combine";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel4.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TextBox databasePathText;
        private Label label1;
        private Button chooseDirButton;
        private ListBox eventListBox;
        private Label label2;
        private Label label3;
        private ComboBox mainEventComboBox;
        private Button combineButton;
        private Button splitLifButton;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel3;
        private CheckBox removeGenderCheckBox;
        private Button reloadButton;
        private TableLayoutPanel tableLayoutPanel4;
        private Button relay100mButton;
        private Button run3200mButton;
        private Button run1600mButton;
        private Button run800mButton;
        private Button relay800mButton;
        private Button relay400mButton;
        private Button relay200mButton;
    }
}
