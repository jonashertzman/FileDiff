﻿using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileDiff
{
	public class DiffControl : Control
	{

		private Size characterSize;

		static DiffControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(DiffControl), new FrameworkPropertyMetadata(typeof(DiffControl)));
		}

		public DiffControl()
		{
			Lines = new ObservableCollection<Line>();
			this.ClipToBounds = true;
		}

		#region Overrides

		protected override void OnRender(DrawingContext drawingContext)
		{
			if (Lines.Count == 0)
				return;

			characterSize = MeasureString("W");
			int margin = (Lines.Count.ToString().Length * (int)characterSize.Width) + 4;
			Typeface typeface = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);

			for (int i = 0; i < VisibleLines; i++)
			{
				if (i + VerticalOffset >= Lines.Count)
					break;

				Line line = Lines[i + VerticalOffset];

				if (line.LineIndex != -1)
				{
					drawingContext.DrawRectangle(line.BackgroundBrush, new Pen(Brushes.Transparent, 0), new Rect(0, characterSize.Height * i, this.ActualWidth, characterSize.Height));

					FormattedText rowNumberText = new FormattedText(line.LineIndex.ToString(), CultureInfo.CurrentCulture, this.FlowDirection, typeface, this.FontSize, SystemColors.ControlDarkBrush, null, TextFormattingMode.Display);
					drawingContext.DrawText(rowNumberText, new Point(1, characterSize.Height * i));

					FormattedText lineText = new FormattedText(line.Text, CultureInfo.CurrentCulture, this.FlowDirection, typeface, this.FontSize, line.ForegroundBrush, null, TextFormattingMode.Display);
					drawingContext.DrawText(lineText, new Point(margin, characterSize.Height * i));
				}
			}

			drawingContext.DrawLine(new Pen(SystemColors.ScrollBarBrush, 1), new Point(margin - 1.5, 0), new Point(margin - 1.5, this.ActualHeight));
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

		public static readonly DependencyProperty LinesProperty = DependencyProperty.Register("Lines", typeof(ObservableCollection<Line>), typeof(DiffControl),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

		public ObservableCollection<Line> Lines
		{
			get { return (ObservableCollection<Line>)GetValue(LinesProperty); }
			set { SetValue(LinesProperty, value); }
		}


		public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register("VerticalOffset", typeof(int), typeof(DiffControl),
			new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

		public int VerticalOffset
		{
			get { return (int)GetValue(VerticalOffsetProperty); }
			set { SetValue(VerticalOffsetProperty, value); }
		}

		public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register("HorizontalOffset", typeof(int), typeof(DiffControl),
			new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

		public int HorizontallOffset
		{
			get { return (int)GetValue(HorizontalOffsetProperty); }
			set { SetValue(HorizontalOffsetProperty, value); }
		}


		#endregion

		#region Methods

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
