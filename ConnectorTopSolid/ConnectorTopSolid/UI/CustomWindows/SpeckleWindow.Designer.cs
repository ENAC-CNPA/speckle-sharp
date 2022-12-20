using System.Linq;
using System.Windows;
using TopSolid.Kernel.WX.Controls;

namespace Speckle.ConnectorTopSolid.UI.CustomWindows
{
    partial class SpeckleWindow
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

        #region Component Designer generated code

        private TreeView treeview;

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.listBox = new TopSolid.Kernel.WX.Controls.ListBox();
            this.button = new TopSolid.Kernel.WX.Controls.Button();
            this.specklePanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // specklePanel
            // 
            this.specklePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.specklePanel.Location = new System.Drawing.Point(0, 0);
            this.specklePanel.Name = "specklePanel";
            this.specklePanel.Size = new System.Drawing.Size(665, 909);
            this.specklePanel.TabIndex = 0;
            //
            // 
            // listBox
            // 
            this.listBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox.Location = new System.Drawing.Point(3, 263);
            this.listBox.Name = "listBox";
            this.listBox.Size = new System.Drawing.Size(382, 162);
            this.listBox.TabIndex = 2;
            // 
            // button
            // 
            this.button.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button.Location = new System.Drawing.Point(3, 195);
            this.button.MaximumSize = new System.Drawing.Size(382, 62);
            this.button.MinimumSize = new System.Drawing.Size(382, 62);
            this.button.Name = "button";
            this.button.Size = new System.Drawing.Size(382, 62);
            this.button.TabIndex = 3;
            this.button.Text = "Open Dialog";
            this.button.Click += new System.EventHandler(this.ButtonOnClick);
            // 
            // SpeckleWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.specklePanel.Controls.Add(this.listBox);
            this.specklePanel.Controls.Add(this.Treeview);
            this.specklePanel.Controls.Add(this.button);
            this.Controls.Add(this.specklePanel);
            this.Name = "SpeckleWindow";
            this.Size = new System.Drawing.Size(388, 428);
            this.ResumeLayout(false);

        }

        #endregion

        private ListBox listBox;
        private Button button;

        public TreeView Treeview { get => treeview; set => treeview = value; }
    }
}
