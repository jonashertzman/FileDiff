using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace FileDiff
{
	public class DiffControl : Control
	{

		#region Members

		private double characterHeight;
		private double characterWidth;
		private double lineNumberMargin;
		private double textMargin;
		private double maxTextwidth = 0;

		private bool cursorBlink = true;

		private int downLine;
		private int downCharacter;

		private double dpiScale = 0;

		private int cursorLine = 0;
		private int cursorCharacter = 0;

		private Typeface typeface;

		private readonly DispatcherTimer blinkTimer = new DispatcherTimer(DispatcherPriority.Render);
		private readonly Stopwatch stopwatch = new Stopwatch();

		private Point mouseHoverPosition;

		#endregion

		#region Constructor

		static DiffControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(DiffControl), new FrameworkPropertyMetadata(typeof(DiffControl)));
		}

		public DiffControl()
		{
			this.ClipToBounds = true;

			blinkTimer.Interval = new TimeSpan(5000000);
			blinkTimer.Tick += BlinkTimer_Tick;
			blinkTimer.Start();

			typeface = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);
		}

		#endregion

		#region Properties

		private Selection _selection = null;
		private Selection Selection
		{
			get { return _selection; }
			set { _selection = value; ResetCursorBlink(); }
		}

		private Point? _mouseDownPosition = null;
		private Point? MouseDownPosition
		{
			get
			{
				return _mouseDownPosition;
			}
			set
			{
				_mouseDownPosition = value;
				if (value != null)
				{
					PointToCharacter(value.Value, out downLine, out downCharacter);
				}
			}
		}

		#endregion

		#region Overrides

		protected override void OnRender(DrawingContext drawingContext)
		{
			Debug.Print("DiffControl OnRender");

#if DEBUG
			MeasureRendeTime();
#endif

			// Fill background
			drawingContext.DrawRectangle(AppSettings.FullMatchBackground, null, new Rect(0, 0, this.ActualWidth, this.ActualHeight));

			if (Lines.Count == 0)
				return;

			typeface = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);

			Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
			dpiScale = 1 / m.M11;

			TextUtils.CreateGlyphRun("W", typeface, this.FontSize, dpiScale, out characterWidth);
			characterHeight = Math.Ceiling(TextUtils.FontHeight(typeface, this.FontSize, dpiScale) / dpiScale) * dpiScale;

			Brush activeDiffBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220));
			activeDiffBrush.Freeze();

			Pen borderPen = new Pen(SystemColors.ScrollBarBrush, RoundToWholePixels(1));
			borderPen.Freeze();
			GuidelineSet borderGuide = CreateGuidelineSet(borderPen);

			textMargin = RoundToWholePixels(4);
			lineNumberMargin = RoundToWholePixels(characterWidth * Lines.Count.ToString().Length) + (2 * textMargin) + borderPen.Thickness;

			VisibleLines = (int)(ActualHeight / characterHeight + 1);
			MaxVerialcalScroll = Lines.Count - VisibleLines + 1;

			drawingContext.DrawRectangle(SystemColors.ControlBrush, null, new Rect(0, 0, lineNumberMargin, this.ActualHeight));

			for (int i = 0; i < VisibleLines; i++)
			{
				int lineIndex = i + VerticalOffset;

				if (lineIndex >= Lines.Count)
					break;

				Line line = Lines[lineIndex];

				// Line Y offset
				drawingContext.PushTransform(new TranslateTransform(0, characterHeight * i));
				{
					// Draw line number
					SolidColorBrush lineNumberColor = SystemColors.ControlDarkBrush;

					if (lineIndex >= CurrentDiff && lineIndex < CurrentDiff + CurrentDiffLength && !Edited)
					{
						lineNumberColor = SystemColors.ControlDarkDarkBrush;
						drawingContext.DrawRectangle(activeDiffBrush, null, new Rect(0, 0, lineNumberMargin, characterHeight));
					}

					// Draw line background
					if (line.Type != TextState.FullMatch)
					{
						drawingContext.DrawRectangle(line.BackgroundBrush, null, new Rect(lineNumberMargin, 0, Math.Max(this.ActualWidth - lineNumberMargin, 0), characterHeight));
					}

					if (line.LineIndex != null)
					{
						GlyphRun rowNumberRun = line.GetRenderedLineIndexText(typeface, this.FontSize, dpiScale, out double rowNumberWidth);

						drawingContext.PushTransform(new TranslateTransform(lineNumberMargin - rowNumberWidth - textMargin - borderPen.Thickness, 0));
						drawingContext.DrawGlyphRun(lineNumberColor, rowNumberRun);
						drawingContext.Pop();
					}

					// Text clipping rect
					drawingContext.PushClip(new RectangleGeometry(new Rect(lineNumberMargin + textMargin, 0, Math.Max(ActualWidth - lineNumberMargin - textMargin * 2, 0), ActualHeight)));
					{
						// Line X offset
						drawingContext.PushTransform(new TranslateTransform(lineNumberMargin + textMargin - HorizontalOffset, 0));
						{
							// Draw line
							if (line.Text != "")
							{
								double nextPosition = 0;
								foreach (TextSegment textSegment in line.TextSegments)
								{
									drawingContext.PushTransform(new TranslateTransform(nextPosition, 0));

									GlyphRun segmentRun = textSegment.GetRenderedText(typeface, this.FontSize, dpiScale, AppSettings.ShowWhiteSpaceCharacters, AppSettings.TabSize, out double runWidth);

									if (nextPosition - HorizontalOffset < ActualWidth && nextPosition + runWidth - HorizontalOffset > 0)
									{
										if (line.Type != textSegment.Type && AppSettings.ShowLineChanges)
										{
											drawingContext.DrawRectangle(textSegment.BackgroundBrush, null, new Rect(nextPosition == 0 ? -textMargin : 0, 0, runWidth + (nextPosition == 0 ? textMargin : 0), characterHeight));
										}

										drawingContext.DrawGlyphRun(AppSettings.ShowLineChanges ? textSegment.ForegroundBrush : line.ForegroundBrush, segmentRun);
									}
									nextPosition += runWidth;

									drawingContext.Pop();
								}
								maxTextwidth = Math.Max(maxTextwidth, nextPosition);
							}

							// Draw cursor
							if (EditMode && this.IsFocused && cursorLine == lineIndex && cursorBlink)
							{
								drawingContext.DrawRectangle(Brushes.Black, null, new Rect(CharacterPosition(lineIndex, cursorCharacter), 0, RoundToWholePixels(1), characterHeight));
							}
						}
						drawingContext.Pop(); // Line X offset
					}
					drawingContext.Pop(); // Text clipping rect

					// Text area clipping rect
					drawingContext.PushClip(new RectangleGeometry(new Rect(lineNumberMargin, 0, Math.Max(ActualWidth - lineNumberMargin, 0), ActualHeight)));
					{
						// Line X offset 2
						drawingContext.PushTransform(new TranslateTransform(lineNumberMargin + textMargin - HorizontalOffset, 0));
						{
							// Draw selection
							if (Selection != null && lineIndex >= Selection.TopLine && lineIndex <= Selection.BottomLine)
							{
								Rect selectionRect = new Rect(0 - textMargin + HorizontalOffset, 0, this.ActualWidth + HorizontalOffset, characterHeight);
								if (Selection.TopLine == lineIndex && Selection.TopCharacter > 0)
								{
									selectionRect.X = Math.Max(0, CharacterPosition(lineIndex, Selection.TopCharacter));
								}
								if (Selection.BottomLine == lineIndex)
								{
									selectionRect.Width = Math.Max(0, CharacterPosition(lineIndex, Selection.BottomCharacter) - selectionRect.X);
								}
								drawingContext.DrawRectangle(AppSettings.SelectionBackground, null, selectionRect);
							}
						}
						drawingContext.Pop(); // Line X offset 2
					}
					drawingContext.Pop(); // Text area clipping rect
				}
				drawingContext.Pop(); // Line Y offset
			}

			// Draw line number border
			drawingContext.PushGuidelineSet(borderGuide);
			{
				drawingContext.DrawLine(borderPen, new Point(lineNumberMargin, -1), new Point(lineNumberMargin, this.ActualHeight));
			}
			drawingContext.Pop();

			TextAreaWidth = (int)(ActualWidth - lineNumberMargin - (textMargin * 2));
			MaxHorizontalScroll = (int)(maxTextwidth - TextAreaWidth + textMargin);

