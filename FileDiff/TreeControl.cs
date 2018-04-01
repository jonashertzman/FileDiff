using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileDiff
{

	public class TreeControl : Control
	{
		private const double handleWidth = 4;

		private double characterHeight;
		private double characterWidth;

		private SolidColorBrush slectionBrush;
		private Pen transpatentPen;

		private GlyphTypeface cachedTypeface;

		private double dpiScale = 0;

		List<FileItem> visibleItems = new List<FileItem>();

		#region Constructor

		static TreeControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeControl), new FrameworkPropertyMetadata(typeof(TreeControl)));
		}

		public TreeControl()
		{
			this.ClipToBounds = true;

			Color selectionColor = SystemColors.HighlightColor;
			selectionColor.A = 40;
			slectionBrush = new SolidColorBrush(selectionColor);

			transpatentPen = new Pen(Brushes.Transparent, 0);
		}

		#endregion

		protected override void OnRender(DrawingContext drawingContext)
		{
			Debug.Print("TreeControl OnRender");

			// Fill background
			drawingContext.DrawRectangle(AppSettings.FullMatchBackground, transpatentPen, new Rect(0, 0, this.ActualWidth, this.ActualHeight));

			if (Lines.Count == 0)
				return;


			Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
			dpiScale = 1 / m.M11;

			Typeface t = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);
			if (t.TryGetGlyphTypeface(out GlyphTypeface temp))
			{
				cachedTypeface = temp;
			}

			CreateGlyphRun("W", out characterWidth);

			characterHeight = Math.Ceiling(MeasureString("W").Height / dpiScale) * dpiScale;

			VisibleLines = (int)(ActualHeight / characterHeight + 1);
			MaxVerialcalScroll = Lines.Count - VisibleLines + 1;

			visibleItems = new List<FileItem>();
			GetVisibleItems(Lines, visibleItems);

			for (int i = 0; i < visibleItems.Count; i++)
			{
				int lineIndex = i + VerticalOffset;

				if (lineIndex >= visibleItems.Count)
					break;

				FileItem line = visibleItems[lineIndex];

				if (line.Type == TextState.Filler)
					continue;

				drawingContext.PushTransform(new TranslateTransform(0, characterHeight * i));

				// Draw line background
				if (line.Type != TextState.FullMatch)
				{
					drawingContext.DrawRectangle(line.BackgroundBrush, transpatentPen, new Rect(0, 0, Math.Max(this.ActualWidth, 0), characterHeight));
				}

				//GlyphRun g = CreateGlyphRun(line.Name, out double runWidth);
				//drawingContext.DrawGlyphRun(line.ForegroundBrush, g);


				drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, AppSettings.NameColumnWidth, characterHeight)));
				drawingContext.DrawText(new FormattedText(line.Name, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, t, this.FontSize, line.ForegroundBrush, null, TextFormattingMode.Display), new Point((line.Level) * characterHeight, 0));
				drawingContext.Pop();

				drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, AppSettings.NameColumnWidth + handleWidth + AppSettings.SizeColumnWidth, characterHeight)));
				drawingContext.DrawText(new FormattedText(line.Size, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, t, this.FontSize, line.ForegroundBrush, null, TextFormattingMode.Display), new Point(AppSettings.NameColumnWidth + handleWidth, 0));
				drawingContext.Pop();

				drawingContext.DrawText(new FormattedText(line.Date, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, t, this.FontSize, line.ForegroundBrush, null, TextFormattingMode.Display), new Point(AppSettings.NameColumnWidth + AppSettings.SizeColumnWidth + (handleWidth * 2), 0));

				drawingContext.Pop(); // Line Y offset 
			}
		}

		private void GetVisibleItems(ObservableCollection<FileItem> parent, List<FileItem> visibleItems)
		{
			foreach (FileItem fileItem in parent)
			{
				visibleItems.Add(fileItem);
				if (fileItem.IsFolder && fileItem.IsExpanded)
				{
					GetVisibleItems(fileItem.Children, visibleItems);
				}
			}
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

		private GlyphRun CreateGlyphRun(string text, out double runWidth)
		{
			ushort[] glyphIndexes = new ushort[text.Length];
			double[] advanceWidths = new double[text.Length];

			double totalWidth = 0;
			for (int n = 0; n < text.Length; n++)
			{
				cachedTypeface.CharacterToGlyphMap.TryGetValue(text[n] == '\t' ? ' ' : text[n], out ushort glyphIndex); // Why does the tab glyph render as a rectangle?
				glyphIndexes[n] = glyphIndex;
				double width = text[n] == '\t' ? AppSettings.TabSize * characterWidth : Math.Ceiling(cachedTypeface.AdvanceWidths[glyphIndex] * this.FontSize / dpiScale) * dpiScale;
				advanceWidths[n] = width;

				totalWidth += width;
			}

			GlyphRun run = new GlyphRun(
				glyphTypeface: cachedTypeface,
				bidiLevel: 0,
				isSideways: false,
				renderingEmSize: Math.Ceiling((FontSize) / dpiScale) * dpiScale,
				glyphIndices: glyphIndexes,
				baselineOrigin: new Point(0, Math.Ceiling((FontSize * cachedTypeface.Baseline) / dpiScale) * dpiScale),
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

		#region Dependency Properties

		public static readonly DependencyProperty LinesProperty = DependencyProperty.Register("Lines", typeof(ObservableCollection<FileItem>), typeof(TreeControl), new FrameworkPropertyMetadata(new ObservableCollection<FileItem>(), FrameworkPropertyMetadataOptions.AffectsRender));

		public ObservableCollection<FileItem> Lines
		{
			get { return (ObservableCollection<FileItem>)GetValue(LinesProperty); }
			set { SetValue(LinesProperty, value); }
		}


		public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register("VerticalOffset", typeof(int), typeof(TreeControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

		public int VerticalOffset
		{
			get { return (int)GetValue(VerticalOffsetProperty); }
			set { SetValue(VerticalOffsetProperty, value); }
		}


		public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register("HorizontalOffset", typeof(int), typeof(TreeControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

		public int HorizontalOffset
		{
			get { return (int)GetValue(HorizontalOffsetProperty); }
			set { SetValue(HorizontalOffsetProperty, value); }
		}


		public static readonly DependencyProperty MaxHorizontalScrollPropery = DependencyProperty.Register("MaxHorizontalScroll", typeof(int), typeof(TreeControl));

		public int MaxHorizontalScroll
		{
			get { return (int)GetValue(MaxHorizontalScrollPropery); }
			set { SetValue(MaxHorizontalScrollPropery, value); }
		}


		public static readonly DependencyProperty TextAreaWidthPropery = DependencyProperty.Register("TextAreaWidth", typeof(int), typeof(TreeControl));

		public int TextAreaWidth
		{
			get { return (int)GetValue(TextAreaWidthPropery); }
			set { SetValue(TextAreaWidthPropery, value); }
		}


		public static readonly DependencyProperty CurrentDiffProperty = DependencyProperty.Register("CurrentDiff", typeof(int), typeof(TreeControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

		public int CurrentDiff
		{
			get { return (int)GetValue(CurrentDiffProperty); }
			set { SetValue(CurrentDiffProperty, value); }
		}

		public static readonly DependencyProperty CurrentDiffLengthProperty = DependencyProperty.Register("CurrentDiffLength", typeof(int), typeof(TreeControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

		public int CurrentDiffLength
		{
			get { return (int)GetValue(CurrentDiffLengthProperty); }
			set { SetValue(CurrentDiffLengthProperty, value); }
		}


		public static readonly DependencyProperty VisibleLinesProperty = DependencyProperty.Register("VisibleLines", typeof(int), typeof(TreeControl));

		public int VisibleLines
		{
			get { return (int)GetValue(VisibleLinesProperty); }
			set { SetValue(VisibleLinesProperty, value); }
		}


		public static readonly DependencyProperty MaxVerialcalScrollProperty = DependencyProperty.Register("MaxVerialcalScroll", typeof(int), typeof(TreeControl));

		public int MaxVerialcalScroll
		{
			get { return (int)GetValue(MaxVerialcalScrollProperty); }
			set { SetValue(MaxVerialcalScrollProperty, value); }
		}


		public static readonly DependencyProperty UpdateTriggerProperty = DependencyProperty.Register("UpdateTrigger", typeof(int), typeof(TreeControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

		public int UpdateTrigger
		{
			get { return (int)GetValue(UpdateTriggerProperty); }
			set { SetValue(UpdateTriggerProperty, value); }
		}

		#endregion

	}
}
