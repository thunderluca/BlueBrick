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
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using BlueBrick.Properties;
using BlueBrick.Actions;
using BlueBrick.Actions.Maps;
using BlueBrick.MapData;
using System.Xml.Serialization;
using System.IO;

namespace BlueBrick
{
	public partial class PreferencesForm : Form
	{
		private enum ColorScheme
		{
			CUSTOM = 0,
			BLUEPRINT,
			PARCHMENT,
			CLASSIC,
		};

		[Flags]
		private enum TabPageFilter
		{
			GENERAL = 1,
			EDITION = 2,
			APPEARANCE = 4,
			PART_LIBRARY = 8,
			SHORTCUT_KEYS = 16,
			ALL = GENERAL | EDITION | APPEARANCE | PART_LIBRARY | SHORTCUT_KEYS,
		};

		// save the old value of the setting to restore the old value after a click on "Reset Default Settings" + "Cancel"
		private readonly Settings mOldSettings = new Settings();

		// a flag to check if the user changed the part lib order
		private bool mHasPartLibOrderChanged = false;

		//save the last sorted column for the shortcut list
		private int mGlobalStatsLastColumnSorted = -1;

		// flag for the main application
		private bool mDoesNeedToRestart = false;

		// a flag to tell if the user has set the new map template file name and the budget file name
		private bool mIsTemplateFilenameForNewMapSet = Settings.Default.TemplateFilenameWhenCreatingANewMap != string.Empty;
		private bool mIsBudgetFilenameToLoadAtStartupSet = Settings.Default.BudgetFilenameToLoadAtStartup != string.Empty;

		#region properties
		public bool DoesNeedToRestart
		{
			get { return mDoesNeedToRestart; }
		}
		#endregion

		#region init / close
		public static void sSaveDefaultKeyInSettings()
		{
			sSaveDefaultKeyInSettings(Settings.Default, true);
		}

		private static void sSaveDefaultKeyInSettings(Settings settings, bool saveOnDisk)
		{
			// PATCH FIX BECAUSE DOT NET FRAMEWORK IS BUGGED
			// Indeed the "Ctrl" string doesn't not exist on german OS (the key name is "Strg"),
			// so the default setting fail to load if you put a CTRL as default key in the default
			// Setting. So instead we save the default key here if the default key is not saved
			// and then after the application will be able to reload correctly the settings
			bool needToSave = false;

			if (settings.MouseMultipleSelectionKey == Keys.None)
			{
				settings.MouseMultipleSelectionKey = Keys.Control;
				needToSave = true;
			}
			if (settings.MouseDuplicateSelectionKey == Keys.None)
			{
				settings.MouseDuplicateSelectionKey = Keys.Alt;
				needToSave = true;
			}
			if (settings.MouseZoomPanKey == Keys.None)
			{
				settings.MouseZoomPanKey = Keys.Shift;
				needToSave = true;
			}

			// try to save (never mind if we can not (for example BlueBrick is launched
			// from a write protected drive)
			try
			{
				if (saveOnDisk && needToSave)
					settings.Save();
			}
			catch
			{
			}
		}

		public PreferencesForm()
		{
			InitializeComponent();
			initControlValues(false, TabPageFilter.ALL);
			// save the old settings
			copySettings(mOldSettings, Settings.Default, TabPageFilter.ALL);
		}

		private void initControlValues(bool isForResetingDefaultSetting, TabPageFilter tabPageFilter)
		{
			// init the controls

			// -- tab general
			if ((tabPageFilter & TabPageFilter.GENERAL) != 0)
			{
				// language
				fillAndSelectLanguageComboBox();
				// new map template
				setTextBoxForTemplateFilenameForNewMap(Settings.Default.TemplateFilenameWhenCreatingANewMap);
				// recent files
				RecentFilesNumericUpDown.Value = Settings.Default.MaxRecentFilesNum;
				clearRecentFilesButton.Enabled = Settings.Default.RecentFiles.Count > 0;
				// undo
				undoRecordedNumericUpDown.Value = Settings.Default.UndoStackDepth;
				undoDisplayedNumericUpDown.Value = Settings.Default.UndoStackDisplayedDepth;
				// notification
				GeneralCheckedListBoxNotification.SetItemChecked(0, Settings.Default.DisplayWarningMessageForNotSavingInBBM);
				GeneralCheckedListBoxNotification.SetItemChecked(1, Settings.Default.DisplayWarningMessageForOverridingExportFiles);
				GeneralCheckedListBoxNotification.SetItemChecked(2, Settings.Default.DisplayWarningMessageForBrickNotAddedDueToBudgetLimitation);
				GeneralCheckedListBoxNotification.SetItemChecked(3, Settings.Default.DisplayWarningMessageForBrickNotCopiedDueToBudgetLimitation);
				GeneralCheckedListBoxNotification.SetItemChecked(4, Settings.Default.DisplayWarningMessageForBrickNotReplacedDueToBudgetLimitation);
				GeneralCheckedListBoxNotification.SetItemChecked(5, Settings.Default.DisplayWarningMessageForShowingBudgetNumbers);
				GeneralCheckedListBoxNotification.SetItemChecked(6, Settings.Default.DisplayWarningMessageForPastingOnWrongLayer);
				// performance
				optimComboBox.SelectedIndex = Settings.Default.StartSavedMipmapLevel;
			}

			// -- tab edition
			if ((tabPageFilter & TabPageFilter.EDITION) != 0)
			{
				// mouse
				mouseZoomCenteredCheckBox.Checked = Settings.Default.WheelMouseIsZoomOnCursor;
				mouseZoomSpeedNumericUpDown.Value = (Decimal)Settings.Default.WheelMouseZoomSpeed;
				fillAndSelectModifierKeyComboBox();
				// copy/paste
				copyOffsetComboBox.SelectedIndex = Settings.Default.OffsetAfterCopyStyle;
				pasteOffsetValueNumericUpDown.Value = (Decimal)Settings.Default.OffsetAfterCopyValue;
				bool enableOffsetValue = Settings.Default.OffsetAfterCopyStyle != 0;
				pasteOffsetValueNumericUpDown.Enabled = enableOffsetValue;
				OffsetValueLabel.Enabled = enableOffsetValue;
				// ruler
				rulerControlPointRadiusNumericUpDown.Value = Settings.Default.RulerControlPointRadiusInPixel;
				RulerSwitchToEditionAfterCreationCheckBox.Checked = Settings.Default.SwitchToEditionAfterRulerCreation;
				// line appearance
				lineThicknessNumericUpDown.Value = (Decimal)Settings.Default.RulerDefaultLineThickness;
				lineColorPictureBox.BackColor = Settings.Default.RulerDefaultLineColor;
				allowOffsetCheckBox.Checked = Settings.Default.RulerDefaultAllowOffset;
				// guideline appearance
				dashPatternLineNumericUpDown.Value = (Decimal)Settings.Default.RulerDefaultDashPatternLine;
				dashPatternSpaceNumericUpDown.Value = (Decimal)Settings.Default.RulerDefaultDashPatternSpace;
				guidelineThicknessNumericUpDown.Value = (Decimal)Settings.Default.RulerDefaultGuidelineThickness;
				guidelineColorPictureBox.BackColor = Settings.Default.RulerDefaultGuidelineColor;
				// measure and unit
				displayUnitCheckBox.Checked = Settings.Default.RulerDefaultDisplayUnit;
				displayMeasureTextCheckBox.Checked = Settings.Default.RulerDefaultDisplayMeasureText;
				unitComboBox.SelectedIndex = Settings.Default.RulerDefaultUnit;
				rulerFontColorPictureBox.BackColor = Settings.Default.RulerDefaultFontColor;
				updateChosenFont(rulerFontNameLabel, Settings.Default.RulerDefaultFontColor, Settings.Default.RulerDefaultFont);
			}

			// -- tab appearance
			if ((tabPageFilter & TabPageFilter.APPEARANCE) != 0)
			{
				brickHullColorPictureBox.BackColor = Settings.Default.DefaultHullColor;
				brickHullThicknessNumericUpDown.Value = (Decimal)Settings.Default.DefaultHullThickness;
				backgroundColorPictureBox.BackColor = Settings.Default.DefaultBackgroundColor;
				gridColorPictureBox.BackColor = Settings.Default.DefaultGridColor;
				subGridColorPictureBox.BackColor = Settings.Default.DefaultSubGridColor;
				setGammaToNumericUpDown(GammaForSelectionNumericUpDown, Settings.Default.GammaForSelection);
				setGammaToNumericUpDown(GammaForSnappingNumericUpDown, Settings.Default.GammaForSnappingPart);
				// grid size
				gridSizeNumericUpDown.Value = Math.Min(gridSizeNumericUpDown.Maximum,
                                                            Math.Max(gridSizeNumericUpDown.Minimum, Settings.Default.DefaultGridSize));
				gridSubdivisionNumericUpDown.Value = Math.Min(gridSubdivisionNumericUpDown.Maximum,
                                                            Math.Max(gridSubdivisionNumericUpDown.Minimum, Settings.Default.DefaultSubDivisionNumber));
				gridEnabledCheckBox.Checked = Settings.Default.DefaultGridEnabled;
				subGridEnabledCheckBox.Checked = Settings.Default.DefaultSubGridEnabled;
				// redraw the sample box only after having set the colors
				redrawSamplePictureBox();
				// only set the color scheme after having set the colors
				fillColorSchemeComboBox();
				findCorectColorSchemeAccordingToColors();
				// font
				defaultFontColorPictureBox.BackColor = Settings.Default.DefaultTextColor;
				updateChosenFont(defaultTextFontNameLabel, Settings.Default.DefaultTextColor, Settings.Default.DefaultTextFont);
				// area
				areaTransparencyNumericUpDown.Value = Settings.Default.DefaultAreaTransparency;
				areaCellSizeNumericUpDown.Value = Settings.Default.DefaultAreaSize;
			}

			// -- tab part lib
			if ((tabPageFilter & TabPageFilter.PART_LIBRARY) != 0)
			{
				fillPartLibraryListBox(isForResetingDefaultSetting);
				PartLibBackColorPictureBox.BackColor = Settings.Default.PartLibBackColor;
				partLibBudgetFilterBackColorPictureBox.BackColor = Settings.Default.PartLibShowOnlyBudgetedPartsColor;
				PartLibFilteredBackColorPictureBox.BackColor = Settings.Default.PartLibFilteredBackColor;
				setTextBoxForBudgetFilenameToLoadAtStartup(Settings.Default.BudgetFilenameToLoadAtStartup);
				PartLibDefaultBudgetNotLimitedradioButton.Checked = Settings.Default.IsDefaultBudgetInfinite;
				PartLibDefaultBudgetZeroRadioButton.Checked = !Settings.Default.IsDefaultBudgetInfinite;
				PartLibDisplayRemaingPartCountCheckBox.Checked = Settings.Default.DisplayRemainingPartCountInBudgetInsteadOfUsedCount;
				PartLibDisplayPartInfoCheckBox.Checked = Settings.Default.PartLibDisplayPartInfo;
				PartLibDisplayPartPartIDCheckBox.Checked = Settings.Default.PartLibPartInfoPartID;
				PartLibDisplayPartPartColorCheckBox.Checked = Settings.Default.PartLibPartInfoPartColor;
				PartLibDisplayPartPartDescriptionCheckBox.Checked = Settings.Default.PartLibPartInfoPartDescription;
				PartLibDisplayBubbleInfoCheckBox.Checked = Settings.Default.PartLibDisplayBubbleInfo;
				PartLibDisplayBubblePartIDCheckBox.Checked = Settings.Default.PartLibBubbleInfoPartID;
				PartLibDisplayBubblePartColorCheckBox.Checked = Settings.Default.PartLibBubbleInfoPartColor;
				PartLibDisplayBubblePartDescriptionCheckBox.Checked = Settings.Default.PartLibBubbleInfoPartDescription;
			}

			// -- tab shortcut key
			if ((tabPageFilter & TabPageFilter.SHORTCUT_KEYS) != 0)
			{
				// init the list view
				listViewShortcutKeys.Items.Clear();
				char[] separator = { '|' };
				foreach (string text in Settings.Default.ShortcutKey)
				{
					string[] itemNames = text.Split(separator);
					addShortcutKey(itemNames);
				}
				// fill the part combobox
				comboBoxPartNum.Items.AddRange(BrickLibrary.Instance.GetBrickNameList());
				// init the combobox selections (the selection for connexion is set in the event handler of the selection of part num)
				comboBoxKey.SelectedIndex = 0;
				comboBoxAction.SelectedIndex = 0;
				// test if there's at list one part in the library, else an exception is raised when seting the selected index
				if (comboBoxPartNum.Items.Count > 0)
					comboBoxPartNum.SelectedIndex = 0;
				// add the little sign of the sort order
				// Be careful, do not set mGlobalStatsLastColumnSorted before calling the method,
				// it must be set inside the method
				if (mGlobalStatsLastColumnSorted == -1)
					setSortIcon(0);
				else
					setSortIcon(mGlobalStatsLastColumnSorted);
			}
		}

