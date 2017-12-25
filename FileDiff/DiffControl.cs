using System.Collections.Generic;
using System.Diagnostics;
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
			Lines = new List<Line>();
			characterSize = MeasureString("W");
		}

		#region Overrides

		protected override Size MeasureOverride(Size constraint)
		{

			return new Size(1000, characterSize.Height * Lines.Count);
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			Debug.Print($"OnRender");

			Typeface typeface = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);

			if (RowNumber != -1)
			{
				rowNumberText = new FormattedText(RowNumber.ToString(), CultureInfo.CurrentCulture, this.FlowDirection, typeface, this.FontSize, Brushes.Black, null, TextFormattingMode.Display);
				drawingContext.DrawText(rowNumberText, new Point(0, 0));
			}

			if (lineText != null)
			{
				drawingContext.DrawRectangle(new SolidColorBrush(AppSettings.Settings.DeletedBackground), new Pen(Brushes.Transparent, 0), new Rect(30, 0, lineText.Width, lineText.Height));
				drawingContext.DrawText(lineText, new Point(30, 0));
			}
		}

		#endregion

		#region Properties

		public static readonly DependencyProperty LinesProperty = DependencyProperty.Register("Lines", typeof(List<Line>), typeof(LineControl),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

		public List<Line> Lines
		{
			get { return (List<Line>)GetValue(LinesProperty); }
			set { SetValue(LinesProperty, value); }
		}


		#endregion

		#region Methods

		private static void OnRowNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
		}

		private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
		}

		private  Size MeasureString(string text)
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
