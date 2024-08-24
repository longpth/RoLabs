

namespace RoLabsSlam.Test
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
                components?.Dispose();
                _frame?.Dispose();
                _videoCapture?.Dispose();
                _timer?.Dispose();
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
            pictureBoxRaw = new PictureBox();
            pictureBoxProcess = new PictureBox();
            startButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)pictureBoxRaw).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxProcess).BeginInit();
            SuspendLayout();
            // 
            // pictureBoxRaw
            // 
            pictureBoxRaw.Location = new System.Drawing.Point(12, 12);
            pictureBoxRaw.Name = "pictureBoxRaw";
            pictureBoxRaw.Size = new System.Drawing.Size(664, 682);
            pictureBoxRaw.TabIndex = 0;
            pictureBoxRaw.TabStop = false;
            // 
            // pictureBoxProcess
            // 
            pictureBoxProcess.Location = new System.Drawing.Point(682, 12);
            pictureBoxProcess.Name = "pictureBoxProcess";
            pictureBoxProcess.Size = new System.Drawing.Size(732, 682);
            pictureBoxProcess.TabIndex = 1;
            pictureBoxProcess.TabStop = false;
            // 
            // startButton
            // 
            startButton.Location = new System.Drawing.Point(631, 754);
            startButton.Name = "startButton";
            startButton.Size = new System.Drawing.Size(94, 29);
            startButton.TabIndex = 2;
            startButton.Text = "Start";
            startButton.UseVisualStyleBackColor = true;
            startButton.Click += startButton_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1436, 795);
            Controls.Add(startButton);
            Controls.Add(pictureBoxProcess);
            Controls.Add(pictureBoxRaw);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)pictureBoxRaw).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxProcess).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pictureBoxRaw;
        private PictureBox pictureBoxProcess;
        private System.Windows.Forms.Button startButton;
    }
}
