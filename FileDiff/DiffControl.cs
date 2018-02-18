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

		private Size characterSize;
		private double lineNumberWidth;
		private double maxTextwidth = 0;

		private Selection selection = null;
		private Point? MouseDownPoint = null;

		private SolidColorBrush slectionBrush;
		private Pen transpatentPen;

		private GlyphTypeface cachedTypeface;

		private double dpiScale = 0;

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

		}

		#endregion

		#region Overrides

		protected override void OnRender(DrawingContext drawingContext)
		{
			if (Lines.Count == 0)
				return;

			Debug.Print("OnRender");

			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			characterSize = MeasureString("W");

			if (dpiScale == 0)
			{
				Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
				dpiScale = 1 / m.M11;
			}

			drawingContext.DrawRectangle(AppSettings.fullMatchBackgroundBrush, transpatentPen, new Rect(0, 0, this.ActualWidth, this.ActualHeight));

			Typeface t = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);
			if (t.TryGetGlyphTypeface(out GlyphTypeface temp))
			{
				cachedTypeface = temp;
			}

			characterSize = MeasureString("W");

			lineNumberWidth = (Lines.Count.ToString().Length * characterSize.Width) + DpiPixels(9);
			double diffMapWidth = 0 * dpiScale;


			for (int i = 0; i < VisibleLines; i++)
			{
				if (i + VerticalOffset >= Lines.Count)
					break;

				Line line = Lines[i + VerticalOffset];

				// Draw line background
				if (line.Type != TextState.FullMatch)
				{
					drawingContext.DrawRectangle(line.BackgroundBrush, transpatentPen, new Rect(0, characterSize.Height * i, Math.Max(this.ActualWidth - diffMapWidth, 0), characterSize.Height));
				}

				// Draw line number
				if (line.LineIndex != null)
				{
					GlyphRun rowNumberText = CreateGlyphRun(line.LineIndex.ToString(), out double rowNumberWidth);
					drawingContext.PushTransform(new TranslateTransform(lineNumberWidth - rowNumberWidth - DpiPixels(6), characterSize.Height * i));
					drawingContext.DrawGlyphRun(SystemColors.ControlDarkBrush, rowNumberText);
					drawingContext.Pop();
				}

				drawingContext.PushClip(new RectangleGeometry(new Rect(lineNumberWidth, 0, Math.Max(ActualWidth - lineNumberWidth - diffMapWidth, 0), ActualHeight)));

				// Draw line
				if (line.Text != "")
				{
					double nextPosition = lineNumberWidth - HorizontalOffset;
					foreach (TextSegment textSegment in line.TextSegments)
					{
						drawingContext.PushTransform(new TranslateTransform(nextPosition, (characterSize.Height * i)));

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
				}

				// Draw selection
				if (selection != null && i + VerticalOffset >= selection.TopLine && i + VerticalOffset <= selection.BottomLine)
				{
					Rect selectionRect = new Rect(lineNumberWidth, characterSize.Height * i, this.ActualWidth, characterSize.Height);
					if (selection.TopLine == i + VerticalOffset)
					{
						selectionRect.X = Math.Max(lineNumberWidth, lineNumberWidth + line.CharacterPosition(selection.TopCharacter) - HorizontalOffset);
					}
					if (selection.BottomLine == i + VerticalOffset)
					{
						selectionRect.Width = Math.Max(0, lineNumberWidth + line.CharacterPosition(selection.BottomCharacter + 1) - selectionRect.X - HorizontalOffset);
					}
					drawingContext.DrawRectangle(slectionBrush, transpatentPen, selectionRect);
				}

				drawingContext.Pop(); // Clipping rect

			}

			drawingContext.PushTransform(new TranslateTransform(.5, .5));
			drawingContext.DrawLine(new Pen(SystemColors.ScrollBarBrush, DpiPixels(1)), new Point(lineNumberWidth - DpiPixels(4), 0), new Point(lineNumberWidth - DpiPixels(4), this.ActualHeight));
			drawingContext.Pop();

			TextAreaWidth = (int)(ActualWidth - lineNumberWidth);
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

			point.Offset((-lineNumberWidth + HorizontalOffset), 0);

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
				double width = text[n] == '\t' ? AppSettings.Settings.TabSize * characterSize.Width : Math.Ceiling(cachedTypeface.AdvanceWidths[glyphIndex] * this.FontSize / dpiScale) * dpiScale;
				advanceWidths[n] = width;

				totalWidth += width;
			}

			GlyphRun run = new GlyphRun(
				glyphTypeface: cachedTypeface,
				bidiLevel: 0,
				isSideways: false,
				renderingEmSize: this.FontSize,
				glyphIndices: glyphIndexes,
				baselineOrigin: new Point(0, cachedTypeface.Baseline * (characterSize.Height) - 2),
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

		double DpiPixels(double x)
		{
			return x * dpiScale;
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