		private void copySettings(Settings destination, Settings source, TabPageFilter tabPageFilter)
		{
			// general
			if ((tabPageFilter & TabPageFilter.GENERAL) != 0)
			{
				// language
				destination.Language = source.Language.Clone() as string;
				// new map template
				destination.TemplateFilenameWhenCreatingANewMap = source.TemplateFilenameWhenCreatingANewMap;
				// performance
				destination.StartSavedMipmapLevel = source.StartSavedMipmapLevel;
				// recent files
				destination.MaxRecentFilesNum = source.MaxRecentFilesNum;
				// undo/redo
				destination.UndoStackDepth = source.UndoStackDepth;
				destination.UndoStackDisplayedDepth = source.UndoStackDisplayedDepth;
				// Notification
				destination.DisplayWarningMessageForNotSavingInBBM = source.DisplayWarningMessageForNotSavingInBBM;
				destination.DisplayWarningMessageForOverridingExportFiles = source.DisplayWarningMessageForOverridingExportFiles;
				destination.DisplayWarningMessageForBrickNotAddedDueToBudgetLimitation = source.DisplayWarningMessageForBrickNotAddedDueToBudgetLimitation;
				destination.DisplayWarningMessageForBrickNotCopiedDueToBudgetLimitation = source.DisplayWarningMessageForBrickNotCopiedDueToBudgetLimitation;
				destination.DisplayWarningMessageForBrickNotReplacedDueToBudgetLimitation = source.DisplayWarningMessageForBrickNotReplacedDueToBudgetLimitation;
				destination.DisplayWarningMessageForShowingBudgetNumbers = source.DisplayWarningMessageForShowingBudgetNumbers;
				destination.DisplayWarningMessageForPastingOnWrongLayer = source.DisplayWarningMessageForPastingOnWrongLayer;
			}
			// edition
			if ((tabPageFilter & TabPageFilter.EDITION) != 0)
			{
				// mouse
				destination.MouseMultipleSelectionKey = source.MouseMultipleSelectionKey;
				destination.WheelMouseIsZoomOnCursor = source.WheelMouseIsZoomOnCursor;
				destination.MouseZoomPanKey = source.MouseZoomPanKey;
				destination.WheelMouseZoomSpeed = source.WheelMouseZoomSpeed;
				// copy/paste
				destination.OffsetAfterCopyStyle = source.OffsetAfterCopyStyle;
				destination.OffsetAfterCopyValue = source.OffsetAfterCopyValue;
				// ruler
				destination.RulerControlPointRadiusInPixel = source.RulerControlPointRadiusInPixel;
				destination.SwitchToEditionAfterRulerCreation = source.SwitchToEditionAfterRulerCreation;
				// line appearance
				destination.RulerDefaultLineThickness = source.RulerDefaultLineThickness;
				destination.RulerDefaultLineColor = source.RulerDefaultLineColor;
				destination.RulerDefaultAllowOffset = source.RulerDefaultAllowOffset;
				// guideline appearance
				destination.RulerDefaultDashPatternLine = source.RulerDefaultDashPatternLine;
				destination.RulerDefaultDashPatternSpace = source.RulerDefaultDashPatternSpace;
				destination.RulerDefaultGuidelineThickness = source.RulerDefaultGuidelineThickness;
				destination.RulerDefaultGuidelineColor = source.RulerDefaultGuidelineColor;
				// measure and unit
				destination.RulerDefaultDisplayUnit = source.RulerDefaultDisplayUnit;
				destination.RulerDefaultDisplayMeasureText = source.RulerDefaultDisplayMeasureText;
				destination.RulerDefaultUnit = source.RulerDefaultUnit;
				destination.RulerDefaultFontColor = source.RulerDefaultFontColor;
				destination.RulerDefaultFont = source.RulerDefaultFont.Clone() as Font;
			}
			// appearance
			if ((tabPageFilter & TabPageFilter.APPEARANCE) != 0)
			{
				destination.DefaultHullColor = source.DefaultHullColor;
				destination.DefaultHullThickness = source.DefaultHullThickness;
				destination.DefaultBackgroundColor = source.DefaultBackgroundColor;
				destination.DefaultAreaTransparency = source.DefaultAreaTransparency;
				destination.DefaultAreaSize = source.DefaultAreaSize;
				destination.GammaForSelection = source.GammaForSelection;
				destination.GammaForSnappingPart = source.GammaForSnappingPart;
				destination.DefaultGridColor = source.DefaultGridColor;
				destination.DefaultGridSize = source.DefaultGridSize;
				destination.DefaultGridEnabled = source.DefaultGridEnabled;
				destination.DefaultSubDivisionNumber = source.DefaultSubDivisionNumber;
				destination.DefaultSubGridEnabled = source.DefaultSubGridEnabled;
				destination.DefaultSubGridColor = source.DefaultSubGridColor;
				destination.DefaultTextColor = source.DefaultTextColor;
				destination.DefaultTextFont = source.DefaultTextFont.Clone() as Font;
			}
			// part lib
			if ((tabPageFilter & TabPageFilter.PART_LIBRARY) != 0)
			{
				destination.PartLibTabOrder = new System.Collections.Specialized.StringCollection();
				foreach (string text in source.PartLibTabOrder)
					destination.PartLibTabOrder.Add(text.Clone() as string);
				destination.PartLibBackColor = source.PartLibBackColor;
				destination.PartLibShowOnlyBudgetedPartsColor = source.PartLibShowOnlyBudgetedPartsColor;
				destination.PartLibFilteredBackColor = source.PartLibFilteredBackColor;
				destination.BudgetFilenameToLoadAtStartup = source.BudgetFilenameToLoadAtStartup;
				destination.IsDefaultBudgetInfinite = source.IsDefaultBudgetInfinite;
				destination.DisplayRemainingPartCountInBudgetInsteadOfUsedCount = source.DisplayRemainingPartCountInBudgetInsteadOfUsedCount;
				destination.PartLibDisplayPartInfo = source.PartLibDisplayPartInfo;
				destination.PartLibPartInfoPartID = source.PartLibPartInfoPartID;
				destination.PartLibPartInfoPartColor = source.PartLibPartInfoPartColor;
				destination.PartLibPartInfoPartDescription = source.PartLibPartInfoPartDescription;
				destination.PartLibDisplayBubbleInfo = source.PartLibDisplayBubbleInfo;
				destination.PartLibBubbleInfoPartID = source.PartLibBubbleInfoPartID;
				destination.PartLibBubbleInfoPartColor = source.PartLibBubbleInfoPartColor;
				destination.PartLibBubbleInfoPartDescription = source.PartLibBubbleInfoPartDescription;
			}
			// shortcut
			if ((tabPageFilter & TabPageFilter.SHORTCUT_KEYS) != 0)
			{
				destination.ShortcutKey = new System.Collections.Specialized.StringCollection();
				foreach (string text in source.ShortcutKey)
					destination.ShortcutKey.Add(text.Clone() as string);
			}
		}

