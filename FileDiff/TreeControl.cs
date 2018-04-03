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

	public class TreeControl : Control
	{
		private const double handleWidth = 4;

		private double characterHeight;
		private double characterWidth;

		private SolidColorBrush slectionBrush;

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
		}

		#endregion

		#region Overrides

		protected override void OnRender(DrawingContext drawingContext)
		{
			Debug.Print("TreeControl OnRender");

			// Fill background
			drawingContext.DrawRectangle(AppSettings.FullMatchBackground, null, new Rect(0, 0, this.ActualWidth, this.ActualHeight));

			if (Lines.Count == 0)
				return;

			int itemMargin = 1;
			Pen selectionPen = new Pen(new SolidColorBrush(SystemColors.HighlightColor), itemMargin);

			Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
			dpiScale = 1 / m.M11;

			Typeface t = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);
			if (t.TryGetGlyphTypeface(out GlyphTypeface temp))
			{
				cachedTypeface = temp;
			}

			CreateGlyphRun("W", out characterWidth);

			characterHeight = Math.Ceiling((MeasureString("W").Height + (2 * itemMargin)) / dpiScale) * dpiScale;

			visibleItems = new List<FileItem>();
			GetVisibleItems(Lines, visibleItems);

			VisibleLines = (int)(ActualHeight / characterHeight + 1);
			MaxVerialcalScroll = visibleItems.Count - VisibleLines + 1;

			for (int i = 0; i < VisibleLines; i++)
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
					drawingContext.DrawRectangle(line.BackgroundBrush, null, new Rect(0, 0, Math.Max(this.ActualWidth, 0), characterHeight));
				}

				// Draw selection
				if (line.IsSelected)
				{
					drawingContext.DrawRectangle(null, selectionPen, new Rect(.5, .5, Math.Max(this.ActualWidth - 1, 0), characterHeight - 1));
				}

				drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, AppSettings.NameColumnWidth, characterHeight)));

				// Draw folder expander
				if (line.IsFolder)
				{
					double expanderWidth = 5.5;
					drawingContext.DrawRectangle(Brushes.Transparent, new Pen(new SolidColorBrush(Colors.Black), 1), new Rect(expanderWidth + ((line.Level - 1) * characterHeight), expanderWidth, characterHeight - (expanderWidth * 2), characterHeight - (expanderWidth * 2)));
					drawingContext.DrawLine(new Pen(new SolidColorBrush(Colors.Black), 1), new Point(expanderWidth + ((line.Level - 1) * characterHeight), characterHeight / 2), new Point(characterHeight - expanderWidth + ((line.Level - 1) * characterHeight), characterHeight / 2));
					if (!line.IsExpanded)
					{
						drawingContext.DrawLine(new Pen(new SolidColorBrush(Colors.Black), 1), new Point((characterHeight / 2) + ((line.Level - 1) * characterHeight), expanderWidth), new Point((characterHeight / 2) + ((line.Level - 1) * characterHeight), characterHeight - expanderWidth));
					}
				}

				// Draw line text
				drawingContext.DrawText(new FormattedText(line.Name, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, t, this.FontSize, line.ForegroundBrush, null, TextFormattingMode.Display), new Point(line.Level * characterHeight, itemMargin));
				drawingContext.Pop();

				drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, AppSettings.NameColumnWidth + handleWidth + AppSettings.SizeColumnWidth, characterHeight)));
				drawingContext.DrawText(new FormattedText(line.Size, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, t, this.FontSize, line.ForegroundBrush, null, TextFormattingMode.Display), new Point(AppSettings.NameColumnWidth + handleWidth, itemMargin));
				drawingContext.Pop();

				drawingContext.DrawText(new FormattedText(line.Date, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, t, this.FontSize, line.ForegroundBrush, null, TextFormattingMode.Display), new Point(AppSettings.NameColumnWidth + AppSettings.SizeColumnWidth + (handleWidth * 2), itemMargin));

				drawingContext.Pop(); // Line Y offset 
			}
		}

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			this.Focus();
		}

		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			int line = (int)(e.GetPosition(this).Y / characterHeight) + VerticalOffset;

			if (e.ChangedButton == MouseButton.Left && line < visibleItems.Count)
			{
				if (e.GetPosition(this).X < (visibleItems[line].Level * characterHeight))
				{
					visibleItems[line].IsExpanded = !visibleItems[line].IsExpanded;

					visibleItems = new List<FileItem>();
					GetVisibleItems(Lines, visibleItems);
				}
				else
				{
					foreach (FileItem i in visibleItems)
					{
						i.IsSelected = false;
					}
					visibleItems[line].IsSelected = true;
				}
				UpdateTrigger++;
			}

			base.OnMouseUp(e);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.Key == Key.PageUp)
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


		#endregion

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

		#region Methods

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

		#endregion

	}
}
