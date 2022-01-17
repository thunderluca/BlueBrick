using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BlueBrick
{
	public partial class LoadErrorForm : Form
	{
		public LoadErrorForm(string title, string message, string details)
		{
			InitializeComponent();
			// set the message and detail texts
			MessageTextBox.Text = message;
			DetailsTextBox.Text = details;
			// set the title
			Text = title;
			// hide the detail box by default
			showHideDetails(false);
		}

		private void DetailButton_Click(object sender, EventArgs e)
		{
			// revert the status based on the status of the detail box
			showHideDetails(!DetailsTextBox.Visible);
		}

		private void showHideDetails(bool isVisible)
		{
			// show or hide the slip container and the detail box
			splitContainer2.Panel2Collapsed = !isVisible;
			DetailsTextBox.Visible = isVisible;
			// if the split container is visible add some border
			// and change the text of the button
			if (isVisible)
			{
				splitContainer2.BorderStyle = BorderStyle.FixedSingle;
				DetailButton.Text = Properties.Resources.HideDetails;
			}
			else
			{
				splitContainer2.BorderStyle = BorderStyle.None;
				DetailButton.Text = Properties.Resources.ShowDetails;
			}
		}

		private void OkButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}