		private void buttonOk_Click(object sender, EventArgs e)
		{
			// set the values of the controls in the settings
			// -- tab general
			// language
			bool hasLanguageChanged = setLanguageSettingAccordingToComboBox();
			// if the language change, we need to restart the application
			mDoesNeedToRestart = hasLanguageChanged;
			// new map template
			Settings.Default.TemplateFilenameWhenCreatingANewMap = mIsTemplateFilenameForNewMapSet ? GeneralNewMapTemplateFilenameTextBox.Text : string.Empty;
			// recent files
			Settings.Default.MaxRecentFilesNum = (int)RecentFilesNumericUpDown.Value;
			// undo
			Settings.Default.UndoStackDepth = (int)undoRecordedNumericUpDown.Value;
			Settings.Default.UndoStackDisplayedDepth = (int)undoDisplayedNumericUpDown.Value;
			// notification
			Settings.Default.DisplayWarningMessageForNotSavingInBBM = GeneralCheckedListBoxNotification.GetItemChecked(0);
			Settings.Default.DisplayWarningMessageForOverridingExportFiles = GeneralCheckedListBoxNotification.GetItemChecked(1);
			Settings.Default.DisplayWarningMessageForBrickNotAddedDueToBudgetLimitation = GeneralCheckedListBoxNotification.GetItemChecked(2);
			Settings.Default.DisplayWarningMessageForBrickNotCopiedDueToBudgetLimitation =  GeneralCheckedListBoxNotification.GetItemChecked(3);
			Settings.Default.DisplayWarningMessageForBrickNotReplacedDueToBudgetLimitation = GeneralCheckedListBoxNotification.GetItemChecked(4);
			Settings.Default.DisplayWarningMessageForShowingBudgetNumbers = GeneralCheckedListBoxNotification.GetItemChecked(5);
			Settings.Default.DisplayWarningMessageForPastingOnWrongLayer = GeneralCheckedListBoxNotification.GetItemChecked(6);
			// performances
			if (setOptimSettingAccordingToComboBox())
			{
				// set the wait cursor because the recreation of image can take a long time
				Cursor = Cursors.WaitCursor;
				Map.Instance.RecomputeBrickMipmapImages();
				Cursor = Cursors.Default;
			}

			// -- tab edition
			// mouse
			Settings.Default.WheelMouseIsZoomOnCursor = mouseZoomCenteredCheckBox.Checked;
			Settings.Default.WheelMouseZoomSpeed = (double)mouseZoomSpeedNumericUpDown.Value;
			setModifierKeySettingAccordingToComboBox();
			// copy/paste
			Settings.Default.OffsetAfterCopyStyle = copyOffsetComboBox.SelectedIndex;
			Settings.Default.OffsetAfterCopyValue = (float)pasteOffsetValueNumericUpDown.Value;
			// ruler
			Settings.Default.RulerControlPointRadiusInPixel = (int)rulerControlPointRadiusNumericUpDown.Value;
			Settings.Default.SwitchToEditionAfterRulerCreation = RulerSwitchToEditionAfterCreationCheckBox.Checked;
			// line appearance
			Settings.Default.RulerDefaultLineThickness = (float)lineThicknessNumericUpDown.Value;
			Settings.Default.RulerDefaultLineColor = lineColorPictureBox.BackColor;
			Settings.Default.RulerDefaultAllowOffset = allowOffsetCheckBox.Checked;
			// guideline appearance
			Settings.Default.RulerDefaultDashPatternLine = (float)dashPatternLineNumericUpDown.Value;
			Settings.Default.RulerDefaultDashPatternSpace = (float)dashPatternSpaceNumericUpDown.Value;
			Settings.Default.RulerDefaultGuidelineThickness = (float)guidelineThicknessNumericUpDown.Value;
			Settings.Default.RulerDefaultGuidelineColor = guidelineColorPictureBox.BackColor;
			// measure and unit
			Settings.Default.RulerDefaultDisplayUnit = displayUnitCheckBox.Checked;
			Settings.Default.RulerDefaultDisplayMeasureText = displayMeasureTextCheckBox.Checked;
			Settings.Default.RulerDefaultUnit = unitComboBox.SelectedIndex;
			Settings.Default.RulerDefaultFontColor = rulerFontColorPictureBox.BackColor;
			Settings.Default.RulerDefaultFont = rulerFontNameLabel.Font;

			// -- tab appearance
			// hull style
			Settings.Default.DefaultHullColor = brickHullColorPictureBox.BackColor;
			Settings.Default.DefaultHullThickness = (float)brickHullThicknessNumericUpDown.Value;
			// check if the user changed the grid color
			bool doesGridColorChanged = (mOldSettings.DefaultBackgroundColor != backgroundColorPictureBox.BackColor) ||
										(mOldSettings.DefaultGridColor != gridColorPictureBox.BackColor) ||
										(mOldSettings.DefaultSubGridColor != subGridColorPictureBox.BackColor);
			Settings.Default.DefaultBackgroundColor = backgroundColorPictureBox.BackColor;
			Settings.Default.DefaultGridColor = gridColorPictureBox.BackColor;
			Settings.Default.DefaultSubGridColor = subGridColorPictureBox.BackColor;
			Settings.Default.GammaForSelection = getGammaFromNumericUpDown(GammaForSelectionNumericUpDown);
			Settings.Default.GammaForSnappingPart = getGammaFromNumericUpDown(GammaForSnappingNumericUpDown);
			// font
			bool doesFontChanged = (!mOldSettings.DefaultTextFont.Equals(defaultTextFontNameLabel.Font)) ||
									(mOldSettings.DefaultTextColor != defaultFontColorPictureBox.BackColor);
			Settings.Default.DefaultTextFont = defaultTextFontNameLabel.Font;
			Settings.Default.DefaultTextColor = defaultFontColorPictureBox.BackColor;
			// grid size
			bool isSizeModified = (mOldSettings.DefaultGridSize != (int)gridSizeNumericUpDown.Value) ||
									(mOldSettings.DefaultSubDivisionNumber != (int)gridSubdivisionNumericUpDown.Value) ||
									(mOldSettings.DefaultGridEnabled != gridEnabledCheckBox.Checked) ||
									(mOldSettings.DefaultSubGridEnabled != subGridEnabledCheckBox.Checked);
			Settings.Default.DefaultGridSize = (int)gridSizeNumericUpDown.Value;
			Settings.Default.DefaultSubDivisionNumber = (int)gridSubdivisionNumericUpDown.Value;
			Settings.Default.DefaultGridEnabled = gridEnabledCheckBox.Checked;
			Settings.Default.DefaultSubGridEnabled = subGridEnabledCheckBox.Checked;

			//area
			bool doesAreaChanged = (mOldSettings.DefaultAreaTransparency != (int)areaTransparencyNumericUpDown.Value) ||
									(mOldSettings.DefaultAreaSize != (int)areaCellSizeNumericUpDown.Value);
			Settings.Default.DefaultAreaTransparency = (int)areaTransparencyNumericUpDown.Value;
			Settings.Default.DefaultAreaSize = (int)areaCellSizeNumericUpDown.Value;

			// if the grid color changed, we prompt a dialog box to ask if we need to apply the change on all the grid layer
			if (doesGridColorChanged || doesFontChanged || isSizeModified || doesAreaChanged)
			{
				DialogResult result = MessageBox.Show(this,
					Resources.ErrorMsgApplyNewSettingsToAllLayers,
					Resources.ErrorMsgTitleWarning, MessageBoxButtons.YesNo,
					MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
				if (result == DialogResult.Yes)
				{
					ActionManager.Instance.doAction(new ChangeMapAppearance(doesGridColorChanged, doesFontChanged, isSizeModified, doesAreaChanged));
				}				
			}

			// -- tab PartLib
			savePartLibraryTabOrder();
			bool didAppearanceChanged = (mOldSettings.PartLibBackColor != PartLibBackColorPictureBox.BackColor) ||
										(mOldSettings.PartLibShowOnlyBudgetedPartsColor != partLibBudgetFilterBackColorPictureBox.BackColor) ||
										(mOldSettings.PartLibFilteredBackColor != PartLibFilteredBackColorPictureBox.BackColor) ||
										(mOldSettings.PartLibDisplayBubbleInfo != PartLibDisplayBubbleInfoCheckBox.Checked);
			bool didStyleChanged = mOldSettings.PartLibDisplayPartInfo != PartLibDisplayPartInfoCheckBox.Checked;
			bool didBudgetCountChanged = (mOldSettings.IsDefaultBudgetInfinite != PartLibDefaultBudgetNotLimitedradioButton.Checked) ||
										(mOldSettings.DisplayRemainingPartCountInBudgetInsteadOfUsedCount != PartLibDisplayRemaingPartCountCheckBox.Checked) ||
										(mOldSettings.PartLibPartInfoPartID != PartLibDisplayPartPartIDCheckBox.Checked) ||
										(mOldSettings.PartLibPartInfoPartColor != PartLibDisplayPartPartColorCheckBox.Checked) ||
										(mOldSettings.PartLibPartInfoPartDescription != PartLibDisplayPartPartDescriptionCheckBox.Checked);
			bool didBubbleInfoChanged = (mOldSettings.PartLibBubbleInfoPartID != PartLibDisplayBubblePartIDCheckBox.Checked) ||
										(mOldSettings.PartLibBubbleInfoPartColor != PartLibDisplayBubblePartColorCheckBox.Checked) ||
										(mOldSettings.PartLibBubbleInfoPartDescription != PartLibDisplayBubblePartDescriptionCheckBox.Checked);
			Settings.Default.PartLibBackColor = PartLibBackColorPictureBox.BackColor;
			Settings.Default.PartLibShowOnlyBudgetedPartsColor = partLibBudgetFilterBackColorPictureBox.BackColor;
			Settings.Default.PartLibFilteredBackColor = PartLibFilteredBackColorPictureBox.BackColor;
			Settings.Default.BudgetFilenameToLoadAtStartup = mIsBudgetFilenameToLoadAtStartupSet ? PartLibBudgetFilenameTextBox.Text : string.Empty;
			Settings.Default.IsDefaultBudgetInfinite = PartLibDefaultBudgetNotLimitedradioButton.Checked;
			Settings.Default.DisplayRemainingPartCountInBudgetInsteadOfUsedCount = PartLibDisplayRemaingPartCountCheckBox.Checked;
			Settings.Default.PartLibDisplayPartInfo = PartLibDisplayPartInfoCheckBox.Checked;
			Settings.Default.PartLibPartInfoPartID = PartLibDisplayPartPartIDCheckBox.Checked;
			Settings.Default.PartLibPartInfoPartColor = PartLibDisplayPartPartColorCheckBox.Checked;
			Settings.Default.PartLibPartInfoPartDescription = PartLibDisplayPartPartDescriptionCheckBox.Checked;
			Settings.Default.PartLibDisplayBubbleInfo = PartLibDisplayBubbleInfoCheckBox.Checked;
			Settings.Default.PartLibBubbleInfoPartID = PartLibDisplayBubblePartIDCheckBox.Checked;
			Settings.Default.PartLibBubbleInfoPartColor = PartLibDisplayBubblePartColorCheckBox.Checked;
			Settings.Default.PartLibBubbleInfoPartDescription = PartLibDisplayBubblePartDescriptionCheckBox.Checked;
            // call the function on the part lib to reflect the change
            MainForm.Instance.PartsTabControl.updateAppearanceAccordingToSettings(mHasPartLibOrderChanged, didAppearanceChanged, didStyleChanged, didBudgetCountChanged, didBubbleInfoChanged, false, false);

			// -- tab shortcut key
			// save the list view
			Settings.Default.ShortcutKey = new System.Collections.Specialized.StringCollection();
			foreach (ListViewItem item in listViewShortcutKeys.Items)
			{
				string text = item.SubItems[0].Tag + "|" + item.SubItems[1].Tag + "|" + item.SubItems[2].Text + "|" + item.SubItems[3].Text;
				Settings.Default.ShortcutKey.Add(text);
			}

			//save the settings and close the window
			Settings.Default.Save();
		}

		/// <summary>
		/// Reset the settings which belong to the specified page, to the default values
		/// </summary>
		/// <param name="tabPageFilter">A bit field that decribe the page that should be reset</param>
		private void resetSettings(TabPageFilter tabPageFilter)
		{
			// reset the settings (except the language)
			string currentLanguage = Settings.Default.Language;
			// create a new setting object cause we only want to copy the settings of the tab page, not the UI settings
			Settings defaultSetting = new Settings();
			defaultSetting.Upgrade();
			defaultSetting.Reset();
			sSaveDefaultKeyInSettings(defaultSetting, false);
			// restore the language
			defaultSetting.Language = currentLanguage;
			// now copy only the settings specified with the default settings
			copySettings(Settings.Default, defaultSetting, tabPageFilter);
			// init the controls
			initControlValues(true, tabPageFilter);
		}

		private void restoreAllDefaultButton_Click(object sender, EventArgs e)
		{
			DialogResult result = MessageBox.Show(this,
				Resources.ErrorMsgConfirmRestoreDefault,
				Resources.ErrorMsgTitleWarning, MessageBoxButtons.YesNo,
				MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
			if (result == DialogResult.Yes)
				resetSettings(TabPageFilter.ALL);
		}

		private void restoreTabDefaultButton_Click(object sender, EventArgs e)
		{
			DialogResult result = MessageBox.Show(this,
				Resources.ErrorMsgConfirmRestoreDefault,
				Resources.ErrorMsgTitleWarning, MessageBoxButtons.YesNo,
				MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
			if (result == DialogResult.Yes)
				resetSettings((TabPageFilter)(1 << optionsTabControl.SelectedIndex));
		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			// copy back the original setting that may have changed if the user clicked on the restore button
			copySettings(Settings.Default, mOldSettings, TabPageFilter.ALL);
		}

		private void updateChosenFont(Label label, Color color, Font newFont)
		{
			label.Text = newFont.Name + " " + newFont.SizeInPoints.ToString();
			label.Font = newFont;
			label.ForeColor = color;
		}

		/// <summary>
		/// Set the text for the textbox that display the template filename for new map.
		/// If the string is empty, it will display an hint, otherwise it display the value of the parameter
		/// </summary>
		/// <param name="filename">a filename to display or an empty string</param>
		private void setTextBoxForTemplateFilenameForNewMap(string filename)
		{
			if (filename == string.Empty)
			{
				// reload the default sentence in the current language
				System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PreferencesForm));
				resources.ApplyResources(GeneralNewMapTemplateFilenameTextBox, "GeneralNewMapTemplateFilenameTextBox");
				// reset the flag
				mIsTemplateFilenameForNewMapSet = false;
			}
			else
			{
				// set the filename in the text box
				GeneralNewMapTemplateFilenameTextBox.Text = filename;
				// set the flag to true
				mIsTemplateFilenameForNewMapSet = true;
			}
		}

		/// <summary>
		/// Set the text for the textbox that display the budget filename to load at startup.
		/// If the string is empty, it will display an hint, otherwise it display the value of the parameter
		/// </summary>
		/// <param name="filename">a filename to display or an empty string</param>
		private void setTextBoxForBudgetFilenameToLoadAtStartup(string filename)
		{
			if (filename == string.Empty)
			{
				// reload the default sentence in the current language
				System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PreferencesForm));
				resources.ApplyResources(PartLibBudgetFilenameTextBox, "PartLibBudgetFilenameTextBox");
				// reset the flag
				mIsBudgetFilenameToLoadAtStartupSet = false;
			}
			else
			{
				// set the filename in the text box
				PartLibBudgetFilenameTextBox.Text = filename;
				// set the flag to true
				mIsBudgetFilenameToLoadAtStartupSet = true;
			}
		}

