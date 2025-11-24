namespace supermario
{
    partial class mainWin
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
            this.BOTTOM = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.BOTTOM)).BeginInit();
            this.SuspendLayout();
            // 
            // BOTTOM
            // 
            this.BOTTOM.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BOTTOM.Image = global::supermario.Properties.Resources.oie_cnoULepStRjX;
            this.BOTTOM.Location = new System.Drawing.Point(-25, 518);
            this.BOTTOM.Name = "BOTTOM";
            this.BOTTOM.Size = new System.Drawing.Size(1005, 58);
            this.BOTTOM.TabIndex = 0;
            this.BOTTOM.TabStop = false;
            // 
            // mainWin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::supermario.Properties.Resources.oie_RF5qvlenQ7kF1;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(982, 553);
            this.Controls.Add(this.BOTTOM);
            this.MaximumSize = new System.Drawing.Size(1000, 600);
            this.MinimumSize = new System.Drawing.Size(1000, 600);
            this.Name = "mainWin";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.mainWin_Load);
            ((System.ComponentModel.ISupportInitialize)(this.BOTTOM)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox BOTTOM;
    }
}

