using System.Globalization;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FileDiff
{
	/// <summary>
	/// Interaction logic for OptionsWindow.xaml
	/// </summary>
	public partial class OptionsWindow : Window
	{

		public OptionsWindow()
		{
			InitializeComponent();
			Owner = App.Current.MainWindow;
		}

		private void ButtonBrowseFont_Click(object sender, RoutedEventArgs e)
		{
			FontDialog fd = new FontDialog();
			fd.FontMustExist = true;

			if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				TextBoxFont.Text = fd.Font.Name;
				TextBoxFontSize.Text = (fd.Font.Size * 96.0 / 72.0).ToString(CultureInfo.InvariantCulture);
			}
		}

		private void Rectangle_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			Rectangle rectangle = e.Source as Rectangle;

			ColorDialog colorDialog = new ColorDialog();
			if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				rectangle.Fill = new SolidColorBrush(Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
			}
		}

		private void ButtonResetColors_Click(object sender, RoutedEventArgs e)
		{

		}

		private void ButtonResetFont_Click(object sender, RoutedEventArgs e)
		{
			TextBoxFont.Text = "Courier New";
			TextBoxFontSize.Text = "12";
		}

		private void ButtonOk_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}