		static public void sChangeRulerSettingsFromRuler(LayerRuler.RulerItem rulerItem)
		{
			// line appearance
			Settings.Default.RulerDefaultLineThickness = rulerItem.LineThickness;
			Settings.Default.RulerDefaultLineColor = rulerItem.Color;
			if (rulerItem is LayerRuler.LinearRuler)
				Settings.Default.RulerDefaultAllowOffset = (rulerItem as LayerRuler.LinearRuler).AllowOffset;
			// guideline appearance
			Settings.Default.RulerDefaultDashPatternLine = rulerItem.GuidelineDashPattern[0];
			Settings.Default.RulerDefaultDashPatternSpace = rulerItem.GuidelineDashPattern[1];
			Settings.Default.RulerDefaultGuidelineThickness = rulerItem.GuidelineThickness;
			Settings.Default.RulerDefaultGuidelineColor = rulerItem.GuidelineColor;
			// measure and unit
			Settings.Default.RulerDefaultDisplayUnit = rulerItem.DisplayUnit;
			Settings.Default.RulerDefaultDisplayMeasureText = rulerItem.DisplayDistance;
			Settings.Default.RulerDefaultUnit = (int)rulerItem.CurrentUnit;
			Settings.Default.RulerDefaultFontColor = rulerItem.MeasureColor;
			Settings.Default.RulerDefaultFont = rulerItem.MeasureFont;
		}

