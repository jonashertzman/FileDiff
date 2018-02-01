using System;
using System.Collections.Generic;
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

		#region Members,

		private Size characterSize;
		private int lineNumberMargin;
		double maxTextwidth = 0;

		private Selection selection = null;
		Point? MouseDownPoint = null;

		private SolidColorBrush slectionBrush;
		private Pen transpatentPen;

		GlyphTypeface cachedTypeface;

		#endregion

		#region Constructor

		static DiffControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(DiffControl), new FrameworkPropertyMetadata(typeof(DiffControl)));
		}

		public DiffControl()
		{
			Lines = new ObservableCollection<Line>();
			this.ClipToBounds = true;

			Color selectionColor = SystemColors.HighlightColor;
			selectionColor.A = 40;
			slectionBrush = new SolidColorBrush(selectionColor);

			transpatentPen = new Pen(Brushes.Transparent, 0);
			characterSize = MeasureString("W");
		}

		#endregion

		#region Overrides

		protected override void OnRender(DrawingContext drawingContext)
		{
			Debug.Print("OnRender");
			drawingContext.DrawRectangle(AppSettings.fullMatchBackgroundBrush, transpatentPen, new Rect(0, 0, this.ActualWidth, this.ActualHeight));

			if (Lines.Count == 0)
				return;

			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			Typeface t = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);
			if (t.TryGetGlyphTypeface(out GlyphTypeface temp))
			{
				cachedTypeface = temp;
			}

			characterSize = MeasureString("W");
			lineNumberMargin = (Lines.Count.ToString().Length * (int)characterSize.Width) + 6;
			RectangleGeometry clippingRect = new RectangleGeometry(new Rect(lineNumberMargin, 0, ActualWidth, ActualHeight));

			for (int i = 0; i < VisibleLines; i++)
			{
				if (i + VerticalOffset >= Lines.Count)
					break;

				Line line = Lines[i + VerticalOffset];

				if (line.LineIndex != -1)
				{
					// Draw line background
					if (line.Type != TextState.FullMatch)
					{
						drawingContext.DrawRectangle(line.BackgroundBrush, transpatentPen, new Rect(0, characterSize.Height * i, this.ActualWidth, characterSize.Height));
					}

					// Draw line number
					if (line.LineIndex != null)
					{
						GlyphRun rowNumberText = CreateGlyphRun(line.LineIndex.ToString(), out double rowNumberWidth);
						drawingContext.PushTransform(new TranslateTransform(lineNumberMargin - rowNumberWidth - 4, characterSize.Height * i));
						drawingContext.DrawGlyphRun(SystemColors.ControlDarkBrush, rowNumberText);
						drawingContext.Pop();
					}

					// Draw line
					if (line.Text != "")
					{
						double nextPosition = lineNumberMargin - HorizontalOffset;
						drawingContext.PushClip(clippingRect);
						foreach (TextSegment textSegment in line.TextSegments)
						{
							drawingContext.PushTransform(new TranslateTransform(nextPosition, characterSize.Height * i));

							textSegment.RenderedText = CreateGlyphRun(textSegment.Text, out double runWidth);
							if (line.Type != textSegment.Type)
							{
								drawingContext.DrawRectangle(textSegment.BackgroundBrush, transpatentPen, new Rect(0, 0, runWidth, characterSize.Height));
							}
							drawingContext.DrawGlyphRun(textSegment.ForegroundBrush, textSegment.RenderedText);
							nextPosition += runWidth;

							drawingContext.Pop();
						}
						maxTextwidth = Math.Max(maxTextwidth, nextPosition);

						drawingContext.Pop();
					}
				}

				// Draw selection
				if (selection != null && i + VerticalOffset >= selection.TopLine && i + VerticalOffset <= selection.BottomLine)
				{
					Rect selectionRect = new Rect(lineNumberMargin, characterSize.Height * i, this.ActualWidth, characterSize.Height);
					if (selection.TopLine == i + VerticalOffset)
					{
						selectionRect.X = Math.Max(lineNumberMargin, lineNumberMargin + line.CharacterPosition(selection.TopCharacter) - HorizontalOffset);
					}
					if (selection.BottomLine == i + VerticalOffset)
					{
						selectionRect.Width = Math.Max(0, lineNumberMargin + line.CharacterPosition(selection.BottomCharacter + 1) - selectionRect.X - HorizontalOffset);
					}
					drawingContext.DrawRectangle(slectionBrush, transpatentPen, selectionRect);
				}
			}

			drawingContext.DrawLine(new Pen(SystemColors.ScrollBarBrush, 1), new Point(lineNumberMargin - 1.5, 0), new Point(lineNumberMargin - 1.5, this.ActualHeight));

			TextAreaWidth = (int)ActualWidth - lineNumberMargin;
			MaxScroll = (int)(maxTextwidth - TextAreaWidth);

			stopwatch.Stop();
			Debug.Print(stopwatch.ElapsedMilliseconds.ToString());
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.Key == Key.A && Keyboard.Modifiers == ModifierKeys.Control)
			{
				if (Lines.Count > 0)
				{
					selection = new Selection();
					selection.StartLine = 0;
					selection.StartCharacter = 0;
					selection.EndLine = Lines.Count - 1;
					selection.EndCharacter = Math.Max(0, Lines[Lines.Count - 1].Text.Length - 1);

					InvalidateVisual();
				}
			}
			else if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
			{
				if (selection != null)
				{
					StringBuilder sb = new StringBuilder();
					int lineIndex = selection.TopLine;
					do
					{
						if (Lines[lineIndex].Type != TextState.Filler)
						{
							if (lineIndex != selection.TopLine)
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
			}
			else if (e.Key == Key.PageUp)
			{
				VerticalOffset = Math.Max(0, VerticalOffset -= VisibleLines - 1);
			}
			else if (e.Key == Key.PageDown)
			{
				VerticalOffset = VerticalOffset += VisibleLines - 1;
			}
			else if (e.Key == Key.Home && Keyboard.Modifiers == ModifierKeys.Control)
			{
				VerticalOffset = 0;
			}
			else if (e.Key == Key.End && Keyboard.Modifiers == ModifierKeys.Control)
			{
				VerticalOffset = Lines.Count;
			}

			base.OnKeyDown(e);
		}

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			this.Focus();

			if (e.ChangedButton == MouseButton.Left)
			{
				selection = new Selection();
				MouseDownPoint = e.GetPosition(this);
				PointToCharacter(e.GetPosition(this), out selection.StartLine, out selection.StartCharacter);
			}

			base.OnMouseDown(e);
		}

		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left && MouseDownPoint != null)
			{
				if (e.GetPosition(this) != MouseDownPoint)
				{
					PointToCharacter(e.GetPosition(this), out selection.EndLine, out selection.EndCharacter);
				}
				else
				{
					selection = null;
				}

				MouseDownPoint = null;
				InvalidateVisual();
			}
			base.OnMouseUp(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (Mouse.LeftButton == MouseButtonState.Pressed && MouseDownPoint != null)
			{
				PointToCharacter(e.GetPosition(this), out selection.EndLine, out selection.EndCharacter);
				InvalidateVisual();
			}

			base.OnMouseMove(e);

		}

		#endregion

		#region Properties

		private int VisibleLines
		{
			get
			{
				if (characterSize.Height == 0)
					return 0;

				return (int)(this.ActualHeight / characterSize.Height + 1);
			}
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


		public static readonly DependencyProperty MaxScrollPropery = DependencyProperty.Register("MaxScroll", typeof(int), typeof(DiffControl));

		public int MaxScroll
		{
			get { return (int)GetValue(MaxScrollPropery); }
			set { SetValue(MaxScrollPropery, value); }
		}


		public static readonly DependencyProperty TextAreaWidthPropery = DependencyProperty.Register("TextAreaWidth", typeof(int), typeof(DiffControl));

		public int TextAreaWidth
		{
			get { return (int)GetValue(TextAreaWidthPropery); }
			set { SetValue(TextAreaWidthPropery, value); }
		}

		#endregion

		#region Methods

		private void PointToCharacter(Point point, out int line, out int character)
		{
			if (Lines.Count == 0)
			{
				line = 0;
				character = 0;
				return;
			}

			line = (int)(point.Y / characterSize.Height) + VerticalOffset;

			if (line >= Lines.Count)
			{
				line = Lines.Count - 1;
				character = Lines[line].Text.Length - 1;
				return;
			}

			point.Offset((-lineNumberMargin + HorizontalOffset), 0);

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

		public GlyphRun CreateGlyphRun(string text, out double runWidth)
		{
			ushort[] glyphIndexes = new ushort[text.Length];
			double[] advanceWidths = new double[text.Length];

			double totalWidth = 0;
			for (int n = 0; n < text.Length; n++)
			{
				cachedTypeface.CharacterToGlyphMap.TryGetValue(text[n] == '\t' ? ' ' : text[n], out ushort glyphIndex); // Why does the tab glyph render as a rectangle?
				glyphIndexes[n] = glyphIndex;
				double width = text[n] == '\t' ? AppSettings.Settings.TabSize * characterSize.Width : Math.Ceiling(cachedTypeface.AdvanceWidths[glyphIndex] * this.FontSize);
				advanceWidths[n] = width;

				totalWidth += width;
			}

			GlyphRun run = new GlyphRun(
				glyphTypeface: cachedTypeface,
				bidiLevel: 0,
				isSideways: false,
				renderingEmSize: this.FontSize,
				glyphIndices: glyphIndexes,
				baselineOrigin: new Point(0, Math.Round(cachedTypeface.Baseline * this.FontSize)),
				advanceWidths: advanceWidths,
				glyphOffsets: null,
				characters: null,
				deviceFontName: null,
				clusterMap: null,
				caretStops: null,
				language: null);

			runWidth = totalWidth;
			return run;
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

				if (selection != null && lineIndex == startLine && selection.TopCharacter > 0 && i > startLine - Lines.Count)
				{
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
