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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BlueBrick.MapData;
using BlueBrick.Properties;
using BlueBrick.Actions;

namespace BlueBrick
{
	/// <summary>
	/// A customized Panel that draw the map.
	/// </summary>
	/// <remarks>
	/// <para>The pannel draw elements of several layers.</para>
	/// </remarks>
	public class MapPanel : Panel
	{
		#region Fields

		/// <summary>
		/// This small internal enum is used to avoid duplcation of code in the mouse event handler
		/// because, you can actually perform one action with different key combination, so the event
		/// handler can first decide which action to do before writing the code that execute it
		/// </summary>
		private enum ActionToDoInMouseEvent
		{
			NONE,
			SCROLL_VIEW,
			ZOOM_VIEW
		};

		// scroll position
		private bool mIsScrolling = false;
		private Point mLastScrollMousePos = new Point();
		// initial zoom position, if not using the wheel
		private bool mIsZooming = false;
		private Point mFirstZoomMousePos = new Point();
		private Point mLastZoomMousePos = new Point();
		// last position if no button is pressed
		private Point mLastMousePos = new Point();
		// last position when a button is down
		private PointF mLastDownMouseCoordInStud = new Point();
		// the coordinate in STUD of the center of the view (that means which part of the map the center of the view is currently targeting)
		private double mViewCornerX = 0;
		private double mViewCornerY = 0;

		// selection in rectangle
		private bool mIsSelectionRectangleOn = false;
		private bool mIsMouseHandledByMap = false;
		private Point mSelectionRectangleInitialPosition = new Point();
		private Rectangle mSelectionRectangle = new Rectangle();
		private readonly Pen mSelectionRectanglePen = new Pen(Color.Black, 2);

        // adjust the map area size if the status bar or scrollbar are displayed
		private int mCurrentStatusBarHeight = 0;

		//dragndrop of a part on the map
		private Layer.LayerItem mCurrentPartDrop = null;
		private LayerBrick mBrickLayerThatReceivePartDrop = null;
		private ContextMenuStrip contextMenuStrip;
		private System.ComponentModel.IContainer components;
		private ToolStripMenuItem bringToFrontToolStripMenuItem;
		private ToolStripMenuItem sendToBackToolStripMenuItem;
		private ToolStripSeparator selectToolStripSeparator;
		private ToolStripMenuItem deselectAllToolStripMenuItem;
		private ToolStripMenuItem selectPathToolStripMenuItem;
		private ToolStripMenuItem selectAllToolStripMenuItem;
		private ToolStripSeparator groupToolStripSeparator;
		private ToolStripMenuItem groupToolStripMenuItem;
		private ToolStripMenuItem ungroupToolStripMenuItem;
		private ToolStripSeparator attachRulerToolStripSeparator;
		private ToolStripMenuItem attachToolStripMenuItem;
		private ToolStripMenuItem detachToolStripMenuItem;
		private ToolStripMenuItem useAsModelToolStripMenuItem;
		private ToolStripSeparator propertiesToolStripSeparator;
		private ToolStripMenuItem propertiesToolStripMenuItem;
		private ToolStripSeparator appearanceToolStripSeparator;
		private ToolStripMenuItem scrollBarToolStripMenuItem;

		// scale
		private double mViewScale = 4.0;

		// a parameter to adjust the scrollbar resolution. This is the slider size but also define the resolution of the whole scrollbar
		const int mMapScrollBarSliderSize = 100;
		private HScrollBar horizontalScrollBar;
		private VScrollBar verticalScrollBar;

		// a const margin added to the total area surface, in order to compute the scrollbar maximums
		const int mScrollBarAddedMarginInStud = 32;

		// for optimization reason we want to memorize the total area, and update only when some opeartion is done on the map (item added, removed or moved)
		private RectangleF mMapTotalAreaInStud = new RectangleF(0, 0, 32, 32);
		#endregion

		#region Get / Set

		/// <summary>
		/// The current scale of the map view.
		/// </summary>
		public double ViewScale
		{
			get { return mViewScale; }
			set
			{
				double oldValue = mViewScale;
				// avoid setting a null scale or negative scale, set only a scale in the range
				if (value < 0.2)
					mViewScale = 0.2;
				else if (value > 16.0)
					mViewScale = 16.0;
				else
					mViewScale = value;
				// compute the difference of scale and call the notification on the map
				if (mViewScale != oldValue)
				{
					// call map notification
					Map.Instance.ZoomScaleChangeNotification(oldValue, mViewScale);
					// update the scrollbars
					UpdateScrollbarSize();
					// invalidate the panel, since we must redraw it to handle the new scale
					Invalidate();
				}
			}
		}

