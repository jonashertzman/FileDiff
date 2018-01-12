using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
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

		private Selection selection = new Selection();
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
					if (line.Type != TextState.FullMatch)
					{
						drawingContext.DrawRectangle(line.BackgroundBrush, transpatentPen, new Rect(0, characterSize.Height * i, this.ActualWidth, characterSize.Height));
					}

					if (line.LineIndex != null)
					{
						GlyphRun rowNumberText = CreateGlyphRun(line.LineIndex.ToString(), out double rowNumberWidth);
						drawingContext.PushTransform(new TranslateTransform(lineNumberMargin - rowNumberWidth - 4, characterSize.Height * i));
						drawingContext.DrawGlyphRun(SystemColors.ControlDarkBrush, rowNumberText);
						drawingContext.Pop();
					}

					if (line.Text != "")
					{
						double nextPosition = lineNumberMargin - HorizontalOffset;
						drawingContext.PushClip(clippingRect);
						foreach (TextSegment textSegment in line.TextSegments)
						{
							drawingContext.PushTransform(new TranslateTransform(nextPosition, characterSize.Height * i));

							GlyphRun run = CreateGlyphRun(textSegment.Text, out double runWidth);
							if (line.Type != textSegment.Type)
							{
								drawingContext.DrawRectangle(textSegment.BackgroundBrush, transpatentPen, new Rect(0, 0, runWidth, characterSize.Height));
							}
							drawingContext.DrawGlyphRun(textSegment.ForegroundBrush, run);
							nextPosition += runWidth;

							drawingContext.Pop();
						}
						maxTextwidth = Math.Max(maxTextwidth, nextPosition);

						drawingContext.Pop();
					}

				}
			}

			TextAreaWidth = (int)ActualWidth - lineNumberMargin;
			MaxScroll = (int)(maxTextwidth - TextAreaWidth);


			//for (int i = selection.TopLine - VerticalOffset; i <= selection.BottomLine - VerticalOffset; i++)
			//{
			//	drawingContext.DrawRectangle(slectionBrush, transpatentPen, new Rect(lineNumberMargin, characterSize.Height * i, this.ActualWidth, characterSize.Height));
			//}

			drawingContext.DrawLine(new Pen(SystemColors.ScrollBarBrush, 1), new Point(lineNumberMargin - 1.5, 0), new Point(lineNumberMargin - 1.5, this.ActualHeight));

			stopwatch.Stop();

			Debug.Print(stopwatch.ElapsedMilliseconds.ToString());
		}

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			Point pos = OffsetMargin(e);
			selection.StartLine = (int)(pos.Y / characterSize.Height) + VerticalOffset;
			selection.StartCharacter = (int)(pos.X / characterSize.Width) + HorizontalOffset;

			base.OnMouseDown(e);
		}

		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			Point pos = OffsetMargin(e);

			selection.EndLine = (int)(pos.Y / characterSize.Height) + VerticalOffset;
			selection.EndCharacter = (int)(pos.X / characterSize.Width) + HorizontalOffset;

			base.OnMouseUp(e);

			InvalidateVisual();
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (System.Windows.Input.Mouse.LeftButton == MouseButtonState.Pressed)
			{
				Point pos = OffsetMargin(e);

				selection.EndLine = (int)(pos.Y / characterSize.Height) + VerticalOffset;
				selection.EndCharacter = (int)(pos.Y / characterSize.Width) + HorizontalOffset;
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

		public GlyphRun CreateGlyphRun(string text, out double runWidth)
		{
			ushort[] glyphIndexes = new ushort[text.Length];
			double[] advanceWidths = new double[text.Length];

			double totalWidth = 0;
			for (int n = 0; n < text.Length; n++)
			{
				ushort glyphIndex;
				cachedTypeface.CharacterToGlyphMap.TryGetValue(text[n] == '\t' ? ' ' : text[n], out glyphIndex);
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

		private List<string> Split(string text)
		{
			List<string> items = new List<string>();
			int lastHit = -1;

			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] == '\t')
				{
					if (i > lastHit + 1)
					{
						items.Add(text.Substring(lastHit + 1, i - lastHit - 1));
					}
					items.Add(text.Substring(i, 1));
					lastHit = i;
				}
			}

			if (lastHit < text.Length - 1)
			{
				items.Add(text.Substring(lastHit + 1, text.Length - lastHit - 1));
			}

			return items;
		}

		private Point OffsetMargin(MouseEventArgs e)
		{
			Point pos = e.GetPosition(this);
			pos.Offset(-lineNumberMargin, 0);
			return pos;
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

		#endregion

	}
}
