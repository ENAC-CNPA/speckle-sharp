using Avalonia.Collections;
using DesktopUI2.Views;
using System;
using System.Linq;
using TopSolid.Cad.Design.DB.Documents;
using TopSolid.Kernel.DB.D3.Shapes;
using TopSolid.Kernel.DB.Entities;
using TopSolid.Kernel.G.D3.Curves;
using TopSolid.Kernel.GR.D3;
using TopSolid.Kernel.GR.Displays;
using TopSolid.Kernel.GR.Items;
using TopSolid.Kernel.SX.Drawing;
using TopSolid.Kernel.SX.Resources;
using TopSolid.Kernel.UI.D3;
using TopSolid.Kernel.UI.Trees;
using TopSolid.Kernel.WX;
using TopSolid.Kernel.WX.Docking;
using TopSolid.Kernel.WX.EnhancedContainers;
using Application = TopSolid.Kernel.WX.Application;
using TreeNode = TopSolid.Kernel.WX.Controls.TreeNode;
using System.Windows;
using System.Windows.Controls;
using DesktopUI2.Views;

namespace Speckle.ConnectorTopSolid.UI.CustomWindows
{
    public partial class SpeckleWindow : EnhancedContainer
    {
        // Static fields:

        /// <summary>
        /// Instance of the docked content.
        /// </summary>
        private DockedContent dockedContent = null;

        /// <summary>
        /// Instance of the general display
        /// </summary>
        private GeneralDisplay previewDisplay;

        /// <summary>
        /// Instance of the initial face color
        /// </summary>
        private static TopSolid.Kernel.SX.Drawing.Color initialFaceColor;
        private System.Windows.Forms.Panel specklePanel;

        /// <summary>
        /// Instance of the previous selected shape entity
        /// </summary>
        private static ShapeEntity previousSelectedShapeEntity;

        // Properties:

        /// <summary>
        /// Gets the docked content.
        /// </summary>
        public DockedContent DockedContent
        {
            get
            {
                return dockedContent;
            }
            set
            {
                dockedContent = value;
            }
        }

        // Constructor:

        /// <summary>
        /// Initializes a new instance of the <see cref="SpeckleWindow"/> class.
        /// </summary>
        public SpeckleWindow()
        {
            InitializeComponent();
            //AvaloniaHost.MessageHook += AvaloniaHost_MessageHook; // TODO

            // Add the preview display in the display of the document
            PartDocument partDocument = Application.ActiveDocument as PartDocument;

            if (partDocument == null) return;
            previewDisplay = null;
            //partDocument.Display.AddDisplay(previewDisplay);

            // Add a tree node in the window
            TreeNode node = new ElementTreeNode(Treeview, null, null);
            node.Text = "Noeud 1";
            node.Nodes.Add("Test enfant");
            Treeview.Nodes.Add(node);

            // Update the list box
            UpdateListBox();

            // Set needed evenments
            Application.CurrentDocumentChanged += ApplicationOnCurrentDocumentChanged;
            partDocument.Updating += PartDocumentOnUpdating;
            listBox.SelectedValueChanged += ListBoxOnSelectedValueChanged;
            listBox.LostFocus += ListBoxOnLostFocus;
            partDocument.Saving += PartDocumentOnSaving;
            button.Click += ButtonOnClick;

        }

        private const UInt32 DLGC_WANTARROWS = 0x0001;
        private const UInt32 DLGC_HASSETSEL = 0x0008;
        private const UInt32 DLGC_WANTCHARS = 0x0080;
        private const UInt32 WM_GETDLGCODE = 0x0087;

