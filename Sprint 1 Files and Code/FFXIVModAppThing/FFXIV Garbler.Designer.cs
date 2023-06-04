namespace FFXIVModAppThing
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
            label1 = new Label();
            inputTextBox = new TextBox();
            label2 = new Label();
            levelTextBox = new TextBox();
            clipboardCheckbox = new CheckBox();
            label3 = new Label();
            outputTextBox = new TextBox();
            garbleButton = new Button();
            copyButton = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(35, 15);
            label1.TabIndex = 0;
            label1.Text = "Input";
            // 
            // inputTextBox
            // 
            inputTextBox.Location = new Point(53, 6);
            inputTextBox.Name = "inputTextBox";
            inputTextBox.Size = new Size(735, 23);
            inputTextBox.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 43);
            label2.Name = "label2";
            label2.Size = new Size(34, 15);
            label2.TabIndex = 2;
            label2.Text = "Level";
            // 
            // levelTextBox
            // 
            levelTextBox.Location = new Point(53, 40);
            levelTextBox.Name = "levelTextBox";
            levelTextBox.Size = new Size(29, 23);
            levelTextBox.TabIndex = 3;
            // 
            // clipboardCheckbox
            // 
            clipboardCheckbox.AutoSize = true;
            clipboardCheckbox.Checked = true;
            clipboardCheckbox.CheckState = CheckState.Checked;
            clipboardCheckbox.Location = new Point(88, 42);
            clipboardCheckbox.Name = "clipboardCheckbox";
            clipboardCheckbox.Size = new Size(200, 19);
            clipboardCheckbox.TabIndex = 4;
            clipboardCheckbox.Text = "Automatically Copy to Clipboard";
            clipboardCheckbox.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(2, 75);
            label3.Name = "label3";
            label3.Size = new Size(45, 15);
            label3.TabIndex = 5;
            label3.Text = "Output";
            // 
            // outputTextBox
            // 
            outputTextBox.Location = new Point(53, 72);
            outputTextBox.Name = "outputTextBox";
            outputTextBox.ReadOnly = true;
            outputTextBox.Size = new Size(735, 23);
            outputTextBox.TabIndex = 6;
            // 
            // garbleButton
            // 
            garbleButton.Location = new Point(53, 101);
            garbleButton.Name = "garbleButton";
            garbleButton.Size = new Size(158, 23);
            garbleButton.TabIndex = 7;
            garbleButton.Text = "Garble";
            garbleButton.UseVisualStyleBackColor = true;
            garbleButton.Click += garbleButton_Click;
            // 
            // copyButton
            // 
            copyButton.Location = new Point(217, 101);
            copyButton.Name = "copyButton";
            copyButton.Size = new Size(154, 23);
            copyButton.TabIndex = 8;
            copyButton.Text = "Copy To Clipboard";
            copyButton.UseVisualStyleBackColor = true;
            copyButton.Click += copyButton_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 134);
            Controls.Add(copyButton);
            Controls.Add(garbleButton);
            Controls.Add(outputTextBox);
            Controls.Add(label3);
            Controls.Add(clipboardCheckbox);
            Controls.Add(levelTextBox);
            Controls.Add(label2);
            Controls.Add(inputTextBox);
            Controls.Add(label1);
            Name = "FFXIV Garbler";
            Text = "FFXIV Garbler";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox inputTextBox;
        private Label label2;
        private TextBox levelTextBox;
        private CheckBox clipboardCheckbox;
        private Label label3;
        private TextBox outputTextBox;
        private Button garbleButton;
        private Button copyButton;
    }
}