		public int CurrentStatusBarHeight
		{
			set { mCurrentStatusBarHeight = value; }
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Construct a new MapPanel.
		/// </summary>
		public MapPanel()
		{
			// Note that we add our own scroll bar, otherwise they appear below the staus bar, and also the panel try to scroll them automatically based on the component on the panel (which are none)
			InitializeComponent();
			// Default values
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
			mSelectionRectanglePen.DashPattern = new float[] { 3.0f, 2.0f };
		}

		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapPanel));
			contextMenuStrip = new ContextMenuStrip(components);
			bringToFrontToolStripMenuItem = new ToolStripMenuItem();
			sendToBackToolStripMenuItem = new ToolStripMenuItem();
			selectToolStripSeparator = new ToolStripSeparator();
			selectAllToolStripMenuItem = new ToolStripMenuItem();
			deselectAllToolStripMenuItem = new ToolStripMenuItem();
			selectPathToolStripMenuItem = new ToolStripMenuItem();
			groupToolStripSeparator = new ToolStripSeparator();
			groupToolStripMenuItem = new ToolStripMenuItem();
			ungroupToolStripMenuItem = new ToolStripMenuItem();
			attachRulerToolStripSeparator = new ToolStripSeparator();
			attachToolStripMenuItem = new ToolStripMenuItem();
			detachToolStripMenuItem = new ToolStripMenuItem();
			useAsModelToolStripMenuItem = new ToolStripMenuItem();
			propertiesToolStripSeparator = new ToolStripSeparator();
			propertiesToolStripMenuItem = new ToolStripMenuItem();
			appearanceToolStripSeparator = new ToolStripSeparator();
			scrollBarToolStripMenuItem = new ToolStripMenuItem();
			horizontalScrollBar = new HScrollBar();
			verticalScrollBar = new VScrollBar();
			contextMenuStrip.SuspendLayout();
			SuspendLayout();
			// 
			// contextMenuStrip
			// 
			resources.ApplyResources(contextMenuStrip, "contextMenuStrip");
			contextMenuStrip.Items.AddRange(new ToolStripItem[] {
            bringToFrontToolStripMenuItem,
            sendToBackToolStripMenuItem,
            selectToolStripSeparator,
            selectAllToolStripMenuItem,
            deselectAllToolStripMenuItem,
            selectPathToolStripMenuItem,
            groupToolStripSeparator,
            groupToolStripMenuItem,
            ungroupToolStripMenuItem,
            attachRulerToolStripSeparator,
            attachToolStripMenuItem,
            detachToolStripMenuItem,
            useAsModelToolStripMenuItem,
            propertiesToolStripSeparator,
            propertiesToolStripMenuItem,
            appearanceToolStripSeparator,
            scrollBarToolStripMenuItem});
			contextMenuStrip.Name = "contextMenuStrip";
			contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(ContextMenuStrip_Opening);
			// 
			// bringToFrontToolStripMenuItem
			// 
			resources.ApplyResources(bringToFrontToolStripMenuItem, "bringToFrontToolStripMenuItem");
			bringToFrontToolStripMenuItem.Name = "bringToFrontToolStripMenuItem";
			bringToFrontToolStripMenuItem.Click += BringToFrontToolStripMenuItem_Click;
			// 
			// sendToBackToolStripMenuItem
			// 
			resources.ApplyResources(sendToBackToolStripMenuItem, "sendToBackToolStripMenuItem");
			sendToBackToolStripMenuItem.Name = "sendToBackToolStripMenuItem";
			sendToBackToolStripMenuItem.Click += SendToBackToolStripMenuItem_Click;
			// 
			// selectToolStripSeparator
			// 
			resources.ApplyResources(selectToolStripSeparator, "selectToolStripSeparator");
			selectToolStripSeparator.Name = "selectToolStripSeparator";
			// 
			// selectAllToolStripMenuItem
			// 
			resources.ApplyResources(selectAllToolStripMenuItem, "selectAllToolStripMenuItem");
			selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
			selectAllToolStripMenuItem.Click += SelectAllToolStripMenuItem_Click;
			// 
			// deselectAllToolStripMenuItem
			// 
			resources.ApplyResources(deselectAllToolStripMenuItem, "deselectAllToolStripMenuItem");
			deselectAllToolStripMenuItem.Name = "deselectAllToolStripMenuItem";
			deselectAllToolStripMenuItem.Click += DeselectAllToolStripMenuItem_Click;
			// 
			// selectPathToolStripMenuItem
			// 
			resources.ApplyResources(selectPathToolStripMenuItem, "selectPathToolStripMenuItem");
			selectPathToolStripMenuItem.Name = "selectPathToolStripMenuItem";
			selectPathToolStripMenuItem.Click += SelectPathToolStripMenuItem_Click;
			// 
			// groupToolStripSeparator
			// 
			resources.ApplyResources(groupToolStripSeparator, "groupToolStripSeparator");
			groupToolStripSeparator.Name = "groupToolStripSeparator";
			// 
			// groupToolStripMenuItem
			// 
			resources.ApplyResources(groupToolStripMenuItem, "groupToolStripMenuItem");
			groupToolStripMenuItem.Name = "groupToolStripMenuItem";
			groupToolStripMenuItem.Click += GroupToolStripMenuItem_Click;
			// 
			// ungroupToolStripMenuItem
			// 
			resources.ApplyResources(ungroupToolStripMenuItem, "ungroupToolStripMenuItem");
			ungroupToolStripMenuItem.Name = "ungroupToolStripMenuItem";
			ungroupToolStripMenuItem.Click += UngroupToolStripMenuItem_Click;
			// 
			// attachRulerToolStripSeparator
			// 
			resources.ApplyResources(attachRulerToolStripSeparator, "attachRulerToolStripSeparator");
			attachRulerToolStripSeparator.Name = "attachRulerToolStripSeparator";
			// 
			// attachToolStripMenuItem
			// 
			resources.ApplyResources(attachToolStripMenuItem, "attachToolStripMenuItem");
			attachToolStripMenuItem.Name = "attachToolStripMenuItem";
			attachToolStripMenuItem.Click += AttachToolStripMenuItem_Click;
			// 
			// detachToolStripMenuItem
			// 
			resources.ApplyResources(detachToolStripMenuItem, "detachToolStripMenuItem");
			detachToolStripMenuItem.Name = "detachToolStripMenuItem";
			detachToolStripMenuItem.Click += DetachToolStripMenuItem_Click;
			// 
			// useAsModelToolStripMenuItem
			// 
			resources.ApplyResources(useAsModelToolStripMenuItem, "useAsModelToolStripMenuItem");
			useAsModelToolStripMenuItem.Name = "useAsModelToolStripMenuItem";
			useAsModelToolStripMenuItem.Click += UseAsModelToolStripMenuItem_Click;
			// 
			// propertiesToolStripSeparator
			// 
			resources.ApplyResources(propertiesToolStripSeparator, "propertiesToolStripSeparator");
			propertiesToolStripSeparator.Name = "propertiesToolStripSeparator";
			// 
			// propertiesToolStripMenuItem
			// 
			resources.ApplyResources(propertiesToolStripMenuItem, "propertiesToolStripMenuItem");
			propertiesToolStripMenuItem.Name = "propertiesToolStripMenuItem";
			propertiesToolStripMenuItem.Click += PropertiesToolStripMenuItem_Click;
			// 
			// appearanceToolStripSeparator
			// 
			resources.ApplyResources(appearanceToolStripSeparator, "appearanceToolStripSeparator");
			appearanceToolStripSeparator.Name = "appearanceToolStripSeparator";
			// 
			// scrollBarToolStripMenuItem
			// 
			resources.ApplyResources(scrollBarToolStripMenuItem, "scrollBarToolStripMenuItem");
			scrollBarToolStripMenuItem.CheckOnClick = true;
			scrollBarToolStripMenuItem.Name = "scrollBarToolStripMenuItem";
			scrollBarToolStripMenuItem.Click += ScrollBarToolStripMenuItem_Click;
			// 
			// horizontalScrollBar
			// 
			resources.ApplyResources(horizontalScrollBar, "horizontalScrollBar");
			horizontalScrollBar.Name = "horizontalScrollBar";
			horizontalScrollBar.Scroll += HorizontalScrollBar_Scroll;
			horizontalScrollBar.MouseEnter += HorizontalScrollBar_MouseEnter;
			// 
			// verticalScrollBar
			// 
			resources.ApplyResources(verticalScrollBar, "verticalScrollBar");
			verticalScrollBar.Name = "verticalScrollBar";
			verticalScrollBar.Scroll += VerticalScrollBar_Scroll;
			verticalScrollBar.MouseEnter += VerticalScrollBar_MouseEnter;
			// 
			// MapPanel
			// 
			resources.ApplyResources(this, "$this");
			AllowDrop = true;
			ContextMenuStrip = contextMenuStrip;
			Controls.Add(horizontalScrollBar);
			Controls.Add(verticalScrollBar);
			SizeChanged += MapPanel_SizeChanged;
			DragDrop += MapPanel_DragDrop;
			DragEnter += MapPanel_DragEnter;
			DragOver += MapPanel_DragOver;
			DragLeave += MapPanel_DragLeave;
			MouseDown += MapPanel_MouseDown;
			MouseEnter += MapPanel_MouseEnter;
			MouseMove += MapPanel_MouseMove;
			MouseUp += MapPanel_MouseUp;
			Resize += MapPanel_Resize;
			contextMenuStrip.ResumeLayout(false);
			ResumeLayout(false);

		}

		public void UpdateView()
		{
			// nothing special except invalidate the view
			Invalidate();
		}

		/// <summary>
		/// This method is inheritated and is usefull to get the event when the arrow are pressed
		/// </summary>
		/// <param name="keyData"></param>
		/// <returns></returns>
		protected override bool IsInputKey(Keys keyData)
		{
			// we need the four arrow keys
			// also page up and down for rotation
			// and delete and backspace for deleting object
			if ((keyData == Keys.Left) || (keyData == Keys.Right) ||
				(keyData == Keys.Up) || (keyData == Keys.Down) ||
				(keyData == Keys.PageDown) || (keyData == Keys.PageUp) ||
				(keyData == Keys.Home) || (keyData == Keys.End) ||
				(keyData == Keys.Insert) || (keyData == Keys.Delete) ||
				(keyData == Keys.Enter) || (keyData == Keys.Return) ||
				(keyData == Keys.Escape) || (keyData == Keys.Back))
				return true;

			return base.IsInputKey(keyData);
		}

		#endregion

		#region Display methods

		/// <summary>
		/// Overridden Paint method to perform a custom draw of the map view.
		/// </summary>
		/// <param name="e">Event argument.</param>
		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics g = e.Graphics;

