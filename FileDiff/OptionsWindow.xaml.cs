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
			DataContext = (Owner as MainWindow).ViewModel;
		}

		private void ButtonBrowseFont_Click(object sender, RoutedEventArgs e)
		{
			FontDialog fd = new FontDialog();
			fd.FontMustExist = true;

			if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				TextBoxFont.Text = fd.Font.Name;
				TextBoxFontSize.Text = ((int)(fd.Font.Size * 96.0 / 72.0)).ToString(CultureInfo.InvariantCulture);
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
			FullMatchForeground.Fill = new SolidColorBrush(DefaultSettings.FullMatchForeground);
			FullMatchBackground.Fill = new SolidColorBrush(DefaultSettings.FullMatchBackground);

			PartialMatchForeground.Fill = new SolidColorBrush(DefaultSettings.PartialMatchForeground);
			PartialMatchBackground.Fill = new SolidColorBrush(DefaultSettings.PartialMatchBackground);

			DeletedForeground.Fill = new SolidColorBrush(DefaultSettings.DeletedForeground);
			DeletedBackground.Fill = new SolidColorBrush(DefaultSettings.DeletedBackground);

			NewForeground.Fill = new SolidColorBrush(DefaultSettings.NewForeground);
			NewBackground.Fill = new SolidColorBrush(DefaultSettings.NewBackground);
		}

		private void ButtonResetFont_Click(object sender, RoutedEventArgs e)
		{
			TextBoxFont.Text = DefaultSettings.Font;
			TextBoxFontSize.Text = DefaultSettings.FontSize.ToString();
		}

		private void ButtonOk_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}
