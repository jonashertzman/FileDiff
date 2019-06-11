using System.Globalization;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Linq;

namespace FileDiff
{
	/// <summary>
	/// Interaction logic for OptionsWindow.xaml
	/// </summary>
	public partial class OptionsWindow : Window
	{

		#region Members

		Rectangle selectecRectangle;

		#endregion

		#region Constructor

		public OptionsWindow()
		{
			InitializeComponent();

			foreach (FontFamily family in Fonts.SystemFontFamilies.OrderBy(x => x.Source))
			{
				ComboBoxFont.Items.Add(family.Source);
			}
		}

		#endregion

		#region Events

		private void ButtonBrowseFont_Click(object sender, RoutedEventArgs e)
		{
			FontDialog fd = new FontDialog();
			fd.FontMustExist = true;

			if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				ComboBoxFont.Text = fd.Font.Name;
				TextBoxFontSize.Text = ((int)(fd.Font.Size * 96.0 / 72.0)).ToString(CultureInfo.InvariantCulture);
			}
		}

		private void Rectangle_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			selectecRectangle = e.Source as Rectangle;

			Color currentColor = Color.FromArgb((byte)(selectecRectangle == SelectionBackground ? 50 : 255), ((SolidColorBrush)selectecRectangle.Fill).Color.R, ((SolidColorBrush)selectecRectangle.Fill).Color.G, ((SolidColorBrush)selectecRectangle.Fill).Color.B);

			SliderR.Value = currentColor.R;
			SliderG.Value = currentColor.G;
			SliderB.Value = currentColor.B;

			ColorHex.Text = currentColor.ToString();

			ColorChooser.IsOpen = true;
		}

		private void ButtonResetColors_Click(object sender, RoutedEventArgs e)
		{
			FullMatchForeground.Fill = new SolidColorBrush(DefaultSettings.FullMatchForeground);
			FullMatchBackground.Fill = new SolidColorBrush(DefaultSettings.FullMatchBackground);

			PartialMatchForeground.Fill = new SolidColorBrush(DefaultSettings.PartialMatchForeground);
			PartialMatchBackground.Fill = new SolidColorBrush(DefaultSettings.PartialMatchBackground);

			DeletedForeground.Fill = new SolidColorBrush(DefaultSettings.DeletedForeground);
			DeletedBackground.Fill = new SolidColorBrush(DefaultSettings.DeletedBackground);

			NewForeground.Fill = new SolidColorBrush(DefaultSettings.NewForeground);
			NewBackground.Fill = new SolidColorBrush(DefaultSettings.NewBackground);

			IgnoredForeground.Fill = new SolidColorBrush(DefaultSettings.IgnoredForeground);
			IgnoredBackground.Fill = new SolidColorBrush(DefaultSettings.IgnoredBackground);

			SelectionBackground.Fill = new SolidColorBrush(DefaultSettings.SelectionBackground);
		}

		private void ButtonResetFont_Click(object sender, RoutedEventArgs e)
		{
			ComboBoxFont.Text = DefaultSettings.Font;
			TextBoxFontSize.Text = DefaultSettings.FontSize.ToString();
		}

		private void ButtonOk_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			Color newColor = Color.FromArgb((byte)(selectecRectangle == SelectionBackground ? 50 : 255), (byte)SliderR.Value, (byte)SliderG.Value, (byte)SliderB.Value);
			ColorHex.Text = newColor.ToString();
			selectecRectangle.Fill = new SolidColorBrush(newColor);
		}

		#endregion

	}
}
