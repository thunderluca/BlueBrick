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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using BlueBrick.MapData;
using BlueBrick.Actions;
using BlueBrick.Actions.Layers;

namespace BlueBrick
{
	public partial class LayerGridOptionForm : Form
	{
		private readonly LayerGrid mEditedGridLayer = null;
		private Font mCurrentChosenFont = null;

		public LayerGridOptionForm(LayerGrid gridLayer)
		{
			InitializeComponent();
			// save the reference on the layer that we are editing
			mEditedGridLayer = gridLayer;
			// update the controls with the data of the gridLayer
			// name and visibility
			nameTextBox.Text = gridLayer.Name;
			isVisibleCheckBox.Checked = gridLayer.Visible;
			// transparency
			alphaNumericUpDown.Value = gridLayer.Transparency;
			alphaTrackBar.Value = gridLayer.Transparency;
			// grid
			gridCheckBox.Checked = gridLayer.DisplayGrid;
			gridSizeNumericUpDown.Value = gridLayer.GridSizeInStud;
			gridPixelNumericUpDown.Value = (int)gridLayer.GridThickness;
			gridColorPictureBox.BackColor = gridLayer.GridColor;
			// subgrid
			subGridCheckBox.Checked = gridLayer.DisplaySubGrid;
			subGridSizeNumericUpDown.Value = gridLayer.SubDivisionNumber;
			subGridPixelNumericUpDown.Value = (int)gridLayer.SubGridThickness;
			subGridColorPictureBox.BackColor = gridLayer.SubGridColor;
			// cell index
			cellIndexCheckBox.Checked = gridLayer.DisplayCellIndex;
			cellIndexColumnComboBox.SelectedIndex = (int)gridLayer.CellIndexColumnType;
			cellIndexRowComboBox.SelectedIndex = (int)gridLayer.CellIndexRowType;
			updateChosenFont(gridLayer.CellIndexFont);
			cellIndexColorPictureBox.BackColor = gridLayer.CellIndexColor;
			cellIndexOriginXNumericUpDown.Value = gridLayer.CellIndexCornerX;
			cellIndexOriginYNumericUpDown.Value = gridLayer.CellIndexCornerY;
		}

		private void buttonOk_Click(object sender, EventArgs e)
		{
			// create a copy of the edited layer to hold the old data
			LayerGrid oldLayerData = new LayerGrid();
			oldLayerData.CopyOptionsFrom(mEditedGridLayer);

            // create a new layer to store the new data
            LayerGrid newLayerData = new LayerGrid
            {

                // name and visibility
                Name = nameTextBox.Text,
                Visible = isVisibleCheckBox.Checked,
                //transparency
                Transparency = (int)alphaNumericUpDown.Value,
                // grid
                DisplayGrid = gridCheckBox.Checked,
                GridSizeInStud = (int)gridSizeNumericUpDown.Value,
                GridThickness = (float)gridPixelNumericUpDown.Value,
                GridColor = gridColorPictureBox.BackColor,
                // subgrid
                DisplaySubGrid = subGridCheckBox.Checked,
                SubDivisionNumber = (int)subGridSizeNumericUpDown.Value,
                SubGridThickness = (float)subGridPixelNumericUpDown.Value,
                SubGridColor = subGridColorPictureBox.BackColor,
                // cell index
                DisplayCellIndex = cellIndexCheckBox.Checked,
                CellIndexColumnType = (LayerGrid.CellIndexType)cellIndexColumnComboBox.SelectedIndex,
                CellIndexRowType = (LayerGrid.CellIndexType)cellIndexRowComboBox.SelectedIndex,
                CellIndexFont = mCurrentChosenFont,
                CellIndexColor = cellIndexColorPictureBox.BackColor,
                CellIndexCornerX = (int)cellIndexOriginXNumericUpDown.Value,
                CellIndexCornerY = (int)cellIndexOriginYNumericUpDown.Value
            };

            // do a change option action
            ActionManager.Instance.doAction(new ChangeLayerOption(mEditedGridLayer, oldLayerData, newLayerData));
		}
		private void alphaTrackBar_Scroll(object sender, EventArgs e)
		{
			alphaNumericUpDown.Value = alphaTrackBar.Value;
		}

