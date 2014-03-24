namespace Potassium.Examples.Signals
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
            this.label1 = new System.Windows.Forms.Label();
            this.frequency = new System.Windows.Forms.NumericUpDown();
            this.startBtn = new System.Windows.Forms.Button();
            this.stopBtn = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.sinValue = new System.Windows.Forms.TextBox();
            this.ticks = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.degValue = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.radValue = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cosValue = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.frequency)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Frequency (Hz)";
            // 
            // frequency
            // 
            this.frequency.Location = new System.Drawing.Point(128, 17);
            this.frequency.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.frequency.Name = "frequency";
            this.frequency.Size = new System.Drawing.Size(167, 20);
            this.frequency.TabIndex = 1;
            this.frequency.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // startBtn
            // 
            this.startBtn.Location = new System.Drawing.Point(13, 56);
            this.startBtn.Name = "startBtn";
            this.startBtn.Size = new System.Drawing.Size(75, 23);
            this.startBtn.TabIndex = 2;
            this.startBtn.Text = "Start";
            this.startBtn.UseVisualStyleBackColor = true;
            // 
            // stopBtn
            // 
            this.stopBtn.Location = new System.Drawing.Point(128, 56);
            this.stopBtn.Name = "stopBtn";
            this.stopBtn.Size = new System.Drawing.Size(75, 23);
            this.stopBtn.TabIndex = 3;
            this.stopBtn.Text = "Stop";
            this.stopBtn.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 200);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(28, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Sine";
            // 
            // sinValue
            // 
            this.sinValue.Location = new System.Drawing.Point(128, 196);
            this.sinValue.Name = "sinValue";
            this.sinValue.ReadOnly = true;
            this.sinValue.Size = new System.Drawing.Size(167, 20);
            this.sinValue.TabIndex = 6;
            // 
            // ticks
            // 
            this.ticks.Location = new System.Drawing.Point(128, 266);
            this.ticks.Name = "ticks";
            this.ticks.ReadOnly = true;
            this.ticks.Size = new System.Drawing.Size(167, 20);
            this.ticks.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(17, 270);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Ticks";
            // 
            // degValue
            // 
            this.degValue.Location = new System.Drawing.Point(128, 126);
            this.degValue.Name = "degValue";
            this.degValue.ReadOnly = true;
            this.degValue.Size = new System.Drawing.Size(167, 20);
            this.degValue.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 130);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Degrees";
            // 
            // radValue
            // 
            this.radValue.Location = new System.Drawing.Point(128, 161);
            this.radValue.Name = "radValue";
            this.radValue.ReadOnly = true;
            this.radValue.Size = new System.Drawing.Size(167, 20);
            this.radValue.TabIndex = 4;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(17, 165);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(46, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Radians";
            // 
            // cosValue
            // 
            this.cosValue.Location = new System.Drawing.Point(128, 231);
            this.cosValue.Name = "cosValue";
            this.cosValue.ReadOnly = true;
            this.cosValue.Size = new System.Drawing.Size(167, 20);
            this.cosValue.TabIndex = 7;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(17, 235);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(39, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Cosine";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(549, 431);
            this.Controls.Add(this.cosValue);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.radValue);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.degValue);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ticks);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.sinValue);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.stopBtn);
            this.Controls.Add(this.startBtn);
            this.Controls.Add(this.frequency);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.frequency)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown frequency;
        private System.Windows.Forms.Button startBtn;
        private System.Windows.Forms.Button stopBtn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox sinValue;
        private System.Windows.Forms.TextBox ticks;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox degValue;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox radValue;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox cosValue;
        private System.Windows.Forms.Label label6;
    }
}