		static public void sChangeTextSettingsFromText(LayerText.TextCell textCell)
		{
			Settings.Default.DefaultTextColor = textCell.FontColor;
			Settings.Default.DefaultTextFont = textCell.Font;
		}
		#endregion

		#region tab general

		private string getLanguageStringAccordingToComboBox()
		{
			// return the language string according to the selected index
			if (languageComboBox.SelectedIndex < MainForm.sLanguageCodeAndName.Length)
				return MainForm.sLanguageCodeAndName[languageComboBox.SelectedIndex].mCode;
			// return the default language
			return MainForm.sLanguageCodeAndName[0].mCode;
		}

		private bool setLanguageSettingAccordingToComboBox()
		{
			// get the new language
			string newLanguage = getLanguageStringAccordingToComboBox();
			// set the new language
			Settings.Default.Language = newLanguage;
			// and return true if the language changed
			return !mOldSettings.Language.Equals(newLanguage);
		}

		private void fillAndSelectLanguageComboBox()
		{
			// by default select the first index
			int selectedIndex = 0;

			// add all the language names in the combobox
			languageComboBox.Items.Clear();
			for (int i = 0; i < MainForm.sLanguageCodeAndName.Length; ++i)
			{
				// add the language name in the combobox
				languageComboBox.Items.Add(MainForm.sLanguageCodeAndName[i].mName);
				// check if this is the current selected one
				if (Settings.Default.Language.Equals(MainForm.sLanguageCodeAndName[i].mCode))
					selectedIndex = i;
			}

			// select the correct index
			languageComboBox.SelectedIndex = selectedIndex;
		}

