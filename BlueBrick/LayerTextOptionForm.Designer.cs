namespace BlueBrick
{
	partial class LayerTextOptionForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LayerTextOptionForm));
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.alphaTrackBar = new System.Windows.Forms.TrackBar();
			this.label3 = new System.Windows.Forms.Label();
			this.alphaNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.isVisibleCheckBox = new System.Windows.Forms.CheckBox();
			this.nameTextBox = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOk = new System.Windows.Forms.Button();
			this.groupBox3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.alphaTrackBar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.alphaNumericUpDown)).BeginInit();
			this.SuspendLayout();
			// 
			// groupBox3
			// 
			resources.ApplyResources(this.groupBox3, "groupBox3");
			this.groupBox3.Controls.Add(this.alphaTrackBar);
			this.groupBox3.Controls.Add(this.label3);
			this.groupBox3.Controls.Add(this.alphaNumericUpDown);
			this.groupBox3.Controls.Add(this.label1);
			this.groupBox3.Controls.Add(this.isVisibleCheckBox);
			this.groupBox3.Controls.Add(this.nameTextBox);
			this.groupBox3.Controls.Add(this.label2);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.TabStop = false;
			// 
			// alphaTrackBar
			// 
			resources.ApplyResources(this.alphaTrackBar, "alphaTrackBar");
			this.alphaTrackBar.Maximum = 100;
			this.alphaTrackBar.Name = "alphaTrackBar";
			this.alphaTrackBar.SmallChange = 10;
			this.alphaTrackBar.TickFrequency = 10;
			this.alphaTrackBar.Value = 50;
			this.alphaTrackBar.Scroll += new System.EventHandler(this.alphaTrackBar_Scroll);
			// 
			// label3
			// 
			resources.ApplyResources(this.label3, "label3");
			this.label3.Name = "label3";
			// 
			// alphaNumericUpDown
			// 
			resources.ApplyResources(this.alphaNumericUpDown, "alphaNumericUpDown");
			this.alphaNumericUpDown.Name = "alphaNumericUpDown";
			this.alphaNumericUpDown.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
			this.alphaNumericUpDown.ValueChanged += new System.EventHandler(this.alphaNumericUpDown_ValueChanged);
			this.alphaNumericUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.alphaNumericUpDown_KeyUp);
			// 
			// label1
			// 
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			// 
			// isVisibleCheckBox
			// 
			resources.ApplyResources(this.isVisibleCheckBox, "isVisibleCheckBox");
			this.isVisibleCheckBox.Name = "isVisibleCheckBox";
			this.isVisibleCheckBox.UseVisualStyleBackColor = true;
			// 
			// nameTextBox
			// 
			resources.ApplyResources(this.nameTextBox, "nameTextBox");
			this.nameTextBox.Name = "nameTextBox";
			// 
			// label2
			// 
			resources.ApplyResources(this.label2, "label2");
			this.label2.Name = "label2";
			// 
			// buttonCancel
			// 
			resources.ApplyResources(this.buttonCancel, "buttonCancel");
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonOk
			// 
			resources.ApplyResources(this.buttonOk, "buttonOk");
			this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.UseVisualStyleBackColor = true;
			this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
			// 
			// LayerTextOptionForm
			// 
			this.AcceptButton = this.buttonOk;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.buttonCancel;
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOk);
			this.Controls.Add(this.groupBox3);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LayerTextOptionForm";
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.alphaTrackBar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.alphaNumericUpDown)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.CheckBox isVisibleCheckBox;
		private System.Windows.Forms.TextBox nameTextBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOk;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.NumericUpDown alphaNumericUpDown;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TrackBar alphaTrackBar;
	}
}