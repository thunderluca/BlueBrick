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
using BlueBrick.Actions.Texts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace BlueBrick.MapData
{
	[Serializable]
	public partial class LayerText : Layer
	{
		/// <summary>
		/// describe all the action that can be done with a mouse when editing a text
		/// </summary>
		private enum EditAction
		{
			NONE,
			MOVE_SELECTION,
			DUPLICATE_SELECTION,
			ADD_OR_EDIT_TEXT,
		}

		private List<TextCell> mTexts = new List<TextCell>();

		// the image attribute to draw the text including the layer transparency
		private readonly ImageAttributes mImageAttribute = new ImageAttributes();

		// related to selection
		private const int BASE_SELECTION_TRANSPARENCY = 112;
		private TextCell mCurrentTextCellUnderMouse = null;
		private SolidBrush mSelectionBrush = new SolidBrush(Color.FromArgb(BASE_SELECTION_TRANSPARENCY, 255, 255, 255));
		private PointF mMouseDownInitialPosition;
		private PointF mMouseDownLastPosition;
		private bool mMouseIsBetweenDownAndUpEvent = false;
		private bool mMouseHasMoved = false;
		private EditAction mEditAction = EditAction.NONE;

		#region set/get
		/// <summary>
		/// get the type name id of this type of layer used in the xml file (not localized)
		/// </summary>
		public override string XmlTypeName
		{
			get { return "text"; }
		}

		/// <summary>
		/// get the localized name of this type of layer
		/// </summary>
		public override string LocalizedTypeName
		{
			get { return Properties.Resources.ErrorMsgLayerTypeText; }
		}

		public override int Transparency
		{
			set
			{
				mTransparency = value;
                ColorMatrix colorMatrix = new ColorMatrix
                {
                    Matrix33 = value / 100.0f
                };
                mImageAttribute.SetColorMatrix(colorMatrix);
				mSelectionBrush = new SolidBrush(Color.FromArgb(BASE_SELECTION_TRANSPARENCY * value / 100, 255, 255, 255));
			}
		}

		/// <summary>
		/// Get the number of texts in this layer.
		/// </summary>
		public override int NbItems
		{
			get { return mTexts.Count; }
		}
		#endregion

		#region constructor
		/// <summary>
		///	Constructor
		/// </summary>
		public LayerText()
		{
		}

		/// <summary>
		/// This method is used to sort items in a list in the same order as they are in the layer list.
		/// The items can be groups, in that case, we use the max index of all the leaf children.
		/// </summary>
		/// <param name="item1">the first item to compare</param>
		/// <param name="item2">the second item t compare</param>
		/// <returns>distance between the two items in the layer list (index1 - index2)</returns>
		public override int CompareItemOrderOnLayer(LayerItem item1, LayerItem item2)
		{
			return CompareItemOrderOnLayer(mTexts, item1, item2);
		}
		#endregion

		#region IXmlSerializable Members

		public override void ReadXml(System.Xml.XmlReader reader)
		{
			// call the common reader class
			base.ReadXml(reader);
            // read all the texts
            ReadItemsListFromXml(reader, ref mTexts, "TextCells", true);
		}

		protected override T ReadItem<T>(System.Xml.XmlReader reader)
		{
			TextCell cell = new TextCell();
			cell.ReadXml(reader);
			return cell as T;
		}

		public override void WriteXml(System.Xml.XmlWriter writer)
		{
			// write the header
			WriteHeaderAndCommonProperties(writer);
			// write all the bricks
			WriteItemsListToXml(writer, mTexts, "TextCells", true);
			// write the footer
			WriteFooter(writer);
		}
		#endregion

		#region action on the layer
		#region add/remove/modify texts
		/// <summary>
		///	Add the specified text cell at the specified position
		/// </summary>
		public void AddTextCell(TextCell cellToAdd, int index)
		{
			if (index < 0)
				mTexts.Add(cellToAdd);
			else
				mTexts.Insert(index, cellToAdd);
		}

		/// <summary>
		/// Remove the specified text cell
		/// </summary>
		/// <param name="cellToRemove"></param>
		/// <returns>the previous index of the cell deleted</returns>
		public int RemoveTextCell(TextCell cellToRemove)
		{
			int index = mTexts.IndexOf(cellToRemove);
			if (index >= 0)
			{
				mTexts.Remove(cellToRemove);
				// remove also the item from the selection list if in it
				if (mSelectedObjects.Contains(cellToRemove))
					RemoveObjectFromSelection(cellToRemove);
			}
			else
				index = 0;
			return index;
		}

		public override void EditSelectedItemsProperties(PointF mouseCoordInStud)
		{
			// does nothing if the selection is empty
			if (mSelectedObjects.Count > 0)
			{
                // in priority get the item under the mouse, if there's several item selected
                // but if user click outside of the item, get the first one of the list
                if (!(GetLayerItemUnderMouse(mSelectedObjects, mouseCoordInStud) is TextCell textToEdit))
                    textToEdit = mSelectedObjects[0] as TextCell;
                // and call the function to edit the properties
                AddOrEditItem(textToEdit, mouseCoordInStud);
			}
		}

		private void AddOrEditItem(TextCell itemToAddOrEdit, PointF mouseCoordInStud)
		{
			// open the form to edit the properties in modal mode
			EditTextForm editTextForm = new EditTextForm(itemToAddOrEdit);
			editTextForm.ShowDialog();
			if (editTextForm.DialogResult == DialogResult.OK)
			{
				// check if it is an edition of an existing text or a new text
				if (itemToAddOrEdit != null)
					ActionManager.Instance.doAction(new EditText(this, itemToAddOrEdit, editTextForm.EditedText, editTextForm.EditedFont, editTextForm.EditedColor, editTextForm.EditedAlignment));
				else
					ActionManager.Instance.doAction(new AddText(this, editTextForm.EditedText, editTextForm.EditedFont, editTextForm.EditedColor, editTextForm.EditedAlignment, mouseCoordInStud));
			}
		}
		#endregion

		#region selection
		/// <summary>
		/// Copy the list of the selected texts in a separate list for later use.
		/// This method should be called on a CTRL+C
		/// </summary>
		public override void CopyCurrentSelectionToClipboard()
		{
            CopyCurrentSelectionToClipboard(mTexts);
		}

		/// <summary>
		/// Select all the items in this layer.
		/// </summary>
		public override void SelectAll()
		{
			// clear the selection and add all the item of this layer
			ClearSelection();
			AddObjectInSelection(mTexts);
		}

		/// <summary>
		/// Select all the item inside the rectangle in the current selected layer
		/// </summary>
		/// <param name="selectionRectangeInStud">the rectangle in which select the items</param>
		public override void SelectInRectangle(RectangleF selectionRectangeInStud)
		{
			SelectInRectangle(selectionRectangeInStud, mTexts);
		}
		#endregion
		#endregion

		#region draw

		/// <summary>
		/// get the total area in stud covered by all the text cells in this layer
		/// </summary>
		/// <returns></returns>
		public override RectangleF GetTotalAreaInStud()
		{
			return GetTotalAreaInStud(mTexts);
		}

		/// <summary>
		/// Draw the layer.
		/// </summary>
		/// <param name="g">the graphic context in which draw the layer</param>
		/// <param name="areaInStud">The region in which we should draw</param>
		/// <param name="scalePixelPerStud">The scale to use to draw</param>
		/// <param name="drawSelection">If true draw the selection rectangle and also the selection overlay (this can be set to false when exporting the map to an image)</param>
		public override void Draw(Graphics g, RectangleF areaInStud, double scalePixelPerStud, bool drawSelection)
		{
			if (!Visible)
				return;

            GraphicsUnit unit = GraphicsUnit.Pixel;
            // take half because we want to compute half width and height
            float scaleForDestinationRectangle = (float)(0.5f * scalePixelPerStud) / TextCell.ANTI_ALIASING_FONT_SCALE;
            foreach (TextCell cell in mTexts)
			{
				float left = cell.Position.X;
				float right = left + cell.Width;
				float top = cell.Position.Y;
				float bottom = top + cell.Height;
				if ((right >= areaInStud.Left) && (left <= areaInStud.Right) && (bottom >= areaInStud.Top) && (top <= areaInStud.Bottom))
				{ 
                    // set the transform
                    Matrix rotation = new Matrix();
                    PointF center = SConvertPointInStudToPixel(cell.Center, areaInStud, scalePixelPerStud);
                    rotation.Translate(center.X, center.Y);
                    rotation.Rotate(cell.Orientation);
                    // get the source and destination rectangle
                    RectangleF srcRect = cell.Image.GetBounds(ref unit);
                    float halfWidth = srcRect.Width * scaleForDestinationRectangle;
                    float halfHeight = srcRect.Height * scaleForDestinationRectangle;
                    PointF[] destRect = { new PointF(-halfWidth, -halfHeight), new PointF(halfWidth, -halfHeight), new PointF(-halfWidth, halfHeight) };
                    rotation.TransformPoints(destRect);
                    // draw the image containing the text
                    g.DrawImage(cell.Image, destRect, srcRect, unit, mImageAttribute);

					// compute the selection area in pixel if the text is selected or we need to draw the hull
					bool isSelected = drawSelection && mSelectedObjects.Contains(cell);
                    PointF[] hull = null;
                    if (isSelected || mDisplayHulls)
                        hull = SConvertPolygonInStudToPixel(cell.SelectionArea.Vertice, areaInStud, scalePixelPerStud);

					// draw the hull if needed
					if (mDisplayHulls)
						g.DrawPolygon(mPenToDrawHull, hull);

					// draw a frame around the selected cell while the text size is still in pixel
					if (isSelected)
						g.FillPolygon(mSelectionBrush, hull);
				}
			}

			// call the base class to draw the surrounding selection rectangle
            base.Draw(g, areaInStud, scalePixelPerStud, drawSelection);
		}

		#endregion

		#region mouse event
		/// <summary>
		/// Return the cursor that should be display when the mouse is above the map without mouse click
		/// </summary>
		/// <param name="mouseCoordInStud"></param>
		public override Cursor GetDefaultCursorWithoutMouseClick(PointF mouseCoordInStud)
		{
			// if the layer is not visible you can basically do nothing on it
			if (!Visible)
			{
				return MainForm.Instance.HiddenLayerCursor;
			}
			else if (mMouseIsBetweenDownAndUpEvent)
			{
				// the second test after the or, is because we give a second chance to the user to duplicate
				// the selection if he press the duplicate key after the mouse down, but before he start to move
				if ((mEditAction == EditAction.DUPLICATE_SELECTION) ||
					((mEditAction == EditAction.MOVE_SELECTION) && !mMouseHasMoved && (Control.ModifierKeys == Properties.Settings.Default.MouseDuplicateSelectionKey)))
					return MainForm.Instance.TextDuplicateCursor;
				else if (mEditAction == EditAction.MOVE_SELECTION)
					return MainForm.Instance.TextMoveCursor;
			}
			else
			{
				if (mouseCoordInStud != PointF.Empty)
				{
					if (Control.ModifierKeys == Properties.Settings.Default.MouseDuplicateSelectionKey)
					{
						if (IsPointInsideSelectionRectangle(mouseCoordInStud))
							return MainForm.Instance.TextDuplicateCursor;
					}
					else if (Control.ModifierKeys == Properties.Settings.Default.MouseMultipleSelectionKey)
					{
						return MainForm.Instance.TextSelectionCursor;
					}
					else if (Control.ModifierKeys == Properties.Settings.Default.MouseZoomPanKey)
					{
						return MainForm.Instance.PanOrZoomViewCursor;
					}
					else if (IsPointInsideSelectionRectangle(mouseCoordInStud))
					{
						return MainForm.Instance.TextMoveCursor;
					}
				}
			}
			// return the default arrow cursor
			return MainForm.Instance.TextArrowCursor;
		}

		/// <summary>
		/// Get the text cell under the specified mouse coordinate or null if there's no text cell under.
		/// The search is done in reverse order of the list to get the topmost item.
		/// </summary>
		/// <param name="mouseCoordInStud">the coordinate of the mouse cursor, where to look for</param>
		/// <returns>the text cell that is under the mouse coordinate or null if there is none.</returns>
		public TextCell GetTextCellUnderMouse(PointF mouseCoordInStud)
		{
			return GetLayerItemUnderMouse(mTexts, mouseCoordInStud) as TextCell;
		}

		/// <summary>
		/// This function is called to know if this layer is interested by the specified mouse click
		/// </summary>
		/// <param name="e">the mouse event arg that describe the mouse click</param>
		/// <returns>true if this layer wants to handle it</returns>
		public override bool HandleMouseDown(MouseEventArgs e, PointF mouseCoordInStud, ref Cursor preferedCursor)
		{
			// if the layer is not visible it is not sensible to mouve click
			if (!Visible)
				return false;

			// do stuff only for the left button
			if (e.Button == MouseButtons.Left)
			{
				// check if the mouse is inside the bounding rectangle of the selected objects
				bool isMouseInsideSelectedObjects = IsPointInsideSelectionRectangle(mouseCoordInStud);
				if (!isMouseInsideSelectedObjects && (Control.ModifierKeys != Properties.Settings.Default.MouseMultipleSelectionKey)
					&& (Control.ModifierKeys != Properties.Settings.Default.MouseDuplicateSelectionKey))
					ClearSelection();

				// compute the current text cell under the mouse
				mCurrentTextCellUnderMouse = null;

				// We search if there is a cell under the mouse but in priority we choose from the current selected cells
				mCurrentTextCellUnderMouse = GetLayerItemUnderMouse(mSelectedObjects, mouseCoordInStud) as TextCell;

				// if the current selected text is not under the mouse we search among the other texts
				// but in reverse order to choose first the brick on top
				if (mCurrentTextCellUnderMouse == null)
					mCurrentTextCellUnderMouse = GetTextCellUnderMouse(mouseCoordInStud);

				// reset the action and the cursor
				mEditAction = EditAction.NONE;
				preferedCursor = MainForm.Instance.TextArrowCursor;

				// check if it is a duplicate of the selection
				// Be carreful for a duplication we take only the selected objects, not the cell
				// under the mouse that may not be selected
				if (isMouseInsideSelectedObjects &&	(Control.ModifierKeys == Properties.Settings.Default.MouseDuplicateSelectionKey))
				{
					mEditAction = EditAction.DUPLICATE_SELECTION;
					preferedCursor = MainForm.Instance.TextDuplicateCursor;
				}
				// we will add or edit a text if we double click
				else if (e.Clicks == 2)
				{
					mEditAction = EditAction.ADD_OR_EDIT_TEXT;
					preferedCursor = MainForm.Instance.TextCreateCursor;
				}
				// check if the user plan to move the selected items
				else if ((isMouseInsideSelectedObjects || (mCurrentTextCellUnderMouse != null))
						&& (Control.ModifierKeys != Properties.Settings.Default.MouseMultipleSelectionKey)
						&& (Control.ModifierKeys != Properties.Settings.Default.MouseDuplicateSelectionKey))
				{
					mEditAction = EditAction.MOVE_SELECTION;
					preferedCursor = MainForm.Instance.TextMoveCursor;
				}
			}

			// handle the mouse down if we duplicate or move the selected texts, or edit a text, or cancel the edition with right click
			return (mEditAction != EditAction.NONE) && ((e.Button == MouseButtons.Left) || (e.Button == MouseButtons.Right));
		}

		/// <summary>
		/// This method is called if the map decided that this layer should handle
		/// this mouse click
		/// </summary>
		/// <param name="e">the mouse event arg that describe the click</param>
		/// <returns>true if the view should be refreshed</returns>
		public override bool MouseDown(MouseEventArgs e, PointF mouseCoordInStud)
		{
			mMouseIsBetweenDownAndUpEvent = true;

			bool mustRefresh = false;

			if (e.Button == MouseButtons.Left)
			{
				// if finally we are called to handle this mouse down,
				// we add the cell under the mouse if the selection list is empty
				if ((mCurrentTextCellUnderMouse != null) && (mEditAction != EditAction.DUPLICATE_SELECTION))
				{
					// if the selection is empty add the text cell, else check the control key state
					if ((mSelectedObjects.Count == 0) && (Control.ModifierKeys != Properties.Settings.Default.MouseMultipleSelectionKey))
					{
						AddObjectInSelection(mCurrentTextCellUnderMouse);
					}
					mustRefresh = true;
				}

				// record the initial position of the mouse
				mMouseDownInitialPosition = mouseCoordInStud;
				mMouseDownLastPosition = mouseCoordInStud;
				mMouseHasMoved = false;
			}
			else if (e.Button == MouseButtons.Right)
			{
				// cancel button down
				if (mEditAction == EditAction.DUPLICATE_SELECTION)
				{
					// undo the duplicate action and clear it
					if (mLastDuplicateAction != null)
						mLastDuplicateAction.Undo();
					mLastDuplicateAction = null;
				}
				else if (mEditAction == EditAction.MOVE_SELECTION)
				{
					// compute the delta mouve of the mouse
					PointF deltaMove = new PointF(mouseCoordInStud.X - mMouseDownInitialPosition.X, mouseCoordInStud.Y - mMouseDownInitialPosition.Y);
					if ((deltaMove.X != 0) || (deltaMove.Y != 0))
					{
						// reset the initial position to each text
						foreach (TextCell cell in mSelectedObjects)
							cell.Position = new PointF(cell.Position.X - deltaMove.X, cell.Position.Y - deltaMove.Y);
						// reset the bounding rectangle
						UpdateBoundingSelectionRectangle();
					}
				}
				mEditAction = EditAction.NONE;
				mCurrentTextCellUnderMouse = null;
				mustRefresh = true;
			}

			return mustRefresh;
		}

		/// <summary>
		/// This method is called when the mouse move.
		/// </summary>
		/// <param name="e">the mouse event arg that describe the mouse move</param>
		/// <returns>true if the view should be refreshed</returns>
		public override bool MouseMove(MouseEventArgs e, PointF mouseCoordInStud, ref Cursor preferedCursor)
		{
			if ((mEditAction == EditAction.MOVE_SELECTION) || (mEditAction == EditAction.DUPLICATE_SELECTION))
			{
				// give a second chance to duplicate if the user press the duplicate key
				// after pressing down the mouse key, but not if the user already moved
				if ((mEditAction == EditAction.MOVE_SELECTION) && !mMouseHasMoved &&
					(Control.ModifierKeys == Properties.Settings.Default.MouseDuplicateSelectionKey))
					mEditAction = EditAction.DUPLICATE_SELECTION;

				// check if it is a move or a duplicate
				if (mEditAction == EditAction.DUPLICATE_SELECTION)
				{
					// this is a duplicate, if we didn't duplicate the text, this is the moment to copy and paste the selection
					// and this will change the current selection, that will be move normally after
					if (mLastDuplicateAction == null)
					{
						CopyCurrentSelectionToClipboard();
						AddActionInHistory addInHistory = AddActionInHistory.DO_NOT_ADD_TO_HISTORY_EXCEPT_IF_POPUP_OCCURED;
						PasteClipboardInLayer(AddOffsetAfterPaste.NO, ref addInHistory);
					}
				}
				// compute the delta move of the mouse
				PointF deltaMove = new PointF(mouseCoordInStud.X - mMouseDownLastPosition.X, mouseCoordInStud.Y - mMouseDownLastPosition.Y);
				// this is move of the selection, not a duplicate selection
				foreach (TextCell cell in mSelectedObjects)
					cell.Position = new PointF(cell.Position.X + deltaMove.X, cell.Position.Y + deltaMove.Y);
				// move also the bounding rectangle
				MoveBoundingSelectionRectangle(deltaMove);
				// memorize the last position of the mouse
				mMouseDownLastPosition = mouseCoordInStud;
				// reset the current brick under the mouse such as we will not remove or add it in the selection
				// in the up event
				mCurrentTextCellUnderMouse = null;
				// set the flag that indicate that we moved the mouse
				mMouseHasMoved = true;
				return true;
			}
			return false;
		}

		/// <summary>
		/// This method is called when the mouse button is released.
		/// </summary>
		/// <param name="e">the mouse event arg that describe the click</param>
		/// <returns>true if the view should be refreshed</returns>
		public override bool MouseUp(MouseEventArgs e, PointF mouseCoordInStud)
		{
			// if it's a double click, we should prompt a box for text editing
			// WARNING: prompt the box in the mouse up event,
			// otherwise, if you do it in the mouse down, the mouse up is not triggered (both under dot net and mono)
			// and this can mess up the click count in mono
			if (mEditAction == EditAction.ADD_OR_EDIT_TEXT)
			{
				// call the function to add or edit, which open the edit text dialog in modal
				AddOrEditItem(mCurrentTextCellUnderMouse, mouseCoordInStud);
			}
			else if (mMouseHasMoved && (mSelectedObjects.Count > 0)) // check if we moved the selected text
			{
				// reset the flag
				mMouseHasMoved = false;

				// compute the delta mouve of the mouse
				PointF deltaMove = new PointF(mouseCoordInStud.X - mMouseDownInitialPosition.X, mouseCoordInStud.Y - mMouseDownInitialPosition.Y);

				// create a new action for this move
				if ((deltaMove.X != 0) || (deltaMove.Y != 0))
				{
					// update the duplicate action or add a move action
					if (mEditAction == EditAction.DUPLICATE_SELECTION)
					{
						if (mLastDuplicateAction != null)
							mLastDuplicateAction.updatePositionShift(deltaMove.X, deltaMove.Y);
					}
					else if (mEditAction == EditAction.MOVE_SELECTION)
					{
						// reset the initial position to each text
						foreach (TextCell cell in mSelectedObjects)
							cell.Position = new PointF(cell.Position.X - deltaMove.X, cell.Position.Y - deltaMove.Y);
						// and add an action
						ActionManager.Instance.doAction(new MoveText(this, mSelectedObjects, deltaMove));
					}
				}

				// add the duplicate action in the manager after the last modification of the movement
				if ((mEditAction == EditAction.DUPLICATE_SELECTION) && (mLastDuplicateAction != null))
				{
					mLastDuplicateAction.Undo();
					ActionManager.Instance.doAction(mLastDuplicateAction);
				}

				// reset anyway the temp reference for the duplication
				mLastDuplicateAction = null;
			}
			else
			{
				// if we didn't move the item and use the control key, we need to add or remove object from the selection
				// we must do it in the up event because if we do it in the down, we may remove an object before moving
				// in the move event we reset the mCurrentBrickUnderMouse to avoid this change if we move
				if ((mCurrentTextCellUnderMouse != null) && (Control.ModifierKeys == Properties.Settings.Default.MouseMultipleSelectionKey))
				{
					if (mSelectedObjects.Contains(mCurrentTextCellUnderMouse))
						RemoveObjectFromSelection(mCurrentTextCellUnderMouse);
					else
						AddObjectInSelection(mCurrentTextCellUnderMouse);
				}
			}

			mEditAction = EditAction.NONE;
			mMouseIsBetweenDownAndUpEvent = false;
			mCurrentTextCellUnderMouse = null;

			// refresh in any case
			return true;
		}
		#endregion
	}
}
