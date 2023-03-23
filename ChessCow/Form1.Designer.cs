
namespace ChessCow
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
            this.ChessBoardPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // ChessBoardPanel
            // 
            this.ChessBoardPanel.Location = new System.Drawing.Point(12, 12);
            this.ChessBoardPanel.Name = "ChessBoardPanel";
            this.ChessBoardPanel.Size = new System.Drawing.Size(522, 522);
            this.ChessBoardPanel.TabIndex = 0;
            this.ChessBoardPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.ChessBoardPanel_Paint);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 558);
            this.Controls.Add(this.ChessBoardPanel);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel ChessBoardPanel;
    }
}