//for debug FPS			DateTime time = DateTime.Now;

			// set the setting of the graphic
			g.CompositingMode = CompositingMode.SourceOver;
			g.SmoothingMode = SmoothingMode.None;
			g.CompositingQuality = CompositingQuality.HighSpeed;
			g.InterpolationMode = InterpolationMode.Low;
            g.PixelOffsetMode = PixelOffsetMode.None;

			// NOTE: the background color is set directly in this.BackColor !!!
			BackColor = Map.Instance.BackgroundColor;

			// compute the eventual height of the scrollbar if visible
			int currentScrollbarHeight = horizontalScrollBar.Visible ? horizontalScrollBar.Height : 0;

			// call the draw of the map
			float widthInStud = (float)(Size.Width / mViewScale);
            float heightInStud = (float)((Size.Height - mCurrentStatusBarHeight - currentScrollbarHeight) / mViewScale);			
			float startXInStud = (float)mViewCornerX;
			float startYInStud = (float)mViewCornerY;
			RectangleF rectangle = new RectangleF(startXInStud, startYInStud, widthInStud, heightInStud);
			Map.Instance.Draw(g, rectangle, mViewScale, true);
			Map.Instance.DrawWatermark(g, rectangle, mViewScale);

			// on top of all the layer draw the selection rectangle
			if (mIsSelectionRectangleOn)
			{
				// avoid drawing an empty rectangle
				int width = mSelectionRectangle.Width;
				if (width == 0)
					width = 1;
				int height = mSelectionRectangle.Height;
				if (height == 0)
					height = 1;
				// draw the rectangle
				g.DrawRectangle(mSelectionRectanglePen, mSelectionRectangle.X, mSelectionRectangle.Y, width, height);
			}

