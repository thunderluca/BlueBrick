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

namespace BlueBrick
{
	public partial class EditRulerForm : Form
	{
		// a clone of the ruler given in the constructor, that hold all the modification on the form is closed
		private readonly LayerRuler.RulerItem mEditedRulerClone = null;

		#region get/set
		public LayerRuler.RulerItem EditedRulerClone
		{
			get { return mEditedRulerClone; }
		}
		#endregion

		public EditRulerForm(LayerRuler.RulerItem rulerItem)
		{
			InitializeComponent();

			// clone the specified ruler in a new instance that will receive the edited properties
			mEditedRulerClone = rulerItem.Clone() as LayerRuler.RulerItem;

			// set the different control with the current state of the ruler
			// line appearance
			lineThicknessNumericUpDown.Value = (decimal)rulerItem.LineThickness;
			lineColorPictureBox.BackColor = rulerItem.Color;
			if (rulerItem is LayerRuler.LinearRuler)
				allowOffsetCheckBox.Checked = (rulerItem as LayerRuler.LinearRuler).AllowOffset;
			else
				allowOffsetCheckBox.Enabled = false;
			// guideline appearance
			dashPatternLineNumericUpDown.Value = (decimal)rulerItem.GuidelineDashPattern[0];
			dashPatternSpaceNumericUpDown.Value = (decimal)rulerItem.GuidelineDashPattern[1];
			guidelineThicknessNumericUpDown.Value = (decimal)rulerItem.GuidelineThickness;
			guidelineColorPictureBox.BackColor = rulerItem.GuidelineColor;
			// measure and unit
			displayUnitCheckBox.Checked = rulerItem.DisplayUnit;
			displayMeasureTextCheckBox.Checked = rulerItem.DisplayDistance;
			unitComboBox.SelectedIndex = (int)rulerItem.CurrentUnit;
			fontColorPictureBox.BackColor = rulerItem.MeasureColor;
			updateChosenFont(rulerItem.MeasureFont);
		}

		private void updateChosenFont(Font newFont)
		{
			fontNameLabel.ForeColor = fontColorPictureBox.BackColor;
			fontNameLabel.Text = newFont.Name + " " + newFont.SizeInPoints.ToString();
			fontNameLabel.Font = newFont;
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			// copy all the properties in the cloned rulers
			mEditedRulerClone.LineThickness = (float)lineThicknessNumericUpDown.Value;
			mEditedRulerClone.Color = lineColorPictureBox.BackColor;
			if (mEditedRulerClone is LayerRuler.LinearRuler)
				(mEditedRulerClone as LayerRuler.LinearRuler).AllowOffset = allowOffsetCheckBox.Checked;
			// guideline appearance
			mEditedRulerClone.GuidelineDashPattern = new float[]{(float)dashPatternLineNumericUpDown.Value, (float)dashPatternSpaceNumericUpDown.Value};
			mEditedRulerClone.GuidelineThickness = (float)guidelineThicknessNumericUpDown.Value;
			mEditedRulerClone.GuidelineColor = guidelineColorPictureBox.BackColor;
			// measure and unit
			mEditedRulerClone.DisplayUnit = displayUnitCheckBox.Checked;
			mEditedRulerClone.DisplayDistance = displayMeasureTextCheckBox.Checked;
			mEditedRulerClone.CurrentUnit = (MapData.Tools.Distance.Unit)unitComboBox.SelectedIndex;
			mEditedRulerClone.MeasureColor = fontColorPictureBox.BackColor;
			mEditedRulerClone.MeasureFont = fontNameLabel.Font;
		}

		private void displayMeasureTextCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			bool enabled = displayMeasureTextCheckBox.Checked;
			// change the unit properties
			displayUnitCheckBox.Enabled = enabled;
			unitLabel.Enabled = enabled;
			unitComboBox.Enabled = enabled;
			// change the font properties
			fontButton.Enabled = enabled;
			fontColorLabel.Enabled = enabled;
			fontColorPictureBox.Enabled = enabled;
			fontNameLabel.Enabled = enabled;
		}

		private void openColorDialogAndUpdatePictureBox(PictureBox pictureBox)
		{
			// set the color with the current back color of the picture box
			colorDialog.Color = pictureBox.BackColor;
			// open the color box in modal
			DialogResult result = colorDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				pictureBox.BackColor = colorDialog.Color;
			}
		}

		private void fontColorPictureBox_Click(object sender, EventArgs e)
		{
			openColorDialogAndUpdatePictureBox(fontColorPictureBox);
			// update alls the font color
			fontNameLabel.ForeColor = fontColorPictureBox.BackColor;
		}

		private void lineColorPictureBox_Click(object sender, EventArgs e)
		{
			openColorDialogAndUpdatePictureBox(lineColorPictureBox);
		}

		private void guidelineColorPictureBox_Click(object sender, EventArgs e)
		{
			openColorDialogAndUpdatePictureBox(guidelineColorPictureBox);
		}

		private void fontButton_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			fontDialog.Font = fontNameLabel.Font;
			// open the color box in modal
			DialogResult result = fontDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				updateChosenFont(fontDialog.Font);
			}
		}
	}
}
