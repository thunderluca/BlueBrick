namespace BlueBrick
{
	partial class PartListForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PartListForm));
			this.listView = new System.Windows.Forms.ListView();
			this.partColumnHeader = new System.Windows.Forms.ColumnHeader();
			this.quantiyColumnHeader = new System.Windows.Forms.ColumnHeader();
			this.colorColumnHeader = new System.Windows.Forms.ColumnHeader();
			this.descriptionColumnHeader = new System.Windows.Forms.ColumnHeader();
			this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.buttonClose = new System.Windows.Forms.Button();
			this.buttonExport = new System.Windows.Forms.Button();
			this.useGroupCheckBox = new System.Windows.Forms.CheckBox();
			this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.tableLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// listView
			// 
			this.listView.AllowColumnReorder = true;
			this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.partColumnHeader,
            this.quantiyColumnHeader,
            this.colorColumnHeader,
            this.descriptionColumnHeader});
			this.tableLayoutPanel.SetColumnSpan(this.listView, 3);
			resources.ApplyResources(this.listView, "listView");
			this.listView.FullRowSelect = true;
			this.listView.Name = "listView";
			this.listView.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.listView.UseCompatibleStateImageBehavior = false;
			this.listView.View = System.Windows.Forms.View.Details;
			this.listView.VisibleChanged += new System.EventHandler(this.listView_VisibleChanged);
			// 
			// partColumnHeader
			// 
			resources.ApplyResources(this.partColumnHeader, "partColumnHeader");
			// 
			// quantiyColumnHeader
			// 
			resources.ApplyResources(this.quantiyColumnHeader, "quantiyColumnHeader");
			// 
			// colorColumnHeader
			// 
			resources.ApplyResources(this.colorColumnHeader, "colorColumnHeader");
			// 
			// descriptionColumnHeader
			// 
			resources.ApplyResources(this.descriptionColumnHeader, "descriptionColumnHeader");
			// 
			// tableLayoutPanel
			// 
			resources.ApplyResources(this.tableLayoutPanel, "tableLayoutPanel");
			this.tableLayoutPanel.Controls.Add(this.buttonClose, 2, 1);
			this.tableLayoutPanel.Controls.Add(this.listView, 0, 0);
			this.tableLayoutPanel.Controls.Add(this.buttonExport, 0, 1);
			this.tableLayoutPanel.Controls.Add(this.useGroupCheckBox, 1, 1);
			this.tableLayoutPanel.Name = "tableLayoutPanel";
			// 
			// buttonClose
			// 
			this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
			resources.ApplyResources(this.buttonClose, "buttonClose");
			this.buttonClose.Name = "buttonClose";
			this.buttonClose.UseVisualStyleBackColor = true;
			this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
			// 
			// buttonExport
			// 
			resources.ApplyResources(this.buttonExport, "buttonExport");
			this.buttonExport.Name = "buttonExport";
			this.buttonExport.UseVisualStyleBackColor = true;
			this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
			// 
			// useGroupCheckBox
			// 
			resources.ApplyResources(this.useGroupCheckBox, "useGroupCheckBox");
			this.useGroupCheckBox.Checked = true;
			this.useGroupCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.useGroupCheckBox.Name = "useGroupCheckBox";
			this.useGroupCheckBox.UseVisualStyleBackColor = true;
			this.useGroupCheckBox.CheckedChanged += new System.EventHandler(this.useGroupCheckBox_CheckedChanged);
			// 
			// saveFileDialog
			// 
			this.saveFileDialog.DefaultExt = "txt";
			resources.ApplyResources(this.saveFileDialog, "saveFileDialog");
			this.saveFileDialog.SupportMultiDottedExtensions = true;
			// 
			// PartListForm
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tableLayoutPanel);
			this.DataBindings.Add(new System.Windows.Forms.Binding("Location", global::BlueBrick.Properties.Settings.Default, "PartListFormLocation", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.Location = global::BlueBrick.Properties.Settings.Default.PartListFormLocation;
			this.Name = "PartListForm";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PartListForm_FormClosing);
			this.tableLayoutPanel.ResumeLayout(false);
			this.tableLayoutPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView listView;
		private System.Windows.Forms.ColumnHeader partColumnHeader;
		private System.Windows.Forms.ColumnHeader descriptionColumnHeader;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
		private System.Windows.Forms.Button buttonExport;
		private System.Windows.Forms.ColumnHeader quantiyColumnHeader;
		private System.Windows.Forms.ColumnHeader colorColumnHeader;
		private System.Windows.Forms.Button buttonClose;
		private System.Windows.Forms.CheckBox useGroupCheckBox;
		private System.Windows.Forms.SaveFileDialog saveFileDialog;
	}
}