//for debug FPS			TimeSpan delta = DateTime.Now - time;
//for debug FPS			g.DrawString(delta.Ticks.ToString(), new Font(FontFamily.GenericMonospace,12), Brushes.Black, 0, 0);
		}

		public void MoveViewToMapCenter()
		{
			float halfViewWidthInStud = (float)(Size.Width / (mViewScale * 2));
			float halfViewHeightInStud = (float)(Size.Height / (mViewScale * 2));
			// get the total area of the map
			mMapTotalAreaInStud = Map.Instance.GetTotalAreaInStud(false);
			mViewCornerX = ((mMapTotalAreaInStud.Left + mMapTotalAreaInStud.Right) / 2) - halfViewWidthInStud;
			mViewCornerY = ((mMapTotalAreaInStud.Top + mMapTotalAreaInStud.Bottom) / 2) - halfViewHeightInStud;
			// and update the scroll bar as we changed the view corner, and also the whole map may have changed
			UpdateScrollbarSize();
		}
		#endregion
	
		#region Mouse event

		/// <summary>
		/// Set the default Cursor on the map according to the current selected layer
		/// </summary>
		private Cursor GetDefaultCursor(PointF mouseCoordInStud)
		{
			if (Map.Instance.SelectedLayer != null)
				return Map.Instance.SelectedLayer.GetDefaultCursorWithoutMouseClick(mouseCoordInStud);
			return Cursors.Default;
		}

		public void SetDefaultCursor()
		{
			Cursor = GetDefaultCursor(GetPointCoordInStud(PointToClient(Cursor.Position)));
		}

		private PointF GetPointCoordInStud(Point pointCoordInPixel)
		{
            return new PointF
            {
                X = (float)(mViewCornerX + (pointCoordInPixel.X / mViewScale)),
                Y = (float)(mViewCornerY + (pointCoordInPixel.Y / mViewScale))
            };
		}

		private PointF GetMouseCoordInStud(MouseEventArgs e)
		{
			return GetPointCoordInStud(e.Location);
		}

		private PointF GetScreenPointInStud(Point pointInScreenCoord)
		{
			return GetPointCoordInStud(pointInScreenCoord);
		}

		private void MapPanel_MouseDown(object sender, MouseEventArgs e)
		{
			ActionToDoInMouseEvent actionToDo = ActionToDoInMouseEvent.NONE;
			bool mustRefreshView = false;

			// take the focus anyway if we clik in the map view
			Focus();

			// the cursor to set according to the action
			mLastDownMouseCoordInStud = GetMouseCoordInStud(e);
			Cursor preferedCursor = GetDefaultCursor(mLastDownMouseCoordInStud);

			// then dispatch the event
			switch (e.Button)
			{
				case MouseButtons.Left:
					if (ModifierKeys == Settings.Default.MouseZoomPanKey)
					{
						// this is the pan with the keys, not the wheel
						actionToDo = ActionToDoInMouseEvent.SCROLL_VIEW;
					}
					else if (Map.Instance.HandleMouseDown(e, mLastDownMouseCoordInStud, ref preferedCursor))
					{
						// left button is handle by layers (so give it to the map)
						mustRefreshView = Map.Instance.MouseDown(e, mLastDownMouseCoordInStud);
						mIsMouseHandledByMap = true;
					}
					else if ((Map.Instance.SelectedLayer != null) && Map.Instance.SelectedLayer.Visible)
					{
						// if the selected layer is not visible don't even start a selection or double click
						if (e.Clicks == 1)
						{
							//simple click not handle by the layer, so we start a selection rectangle
							mSelectionRectangleInitialPosition = new Point(e.Location.X, e.Location.Y);
							mSelectionRectangle = new Rectangle(e.Location.X, e.Location.Y, 0, 0);
							mIsSelectionRectangleOn = true;
							mustRefreshView = true;
						}
						else
						{
							// this is a double click
							mIsSelectionRectangleOn = false;
						}
					}
					break;

				case MouseButtons.Middle:
					// middle button is used to scroll
					actionToDo = ActionToDoInMouseEvent.SCROLL_VIEW;
					break;

				case MouseButtons.Right:
					if (ModifierKeys == Settings.Default.MouseZoomPanKey)
					{
						actionToDo = ActionToDoInMouseEvent.ZOOM_VIEW;
					}
					else if (Map.Instance.HandleMouseDown(e, mLastDownMouseCoordInStud, ref preferedCursor))
					{
						// right button is handle by layers (so give it to the map)
						mustRefreshView = Map.Instance.MouseDown(e, mLastDownMouseCoordInStud);
						mIsMouseHandledByMap = true;
					}

					break;
			}

			switch (actionToDo)
			{
				case ActionToDoInMouseEvent.SCROLL_VIEW:
					mIsScrolling = true;
					mLastScrollMousePos = e.Location;
					preferedCursor = MainForm.Instance.PanViewCursor;
					break;

				case ActionToDoInMouseEvent.ZOOM_VIEW:
					mIsZooming = true;
					mFirstZoomMousePos = e.Location;
					mLastZoomMousePos = e.Location;
					preferedCursor = MainForm.Instance.ZoomCursor;
					break;
			}

			// set the cursor with the preference
			Cursor = preferedCursor;

			// check if we need to update the view
			if (mustRefreshView)
				UpdateView();
		}

		private void MapPanel_MouseMove(object sender, MouseEventArgs e)
		{
			ActionToDoInMouseEvent actionToDo = ActionToDoInMouseEvent.NONE;
			bool mustRefreshView = false;
			PointF mouseCoordInStud = GetMouseCoordInStud(e);
			string statusBarMessage = "(" + mouseCoordInStud.X.ToString("F1") + " / " + mouseCoordInStud.Y.ToString("F1") + ") ";
			Cursor preferedCursor = Cursor;

			switch (e.Button)
			{
				case MouseButtons.Left:
					// take the focus anyway that way, we can receive an event mouseup
					// and this simulate a dragndrop in the map
					Focus();

					// check if we are using a selecion rectangle
					if (mIsSelectionRectangleOn)
					{
						mSelectionRectangle.X = Math.Min(e.X, mSelectionRectangleInitialPosition.X);
						mSelectionRectangle.Y = Math.Min(e.Y, mSelectionRectangleInitialPosition.Y);
						mSelectionRectangle.Width = Math.Abs(e.X - mSelectionRectangleInitialPosition.X);
						mSelectionRectangle.Height = Math.Abs(e.Y - mSelectionRectangleInitialPosition.Y);
						Cursor = Cursors.Cross;
						mustRefreshView = true;
					}
					else if (mIsMouseHandledByMap)
					{
						// left button is handle by layers (so give it to the map)
						mustRefreshView = Map.Instance.MouseMove(e, mouseCoordInStud, ref preferedCursor);
						Cursor = preferedCursor;
					}
					else if (mIsZooming)
					{
						actionToDo = ActionToDoInMouseEvent.ZOOM_VIEW;
					}
					else if (mIsScrolling)
					{
						actionToDo = ActionToDoInMouseEvent.SCROLL_VIEW;
					}
					break;

				case MouseButtons.Middle:
					// middle button is used to scroll
					actionToDo = ActionToDoInMouseEvent.SCROLL_VIEW;
					break;

				case MouseButtons.Right:
					if (mIsMouseHandledByMap)
					{
						// left button is handle by layers (so give it to the map)
						mustRefreshView = Map.Instance.MouseMove(e, mouseCoordInStud, ref preferedCursor);
						Cursor = preferedCursor;
					}
					else if (mIsZooming)
					{
						actionToDo = ActionToDoInMouseEvent.ZOOM_VIEW;
					}
					break;

				case MouseButtons.None:
					// if the map also want to handle this free move, call it
					preferedCursor = GetDefaultCursor(mouseCoordInStud);
					mIsMouseHandledByMap = Map.Instance.HandleMouseMoveWithoutClick(e, mouseCoordInStud, ref preferedCursor);
					if (mIsMouseHandledByMap)
						mustRefreshView = Map.Instance.MouseMove(e, mouseCoordInStud, ref preferedCursor);
					// set the cursor with the preference
					Cursor = preferedCursor;

					// nothing to do if we didn't move
					if ((mLastMousePos.X != e.X) || (mLastMousePos.Y != e.Y))
					{
                        // if there's a brick under the mouse, and the player don't use a button,
                        // display the description of the brick in the status bar
                        if (Map.Instance.SelectedLayer is LayerBrick brickLayer)
                        {
                            LayerBrick.Brick brickUnderMouse = brickLayer.getBrickUnderMouse(mouseCoordInStud);
                            if (brickUnderMouse != null)
                                statusBarMessage += BrickLibrary.Instance.GetFormatedBrickInfo(brickUnderMouse.PartNumber, true, true, true);
                        }
                        else
                        {
                            if (Map.Instance.SelectedLayer is LayerText textLayer)
                            {
                                LayerText.TextCell textUnderMouse = textLayer.GetTextCellUnderMouse(mouseCoordInStud);
                                if (textUnderMouse != null)
                                {
                                    statusBarMessage += textUnderMouse.Text.Replace("\r", "");
                                    statusBarMessage = statusBarMessage.Replace('\n', ' ');
                                    if (statusBarMessage.Length > 80)
                                        statusBarMessage = statusBarMessage.Substring(0, 80) + "...";
                                }
                            }
                        }
                    }
					break;
			}

			switch (actionToDo)
			{
				case ActionToDoInMouseEvent.SCROLL_VIEW:
					Pan((mLastScrollMousePos.X - e.X) / mViewScale, (mLastScrollMousePos.Y - e.Y) / mViewScale);
					mLastScrollMousePos.X = e.X;
					mLastScrollMousePos.Y = e.Y;
					// update the view continuously
					mustRefreshView = true;
					break;

				case ActionToDoInMouseEvent.ZOOM_VIEW:
					int yDiff = e.Y - mLastZoomMousePos.Y;
					mLastZoomMousePos = e.Location;
					Zoom((float)(1.0f + (yDiff * 4.0f * Settings.Default.WheelMouseZoomSpeed)),
							Settings.Default.WheelMouseIsZoomOnCursor, mFirstZoomMousePos);
					// the zoom function already update the view, don't need to set the bool flag here
					break;
			}

			//display the message in the status bar
			if ((mLastMousePos.X != e.X) || (mLastMousePos.Y != e.Y))
				MainForm.Instance.SetStatusBarMessage(statusBarMessage);

			// save the last mouse position anyway
			mLastMousePos = e.Location;

			// check if we need to update the view
			if (mustRefreshView)
				UpdateView();
		}

		private void MapPanel_MouseUp(object sender, MouseEventArgs e)
		{
			bool mustRefreshView = false;
			PointF mouseCoordInStud = GetMouseCoordInStud(e);

			// check if we are using a selecion rectangle
			if (mIsSelectionRectangleOn)
			{
				mSelectionRectangle.X = Math.Min(e.X, mSelectionRectangleInitialPosition.X);
				mSelectionRectangle.Y = Math.Min(e.Y, mSelectionRectangleInitialPosition.Y);
				mSelectionRectangle.Width = Math.Abs(e.X - mSelectionRectangleInitialPosition.X);
				mSelectionRectangle.Height = Math.Abs(e.Y - mSelectionRectangleInitialPosition.Y);

				// compute the new selection rectangle in stud and call the selection on the map
				PointF topLeftCorner = GetScreenPointInStud(mSelectionRectangle.Location);
				PointF BottomRightCorner = GetScreenPointInStud(new Point(mSelectionRectangle.Right, mSelectionRectangle.Bottom));
				RectangleF selectionRectangeInStud = new RectangleF(topLeftCorner.X, topLeftCorner.Y, BottomRightCorner.X - topLeftCorner.X, BottomRightCorner.Y - topLeftCorner.Y);
				Map.Instance.SelectInRectangle(selectionRectangeInStud);

				// delete the selection rectangle
				mIsSelectionRectangleOn = false;
				// refresh the view
				mustRefreshView = true;
			}
			else if (mIsMouseHandledByMap)
			{
				// left button is handle by layers (so give it to the map)
				mustRefreshView = Map.Instance.MouseUp(e, mouseCoordInStud);
				mIsMouseHandledByMap = false;
			}
			else if (mIsZooming)
			{
				// the zoom is finished
				mIsZooming = false;
				// no need to update the view
			}
			else if (mIsScrolling)
			{
				// the scroll is finished
				mIsScrolling = false;
				// update the view at the end of the scroll
				mustRefreshView = true;
			}

			// restore the default cursor
			Cursor = GetDefaultCursor(mouseCoordInStud);

			// check if we need to update the view
			if (mustRefreshView)
				UpdateView();
		}

		private void MapPanel_DragEnter(object sender, DragEventArgs e)
		{
			// if the number of click is null, that means it can be a dragndrop from another view, such as the part lib
			// check if we need to search the image dropped, or if we already have it
			if (mCurrentPartDrop == null)
			{
				// by default do not accept the drop
				e.Effect = DragDropEffects.None;

				// ask the main window if one part was selected in the part lib
				// because the getData from the event doesn't work well under mono, normally it should be: e.Data.GetData(DataFormats.SystemString) as string
				string partDropNumber = (TopLevelControl as MainForm).GetDraggingPartNumberInPartLib();

				// check if we can add it
				Map.BrickAddability canAdd = Map.Instance.CanAddBrick(partDropNumber);
				if (canAdd == Map.BrickAddability.YES)
				{
					mBrickLayerThatReceivePartDrop = Map.Instance.SelectedLayer as LayerBrick;
					if (partDropNumber != null && mBrickLayerThatReceivePartDrop != null)
					{
						if (BrickLibrary.Instance.IsAGroup(partDropNumber))
							mCurrentPartDrop = new Layer.Group(partDropNumber);
						else
							mCurrentPartDrop = new LayerBrick.Brick(partDropNumber);
						mBrickLayerThatReceivePartDrop.addTemporaryPartDrop(mCurrentPartDrop);
						// set the effect in order to get the drop event
						e.Effect = DragDropEffects.Copy;
					}
				}
				else
					Map.Instance.GiveFeedbackForNotAddingBrick(canAdd);
			}

			// check again if we are not dragging a part, maybe we drag a file
			if (mCurrentPartDrop == null)
				(TopLevelControl as MainForm).MainForm_DragEnter(sender, e);
		}

		private void MapPanel_DragOver(object sender, DragEventArgs e)
		{
			// check if we are currently dragging a part
			if ((mCurrentPartDrop != null) && (mBrickLayerThatReceivePartDrop != null))
			{
				// memorise the position of the mouse snapped to the grid
				PointF mouseCoordInStud = GetScreenPointInStud(PointToClient(new Point(e.X, e.Y)));
				mCurrentPartDrop.Center = mBrickLayerThatReceivePartDrop.getMovedSnapPoint(mouseCoordInStud, mCurrentPartDrop);
				mBrickLayerThatReceivePartDrop.UpdateBoundingSelectionRectangle();
				// refresh the view
				UpdateView();
			}
		}

		private void MapPanel_DragDrop(object sender, DragEventArgs e)
		{
			if (mCurrentPartDrop != null)
			{
				// we have finished a dragndrop, remove the temporary part
				if (mBrickLayerThatReceivePartDrop != null)
				{
					mBrickLayerThatReceivePartDrop.removeTemporaryPartDrop(mCurrentPartDrop);
					mBrickLayerThatReceivePartDrop = null;
				}
				// and add the real new part
				Map.Instance.AddBrick(mCurrentPartDrop);
				// reset the dropping part number here and there
				mCurrentPartDrop = null;
				(TopLevelControl as MainForm).ResetDraggingPartNumberInPartLib();
				// refresh the view
				UpdateView();
				// and give the focus to the map panel such as if the user use the wheel to zoom
				// just after after the drop, the zoom is performed instead of the part lib scrolling
				Focus();
			}
			else
			{
				// if it is not a string, call the drag enter of the main app
				(TopLevelControl as MainForm).MainForm_DragDrop(sender, e);
			}
		}

		private void MapPanel_DragLeave(object sender, EventArgs e)
		{
			// if the user leave the panel while is was dropping a part,
			// just cancel the drop
			if (mCurrentPartDrop != null)
			{
				// remove and destroy the part
				if (mBrickLayerThatReceivePartDrop != null)
				{
					mBrickLayerThatReceivePartDrop.removeTemporaryPartDrop(mCurrentPartDrop);
					mBrickLayerThatReceivePartDrop = null;
				}
				mCurrentPartDrop = null;
				// update the view
				UpdateView();
			}
		}

		private void MapPanel_MouseEnter(object sender, EventArgs e)
		{
			// set the default cursor
			Cursor = GetDefaultCursor(PointF.Empty);
			// focus on the panel for handling the mouse scroll
			Focus();
		}

		public void MapPanel_MouseWheel(object sender, MouseEventArgs e)
		{
			// use the zoom cursor
			Cursor = MainForm.Instance.ZoomCursor;
			// and call the zoom function
			Zoom((float)(1.0f + (e.Delta * Settings.Default.WheelMouseZoomSpeed)),
				Settings.Default.WheelMouseIsZoomOnCursor, e.Location);
		}

		private void Pan(double deltaX, double deltaY)
		{
			// pan the view according to the delta
			mViewCornerX += deltaX;
			mViewCornerY += deltaY;
			// and update the scrollbar position
			UpdateScrollBarThumbFromViewCorner(true, true);
		}

		private void Zoom(float zoomFactor, bool zoomOnMousePosition, Point mousePosition)
		{
			// compute the center of the view in case of we need to zoom in the center
			Point screenCenterInPixel = new Point(ClientSize.Width / 2, ClientSize.Height / 2);

			// if the zoom must be performed on the mouse (or on the center of the screen),
			// we need to compute the point under the mouse (or under the center of the
			// screen) in stud coord system
			PointF previousZoomPointInStud;
			if (zoomOnMousePosition)
				previousZoomPointInStud = GetPointCoordInStud(mousePosition);
			else
				previousZoomPointInStud = GetPointCoordInStud(screenCenterInPixel);

			// compute the delta of the zoom according to the setting and
			// set the new scale by using the accessor to clamp and refresh the view
			ViewScale = mViewScale * zoomFactor;

			// recompute the new zoom point in the same way because the scaled changed
			// but the zoom point (mouse coord or center) in pixel didn't changed
			PointF newZoomPointInStud;
			if (zoomOnMousePosition)
				newZoomPointInStud = GetPointCoordInStud(mousePosition);
			else
				newZoomPointInStud = GetPointCoordInStud(screenCenterInPixel);

			// compute how much we should scroll the view to keep the same
			// point in stud under the mouse coord on screen
			Pan(previousZoomPointInStud.X - newZoomPointInStud.X, previousZoomPointInStud.Y - newZoomPointInStud.Y);
			UpdateView();
		}
		#endregion

		#region right click context menu
		private void ContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			// if the zoom modifier key is pressed, cancel the opening of the context menu
			if (mIsMouseHandledByMap || mIsZooming || (ModifierKeys == Settings.Default.MouseZoomPanKey))
			{
				e.Cancel = true;
			}
			else
			{
				Layer selectedLayer = Map.Instance.SelectedLayer;
				bool isThereAVisibleSelectedLayer = (selectedLayer != null) && selectedLayer.Visible;
				bool enableItemRelatedToSelection = isThereAVisibleSelectedLayer && (selectedLayer.SelectedObjects.Count > 0);
				bringToFrontToolStripMenuItem.Enabled = enableItemRelatedToSelection;
				sendToBackToolStripMenuItem.Enabled = enableItemRelatedToSelection;
				selectAllToolStripMenuItem.Enabled = isThereAVisibleSelectedLayer && selectedLayer.HasSomethingToSelect;
				deselectAllToolStripMenuItem.Enabled = enableItemRelatedToSelection;
				selectPathToolStripMenuItem.Visible = selectedLayer is LayerBrick;
				selectPathToolStripMenuItem.Enabled = isThereAVisibleSelectedLayer && (selectedLayer.SelectedObjects.Count >= 2);
				if (isThereAVisibleSelectedLayer)
				{
					groupToolStripMenuItem.Enabled = Actions.Items.GroupItems.findItemsToGroup(selectedLayer.SelectedObjects).Count > 1;
					ungroupToolStripMenuItem.Enabled = Actions.Items.UngroupItems.findItemsToUngroup(selectedLayer.SelectedObjects).Count > 0;
				}
				else
				{
					groupToolStripMenuItem.Enabled = false;
					ungroupToolStripMenuItem.Enabled = false;
				}
				// check if the current layer is of type ruler
				bool isSelectedLayerRuler = isThereAVisibleSelectedLayer && (selectedLayer is LayerRuler);
				bool isSelectedLayerText = isThereAVisibleSelectedLayer && (selectedLayer is LayerText);
				attachRulerToolStripSeparator.Visible = isSelectedLayerRuler;
				attachToolStripMenuItem.Visible = isSelectedLayerRuler;
				detachToolStripMenuItem.Visible = isSelectedLayerRuler;
				useAsModelToolStripMenuItem.Visible = isSelectedLayerRuler || isSelectedLayerText;
				useAsModelToolStripMenuItem.Enabled = (isSelectedLayerRuler || isSelectedLayerText) && (selectedLayer.SelectedObjects.Count == 1);
				if (isSelectedLayerRuler)
				{
					LayerRuler rulerLayer = selectedLayer as LayerRuler;
					attachToolStripMenuItem.Enabled = rulerLayer.CanAttachRuler();
					detachToolStripMenuItem.Enabled = rulerLayer.CanDetachRuler();
				}
				else
				{
					attachToolStripMenuItem.Enabled = false;
					detachToolStripMenuItem.Enabled = false;
				}

				// check is we need to enable the properties
				propertiesToolStripMenuItem.Enabled = enableItemRelatedToSelection && ((selectedLayer is LayerRuler) || (selectedLayer is LayerText) || (selectedLayer is LayerBrick));

				// update the check mark of the scrollbar depending on the current state of the scrollbar
				scrollBarToolStripMenuItem.Checked = horizontalScrollBar.Visible || verticalScrollBar.Visible;

				// finally after enabling the context menu items
				// check if at leat one toolstrip menu item is enabled otherwise, cancel the opening
				bool isEnabled = false;
				foreach (ToolStripItem stripItem in ContextMenuStrip.Items)
					if (stripItem is ToolStripMenuItem)
						isEnabled = isEnabled || stripItem.Enabled;
				// do we cancel it? yes if none is enabled
				e.Cancel = !isEnabled;
			}
		}

		private void BringToFrontToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MainForm.Instance.BringToFrontToolStripMenuItem_Click(sender, e);
		}

		private void SendToBackToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MainForm.Instance.SendToBackToolStripMenuItem_Click(sender, e);
		}

		private void SelectAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MainForm.Instance.SelectAllToolStripMenuItem_Click(sender, e);
		}

		private void DeselectAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MainForm.Instance.DeselectAllToolStripMenuItem_Click(sender, e);
		}

		private void SelectPathToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MainForm.Instance.SelectPathToolStripMenuItem_Click(sender, e);
		}

		private void GroupToolStripMenuItem_Click(object sender, EventArgs e)
		{
            MainForm.Instance.GroupToolStripMenuItem_Click(sender, e);
		}

		private void UngroupToolStripMenuItem_Click(object sender, EventArgs e)
		{
            MainForm.Instance.UngroupToolStripMenuItem_Click(sender, e);
		}

		private void AttachToolStripMenuItem_Click(object sender, EventArgs e)
		{
            if (Map.Instance.SelectedLayer is LayerRuler rulerLayer)
                ActionManager.Instance.doAction(new Actions.Rulers.AttachRulerToBrick(rulerLayer.CurrentRulerWithHighlightedControlPoint, rulerLayer.CurrentBrickUsedForRulerAttachement));
        }

		private void DetachToolStripMenuItem_Click(object sender, EventArgs e)
		{
            if (Map.Instance.SelectedLayer is LayerRuler rulerLayer)
                ActionManager.Instance.doAction(new Actions.Rulers.DetachRuler(rulerLayer.CurrentRulerWithHighlightedControlPoint, rulerLayer.CurrentBrickUsedForRulerAttachement));
        }

		private void UseAsModelToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// this action (i.e. changing the settings) is not undoable
			if (Map.Instance.SelectedLayer is LayerRuler)
			{
				var rulerLayer = Map.Instance.SelectedLayer as LayerRuler;
				if (rulerLayer.SelectedObjects.Count == 1)
				{
                    if (rulerLayer.SelectedObjects[0] is LayerRuler.RulerItem item)
                        PreferencesForm.sChangeRulerSettingsFromRuler(item);
                }
			}
			else if (Map.Instance.SelectedLayer is LayerText)
			{
				var textLayer = Map.Instance.SelectedLayer as LayerText;
				if (textLayer.SelectedObjects.Count == 1)
				{
                    if (textLayer.SelectedObjects[0] is LayerText.TextCell item)
                        PreferencesForm.sChangeTextSettingsFromText(item);
                }
			}
		}

		private void PropertiesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Map.Instance.EditSelectedItemsProperties(mLastDownMouseCoordInStud);
		}

		private void ScrollBarToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// Warn Main form to update its menu item for the scroll bars
			MainForm.Instance.MapScrollBarsVisibilityChangeNotification(scrollBarToolStripMenuItem.Checked);
			// show or hide the scrollbar
			ShowHideScrollBars(scrollBarToolStripMenuItem.Checked);
		}
		#endregion

		#region scrollbars

		/// <summary>
		/// This notification function should be called when the total area of the map (including text and rulers) changed.
		/// This include when an item is added, removed, or moved, when a layer is deleted or readded, etc...
		/// </summary>
		public void MapAreaChangedNotification()
		{
			// recompute the total area, but only if the scrollbars are visible, otherwise it's useless
			if (horizontalScrollBar.Visible || verticalScrollBar.Visible)
			{
				// recompute the map area
				mMapTotalAreaInStud = Map.Instance.GetTotalAreaInStud(false);
				// update the scrollbars size (if they are not visible, nothing happen)
				UpdateScrollbarSize();
			}
		}

		public void ShowHideScrollBars(bool isVisible)
		{
			// show or hide the two scrollbars
			horizontalScrollBar.Visible = isVisible;
			verticalScrollBar.Visible = isVisible;

			// call the map area change notification in order to recompute the area size and update the scrollbar size
			MapAreaChangedNotification();
		}

		private void UpdateScrollBarThumbFromViewCorner(bool updateX, bool updateY)
		{
			// update the scrollbar values
			if (updateX)
			{
				int newValue = (int)((mViewCornerX - mMapTotalAreaInStud.Left + mScrollBarAddedMarginInStud) * mViewScale / Size.Width * mMapScrollBarSliderSize);
				horizontalScrollBar.Value = Math.Max(Math.Min(newValue, horizontalScrollBar.Maximum), horizontalScrollBar.Minimum);
			}
			if (updateY)
			{
				int newValue = (int)((mViewCornerY - mMapTotalAreaInStud.Top + mScrollBarAddedMarginInStud) * mViewScale / Size.Height * mMapScrollBarSliderSize);
				verticalScrollBar.Value = Math.Max(Math.Min(newValue, verticalScrollBar.Maximum), verticalScrollBar.Minimum);
			}
		}

	private void UpdateViewCornerFromScrollBarThumb(bool updateX, bool updateY)
		{
			// update the view corner
			if (updateX)
				mViewCornerX = ((double)horizontalScrollBar.Value / mMapScrollBarSliderSize * Size.Width / mViewScale) + mMapTotalAreaInStud.Left - mScrollBarAddedMarginInStud;
			if (updateY)
				mViewCornerY = ((double)verticalScrollBar.Value / mMapScrollBarSliderSize * Size.Height / mViewScale) + mMapTotalAreaInStud.Top - mScrollBarAddedMarginInStud;
		}

		private void UpdateScrollbarSize()
		{
			// does nothing if the scrollbars are not visible
			if (horizontalScrollBar.Visible || verticalScrollBar.Visible)
			{
				// compute the total area in screen pixel (from studs)
				double totalWidthInPixel = (mMapTotalAreaInStud.Right - mMapTotalAreaInStud.Left + (mScrollBarAddedMarginInStud * 2)) * mViewScale;
				double totalHeightInPixel = (mMapTotalAreaInStud.Bottom - mMapTotalAreaInStud.Top + (mScrollBarAddedMarginInStud * 2)) * mViewScale;

				// compute how many screens are needed to display the total map area, that will define how long should be the scroll bar
				double screenCountToDisplayTotalWidth = totalWidthInPixel / Size.Width;
				double screenCountToDisplayTotalHeight = totalHeightInPixel / Size.Height;

				// set the maximum of the scroll bars
				horizontalScrollBar.Maximum = (int)(mMapScrollBarSliderSize * screenCountToDisplayTotalWidth);
				horizontalScrollBar.LargeChange = mMapScrollBarSliderSize;
				verticalScrollBar.Maximum = (int)(mMapScrollBarSliderSize * screenCountToDisplayTotalHeight);
				verticalScrollBar.LargeChange = mMapScrollBarSliderSize;

				// set the value of the scrollbar depending on the current view
				UpdateScrollBarThumbFromViewCorner(true, true);
			}
		}

		private void HorizontalScrollBar_Scroll(object sender, ScrollEventArgs e)
		{
			UpdateViewCornerFromScrollBarThumb(true, false);
			Invalidate();
		}

		private void VerticalScrollBar_Scroll(object sender, ScrollEventArgs e)
		{
			UpdateViewCornerFromScrollBarThumb(false, true);
			Invalidate();
		}
		private void VerticalScrollBar_MouseEnter(object sender, EventArgs e)
		{
			Cursor = DefaultCursor;
		}

		private void HorizontalScrollBar_MouseEnter(object sender, EventArgs e)
		{
			Cursor = DefaultCursor;
		}

		private void MapPanel_Resize(object sender, EventArgs e)
		{
			UpdateScrollbarSize();
		}

		private void MapPanel_SizeChanged(object sender, EventArgs e)
		{
			UpdateScrollbarSize();
		}
		#endregion
	}
}