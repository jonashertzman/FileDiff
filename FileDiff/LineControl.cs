using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FileDiff
{
	public class LineControl : Control
	{

		private Size textSize;

		private Size size = new Size();

		#region Constructors

		static LineControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LineControl), new FrameworkPropertyMetadata(typeof(LineControl)));
		}

		public LineControl()
		{
			textSize = MeasureString("W");
			size = new Size(100, textSize.Height);
		}

		#endregion

		#region Overrides

		protected override Size MeasureOverride(Size constraint)
		{
			//Debug.Print("MeasureOverride");
			return size;
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			//Debug.Print("OnRender" + VisualOffset.ToString());
			Typeface typeface = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);
			FormattedText rowNumberText = new FormattedText(RowNumber.ToString(), CultureInfo.CurrentCulture, this.FlowDirection, typeface, this.FontSize, Brushes.Black, null, TextFormattingMode.Display);
			drawingContext.DrawText(rowNumberText, new Point(0, 0));

			FormattedText lineText = new FormattedText(Text, CultureInfo.CurrentCulture, this.FlowDirection, typeface, this.FontSize, Brushes.Black, null, TextFormattingMode.Display);
			drawingContext.DrawRectangle(new SolidColorBrush(AppSettings.Settings.DeletedBackground), new Pen(Brushes.Transparent, 0), new Rect(30, 0, lineText.Width, size.Height));
			drawingContext.DrawText(lineText, new Point(30, 0));
		}

		#endregion


		public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(LineControl));
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}


		public static readonly DependencyProperty RowNumberProperty = DependencyProperty.Register("RowNumber", typeof(int), typeof(LineControl));
		public int RowNumber
		{
			get { return (int)GetValue(RowNumberProperty); }
			set { SetValue(RowNumberProperty, value); }
		}


		private Size MeasureString(string text)
		{
			FormattedText formattedText = new FormattedText(
					text,
					CultureInfo.CurrentCulture,
					FlowDirection.LeftToRight,
					new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch),
					this.FontSize,
					Brushes.Black,
					new NumberSubstitution(),
					TextFormattingMode.Display);

			return new Size(formattedText.Width, formattedText.Height);
		}


	}
}
