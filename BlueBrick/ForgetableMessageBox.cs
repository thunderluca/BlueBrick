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

using System.Drawing;
using System.Windows.Forms;

namespace BlueBrick
{
	/// <summary>
	/// A simple implementation of a MessageBox with a checkbox to not display it again.
	/// </summary>
	public partial class ForgetableMessageBox : Form
	{
		#region the Show methods
		/// <summary>
		/// Use the same interface than the MessageBox. See the doc of the Message box.
		/// </summary>
		/// <returns>the dialog result as for the Message box</returns>
		public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons,
			MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, ref bool checkboxValue)
		{
            // create a message box
            ForgetableMessageBox messageBox = new ForgetableMessageBox
            {

                // set the parameters
                Text = caption
            };
            messageBox.messageLabel.Text = text;
			messageBox.setButtons(buttons);
			messageBox.setDefaultButtons(defaultButton);
			messageBox.setIcon(icon);
			messageBox.dontShowCheckBox.Checked = checkboxValue;

			// show the message box in modal state and get its result
			DialogResult result = messageBox.ShowDialog(owner);

			// put back the check state flag in the ref variable
			checkboxValue = messageBox.dontShowCheckBox.Checked;

			// and return the result
			return result;
		}
		#endregion

		public ForgetableMessageBox()
		{
			InitializeComponent();
		}

		#region set parameters
		private void setButtons(MessageBoxButtons buttons)
		{
			switch (buttons)
			{
				case MessageBoxButtons.AbortRetryIgnore:
					button1.Text = Properties.Resources.ErrorMsgAbortButton;
					button1.DialogResult = DialogResult.Abort;
					button2.Text = Properties.Resources.ErrorMsgRetryButton;
					button2.DialogResult = DialogResult.Retry;
					button3.Text = Properties.Resources.ErrorMsgIgnoreButton;
					button3.DialogResult = DialogResult.Ignore;
					AcceptButton = button3;
					CancelButton = button1;
					break;
				case MessageBoxButtons.OK:
					button1.Text = Properties.Resources.ErrorMsgOkButton;
					button1.DialogResult = DialogResult.OK;
					button2.Hide();
					button3.Hide();
					AcceptButton = button1;
					CancelButton = button1;
					break;
				case MessageBoxButtons.OKCancel:
					button1.Text = Properties.Resources.ErrorMsgOkButton;
					button1.DialogResult = DialogResult.OK;
					button2.Text = Properties.Resources.ErrorMsgCancelButton;
					button2.DialogResult = DialogResult.Cancel;
					button3.Hide();
					AcceptButton = button1;
					CancelButton = button2;
					break;
				case MessageBoxButtons.RetryCancel:
					button1.Text = Properties.Resources.ErrorMsgRetryButton;
					button1.DialogResult = DialogResult.Retry;
					button2.Text = Properties.Resources.ErrorMsgCancelButton;
					button2.DialogResult = DialogResult.Cancel;
					button3.Hide();
					AcceptButton = button1;
					CancelButton = button2;
					break;
				case MessageBoxButtons.YesNo:
					button1.Text = Properties.Resources.ErrorMsgYesButton;
					button1.DialogResult = DialogResult.Yes;
					button2.Text = Properties.Resources.ErrorMsgNoButton;
					button2.DialogResult = DialogResult.No;
					button3.Hide();
					AcceptButton = button1;
					CancelButton = button2;
					break;
				case MessageBoxButtons.YesNoCancel:
					button1.Text = Properties.Resources.ErrorMsgYesButton;
					button1.DialogResult = DialogResult.Yes;
					button2.Text = Properties.Resources.ErrorMsgNoButton;
					button2.DialogResult = DialogResult.No;
					button3.Text = Properties.Resources.ErrorMsgCancelButton;
					button3.DialogResult = DialogResult.Cancel;
					AcceptButton = button1;
					CancelButton = button3;
					break;
			}
		}

		private void setDefaultButtons(MessageBoxDefaultButton defaultButton)
		{
			switch (defaultButton)
			{
				case MessageBoxDefaultButton.Button1:
					button1.Focus();
					break;
				case MessageBoxDefaultButton.Button2:
					button2.Focus();
					break;
				case MessageBoxDefaultButton.Button3:
					button3.Focus();
					break;
			}
		}

		private void setIcon(MessageBoxIcon icon)
		{
			switch (icon)
			{
				case MessageBoxIcon.Information:
//				case MessageBoxIcon.Asterisk: // same as information
					iconPictureBox.Image = SystemIcons.Information.ToBitmap();
					break;
				case MessageBoxIcon.Error:
//				case MessageBoxIcon.Hand: // same as error
//				case MessageBoxIcon.Stop: // same as error
					iconPictureBox.Image = SystemIcons.Error.ToBitmap();
					break;
				case MessageBoxIcon.Exclamation:
//				case MessageBoxIcon.Warning: //same as exclamation
					iconPictureBox.Image = SystemIcons.Exclamation.ToBitmap();
					break;
				case MessageBoxIcon.Question:
					iconPictureBox.Image = SystemIcons.Question.ToBitmap();
					break;
				case MessageBoxIcon.None:
					iconAndMessageTableLayoutPanel.ColumnStyles[0].Width = 0;
					break;
			}
		}
		#endregion
	}
}
