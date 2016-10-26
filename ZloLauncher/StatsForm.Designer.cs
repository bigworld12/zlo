namespace ZloLauncher
{
    partial class StatsForm
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
            this.StatsDGV = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.StatsDGV)).BeginInit();
            this.SuspendLayout();
            // 
            // StatsDGV
            // 
            this.StatsDGV.AllowUserToAddRows = false;
            this.StatsDGV.AllowUserToDeleteRows = false;
            this.StatsDGV.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.StatsDGV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StatsDGV.Location = new System.Drawing.Point(0, 0);
            this.StatsDGV.MultiSelect = false;
            this.StatsDGV.Name = "StatsDGV";
            this.StatsDGV.ReadOnly = true;
            this.StatsDGV.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.StatsDGV.ShowEditingIcon = false;
            this.StatsDGV.Size = new System.Drawing.Size(583, 374);
            this.StatsDGV.TabIndex = 0;
            // 
            // StatsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(583, 374);
            this.Controls.Add(this.StatsDGV);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "StatsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Stats";
            this.Load += new System.EventHandler(this.StatsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.StatsDGV)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView StatsDGV;
    }
}