// BlueBrick, a LEGO(c) layout editor.
// Copyright (C) 2008 Alban NANTY
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// see http://www.fsf.org/licensing/licenses/gpl.html
// and http://www.gnu.org/licenses/
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

using System;
using System.Windows.Forms;
using System.Drawing;
using BlueBrick.MapData;
using BlueBrick.Actions;
using BlueBrick.Actions.Layers;
using BlueBrick.Properties;

namespace BlueBrick
{
	public class LayerPanel : FlowLayoutPanel
	{
		private static readonly Color mSelectedColor = Color.FromKnownColor(KnownColor.ActiveBorder);
		private static readonly Color mUnselectedColor = Color.FromKnownColor(KnownColor.ControlLightLight);

		private readonly Layer mLayerReference = null;
		private Label nameLabel;
		protected PictureBox layerTypePictureBox;
		protected Button displayHullButton;
		private Button visibilityButton;

		#region get/set

		public Layer LayerReference
		{
			get { return mLayerReference; }
		}

		#endregion

		#region method for selection

		/// <summary>
		/// Call this method when you want to change the back color of the panel to make it
		/// look selected or not
		/// </summary>
		public void changeBackColor(bool isSelected)
		{
			// check if the panel to select is not already selected
			if (isSelected)
			{
				BackColor = mSelectedColor;
				nameLabel.BackColor = mSelectedColor;
			}
			else
			{
				BackColor = mUnselectedColor;
				nameLabel.BackColor = mUnselectedColor;
			}
		}
		#endregion

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LayerPanel));
			visibilityButton = new Button();
			nameLabel = new Label();
			layerTypePictureBox = new PictureBox();
			displayHullButton = new Button();
			((System.ComponentModel.ISupportInitialize)layerTypePictureBox).BeginInit();
			SuspendLayout();
			// 
			// visibilityButton
			// 
			visibilityButton.Image = (Image)resources.GetObject("visibilityButton.Image");
			visibilityButton.Location = new Point(23, 3);
			visibilityButton.Margin = new Padding(0, 3, 3, 3);
			visibilityButton.Name = "visibilityButton";
			visibilityButton.Size = new Size(20, 20);
			visibilityButton.TabIndex = 0;
			visibilityButton.UseVisualStyleBackColor = true;
			visibilityButton.Click += new EventHandler(visibilityButton_Click);
			// 
			// nameLabel
			// 
			nameLabel.BackColor = SystemColors.ControlLightLight;
			nameLabel.Location = new Point(3, 28);
			nameLabel.Margin = new Padding(3, 2, 3, 0);
			nameLabel.Name = "nameLabel";
			nameLabel.Size = new Size(100, 23);
			nameLabel.TabIndex = 0;
			nameLabel.TextAlign = ContentAlignment.MiddleLeft;
			nameLabel.UseMnemonic = false;
			nameLabel.Click += new EventHandler(LayerPanel_Click);
			nameLabel.DoubleClick += new EventHandler(LayerPanel_DoubleClick);
			// 
			// layerTypePictureBox
			// 
			layerTypePictureBox.Anchor = AnchorStyles.Left;
			layerTypePictureBox.Location = new Point(3, 5);
			layerTypePictureBox.Margin = new Padding(3, 5, 0, 0);
			layerTypePictureBox.Name = "layerTypePictureBox";
			layerTypePictureBox.Size = new Size(20, 20);
			layerTypePictureBox.TabIndex = 0;
			layerTypePictureBox.TabStop = false;
			layerTypePictureBox.Click += new EventHandler(LayerPanel_Click);
			layerTypePictureBox.DoubleClick += new EventHandler(LayerPanel_DoubleClick);
			// 
			// displayHullButton
			// 
			displayHullButton.Location = new Point(46, 3);
			displayHullButton.Margin = new Padding(0, 3, 3, 3);
			displayHullButton.Name = "displayHullButton";
			displayHullButton.Size = new Size(20, 20);
			displayHullButton.TabIndex = 0;
			displayHullButton.UseVisualStyleBackColor = true;
			displayHullButton.Click += new EventHandler(displayHullButton_Click);
			// 
			// LayerPanel
			// 
			BackColor = SystemColors.ControlLightLight;
			BorderStyle = BorderStyle.FixedSingle;
			Controls.Add(layerTypePictureBox);
			Controls.Add(visibilityButton);
			Controls.Add(displayHullButton);
			Controls.Add(nameLabel);
			Size = new Size(80, 28);
			ClientSizeChanged += new EventHandler(LayerPanel_ClientSizeChanged);
			Click += new EventHandler(LayerPanel_Click);
			DoubleClick += new EventHandler(LayerPanel_DoubleClick);
			((System.ComponentModel.ISupportInitialize)layerTypePictureBox).EndInit();
			ResumeLayout(false);

		}

		/// <summary>
		/// Default constructor only to make work the Form Designer: should not be used!
		/// </summary>
		protected LayerPanel()
		{
		}

		public LayerPanel(Layer layer)
		{
			InitializeComponent();
			mLayerReference = layer;
			updateView();

			// check if this panel is linked with the selected panel to change the color
			if (Map.Instance.SelectedLayer == layer)
			{
				BackColor = mSelectedColor;
				nameLabel.BackColor = mSelectedColor;
			}
			else
			{
				BackColor = mUnselectedColor;
				nameLabel.BackColor = mUnselectedColor;
			}
		}

		/// <summary>
		/// This method is inheritated and is usefull to get the event when the arrow are pressed
		/// </summary>
		/// <param name="keyData"></param>
		/// <returns></returns>
		protected override bool IsInputKey(Keys keyData)
		{
			// we need the four arrow keys
			// also page up and down for rotation
			// and delete and backspace for deleting object
			if ((keyData == Keys.Left) || (keyData == Keys.Right) ||
				(keyData == Keys.Up) || (keyData == Keys.Down) ||
				(keyData == Keys.PageDown) || (keyData == Keys.PageUp) ||
				(keyData == Keys.Home) || (keyData == Keys.End) ||
				(keyData == Keys.Insert) || (keyData == Keys.Delete) ||
				(keyData == Keys.Enter) || (keyData == Keys.Return) ||
				(keyData == Keys.Escape) || (keyData == Keys.Back))
				return true;

			return base.IsInputKey(keyData);
		}

		/// <summary>
		/// Update the view of this panel according to the referenced panel data
		/// </summary>
		public void updateView()
		{
			// change the name of the layer
			nameLabel.Text = mLayerReference.Name;
			// change the visible button
			if (mLayerReference.Visible)
			{
				System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LayerPanel));
				visibilityButton.Image = (Image)resources.GetObject("visibilityButton.Image");
			}
			else
			{
				visibilityButton.Image = null;
			}
			// change the display hull button
			if (mLayerReference.DisplayHulls)
				displayHullButton.Image = Resources.showHullIcon;
			else
				displayHullButton.Image = Resources.hideHullIcon;

			// change the back color if I am selected
			changeBackColor(mLayerReference == Map.Instance.SelectedLayer);
		}

		private void LayerPanel_ClientSizeChanged(object sender, EventArgs e)
		{
			int newLabelWidth = Width - visibilityButton.Width - layerTypePictureBox.Width - 20;
			if (displayHullButton.Visible)
				newLabelWidth -= displayHullButton.Width - displayHullButton.Margin.Left - displayHullButton.Margin.Right;
			nameLabel.Width = newLabelWidth;
		}

		private void visibilityButton_Click(object sender, EventArgs e)
		{
			// take the focus anyway if we click the panel
			Focus();
			// change the visibility
			if (mLayerReference.Visible)
				ActionManager.Instance.doAction(new HideLayer(mLayerReference));
			else
				ActionManager.Instance.doAction(new ShowLayer(mLayerReference));
		}

		private void displayHullButton_Click(object sender, EventArgs e)
		{
			// take the focus anyway if we click the panel
			Focus();

			// create a copy of the edited layer to hold the old data (the layer can be on any type, we just want to copy the options)
			LayerText oldLayerData = new LayerText();
			oldLayerData.CopyOptionsFrom(mLayerReference);

			// create a new layer to store the new data, and reverse the display hull flag in the new data
			LayerText newLayerData = new LayerText();
			newLayerData.CopyOptionsFrom(mLayerReference);
			newLayerData.DisplayHulls = !mLayerReference.DisplayHulls;

			// do a change option action
			ActionManager.Instance.doAction(new ChangeLayerOption(mLayerReference, oldLayerData, newLayerData));
		}

		private void LayerPanel_DoubleClick(object sender, EventArgs e)
		{
			// take the focus anyway if we click the panel
			Focus();
			// check the type of the layer for option edition
			if (GetType().Name == "LayerGridPanel")
			{
				LayerGridOptionForm optionForm = new LayerGridOptionForm(mLayerReference as LayerGrid);
				optionForm.ShowDialog();
			}
			else if (GetType().Name == "LayerBrickPanel")
			{
				LayerBrickOptionForm optionForm = new LayerBrickOptionForm(mLayerReference as LayerBrick);
				optionForm.ShowDialog();
			}
			else if (GetType().Name == "LayerTextPanel")
			{
				LayerTextOptionForm optionForm = new LayerTextOptionForm(mLayerReference as LayerText);
				optionForm.ShowDialog();
			}
			else if (GetType().Name == "LayerAreaPanel")
			{
				LayerAreaOptionForm optionForm = new LayerAreaOptionForm(mLayerReference as LayerArea);
				optionForm.ShowDialog();
			}
			else if (GetType().Name == "LayerRulerPanel")
			{
				LayerTextOptionForm optionForm = new LayerTextOptionForm(mLayerReference as LayerRuler);
				optionForm.ShowDialog();
			}
		}

		private void LayerPanel_Click(object sender, EventArgs e)
		{
			// take the focus anyway if we click the panel
			Focus();
			// and select this panel if not already done to avoid adding useless action in the stack
			if (Map.Instance.SelectedLayer != mLayerReference)
				ActionManager.Instance.doAction(new SelectLayer(mLayerReference));
		}
	}
}
