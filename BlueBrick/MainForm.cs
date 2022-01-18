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

using BlueBrick.Actions;
using BlueBrick.Actions.Bricks;
using BlueBrick.Actions.Maps;
using BlueBrick.Actions.Rulers;
using BlueBrick.Actions.Texts;
using BlueBrick.MapData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;

namespace BlueBrick
{
    public partial class MainForm : Form
    {
        #region variable
        // reference on the main form (set in the constructor)
        private static MainForm sInstance = null;

        // An array that contains all the language supported in the application
        public static readonly LanguageCodeAndName[] sLanguageCodeAndName = new[]
        {
            new LanguageCodeAndName("en", Properties.Resources.LanguageEnglish), // DEFAULT LANGUAGE SHOULD BE FIRST
			new LanguageCodeAndName("fr", Properties.Resources.LanguageFrench),
            new LanguageCodeAndName("de", Properties.Resources.LanguageGerman),
            new LanguageCodeAndName("nl", Properties.Resources.LanguageDutch),
            new LanguageCodeAndName("pt", Properties.Resources.LanguagePortuguese),
            new LanguageCodeAndName("es", Properties.Resources.LanguageSpanish),
            new LanguageCodeAndName("it", Properties.Resources.LanguageItalian),
            new LanguageCodeAndName("no", Properties.Resources.LanguageNorwegian),
            new LanguageCodeAndName("sv", Properties.Resources.LanguageSwedish)
			//new LanguageCodeAndName("cn", Properties.Resources.LanguageChinese) //not integrated yet
		};

        // a flag mostly never used, only when the application wants to restart, to prevent the user to
        // be able to cancel the close of the application and then finally end up with two instance of the application
        private bool mCanUserCancelTheApplicationClose = true;

        // custom cursors for the application
        private Cursor mHiddenLayerCursor = null;
        private Cursor mPanViewCursor = null;
        private Cursor mZoomCursor = null;
        private Cursor mPanOrZoomViewCursor = null;
        private Cursor mGridArrowCursor = null;
        private Cursor mBrickArrowCursor = null;
        private Cursor mFlexArrowCursor = null;
        private Cursor mBrickDuplicateCursor = null;
        private Cursor mBrickSelectionCursor = null;
        private Cursor mBrickSelectPathCursor = null;
        private Cursor mTextArrowCursor = null;
        private Cursor mTextDuplicateCursor = null;
        private Cursor mTextCreateCursor = null;
        private Cursor mTextSelectionCursor = null;
        private Cursor mAreaPaintCursor = null;
        private Cursor mAreaEraserCursor = null;
        private Cursor mRulerArrowCursor = null;
        private Cursor mRulerEditCursor = null;
        private Cursor mRulerDuplicateCursor = null;
        private Cursor mRulerSelectionCursor = null;
        private Cursor mRulerAddPoint1Cursor = null;
        private Cursor mRulerAddPoint2Cursor = null;
        private Cursor mRulerAddCircleCursor = null;
        private Cursor mRulerMovePointCursor = null;
        private Cursor mRulerScaleVerticalCursor = null;
        private Cursor mRulerScaleHorizontalCursor = null;
        private Cursor mRulerScaleDiagonalUpCursor = null;
        private Cursor mRulerScaleDiagonalDownCursor = null;

        // for shortcut key
        // var for updating the move
        private PointF mObjectTotalMove = new PointF(0, 0);
        private bool mIsLeftArrowDown = false;
        private bool mIsRightArrowDown = false;
        private bool mIsUpArrowDown = false;
        private bool mIsDownArrowDown = false;
        // var for updating the rotation
        private int mObjectTotalStepToRotate = 0;
        private bool mIsRotateLeftDown = false;
        private bool mIsRotateRightDown = false;
        // flags for the key events
        private Keys mLastModifierKeyDown = Keys.None;
        private bool mModifierWasPressedIgnoreCustomShortcut = false;

        // painting tool
        private Bitmap mPaintIcon = null; // the paint icon contains the color of the paint in the background
        private Color mCurrentPaintIconColor = Color.Empty;

        // for some strange reason, under Mono, the export form crash in the ctor when instanciated a second time.
        // so instanciate only one time and keep the instance
        private readonly ExportImageForm mExportImageForm = new ExportImageForm();

        private List<KeyAndPart>[] mShortcutKeys = null;
        #endregion

        #region get/set
        /// <summary>
        /// Get the reference pointer on the main form window
        /// </summary>
        public static MainForm Instance
        {
            get { return sInstance; }
        }

        /// <summary>
        /// Get the current scale of the map view.
        /// </summary>
        public double MapViewScale
        {
            get { return mapPanel.ViewScale; }
        }

        #region cursors
        #region cursors for all layers
        /// <summary>
        /// Get the cursor to display when the current layer is hidden
        /// </summary>
        public Cursor HiddenLayerCursor
        {
            get { return mHiddenLayerCursor; }
        }

        /// <summary>
        /// Get the cursor to display when no action is possible on the layer
        /// </summary>
        public Cursor NoCursor
        {
            get { return Cursors.No; }
        }

        /// <summary>
        /// Get the cursor for panning the view
        /// </summary>
        public Cursor PanViewCursor
        {
            get { return mPanViewCursor; }
        }

        /// <summary>
        /// Get the cursor for zooming the view
        /// </summary>
        public Cursor ZoomCursor
        {
            get { return mZoomCursor; }
        }

        /// <summary>
        /// Get the cursor for panning or zooming the view
        /// </summary>
        public Cursor PanOrZoomViewCursor
        {
            get { return mPanOrZoomViewCursor; }
        }
        #endregion

        #region grid cursors
        /// <summary>
        /// Get the default cursor for the grid layer
        /// </summary>
        public Cursor GridArrowCursor
        {
            get { return mGridArrowCursor; }
        }

        /// <summary>
        /// Get the cursor when moving the grid
        /// </summary>
        public Cursor GridMoveCursor
        {
            get { return Cursors.SizeAll; }
        }
        #endregion

        #region brick cursor
        /// <summary>
        /// Get the cursor for duplication of layer bricks
        /// </summary>
        public Cursor BrickArrowCursor
        {
            get { return mBrickArrowCursor; }
        }

        /// <summary>
        /// Get the cursor for duplication of layer bricks
        /// </summary>
        public Cursor FlexArrowCursor
        {
            get { return mFlexArrowCursor; }
        }

        /// <summary>
        /// Get the cursor for duplication of layer bricks
        /// </summary>
        public Cursor BrickDuplicateCursor
        {
            get { return mBrickDuplicateCursor; }
        }

        /// <summary>
        /// Get the cursor for selection of layer bricks
        /// </summary>
        public Cursor BrickSelectionCursor
        {
            get { return mBrickSelectionCursor; }
        }

        /// <summary>
        /// Get the cursor for selection of a path of connected bricks
        /// </summary>
        public Cursor BrickSelectPathCursor
        {
            get { return mBrickSelectPathCursor; }
        }

        /// <summary>
        /// Get the cursor when moving bricks
        /// </summary>
        public Cursor BrickMoveCursor
        {
            get { return Cursors.SizeAll; }
        }
        #endregion

        #region text cursor
        /// <summary>
        /// Get the default cursor for the text layer
        /// </summary>
        public Cursor TextArrowCursor
        {
            get { return mTextArrowCursor; }
        }

        /// <summary>
        /// Get the cursor for duplication of layer texts
        /// </summary>
        public Cursor TextDuplicateCursor
        {
            get { return mTextDuplicateCursor; }
        }

        /// <summary>
        /// Get the cursor for creation of a new text cell
        /// </summary>
        public Cursor TextCreateCursor
        {
            get { return mTextCreateCursor; }
        }

        /// <summary>
        /// Get the cursor for selection of layer texts
        /// </summary>
        public Cursor TextSelectionCursor
        {
            get { return mTextSelectionCursor; }
        }

        /// <summary>
        /// Get the cursor when moving texts
        /// </summary>
        public Cursor TextMoveCursor
        {
            get { return Cursors.SizeAll; }
        }
        #endregion

        #region area cursors
        /// <summary>
        /// Get the cursor for painting the area layer
        /// </summary>
        public Cursor AreaPaintCursor
        {
            get { return mAreaPaintCursor; }
        }

        /// <summary>
        /// Get the cursor for erasing the area layer
        /// </summary>
        public Cursor AreaEraserCursor
        {
            get { return mAreaEraserCursor; }
        }

        /// <summary>
        /// Get the cursor when moving areas
        /// </summary>
        public Cursor AreaMoveCursor
        {
            get { return Cursors.SizeAll; }
        }
        #endregion

        #region ruler cursors
        /// <summary>
        /// Get the cursor for selecting a ruler (the default cursor for a ruler layer)
        /// </summary>
        public Cursor RulerArrowCursor
        {
            get { return mRulerArrowCursor; }
        }

        /// <summary>
        /// Get the cursor for editing the properties of a ruler
        /// </summary>
        public Cursor RulerEditCursor
        {
            get { return mRulerEditCursor; }
        }

        /// <summary>
        /// Get the cursor for duplication of rulers
        /// </summary>
        public Cursor RulerDuplicateCursor
        {
            get { return mRulerDuplicateCursor; }
        }

        /// <summary>
        /// Get the cursor for multiple selection of rulers
        /// </summary>
        public Cursor RulerSelectionCursor
        {
            get { return mRulerSelectionCursor; }
        }

        /// <summary>
        /// Get the cursor for adding the first ruler point
        /// </summary>
        public Cursor RulerAddPoint1Cursor
        {
            get { return mRulerAddPoint1Cursor; }
        }

        /// <summary>
        /// Get the cursor for adding the second ruler point
        /// </summary>
        public Cursor RulerAddPoint2Cursor
        {
            get { return mRulerAddPoint2Cursor; }
        }

        /// <summary>
        /// Get the cursor for adding a circle ruler
        /// </summary>
        public Cursor RulerAddCircleCursor
        {
            get { return mRulerAddCircleCursor; }
        }

        /// <summary>
        /// Get the cursor for moving a linear ruler point
        /// </summary>
        public Cursor RulerMovePointCursor
        {
            get { return mRulerMovePointCursor; }
        }

        /// <summary>
        /// Get the cursor for scaling a ruler verticaly
        /// </summary>
        public Cursor RulerScaleVerticalCursor
        {
            get { return mRulerScaleVerticalCursor; }
        }

        /// <summary>
        /// Get the cursor for scaling a ruler horizontaly
        /// </summary>
        public Cursor RulerScaleHorizontalCursor
        {
            get { return mRulerScaleHorizontalCursor; }
        }

        /// <summary>
        /// Get the cursor for scaling toward north-east or south-west
        /// </summary>
        public Cursor RulerScaleDiagonalUpCursor
        {
            get { return mRulerScaleDiagonalUpCursor; }
        }

        /// <summary>
        /// Get the cursor for scaling north-west or south-east
        /// </summary>
        public Cursor RulerScaleDiagonalDownCursor
        {
            get { return mRulerScaleDiagonalDownCursor; }
        }

        /// <summary>
        /// Get the cursor when moving rulers
        /// </summary>
        public Cursor RulerMoveCursor
        {
            get { return Cursors.SizeAll; }
        }
        #endregion
        #endregion

        #region localized labels
        public string LabelAuthorLocalized
        {
            get { return labelAuthor.Text; }
        }
        public string LabelLUGLocalized
        {
            get { return labelLUG.Text; }
        }
        public string LabelEventLocalized
        {
            get { return labelEvent.Text; }
        }
        public string LabelDateLocalized
        {
            get { return labelDate.Text; }
        }
        public string LabelCommentLocalized
        {
            get { return labelComment.Text; }
        }
        #endregion

        #endregion

        #region Initialisation of the application

        public MainForm(string fileToOpen)
        {
            InitializeComponent();
            sInstance = this;

            // load the custom cursors and icons
            LoadEmbededCustomCursors();

            // reset the shortcut keys
            InitShortcutKeyArrayFromSettings();

            // PATCH FIX BECAUSE DOT NET FRAMEWORK IS BUGGED
            PreferencesForm.sSaveDefaultKeyInSettings();

            // load the part info
            LoadPartLibraryFromDisk();

            // PATCH FIX BECAUSE DOT NET FRAMEWORK IS BUGGED for mapping UI properties in settings
            // and do it after loading the library cause some UI settings concern the library
            LoadUISettingFromDefaultSettings();

            // disbale all the buttons of the toolbar and menu items by default
            // the open of the file or the creation of new map will enable the correct buttons
            EnableGroupingButton(false, false);
            EnablePasteButton(false);
            EnableToolbarButtonOnItemSelection(false);
            EnableToolbarButtonOnLayerSelection(false, false, false);

            // check if we need to open a budget at startup
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.BudgetFilenameToLoadAtStartup))
            {
                if (!OpenBudget(Properties.Settings.Default.BudgetFilenameToLoadAtStartup))
                {
                    // clear the settings if the budget is not valid, to avoid throwing the error message every time
                    Properties.Settings.Default.BudgetFilenameToLoadAtStartup = string.Empty;
                }
            }
            else
            {
                UpdateEnableStatusForBudgetMenuItem();
                PartUsageListView.updateBudgetNotification();
            }

            // check if we need to open a file or create a new map
            if (!string.IsNullOrWhiteSpace(fileToOpen) && CanOpenThisFile(fileToOpen))
            {
                OpenMap(fileToOpen);
                return;
            }

            CreateNewMap();

