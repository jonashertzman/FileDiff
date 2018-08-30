using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

		private Selection selection = null;

		private int downLine;
		private int downCharacter;

		private Point? _mouseDownPosition = null;
		private Point? mouseDownPosition
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

		private readonly SolidColorBrush slectionBrush;

		private GlyphTypeface cachedTypeface;

		private double dpiScale = 0;

		private int cursorLine = 0;
		private int cursorCharacter = 0;

		#endregion

		#region Constructor

		static DiffControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(DiffControl), new FrameworkPropertyMetadata(typeof(DiffControl)));
		}

		public DiffControl()
		{
			this.ClipToBounds = true;

			Color selectionColor = SystemColors.HighlightColor;
			selectionColor.A = 40;
			slectionBrush = new SolidColorBrush(selectionColor);
		}

		#endregion

		#region Overrides

		protected override void OnRender(DrawingContext drawingContext)
		{
			Debug.Print("DiffControl OnRender");

			// Fill background
			drawingContext.DrawRectangle(AppSettings.FullMatchBackground, null, new Rect(0, 0, this.ActualWidth, this.ActualHeight));

			if (Lines.Count == 0)
				return;


			Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
			dpiScale = 1 / m.M11;

			Typeface t = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);
			if (t.TryGetGlyphTypeface(out GlyphTypeface temp))
			{
				cachedTypeface = temp;
			}

			GlyphRun g = TextUtils.CreateGlyphRun("W", this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch, this.FontSize, dpiScale, out characterWidth);

			characterHeight = Math.Ceiling(MeasureString("W").Height / dpiScale) * dpiScale;

			textMargin = RoundToWholePixels(3);
			lineNumberMargin = (characterWidth * Lines.Count.ToString().Length) + (2 * textMargin);

			VisibleLines = (int)(ActualHeight / characterHeight + 1);
			MaxVerialcalScroll = Lines.Count - VisibleLines + 1;

			for (int i = 0; i < VisibleLines; i++)
			{
				int lineIndex = i + VerticalOffset;

				if (lineIndex >= Lines.Count)
					break;

				Line line = Lines[lineIndex];

				drawingContext.PushTransform(new TranslateTransform(0, characterHeight * i));

				// Draw line background
				if (line.Type != TextState.FullMatch)
				{
					drawingContext.DrawRectangle(line.BackgroundBrush, null, new Rect(0, 0, Math.Max(this.ActualWidth, 0), characterHeight));
				}

				// Draw line number
				SolidColorBrush lineNumberColor = new SolidColorBrush();

				if (lineIndex >= CurrentDiff && lineIndex < CurrentDiff + CurrentDiffLength && !Edited)
				{
					lineNumberColor = AppSettings.FullMatchBackground;
					drawingContext.DrawRectangle(SystemColors.ControlDarkBrush, null, new Rect(0, 0, lineNumberMargin, characterHeight));
				}
				else
				{
					lineNumberColor = SystemColors.ControlDarkBrush;
				}

				if (line.LineIndex != null)
				{
					GlyphRun rowNumberRun = line.GetRenderedLineIndexText(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch, this.FontSize, dpiScale, out double rowNumberWidth);

					drawingContext.PushTransform(new TranslateTransform(lineNumberMargin - rowNumberWidth - textMargin, 0));
					drawingContext.DrawGlyphRun(lineNumberColor, rowNumberRun);
					drawingContext.Pop();
				}

				drawingContext.PushClip(new RectangleGeometry(new Rect(lineNumberMargin + textMargin, 0, Math.Max(ActualWidth - lineNumberMargin - textMargin * 2, 0), ActualHeight)));
				drawingContext.PushTransform(new TranslateTransform(lineNumberMargin + textMargin - HorizontalOffset, 0));

				// Draw line
				if (line.Text != "")
				{
					double nextPosition = 0;
					foreach (TextSegment textSegment in line.TextSegments)
					{
						drawingContext.PushTransform(new TranslateTransform(nextPosition, 0));

						GlyphRun segmentRun = textSegment.GetRenderedText(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch, this.FontSize, dpiScale, AppSettings.ShowWhiteSpaceCharacters, AppSettings.TabSize, out double runWidth);
						if (line.Type != textSegment.Type && AppSettings.ShowLineChanges)
						{
							drawingContext.DrawRectangle(textSegment.BackgroundBrush, null, new Rect(nextPosition == 0 ? -textMargin : 0, 0, runWidth + (nextPosition == 0 ? textMargin : 0), characterHeight));
						}
						drawingContext.DrawGlyphRun(AppSettings.ShowLineChanges ? textSegment.ForegroundBrush : line.ForegroundBrush, segmentRun);
						nextPosition += runWidth;

						drawingContext.Pop();
					}
					maxTextwidth = Math.Max(maxTextwidth, nextPosition);
				}

				// Draw selection
				if (selection != null && lineIndex >= selection.TopLine && lineIndex <= selection.BottomLine)
				{
					Rect selectionRect = new Rect(0, 0, this.ActualWidth + HorizontalOffset, characterHeight);
					if (selection.TopLine == lineIndex)
					{
						selectionRect.X = Math.Max(0, CharacterPosition(lineIndex, selection.TopCharacter));
					}
					if (selection.BottomLine == lineIndex)
					{
						selectionRect.Width = Math.Max(0, CharacterPosition(lineIndex, selection.BottomCharacter + 1) - selectionRect.X);
					}
					drawingContext.DrawRectangle(slectionBrush, null, selectionRect);
				}

				// Draw cursor
				if (EditMode && this.IsFocused && cursorLine == lineIndex)
				{
					drawingContext.DrawRectangle(Brushes.Black, null, new Rect(CharacterPosition(lineIndex, cursorCharacter), 0, RoundToWholePixels(1), characterHeight));
				}

				drawingContext.Pop(); // Line X offset
				drawingContext.Pop(); // Clipping rect
				drawingContext.Pop(); // Line Y offset
			}

			// Draw line number border
			drawingContext.PushTransform(new TranslateTransform(.5, -.5));
			drawingContext.DrawLine(new Pen(SystemColors.ScrollBarBrush, RoundToWholePixels(1)), new Point(lineNumberMargin, 0), new Point(lineNumberMargin, this.ActualHeight + 1));
			drawingContext.Pop();

			TextAreaWidth = (int)(ActualWidth - lineNumberMargin - (textMargin * 2));
			MaxHorizontalScroll = (int)(maxTextwidth - TextAreaWidth + textMargin);
		}

		protected override void OnTextInput(TextCompositionEventArgs e)
		{
			if (EditMode && e.Text.Length > 0 && Lines.Count > 0)
			{
				if (e.Text == "\b")
				{
					if (selection != null)
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
							SetLineText(cursorLine, Lines[cursorLine].Text.Substring(0, cursorCharacter - 1) + Lines[cursorLine].Text.Substring(cursorCharacter));
							cursorCharacter--;
						}
					}
				}
				else if (e.Text == "\r")
				{
					if (selection != null)
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
					if (selection != null)
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

			if (e.Key == Key.Delete)
			{
				if (selection != null)
				{
					DeleteSelection();
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
						SetLineText(cursorLine, Lines[cursorLine].Text.Substring(0, cursorCharacter) + Lines[cursorLine].Text.Substring(cursorCharacter + 1));
					}
				}
				EnsureCursorVisibility();
			}
			else if (e.Key == Key.Tab && EditMode)
			{
				if (selection != null)
				{
					DeleteSelection();
				}
				SetLineText(cursorLine, Lines[cursorLine].Text.Substring(0, cursorCharacter) + "\t" + Lines[cursorLine].Text.Substring(cursorCharacter));
				cursorCharacter++;
				EnsureCursorVisibility();
			}
			else if (e.Key == Key.A && Keyboard.Modifiers == ModifierKeys.Control)
			{
				if (Lines.Count > 0)
				{
					selection = new Selection(0, 0, Lines.Count - 1, Math.Max(0, Lines[Lines.Count - 1].Text.Length - 1));
				}
			}
			else if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
			{
				if (selection != null)
				{
					CopyToClipboard();
				}
			}
			else if (e.Key == Key.X && Keyboard.Modifiers == ModifierKeys.Control && EditMode)
			{
				if (selection != null)
				{
					CopyToClipboard();
					DeleteSelection();
					EnsureCursorVisibility();
				}
			}
			else if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control && EditMode)
			{
				if (Clipboard.ContainsText())
				{
					if (selection != null)
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
			else if (e.Key == Key.PageUp)
			{
				if (EditMode)
				{
					cursorLine = Math.Max(cursorLine - VisibleLines, 0);
					cursorCharacter = Math.Min(cursorCharacter, Lines[cursorLine].Text.Length);
					selection = null;
					EnsureCursorVisibility();
				}
				else
				{
					VerticalOffset = Math.Max(0, VerticalOffset -= VisibleLines - 1);
				}
			}
			else if (e.Key == Key.PageDown)
			{
				if (EditMode)
				{
					cursorLine = Math.Min(cursorLine + VisibleLines, Lines.Count - 1);
					cursorCharacter = Math.Min(cursorCharacter, Lines[cursorLine].Text.Length);
					selection = null;
					EnsureCursorVisibility();
				}
				else
				{
					VerticalOffset = VerticalOffset += VisibleLines - 1;
				}
			}
			else if (e.Key == Key.Home  )
			{
				if (Keyboard.Modifiers == ModifierKeys.Control)
				{
					VerticalOffset = 0;
					if (EditMode)
					{
						cursorLine = 0;
						cursorCharacter = 0;
						EnsureCursorVisibility();
					}
				}
				else
				{
					HorizontalOffset = 0;
					if (EditMode)
					{
						cursorCharacter = 0;
						EnsureCursorVisibility();
					}
				}
			}
			else if (e.Key == Key.End  )
			{
				if (Keyboard.Modifiers == ModifierKeys.Control)
				{
					VerticalOffset = Lines.Count;
					if (EditMode)
					{
						cursorLine = Lines.Count - 1;
						cursorCharacter = Lines[cursorLine].Text.Length;
						EnsureCursorVisibility();
					}
				}
				else
				{
					if (EditMode)
					{
						cursorCharacter = Lines[cursorLine].Text.Length;
						EnsureCursorVisibility();
					}
				}
			}
			else if (e.Key == Key.Down && EditMode)
			{
				if (cursorLine < Lines.Count - 1)
				{
					cursorLine++;
					cursorCharacter = Math.Min(cursorCharacter, Lines[cursorLine].Text.Length);
					selection = null;
					EnsureCursorVisibility();
				}
			}
			else if (e.Key == Key.Up && EditMode)
			{
				if (cursorLine > 0)
				{
					cursorLine--;
					cursorCharacter = Math.Min(cursorCharacter, Lines[cursorLine].Text.Length);
					selection = null;
					EnsureCursorVisibility();
				}
			}
			else if (e.Key == Key.Left && EditMode)
			{
				if (cursorCharacter == 0)
				{
					if (cursorLine > 0)
					{
						cursorLine--;
						cursorCharacter = Lines[cursorLine].Text.Length;
					}
				}
				else
				{
					cursorCharacter--;
				}
				selection = null;
				EnsureCursorVisibility();
			}
			else if (e.Key == Key.Right && EditMode)
			{
				if (cursorCharacter >= Lines[cursorLine].Text.Length)
				{
					if (cursorLine < Lines.Count - 1)
					{
						cursorLine++;
						cursorCharacter = 0;
					}
				}
				else
				{
					cursorCharacter++;
				}
				selection = null;
				EnsureCursorVisibility();
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
				mouseDownPosition = e.GetPosition(this);
			}

			base.OnMouseDown(e);
		}

		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			Point currentMousePosition = e.GetPosition(this);

			PointToCharacter(currentMousePosition, out cursorLine, out cursorCharacter);

			if (e.ChangedButton == MouseButton.Left && mouseDownPosition != null && Lines.Count > 0)
			{

				if (currentMousePosition != mouseDownPosition || currentMousePosition.X < lineNumberMargin)
				{
					PointToCharacter(currentMousePosition, out int upLine, out int upCharacter);

					if (mouseDownPosition.Value.X < lineNumberMargin || currentMousePosition.X < lineNumberMargin)
					{
						downCharacter = upLine < downLine ? Lines[downLine].Text.Length : 0;
						upCharacter = upLine < downLine ? 0 : Lines[upLine].Text.Length;
					}

					selection = new Selection(downLine, downCharacter, upLine, upCharacter);

					if (selection.StartLine < cursorLine || (selection.StartLine == cursorLine && selection.StartCharacter < cursorCharacter))
					{
						cursorCharacter = upCharacter + 1;
					}
				}
				else
				{
					selection = null;
				}

				mouseDownPosition = null;
				InvalidateVisual();
			}

			base.OnMouseUp(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (Mouse.LeftButton == MouseButtonState.Pressed && mouseDownPosition != null && Lines.Count > 0)
			{
				Point currentMousePosition = e.GetPosition(this);

				PointToCharacter(currentMousePosition, out cursorLine, out cursorCharacter);
				PointToCharacter(currentMousePosition, out int upLine, out int upCharacter);

				int selectionStartCharacter = downCharacter;

				if (mouseDownPosition.Value.X < lineNumberMargin || currentMousePosition.X < lineNumberMargin)
				{
					selectionStartCharacter = upLine < downLine ? Lines[downLine].Text.Length : 0;
					upCharacter = upLine < downLine ? 0 : Lines[upLine].Text.Length;
				}

				selection = new Selection(downLine, selectionStartCharacter, upLine, upCharacter);

				if (selection.StartLine < cursorLine || (selection.StartLine == cursorLine && selection.StartCharacter < cursorCharacter))
				{
					cursorCharacter = upCharacter + 1;
				}

				InvalidateVisual();
			}

			base.OnMouseMove(e);
		}

		protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			InvalidateVisual();

			base.OnLostKeyboardFocus(e);
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
			selection = null;
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

		private void DeleteSelection()
		{
			for (int index = selection.BottomLine; index >= selection.TopLine; index--)
			{
				if (selection.TopLine == selection.BottomLine)
				{
					SetLineText(index, Lines[index].Text.Substring(0, selection.TopCharacter) + Lines[index].Text.Substring(Math.Min(selection.BottomCharacter + 1, Lines[index].Text.Length)));
				}
				else if (index == selection.TopLine)
				{
					SetLineText(index, Lines[index].Text.Substring(0, selection.TopCharacter) + Lines[index + 1].Text);
					RemoveLine(index + 1);
				}
				else if (index == selection.BottomLine)
				{
					SetLineText(index, Lines[index].Text.Substring(Math.Min(selection.BottomCharacter + 1, Lines[index].Text.Length)));
				}
				else if (index > selection.TopLine && index < selection.BottomLine)
				{
					RemoveLine(index);
				}
			}

			cursorLine = selection.TopLine;
			cursorCharacter = selection.TopCharacter;
			selection = null;
		}

		private void InsertNewLine(int index, string newText)
		{
			Lines.Insert(index, new Line() { Text = newText, LineIndex = -1 });
			Edited = true;
		}

		private void RemoveLine(int index)
		{
			Lines.RemoveAt(index);
			Edited = true;
		}

		private void SetLineText(int index, string newText)
		{
			Lines[index].Text = newText;
			Lines[index].Type = TextState.FullMatch;
			if (Lines[index].LineIndex == null && newText !="")
			{
				Lines[index].LineIndex = -1;
			}
			Edited = true;
		}

		private void CopyToClipboard()
		{
			StringBuilder sb = new StringBuilder();
			int lineIndex = selection.TopLine;
			do
			{
				if (Lines[lineIndex].Type != TextState.Filler)
				{
					if (sb.Length > 0)
					{
						sb.AppendLine("");
					}
					int startCharacter = lineIndex == selection.TopLine ? selection.TopCharacter : 0;
					int length = lineIndex == selection.BottomLine ? selection.BottomCharacter - startCharacter + 1 : Lines[lineIndex].Text.Length - startCharacter;
					if (startCharacter < Lines[lineIndex].Text.Length)
					{
						sb.Append(Lines[lineIndex].Text.Substring(startCharacter, Math.Min(length, Lines[lineIndex].Text.Length - startCharacter)));
					}
				}
				lineIndex++;
			} while (lineIndex <= selection.BottomLine);

			Clipboard.SetText(sb.ToString());
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
		}

		private double CharacterPosition(int lineIndex, int characterIndex)
		{
			double position = 0;
			int i = 0;

			foreach (TextSegment textSegment in Lines[lineIndex].TextSegments)
			{
				if (textSegment.GetRenderedText(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch, this.FontSize, dpiScale, AppSettings.ShowWhiteSpaceCharacters, AppSettings.TabSize, out double runWidth) != null)
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
				character = Math.Max(Lines[line].Text.Length - 1, 0);
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

		private Size MeasureString(string text)
		{
			FormattedText formattedText = new FormattedText(
				text,
				CultureInfo.CurrentCulture,
				FlowDirection.LeftToRight,
				new Typeface(this.FontFamily.ToString()),
				this.FontSize,
				Brushes.Black,
				new NumberSubstitution(),
				TextFormattingMode.Display);

			return new Size(formattedText.Width, formattedText.Height);
		}

		internal int Search(string text, bool matchCase)
		{
			int searchStart = 0;

			if (selection != null)
			{
				searchStart = selection.EndLine;
			}

			for (int i = 0; i < Lines.Count; i++)
			{
				int hit = Lines[i].Text.IndexOf(text, matchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);
				if (hit != -1)
				{
					selection = new Selection() { StartLine = i, EndLine = i, StartCharacter = hit, EndCharacter = hit + text.Length - 1 };
					InvalidateVisual();
					return i;
				}
			}
			selection = null;
			InvalidateVisual();
			return -1;
		}

		internal int SearchNext(string text, bool matchCase)
		{
			int startLine = 0;

			if (selection != null)
			{
				startLine = selection.BottomLine;
			}

			for (int i = startLine; i <= Lines.Count + startLine; i++)
			{
				int lineIndex = i >= Lines.Count ? i - Lines.Count : i;
				int hit;

				if (selection != null && lineIndex == startLine && i < Lines.Count + startLine)
				{
					if (selection.BottomCharacter >= Lines[lineIndex].Text.Length)
					{
						continue;
					}
					hit = Lines[lineIndex].Text.IndexOf(text, selection.BottomCharacter + 1, matchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);
				}
				else
				{
					hit = Lines[lineIndex].Text.IndexOf(text, matchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);
				}

				if (hit != -1)
				{
					selection = new Selection() { StartLine = lineIndex, EndLine = lineIndex, StartCharacter = hit, EndCharacter = hit + text.Length - 1 };
					InvalidateVisual();
					return lineIndex;
				}
			}
			selection = null;
			InvalidateVisual();
			return -1;
		}

		internal int SearchPrevious(string text, bool matchCase)
		{
			int startLine = Lines.Count - 1;

			if (selection != null)
			{
				startLine = selection.TopLine;
			}

			for (int i = startLine; i >= startLine - Lines.Count; i--)
			{
				int lineIndex = i < 0 ? i + Lines.Count : i;
				int hit;

				if (selection != null && lineIndex == startLine && i > startLine - Lines.Count)
				{
					if (selection.TopCharacter == 0)
					{
						continue;
					}
					hit = Lines[lineIndex].Text.LastIndexOf(text, selection.TopCharacter - 1, matchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);
				}
				else
				{
					hit = Lines[lineIndex].Text.LastIndexOf(text, matchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);
				}

				if (hit != -1)
				{
					selection = new Selection() { StartLine = lineIndex, EndLine = lineIndex, StartCharacter = hit, EndCharacter = hit + text.Length - 1 };
					InvalidateVisual();
					return lineIndex;
				}
			}
			selection = null;
			InvalidateVisual();
			return -1;
		}

		#endregion

	}
}