		private void GeneralBrowseNewMapTemplateFileButton_Click(object sender, EventArgs e)
		{
			// set the filename in the dialog from the setting
			if (mIsTemplateFilenameForNewMapSet)
			{
                FileInfo file = new FileInfo(GeneralNewMapTemplateFilenameTextBox.Text);
				openTemplateFileDialog.InitialDirectory = file.DirectoryName;
				openTemplateFileDialog.FileName = file.Name;
			}
			// then open the dialog box in modal
			DialogResult result = openTemplateFileDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				// Try to load the file and copy the style settings like the background colors, etc.. in the appearance settings
				// so that it keeps a certain coherence. However the user may style override the appearance settings if he wants later
				bool isFileValid = loadTemplateFileAndCopyStyleToSettings(openTemplateFileDialog.FileName);
				// set the chosen file name in the text box (if the file is valid, otherwise clear the text box)
				if (isFileValid)
					setTextBoxForTemplateFilenameForNewMap(openTemplateFileDialog.FileName);
				else
					setTextBoxForTemplateFilenameForNewMap(string.Empty);
			}
		}

		private void GeneralTrashTemplateFileForNewMapButton_Click(object sender, EventArgs e)
		{
			setTextBoxForTemplateFilenameForNewMap(string.Empty);
		}

		private void clearRecentFilesButton_Click(object sender, EventArgs e)
		{
			Settings.Default.RecentFiles.Clear();
			clearRecentFilesButton.Enabled = false;
		}

		private bool setOptimSettingAccordingToComboBox()
		{
			Settings.Default.StartSavedMipmapLevel = optimComboBox.SelectedIndex;
			return mOldSettings.StartSavedMipmapLevel != Settings.Default.StartSavedMipmapLevel;
		}

		private bool loadTemplateFileAndCopyStyleToSettings(string filename)
		{
			try
			{
				// create a serializer to load the map
				XmlSerializer mySerializer = new XmlSerializer(typeof(Map));
				FileStream myFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
				// parse and copy the data into this
				Map templateMap = mySerializer.Deserialize(myFileStream) as Map;
				// release the file stream
				myFileStream.Close();
				myFileStream.Dispose();
				// now copy the style of the template file to the Appearance settings
				copyMapStyleToAppearanceSettings(templateMap);
				// file was loaded without problem
				return true;
			}
			catch (Exception e)
			{
				string message = Resources.ErrorMsgCannotOpenMap.Replace("&", filename);
				LoadErrorForm errorMessageDialog = new LoadErrorForm(Resources.ErrorMsgTitleError, message, e.Message);
				errorMessageDialog.ShowDialog();
				// there was a problem loading the file
				return false;
			}
		}

		private void copyMapStyleToAppearanceSettings(Map templateMap)
		{
			// global parameters always present in the Map (back color only so far)
			backgroundColorPictureBox.BackColor = templateMap.BackgroundColor;

			// copy also the grid colors (of the lowest grid, if there's a grid in the template), do not copy other grid params
			foreach (Layer layer in templateMap.LayerList)
				if (layer is LayerGrid)
				{
                    LayerGrid gridLayer = layer as LayerGrid;
					gridColorPictureBox.BackColor = gridLayer.GridColor;
					subGridColorPictureBox.BackColor = gridLayer.SubGridColor;
					break;
				}

			// do not copy text, area and rulers default values
			// then update the preview
			redrawSamplePictureBox();
			colorSchemeComboBox.SelectedIndex = (int)ColorScheme.CUSTOM;
			findCorectColorSchemeAccordingToColors();
		}
		#endregion

		#region edition
		private void lineColorPictureBox_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			colorDialog.Color = lineColorPictureBox.BackColor;
			// open the color box in modal
			DialogResult result = colorDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				lineColorPictureBox.BackColor = colorDialog.Color;
			}
		}

		private void guidelineColorPictureBox_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			colorDialog.Color = guidelineColorPictureBox.BackColor;
			// open the color box in modal
			DialogResult result = colorDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				guidelineColorPictureBox.BackColor = colorDialog.Color;
			}
		}

		private void rulerFontColorPictureBox_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			colorDialog.Color = rulerFontColorPictureBox.BackColor;
			// open the color box in modal
			DialogResult result = colorDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				rulerFontColorPictureBox.BackColor = colorDialog.Color;
				rulerFontNameLabel.ForeColor = colorDialog.Color;
			}
		}

		private void rulerFontButton_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			fontDialog.Font = rulerFontNameLabel.Font;
			// open the color box in modal
			DialogResult result = fontDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				updateChosenFont(rulerFontNameLabel, rulerFontColorPictureBox.BackColor, fontDialog.Font);
			}
		}

		private void setModifierKeySettingAccordingToComboBox()
		{
			// Multiple selection
			switch (mouseMultipleSelKeyComboBox.SelectedIndex)
			{
				case 0: Settings.Default.MouseMultipleSelectionKey = Keys.Control; break;
				case 1: Settings.Default.MouseMultipleSelectionKey = Keys.Alt; break;
				case 2: Settings.Default.MouseMultipleSelectionKey = Keys.Shift; break;
			}

			// Duplicate
			switch (mouseDuplicateSelKeyComboBox.SelectedIndex)
			{
				case 0: Settings.Default.MouseDuplicateSelectionKey = Keys.Control; break;
				case 1: Settings.Default.MouseDuplicateSelectionKey = Keys.Alt; break;
				case 2: Settings.Default.MouseDuplicateSelectionKey = Keys.Shift; break;
			}

			// zoompan
			switch (mousePanViewKeyComboBox.SelectedIndex)
			{
				case 0: Settings.Default.MouseZoomPanKey = Keys.Control; break;
				case 1: Settings.Default.MouseZoomPanKey = Keys.Alt; break;
				case 2: Settings.Default.MouseZoomPanKey = Keys.Shift; break;
			}
		}

		/// <summary>
		/// Return the third value among {0, 1, 2} which is not one of the two specified
		/// </summary>
		/// <param name="firstOne">a value among {0, 1, 2}</param>
		/// <param name="secondOne">a value among {0, 1, 2}</param>
		/// <returns></returns>
		private int getTheThirdOne(int firstOne, int secondOne)
		{
			int thirdOne = 0;

			switch (firstOne)
			{
				case 0: thirdOne = (secondOne == 1) ? 2 : 1; break;
				case 1: thirdOne = (secondOne == 0) ? 2 : 0; break;
				case 2: thirdOne = (secondOne == 1) ? 0 : 1; break;
			}
			return thirdOne;
		}

		private void mousePanViewKeyComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			// avoid to have the multiple selection, the duplicate and the zoompan on the same key
			// we are in the event of the duplicate key, so don't change this one,
			// change one of the two others.
			if (mousePanViewKeyComboBox.SelectedIndex == mouseMultipleSelKeyComboBox.SelectedIndex)
				mouseMultipleSelKeyComboBox.SelectedIndex = getTheThirdOne(mousePanViewKeyComboBox.SelectedIndex, mouseDuplicateSelKeyComboBox.SelectedIndex);
			else
				mouseDuplicateSelKeyComboBox.SelectedIndex = getTheThirdOne(mousePanViewKeyComboBox.SelectedIndex, mouseMultipleSelKeyComboBox.SelectedIndex);
		}

		private void mouseDuplicateSelKeyComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			// avoid to have the multiple selection, the duplicate and the zoompan on the same key
			// we are in the event of the duplicate key, so don't change this one,
			// change one of the two others.
			if (mouseDuplicateSelKeyComboBox.SelectedIndex == mouseMultipleSelKeyComboBox.SelectedIndex)
				mouseMultipleSelKeyComboBox.SelectedIndex = getTheThirdOne(mouseDuplicateSelKeyComboBox.SelectedIndex, mousePanViewKeyComboBox.SelectedIndex);
			else
				mousePanViewKeyComboBox.SelectedIndex = getTheThirdOne(mouseDuplicateSelKeyComboBox.SelectedIndex, mouseMultipleSelKeyComboBox.SelectedIndex);
		}

		private void mouseMultipleSelKeyComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			// avoid to have the multiple selection, the duplicate and the zoompan on the same key
			// we are in the event of the multiple select key, so don't change this one,
			// change one of the two others.
			if (mouseMultipleSelKeyComboBox.SelectedIndex == mouseDuplicateSelKeyComboBox.SelectedIndex)
				mouseDuplicateSelKeyComboBox.SelectedIndex = getTheThirdOne(mouseMultipleSelKeyComboBox.SelectedIndex, mousePanViewKeyComboBox.SelectedIndex);
			else
				mousePanViewKeyComboBox.SelectedIndex = getTheThirdOne(mouseMultipleSelKeyComboBox.SelectedIndex, mouseDuplicateSelKeyComboBox.SelectedIndex);
		}

		private void fillAndSelectModifierKeyComboBox()
		{
			// select the correct index for multiple selection
			switch (Settings.Default.MouseMultipleSelectionKey)
			{
				case Keys.Control: mouseMultipleSelKeyComboBox.SelectedIndex = 0; break;
				case Keys.Alt: mouseMultipleSelKeyComboBox.SelectedIndex = 1; break;
				case Keys.Shift: mouseMultipleSelKeyComboBox.SelectedIndex = 2; break;
			}

			// select the correct index for duplicate
			switch (Settings.Default.MouseDuplicateSelectionKey)
			{
				case Keys.Control: mouseDuplicateSelKeyComboBox.SelectedIndex = 0; break;
				case Keys.Alt: mouseDuplicateSelKeyComboBox.SelectedIndex = 1; break;
				case Keys.Shift: mouseDuplicateSelKeyComboBox.SelectedIndex = 2; break;
			}

			// select the correct index for zoompan
			switch (Settings.Default.MouseZoomPanKey)
			{
				case Keys.Control: mousePanViewKeyComboBox.SelectedIndex = 0; break;
				case Keys.Alt: mousePanViewKeyComboBox.SelectedIndex = 1; break;
				case Keys.Shift: mousePanViewKeyComboBox.SelectedIndex = 2; break;
			}
		}

		private void copyOffsetComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool enableOffsetValue = copyOffsetComboBox.SelectedIndex != 0;
			pasteOffsetValueNumericUpDown.Enabled = enableOffsetValue;
			OffsetValueLabel.Enabled = enableOffsetValue;
		}
		#endregion

		#region tab appearance
		private float getGammaFromNumericUpDown(NumericUpDown control)
		{
			float gamma = (100 - (int)control.Value) / 100.0f;
			if (gamma == 0.0f)
				gamma = 0.01f;
			return gamma;
		}

		private void setGammaToNumericUpDown(NumericUpDown control, float gamma)
		{
			if (gamma == 0.01f)
				gamma = 0.0f;
			gamma = 100 - (int)(gamma * 100.0f);
			control.Value = (Decimal)gamma;
		}

		private void redrawSamplePictureBox()
		{
			// create the image if not already existing
			if (samplePictureBox.Image == null)
				samplePictureBox.Image = new Bitmap(samplePictureBox.Width, samplePictureBox.Height);
			// get the gc of the image
			Graphics graphics = Graphics.FromImage(samplePictureBox.Image);
			// set the background color
			graphics.Clear(backgroundColorPictureBox.BackColor);
			// start corner for the grid
			int startGridCoord = 10;
			// draw some sub-grid lines
			Pen linePen = new Pen(subGridColorPictureBox.BackColor, 1);
			for (int i = startGridCoord - 8; i < samplePictureBox.Height; i += 8)
			{
				graphics.DrawLine(linePen, 0, i, samplePictureBox.Width, i);
				graphics.DrawLine(linePen, i, 0, i, samplePictureBox.Height);
			}
			// draw some grid lines
			linePen = new Pen(gridColorPictureBox.BackColor, 2);
			for (int i = startGridCoord; i < samplePictureBox.Height; i += 32)
			{
				graphics.DrawLine(linePen, 0, i, samplePictureBox.Width, i);
				graphics.DrawLine(linePen, i, 0, i, samplePictureBox.Height);
			}

			// create the image attributes
			ImageAttributes imageAttributeForSelection = new ImageAttributes();
			imageAttributeForSelection.SetGamma(getGammaFromNumericUpDown(GammaForSelectionNumericUpDown));
			ImageAttributes imageAttributeForSnapping = new ImageAttributes();
			imageAttributeForSnapping.SetGamma(getGammaFromNumericUpDown(GammaForSnappingNumericUpDown));

			// get the part example image from the resource
			Image image = Resources.PartForOptionPreview;

			// create the pen do draw the hull around the parts
			Pen penToDrawHull = new Pen(brickHullColorPictureBox.BackColor, (float)brickHullThicknessNumericUpDown.Value);

			// draw a text and its hull
			SizeF textSize = graphics.MeasureString(previewLabel.Text, SystemFonts.DefaultFont);
			Rectangle textRectangle = new Rectangle(20, 10, (int)textSize.Width + 1, (int)textSize.Height + 1);
			graphics.DrawString(previewLabel.Text, SystemFonts.DefaultFont, Brushes.Black, textRectangle);
			graphics.DrawRectangle(penToDrawHull, textRectangle);

			// draw 3 part images as an example
			Rectangle destinationRectangle = new Rectangle(42, 42, image.Width, image.Height);
			graphics.DrawImage(image, destinationRectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
			destinationRectangle.Y += 32;
			graphics.DrawImage(image, destinationRectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributeForSelection);
			destinationRectangle.Y += 32;
			graphics.DrawImage(image, destinationRectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributeForSnapping);
			graphics.DrawRectangle(penToDrawHull, destinationRectangle);

			// invalidate the picture box
			samplePictureBox.Invalidate();
		}

		private void brickHullColorPictureBox_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			colorDialog.Color = brickHullColorPictureBox.BackColor;
			// open the color box in modal
			DialogResult result = colorDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				brickHullColorPictureBox.BackColor = colorDialog.Color;
				redrawSamplePictureBox();
			}
		}

		private void brickHullThicknessNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			redrawSamplePictureBox();
		}

		private void backgroundColorPictureBox_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			colorDialog.Color = backgroundColorPictureBox.BackColor;
			// open the color box in modal
			DialogResult result = colorDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				backgroundColorPictureBox.BackColor = colorDialog.Color;
				redrawSamplePictureBox();
				colorSchemeComboBox.SelectedIndex = (int)ColorScheme.CUSTOM;
				findCorectColorSchemeAccordingToColors();
			}
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
				redrawSamplePictureBox();
				colorSchemeComboBox.SelectedIndex = (int)ColorScheme.CUSTOM;
				findCorectColorSchemeAccordingToColors();
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
				redrawSamplePictureBox();
				colorSchemeComboBox.SelectedIndex = (int)ColorScheme.CUSTOM;
				findCorectColorSchemeAccordingToColors();
			}
		}

		private void GammaForSelectionNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			redrawSamplePictureBox();
		}

		private void GammaForSnappingNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			redrawSamplePictureBox();
		}

		private void colorSchemeComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch ((ColorScheme)colorSchemeComboBox.SelectedIndex)
			{
				case ColorScheme.BLUEPRINT:
					{
						backgroundColorPictureBox.BackColor = Color.CornflowerBlue;
						gridColorPictureBox.BackColor = Color.White;
						subGridColorPictureBox.BackColor = Color.LightGray;
						break;
					}
				case ColorScheme.PARCHMENT:
					{
						backgroundColorPictureBox.BackColor = Color.LemonChiffon;
						gridColorPictureBox.BackColor = Color.Olive;
						subGridColorPictureBox.BackColor = Color.DarkKhaki;
						break;
					}
				case ColorScheme.CLASSIC:
					{
						backgroundColorPictureBox.BackColor = Color.White;
						gridColorPictureBox.BackColor = Color.Black;
						subGridColorPictureBox.BackColor = Color.Gray;
						break;
					}
			}
			// redraw the picture boxes
			if ((ColorScheme)colorSchemeComboBox.SelectedIndex != ColorScheme.CUSTOM)
			{
				backgroundColorPictureBox.Invalidate();
				gridColorPictureBox.Invalidate();
				subGridColorPictureBox.Invalidate();
				redrawSamplePictureBox();
			}
		}

		private void findCorectColorSchemeAccordingToColors()
		{
			if (backgroundColorPictureBox.BackColor.Equals(Color.CornflowerBlue) &&
				gridColorPictureBox.BackColor.Equals(Color.White) &&
				subGridColorPictureBox.BackColor.Equals(Color.LightGray))
				colorSchemeComboBox.SelectedIndex = (int)ColorScheme.BLUEPRINT;
			else if (backgroundColorPictureBox.BackColor.Equals(Color.LemonChiffon) &&
				gridColorPictureBox.BackColor.Equals(Color.Olive) &&
				subGridColorPictureBox.BackColor.Equals(Color.DarkKhaki))
				colorSchemeComboBox.SelectedIndex = (int)ColorScheme.PARCHMENT;
			else if (backgroundColorPictureBox.BackColor.Equals(Color.White) &&
				gridColorPictureBox.BackColor.Equals(Color.Black) &&
				subGridColorPictureBox.BackColor.Equals(Color.Gray))
				colorSchemeComboBox.SelectedIndex = (int)ColorScheme.CLASSIC;
			else
				colorSchemeComboBox.SelectedIndex = (int)ColorScheme.CUSTOM;
		}

		private void fillColorSchemeComboBox()
		{
			// respect the order of the enum ColorScheme
			colorSchemeComboBox.Items.Clear();
			colorSchemeComboBox.Items.Add(Resources.ColorSchemeCustom);
			colorSchemeComboBox.Items.Add(Resources.ColorSchemeBluePrint);
			colorSchemeComboBox.Items.Add(Resources.ColorSchemeParchment);
			colorSchemeComboBox.Items.Add(Resources.ColorSchemeClassic);
		}

		private void defaultFontButton_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			fontDialog.Font = defaultTextFontNameLabel.Font;
			// open the color box in modal
			DialogResult result = fontDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				updateChosenFont(defaultTextFontNameLabel, defaultFontColorPictureBox.BackColor, fontDialog.Font);
			}
		}

		private void defaultFontColorPictureBox_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			colorDialog.Color = defaultFontColorPictureBox.BackColor;
			// open the color box in modal
			DialogResult result = colorDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				defaultFontColorPictureBox.BackColor = colorDialog.Color;
				defaultTextFontNameLabel.ForeColor = colorDialog.Color;
			}
		}


		private void gridEnabledCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			gridSizeNumericUpDown.Enabled = gridEnabledCheckBox.Checked;
		}

		private void subGridEnabledCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			gridSubdivisionNumericUpDown.Enabled = subGridEnabledCheckBox.Checked;
		}
		#endregion

		#region tab PartLib
		private void fillPartLibraryListBox(bool isForResetingDefaultSetting)
		{
			// get the list of tabs
			List<string> tabNames = MainForm.Instance.PartsTabControl.getTabNames();

			// if we need to reset the order from the setting, change the tab name list
			// according to the default setting
			if (isForResetingDefaultSetting)
			{
				System.Collections.Specialized.StringCollection settingNameList = Settings.Default.PartLibTabOrder;
				int insertIndex = 0;
				foreach (string settingName in settingNameList)
				{
					int currentIndex = tabNames.IndexOf(settingName);
					if (currentIndex != -1)
					{
						// get the tab page, remove it and reinsert it at the correct position
						string name = tabNames[currentIndex];
						tabNames.Remove(name); // do not use RemoveAt() that throw an exception even if the index is correct
						tabNames.Insert(insertIndex, name);

						// increment the insert point
						if (insertIndex < tabNames.Count)
							insertIndex++;
					}
				}
			}

			// fill the list with it
			PartLibTabListBox.Items.Clear();
			foreach (string name in tabNames)
				PartLibTabListBox.Items.Add(name);
		}

		private void savePartLibraryTabOrder()
		{
			if (mHasPartLibOrderChanged)
			{
				// recreate the setting array
				Settings.Default.PartLibTabOrder = new System.Collections.Specialized.StringCollection();
				// iterate on the list in the control
				foreach (object item in PartLibTabListBox.Items)
					Settings.Default.PartLibTabOrder.Add(item as string);
			}
		}

		private void MoveUpButton_Click(object sender, EventArgs e)
		{
			int selectedIndex = PartLibTabListBox.SelectedIndex;
			if (selectedIndex > 0)
			{
				string name = PartLibTabListBox.Items[selectedIndex] as string;
				PartLibTabListBox.Items.RemoveAt(selectedIndex);
				int newIndex = selectedIndex - 1;
				PartLibTabListBox.Items.Insert(newIndex, name);
				PartLibTabListBox.SelectedIndex = newIndex;
				mHasPartLibOrderChanged = true;
			}
		}

		private void MoveDownButton_Click(object sender, EventArgs e)
		{
			int selectedIndex = PartLibTabListBox.SelectedIndex;
			if ((selectedIndex >= 0) && (selectedIndex < PartLibTabListBox.Items.Count - 1))
			{
				string name = PartLibTabListBox.Items[selectedIndex] as string;
				PartLibTabListBox.Items.RemoveAt(selectedIndex);
				int newIndex = selectedIndex + 1;
				PartLibTabListBox.Items.Insert(newIndex, name);
				PartLibTabListBox.SelectedIndex = newIndex;
				mHasPartLibOrderChanged = true;
			}
		}

		private void PartLibTabListBox_SelectedValueChanged(object sender, EventArgs e)
		{
			int selectedIndex = PartLibTabListBox.SelectedIndex;
			MoveUpButton.Enabled = selectedIndex > 0;
			MoveDownButton.Enabled = (selectedIndex >= 0) && (selectedIndex < PartLibTabListBox.Items.Count - 1);
		}

		private void alphabeticOrderButton_Click(object sender, EventArgs e)
		{
			PartLibTabListBox.Sorted = true;
			PartLibTabListBox.Sorted = false;
			mHasPartLibOrderChanged = true;
		}

		private void PartLibDisplayPartInfoCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			bool isEnabled = PartLibDisplayPartInfoCheckBox.Checked;
			PartLibDisplayPartPartIDCheckBox.Enabled = isEnabled;
			PartLibDisplayPartPartColorCheckBox.Enabled = isEnabled;
			PartLibDisplayPartPartDescriptionCheckBox.Enabled = isEnabled;
		}

		private void PartLibDisplayPartPartIDCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			// avoid having all the 3 checkbox empty
			if (!PartLibDisplayPartPartIDCheckBox.Checked && !PartLibDisplayPartPartColorCheckBox.Checked && !PartLibDisplayPartPartDescriptionCheckBox.Checked)
				PartLibDisplayPartPartDescriptionCheckBox.Checked = true;
		}

		private void PartLibDisplayPartPartColorCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			// avoid having all the 3 checkbox empty
			if (!PartLibDisplayPartPartIDCheckBox.Checked && !PartLibDisplayPartPartColorCheckBox.Checked && !PartLibDisplayPartPartDescriptionCheckBox.Checked)
				PartLibDisplayPartPartDescriptionCheckBox.Checked = true;
		}

		private void PartLibDisplayPartPartDescriptionCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			// avoid having all the 3 checkbox empty
			if (!PartLibDisplayPartPartIDCheckBox.Checked && !PartLibDisplayPartPartColorCheckBox.Checked && !PartLibDisplayPartPartDescriptionCheckBox.Checked)
				PartLibDisplayPartPartIDCheckBox.Checked = true;
		}

		private void displayBubbleInfoCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			bool isEnabled = PartLibDisplayBubbleInfoCheckBox.Checked;
			PartLibDisplayBubblePartIDCheckBox.Enabled = isEnabled;
			PartLibDisplayBubblePartColorCheckBox.Enabled = isEnabled;
			PartLibDisplayBubblePartDescriptionCheckBox.Enabled = isEnabled;
		}

		private void PartLibDisplayBubblePartIDCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			// avoid having all the 3 checkbox empty
			if (!PartLibDisplayBubblePartIDCheckBox.Checked && !PartLibDisplayBubblePartColorCheckBox.Checked && !PartLibDisplayBubblePartDescriptionCheckBox.Checked)
				PartLibDisplayBubblePartDescriptionCheckBox.Checked = true;
		}

		private void PartLibDisplayBubblePartColorCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			// avoid having all the 3 checkbox empty
			if (!PartLibDisplayBubblePartIDCheckBox.Checked && !PartLibDisplayBubblePartColorCheckBox.Checked && !PartLibDisplayBubblePartDescriptionCheckBox.Checked)
				PartLibDisplayBubblePartDescriptionCheckBox.Checked = true;
		}

		private void PartLibDisplayBubblePartDescriptionCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			// avoid having all the 3 checkbox empty
			if (!PartLibDisplayBubblePartIDCheckBox.Checked && !PartLibDisplayBubblePartColorCheckBox.Checked && !PartLibDisplayBubblePartDescriptionCheckBox.Checked)
				PartLibDisplayBubblePartIDCheckBox.Checked = true;
		}

		private void PartLibBackColorPictureBox_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			colorDialog.Color = PartLibBackColorPictureBox.BackColor;
			// open the color box in modal
			DialogResult result = colorDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				PartLibBackColorPictureBox.BackColor = colorDialog.Color;
			}
		}

		private void partLibBudgetFilterBackColorPictureBox_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			colorDialog.Color = partLibBudgetFilterBackColorPictureBox.BackColor;
			// open the color box in modal
			DialogResult result = colorDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				partLibBudgetFilterBackColorPictureBox.BackColor = colorDialog.Color;
			}
		}

		private void PartLibFilteredBackColorPictureBox_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			colorDialog.Color = PartLibFilteredBackColorPictureBox.BackColor;
			// open the color box in modal
			DialogResult result = colorDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				PartLibFilteredBackColorPictureBox.BackColor = colorDialog.Color;
			}
		}

		private void PartLibBrowseBudgetFileButton_Click(object sender, EventArgs e)
		{
			// get the open file dialog for budget from the main form
			OpenFileDialog openBudgetFileDialog = MainForm.Instance.openBudgetFileDialog;
			// set the filename in the dialog from the setting
			if (mIsBudgetFilenameToLoadAtStartupSet)
			{
                FileInfo file = new FileInfo(PartLibBudgetFilenameTextBox.Text);
				openBudgetFileDialog.InitialDirectory = file.DirectoryName;
				openBudgetFileDialog.FileName = file.Name;
			}
			// then open the dialog box in modal
			DialogResult result = openBudgetFileDialog.ShowDialog();
			if (result == DialogResult.OK)
				setTextBoxForBudgetFilenameToLoadAtStartup(openBudgetFileDialog.FileName);
		}

		private void PartLibTrashBudgetFileToLoadArStartUpButton_Click(object sender, EventArgs e)
		{
			setTextBoxForBudgetFilenameToLoadAtStartup(string.Empty);
		}
		#endregion

		#region tab Shortcut key

		/// <summary>
		/// This class is used to sort the column of the list view that list the shortcut keys
		/// </summary>
		private class MyListViewItemComparer : System.Collections.IComparer
		{
			private readonly int mColumn;
			private readonly SortOrder mOrder;

			public MyListViewItemComparer(int column, SortOrder order)
			{
				mColumn = column;
				mOrder = order;
			}

			public int Compare(Object x, Object y)
			{
				// use a string compare with the correct column
				int result = string.Compare((x as ListViewItem).SubItems[mColumn].Text, (y as ListViewItem).SubItems[mColumn].Text);
				// check the order
				if (mOrder == SortOrder.Descending)
					result = -result;
				return result;
			}
		}

		/// <summary>
		/// Add a new item (with its subitems) to the list view of shortcut key, from an array of string.
		/// The first cell of the array is the index in the shortcut key dropdown list.
		/// The first cell of the array is the index in the action dropdown list.
		/// The two last cells are the part number (in clear) and the connection number.
		/// </summary>
		/// <param name="itemNames"></param>
		private void addShortcutKey(string[] itemNames)
		{
			ListViewItem.ListViewSubItem[] subItems = { new ListViewItem.ListViewSubItem(), new ListViewItem.ListViewSubItem(), new ListViewItem.ListViewSubItem(), new ListViewItem.ListViewSubItem() };

			// try to replace the first two indices by the corresponding text
			try
			{
				// key
				int index = int.Parse(itemNames[0]);
				subItems[0].Text = comboBoxKey.Items[index].ToString();
				subItems[0].Tag = itemNames[0];
				// action
				index = int.Parse(itemNames[1]);
				subItems[1].Text = comboBoxAction.Items[index].ToString();
				subItems[1].Tag = itemNames[1];
			}
			catch { }
			subItems[2].Text = itemNames[2];
			subItems[3].Text = itemNames[3];

			// add the new item
			ListViewItem newItem = new ListViewItem(subItems,0);
			listViewShortcutKeys.Items.Add(newItem);
		}

		private void comboBoxAction_SelectedIndexChanged(object sender, EventArgs e)
		{
			// disable the 2 last combobox if we don't choose the add action
			bool isEnabled = comboBoxAction.SelectedIndex == 0;
			comboBoxPartNum.Enabled = isEnabled;
			comboBoxConnexion.Enabled = isEnabled;
		}

		private void comboBoxPartNum_SelectedIndexChanged(object sender, EventArgs e)
		{
			// refill the connexion combobox according to the current selected part
			comboBoxConnexion.Items.Clear();
			// ask the list of connexion to the brick library
			List<BrickLibrary.Brick.ConnectionPoint> connexionList = BrickLibrary.Instance.GetConnectionList(comboBoxPartNum.SelectedItem.ToString());
			int nbConnexion = 1;
			if (connexionList != null)
				nbConnexion = connexionList.Count;
			// fill the combobox
			for (int i = 0; i < nbConnexion; ++i)
				comboBoxConnexion.Items.Add(i.ToString());
			// select the first item
			comboBoxConnexion.SelectedIndex = 0;
		}

		private void buttonAdd_Click(object sender, EventArgs e)
		{
			string[] itemNames = { comboBoxKey.SelectedIndex.ToString(), comboBoxAction.SelectedIndex.ToString(), comboBoxPartNum.SelectedItem.ToString(), comboBoxConnexion.SelectedItem.ToString() };
			if (comboBoxAction.SelectedIndex != 0)
			{
				itemNames[2] = String.Empty;
				itemNames[3] = String.Empty;
			}
			
			// search for existing shortcut with the same key
			List<ListViewItem> existingItemForThisKey = new List<ListViewItem>();
			foreach (ListViewItem item in listViewShortcutKeys.Items)
				if (item.SubItems[0].Tag.Equals(itemNames[0]))
					existingItemForThisKey.Add(item);

			// check if we found any and ask for replacement or adding
			if (existingItemForThisKey.Count > 0)
			{
				DialogResult result = MessageBox.Show(this,
                    Resources.ErrorMsgReplaceExistingShortcut,
                    Resources.ErrorMsgTitleWarning, MessageBoxButtons.YesNoCancel,
					MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
				// check the result of the question
				if (result == DialogResult.Yes)
				{
					// delete all the existing shortcut
					foreach (ListViewItem item in existingItemForThisKey)
						listViewShortcutKeys.Items.Remove(item);
				}
				else if (result == DialogResult.Cancel)
				{
					// do not delete neither add shortcut, just exit
					return;
				}
			}

			// now add the shortcuts
			addShortcutKey(itemNames);
		}

		private void buttonDelete_Click(object sender, EventArgs e)
		{
			List<ListViewItem>	itemsToDelete = new List<ListViewItem>();
			foreach (ListViewItem item in listViewShortcutKeys.SelectedItems)
				itemsToDelete.Add(item);
			foreach (ListViewItem item in itemsToDelete)
				listViewShortcutKeys.Items.Remove(item);
		}

		private void listViewShortcutKeys_SelectedIndexChanged(object sender, EventArgs e)
		{
			// enable the delete button
			buttonDelete.Enabled = listViewShortcutKeys.SelectedItems.Count > 0;
			
			// also update the combox below with the values of the selected shortcut if only one is selected
			if (listViewShortcutKeys.SelectedItems.Count == 1)
			{
				ListViewItem.ListViewSubItemCollection selectedItem = listViewShortcutKeys.SelectedItems[0].SubItems;
				comboBoxKey.SelectedIndex = int.Parse(selectedItem[0].Tag as string);
				comboBoxAction.SelectedIndex = int.Parse(selectedItem[1].Tag as string);
				comboBoxPartNum.SelectedItem = selectedItem[2].Text;
				comboBoxConnexion.SelectedItem = selectedItem[3].Text;
			}
		}

		private void listViewShortcutKeys_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			// start of the update of the control
			listViewShortcutKeys.BeginUpdate();

			// change the sort order if we click again on the same column
			// but if we change the column, don't change the sort order
			if (mGlobalStatsLastColumnSorted == e.Column)
			{
				if (listViewShortcutKeys.Sorting == SortOrder.Ascending)
					listViewShortcutKeys.Sorting = SortOrder.Descending;
				else
					listViewShortcutKeys.Sorting = SortOrder.Ascending;
			}
			else
			{
				// We change the twice the listViewShortcutKeys.Sorting value
				// and reset it with the same value to FORCE the Sort to be done,
				// once if the listViewShortcutKeys.Sorting didn't changed the Sort
				// method does nothing.
				SortOrder oldOrder = listViewShortcutKeys.Sorting;
				listViewShortcutKeys.Sorting = SortOrder.None;
				listViewShortcutKeys.Sorting = oldOrder;
			}

			// remove the previous sorting icon, and add the icon to the new column
			setSortIcon(e.Column);

			// create a new comparer with the right column then call the sort method
			listViewShortcutKeys.ListViewItemSorter = new MyListViewItemComparer(e.Column, listViewShortcutKeys.Sorting);
			listViewShortcutKeys.Sort();

			// end of the update of the control
			listViewShortcutKeys.EndUpdate();
		}

		private void setSortIcon(int columnIndex)
		{
			// remove the order sign on the previous sorted column
			if (mGlobalStatsLastColumnSorted != -1)
			{
				string header = listViewShortcutKeys.Columns[mGlobalStatsLastColumnSorted].Text;
				listViewShortcutKeys.Columns[mGlobalStatsLastColumnSorted].Text = header.Substring(2);
			}

			// save the new current column index
			mGlobalStatsLastColumnSorted = columnIndex;

			// add a descending or ascending sign to the header of the column
			if (listViewShortcutKeys.Sorting == SortOrder.Ascending)
				listViewShortcutKeys.Columns[columnIndex].Text = char.ConvertFromUtf32(0x25B2) + " " + listViewShortcutKeys.Columns[columnIndex].Text;
			else
				listViewShortcutKeys.Columns[columnIndex].Text = char.ConvertFromUtf32(0x25BC) + " " + listViewShortcutKeys.Columns[columnIndex].Text;
		}

		#endregion
	}
}