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
            this.Treeview = new TopSolid.Kernel.WX.Controls.TreeView();
            this.listBox = new TopSolid.Kernel.WX.Controls.ListBox();
            this.button = new TopSolid.Kernel.WX.Controls.Button();
            this.SuspendLayout();
            // 
            // treeview
            // 
            this.Treeview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Treeview.Location = new System.Drawing.Point(3, 3);
            this.Treeview.MaximumSize = new System.Drawing.Size(382, 186);
            this.Treeview.MinimumSize = new System.Drawing.Size(382, 186);
            this.Treeview.Name = "treeview";
            this.Treeview.Size = new System.Drawing.Size(382, 186);
            this.Treeview.TabIndex = 1;
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
            this.Controls.Add(this.listBox);
            this.Controls.Add(this.Treeview);
            this.Controls.Add(this.button);
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