#if DEBUG
			ReportRenderTime();
#endif
		}

		protected override void OnTextInput(TextCompositionEventArgs e)
		{
			if (EditMode && e.Text.Length > 0 && Lines.Count > 0)
			{
				if (e.Text == "\b")
				{
					if (Selection != null)
					{
						DeleteSelection();
					}
					else
					{
						if (cursorCharacter == 0)
						{
							if (cursorLine > 0)
							{
								cursorCharacter = Lines[cursorLine - 1].Text.Length;
								SetLineText(cursorLine - 1, Lines[cursorLine - 1].Text + Lines[cursorLine].Text);
								RemoveLine(cursorLine);
								cursorLine--;
							}
						}
						else
						{
							int chars = char.IsLowSurrogate(Lines[cursorLine].Text[cursorCharacter - 1]) ? 2 : 1;
							SetLineText(cursorLine, Lines[cursorLine].Text.Substring(0, cursorCharacter - chars) + Lines[cursorLine].Text.Substring(cursorCharacter));
							cursorCharacter -= chars;
						}
					}
				}
				else if (e.Text == "\r")
				{
					if (Selection != null)
					{
						DeleteSelection();
					}
					InsertNewLine(cursorLine + 1, Lines[cursorLine].Text.Substring(cursorCharacter));
					SetLineText(cursorLine, Lines[cursorLine].Text.Substring(0, cursorCharacter));
					cursorLine++;
					cursorCharacter = 0;
				}
				else
				{
					if (Selection != null)
					{
						DeleteSelection();
					}
					SetLineText(cursorLine, Lines[cursorLine].Text.Substring(0, cursorCharacter) + e.Text + Lines[cursorLine].Text.Substring(cursorCharacter));
					cursorCharacter++;
				}
			}
			else
			{
				base.OnTextInput(e);
				return;
			}

			this.InvalidateVisual();
			EnsureCursorVisibility();
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (Lines.Count == 0)
			{
				return;
			}

			bool controlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
			bool shiftPressed = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;

			if (e.Key == Key.Delete && EditMode)
			{
				if (Selection != null)
				{
					DeleteSelection();
				}
				else if (shiftPressed)
				{
					RemoveLine(cursorLine);
					if (cursorLine >= Lines.Count)
					{
						cursorLine = Math.Max(Lines.Count - 1, 0);
						cursorCharacter = 0;
					}
				}
				else
				{
					if (cursorCharacter == Lines[cursorLine].Text.Length)
					{
						if (cursorLine < Lines.Count - 1)
						{
							SetLineText(cursorLine, Lines[cursorLine].Text + Lines[cursorLine + 1].Text);
							RemoveLine(cursorLine + 1);
						}
					}
					else
					{
						int chars = char.IsHighSurrogate(Lines[cursorLine].Text[cursorCharacter]) ? 2 : 1;
						SetLineText(cursorLine, Lines[cursorLine].Text.Substring(0, cursorCharacter) + Lines[cursorLine].Text.Substring(cursorCharacter + chars));
					}
				}
				EnsureCursorVisibility();
			}
			else if (e.Key == Key.Tab && EditMode)
			{
				if (Selection != null && Selection.TopLine < Selection.BottomLine)
				{
					for (int i = Selection.TopLine; i <= Selection.BottomLine; i++)
					{
						if (shiftPressed)
						{
							if (Lines[i].Text.Length > 0 && Lines[i].Text[0] == '\t')
							{
								Lines[i].Text = Lines[i].Text.Remove(0, 1);
							}
						}
						else
						{
							Lines[i].Text = "\t" + Lines[i].Text;
						}
					}
					Selection = new Selection(Selection.TopLine, 0, Selection.BottomLine, Lines[Selection.BottomLine].Text.Length);
					Edited = true;
				}
				else if (Selection != null && Selection.TopLine == Selection.BottomLine)
				{
					DeleteSelection();
					SetLineText(cursorLine, Lines[cursorLine].Text.Substring(0, cursorCharacter) + "\t" + Lines[cursorLine].Text.Substring(cursorCharacter));
					cursorCharacter++;
					EnsureCursorVisibility();
				}
				else
				{
					SetLineText(cursorLine, Lines[cursorLine].Text.Substring(0, cursorCharacter) + "\t" + Lines[cursorLine].Text.Substring(cursorCharacter));
					cursorCharacter++;
					EnsureCursorVisibility();
				}
			}
			else if (e.Key == Key.Escape && Selection != null)
			{
				Selection = null;
			}
			else if (e.Key == Key.A && controlPressed)
			{
				if (Lines.Count > 0)
				{
					if (EditMode)
					{
						Selection = null;
						cursorLine = 0;
						cursorCharacter = 0;
						SetCursorPosition(Lines.Count - 1, Math.Max(0, Lines[Lines.Count - 1].Text.Length), true);
					}
					else
					{
						Selection = new Selection(0, 0, Lines.Count - 1, Math.Max(0, Lines[Lines.Count - 1].Text.Length));
					}
				}
			}
			else if (e.Key == Key.C && controlPressed)
			{
				if (Selection != null)
				{
					CopyToClipboard();
				}
			}
			else if (e.Key == Key.X && controlPressed && EditMode)
			{
				if (Selection != null)
				{
					CopyToClipboard();
					DeleteSelection();
					EnsureCursorVisibility();
				}
			}
			else if (e.Key == Key.V && controlPressed && EditMode)
			{
				if (Clipboard.ContainsText())
				{
					if (Selection != null)
					{
						DeleteSelection();
					}

					string[] pastedRows = Clipboard.GetText().Split(new string[] { "\r\n" }, StringSplitOptions.None);

					string leftOfCursor = Lines[cursorLine].Text.Substring(0, cursorCharacter);
					string rightOfCursor = Lines[cursorLine].Text.Substring(cursorCharacter);

					for (int i = 0; i < pastedRows.Length; i++)
					{
						if (i > 0)
						{
							InsertNewLine(cursorLine, "");
						}

						SetLineText(cursorLine, (i == 0 ? leftOfCursor : "") + pastedRows[i] + (i == pastedRows.Length - 1 ? rightOfCursor : ""));
						cursorCharacter = (i == 0 ? leftOfCursor.Length : 0) + pastedRows[i].Length;

						if (i < pastedRows.Length - 1)
						{
							cursorLine++;
						}
					}
					EnsureCursorVisibility();
				}
			}
			else if (e.Key == Key.PageUp && !controlPressed)
			{
				if (EditMode)
				{
					int line = Math.Max(cursorLine - VisibleLines, 0);
					SetCursorPosition(line, Math.Min(cursorCharacter, Lines[line].Text.Length), shiftPressed);
				}
				else
				{
					VerticalOffset = Math.Max(0, VerticalOffset -= VisibleLines - 1);
				}
			}
			else if (e.Key == Key.PageDown && !controlPressed)
			{
				if (EditMode)
				{
					int line = Math.Min(cursorLine + VisibleLines, Lines.Count - 1);
					SetCursorPosition(line, Math.Min(cursorCharacter, Lines[line].Text.Length), shiftPressed);
				}
				else
				{
					VerticalOffset = VerticalOffset += VisibleLines - 1;
				}
			}
			else if (e.Key == Key.Home)
			{
				if (controlPressed)
				{
					VerticalOffset = 0;
					if (EditMode)
					{
						SetCursorPosition(0, 0, shiftPressed);
					}
				}
				else
				{
					HorizontalOffset = 0;
					if (EditMode)
					{
						SetCursorPosition(cursorLine, 0, shiftPressed);
					}
				}
			}
			else if (e.Key == Key.End)
			{
				if (controlPressed)
				{
					VerticalOffset = Lines.Count;
					if (EditMode)
					{
						SetCursorPosition(Lines.Count - 1, Lines[Lines.Count - 1].Text.Length, shiftPressed);
					}
				}
				else
				{
					if (EditMode)
					{
						SetCursorPosition(cursorLine, Lines[cursorLine].Text.Length, shiftPressed);
					}
				}
			}
			else if (e.Key == Key.Down && EditMode)
			{
				if (cursorLine < Lines.Count - 1)
				{
					SetCursorPosition(cursorLine + 1, Math.Min(cursorCharacter, Lines[cursorLine + 1].Text.Length), shiftPressed);
				}
			}
			else if (e.Key == Key.Up && EditMode)
			{
				if (cursorLine > 0)
				{
					SetCursorPosition(cursorLine - 1, Math.Min(cursorCharacter, Lines[cursorLine - 1].Text.Length), shiftPressed);
				}
			}
			else if (e.Key == Key.Left)
			{
				if (EditMode)
				{
					if (cursorCharacter == 0)
					{
						if (cursorLine > 0)
						{
							SetCursorPosition(cursorLine - 1, Lines[cursorLine - 1].Text.Length, shiftPressed);
						}
					}
					else
					{
						int step = 1;
						if (controlPressed)
						{
							while (cursorCharacter - step > 0 && char.IsLetterOrDigit(Lines[cursorLine].Text[cursorCharacter - step]))
							{
								step++;
							}
						}
						if (char.IsLowSurrogate(Lines[cursorLine].Text[cursorCharacter - step]))
						{
							step++;
						}
						SetCursorPosition(cursorLine, cursorCharacter - step, shiftPressed);
					}
				}
				else if (controlPressed)
				{
					e.Handled = false;
					base.OnKeyDown(e);
					return;
				}
			}
			else if (e.Key == Key.Right)
			{
				if (EditMode)
				{
					if (cursorCharacter >= Lines[cursorLine].Text.Length)
					{
						if (cursorLine < Lines.Count - 1)
						{
							SetCursorPosition(cursorLine + 1, 0, shiftPressed);
						}
					}
					else
					{
						int step = 1;
						if (controlPressed)
						{
							while (cursorCharacter + step < Lines[cursorLine].Text.Length && char.IsLetterOrDigit(Lines[cursorLine].Text[cursorCharacter + step]))
							{
								step++;
							}
						}
						if (char.IsHighSurrogate(Lines[cursorLine].Text[cursorCharacter + step - 1]))
						{
							step++;
						}
						SetCursorPosition(cursorLine, cursorCharacter + step, shiftPressed);
					}
				}
				else if (controlPressed)
				{
					e.Handled = false;
					base.OnKeyDown(e);
					return;
				}
			}
			else
			{
				base.OnKeyDown(e);
				return;
			}

			e.Handled = true;
			InvalidateVisual();
		}

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			this.Focus();

			if (e.ChangedButton == MouseButton.Left)
			{
				MouseDownPosition = e.GetPosition(this);
			}

			base.OnMouseDown(e);
		}

		protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
		{
			this.Focus();

			if (e.ChangedButton == MouseButton.Left && Lines.Count > 0)
			{
				MouseDownPosition = null;
				PointToCharacter(e.GetPosition(this), out downLine, out downCharacter);

				int left = 0;
				int right = 1;
				while (downCharacter - left > 0 && char.IsLetterOrDigit(Lines[downLine].Text[downCharacter - left - 1]))
				{
					left++;
				}

				while (downCharacter + right < Lines[downLine].Text.Length && char.IsLetterOrDigit(Lines[downLine].Text[downCharacter + right]))
				{
					right++;
				}

				SetCursorPosition(downLine, downCharacter - left, false);
				SetCursorPosition(downLine, downCharacter + right, true);

				InvalidateVisual();
			}

			base.OnMouseDoubleClick(e);
		}

		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			Point currentMousePosition = e.GetPosition(this);

			PointToCharacter(currentMousePosition, out cursorLine, out cursorCharacter);

			if (e.ChangedButton == MouseButton.Left && MouseDownPosition != null && Lines.Count > 0)
			{

				if (currentMousePosition != MouseDownPosition || currentMousePosition.X < lineNumberMargin)
				{
					PointToCharacter(currentMousePosition, out int upLine, out int upCharacter);

					if (MouseDownPosition.Value.X < lineNumberMargin || currentMousePosition.X < lineNumberMargin)
					{
						downCharacter = upLine < downLine ? Lines[downLine].Text.Length : 0;
						upCharacter = upLine < downLine ? 0 : Lines[upLine].Text.Length;
					}

					Selection = new Selection(downLine, downCharacter, upLine, upCharacter);
				}
				else
				{
					Selection = null;
				}

				MouseDownPosition = null;
				InvalidateVisual();
			}

			base.OnMouseUp(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			Point currentMousePosition = e.GetPosition(this);

			if (Mouse.LeftButton == MouseButtonState.Pressed && MouseDownPosition != null && Lines.Count > 0)
			{
				PointToCharacter(currentMousePosition, out cursorLine, out cursorCharacter);
				PointToCharacter(currentMousePosition, out int upLine, out int upCharacter);

				int selectionStartCharacter = downCharacter;

				if (MouseDownPosition.Value.X < lineNumberMargin || currentMousePosition.X < lineNumberMargin)
				{
					selectionStartCharacter = upLine < downLine ? Lines[downLine].Text.Length : 0;
					upCharacter = upLine < downLine ? 0 : Lines[upLine].Text.Length;
				}

				Selection = new Selection(downLine, selectionStartCharacter, upLine, upCharacter);

				InvalidateVisual();
			}

			mouseHoverPosition = currentMousePosition;

			base.OnMouseMove(e);
		}

		protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			InvalidateVisual();

			base.OnLostKeyboardFocus(e);
		}

		protected override void OnToolTipOpening(ToolTipEventArgs e)
		{
			if (Lines.Count > 0)
			{
				int lineIndex = (int)(mouseHoverPosition.Y / characterHeight) + VerticalOffset;

				if (lineIndex < Lines.Count)
				{
					if (Lines[lineIndex].Type == TextState.MovedFrom)
					{
						this.ToolTip = $"Matches new lines at index {Lines[lineIndex].MatchingLineIndex}";
						return;
					}
					else if (Lines[lineIndex].Type == TextState.MovedTo)
					{
						this.ToolTip = $"Matches removed lines at index {Lines[lineIndex].MatchingLineIndex}";
						return;
					}
				}
			}
			e.Handled = true;
		}

		#endregion

		#region Dependency Properties

		public static readonly DependencyProperty LinesProperty = DependencyProperty.Register("Lines", typeof(ObservableCollection<Line>), typeof(DiffControl), new FrameworkPropertyMetadata(new ObservableCollection<Line>(), FrameworkPropertyMetadataOptions.AffectsRender));

		public ObservableCollection<Line> Lines
		{
			get { return (ObservableCollection<Line>)GetValue(LinesProperty); }
			set { SetValue(LinesProperty, value); }
		}


		public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register("VerticalOffset", typeof(int), typeof(DiffControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

		public int VerticalOffset
		{
			get { return (int)GetValue(VerticalOffsetProperty); }
			set { SetValue(VerticalOffsetProperty, value); }
		}


		public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register("HorizontalOffset", typeof(int), typeof(DiffControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

		public int HorizontalOffset
		{
			get { return (int)GetValue(HorizontalOffsetProperty); }
			set { SetValue(HorizontalOffsetProperty, value); }
		}


		public static readonly DependencyProperty MaxHorizontalScrollPropery = DependencyProperty.Register("MaxHorizontalScroll", typeof(int), typeof(DiffControl));

		public int MaxHorizontalScroll
		{
			get { return (int)GetValue(MaxHorizontalScrollPropery); }
			set { SetValue(MaxHorizontalScrollPropery, value); }
		}


		public static readonly DependencyProperty TextAreaWidthPropery = DependencyProperty.Register("TextAreaWidth", typeof(int), typeof(DiffControl));

		public int TextAreaWidth
		{
			get { return (int)GetValue(TextAreaWidthPropery); }
			set { SetValue(TextAreaWidthPropery, value); }
		}


		public static readonly DependencyProperty CurrentDiffProperty = DependencyProperty.Register("CurrentDiff", typeof(int), typeof(DiffControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

		public int CurrentDiff
		{
			get { return (int)GetValue(CurrentDiffProperty); }
			set { SetValue(CurrentDiffProperty, value); }
		}


		public static readonly DependencyProperty CurrentDiffLengthProperty = DependencyProperty.Register("CurrentDiffLength", typeof(int), typeof(DiffControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

		public int CurrentDiffLength
		{
			get { return (int)GetValue(CurrentDiffLengthProperty); }
			set { SetValue(CurrentDiffLengthProperty, value); }
		}


		public static readonly DependencyProperty VisibleLinesProperty = DependencyProperty.Register("VisibleLines", typeof(int), typeof(DiffControl));

		public int VisibleLines
		{
			get { return (int)GetValue(VisibleLinesProperty); }
			set { SetValue(VisibleLinesProperty, value); }
		}


		public static readonly DependencyProperty MaxVerialcalScrollProperty = DependencyProperty.Register("MaxVerialcalScroll", typeof(int), typeof(DiffControl));

		public int MaxVerialcalScroll
		{
			get { return (int)GetValue(MaxVerialcalScrollProperty); }
			set { SetValue(MaxVerialcalScrollProperty, value); }
		}


		public static readonly DependencyProperty EditedProperty = DependencyProperty.Register("Edited", typeof(bool), typeof(DiffControl));

		public bool Edited
		{
			get { return (bool)GetValue(EditedProperty); }
			set { SetValue(EditedProperty, value); }
		}


		public static readonly DependencyProperty EditModeProperty = DependencyProperty.Register("EditMode", typeof(bool), typeof(DiffControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

		public bool EditMode
		{
			get { return (bool)GetValue(EditModeProperty); }
			set { SetValue(EditModeProperty, value); }
		}


		public static readonly DependencyProperty UpdateTriggerProperty = DependencyProperty.Register("UpdateTrigger", typeof(int), typeof(DiffControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

		public int UpdateTrigger
		{
			get { return (int)GetValue(UpdateTriggerProperty); }
			set { SetValue(UpdateTriggerProperty, value); }
		}

		#endregion

		#region Methods

		internal void Init()
		{
			Selection = null;
			VerticalOffset = 0;
			HorizontalOffset = 0;
			TextAreaWidth = 0;
			MaxHorizontalScroll = 0;
			maxTextwidth = 0;
			cursorLine = 0;
			cursorCharacter = 0;

			if (Lines.Count == 0)
			{
				InsertNewLine(0, "");
				Edited = false;
			}
		}

		private GuidelineSet CreateGuidelineSet(Pen pen)
		{
			GuidelineSet guidelineSet = new GuidelineSet();
			guidelineSet.GuidelinesX.Add(pen.Thickness / 2);
			guidelineSet.GuidelinesY.Add(pen.Thickness / 2);
			guidelineSet.Freeze();

			return guidelineSet;
		}

		private void DeleteSelection()
		{
			for (int index = Selection.BottomLine; index >= Selection.TopLine; index--)
			{
				if (Selection.TopLine == Selection.BottomLine)
				{
					SetLineText(index, Lines[index].Text.Substring(0, Selection.TopCharacter) + Lines[index].Text.Substring(Math.Min(Selection.BottomCharacter, Lines[index].Text.Length)));
				}
				else if (index == Selection.TopLine)
				{
					SetLineText(index, Lines[index].Text.Substring(0, Selection.TopCharacter) + Lines[index + 1].Text);
					RemoveLine(index + 1);
				}
				else if (index == Selection.BottomLine)
				{
					SetLineText(index, Lines[index].Text.Substring(Math.Min(Selection.BottomCharacter, Lines[index].Text.Length)));
				}
				else if (index > Selection.TopLine && index < Selection.BottomLine)
				{
					RemoveLine(index);
				}
			}

			cursorLine = Selection.TopLine;
			cursorCharacter = Selection.TopCharacter;
			Selection = null;
		}

		private void InsertNewLine(int index, string newText)
		{
			Lines.Insert(index, new Line() { Text = newText, LineIndex = -1 });
			Edited = true;
		}

		private void RemoveLine(int index)
		{
			Lines.RemoveAt(index);
			if (Lines.Count == 0)
			{
				InsertNewLine(0, "");
			}
			Edited = true;
		}

		private void SetLineText(int index, string newText)
		{
			Lines[index].Text = newText;
			Lines[index].Type = TextState.FullMatch;
			if (Lines[index].LineIndex == null && newText != "")
			{
				Lines[index].LineIndex = -1;
			}
			Edited = true;
		}

		private void CopyToClipboard()
		{
			StringBuilder sb = new StringBuilder();
			int lineIndex = Selection.TopLine;
			do
			{
				if (Lines[lineIndex].Type != TextState.Filler)
				{
					if (sb.Length > 0)
					{
						sb.AppendLine("");
					}
					int startCharacter = lineIndex == Selection.TopLine ? Selection.TopCharacter : 0;
					int length = lineIndex == Selection.BottomLine ? Selection.BottomCharacter - startCharacter : Lines[lineIndex].Text.Length - startCharacter;
					if (startCharacter < Lines[lineIndex].Text.Length)
					{
						sb.Append(Lines[lineIndex].Text.Substring(startCharacter, Math.Min(length, Lines[lineIndex].Text.Length - startCharacter)));
					}
				}
				lineIndex++;
			} while (lineIndex <= Selection.BottomLine);

			Clipboard.SetText(sb.ToString());
		}

		private void SetCursorPosition(int line, int character, bool select)
		{
			if (Lines[line].Text.Length > character && char.IsLowSurrogate(Lines[line].Text[character]))
			{
				character--;
			}
			if (select)
			{
				if (Selection == null)
				{
					Selection = new Selection(cursorLine, cursorCharacter, line, character);
				}
				else
				{
					Selection.EndLine = line;
					Selection.EndCharacter = character;
				}
			}
			else
			{
				Selection = null;
			}

			cursorLine = line;
			cursorCharacter = character;

			EnsureCursorVisibility();
		}

		private void EnsureCursorVisibility()
		{
			if (cursorLine < VerticalOffset)
			{
				VerticalOffset = cursorLine;
			}
			else if (cursorLine > VerticalOffset + VisibleLines - 2)
			{
				VerticalOffset = cursorLine - (VisibleLines - 2);
			}

			double cursorPosX = CharacterPosition(cursorLine, cursorCharacter);

			if (cursorPosX < HorizontalOffset)
			{
				HorizontalOffset = (int)cursorPosX;
			}
			else if (cursorPosX > HorizontalOffset + TextAreaWidth - 10)
			{
				HorizontalOffset = (int)cursorPosX - (TextAreaWidth - 10);
			}

			ResetCursorBlink();
		}

		private void ResetCursorBlink()
		{
			cursorBlink = true;
			blinkTimer.Stop();
			blinkTimer.Start();
		}

		private double CharacterPosition(int lineIndex, int characterIndex)
		{
			double position = 0;
			int i = 0;

			foreach (TextSegment textSegment in Lines[lineIndex].TextSegments)
			{
				if (textSegment.GetRenderedText(typeface, this.FontSize, dpiScale, AppSettings.ShowWhiteSpaceCharacters, AppSettings.TabSize, out double runWidth) != null)
				{
					foreach (double x in textSegment.RenderedText.AdvanceWidths)
					{
						if (i++ == characterIndex)
						{
							return position;
						}
						position += x;
					}
				}
			}
			return position;
		}

		private void PointToCharacter(Point point, out int line, out int character)
		{
			if (Lines.Count == 0)
			{
				line = 0;
				character = 0;
				return;
			}

			line = (int)(point.Y / characterHeight) + VerticalOffset;

			if (line >= Lines.Count)
			{
				line = Lines.Count - 1;
				character = Math.Max(Lines[line].Text.Length, 0);
				return;
			}

			point.Offset((-lineNumberMargin - textMargin + HorizontalOffset), 0);

			character = 0;
			double totalWidth = 0;

			foreach (TextSegment textSegment in Lines[line].TextSegments)
			{
				if (textSegment.RenderedText != null)
				{
					foreach (double characterWidth in textSegment?.RenderedText.AdvanceWidths)
					{
						if (totalWidth + characterWidth > point.X)
						{
							return;
						}

						totalWidth += characterWidth;
						character++;
					}
				}
			}
		}

		private double RoundToWholePixels(double x)
		{
			return Math.Round(x / dpiScale) * dpiScale;
		}

		internal int Search(string text, bool matchCase)
		{
			for (int i = 0; i < Lines.Count; i++)
			{
				int hit = Lines[i].Text.IndexOf(text, matchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);
				if (hit != -1)
				{
					Selection = new Selection() { StartLine = i, EndLine = i, StartCharacter = hit, EndCharacter = hit + text.Length };
					InvalidateVisual();
					return i;
				}
			}
			Selection = null;
			InvalidateVisual();
			return -1;
		}

		internal int SearchNext(string text, bool matchCase)
		{
			int startLine = 0;

			if (Selection != null)
			{
				startLine = Selection.BottomLine;
			}

			for (int i = startLine; i <= Lines.Count + startLine; i++)
			{
				int lineIndex = i >= Lines.Count ? i - Lines.Count : i;
				int hit;

				if (Selection != null && lineIndex == startLine && i < Lines.Count + startLine)
				{
					if (Selection.BottomCharacter >= Lines[lineIndex].Text.Length)
					{
						continue;
					}
					hit = Lines[lineIndex].Text.IndexOf(text, Selection.BottomCharacter + 1, matchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);
				}
				else
				{
					hit = Lines[lineIndex].Text.IndexOf(text, matchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);
				}

				if (hit != -1)
				{
					Selection = new Selection() { StartLine = lineIndex, EndLine = lineIndex, StartCharacter = hit, EndCharacter = hit + text.Length };
					InvalidateVisual();
					return lineIndex;
				}
			}
			Selection = null;
			InvalidateVisual();
			return -1;
		}

		internal int SearchPrevious(string text, bool matchCase)
		{
			int startLine = Lines.Count - 1;

			if (Selection != null)
			{
				startLine = Selection.TopLine;
			}

			for (int i = startLine; i >= startLine - Lines.Count; i--)
			{
				int lineIndex = i < 0 ? i + Lines.Count : i;
				int hit;

				if (Selection != null && lineIndex == startLine && i > startLine - Lines.Count)
				{
					if (Selection.TopCharacter == 0)
					{
						continue;
					}
					hit = Lines[lineIndex].Text.LastIndexOf(text, Selection.TopCharacter - 1, matchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);
				}
				else
				{
					hit = Lines[lineIndex].Text.LastIndexOf(text, matchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);
				}

				if (hit != -1)
				{
					Selection = new Selection() { StartLine = lineIndex, EndLine = lineIndex, StartCharacter = hit, EndCharacter = hit + text.Length };
					InvalidateVisual();
					return lineIndex;
				}
			}
			Selection = null;
			InvalidateVisual();
			return -1;
		}

		private void MeasureRendeTime()
		{
			stopwatch.Restart();
		}

		private void ReportRenderTime()
		{
			Dispatcher.BeginInvoke(
				DispatcherPriority.Loaded,
				new Action(() =>
				{
					stopwatch.Stop();
					Debug.Print($"Took {stopwatch.ElapsedMilliseconds} ms");
				})
			);
		}

		#endregion

		#region Events

		private void BlinkTimer_Tick(object sender, EventArgs e)
		{
			if (this.EditMode && this.IsFocused)
			{
				cursorBlink = !cursorBlink;
				InvalidateVisual();
			}
		}

		#endregion

	}
}