            // we update the list in the else because it is already updated in the openMap()
            UpdateRecentFileMenuFromConfigFile();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            // set the split container distance in the shown event because else the distance is not
            // correct if the window was maximise
            mainSplitContainer.SplitterDistance = Properties.Settings.Default.UIMainSplitContainerDistance;
            toolSplitContainer.SplitterDistance = Properties.Settings.Default.UIToolSplitContainerDistance;
        }

        /// <summary>
        /// A util function to fill a combobox with text that is read from a text file.
        /// The format of the text file is simple: every line in the text file will create an entry in the combo box
        /// This is used to fill the LUG and Event combo box
        /// </summary>
        /// <param name="comboBoxToFill">The combobox you want to fill</param>
        /// <param name="sourceDataFileName">The text file you want to read the data from</param>
        private void FillComboBoxFromTextFile(ComboBox comboBoxToFill, string sourceDataFileName)
        {
            try
            {
                var sourceDataFullFileName = Application.StartupPath + sourceDataFileName;

                comboBoxToFill.Items.Clear();
                comboBoxToFill.Sorted = true;

                using (var textReader = new StreamReader(sourceDataFullFileName))
                {
                    while (!textReader.EndOfStream)
                    {
                        comboBoxToFill.Items.Add(textReader.ReadLine());
                    }
                }
            }
            catch
            {
            }
        }

        private void LoadUISettingFromDefaultSettings()
        {
            // DOT NET BUG: the data binding of the Form size and window state interfere with the
            // the normal behavior of saving, so we remove the data binding and do it manually
            Location = Properties.Settings.Default.UIMainFormLocation;
            Size = Properties.Settings.Default.UIMainFormSize;
            WindowState = Properties.Settings.Default.UIMainFormWindowState;

            // part lib
            if (Properties.Settings.Default.UIFilterAllSentence != string.Empty)
            {
                RemoveInputFilterIndication();
                textBoxPartFilter.Text = Properties.Settings.Default.UIFilterAllSentence;
            }
            else if (string.IsNullOrWhiteSpace(textBoxPartFilter.Text))
            {
                // add the filter incitation message (which is saved in the ressource)
                // only if not another filter sentence was set during the loading
                AddInputFilterIndication();
            }

            // set the flag after the sentence, cause the checked event handler will check the text
            filterAllTabCheckBox.Checked = Properties.Settings.Default.UIFilterAllLibraryTab;

            // budget menu
            showOnlyBudgetedPartsToolStripMenuItem.Checked = Properties.Settings.Default.ShowOnlyBudgetedParts;
            showBudgetNumbersToolStripMenuItem.Checked = Properties.Settings.Default.ShowBudgetNumbers;
            useBudgetLimitationToolStripMenuItem.Checked = Properties.Settings.Default.UseBudgetLimitation;

            // snap grid button enable and size
            EnableSnapGridButton(Properties.Settings.Default.UISnapGridEnabled, Properties.Settings.Default.UISnapGridSize);

            // rotation step
            UpdateRotationStepButton(Properties.Settings.Default.UIRotationStep);

            // the zooming value
            mapPanel.ViewScale = Properties.Settings.Default.UIViewScale;

            // setting the correct Ruler tool
            switch (Properties.Settings.Default.UIRulerToolSelected)
            {
                case 0:
                    {
                        RulerSelectAndEditToolStripMenuItem_Click(this, null);
                        break;
                    }
                case 1:
                    {
                        RulerAddRulerToolStripMenuItem_Click(this, null);
                        break;
                    }
                case 2:
                    {
                        RulerAddCircleToolStripMenuItem_Click(this, null);
                        break;
                    }
            }

            // regenerate the paint icon with the right color in the background
            GeneratePaintIcon(Properties.Settings.Default.UIPaintColor);

            if (Properties.Settings.Default.UIIsEraserToolSelected)
            {
                PaintToolEraseToolStripMenuItem_Click(this, null);
            }
            else
            {
                PaintToolPaintToolStripMenuItem_Click(this, null);
            }

            // flag to split the part usage list (and force the check change in case it was not changed)
            SplitPartUsagePerLayerCheckBox.Checked = Properties.Settings.Default.UISplitPartUsagePerLayer;
            SplitPartUsagePerLayerCheckBox_CheckedChanged(this, null);
            IncludeHiddenLayerInPartListCheckBox.Checked = Properties.Settings.Default.UIIncludeHiddenPartsInPartUsage;
            IncludeHiddenLayerInPartListCheckBox_CheckedChanged(this, null);

            // toolbar and status bar visibility
            toolBar.Visible = toolbarMenuItem.Checked = Properties.Settings.Default.UIToolbarIsVisible;
            statusBar.Visible = statusBarMenuItem.Checked = Properties.Settings.Default.UIStatusbarIsVisible;
            mapPanel.CurrentStatusBarHeight = Properties.Settings.Default.UIStatusbarIsVisible ? statusBar.Height : 0;
            mapScrollBarsToolStripMenuItem.Checked = Properties.Settings.Default.UIMapScrollBarsAreVisible;
            mapPanel.ShowHideScrollBars(Properties.Settings.Default.UIMapScrollBarsAreVisible);
            watermarkToolStripMenuItem.Checked = Properties.Settings.Default.DisplayGeneralInfoWatermark;
            electricCircuitsMenuItem.Checked = Properties.Settings.Default.DisplayElectricCircuit;
            connectionPointsToolStripMenuItem.Checked = Properties.Settings.Default.DisplayFreeConnexionPoints;
            rulerAttachPointsToolStripMenuItem.Checked = Properties.Settings.Default.DisplayRulerAttachPoints;

            // the export window
            mExportImageForm.loadUISettingFromDefaultSettings();

            // fill the combo box in the Properties panel
            FillComboBoxFromTextFile(lugComboBox, @"/config/LugList.txt");
            FillComboBoxFromTextFile(eventComboBox, @"/config/EventList.txt");
        }

        private void SaveUISettingInDefaultSettings()
        {
            // DOT NET BUG: the data binding of the Form size and window state interfere with the
            // the normal behavior of saving, so we remove the data binding and do it manually

            // don't save the window state in minimized, else when you reopen the application
            // it only appears in the task bar
            Properties.Settings.Default.UIMainFormWindowState = WindowState == FormWindowState.Minimized ? FormWindowState.Normal : WindowState;

            // save the normal size or the restore one
            if (WindowState == FormWindowState.Normal)
            {
                // normal window size
                Properties.Settings.Default.UIMainFormLocation = Location;
                Properties.Settings.Default.UIMainFormSize = Size;
            }
            else
            {
                // save the restore window size
                Properties.Settings.Default.UIMainFormLocation = RestoreBounds.Location;
                Properties.Settings.Default.UIMainFormSize = RestoreBounds.Size;
            }

            // split container
            Properties.Settings.Default.UIMainSplitContainerDistance = mainSplitContainer.SplitterDistance;
            Properties.Settings.Default.UIToolSplitContainerDistance = toolSplitContainer.SplitterDistance;

            // snap grid size and rotation and the current zooming value (view scale)
            Properties.Settings.Default.UISnapGridEnabled = Layer.SnapGridEnabled;
            Properties.Settings.Default.UISnapGridSize = Layer.CurrentSnapGridSize;
            Properties.Settings.Default.UIRotationStep = Layer.CurrentRotationStep;
            Properties.Settings.Default.UIViewScale = mapPanel.ViewScale;

            // paint color
            Properties.Settings.Default.UIPaintColor = mCurrentPaintIconColor;
            Properties.Settings.Default.UIIsEraserToolSelected = LayerArea.IsCurrentToolTheEraser;

            // ruler tool selected
            Properties.Settings.Default.UIRulerToolSelected = (int)LayerRuler.CurrentEditTool;

            // flags for the part usage list
            Properties.Settings.Default.UISplitPartUsagePerLayer = SplitPartUsagePerLayerCheckBox.Checked;
            Properties.Settings.Default.UIIncludeHiddenPartsInPartUsage = IncludeHiddenLayerInPartListCheckBox.Checked;

            // toolbar and status bar visibility
            Properties.Settings.Default.UIToolbarIsVisible = toolBar.Visible;
            Properties.Settings.Default.UIStatusbarIsVisible = statusBar.Visible;
            Properties.Settings.Default.UIMapScrollBarsAreVisible = mapScrollBarsToolStripMenuItem.Checked;

            // the part lib display config
            SavePartLibUISettingInDefaultSettings();

            // the export window
            mExportImageForm.saveUISettingInDefaultSettings();

            // try to save (never mind if we can not (for example BlueBrick is launched
            // from a write protected drive)
            try
            {
                Properties.Settings.Default.Save();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Save all the UI setting related to the part library panel inside the default settings.
        /// This function is separated from the general save UI setting function, because it should
        /// be called also before reloading the library without closing the application
        /// </summary>
        private void SavePartLibUISettingInDefaultSettings()
        {
            PartsTabControl.savePartListDisplayStatusInSettings();
            Properties.Settings.Default.UIFilterAllLibraryTab = filterAllTabCheckBox.Checked;
        }

        /// <summary>
        /// Load the specified cursor from the specified assembly and return it
        /// </summary>
        /// <param name="assembly">the assembly from which loading the cursor</param>
        /// <param name="cursorResourceName">the resource name of the embeded cursor</param>
        /// <returns>a new created cursor</returns>
        private Cursor LoadEmbededCustomCursors(System.Reflection.Assembly assembly, string cursorResourceName)
        {
            // get the stream from the assembly and create the cursor giving the stream
            Cursor cursor;

            using (var stream = assembly.GetManifestResourceStream(cursorResourceName))
            {
                cursor = new Cursor(stream);
            }

            // return the created cursor
            return cursor;
        }

        /// <summary>
        /// Load and create all the embeded cursors creates specially for this application
        /// </summary>
        private void LoadEmbededCustomCursors()
        {
            // get the assembly
            System.Reflection.Assembly assembly = GetType().Assembly;

            // the load all the cursors
            mHiddenLayerCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.HiddenLayerCursor.cur");
            mPanViewCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.PanViewCursor.cur");
            mZoomCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.ZoomCursor.cur");
            mPanOrZoomViewCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.PanOrZoomViewCursor.cur");
            mGridArrowCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.GridArrowCursor.cur");
            mBrickArrowCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.BrickArrowCursor.cur");
            mFlexArrowCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.FlexArrowCursor.cur");
            mBrickDuplicateCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.BrickDuplicateCursor.cur");
            mBrickSelectionCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.BrickSelectionCursor.cur");
            mBrickSelectPathCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.BrickSelectPathCursor.cur");
            mTextArrowCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.TextArrowCursor.cur");
            mTextDuplicateCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.TextDuplicateCursor.cur");
            mTextCreateCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.TextCreateCursor.cur");
            mTextSelectionCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.TextSelectionCursor.cur");
            mAreaPaintCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.AreaPaintCursor.cur");
            mAreaEraserCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.AreaEraserCursor.cur");
            mRulerArrowCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.RulerArrowCursor.cur");
            mRulerEditCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.RulerEditCursor.cur");
            mRulerDuplicateCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.RulerDuplicateCursor.cur");
            mRulerSelectionCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.RulerSelectionCursor.cur");
            mRulerAddPoint1Cursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.RulerAddPoint1Cursor.cur");
            mRulerAddPoint2Cursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.RulerAddPoint2Cursor.cur");
            mRulerAddCircleCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.RulerAddCircleCursor.cur");
            mRulerMovePointCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.RulerMovePointCursor.cur");
            mRulerScaleVerticalCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.RulerScaleVerticalCursor.cur");
            mRulerScaleHorizontalCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.RulerScaleHorizontalCursor.cur");
            mRulerScaleDiagonalUpCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.RulerScaleDiagonalUpCursor.cur");
            mRulerScaleDiagonalDownCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.RulerScaleDiagonalDownCursor.cur");
        }

        /// <summary>
        /// Init and load the part library for both
        /// the part library database and the part library panel
        /// The part lib is supposed to have been cleared before, but if not we clear it anyway
        /// </summary>
        private void LoadPartLibraryFromDisk()
        {
            // first clear the database for precaution
            BrickLibrary.Instance.ClearAllData();

            // reload first the connection type info because we will need it for loading the part library
            BrickLibrary.Instance.LoadConnectionTypeInfo();

            // reload the color info, because the bricks also need the color name (for correctly setting up the color name in the bubble info)
            BrickLibrary.Instance.LoadColorInfo();

            // reinit the parts tab control (that will fill the brick library again)
            PartsTabControl.initPartsTabControl();

            // and relod the other data for the brick library (after the brick library is loaded)
            BrickLibrary.Instance.CreateEntriesForRenamedParts();
            BrickLibrary.Instance.LoadTrackDesignerRegistryFileList();
        }

        /// <summary>
        /// Tell if the specified file name has a valid extension that BlueBrick can open.
        /// </summary>
        /// <param name="filename">the filename to check</param>
        /// <returns>true if the extension of the file is well known</returns>
        private bool CanOpenThisFile(string filename)
        {
            // get the last dot
            var fileExtension = filename.Substring(filename.LastIndexOf('.') + 1).ToLower();

            // authorize the drop if it's a file with the good extension
            return fileExtension.Equals("bbm")
                || fileExtension.Equals("ldr")
                || fileExtension.Equals("ncp")
                || fileExtension.Equals("mpd")
                || fileExtension.Equals("tdl")
                || fileExtension.Equals("dat");
        }

        #endregion

        #region update function

        /// <summary>
        /// update all the view of the application
        /// </summary>
        public void UpdateView(Actions.Action.UpdateViewType mapUpdateType, Actions.Action.UpdateViewType layerUpdateType)
        {
            // update the map
            if (mapUpdateType != Actions.Action.UpdateViewType.NONE)
            {
                mapPanel.UpdateView();
            }

            // update the layer
            if (layerUpdateType != Actions.Action.UpdateViewType.NONE)
            {
                layerStackPanel.updateView(layerUpdateType);
            }

            // check if we need to change the "*" on the title bar
            UpdateTitleBar();

            // update the undo/redo stack
            UpdateUndoRedoMenuItems();
        }

        /// <summary>
        /// Update the title bar by displaying the application Name (BlueBrick) followed by the map file name with or without
        /// an asterix, and the budget file name with or without an asterix, like this:
        /// BlueBrick - Untitled.bbm (*) - MyBudget.bbb (*)
        /// </summary>
        public void UpdateTitleBar()
        {
            // use the file name of the map (not the full path)
            var fileInfo = new FileInfo(Map.Instance.MapFileName);

            var title = "BlueBrick - " + fileInfo.Name;
            if (Map.Instance.WasModified)
            {
                title += " *";
            }

            // Add the budget name if any is loaded
            if (Budget.Budget.Instance.IsExisting)
            {
                var budgetFileInfo = new FileInfo(Budget.Budget.Instance.BudgetFileName);
                title += " - " + budgetFileInfo.Name;
                if (Budget.Budget.Instance.WasModified)
                {
                    title += " *";
                }
            }

            // set the title bar text
            Text = title;
        }

        /// <summary>
        /// This function can be called when the current tab is changed, and we want to update the filter box
        /// with the current one saved in the tab
        /// </summary>
        /// <param name="filterSentence">the new filter sentence to set in the filter box</param>
        public void UpdateFilterComboBox(string filterSentence)
        {
            if (string.IsNullOrWhiteSpace(filterSentence))
            {
                AddInputFilterIndication();
            }
            else
            {
                RemoveInputFilterIndication();
                textBoxPartFilter.Text = filterSentence;
            }
        }

        /// <summary>
        /// Enable or disable the group/ungroup menu item in the edit menu and context menu
        /// </summary>
        /// <param name="canGroup">if true, the grouping buttons are enabled</param>
        /// <param name="canUngroup">if true the ungrouping buttons are enabled</param>
        public void EnableGroupingButton(bool canGroup, bool canUngroup)
        {
            groupToolStripMenuItem.Enabled = canGroup;
            ungroupToolStripMenuItem.Enabled = canUngroup;
            groupMenuToolStripMenuItem.Enabled = canGroup || canUngroup;
        }

        /// <summary>
        /// Enable or disable the paste item button depending on the parameter and also if there's something
        /// to paste. So if the parameter is true but nothing was copied, the paste button will stay disabled
        /// <param name="canPaste">if true that means you can enable the button is needed</param>
        /// </summary>
        public void EnablePasteButton(bool canPaste)
        {
            if (!canPaste)
            {
                pasteToolStripMenuItem.Enabled = false;
                toolBarPasteButton.Enabled = false;
                return;
            }

            var isThereAnyItemCopied = false;
            try
            {
                // if there's crazy stuff copied in the clipboard, this function may throw an exception
                // and this can happen at startup
                isThereAnyItemCopied = Clipboard.ContainsText();
            }
            catch
            {
                // do nothing, we probably cannot copy that data
            }
            pasteToolStripMenuItem.Enabled = isThereAnyItemCopied;
            toolBarPasteButton.Enabled = isThereAnyItemCopied;
        }

        /// <summary>
        /// Enable or disable the buttons in the tool bar that allow manipulation of the layer item
        /// such as cut/copy/delete and rotate and sned to back/bring to front button WHEN the item selection
        /// changed on the current layer.
        /// <param name="isThereAnyItemSelected">if true that means no item is selected on the current layer</param>
        /// </summary>
        public void EnableToolbarButtonOnItemSelection(bool isThereAnyItemSelected)
        {
            // enable/disable the copy button (toolbar and menu)
            toolBarCopyButton.Enabled = isThereAnyItemSelected;
            copyToolStripMenuItem.Enabled = isThereAnyItemSelected;

            // enable/disable the cut button (toolbar and menu)
            toolBarCutButton.Enabled = isThereAnyItemSelected;
            cutToolStripMenuItem.Enabled = isThereAnyItemSelected;

            // enable/disable the delete button (toolbar and menu)
            toolBarDeleteButton.Enabled = isThereAnyItemSelected;
            deleteToolStripMenuItem.Enabled = isThereAnyItemSelected;

            // enable/disable the rotate buttons (toolbar and menu)
            toolBarRotateCCWButton.Enabled = isThereAnyItemSelected;
            toolBarRotateCWButton.Enabled = isThereAnyItemSelected;
            rotateCCWToolStripMenuItem.Enabled = isThereAnyItemSelected;
            rotateCWToolStripMenuItem.Enabled = isThereAnyItemSelected;

            // enable/disable the send to back/bring to front buttons (toolbar and menu)
            toolBarBringToFrontButton.Enabled = isThereAnyItemSelected;
            toolBarSendToBackButton.Enabled = isThereAnyItemSelected;
            bringToFrontToolStripMenuItem.Enabled = isThereAnyItemSelected;
            sendToBackToolStripMenuItem.Enabled = isThereAnyItemSelected;

            // enable/disable the deselect all button (menu only)
            deselectAllToolStripMenuItem.Enabled = isThereAnyItemSelected;

            // enable/disable the select path button (menu only)
            bool selectedLayerIsBrick = (Map.Instance.SelectedLayer != null) && (Map.Instance.SelectedLayer is LayerBrick);
            selectPathToolStripMenuItem.Enabled = selectedLayerIsBrick && (Map.Instance.SelectedLayer.SelectedObjects.Count >= 2);

            // enable/disable the save of the selection to the library
            saveSelectionInLibraryToolStripMenuItem.Enabled = selectedLayerIsBrick && (Map.Instance.SelectedLayer.SelectedObjects.Count > 1);
        }

        /// <summary>
        /// Enable or disable the buttons in the tool bar that allow manipulation of the layer item
        /// such as snap grid, snap rotation, rotate button and paint/erase WHEN the selected layer is changed.
        /// This method do not affect the cut/copy/delete and rotate and send to back/bring to front button
        /// because these buttons depends on the items selected on the layer, not on the layer type.
        /// </summary>
        /// <param name="enableMoveRotateButton">if true, enable the button related to move and rotate</param>
        /// <param name="enablePaintButton">if true, show and enable the tools button related to paint</param>
        /// <param name="enableRulerButton">if true, show and enable the tools buttons related to ruler</param>
        public void EnableToolbarButtonOnLayerSelection(bool enableMoveRotateButton, bool enablePaintButton, bool enableRulerButton)
        {
            // enable the paste button if a layer with item has been selected (brick ot text layer)
            EnablePasteButton(enableMoveRotateButton);

            // enable/disable the sub menu item Transform (only menu)
            transformToolStripMenuItem.Enabled = enableMoveRotateButton;

            // enable/disable the snapping grid (toolbar and menu)
            toolBarSnapGridButton.Enabled = enableMoveRotateButton;
            moveStepToolStripMenuItem.Enabled = enableMoveRotateButton;
            moveStepDisabledToolStripMenuItem.Enabled = enableMoveRotateButton;
            moveStep32ToolStripMenuItem.Enabled = enableMoveRotateButton;
            moveStep16ToolStripMenuItem.Enabled = enableMoveRotateButton;
            moveStep8ToolStripMenuItem.Enabled = enableMoveRotateButton;
            moveStep4ToolStripMenuItem.Enabled = enableMoveRotateButton;
            moveStep2ToolStripMenuItem.Enabled = enableMoveRotateButton;
            moveStep1ToolStripMenuItem.Enabled = enableMoveRotateButton;
            moveStep05ToolStripMenuItem.Enabled = enableMoveRotateButton;

            // enable/disable the rotation step (toolbar and menu)
            toolBarRotationAngleButton.Enabled = enableMoveRotateButton;
            rotationStepToolStripMenuItem.Enabled = enableMoveRotateButton;
            rotationStep1ToolStripMenuItem.Enabled = enableMoveRotateButton;
            rotationStep22ToolStripMenuItem.Enabled = enableMoveRotateButton;
            rotationStep45ToolStripMenuItem.Enabled = enableMoveRotateButton;
            rotationStep90ToolStripMenuItem.Enabled = enableMoveRotateButton;

            // the toolbar is enabled either if there's a paint or ruler to enable
            toolBarToolButton.Enabled = enablePaintButton || enableRulerButton;

            // enable/disable the paint button in the menu
            paintToolToolStripMenuItem.Enabled = enablePaintButton;
            paintToolEraseToolStripMenuItem.Enabled = enablePaintButton;
            paintToolPaintToolStripMenuItem.Enabled = enablePaintButton;
            paintToolChooseColorToolStripMenuItem.Enabled = enablePaintButton;
            // show/hide the paint button in the toolbar
            paintToolStripMenuItem.Visible = enablePaintButton;
            eraseToolStripMenuItem.Visible = enablePaintButton;
            // adjust the image of the main button
            if (enablePaintButton)
            {
                if (LayerArea.IsCurrentToolTheEraser)
                    toolBarToolButton.Image = eraseToolStripMenuItem.Image;
                else
                    toolBarToolButton.Image = mPaintIcon;
            }

            // enable/disable the ruler buttons in the menu
            rulerToolToolStripMenuItem.Enabled = enableRulerButton;
            selectAndEditToolStripMenuItem.Enabled = enableRulerButton;
            addRulerToolStripMenuItem.Enabled = enableRulerButton;
            addCircleToolStripMenuItem.Enabled = enableRulerButton;
            // show/hide the ruler button in the toolbar
            rulerSelectAndEditToolStripMenuItem.Visible = enableRulerButton;
            rulerAddRulerToolStripMenuItem.Visible = enableRulerButton;
            rulerAddCircleToolStripMenuItem.Visible = enableRulerButton;
            // adjust the image of the main tool button
            if (enableRulerButton)
            {
                switch (LayerRuler.CurrentEditTool)
                {
                    case LayerRuler.EditTool.SELECT:
                        toolBarToolButton.Image = rulerSelectAndEditToolStripMenuItem.Image;
                        break;
                    case LayerRuler.EditTool.LINE:
                        toolBarToolButton.Image = rulerAddRulerToolStripMenuItem.Image;
                        break;
                    case LayerRuler.EditTool.CIRCLE:
                        toolBarToolButton.Image = rulerAddCircleToolStripMenuItem.Image;
                        break;
                }
            }
        }

        /// <summary>
        /// This private method is only to mutualize code between the menu and the toolbar,
        /// to enable/disable the snap grid button
        /// </summary>
        /// <param name="enable">true is the snap grid is enabled</param>
        /// <param name="size">the new size</param>
        private void EnableSnapGridButton(bool enable, float size)
        {
            // uncheck all the menu item
            moveStep32ToolStripMenuItem.Checked = false;
            moveStep16ToolStripMenuItem.Checked = false;
            moveStep8ToolStripMenuItem.Checked = false;
            moveStep4ToolStripMenuItem.Checked = false;
            moveStep2ToolStripMenuItem.Checked = false;
            moveStep1ToolStripMenuItem.Checked = false;
            moveStep05ToolStripMenuItem.Checked = false;
            // uncheck all the toolbar item
            toolBarGrid32Button.Checked = false;
            toolBarGrid16Button.Checked = false;
            toolBarGrid8Button.Checked = false;
            toolBarGrid4Button.Checked = false;
            toolBarGrid2Button.Checked = false;
            toolBarGrid1Button.Checked = false;
            toolBarGrid05Button.Checked = false;

            // enable or disable the correct items
            if (enable)
            {
                // menu
                moveStepDisabledToolStripMenuItem.Checked = false;

                switch (size)
                {
                    case 32.0f:
                        {
                            moveStep32ToolStripMenuItem.Checked = true;
                            toolBarGrid32Button.Checked = true;
                            break;
                        }
                    case 16.0f:
                        {
                            moveStep16ToolStripMenuItem.Checked = true;
                            toolBarGrid16Button.Checked = true;
                            break;
                        }
                    case 8.0f:
                        {
                            moveStep8ToolStripMenuItem.Checked = true;
                            toolBarGrid8Button.Checked = true;
                            break;
                        }
                    case 4.0f:
                        {
                            moveStep4ToolStripMenuItem.Checked = true;
                            toolBarGrid4Button.Checked = true;
                            break;
                        }
                    case 2.0f:
                        {
                            moveStep2ToolStripMenuItem.Checked = true;
                            toolBarGrid2Button.Checked = true;
                            break;
                        }
                    case 1.0f:
                        {
                            moveStep1ToolStripMenuItem.Checked = true;
                            toolBarGrid1Button.Checked = true;
                            break;
                        }
                    default:
                        {
                            moveStep05ToolStripMenuItem.Checked = true;
                            toolBarGrid05Button.Checked = true;
                            break;
                        }
                }

                // toolbar
                toolBarSnapGridButton.DropDown.Enabled = true;
                toolBarSnapGridButton.Image = Properties.Resources.SnapGridOn;
                Layer.SnapGridEnabled = true;
            }
            else
            {
                // menu
                moveStepDisabledToolStripMenuItem.Checked = true;
                // toolbar
                toolBarSnapGridButton.DropDown.Enabled = false;
                toolBarSnapGridButton.Image = Properties.Resources.SnapGridOff;
                Layer.SnapGridEnabled = false;
            }

            // set the size
            Layer.CurrentSnapGridSize = size;
        }

        /// <summary>
        /// Update the General info UI controls in the property tab from the currently loaded map.
        /// This function is called programatically to update the UI when the program need to, so the event handler
        /// of the UI element are disabled during this function to avoid having another change map info action created.
        /// <param name="doesNeedToUpdateDimension"/>If true, the dimension of the full map will also be updated, otherwise they are left as they are</param>
        /// </summary>
        public void UpdateMapGeneralInfo(bool doesNeedToUpdateDimension)
        {
            // disable the UI event handlers
            AuthorTextBox.Leave -= AuthorTextBox_Leave;
            lugComboBox.Leave -= LugComboBox_Leave;
            eventComboBox.Leave -= EventComboBox_Leave;
            dateTimePicker.ValueChanged -= DateTimePicker_ValueChanged;
            commentTextBox.Leave -= CommentTextBox_Leave;

            // update the back color of the background color button
            DocumentDataPropertiesMapBackgroundColorButton.BackColor = Map.Instance.BackgroundColor;
            // fill the text controls
            AuthorTextBox.Text = Map.Instance.Author;
            lugComboBox.Text = Map.Instance.LUG;
            eventComboBox.Text = Map.Instance.Event;
            dateTimePicker.Value = Map.Instance.Date;

            commentTextBox.Lines = Map.Instance.Comment.Split('\n');

            // update also the map dimensions if needed
            if (doesNeedToUpdateDimension)
            {
                UpdateMapDimensionInfo();
            }

            // re-enable the UI event handlers
            AuthorTextBox.Leave += AuthorTextBox_Leave;
            lugComboBox.Leave += LugComboBox_Leave;
            eventComboBox.Leave += EventComboBox_Leave;
            dateTimePicker.ValueChanged += DateTimePicker_ValueChanged;
            commentTextBox.Leave += CommentTextBox_Leave;
        }

        /// <summary>
        /// This function recompute the overall size of the map, and display it on the properties tab
        /// </summary>
        private void UpdateMapDimensionInfo()
        {
            // warn the map Panel that the map dimension may have changed
            mapPanel.MapAreaChangedNotification();

            // ignore if the properties tab is not visible
            if (DocumentDataTabControl.SelectedTab != DocumentDataPropertiesTabPage)
            {
                return;
            }

            // compute the size
            var totalArea = Map.Instance.GetTotalAreaInStud(true);
            MapData.Tools.Distance width = new MapData.Tools.Distance(totalArea.Width, MapData.Tools.Distance.Unit.STUD);
            MapData.Tools.Distance height = new MapData.Tools.Distance(totalArea.Height, MapData.Tools.Distance.Unit.STUD);

            labelWidthModule.Text = Math.Ceiling(width.DistanceInModule).ToString();
            labelHeightModule.Text = Math.Ceiling(height.DistanceInModule).ToString();

            labelWidthStud.Text = Math.Round(width.DistanceInStud).ToString();
            labelHeightStud.Text = Math.Round(height.DistanceInStud).ToString();

            labelWidthMeter.Text = width.DistanceInMeter.ToString("N2");
            labelHeightMeter.Text = height.DistanceInMeter.ToString("N2");

            labelWidthFeet.Text = width.DistanceInFeet.ToString("N2");
            labelHeightFeet.Text = height.DistanceInFeet.ToString("N2");
        }

        public void NotifyPartListForLayerAdded(Layer layer)
        {
            if (!(layer is LayerBrick layerBrick))
            {
                return;
            }

            PartUsageListView.addLayerNotification(layerBrick);
        }

        public void NotifyPartListForLayerRemoved(Layer layer)
        {
            if (!(layer is LayerBrick layerBrick))
            {
                return;
            }

            PartUsageListView.removeLayerNotification(layerBrick);
        }

        public void NotifyPartListForLayerRenamed(Layer layer)
        {
            if (!(layer is LayerBrick layerBrick))
            {
                return;
            }

            PartUsageListView.renameLayerNotification(layerBrick);
        }

        public void NotifyPartListForBrickAdded(LayerBrick layer, Layer.LayerItem brickOrGroup, bool isCausedByRegroup)
        {
            // inform the budget first, because the part usage also display the budget
            Budget.Budget.Instance.addBrickNotification(layer, brickOrGroup, isCausedByRegroup);
            PartUsageListView.addBrickNotification(layer, brickOrGroup, isCausedByRegroup);
            PartsTabControl.updatePartCountAndBudget(brickOrGroup);
            // update the map dimensions
            UpdateMapDimensionInfo();
        }

        public void NotifyPartListForBrickRemoved(LayerBrick layer, Layer.LayerItem brickOrGroup, bool isCausedByUngroup)
        {
            // inform the budget first, because the part usage also display the budget
            Budget.Budget.Instance.removeBrickNotification(layer, brickOrGroup, isCausedByUngroup);
            PartUsageListView.removeBrickNotification(layer, brickOrGroup, isCausedByUngroup);
            PartsTabControl.updatePartCountAndBudget(brickOrGroup);
            // update the map dimensions
            UpdateMapDimensionInfo();
        }

        public void NotifyForPartMoved()
        {
            // update the map dimensions
            UpdateMapDimensionInfo();
        }

        public void NotifyForLayerVisibilityChangedOrLayerDeletion()
        {
            // update the map dimensions
            UpdateMapDimensionInfo();
        }

        public void NotifyForMapBackgroundColorChanged()
        {
            // update the back color of the background color button
            DocumentDataPropertiesMapBackgroundColorButton.BackColor = Map.Instance.BackgroundColor;
        }
        public void NotifyForBudgetChanged(string partId)
        {
            PartUsageListView.updateBudgetNotification(partId);
        }
        #endregion

        #region status bar

        public void SetStatusBarMessage(string message)
        {
            // escape the ampersome character
            statusBarLabel.Text = message.Replace("&", "&&");
        }

        public void ResetProgressBar(int maxValue)
        {
            try
            {
                if (maxValue <= 0)
                {
                    return;
                }

                statusBarProgressBar.Step = 1;
                statusBarProgressBar.Minimum = 0;
                statusBarProgressBar.Maximum = maxValue;
                statusBarProgressBar.Value = 0;
                statusBarProgressBar.Visible = true;
            }
            catch
            {
                // ignore if if the progressbar is already dead
            }
        }

        public void StepProgressBar()
        {
            try
            {
                // perform the step
                statusBarProgressBar.PerformStep();

                // hide automatically the progress bar when the end is reached
                if (statusBarProgressBar.Value >= statusBarProgressBar.Maximum)
                {
                    statusBarProgressBar.Visible = false;
                }
            }
            catch
            {
                // ignore if if the progressbar is already dead
            }
        }

        public void StepProgressBar(int stepValue)
        {
            try
            {
                if (stepValue <= 0)
                {
                    return;
                }

                statusBarProgressBar.Step = stepValue;
                StepProgressBar();
            }
            catch
            {
                // ignore if if the progressbar is already dead
            }
        }

        public void FinishProgressBar()
        {
            try
            {
                if (statusBarProgressBar.Value >= statusBarProgressBar.Maximum)
                {
                    return;
                }

                statusBarProgressBar.Step = statusBarProgressBar.Maximum - statusBarProgressBar.Value;
                StepProgressBar();
            }
            catch
            {
                // ignore if if the progressbar is already dead
            }
        }
        #endregion

        #region event handler for menu bar

        #region File menu
        /// <summary>
        /// This method check if the current map is not save and prompt a message box asking
        /// what to do (save, do not save or cancel). This method sould be called before
        /// exiting the application, and before creating a new map
        /// <returns>true if the action can continue, false if user choose to cancel</returns>
        /// </summary>
        private bool CheckForUnsavedMap()
        {
            // the map was not modified, the action can continue
            if (!Map.Instance.WasModified)
            {
                return true;
            }

            // if the user can cancel the application close, give him 3 buttons yes/no/cancel,
            // else give him only 2 buttons yes/no:
            var result = MessageBox.Show(
                this,
                Properties.Resources.ErrorMsgMapWasModified,
                Properties.Resources.ErrorMsgTitleWarning,
                mCanUserCancelTheApplicationClose ? MessageBoxButtons.YesNoCancel : MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1
            );

            switch (result)
            {
                case DialogResult.Yes:
                    {
                        // call the save method (that maybe will perform a save as)
                        // if the save failed for whatever reason, return true to cancel the continuation
                        // and give a second chance to the user to save its file or just to quit without saving
                        if (!SaveMap())
                        {
                            return false;
                        }
                        break;
                    }
                case DialogResult.Cancel:
                    {
                        // user cancel so return false
                        return false;
                    }
            }

            // user choose "yes", the action can continue
            return true;
        }

        private void ReinitializeCurrentMap()
        {
            // create a new map to trash the previous one
            Map.Instance = new Map();
            Layer.ResetNameInstanceCounter();
            ActionManager.Instance.clearStacks();
            // reset the modified flag
            Map.Instance.WasModified = false;
            // reset the current file name
            ChangeCurrentMapFileName(Properties.Resources.DefaultSaveFileName, false);
            // update the view any way
            UpdateView(Actions.Action.UpdateViewType.FULL, Actions.Action.UpdateViewType.FULL);
            Budget.Budget.Instance.recountAllBricks();
            PartUsageListView.rebuildList();
            PartsTabControl.updateAllPartCountAndBudget();
            // update the properties
            UpdateMapGeneralInfo(true);
            // force a garbage collect because we just trashed the previous map
            GC.Collect();
        }

        private void CreateNewMap()
        {
            // trash the previous map
            ReinitializeCurrentMap();

            // check the name of the template file to load when creating a new map, and load it if it is valid
            var templateFileToOpen = Properties.Settings.Default.TemplateFilenameWhenCreatingANewMap;
            if (!string.IsNullOrWhiteSpace(templateFileToOpen) && File.Exists(templateFileToOpen) && CanOpenThisFile(templateFileToOpen))
            {
                // the template file seems valid, so open it
                OpenMap(templateFileToOpen, true);
            }
            else
            {
                // no valid template file, create a default map with default settings
                ActionManager.Instance.doAction(new AddLayer(nameof(LayerGrid), false));
                ActionManager.Instance.doAction(new AddLayer(nameof(LayerBrick), false));
            }

            // after adding the two default layer, we reset the WasModified flag of the map
            // (and before the update of the title bar)
            Map.Instance.WasModified = false;
            // update the view any way
            UpdateView(Actions.Action.UpdateViewType.FULL, Actions.Action.UpdateViewType.FULL);
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // check if the current map is not save and display a warning message
            if (CheckForUnsavedMap())
            {
                CreateNewMap();
            }
        }

        private void ChangeCurrentMapFileName(string filename, bool isAValidName)
        {
            // save the filename
            Map.Instance.MapFileName = filename;
            Map.Instance.IsMapNameValid = isAValidName;
            // update the name of the title bar with the name of the file
            UpdateTitleBar();
        }

        private void OpenMap(string filename, bool isTemplateFile = false)
        {
            // set the wait cursor
            Cursor = Cursors.WaitCursor;

            // reset the action stacks and layer counter
            Layer.ResetNameInstanceCounter();
            ActionManager.Instance.clearStacks();
            BrickLibrary.Instance.WereUnknownBricksAdded = false;

            // load the file
            var isFileValid = SaveLoadManager.load(filename);
            if (isFileValid)
            {
                // move the view to center of the map
                mapPanel.MoveViewToMapCenter();
                // update the view
                UpdateView(Actions.Action.UpdateViewType.FULL, Actions.Action.UpdateViewType.FULL);
                Budget.Budget.Instance.recountAllBricks();
                PartUsageListView.rebuildList();
                PartsTabControl.updateAllPartCountAndBudget();
                // update the properties
                UpdateMapGeneralInfo(true);
                //check if some parts were missing in the library for displaying a warning message
                if (BrickLibrary.Instance.WereUnknownBricksAdded)
                {
                    // restore the cursor before displaying the error message box
                    Cursor = Cursors.Default;
                    string message = Properties.Resources.ErrorMsgMissingParts.Replace("&", Map.Instance.Author);
                    MessageBox.Show(this, message,
                        Properties.Resources.ErrorMsgTitleWarning, MessageBoxButtons.OK,
                        MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                }
            }
            else
            {
                // call the finish progress bar to hide it, in case we had a error while loading the file
                FinishProgressBar();
            }

            // restore the cursor after loading
            Cursor = Cursors.Default;

            // save the current file name of the loaded map
            if (isFileValid && !isTemplateFile)
            {
                ChangeCurrentMapFileName(filename, true);
            }
            else
            {
                ChangeCurrentMapFileName(Properties.Resources.DefaultSaveFileName, false);
            }

            // update the recent file list
            if (!isTemplateFile)
            {
                UpdateRecentFileMenuFromConfigFile(filename, isFileValid);
            }

            // force a garbage collect because we just trashed the previous map
            GC.Collect();
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // check if the current map is not save and display a warning message
            if (CheckForUnsavedMap())
            {
                return;
            }

            var result = openFileDialog.ShowDialog();
            if (result != DialogResult.OK)
            {
                return;
            }

            OpenMap(openFileDialog.FileName);
        }

        private void OpenRecentFileMenuItem_Click(object sender, EventArgs e)
        {
            // check if the current map is not save and display a warning message
            if (!CheckForUnsavedMap() || !(sender is ToolStripMenuItem menuItem))
            {
                return;
            }

            OpenMap(menuItem.Tag as string);
        }

        private void UpdateRecentFileMenuFromConfigFile(string fileName, bool isAdded)
        {
            // get a reference on the list
            // and remove the file from this list since it will be re-added (or not) on top of the list
            var recentFiles = Properties.Settings.Default.RecentFiles
                .Cast<string>()
                .Where(fn => !string.Equals(fn, fileName, StringComparison.InvariantCultureIgnoreCase))
                .ToList();

            // add the filename on top of the list (according to the parameter)
            if (isAdded)
            {
                recentFiles.Insert(0, fileName);
            }

            // if the maximum files is reached, we delete the last one (the old one)
            if (recentFiles.Count > 20)
            {
                recentFiles.RemoveAt(20);
            }

            // In order to save the change we need to recreate the list due to a Mono BUG:
            // if you just modify the list in the default settings, Mono didn't detect it and think no modification
            // was made, therefore the Save() method does nothing (don't save the modified list)
            // Moreover, if you just try to assign the recentFile list to Default.RecentFiles an exception is raised
            // because I guess it references the same list.
            // Moreover the Specialized.StringCollection class do not have a constructor to clone the list.
            // Therefore the only way is to recreate an empty list and fill it again. While doing it I clean it
            // a bit by removing the empty string (because the list is initiallized with empty string)
            Properties.Settings.Default.RecentFiles = new System.Collections.Specialized.StringCollection();

            for (var i = 0; i < recentFiles.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(recentFiles[i]))
                {
                    Properties.Settings.Default.RecentFiles.Add(recentFiles[i]);
                }
            }

            // save the change
            Properties.Settings.Default.Save();

            // then update the menu item list
            UpdateRecentFileMenuFromConfigFile();
        }

        private void UpdateRecentFileMenuFromConfigFile()
        {
            // clear the list of recent files
            openRecentToolStripMenuItem.DropDownItems.Clear();

            // if the list is not empty, populate the menu
            for (var i = 0; i < Properties.Settings.Default.RecentFiles.Count; i++)
            {
                // check if the filename is valid
                var fileName = Properties.Settings.Default.RecentFiles[i];
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    var item = new ToolStripMenuItem
                    {
                        Tag = fileName,
                        Text = Path.GetFileName(fileName),
                        ToolTipText = fileName // to display the full path as tooltip
                    };
                    item.Click += new EventHandler(OpenRecentFileMenuItem_Click);
                    openRecentToolStripMenuItem.DropDownItems.Add(item);
                }

                // stop adding files if we reach the maximum number to display
                if (openRecentToolStripMenuItem.DropDownItems.Count >= Properties.Settings.Default.MaxRecentFilesNum)
                {
                    break;
                }
            }

            // enable the Open Recent menu item if we have some recent files
            openRecentToolStripMenuItem.Enabled = openRecentToolStripMenuItem.DropDownItems.Count > 0;
        }

        /// <summary>
        /// Save the map that we know as a specified name. If you're not sure if the map as a name, call the saveMap() method instead.
        /// </summary>
        /// <returns>true if the map was correctly saved</returns>
        private bool SaveNamedMap()
        {
            // set the wait cursor
            Cursor = Cursors.WaitCursor;

            // save the file
            var saveDone = SaveLoadManager.save(Map.Instance.MapFileName);
            if (saveDone)
            {
                // after saving the map in any kind of format, we reset the WasModified flag of the map
                // (and before the update of the title bar)
                Map.Instance.WasModified = false;
                // update the title bar to remove the asterix
                UpdateTitleBar();
            }

            // restore the cursor
            Cursor = Cursors.Default;

            // return the save status
            return saveDone;
        }

        /// <summary>
        /// Save the current map with its current name. If the map is untitle (unnamed) then the saveMapAs() method will be called.
        /// </summary>
        /// <returns>true if the map was correctly saved</returns>
        private bool SaveMap()
        {
            // a flag to know if the save was correctly done or not
            // if the current file name is not defined we do a "save as..."
            return Map.Instance.IsMapNameValid ? SaveNamedMap() : SaveMapAs();
        }

        /// <summary>
        /// Save the current map under a different name (no matter if the current map has a name or not).
        /// This will pop out the save as dialog, and may also pop out a warning message if the user choose a not
        /// complete file format for the save.
        /// </summary>
        /// <returns>true if the map was correctly saved</returns>
        private bool SaveMapAs()
        {
            // put the current file name in the dialog (which can be the default one)
            // but remove the extension, such as the user can easily change the extension in
            // the save dialog drop list, and the save dialog will add it automatically
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(Map.Instance.MapFileName);
            // if there's no initial directory, choose the My Documents directory
            saveFileDialog.InitialDirectory = Path.GetDirectoryName(Map.Instance.MapFileName);
            if (saveFileDialog.InitialDirectory != null && saveFileDialog.InitialDirectory.Length == 0)
            {
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            // open the save as dialog
            var result = saveFileDialog.ShowDialog();
            if (result != DialogResult.OK)
            {
                return false;
            }

            // for a "Save As..." only (not for a save), we check if the user choose a LDRAW or TDL format
            // to display a warning message, that he will lost data
            string filenameLower = saveFileDialog.FileName.ToLower();
            if (Properties.Settings.Default.DisplayWarningMessageForNotSavingInBBM && !filenameLower.EndsWith("bbm"))
            {
                // use a local variable to get the value of the checkbox, by default we don't suggest the user to hide it
                var dontDisplayMessageAgain = false;

                // display the warning message
                result = ForgetableMessageBox.Show(
                    this,
                    Properties.Resources.ErrorMsgNotSavingInBBM,
                    Properties.Resources.ErrorMsgTitleWarning,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1,
                    ref dontDisplayMessageAgain
                );

                // set back the checkbox value in the settings (don't save the settings now, it will be done when exiting the application)
                Properties.Settings.Default.DisplayWarningMessageForNotSavingInBBM = !dontDisplayMessageAgain;

                // if the user doesn't want to continue, do not save and
                // do not add the name in the recent list file
                if (result == DialogResult.No)
                {
                    return false;
                }
            }

            // change the current file name before calling the save
            ChangeCurrentMapFileName(saveFileDialog.FileName, true);

            // save the map
            var saveDone = SaveNamedMap();
            // update the recent file list with the new file saved (if correctly saved)
            if (saveDone)
            {
                UpdateRecentFileMenuFromConfigFile(saveFileDialog.FileName, true);
            }

            // return the flag to know if the save was done
            return saveDone;
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveMap();
        }

        private void SaveasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveMapAs();
        }

        private void ExportAsPictureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // open the export form
            mExportImageForm.init();

            var result = mExportImageForm.ShowDialog();
            if (result != DialogResult.OK)
            {
                return;
            }

            // if some export options (at least the first one) were validated, we need to update the view
            // to set the little "*" after the name of the file in the title bar, because the export options
            // have been saved in the map, therefore the map was modified.
            UpdateTitleBar();
        }

        private void ExportPartListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // open the save file dialog
            var result = exportPartListFileDialog.ShowDialog();
            if (result != DialogResult.OK)
            {
                return;
            }

            PartUsageListView.export(exportPartListFileDialog.FileName);
        }

        private void ReloadPartLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // try to unload the part library, if sucessful continue
            if (!UnloadPartLibrary(out string mapFileNameToReload))
            {
                return;
            }

            // then display a waiting message box, giving the user the oppotunity to change the data before reloading
            MessageBox.Show(
                this,
                Properties.Resources.ErrorMsgReadyToReloadPartLib,
                Properties.Resources.ErrorMsgTitleWarning,
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button1
            );

            // reload the part library when the user close the message box
            ReloadPartLibrary(mapFileNameToReload);
        }

        private bool UnloadPartLibrary(out string mapFileNameToReload)
        {
            // by default no open file
            mapFileNameToReload = null;

            // we have first to undload the current file
            // check if the current map is not save and display a warning message
            // we also need to check for unsave budget because we will destroy the library
            if (!CheckForUnsavedMap() || !CheckForUnsavedBudget())
            {
                // part lib was not unloaded
                return false;
            }

            // save the name of the current map open to reload it (if it is valid)
            if (Map.Instance.IsMapNameValid)
            {
                mapFileNameToReload = Map.Instance.MapFileName;
            }

            // save the UI settings of the part lib before reloading it (and before clearing it), because the user may
            // have change some UI setting after the startup of the application, and now he wants to reload the part lib
            // and normally the settings are saved when you exit the application.
            SavePartLibUISettingInDefaultSettings();

            // then clear the part lib panel and the brick library (before creating the new map)
            PartsTabControl.clearAllData();
            BrickLibrary.Instance.ClearAllData();

            // destroy the current map
            ReinitializeCurrentMap();

            // call the GC to be sure that all the image are correctly released, and no files stay locked
            // the garbage collector was called at then end of the reinitializeCurrentMap function called just above
            // GC.Collect();
            return true;
        }

        private void ReloadPartLibrary(string mapFileNameToReload)
        {
            // then reload the library
            Cursor = Cursors.WaitCursor;
            LoadPartLibraryFromDisk();
            Cursor = Cursors.Default;

            // Update the budget: most of the time the budget text for the item are correct (cause correctly set during creation)
            // however, the user may have rename a part just before reloading the part lib, 
            // so we need to update the budget and the view again if the budget was modified
            if (Budget.Budget.Instance.updatePartId())
            {
                // update the count and budget
                PartsTabControl.updateAllPartCountAndBudget();
                // update the part lib view filtering on budget (because the renamed items may appear/disappear)
                PartsTabControl.updateFilterOnBudgetedParts();
            }

            // finally reload the previous map or create a new one
            if (string.IsNullOrWhiteSpace(mapFileNameToReload))
            {
                CreateNewMap();
                return;
            }

            OpenMap(mapFileNameToReload);

            // since the connexion position may have changed after the reload of the library,
            // maybe so 2 free connexions will become aligned, and can be connected, that's why
            // we perform a slow update connectivity in that case.
            for (var i = 0; i < Map.Instance.LayerList.Count; i++)
            {
                if (Map.Instance.LayerList[i] is LayerBrick layerBrick)
                {
                    layerBrick.updateFullBrickConnectivity();
                }
            }
        }

        private void DownloadAdditionnalPartsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // first spawn the window to let the user choose the online source of the part library
            var packageSourceForm = new LibraryPackageSourceForm();
            packageSourceForm.ShowDialog();

            var fileList = packageSourceForm.FilesToDownload?.ToArray();

            // get the list of files that has been collected by the previous form dialog
            // check if we have something to download (if not just ignore, because the error message as already been displayed in the previous form)
            if (fileList == null || fileList.Length == 0)
            {
                return;
            }

            // open the download center form in dialog mode
            var downloadCenterForm = new DownloadCenterForm(fileList, true);
            downloadCenterForm.ShowDialog();

            fileList = downloadCenterForm.SuccessfullyDownloadedFiles?.ToArray();

            // get the list of files that has been succesfully downloaded
            // when the user closed the dialog, check if any package was successfully download, and if yes, we need to reload the library
            if (fileList == null || fileList.Length == 0)
            {
                return;
            }

            // display a warning message and reload the library
            MessageBox.Show(
                this,
                Properties.Resources.ErrorMsgNeedToReloadPartLib,
                Properties.Resources.ErrorMsgTitleWarning,
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button1
            );

            // after the user close the message box, unload, install and reload the library
            if (!UnloadPartLibrary(out string mapFileNameToReload))
            {
                return;
            }

            // a string to get the part folder
            var partsFolder = Application.StartupPath + @"/parts";

            string currentPackageFolderName, zipFileName;
            DialogResult result;

            for (var i = 0; i < fileList.Length; i++)
            {
                try
                {
                    // get the current folder of the library
                    currentPackageFolderName = partsFolder + @"/" + fileList[i].FileName;
                    currentPackageFolderName = currentPackageFolderName.Remove(currentPackageFolderName.Length - 4);

                    // check if the package already exists, and delete it in that case.
                    // give several chance to user in case this directory is locked
                    result = DialogResult.Retry;

                    while (result == DialogResult.Retry && Directory.Exists(currentPackageFolderName))
                    {
                        try
                        {
                            Directory.Delete(currentPackageFolderName, true);
                        }
                        catch (IOException ioe)
                        {
                            // display a warning message and reload the library
                            result = MessageBox.Show(
                                this,
                                Properties.Resources.ErrorMsgExceptionWhenDeletingPartLib
                                    .Replace("&&", ioe.Message)
                                    .Replace("&", fileList[i].FileName),
                                Properties.Resources.ErrorMsgTitleError,
                                MessageBoxButtons.AbortRetryIgnore,
                                MessageBoxIcon.Exclamation,
                                MessageBoxDefaultButton.Button1
                            );
                        }
                    }
                    // check if user aborted the library installation, in that case break the loop to stop the install
                    // else if he ignore, then skip that package and continue with the other packages
                    if (result == DialogResult.Abort)
                    {
                        break;
                    }
                    else if (result == DialogResult.Ignore)
                    {
                        continue;
                    }

                    // unzip the new archive
                    zipFileName = Application.StartupPath + fileList[i].DestinationFolder;

                    ZipFile.ExtractToDirectory(zipFileName, partsFolder);

                    // then delete the archive
                    File.Delete(zipFileName);
                }
                catch
                {
                }
            }

            // reload the part library when the user close the message box
            ReloadPartLibrary(mapFileNameToReload);
        }

        private void SaveSelectionInLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // create a window for editing the option of the group and show it
            var form = new SaveGroupNameForm();
            var result = form.ShowDialog();

            // check if we need to update the part lib
            if (result != DialogResult.OK || form.NewXmlFilesToLoad == null || form.NewXmlFilesToLoad.Count == 0)
            {
                return;
            }

            PartsTabControl.loadAdditionnalGroups(form.NewXmlFilesToLoad, form.NewGroupName);
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // the check if the current map is not save is done in the Closing Event
            // in order to catch all the path to close the application
            // (such as Alt+F4, the cross button in the title bar, the Close item
            // in the menu of the title bar, etc...)
            Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // check if the current map or budget is not save and display a warning message
            if (!CheckForUnsavedMap() || !CheckForUnsavedBudget())
            {
                // if the player cancel the closing then cancel the event
                e.Cancel = true;
            }

            // if the user didn't cancel the close, save the setting of the user,
            // in order to save the main form position, size and state
            if (!e.Cancel)
            {
                SaveUISettingInDefaultSettings();
            }
        }

        #endregion

        #region Edit Menu

        private void UpdateUndoRedoMenuItems()
        {
            // reinit undo/redo menu with the correct language
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            resources.ApplyResources(undoToolStripMenuItem, "undoToolStripMenuItem");
            resources.ApplyResources(redoToolStripMenuItem, "redoToolStripMenuItem");

            // check if the undo stack is empty or not
            var actionName = ActionManager.Instance.getUndoableActionName();
            if (string.IsNullOrWhiteSpace(actionName))
            {
                undoToolStripMenuItem.Enabled = false;
            }
            else
            {
                undoToolStripMenuItem.Enabled = true;
                undoToolStripMenuItem.Text += " \"" + actionName + "\"";
            }

            // check if the redo stack is empty or not
            actionName = ActionManager.Instance.getRedoableActionName();
            if (string.IsNullOrWhiteSpace(actionName))
            {
                redoToolStripMenuItem.Enabled = false;
            }
            else
            {
                redoToolStripMenuItem.Enabled = true;
                redoToolStripMenuItem.Text += " \"" + actionName + "\"";
            }

            // the undo toolbar button
            toolBarUndoButton.Enabled = undoToolStripMenuItem.Enabled;
            toolBarUndoButton.DropDownItems.Clear();

            int i;

            var actionNameList = ActionManager.Instance.getUndoActionNameList(Properties.Settings.Default.UndoStackDisplayedDepth);
            if (actionNameList == null)
            {
                actionNameList = Array.Empty<string>();
            }

            for (i = 0; i < actionNameList.Length; i++)
            {
                toolBarUndoButton.DropDownItems.Add(actionNameList[i]);
            }

            // the redo toolbar button
            toolBarRedoButton.Enabled = redoToolStripMenuItem.Enabled;
            toolBarRedoButton.DropDownItems.Clear();

            actionNameList = ActionManager.Instance.getRedoActionNameList(Properties.Settings.Default.UndoStackDisplayedDepth);
            if (actionNameList == null)
            {
                actionNameList = Array.Empty<string>();
            }

            for (i = 0; i < actionNameList.Length; i++)
            {
                toolBarRedoButton.DropDownItems.Add(actionNameList[i]);
            }
        }

        private void UndoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActionManager.Instance.undo(1);
        }

        private void RedoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActionManager.Instance.redo(1);
        }

        private void CutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // first get the current selected layer
            var selectedLayer = Map.Instance.SelectedLayer;
            if (selectedLayer == null || selectedLayer.SelectedObjects == null || selectedLayer.SelectedObjects.Count == 0)
            {
                return;
            }

            // a cut is a copy followed by a delete
            selectedLayer.CopyCurrentSelectionToClipboard();
            DeleteToolStripMenuItem_Click(sender, e);
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // first get the current selected layer
            var selectedLayer = Map.Instance.SelectedLayer;

            // then call the funcion to copy the selection on the selected layer
            selectedLayer?.CopyCurrentSelectionToClipboard();
        }

        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // first get the current selected layer
            var selectedLayer = Map.Instance.SelectedLayer;
            if (selectedLayer == null)
            {
                return;
            }

            var addInHistory = Layer.AddActionInHistory.ADD_TO_HISTORY;
            // call the paste method on the selected layer, and display an error if the paste was not possible
            if (selectedLayer.PasteClipboardInLayer(Layer.AddOffsetAfterPaste.USE_SETTINGS_RULE, out string itemTypeName, ref addInHistory))
            {
                return;
            }

            // we have a type mismatch
            if (!Properties.Settings.Default.DisplayWarningMessageForPastingOnWrongLayer)
            {
                System.Media.SystemSounds.Beep.Play();
                return;
            }

            // first replace the layer type name, then replace the item type name
            var message = Properties.Resources.ErrorMsgCanNotPaste.Replace("&&", selectedLayer.LocalizedTypeName).Replace("&", itemTypeName);

            // and display the message box
            var dontDisplayMessageAgain = false;

            ForgetableMessageBox.Show(
                this,
                message,
                Properties.Resources.ErrorMsgTitleError,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                ref dontDisplayMessageAgain
            );

            Properties.Settings.Default.DisplayWarningMessageForPastingOnWrongLayer = !dontDisplayMessageAgain;
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // first get the current selected layer
            var selectedLayer = Map.Instance.SelectedLayer;
            if (selectedLayer == null || selectedLayer.SelectedObjects == null || selectedLayer.SelectedObjects.Count == 0)
            {
                return;
            }

            if (selectedLayer is LayerText layerText)
            {
                ActionManager.Instance.doAction(new DeleteText(layerText, selectedLayer.SelectedObjects));
            }
            else if (selectedLayer is LayerBrick layerBrick)
            {
                ActionManager.Instance.doAction(new DeleteBrick(layerBrick, selectedLayer.SelectedObjects));
            }
            else if (selectedLayer is LayerRuler layerRuler)
            {
                ActionManager.Instance.doAction(new DeleteRuler(layerRuler, selectedLayer.SelectedObjects));
            }
        }

        private void FindAndReplaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var findForm = new FindForm();
            findForm.ShowDialog(this);
            mapPanel.Invalidate();
        }

        public void SelectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // select all in the current selected layer
            var selectedLayer = Map.Instance.SelectedLayer;
            if (selectedLayer == null)
            {
                return;
            }

            selectedLayer.SelectAll();
            mapPanel.Invalidate();
        }

        public void DeselectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedLayer = Map.Instance.SelectedLayer;
            if (selectedLayer == null || selectedLayer.SelectedObjects == null || selectedLayer.SelectedObjects.Count == 0)
            {
                return;
            }

            selectedLayer.ClearSelection();
            mapPanel.Invalidate();
        }

        public void SelectPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedLayer = Map.Instance.SelectedLayer;
            if (selectedLayer == null || !(selectedLayer is LayerBrick layerBrick))
            {
                return;
            }

            var selectedBrickCount = layerBrick.SelectedObjects.Count;
            if (selectedBrickCount < 2)
            {
                return;
            }

            var brickToSelect = MapData.Tools.AStar.FindPath(
                layerBrick.SelectedObjects[selectedBrickCount - 2] as LayerBrick.Brick,
                layerBrick.SelectedObjects[selectedBrickCount - 1] as LayerBrick.Brick
            );

            // if AStar found a path, select the path
            if (brickToSelect.Count <= 0)
            {
                return;
            }

            layerBrick.AddObjectInSelection(brickToSelect);
            mapPanel.Invalidate();
        }

        public void GroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedLayer = Map.Instance.SelectedLayer;
            if (selectedLayer == null || selectedLayer.SelectedObjects == null || selectedLayer.SelectedObjects.Count == 0)
            {
                return;
            }

            if (selectedLayer is LayerBrick || selectedLayer is LayerText || selectedLayer is LayerRuler)
            {
                ActionManager.Instance.doAction(new Actions.Items.GroupItems(selectedLayer.SelectedObjects, selectedLayer));
            }
        }

        public void UngroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedLayer = Map.Instance.SelectedLayer;
            if (selectedLayer == null || selectedLayer.SelectedObjects == null || selectedLayer.SelectedObjects.Count == 0)
            {
                return;
            }

            if (selectedLayer is LayerBrick || selectedLayer is LayerText || selectedLayer is LayerRuler)
            {
                ActionManager.Instance.doAction(new Actions.Items.UngroupItems(selectedLayer.SelectedObjects, selectedLayer));
            }
        }

        private void RotateCWToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedLayer = Map.Instance.SelectedLayer;
            if (selectedLayer == null || selectedLayer.SelectedObjects == null || selectedLayer.SelectedObjects.Count == 0)
            {
                return;
            }

            if (selectedLayer is LayerBrick layerBrick)
            {
                ActionManager.Instance.doAction(new RotateBrick(layerBrick, selectedLayer.SelectedObjects, 1));
            }
            else if (selectedLayer is LayerText layerText)
            {
                ActionManager.Instance.doAction(new RotateText(layerText, selectedLayer.SelectedObjects, 1));
            }
            else if (selectedLayer is LayerRuler layerRuler)
            {
                ActionManager.Instance.doAction(new RotateRulers(layerRuler, selectedLayer.SelectedObjects, 1));
            }
        }

        private void RotateCCWToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedLayer = Map.Instance.SelectedLayer;
            if (selectedLayer == null || selectedLayer.SelectedObjects == null || selectedLayer.SelectedObjects.Count == 0)
            {
                return;
            }

            if (selectedLayer is LayerBrick layerBrick)
            {
                ActionManager.Instance.doAction(new RotateBrick(layerBrick, selectedLayer.SelectedObjects, -1));
            }
            else if (selectedLayer is LayerText layerText)
            {
                ActionManager.Instance.doAction(new RotateText(layerText, selectedLayer.SelectedObjects, -1));
            }
            else if (selectedLayer is LayerRuler layerRuler)
            {
                ActionManager.Instance.doAction(new RotateRulers(layerRuler, selectedLayer.SelectedObjects, -1));
            }
        }

        public void SendToBackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedLayer = Map.Instance.SelectedLayer;
            if (selectedLayer == null || selectedLayer.SelectedObjects == null || selectedLayer.SelectedObjects.Count == 0)
            {
                return;
            }

            Actions.Action action = null;
            
            if (selectedLayer is LayerBrick layerBrick)
            {
                action = new SendBrickToBack(layerBrick, layerBrick.SelectedObjects);
            }
            else if (selectedLayer is LayerText layerText)
            {
                action = new SendTextToBack(layerText, layerText.SelectedObjects);
            }
            else if (selectedLayer is LayerRuler layerRuler)
            {
                action = new SendRulerToBack(layerRuler, layerRuler.SelectedObjects);
            }

            if (action != null)
            {
                ActionManager.Instance.doAction(action);
            }
        }

        public void BringToFrontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedLayer = Map.Instance.SelectedLayer;
            if (selectedLayer == null || selectedLayer.SelectedObjects == null || selectedLayer.SelectedObjects.Count == 0)
            {
                return;
            }

            Actions.Action action = null;
            
            if (selectedLayer is LayerBrick layerBrick)
            {
                action = new BringBrickToFront(layerBrick, layerBrick.SelectedObjects);
            }
            else if (selectedLayer is LayerText layerText)
            {
                action = new BringTextToFront(layerText, layerText.SelectedObjects);
            }
            else if (selectedLayer is LayerRuler layerRuler)
            {
                action = new BringRulerToFront(layerRuler, layerRuler.SelectedObjects);
            }

            if (action != null)
            {
                ActionManager.Instance.doAction(action);
            }
        }

        private void MoveStepDisabledToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EnableSnapGridButton(false, Layer.CurrentSnapGridSize);
        }

        private void MoveStep32ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // enable the toolbar and the menu item
            EnableSnapGridButton(true, 32.0f);
        }

        private void MoveStep16ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // enable the toolbar and the menu item
            EnableSnapGridButton(true, 16.0f);
        }

        private void MoveStep8ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // enable the toolbar and the menu item
            EnableSnapGridButton(true, 8.0f);
        }

        private void MoveStep4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // enable the toolbar and the menu item
            EnableSnapGridButton(true, 4.0f);
        }

        private void MoveStep2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // enable the toolbar and the menu item
            EnableSnapGridButton(true, 2.0f);
        }

        private void MoveStep1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // enable the toolbar and the menu item
            EnableSnapGridButton(true, 1.0f);
        }

        private void MoveStep05ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // enable the toolbar and the menu item
            EnableSnapGridButton(true, 0.5f);
        }

        /// <summary>
        /// Update the checkmark in front of the correct rotation step in the menu and the toolbard
        /// according to the specified angle value
        /// </summary>
        /// <param name="angle">the new angle chosen</param>
        private void UpdateRotationStepButton(float angle)
        {
            // toolbar and menu
            toolBarAngle90Button.Checked = rotationStep90ToolStripMenuItem.Checked = angle == 90.0f;
            toolBarAngle45Button.Checked = rotationStep45ToolStripMenuItem.Checked = angle == 45.0f;
            toolBarAngle22Button.Checked = rotationStep22ToolStripMenuItem.Checked = angle == 22.5f;
            toolBarAngle1Button.Checked = rotationStep1ToolStripMenuItem.Checked = angle == 1.0f;
            
            //layer
            Layer.CurrentRotationStep = angle;
        }

        private void RotationStep90ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateRotationStepButton(90.0f);
        }

        private void RotationStep45ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateRotationStepButton(45.0f);
        }

        private void RotationStep22ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateRotationStepButton(22.5f);
        }

        private void RotationStep1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateRotationStepButton(1.0f);
        }

        private void PaintToolPaintToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // toolbar
            toolBarToolButton.Image = mPaintIcon;
            
            // menu
            paintToolPaintToolStripMenuItem.Checked = true;
            paintToolEraseToolStripMenuItem.Checked = false;
            
            // set the current static paint color in the area layer
            LayerArea.CurrentDrawColor = mCurrentPaintIconColor;
        }

        private void PaintToolEraseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // toolbar
            toolBarToolButton.Image = eraseToolStripMenuItem.Image;
            
            // menu
            paintToolPaintToolStripMenuItem.Checked = false;
            paintToolEraseToolStripMenuItem.Checked = true;
            
            // set the current static paint color in the area layer
            LayerArea.CurrentDrawColor = Color.Empty;
        }

        public void RulerSelectAndEditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // toolbar
            toolBarToolButton.Image = rulerSelectAndEditToolStripMenuItem.Image;

            // menu
            selectAndEditToolStripMenuItem.Checked = true;
            addRulerToolStripMenuItem.Checked = false;
            addCircleToolStripMenuItem.Checked = false;

            // set the selected tool in the static vaqr of the ruler layer
            LayerRuler.CurrentEditTool = LayerRuler.EditTool.SELECT;
        }

        private void RulerAddRulerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // toolbar
            toolBarToolButton.Image = rulerAddRulerToolStripMenuItem.Image;
            
            // menu
            selectAndEditToolStripMenuItem.Checked = false;
            addRulerToolStripMenuItem.Checked = true;
            addCircleToolStripMenuItem.Checked = false;
            
            // set the selected tool in the static vaqr of the ruler layer
            LayerRuler.CurrentEditTool = LayerRuler.EditTool.LINE;
        }

        private void RulerAddCircleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // toolbar
            toolBarToolButton.Image = rulerAddCircleToolStripMenuItem.Image;
            
            // menu
            selectAndEditToolStripMenuItem.Checked = false;
            addRulerToolStripMenuItem.Checked = false;
            addCircleToolStripMenuItem.Checked = true;
            
            // set the selected tool in the static vaqr of the ruler layer
            LayerRuler.CurrentEditTool = LayerRuler.EditTool.CIRCLE;
        }

        private void GeneratePaintIcon(Color color)
        {
            // assign the current paint color with the specified parameter
            mCurrentPaintIconColor = color;
            
            // get the background color from the specified color
            // but cheat a little if the user choose the magenta color, because it is the transparent
            // color of the original bitmap
            Color backColor = color;
            if ((backColor == Color.Magenta) || (backColor == Color.Fuchsia))
            {
                backColor = Color.FromArgb(unchecked((int)0xFFFF00FE));
            }
            
            // recreate the icon and use the color of the color dialog for the background
            mPaintIcon = new Bitmap(16, 16);
            Graphics g = Graphics.FromImage(mPaintIcon);
            g.Clear(backColor);
            g.DrawImage(paintToolStripMenuItem.Image, 0, 0);
            g.Flush();
            
            // refresh the icon
            toolBarToolButton.Image = mPaintIcon;
            
            // set the current static paint color in the area layer
            LayerArea.CurrentDrawColor = color;
        }

        private void PaintToolChooseColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            colorDialog.Color = mCurrentPaintIconColor;
            var result = colorDialog.ShowDialog();
            if (result != DialogResult.OK)
            {
                return;
            }

            // regenerate the icon
            GeneratePaintIcon(colorDialog.Color);
            // and reselect the paint tool
            PaintToolPaintToolStripMenuItem_Click(sender, e);
        }

        /// <summary>
        /// This function can be called via two different places, either via the menu "Edit > Map Background Color"
        /// of via the button in the Properties tab. Both way do the same thing.
        /// </summary>
        private void OpenColorPickerToChangeMapBackgroundColor()
        {
            var result = colorDialog.ShowDialog();
            if (result != DialogResult.OK)
            {
                return;
            }

            ActionManager.Instance.doAction(new ChangeBackgroundColor(colorDialog.Color));
        }

        private void MapBackgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenColorPickerToChangeMapBackgroundColor();
        }

        private void CurrentLayerOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // first get the current selected layer
            if (Map.Instance.SelectedLayer == null)
            {
                return;
            }

            var selectedLayer = Map.Instance.SelectedLayer;

            Form optionForm = null;

            if (selectedLayer is LayerGrid layerGrid)
            {
                optionForm = new LayerGridOptionForm(layerGrid);
            }
            else if (selectedLayer is LayerBrick layerBrick)
            {
                optionForm = new LayerBrickOptionForm(layerBrick);
            }
            else if (selectedLayer is LayerText layerText)
            {
                optionForm = new LayerTextOptionForm(layerText);
            }
            else if (selectedLayer is LayerArea layerArea)
            {
                optionForm = new LayerAreaOptionForm(layerArea);
            }
            else if (selectedLayer is LayerRuler layerRuler)
            {
                optionForm = new LayerTextOptionForm(layerRuler);
            }

            optionForm?.ShowDialog();
        }

        private void PreferencesMenuItem_Click(object sender, EventArgs e)
        {
            var preferenceForm = new PreferencesForm();

            var result = preferenceForm.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                // reinit the array of shortcut
                InitShortcutKeyArrayFromSettings();
                // update the gamma for the layer bricks because they may have changed (before redrawing the map)
                Map.Instance.UpdateGammaFromSettings();
                // redraw the map because the color scheme may have changed
                mapPanel.Invalidate();
                // also redraw the undo stack
                UpdateUndoRedoMenuItems();
            }

            // update the the recent file list anyway because the user may have click the
            // clear recent file list button before clicking cancel
            UpdateRecentFileMenuFromConfigFile();

            // before checking if we need to restart, check if the language package is correctly installed
            LanguageManager.CheckLanguage(Properties.Settings.Default.Language);

            // check if we need to restart, if yes, ask the user what he wants to do
            if (!preferenceForm.DoesNeedToRestart)
            {
                return;
            }

            result = MessageBox.Show(
                this,
                Properties.Resources.ErrorMsgLanguageHasChanged,
                Properties.Resources.ErrorMsgTitleWarning,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1
            );

            if (result != DialogResult.Yes)
            {
                return;
            }

            // the user can not cancel the close of the application, because once the restart is called
            // it will launch a new instance of the application, so if the user can cancel the close
            // of the current instance he may end up with two instances.
            mCanUserCancelTheApplicationClose = false;
            // and restart the application
            Application.Restart();
        }

        #endregion

        #region View Menu
        private void ToolbarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolBar.Visible = toolbarMenuItem.Checked;
        }

        private void StatusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusBar.Visible = statusBarMenuItem.Checked;
            mapPanel.CurrentStatusBarHeight = statusBar.Visible ? statusBar.Height : 0;
            mapPanel.Invalidate();
        }

        public void MapScrollBarsVisibilityChangeNotification(bool newVisibility)
        {
            mapScrollBarsToolStripMenuItem.Checked = newVisibility;
        }

        private void MapScrollBarsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mapPanel.ShowHideScrollBars(mapScrollBarsToolStripMenuItem.Checked);
            mapPanel.Invalidate();
        }

        private void WatermarkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.DisplayGeneralInfoWatermark = watermarkToolStripMenuItem.Checked;
            mapPanel.Invalidate();
        }

        private void ElectricCircuitsMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.DisplayElectricCircuit = electricCircuitsMenuItem.Checked;
            mapPanel.Invalidate();
        }

        private void ConnectionPointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.DisplayFreeConnexionPoints = connectionPointsToolStripMenuItem.Checked;
            mapPanel.Invalidate();
        }

        private void RulerAttachPointsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.DisplayRulerAttachPoints = rulerAttachPointsToolStripMenuItem.Checked;
            mapPanel.Invalidate();
        }
        #endregion

        #region Budget Menu
        /// <summary>
        /// Enable or disable the budget menu item, depending if there's a current budget existing
        /// </summary>
        /// <param name="isEnabled"></param>
        private void UpdateEnableStatusForBudgetMenuItem()
        {
            budgetImportAndMergeToolStripMenuItem.Enabled = Budget.Budget.Instance.IsExisting;
            budgetCloseToolStripMenuItem.Enabled = Budget.Budget.Instance.IsExisting;
            budgetSaveToolStripMenuItem.Enabled = Budget.Budget.Instance.IsExisting;
            budgetSaveAsToolStripMenuItem.Enabled = Budget.Budget.Instance.IsExisting;
            showOnlyBudgetedPartsToolStripMenuItem.Enabled = Budget.Budget.Instance.IsExisting;
            showBudgetNumbersToolStripMenuItem.Enabled = Budget.Budget.Instance.IsExisting;
            useBudgetLimitationToolStripMenuItem.Enabled = Budget.Budget.Instance.IsExisting;
        }

        /// <summary>
        /// This method check if the current budget is not saved and prompt a message box asking
        /// what to do (save, do not save or cancel). This method sould be called before
        /// exiting the application, and before creating or loading a new budget
        /// <returns>true if the action can continue, false if user choose to cancel</returns>
        /// </summary>
        private bool CheckForUnsavedBudget()
        {
            // the map was not modified, the action can continue
            if (!Budget.Budget.Instance.WasModified)
            {
                return true;
            }

            // if the user can cancel the application close, give him 3 buttons yes/no/cancel,
            // else give him only 2 buttons yes/no:
            var result = MessageBox.Show(
                this,
                Properties.Resources.ErrorMsgBudgetWasModified,
                Properties.Resources.ErrorMsgTitleWarning,
                mCanUserCancelTheApplicationClose ? MessageBoxButtons.YesNoCancel : MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1
            );

            switch (result)
            {
                case DialogResult.Yes:
                    {
                        // call the save method (that maybe will perform a save as)
                        BudgetSaveToolStripMenuItem_Click(null, null);
                        break;
                    }
                case DialogResult.Cancel:
                    {
                        // user cancel so return false
                        return false;
                    }
            }

            // user choose "yes", the action can continue
            return true;
        }

        /// <summary>
        /// Open the specified bugdet and eventually merge it with the other specified budget
        /// </summary>
        /// <param name="filename">the budget file to open</param>
        /// <param name="budgetToMerge">the optionnal budget to merge with or null</param>
        /// <returns>true if the budget was correctly open</returns>
        private bool OpenBudget(string filename, Budget.Budget budgetToMerge = null)
        {
            // set the wait cursor
            Cursor = Cursors.WaitCursor;

            // load the file
            var isFileValid = SaveLoadManager.load(filename);
            if (isFileValid)
            {
                // check if we need to merge a budget in the new opened budget or not
                if (budgetToMerge != null)
                {
                    Budget.Budget.Instance.mergeWith(budgetToMerge);
                    // give back the title of the original budget to the loaded budget
                    ChangeCurrentBudgetFileName(budgetToMerge.BudgetFileName, true);
                }
                else
                {
                    // change the filename in the title bar
                    ChangeCurrentBudgetFileName(filename, true);
                }

                // recount the parts because, opening a new budget actually create a new instance of Budget, so the count is destroyed
                Budget.Budget.Instance.recountAllBricks();
                PartUsageListView.rebuildList();
                // update the filtering on of the part lib after recounting all the bricks
                PartsTabControl.updateFilterOnBudgetedParts();
            }

            // update the menu items
            UpdateEnableStatusForBudgetMenuItem();
            // then update the view of the part lib
            PartsTabControl.updateViewStyle();
            // update the budgets in the part usage view
            PartUsageListView.updateBudgetNotification();
            // restore the cursor after loading
            Cursor = Cursors.Default;
            // return if the file was correctly loaded
            return isFileValid;
        }

        private void ChangeCurrentBudgetFileName(string filename, bool isAValidName)
        {
            // save the filename
            Budget.Budget.Instance.BudgetFileName = filename;
            Budget.Budget.Instance.IsFileNameValid = isAValidName;
            // update the name of the title bar with the name of the file
            UpdateTitleBar();
        }

        private void BudgetNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForUnsavedBudget())
            {
                return;
            }

            // create a new budget
            Budget.Budget.Instance.create();
            // update the part lib view
            PartsTabControl.updateFilterOnBudgetedParts();
            // update the title bar
            UpdateTitleBar();
            // update the menu items
            UpdateEnableStatusForBudgetMenuItem();
            // then update the view of the part lib
            PartsTabControl.updateViewStyle();
            // update the budgets in the part usage view
            PartUsageListView.updateBudgetNotification();
        }

        private void BudgetOpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // check if the current budget is not save and display a warning message
            if (!CheckForUnsavedBudget())
            {
                return;
            }

            var result = openBudgetFileDialog.ShowDialog();
            if (result != DialogResult.OK)
            {
                return;
            }

            OpenBudget(openBudgetFileDialog.FileName);
        }

        private void BudgetImportAndMergeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // we don't check for unsaved budget, cause we will merge the current budget with the one we open
            // save the current budget instance, cause the loading of new budget will erase it
            // open the new one
            var result = openBudgetFileDialog.ShowDialog();
            if (result != DialogResult.OK)
            {
                return;
            }

            OpenBudget(openBudgetFileDialog.FileName, Budget.Budget.Instance);
        }

        private void BudgetCloseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForUnsavedBudget())
            {
                return;
            }

            // destroy the budget
            Budget.Budget.Instance.destroy();
            // update the part lib view
            PartsTabControl.updateFilterOnBudgetedParts();
            // update the title bar (to remove the budget name from the title bar)
            UpdateTitleBar();
            // update the menu items
            UpdateEnableStatusForBudgetMenuItem();
            // then update the view of the part lib
            PartsTabControl.updateViewStyle();
            // update the budgets in the part usage view
            PartUsageListView.updateBudgetNotification();
        }

        private void SaveBudget()
        {
            // set the wait cursor
            Cursor = Cursors.WaitCursor;

            // save the file
            var saveDone = SaveLoadManager.save(Budget.Budget.Instance.BudgetFileName);
            if (saveDone)
            {
                // after saving the budget, we reset the WasModified flag of the budget
                // (and before the update of the title bar)
                Budget.Budget.Instance.WasModified = false;
                // update the title bar
                UpdateTitleBar();
            }

            // restore the cursor
            Cursor = Cursors.Default;
        }

        private void BudgetSaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // if the current file name is not defined we do a "save as..."
            if (Budget.Budget.Instance.IsFileNameValid)
            {
                SaveBudget();
            }
            else
            {
                BudgetSaveAsToolStripMenuItem_Click(sender, e);
            }
        }

        private void BudgetSaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // put the current file name in the dialog (which can be the default one)
            saveBudgetFileDialog.FileName = Budget.Budget.Instance.BudgetFileName;

            // if there's no initial directory, choose the My Documents directory
            saveBudgetFileDialog.InitialDirectory = Path.GetDirectoryName(Budget.Budget.Instance.BudgetFileName);
            if (saveBudgetFileDialog.InitialDirectory != null && saveBudgetFileDialog.InitialDirectory.Length == 0)
            {
                saveBudgetFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            // open the save as dialog
            var result = saveBudgetFileDialog.ShowDialog();
            if (result != DialogResult.OK)
            {
                return;
            }

            // change the current file name before calling the save
            ChangeCurrentBudgetFileName(saveBudgetFileDialog.FileName, true);
            // save the map
            SaveBudget();
        }

        public void ShowOnlyBudgetedPartsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // update the setting
            Properties.Settings.Default.ShowOnlyBudgetedParts = !Properties.Settings.Default.ShowOnlyBudgetedParts;
            // then udpate the check state according to the new setting
            showOnlyBudgetedPartsToolStripMenuItem.Checked = Properties.Settings.Default.ShowOnlyBudgetedParts;
            // update the filtering on budget of the part lib
            PartsTabControl.updateFilterOnBudgetedParts();
        }

        public void ShowBudgetNumbersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // update the setting
            Properties.Settings.Default.ShowBudgetNumbers = !Properties.Settings.Default.ShowBudgetNumbers;
            // then udpate the check state according to the new setting
            showBudgetNumbersToolStripMenuItem.Checked = Properties.Settings.Default.ShowBudgetNumbers;
            // then update the view of the part lib
            PartsTabControl.updateViewStyle();
        }

        public void UseBudgetLimitationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // update the setting
            Properties.Settings.Default.UseBudgetLimitation = !Properties.Settings.Default.UseBudgetLimitation;

            // then udpate the check state according to the new setting
            useBudgetLimitationToolStripMenuItem.Checked = Properties.Settings.Default.UseBudgetLimitation;

            // now check if we need to also show the budget numbers
            if (!Properties.Settings.Default.DisplayWarningMessageForShowingBudgetNumbers
                || !Properties.Settings.Default.UseBudgetLimitation
                || showBudgetNumbersToolStripMenuItem.Checked
            )
            {
                return;
            }

            var dontDisplayMessageAgain = false;

            var result = ForgetableMessageBox.Show(
                this,
                Properties.Resources.ErrorMsgDoYouWantToDisplayBudgetNumber,
                Properties.Resources.ErrorMsgTitleWarning,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2,
                ref dontDisplayMessageAgain
            );

            // set back the checkbox value in the settings (don't save the settings now, it will be done when exiting the application)
            Properties.Settings.Default.DisplayWarningMessageForShowingBudgetNumbers = !dontDisplayMessageAgain;

            // check the result, if yes, click also on display the budget numbers
            if (result != DialogResult.Yes)
            {
                return;
            }

            ShowBudgetNumbersToolStripMenuItem_Click(null, null);
        }
        #endregion

        #region Help Menu
        private void HelpContentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fileName = $"{nameof(BlueBrick)}.chm";

            // construct the path of the help file from the current language
            var helpFileInfo = new FileInfo(Application.StartupPath + @"/" + Properties.Settings.Default.Language + $@"/{fileName}");

            // If the help file related with the current language of the application does not exist,
            // load the default one which is in the same folder as the application
            if (!helpFileInfo.Exists)
            {
                helpFileInfo = new FileInfo(Application.StartupPath + $@"/{fileName}");
            }

            // now check if we can open the help file, else display a warning message
            if (!helpFileInfo.Exists)
            {
                MessageBox.Show(
                    this,
                    Properties.Resources.ErrorMsgNoHelpFile,
                    Properties.Resources.ErrorMsgTitleError,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1
                );
                return;
            }

            // MONOBUG: The Help.ShowHelp is not implemented yet on Mono, do our own implementation for Linux or Mac
            if ((Environment.OSVersion.Platform == PlatformID.Unix) || (Environment.OSVersion.Platform == PlatformID.MacOSX))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = helpFileInfo.FullName
                });
            }
            else
            {
                Help.ShowHelp(this, helpFileInfo.FullName);
            }
        }

        private void AboutBlueBrickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }
        #endregion

        #endregion

        #region event handler for toolbar

        private void ToolBarUndoButton_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var index = e.ClickedItem.Owner.Items.IndexOf(e.ClickedItem);
            ActionManager.Instance.undo(index + 1);
        }

        private void ToolBarRedoButton_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var index = e.ClickedItem.Owner.Items.IndexOf(e.ClickedItem);
            ActionManager.Instance.redo(index + 1);
        }

        private void ToolBarSnapGridButton_Click(object sender, EventArgs e)
        {
            if (!toolBarSnapGridButton.ButtonPressed)
            {
                return;
            }

            EnableSnapGridButton(!toolBarSnapGridButton.DropDown.Enabled, Layer.CurrentSnapGridSize);
        }

        private void ToolBarPaintButton_ButtonClick(object sender, EventArgs e)
        {
            // nothing happen by default, but if the paint icon is selected this call the color picker
            if (toolBarToolButton.Image != mPaintIcon)
            {
                return;
            }

            PaintToolChooseColorToolStripMenuItem_Click(sender, e);
        }
        #endregion

        #region event handler for part lib
        private void AddInputFilterIndication()
        {
            // set the color first cause we will check it in the TextChange event
            textBoxPartFilter.ForeColor = Color.Gray;
            textBoxPartFilter.Text = Properties.Resources.InputFilterIndication;
        }

        private void RemoveInputFilterIndication()
        {
            // set the color first cause we will check it in the TextChange event
            textBoxPartFilter.ForeColor = Color.Black;
            textBoxPartFilter.Text = string.Empty;
        }

        private bool IsThereAnyUserFilterSentence()
        {
            // we use the color of the text box filter, because the user may type a sentence
            // which could be exactly as the input filter indication (i.e. the hint)
            return textBoxPartFilter.ForeColor == Color.Black;
        }

        private string GetUserFilterSentence()
        {
            return IsThereAnyUserFilterSentence() ? textBoxPartFilter.Text : string.Empty;
        }

        private void TextBoxPartFilter_TextChanged(object sender, EventArgs e)
        {
            // do not call the filtering if the box is disabled
            if (!IsThereAnyUserFilterSentence())
            {
                return;
            }

            // checked which filter method to call
            if (filterAllTabCheckBox.Checked)
            {
                PartsTabControl.filterAllTabs(textBoxPartFilter.Text);
            }
            else
            {
                PartsTabControl.filterCurrentTab(textBoxPartFilter.Text);
            }
        }

        private void TextBoxPartFilter_Enter(object sender, EventArgs e)
        {
            // if the user enter the text box without any keyword set, delete the hint
            if (!IsThereAnyUserFilterSentence())
            {
                RemoveInputFilterIndication();
            }
        }

        private void TextBoxPartFilter_Leave(object sender, EventArgs e)
        {
            // if the user deleted the whole text when leaving the box, add the incitation text
            if (!string.IsNullOrEmpty(textBoxPartFilter.Text))
            {
                return;
            }

            AddInputFilterIndication();
        }

        private void TextBoxPartFilter_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            // capture all the keys in the text box, such as CTRL+C or CTRL+A to avoid these
            // key to be executed at the map level (the MainForm level) and avoid modification
            // of the map (copy/paste, etc..) while the filter box is focused
            e.IsInputKey = true;
        }

        private void TextBoxPartFilter_KeyDown(object sender, KeyEventArgs e)
        {
            // it seems the CTRL+A is not handled by default by the text box control??
            if (!e.Control || e.KeyCode != Keys.A)
            {
                return;
            }

            textBoxPartFilter.SelectAll();
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void FilterAllTabCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            // get the filter sentence that can be empty if the user didn't set one
            var filterSentence = GetUserFilterSentence();

            // change the icon of the button according to the button state
            if (filterAllTabCheckBox.Checked)
            {
                filterAllTabCheckBox.ImageIndex = 0;
                PartsTabControl.filterAllTabs(filterSentence);
                return;
            }

            filterAllTabCheckBox.ImageIndex = 1;
            // after refiltering all tabs with their own filter, filter the current tab
            // with the current filter text of the combo box. because we want this behavior:
            // 1) select tab A, filter with "A"
            // 2) select tab B, filter with "B"
            // 3) select tab C, filter with "C"
            // 3) hit the filter all checkbox: now all tabs are filtered with "C"
            // 4) select tab A (which is filetered with "C" as expected, with the filter text set to "C")
            // 5) uncheck the filter all
            // 6) we want to keep the filtering of A with "C" (and tab B is with "B" and tab C with "C")
            // another possible behavior would be to update the filter text with the filter sentence of the
            // current tab: in that case, the user would see the text change when he uncheck the filter all checkbox
            // so actually we give in parameter the filter sentence to use for the current tab
            PartsTabControl.unfilterAllTabs(filterSentence);
        }
        #endregion

        #region event handler for layers
        private void LayerUpButton_Click(object sender, EventArgs e)
        {
            if (Map.Instance.SelectedLayer == null)
            {
                return;
            }

            var index = Map.Instance.GetIndexOf(Map.Instance.SelectedLayer);
            if (index >= Map.Instance.NumLayers - 1)
            {
                return;
            }

            ActionManager.Instance.doAction(new MoveLayerUp(Map.Instance.SelectedLayer));
        }

        private void LayerDownButton_Click(object sender, EventArgs e)
        {
            if (Map.Instance.SelectedLayer == null)
            {
                return;
            }

            var index = Map.Instance.GetIndexOf(Map.Instance.SelectedLayer);
            if (index <= 0)
            {
                return;
            }

            ActionManager.Instance.doAction(new MoveLayerDown(Map.Instance.SelectedLayer));
        }

        private void NewLayerGridButton_Click(object sender, EventArgs e)
        {
            ActionManager.Instance.doAction(new AddLayer(nameof(LayerGrid), true));
        }

        private void NewLayerBrickButton_Click(object sender, EventArgs e)
        {
            ActionManager.Instance.doAction(new AddLayer(nameof(LayerBrick), true));
        }

        private void NewLayerAreaButton_Click(object sender, EventArgs e)
        {
            ActionManager.Instance.doAction(new AddLayer(nameof(LayerArea), true));
        }

        private void NewLayerTextButton_Click(object sender, EventArgs e)
        {
            ActionManager.Instance.doAction(new AddLayer(nameof(LayerText), true));
        }

        private void NewLayerRulerButton_Click(object sender, EventArgs e)
        {
            ActionManager.Instance.doAction(new AddLayer(nameof(LayerRuler), true));
        }

        private void TrashLayerButton_Click(object sender, EventArgs e)
        {
            if (Map.Instance.SelectedLayer == null)
            {
                return;
            }

            ActionManager.Instance.doAction(new RemoveLayer(Map.Instance.SelectedLayer));
        }

        #endregion

        #region event handler for part usage list tab
        private void SplitPartUsagePerLayerCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            // change the status of the split status
            PartUsageListView.SplitPartPerLayer = SplitPartUsagePerLayerCheckBox.Checked;
        }

        private void IncludeHiddenLayerInPartListCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            // change the status of the hidden status
            PartUsageListView.IncludeHiddenLayers = IncludeHiddenLayerInPartListCheckBox.Checked;
        }
        #endregion

        #region event handler for properties tab
        private ChangeGeneralInfo mGeneralInfoStateWhenEnteringFocus = null;

        private void DocumentDataTabControl_Selected(object sender, TabControlEventArgs e)
        {
            // update the map dimension if we just selected the properties tab
            if (e.TabPage == DocumentDataPropertiesTabPage)
            {
                UpdateMapDimensionInfo();
            }
        }

        private void DocumentDataPropertiesMapBackgroundColorButton_Click(object sender, EventArgs e)
        {
            OpenColorPickerToChangeMapBackgroundColor();
        }

        private ChangeGeneralInfo GetGeneralInfoActionFromUI()
        {
            return new ChangeGeneralInfo(
                AuthorTextBox.Text,
                lugComboBox.Text,
                eventComboBox.Text,
                dateTimePicker.Value,
                commentTextBox.Text
            );
        }

        private void MemorizeGeneralInfoActionWhenEnteringFocus()
        {
            mGeneralInfoStateWhenEnteringFocus = GetGeneralInfoActionFromUI();
        }

        private void DoChangeGeneralInfoActionIfSomethingChangedInUI()
        {
            // create a new action from UI
            var newAction = GetGeneralInfoActionFromUI();
            // check if the new action is diffent than we enter the focus, add it to the action manager
            if (!newAction.Equals(mGeneralInfoStateWhenEnteringFocus))
            {
                ActionManager.Instance.doAction(newAction);
            }
        }

        private void AuthorTextBox_Enter(object sender, EventArgs e)
        {
            MemorizeGeneralInfoActionWhenEnteringFocus();
        }

        private void AuthorTextBox_Leave(object sender, EventArgs e)
        {
            DoChangeGeneralInfoActionIfSomethingChangedInUI();
        }

        private void AuthorTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void LugComboBox_Enter(object sender, EventArgs e)
        {
            MemorizeGeneralInfoActionWhenEnteringFocus();
        }

        private void LugComboBox_Leave(object sender, EventArgs e)
        {
            DoChangeGeneralInfoActionIfSomethingChangedInUI();
        }

        private void LugComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void EventComboBox_Enter(object sender, EventArgs e)
        {
            MemorizeGeneralInfoActionWhenEnteringFocus();
        }

        private void EventComboBox_Leave(object sender, EventArgs e)
        {
            DoChangeGeneralInfoActionIfSomethingChangedInUI();
        }

        private void EventComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void DateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            DoChangeGeneralInfoActionIfSomethingChangedInUI();
        }

        private void CommentTextBox_Enter(object sender, EventArgs e)
        {
            MemorizeGeneralInfoActionWhenEnteringFocus();
        }

        private void CommentTextBox_Leave(object sender, EventArgs e)
        {
            DoChangeGeneralInfoActionIfSomethingChangedInUI();
        }

        #endregion

        #region event handler for map
        private void MainForm_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!mapPanel.Focused)
            {
                return;
            }

            // convert the coord of the mouse position into client area
            var clientCoord = TopLevelControl.PointToScreen(e.Location);
            clientCoord = mapPanel.PointToClient(clientCoord);
            // recreate a new event for the client and call the client
            mapPanel.MapPanel_MouseWheel(
                sender,
                new MouseEventArgs(
                    e.Button,
                    e.Clicks,
                    clientCoord.X,
                    clientCoord.Y,
                    e.Delta
                )
            );
        }

        /// <summary>
        /// Move the currently selected object from the specified value
        /// </summary>
        /// <param name="isRealMove">tell if it is a real move that must be recorded in the undo stack, or just an update</param>
        /// <param name="move">an incremental move if this is not a real move, else the total move</param>
        private void MoveSelectedObjects(bool isRealMove, PointF move)
        {
            // check if there's nothing to move
            if (move.X == 0.0f && move.Y == 0.0f)
            {
                return;
            }

            // first get the current selected layer
            var selectedLayer = Map.Instance.SelectedLayer;
            if (selectedLayer == null || selectedLayer.SelectedObjects == null || selectedLayer.SelectedObjects.Count == 0)
            {
                return;
            }

            // create the action according to the layer type
            Actions.Action moveAction = null;

            if (selectedLayer is LayerBrick layerBrick)
            {
                moveAction = new MoveBrick(layerBrick, selectedLayer.SelectedObjects, move);
            }
            else if (selectedLayer is LayerText layerText)
            {
                moveAction = new MoveText(layerText, selectedLayer.SelectedObjects, move);
            }
            else if (selectedLayer is LayerRuler layerRuler)
            {
                moveAction = new MoveRulers(layerRuler, selectedLayer.SelectedObjects, move);
            }

            // if we found a compatible layer
            if (moveAction == null)
            {
                return;
            }

            if (isRealMove)
            {
                // undo the action
                moveAction.Undo();
                // then add it to the undo stack (that will perform the redo)
                ActionManager.Instance.doAction(moveAction);
            }
            else
            {
                // do a move action without puting it in the undo stack
                moveAction.Redo();
            }
        }

        /// <summary>
        /// Rotate the currently selected object from the specified value
        /// </summary>
        /// <param name="isRealMove">tell if it is a real rotation that must be recorded in the undo stack, or just an update</param>
        /// <param name="move">an incremental angle to rotate if this is not a real move, else the total angle</param>
        private void RotateSelectedObjects(bool isRealMove, int angleStep)
        {
            // check if there's nothing to rotate
            if (angleStep == 0)
            {
                return;
            }

            // first get the current selected layer
            var selectedLayer = Map.Instance.SelectedLayer;
            if (selectedLayer == null || selectedLayer.SelectedObjects == null || selectedLayer.SelectedObjects.Count == 0)
            {
                return;
            }

            if (selectedLayer is LayerBrick layerBrick)
            {
                if (isRealMove)
                {
                    // create the opposite action and do it, to cancel all the incremental moves
                    // we can not create the normal action and undo it because the rotation of connected
                    // brick is not symetrical (because the rotation step is not constant)
                    var unrotateAction = new RotateBrick(layerBrick, Map.Instance.SelectedLayer.SelectedObjects, -angleStep, true);
                    unrotateAction.Redo();
                    // So create a new move action to add in the undo stack
                    ActionManager.Instance.doAction(new RotateBrick(layerBrick, Map.Instance.SelectedLayer.SelectedObjects, angleStep, true));
                }
                else
                {
                    // do a move action without puting it in the undo stack
                    var rotateAction = new RotateBrick(layerBrick, Map.Instance.SelectedLayer.SelectedObjects, angleStep, (angleStep != -1) && (angleStep != 1));
                    rotateAction.Redo();
                }
            }
            else if (selectedLayer is LayerText layerText)
            {
                if (isRealMove)
                {
                    // create the rotation action
                    var rotateAction = new RotateText(layerText, Map.Instance.SelectedLayer.SelectedObjects, angleStep, true);
                    // undo the total rotate of all the objects
                    rotateAction.Undo();
                    // then add it to the undo stack (that will perform the redo)
                    ActionManager.Instance.doAction(rotateAction);
                }
                else
                {
                    // do a move action without puting it in the undo stack
                    var rotateAction = new RotateText(layerText, Map.Instance.SelectedLayer.SelectedObjects, angleStep, (angleStep != -1) && (angleStep != 1));
                    rotateAction.Redo();
                }
            }
            else if (selectedLayer is LayerRuler layerRuler)
            {
                if (isRealMove)
                {
                    // create the rotation action
                    var rotateAction = new RotateRulers(layerRuler, Map.Instance.SelectedLayer.SelectedObjects, angleStep, true);
                    // undo the total rotate of all the objects
                    rotateAction.Undo();
                    // then add it to the undo stack (that will perform the redo)
                    ActionManager.Instance.doAction(rotateAction);
                }
                else
                {
                    // do a move action without puting it in the undo stack
                    var rotateAction = new RotateRulers(layerRuler, Map.Instance.SelectedLayer.SelectedObjects, angleStep, (angleStep != -1) && (angleStep != 1));
                    rotateAction.Redo();
                }
            }
        }
        #endregion

        #region event handler for drag and drop file
        public void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            // by default do not accept the drop
            e.Effect = DragDropEffects.None;

            // we accept the drop if the data is a file name
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            // check if the first file has a valid suported extension
            var filenames = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (filenames == null || filenames.Length == 0)
            {
                return;
            }

            // authorize the drop if it's a file with the good extension
            if (CanOpenThisFile(filenames[0]))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        public void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            var filenames = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (filenames == null || filenames.Length == 0)
            {
                return;
            }

            // check if the current map is not save and display a warning message
            if (CheckForUnsavedMap())
            {
                OpenMap(filenames[0]);
            }
        }
        #endregion

        // we use an array to remap the keys from the index in the dropdown list of the option page to the real key code
        private static readonly Keys[] KeyRemap = new[]
            {
                Keys.A, Keys.B, Keys.C, Keys.D, Keys.E, Keys.F, Keys.G, Keys.H, Keys.I, Keys.J, Keys.K,
                Keys.L, Keys.M, Keys.N, Keys.O, Keys.P, Keys.Q, Keys.R, Keys.S, Keys.T, Keys.U, Keys.V,
                Keys.W, Keys.X, Keys.Y, Keys.Z, Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5,
                Keys.D6, Keys.D7, Keys.D8, Keys.D9, Keys.Space, Keys.NumPad0, Keys.NumPad1, Keys.NumPad2,
                Keys.NumPad3, Keys.NumPad4, Keys.NumPad5, Keys.NumPad6, Keys.NumPad7, Keys.NumPad8, Keys.NumPad9,
                Keys.Divide, Keys.Multiply, Keys.Subtract, Keys.Add, Keys.Decimal, Keys.F1, Keys.F2, Keys.F3,
                Keys.F4, Keys.F5, Keys.F6, Keys.F7, Keys.F8, Keys.F9, Keys.F10, Keys.F11, Keys.F12,
                Keys.Escape, Keys.Back, Keys.Return, Keys.Left, Keys.Right, Keys.Up, Keys.Down, Keys.Insert,
                Keys.Delete, Keys.Home, Keys.End, Keys.PageUp, Keys.PageDown
            };

        #region Keyboard shortcut
        private void InitShortcutKeyArrayFromSettings()
        {
            // recreate a new array
            var actionMaxCount = (int)ShortcutableAction.NB_ACTIONS;

            mShortcutKeys = new List<KeyAndPart>[actionMaxCount];

            int actionIndex;

            for (actionIndex = 0; actionIndex < actionMaxCount; ++actionIndex)
            {
                mShortcutKeys[actionIndex] = new List<KeyAndPart>();
            }

            // iterate on the settings list
            var separators = new[] { '|' };
            string[] itemNames;
            int keyIndex, connexion;

            for (var i = 0; i < Properties.Settings.Default.ShortcutKey.Count; i++)
            {
                try
                {
                    itemNames = Properties.Settings.Default.ShortcutKey[i].Split(separators);
                    keyIndex = int.Parse(itemNames[0]);
                    actionIndex = int.Parse(itemNames[1]);
                    connexion = actionIndex == 0 ? int.Parse(itemNames[3]) : 0;

                    // add the key to the corresponding array
                    mShortcutKeys[actionIndex].Add(new KeyAndPart(KeyRemap[keyIndex], itemNames[2], connexion));
                }
                catch { }
            }
        }

        private bool IsUserTypingTextInATextBox()
        {
            return textBoxPartFilter.Focused
                || PartsTabControl.IsEditingBudget
                || AuthorTextBox.Focused
                || lugComboBox.Focused
                || eventComboBox.Focused
                || commentTextBox.Focused;
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            // by default we don't handle the keys
            e.Handled = false;

            // if we are inputing text in the filter box do not handle the key
            if (IsUserTypingTextInATextBox())
            {
                return;
            }

            // if any modifier is pressed, we don't handle the key, for example the CTRL+S will be handle
            // by the shortcut of the "Save" menu item
            if (e.Alt || e.Control || e.Shift)
            {
                // a modifier is pressed, we start to ignore all the shortcut until all the key of the
                // keyboard are released.
                mModifierWasPressedIgnoreCustomShortcut = true;

                // if a modifier is just pressed we will warn the mapPanel in case it wants to change the cursor
                // we need to check if the modifier changed because of the auto repeat key down event
                // if you keep pressing a keep
                if (mLastModifierKeyDown != e.Modifiers)
                {
                    mLastModifierKeyDown = e.Modifiers;
                    mapPanel.SetDefaultCursor();
                }
                return;
            }

            // check if we need to ignore the custom shortcut
            if (mModifierWasPressedIgnoreCustomShortcut)
            {
                return;
            }

            // get the current value of the grid step in case of we need to move the selected objects
            var moveSize = !Layer.SnapGridEnabled ? 0.1f : Layer.CurrentSnapGridSize;

            int i;
            KeyAndPart shortcut;

            // iterate on all the possible actions
            for (ShortcutableAction actionIndex = 0; actionIndex < ShortcutableAction.NB_ACTIONS; ++actionIndex)
            {
                // iterate on all the shortcut key for the current action
                for (i = 0; i < mShortcutKeys[(int)actionIndex].Count; i++)
                {
                    shortcut = mShortcutKeys[(int)actionIndex][i];

                    // check the key to see if we handle it
                    if (e.KeyCode != shortcut.mKeyCode)
                    {
                        continue;
                    }

                    // we handle this key
                    e.Handled = true;

                    // more stuff to init for some specific actions
                    switch (actionIndex)
                    {
                        case ShortcutableAction.ADD_PART:
                            {
                                if (!(Map.Instance.SelectedLayer is LayerBrick brickLayer))
                                {
                                    break;
                                }

                                // add a connected brick, or if there's no connectable brick, add a brick in the origin
                                if (brickLayer.getConnectableBrick() != null)
                                {
                                    Map.Instance.AddConnectBrick(shortcut.mPartName, shortcut.mConnexion);
                                }
                                else
                                {
                                    Map.Instance.AddBrick(shortcut.mPartName);
                                }
                                break;
                            }
                        case ShortcutableAction.DELETE_PART:
                            // shortcut to the event handler of the menu
                            DeleteToolStripMenuItem_Click(sender, e);
                            break;
                        case ShortcutableAction.ROTATE_LEFT:
                            // set the flag
                            mIsRotateLeftDown = true;
                            // modify the move vector
                            mObjectTotalStepToRotate--;
                            // add a fake rotation for updating the view
                            RotateSelectedObjects(false, -1);
                            break;
                        case ShortcutableAction.ROTATE_RIGHT:
                            // set the flag
                            mIsRotateRightDown = true;
                            // modify the move vector
                            mObjectTotalStepToRotate++;
                            // add a fake rotation for updating the view
                            RotateSelectedObjects(false, 1);
                            break;
                        case ShortcutableAction.MOVE_LEFT:
                            // set the flag
                            mIsLeftArrowDown = true;
                            // modify the move vector
                            mObjectTotalMove.X -= moveSize;
                            // add a fake move for updating the view
                            MoveSelectedObjects(false, new PointF(-moveSize, 0));
                            break;
                        case ShortcutableAction.MOVE_RIGHT:
                            // set the flag
                            mIsRightArrowDown = true;
                            // modify the move vector
                            mObjectTotalMove.X += moveSize;
                            // add a fake move for updating the view
                            MoveSelectedObjects(false, new PointF(moveSize, 0));
                            break;
                        case ShortcutableAction.MOVE_UP:
                            // set the flag
                            mIsUpArrowDown = true;
                            // modify the move vector
                            mObjectTotalMove.Y -= moveSize;
                            // add a fake move for updating the view
                            MoveSelectedObjects(false, new PointF(0, -moveSize));
                            break;
                        case ShortcutableAction.MOVE_DOWN:
                            // set the flag
                            mIsDownArrowDown = true;
                            // modify the move vector
                            mObjectTotalMove.Y += moveSize;
                            // add a fake move for updating the view
                            MoveSelectedObjects(false, new PointF(0, moveSize));
                            break;
                        case ShortcutableAction.CHANGE_CURRENT_CONNEXION:
                            {
                                if (!(Map.Instance.SelectedLayer is LayerBrick brickLayer))
                                {
                                    break;
                                }

                                var selectedBrick = brickLayer.getConnectableBrick();
                                if (selectedBrick != null)
                                {
                                    selectedBrick.setActiveConnectionPointWithNextOne(true);
                                    mapPanel.UpdateView();
                                }
                                break;
                            }
                        case ShortcutableAction.SEND_TO_BACK:
                            // shortcut to the event handler of the menu
                            SendToBackToolStripMenuItem_Click(sender, e);
                            break;
                        case ShortcutableAction.BRING_TO_FRONT:
                            // shortcut to the event handler of the menu
                            BringToFrontToolStripMenuItem_Click(sender, e);
                            break;
                    }

                    // we need to force the refresh of the map immediatly because the invalidate
                    // is not fast enough compare to the repeat key event.
                    mapPanel.Refresh();

                    // and just return, because don't need to search more keys
                    return;
                }
            }
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            // we will try to handle the key, but there are several case for which 
            // we don't handle them
            e.Handled = true;

            // if we are inputing text in the filter box do not handle the key
            if (IsUserTypingTextInATextBox())
            {
                return;
            }

            // We will warn the mapPanel if a modifier is released in case it wants to change the cursor
            var wasModifierReleased = mLastModifierKeyDown != e.Modifiers;
            if (wasModifierReleased)
            {
                // save the new modifier state
                mLastModifierKeyDown = e.Modifiers;
                // and change the cursor of the panel
                mapPanel.SetDefaultCursor();
                // a modifier was released anyway, we don't handle the key
                e.Handled = false;
            }

            // if any modifier is pressed, we don't handle the key released,
            // for example the CTRL+S will be handle by the shortcut of the "Save" menu item
            // when the S is released. Than means you can releas any keys you want if one
            // modifier is pressed, we ignore all of them
            if (e.Alt || e.Control || e.Shift)
            {
                e.Handled = false;
            }

            // if we still need to ignore the shortcut, because one modifier was pressed before,
            // the user must release all the keys before we start to handle the normal shortcut again
            if (mModifierWasPressedIgnoreCustomShortcut)
            {
                // if the modifier was pressed, but now it is another key which is released, we can reset
                // the flag, because we assume the other key was pressed during the modifier down
                // unfortunatly this is only working with one normal key pressed with a modifier,
                // if you press two normal key with a modifier, and release the modifier first, the
                // first released normal key will be skiped but not the second one.
                if (!wasModifierReleased)
                {
                    mModifierWasPressedIgnoreCustomShortcut = false;
                }

                e.Handled = false;
            }

            // if we don't handle the key just return
            if (!e.Handled)
            {
                return;
            }

            // now reset the handle flag to false and we try to find if we handle that key
            e.Handled = false;

            int i;
            KeyAndPart shortcut;
            bool mustMoveObject, mustRotateObject;

            // iterate on all the possible actions
            for (ShortcutableAction actionIndex = 0; actionIndex < ShortcutableAction.NB_ACTIONS; ++actionIndex)
            {
                // iterate on all the shortcut key for the current action
                for (i = 0; i < mShortcutKeys[(int)actionIndex].Count; i++)
                {
                    shortcut = mShortcutKeys[(int)actionIndex][i];

                    // check the key to see if we handle it
                    if (e.KeyCode != shortcut.mKeyCode)
                    {
                        continue;
                    }

                    // we handle this key
                    e.Handled = true;

                    // a boolean to check if we must move the objects
                    mustMoveObject = false;
                    mustRotateObject = false;

                    // more stuff to init for some specific actions
                    switch (actionIndex)
                    {
                        case ShortcutableAction.ROTATE_LEFT:
                            mIsRotateLeftDown = false;
                            mustRotateObject = !mIsRotateRightDown;
                            break;
                        case ShortcutableAction.ROTATE_RIGHT:
                            mIsRotateRightDown = false;
                            mustRotateObject = !mIsRotateLeftDown;
                            break;
                        case ShortcutableAction.MOVE_LEFT:
                            mIsLeftArrowDown = false;
                            mustMoveObject = !(mIsRightArrowDown || mIsDownArrowDown || mIsUpArrowDown);
                            break;
                        case ShortcutableAction.MOVE_RIGHT:
                            mIsRightArrowDown = false;
                            mustMoveObject = !(mIsLeftArrowDown || mIsDownArrowDown || mIsUpArrowDown);
                            break;
                        case ShortcutableAction.MOVE_UP:
                            mIsUpArrowDown = false;
                            mustMoveObject = !(mIsLeftArrowDown || mIsRightArrowDown || mIsDownArrowDown);
                            break;
                        case ShortcutableAction.MOVE_DOWN:
                            mIsDownArrowDown = false;
                            mustMoveObject = !(mIsLeftArrowDown || mIsRightArrowDown || mIsUpArrowDown);
                            break;
                    }

                    // check if one of these four keys are still pressed
                    if (mustMoveObject)
                    {
                        // move the object with the specified vector
                        MoveSelectedObjects(true, mObjectTotalMove);
                        // reset the move object
                        mObjectTotalMove.X = 0;
                        mObjectTotalMove.Y = 0;
                    }

                    // check if one of the two rotate key are still pressed, else do the real rotation
                    if (mustRotateObject)
                    {
                        // rotate the selected parts
                        RotateSelectedObjects(true, mObjectTotalStepToRotate);
                        // reset the total rotate angle
                        mObjectTotalStepToRotate = 0;
                    }

                    // and just return, because don't need to search more keys
                    return;
                }
            }
        }
        #endregion

        #region function related to parts library
        public string GetDraggingPartNumberInPartLib()
        {
            return PartsTabControl.DraggingPartNumber;
        }

        public void ResetDraggingPartNumberInPartLib()
        {
            PartsTabControl.DraggingPartNumber = null;
        }
        #endregion
    }
}