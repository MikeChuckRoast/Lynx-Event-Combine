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
            SuspendLayout();
            // 
            // databasePathText
            // 
            databasePathText.Location = new Point(25, 33);
            databasePathText.Name = "databasePathText";
            databasePathText.Size = new Size(379, 23);
            databasePathText.TabIndex = 0;
            databasePathText.TextChanged += databasePathText_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(25, 15);
            label1.Name = "label1";
            label1.Size = new Size(109, 15);
            label1.TabIndex = 1;
            label1.Text = "Lynx event file path";
            // 
            // chooseDirButton
            // 
            chooseDirButton.Location = new Point(410, 33);
            chooseDirButton.Name = "chooseDirButton";
            chooseDirButton.Size = new Size(75, 23);
            chooseDirButton.TabIndex = 2;
            chooseDirButton.Text = "Choose...";
            chooseDirButton.UseVisualStyleBackColor = true;
            chooseDirButton.Click += chooseDirButton_Click;
            // 
            // eventListBox
            // 
            eventListBox.FormattingEnabled = true;
            eventListBox.ItemHeight = 15;
            eventListBox.Location = new Point(25, 152);
            eventListBox.Name = "eventListBox";
            eventListBox.SelectionMode = SelectionMode.MultiSimple;
            eventListBox.Size = new Size(379, 94);
            eventListBox.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(25, 134);
            label2.Name = "label2";
            label2.Size = new Size(121, 15);
            label2.TabIndex = 4;
            label2.Text = "Choose events to add";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(25, 71);
            label3.Name = "label3";
            label3.Size = new Size(316, 15);
            label3.TabIndex = 6;
            label3.Text = "Main event (other event entries will be added to this event)";
            // 
            // mainEventComboBox
            // 
            mainEventComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            mainEventComboBox.FormattingEnabled = true;
            mainEventComboBox.Location = new Point(25, 89);
            mainEventComboBox.Name = "mainEventComboBox";
            mainEventComboBox.Size = new Size(379, 23);
            mainEventComboBox.TabIndex = 7;
            // 
            // combineButton
            // 
            combineButton.Location = new Point(95, 269);
            combineButton.Name = "combineButton";
            combineButton.Size = new Size(75, 23);
            combineButton.TabIndex = 8;
            combineButton.Text = "Combine";
            combineButton.UseVisualStyleBackColor = true;
            combineButton.Click += combineButton_Click;
            // 
            // splitLifButton
            // 
            splitLifButton.Location = new Point(250, 269);
            splitLifButton.Name = "splitLifButton";
            splitLifButton.Size = new Size(75, 23);
            splitLifButton.TabIndex = 9;
            splitLifButton.Text = "Split LIF file";
            splitLifButton.UseVisualStyleBackColor = true;
            splitLifButton.Click += splitLifButton_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(510, 316);
            Controls.Add(splitLifButton);
            Controls.Add(combineButton);
            Controls.Add(mainEventComboBox);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(eventListBox);
            Controls.Add(chooseDirButton);
            Controls.Add(label1);
            Controls.Add(databasePathText);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            Text = "Lynx Event Combine";
            ResumeLayout(false);
            PerformLayout();
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
    }
}
