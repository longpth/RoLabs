

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
            stopButton = new System.Windows.Forms.Button();
            glControl = new OpenTK.WinForms.GLControl();
            ((System.ComponentModel.ISupportInitialize)pictureBoxRaw).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxProcess).BeginInit();
            SuspendLayout();
            // 
            // pictureBoxRaw
            // 
            pictureBoxRaw.Location = new System.Drawing.Point(6, 5);
            pictureBoxRaw.Name = "pictureBoxRaw";
            pictureBoxRaw.Size = new System.Drawing.Size(620, 405);
            pictureBoxRaw.TabIndex = 0;
            pictureBoxRaw.TabStop = false;
            // 
            // pictureBoxProcess
            // 
            pictureBoxProcess.Location = new System.Drawing.Point(6, 416);
            pictureBoxProcess.Name = "pictureBoxProcess";
            pictureBoxProcess.Size = new System.Drawing.Size(620, 429);
            pictureBoxProcess.TabIndex = 1;
            pictureBoxProcess.TabStop = false;
            // 
            // startButton
            // 
            startButton.Location = new System.Drawing.Point(6, 869);
            startButton.Name = "startButton";
            startButton.Size = new System.Drawing.Size(94, 29);
            startButton.TabIndex = 2;
            startButton.Text = "Start";
            startButton.UseVisualStyleBackColor = true;
            startButton.Click += startButton_Click;
            // 
            // stopButton
            // 
            stopButton.Location = new System.Drawing.Point(6, 904);
            stopButton.Name = "stopButton";
            stopButton.Size = new System.Drawing.Size(94, 29);
            stopButton.TabIndex = 3;
            stopButton.Text = "Stop";
            stopButton.UseVisualStyleBackColor = true;
            stopButton.Click += stopButton_Click;
            // 
            // glControl
            // 
            glControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            glControl.API = OpenTK.Windowing.Common.ContextAPI.OpenGL;
            glControl.APIVersion = new Version(3, 3, 0, 0);
            glControl.Flags = OpenTK.Windowing.Common.ContextFlags.Debug;
            glControl.IsEventDriven = true;
            glControl.Location = new System.Drawing.Point(632, 5);
            glControl.Margin = new Padding(3, 4, 3, 4);
            glControl.Name = "glControl";
            glControl.Profile = OpenTK.Windowing.Common.ContextProfile.Core;
            glControl.Size = new System.Drawing.Size(830, 840);
            glControl.TabIndex = 4;
            glControl.Text = "glControl1";
            glControl.Load += glControl_Load;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1474, 945);
            Controls.Add(glControl);
            Controls.Add(stopButton);
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
        private System.Windows.Forms.Button stopButton;
        private OpenTK.WinForms.GLControl glControl;
    }
}
