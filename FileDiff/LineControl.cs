using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileDiff
{
	public class LineControl : Control
	{

		private Size characterSize = MeasureString("W");
		protected FormattedText rowNumberText;
		protected FormattedText lineText;

		#region Constructors

		static LineControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LineControl), new FrameworkPropertyMetadata(typeof(LineControl)));
		}

		public LineControl()
		{

		}

		#endregion

		#region Overrides

		protected override Size MeasureOverride(Size constraint)
		{
			//Debug.Print($"MeasureOverride rownumber:{RowNumber} text:{Text}");

			if (lineText == null)
			{
				return characterSize;
			}
			return new Size(lineText.Width + 30, lineText.Height);
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			//Debug.Print($"OnRender rownumber:{RowNumber} text:{Text}");

			var typeface = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);

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

		public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(LineControl),
			new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure, OnTextChanged));

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public static readonly DependencyProperty RowNumberProperty = DependencyProperty.Register("RowNumber", typeof(int), typeof(LineControl),
			new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure, OnRowNumberChanged));

		public int RowNumber
		{
			get { return (int)GetValue(RowNumberProperty); }
			set { SetValue(RowNumberProperty, value); }
		}

		#endregion

		#region Methods

		private static void OnRowNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
		}

		private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((LineControl)d).SetText(e.NewValue as string);
		}

		private void SetText(string v)
		{
			var typeface = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);

			lineText = new FormattedText(v, CultureInfo.CurrentCulture, this.FlowDirection, typeface, this.FontSize, Brushes.Black, null, TextFormattingMode.Display);
		}

		private static Size MeasureString(string text)
		{
			FormattedText formattedText = new FormattedText(
			text,
			CultureInfo.CurrentCulture,
			FlowDirection.LeftToRight,
			new Typeface(AppSettings.Settings.Font),
			AppSettings.Settings.FontSize,
			Brushes.Black,
			new NumberSubstitution(),
			TextFormattingMode.Display);

			return new Size(formattedText.Width, formattedText.Height);
		}

		#endregion

	}
}