		private void alphaNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			alphaTrackBar.Value = (int)alphaNumericUpDown.Value;
		}

		private void alphaNumericUpDown_KeyUp(object sender, KeyEventArgs e)
		{
			alphaNumericUpDown_ValueChanged(null, null);
		}

		private void updateChosenFont(Font newFont)
		{
			mCurrentChosenFont = newFont;
			cellIndexFontNameLabel.Text = mCurrentChosenFont.Name + " " + mCurrentChosenFont.SizeInPoints.ToString();
			cellIndexFontNameLabel.Font = mCurrentChosenFont;
		}

		private void gridColorPictureBox_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			colorDialog.Color = gridColorPictureBox.BackColor;
			// open the color box in modal
			DialogResult result = colorDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				gridColorPictureBox.BackColor = colorDialog.Color;
			}
		}

		private void subGridColorPictureBox_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			colorDialog.Color = subGridColorPictureBox.BackColor;
			// open the color box in modal
			DialogResult result = colorDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				subGridColorPictureBox.BackColor = colorDialog.Color;
			}
		}

		private void cellIndexColorPictureBox_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			colorDialog.Color = cellIndexColorPictureBox.BackColor;
			// open the color box in modal
			DialogResult result = colorDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				cellIndexColorPictureBox.BackColor = colorDialog.Color;
			}
		}

		private void buttonFont_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			fontDialog.Font = mCurrentChosenFont;
			// open the color box in modal
			DialogResult result = fontDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				updateChosenFont(fontDialog.Font);
			}
		}

		private void gridCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			bool enabled = gridCheckBox.Checked;
			// enable or disable all the grid line according to the state
			gridColorlabel.Enabled = enabled;
			gridColorPictureBox.Enabled = enabled;
			gridPixelNumericUpDown.Enabled = enabled;
			gridSizeNumericUpDown.Enabled = enabled;
			gridThicknessLabel.Enabled = enabled;
		}

		private void subGridCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			bool enabled = subGridCheckBox.Checked;
			// enable or disable all the sub grid line according to the state
			subGridColorlabel.Enabled = enabled;
			subGridColorPictureBox.Enabled = enabled;
			subGridPixelNumericUpDown.Enabled = enabled;
			subGridSizeNumericUpDown.Enabled = enabled;
			subGridThicknessLabel.Enabled = enabled;
		}

		private void cellIndexCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			bool enabled = cellIndexCheckBox.Checked;
			// enable or disable all the sub grid line according to the state
			cellIndexColorLabel.Enabled = enabled;
			cellIndexColorPictureBox.Enabled = enabled;
			cellIndexColumnComboBox.Enabled = enabled;
			cellIndexColumnLabel.Enabled = enabled;
			cellIndexFontButton.Enabled = enabled;
			cellIndexFontNameLabel.Enabled = enabled;
			cellIndexRowComboBox.Enabled = enabled;
			cellIndexRowLabel.Enabled = enabled;
			cellIndexOriginLabel.Enabled = enabled;
			cellIndexOriginXNumericUpDown.Enabled = enabled;
			cellIndexOriginYNumericUpDown.Enabled = enabled;
			cellIndexCommaLabel.Enabled = enabled;
			cellIndexOriginButton.Enabled = enabled;
		}

		private void cellIndexOriginButton_Click(object sender, EventArgs e)
		{
			PointF position = Map.Instance.GetMostTopLeftBrickPosition();
			int x = (int)(position.X / (int)gridSizeNumericUpDown.Value) - 1;
			int y = (int)(position.Y / (int)gridSizeNumericUpDown.Value) - 1;
			if (position.X < 0)
				x--;
			if (position.Y < 0)
				y--;
			// set the new values in the controls
			cellIndexOriginXNumericUpDown.Value = x;
			cellIndexOriginYNumericUpDown.Value = y;
		}
	}
}