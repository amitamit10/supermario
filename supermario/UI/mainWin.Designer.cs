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
            if (disposing)
            {
                if (components != null) components.Dispose();
                gameTimer?.Dispose();
                _hudLabel?.Font?.Dispose();
                _scoreLabel?.Font?.Dispose();
                if (_heartLabels != null)
                {
                    foreach (var lbl in _heartLabels) lbl?.Font?.Dispose();
                }
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
            this.picboxplayer = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picboxplayer)).BeginInit();
            this.SuspendLayout();
            // 
            // picboxplayer
            // 
            this.picboxplayer.BackColor = System.Drawing.Color.Transparent;
            this.picboxplayer.Image = global::supermario.Properties.Resources.dcaeqy1_614416a8_3ae1_4448_94b4_e3ecefa3e53a;
            this.picboxplayer.Location = new System.Drawing.Point(5, 342);
            this.picboxplayer.Name = "picboxplayer";
            this.picboxplayer.Size = new System.Drawing.Size(135, 108);
            this.picboxplayer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picboxplayer.TabIndex = 1;
            this.picboxplayer.TabStop = false;
            this.picboxplayer.WaitOnLoad = true;
            // 
            // mainWin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::supermario.Properties.Resources.oie_RF5qvlenQ7kF1;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(982, 553);
            this.Controls.Add(this.picboxplayer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Name = "mainWin";
            this.Text = "Super Mario";
            this.Load += new System.EventHandler(this.mainWin_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picboxplayer)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.PictureBox picboxplayer;
    }
}