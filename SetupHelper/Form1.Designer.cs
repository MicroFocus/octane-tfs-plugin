namespace SetupHelper
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtVersion = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnUpdateVersion = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSolutionPath = new System.Windows.Forms.TextBox();
            this.btnSelectPath = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // txtVersion
            // 
            this.txtVersion.Location = new System.Drawing.Point(92, 37);
            this.txtVersion.Name = "txtVersion";
            this.txtVersion.Size = new System.Drawing.Size(351, 20);
            this.txtVersion.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(38, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Version :";
            // 
            // btnUpdateVersion
            // 
            this.btnUpdateVersion.Location = new System.Drawing.Point(458, 35);
            this.btnUpdateVersion.Name = "btnUpdateVersion";
            this.btnUpdateVersion.Size = new System.Drawing.Size(112, 23);
            this.btnUpdateVersion.TabIndex = 2;
            this.btnUpdateVersion.Text = "Update Version";
            this.btnUpdateVersion.UseVisualStyleBackColor = true;
            this.btnUpdateVersion.Click += new System.EventHandler(this.btnUpdateVersion_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Solution path :";
            // 
            // txtSolutionPath
            // 
            this.txtSolutionPath.Location = new System.Drawing.Point(92, 6);
            this.txtSolutionPath.Name = "txtSolutionPath";
            this.txtSolutionPath.ReadOnly = true;
            this.txtSolutionPath.Size = new System.Drawing.Size(351, 20);
            this.txtSolutionPath.TabIndex = 4;
            // 
            // btnSelectPath
            // 
            this.btnSelectPath.Location = new System.Drawing.Point(458, 4);
            this.btnSelectPath.Name = "btnSelectPath";
            this.btnSelectPath.Size = new System.Drawing.Size(112, 23);
            this.btnSelectPath.TabIndex = 5;
            this.btnSelectPath.Text = "Select path";
            this.btnSelectPath.UseVisualStyleBackColor = true;
            this.btnSelectPath.Click += new System.EventHandler(this.btnSelectPath_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(14, 106);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(594, 384);
            this.richTextBox1.TabIndex = 6;
            this.richTextBox1.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(620, 502);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.btnSelectPath);
            this.Controls.Add(this.txtSolutionPath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnUpdateVersion);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtVersion);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtVersion;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnUpdateVersion;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSolutionPath;
        private System.Windows.Forms.Button btnSelectPath;
        private System.Windows.Forms.RichTextBox richTextBox1;
    }
}

