namespace RacerCS
{
    partial class RacerForm
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
            this.SuspendLayout();
            // 
            // RacerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(304, 201);
            this.Name = "RacerForm";
            this.Text = "Racer C#";
            this.Load += new System.EventHandler(this.RacerForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RacerForm_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.RacerForm_KeyUp);
            this.ResumeLayout(false);

        }

        #endregion
    }
}

