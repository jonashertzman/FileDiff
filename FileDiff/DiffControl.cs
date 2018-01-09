using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		private Selection selection = new Selection();
		private SolidColorBrush slectionBrush;
		private Pen transpatentPen;

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
			drawingContext.DrawRectangle(AppSettings.fullMatchBackgroundBrush, transpatentPen, new Rect(0, 0, this.ActualWidth, this.ActualHeight));

			if (Lines.Count == 0)
				return;

			characterSize = MeasureString("W");
			lineNumberMargin = (Lines.Count.ToString().Length * (int)characterSize.Width) + 4;
			double maxTextwidth = 0;
			RectangleGeometry clippingRect = new RectangleGeometry(new Rect(lineNumberMargin, 0, ActualWidth, ActualHeight));

			Typeface typeface = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);

			for (int i = 0; i < VisibleLines; i++)
			{
				if (i + VerticalOffset >= Lines.Count)
					break;

				Line line = Lines[i + VerticalOffset];

				if (line.LineIndex != -1)
				{
					drawingContext.DrawRectangle(line.BackgroundBrush, transpatentPen, new Rect(0, characterSize.Height * i, this.ActualWidth, characterSize.Height));

					FormattedText rowNumberText = new FormattedText(line.LineIndex.ToString(), CultureInfo.CurrentCulture, this.FlowDirection, typeface, this.FontSize, SystemColors.ControlDarkBrush, null, TextFormattingMode.Display);
					drawingContext.DrawText(rowNumberText, new Point(lineNumberMargin - rowNumberText.Width - 3, characterSize.Height * i));

					drawingContext.PushClip(clippingRect);

					double nextPosition = lineNumberMargin - HorizontalOffset;
					foreach (TextSegment textSegment in line.TextSegments)
					{
						foreach (string s in Split(textSegment.Text))
						{
							FormattedText segmentText = new FormattedText(s, CultureInfo.CurrentCulture, this.FlowDirection, typeface, this.FontSize, textSegment.ForegroundBrush, null, TextFormattingMode.Display);
							double segmentWidth = s == "\t" ? AppSettings.Settings.TabSize * characterSize.Width : segmentText.WidthIncludingTrailingWhitespace;
							drawingContext.DrawRectangle(textSegment.BackgroundBrush, transpatentPen, new Rect(nextPosition, characterSize.Height * i, segmentWidth, segmentText.Height));
							drawingContext.DrawText(segmentText, new Point(nextPosition, characterSize.Height * i));
							nextPosition += segmentWidth;
						}
					}
					maxTextwidth = Math.Max(maxTextwidth, nextPosition);

					drawingContext.Pop();

				}
			}
			TextWidth = (int)maxTextwidth;

			//for (int i = selection.TopLine - VerticalOffset; i <= selection.BottomLine - VerticalOffset; i++)
			//{
			//	drawingContext.DrawRectangle(slectionBrush, transpatentPen, new Rect(lineNumberMargin, characterSize.Height * i, this.ActualWidth, characterSize.Height));
			//}

			drawingContext.DrawLine(new Pen(SystemColors.ScrollBarBrush, 1), new Point(lineNumberMargin - 1.5, 0), new Point(lineNumberMargin - 1.5, this.ActualHeight));
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
			selection.EndCharacter = (int)(pos.Y / characterSize.Width) + HorizontalOffset;

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
			}

			base.OnMouseMove(e);

			InvalidateVisual();
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

		public static readonly DependencyProperty LinesProperty = DependencyProperty.Register("Lines", typeof(ObservableCollection<Line>), typeof(DiffControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

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


		public static readonly DependencyProperty TextWidthPropery = DependencyProperty.Register("TextWidth", typeof(int), typeof(DiffControl));

		public int TextWidth
		{
			get { return (int)GetValue(TextWidthPropery); }
			set { SetValue(TextWidthPropery, value); }
		}

		#endregion

		#region Methods

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
