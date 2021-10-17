
namespace TheLeftExit.Trickster {
    partial class MainForm {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.openButton = new System.Windows.Forms.Button();
            this.readButton = new System.Windows.Forms.Button();
            this.getTypesButton = new System.Windows.Forms.Button();
            this.scanButton = new System.Windows.Forms.Button();
            this.getTypesComboBox = new System.Windows.Forms.ComboBox();
            this.scanListBox = new System.Windows.Forms.ListBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.openTextBox = new System.Windows.Forms.TextBox();
            this.openLabel = new System.Windows.Forms.Label();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // openButton
            // 
            this.openButton.Location = new System.Drawing.Point(380, 11);
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(156, 23);
            this.openButton.TabIndex = 0;
            this.openButton.Text = "Open...";
            this.openButton.UseVisualStyleBackColor = true;
            this.openButton.Click += new System.EventHandler(this.openButtonClick);
            // 
            // readButton
            // 
            this.readButton.Location = new System.Drawing.Point(380, 41);
            this.readButton.Name = "readButton";
            this.readButton.Size = new System.Drawing.Size(75, 23);
            this.readButton.TabIndex = 0;
            this.readButton.Text = "Read";
            this.readButton.UseVisualStyleBackColor = true;
            this.readButton.Click += new System.EventHandler(this.readButton_Click);
            // 
            // getTypesButton
            // 
            this.getTypesButton.Location = new System.Drawing.Point(12, 41);
            this.getTypesButton.Name = "getTypesButton";
            this.getTypesButton.Size = new System.Drawing.Size(75, 23);
            this.getTypesButton.TabIndex = 0;
            this.getTypesButton.Text = "Get types";
            this.getTypesButton.UseVisualStyleBackColor = true;
            this.getTypesButton.Click += new System.EventHandler(this.getTypesButtonClick);
            // 
            // scanButton
            // 
            this.scanButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.scanButton.Location = new System.Drawing.Point(461, 41);
            this.scanButton.Name = "scanButton";
            this.scanButton.Size = new System.Drawing.Size(75, 23);
            this.scanButton.TabIndex = 0;
            this.scanButton.Text = "Scan";
            this.scanButton.UseVisualStyleBackColor = true;
            this.scanButton.Click += new System.EventHandler(this.scanButton_Click);
            // 
            // getTypesComboBox
            // 
            this.getTypesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.getTypesComboBox.FormattingEnabled = true;
            this.getTypesComboBox.Location = new System.Drawing.Point(93, 41);
            this.getTypesComboBox.Name = "getTypesComboBox";
            this.getTypesComboBox.Size = new System.Drawing.Size(281, 23);
            this.getTypesComboBox.TabIndex = 1;
            this.getTypesComboBox.SelectedIndexChanged += new System.EventHandler(this.getTypesComboBoxSelectedIndexChanged);
            // 
            // scanListBox
            // 
            this.scanListBox.FormattingEnabled = true;
            this.scanListBox.ItemHeight = 15;
            this.scanListBox.Location = new System.Drawing.Point(12, 70);
            this.scanListBox.Name = "scanListBox";
            this.scanListBox.Size = new System.Drawing.Size(524, 169);
            this.scanListBox.TabIndex = 3;
            this.scanListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.scanListBoxMouseDown);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 242);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(548, 22);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 5;
            this.statusStrip.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(29, 17);
            this.statusLabel.Text = "N/A";
            // 
            // openTextBox
            // 
            this.openTextBox.Location = new System.Drawing.Point(93, 12);
            this.openTextBox.Name = "openTextBox";
            this.openTextBox.Size = new System.Drawing.Size(281, 23);
            this.openTextBox.TabIndex = 6;
            // 
            // openLabel
            // 
            this.openLabel.AutoSize = true;
            this.openLabel.Location = new System.Drawing.Point(37, 15);
            this.openLabel.Name = "openLabel";
            this.openLabel.Size = new System.Drawing.Size(50, 15);
            this.openLabel.TabIndex = 7;
            this.openLabel.Text = "Process:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(548, 264);
            this.Controls.Add(this.openLabel);
            this.Controls.Add(this.openTextBox);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.scanListBox);
            this.Controls.Add(this.getTypesComboBox);
            this.Controls.Add(this.getTypesButton);
            this.Controls.Add(this.scanButton);
            this.Controls.Add(this.readButton);
            this.Controls.Add(this.openButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MainForm";
            this.Text = "Trickster";
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button openButton;
        private System.Windows.Forms.Button readButton;
        private System.Windows.Forms.Button getTypesButton;
        private System.Windows.Forms.Button scanButton;
        private System.Windows.Forms.ComboBox getTypesComboBox;
        private System.Windows.Forms.ListBox scanListBox;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.TextBox openTextBox;
        private System.Windows.Forms.Label openLabel;
    }
}

