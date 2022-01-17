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
	public partial class EditTextForm : Form
	{
		// the font edited for the text
		private Font mEditedFont = Properties.Settings.Default.DefaultTextFont;

		// we use a constant size of the font for the edition,
		// else the text is unreadable if you use a small font
		private const float FONT_SIZE_FOR_EDITION = 14;

		public Font EditedFont
		{
			get { return mEditedFont; }
		}

		public Color EditedColor
		{
			get { return fontColorPictureBox.BackColor; }
		}

		public StringAlignment EditedAlignment
		{
			get
			{
				if (textBox.TextAlign == HorizontalAlignment.Left)
					return StringAlignment.Near;
				else if (textBox.TextAlign == HorizontalAlignment.Center)
					return StringAlignment.Center;
				else
					return StringAlignment.Far;

			}
		}

		public string EditedText
		{
			get { return textBox.Text; }
		}

		public EditTextForm(LayerText.TextCell textCell)
		{
			InitializeComponent();

			if (textCell != null)
			{
				// text font
				mEditedFont = textCell.Font;
				// color
				changeColor(textCell.FontColor);
				// text alignement
				if (textCell.TextAlignment == StringAlignment.Near)
					alignLeftButton_Click(null, null);
				else if (textCell.TextAlignment == StringAlignment.Center)
					alignCenterButton_Click(null, null);
				else
					alignRightButton_Click(null, null);
				// the text itself
				textBox.Text = textCell.Text;
			}
			else
			{
				// text font
				mEditedFont = Properties.Settings.Default.DefaultTextFont;
				// color
				changeColor(Properties.Settings.Default.DefaultTextColor);
				// text alignement
				alignCenterButton_Click(null, null);
				// the text itself
				textBox.Text = Properties.Resources.TextEnterText;
				textBox.SelectAll();
			}

			// text box font
			labelSize.Text = mEditedFont.Size.ToString();
			textBox.Font = new Font(mEditedFont.FontFamily, FONT_SIZE_FOR_EDITION, mEditedFont.Style);
		}

		private void EditTextForm_Shown(object sender, EventArgs e)
		{
			// focus the text box such as the user can type the text immediately
			textBox.Focus();
		}

		private void alignLeftButton_Click(object sender, EventArgs e)
		{
			textBox.TextAlign = HorizontalAlignment.Left;
			alignLeftButton.FlatStyle = FlatStyle.Popup;
			alignCenterButton.FlatStyle = FlatStyle.Standard;
			alignRightButton.FlatStyle = FlatStyle.Standard;
		}

		private void alignCenterButton_Click(object sender, EventArgs e)
		{
			textBox.TextAlign = HorizontalAlignment.Center;
			alignLeftButton.FlatStyle = FlatStyle.Standard;
			alignCenterButton.FlatStyle = FlatStyle.Popup;
			alignRightButton.FlatStyle = FlatStyle.Standard;
		}

		private void alignRightButton_Click(object sender, EventArgs e)
		{
			textBox.TextAlign = HorizontalAlignment.Right;
			alignLeftButton.FlatStyle = FlatStyle.Standard;
			alignCenterButton.FlatStyle = FlatStyle.Standard;
			alignRightButton.FlatStyle = FlatStyle.Popup;
		}

		private void fontButton_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			fontDialog.Font = mEditedFont;
			// open the color box in modal
			DialogResult result = fontDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// save the edited font
				mEditedFont = fontDialog.Font;
				// and use the same in the edit box, except that we override the font size
				labelSize.Text = mEditedFont.Size.ToString();
				textBox.Font = new Font(mEditedFont.FontFamily, FONT_SIZE_FOR_EDITION, mEditedFont.Style);
			}
		}

		private void fontColorPictureBox_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			colorDialog.Color = fontColorPictureBox.BackColor;
			// open the color box in modal
			DialogResult result = colorDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				changeColor(colorDialog.Color);
			}
		}

		private void changeColor(Color newColor)
		{
			// set the specified color in the back color of the picture box
			fontColorPictureBox.BackColor = newColor;
		}

		private void textBox_TextChanged(object sender, EventArgs e)
		{
			okButton.Enabled = textBox.Text.Length > 0;
		}
	}
}