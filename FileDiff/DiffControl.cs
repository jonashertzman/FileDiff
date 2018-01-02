using System.Collections.ObjectModel;
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
		}

		#region Overrides

		protected override Size MeasureOverride(Size constraint)
		{
			characterSize = MeasureString("W");
			return new Size(1000, characterSize.Height * Lines.Count);
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			characterSize = MeasureString("W");
			int margin = (Lines.Count.ToString().Length * (int)characterSize.Width) + 3;

			Typeface typeface = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);

			int lineIndex = 0;

			foreach (Line l in Lines)
			{
				if (l.LineIndex != -1)
				{
					if (l.Type != TextState.FullMatch)
					{
						drawingContext.DrawRectangle(new SolidColorBrush(AppSettings.Settings.DeletedBackground), new Pen(Brushes.Transparent, 0), new Rect(0, characterSize.Height * lineIndex, 1000, characterSize.Height));
					}
					FormattedText rowNumberText = new FormattedText(l.LineIndex.ToString(), CultureInfo.CurrentCulture, this.FlowDirection, typeface, this.FontSize, SystemColors.ScrollBarBrush, null, TextFormattingMode.Display);
					drawingContext.DrawText(rowNumberText, new Point(1, characterSize.Height * lineIndex));

					FormattedText lineText = new FormattedText(l.Text, CultureInfo.CurrentCulture, this.FlowDirection, typeface, this.FontSize, Brushes.Black, null, TextFormattingMode.Display);
					drawingContext.DrawText(lineText, new Point(margin, characterSize.Height * lineIndex));
				}

				lineIndex++;
			}
		}

		#endregion

		#region Properties

		public static readonly DependencyProperty LinesProperty = DependencyProperty.Register("Lines", typeof(ObservableCollection<Line>), typeof(DiffControl),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

		public ObservableCollection<Line> Lines
		{
			get { return (ObservableCollection<Line>)GetValue(LinesProperty); }
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