        private IntPtr AvaloniaHost_MessageHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != WM_GETDLGCODE) return IntPtr.Zero;
            handled = true;
            return new IntPtr(DLGC_WANTCHARS | DLGC_WANTARROWS | DLGC_HASSETSEL);
        }


        // Methods:

        /// <summary>
        /// Adds the docked <see cref="SpeckleWindow"/>.
        /// </summary>
        public void AddOrModifyDockedWindow()
        {
            // Create a new application window and update the list box
            ApplicationWindow window = Application.Window;

            if (window == null)
            {
                UpdateListBox();
                return;
            }

            ResourceManager res =  Speckle.ConnectorTopSolid.UI.Resources.Manager;

            DockedContent = window.AddDockedContent(
                this,
                GetType(),
                DockedContentVisibility.Visible,
                res.GetString("SpeckleWindow"),
                typeof(Resources), "SpeckleWindow.ico",
                true,
                DockingState.Left,
                false);

            // Update the list box
            UpdateListBox();
        }

        private void UpdateListBox()
        {
            // Clear the list box
            listBox.Items.Clear();

            if (Application.ActiveDocument == null) return;

            if (!(Application.ActiveDocument is PartDocument)) return;

            // Get the active document
            PartDocument partDocument = Application.ActiveDocument as PartDocument;

            // Add all shape entities in the list box
            foreach (Entity entity in partDocument.ShapesFolderEntity.Entities)
            {
                listBox.Items.Add(entity);
            }

        }

        private void ClearDisplay(PartDocument partDocument)
        {
            //Clear the display
            if (previewDisplay != null)
                partDocument.Display.RemoveDisplay(previewDisplay);

            //Clear the echo of the previously selected shape
            if (previousSelectedShapeEntity != null)
            {
                foreach (DisplayItem displayItem in previousSelectedShapeEntity.Display.Items)
                {
                    if (displayItem is FaceItem)
                    {
                        // Set the color of the face to the initial face color
                        displayItem.Color = initialFaceColor;
                    }
                }

                // Update the document display
                partDocument.Display.UpdateDisplay(previousSelectedShapeEntity.Display);
                partDocument.Display.UpdateDisplay(previewDisplay);
            }
        }

        private void ListBoxOnSelectedValueChanged(object sender, EventArgs e)
        {
            PartDocument partDocument = Application.ActiveDocument as PartDocument;
            if (partDocument == null) return;

            // Clear the display
            ClearDisplay(partDocument);

            // Set a new preview display
            previewDisplay = new GeneralDisplay(null);

            if (listBox.SelectedItem == null) return;

            // Get the entity to highlight
            ShapeEntity entity = partDocument.ShapesFolderEntity.SearchEntity(listBox.SelectedItem.ToString()) as ShapeEntity;

            // Go through all display items of the shape
            foreach (DisplayItem displayItem in entity.Display.Items)
            {
                // Set a color to all faces
                if (displayItem is FaceItem)
                {
                    // Keep the initial color in a static field in order to change it when the entity is deselected
                    initialFaceColor = displayItem.Color;

                    // Set the color
                    displayItem.Color = ApplicationColors.FirstActiveSelectionColor;
                }

                // Set a color to all edeges
                // In order to change the color of all edges and to see those edges everywhere, it is necessary to use curve display item
                // EdgeItems are only visible if there is no object to occur the view
                if (displayItem is EdgeItem)
                {
                    Curve curve = entity.Geometry.Edges.FirstOrDefault(x => x.Label == displayItem.Label).GetGeometry(true);

                    // Make a curve display item
                    DisplayItem curveDisplayItem = ItemsMaker.MakeCurveDisplayItem(curve);

                    // Set the color
                    curveDisplayItem.Color = ApplicationColors.FirstActiveSelectionColor;

                    // Add curves to the preview display
                    previewDisplay.Add(curveDisplayItem);
                }
            }

            // Update the display of the entity
            partDocument.Display.UpdateDisplay(entity.Display);

            // Add the preview display to the display of the document
            partDocument.Display.AddDisplay(previewDisplay);

            // Keep the modified entity in a static field in order to change it when the entity is deselected
            previousSelectedShapeEntity = entity;
        }

        private void ApplicationOnCurrentDocumentChanged(object sender, EventArgs e)
        {
            UpdateListBox();

            PartDocument partDocument = Application.ActiveDocument as PartDocument;
            if (partDocument == null) return;
            ClearDisplay(partDocument);
        }

        private void PartDocumentOnUpdating(object sender, EventArgs e)
        {
            PartDocument partDocument = Application.ActiveDocument as PartDocument;
            if (partDocument == null) return;
            ClearDisplay(partDocument);
        }

        private void ListBoxOnLostFocus(object sender, EventArgs e)
        {
            listBox.ClearSelected();
            
            PartDocument partDocument = Application.ActiveDocument as PartDocument;
            if (partDocument == null) return;
            ClearDisplay(partDocument);
        }

        private void PartDocumentOnSaving(object sender, EventArgs e)
        {
            PartDocument partDocument = Application.ActiveDocument as PartDocument;
            if (partDocument == null) return;
            ClearDisplay(partDocument);
        }

        private void ButtonOnClick(object sender, EventArgs e)
        {
            //SmartSurfaceCommand cmd = new SmartSurfaceCommand();
            //cmd.DoInvoke();

            listBox.Items.Add("EPFL");
            listBox.Items.Add("Super");
            listBox.Items.Add("Speckle");
        }


    }
}
