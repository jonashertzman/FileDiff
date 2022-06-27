using System.Security.Policy;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FileDiff;

public partial class OptionsWindow : Window
{

	#region Members

	Rectangle selectecRectangle;

	#endregion

	#region Constructor

	public OptionsWindow()
	{
		InitializeComponent();

		Utils.HideMinimizeAndMaximizeButtons(this);

		foreach (FontFamily family in Fonts.SystemFontFamilies.OrderBy(x => x.Source))
		{
			ComboBoxFont.Items.Add(family.Source);
		}

		foreach (string name in Enum.GetNames(typeof(ColorTheme)))
		{
			ComboBoxTheme.Items.Add(name);
		}
	}

	#endregion

	#region Methods

	private void CleanIgnores()
	{
		MainWindowViewModel viewModel = DataContext as MainWindowViewModel;

		for (int i = viewModel.IgnoredFolders.Count - 1; i >= 0; i--)
		{
			if (string.IsNullOrWhiteSpace(viewModel.IgnoredFolders[i].Text))
			{
				viewModel.IgnoredFolders.RemoveAt(i);
			}
		}

		for (int i = viewModel.IgnoredFiles.Count - 1; i >= 0; i--)
		{
			if (string.IsNullOrWhiteSpace(viewModel.IgnoredFiles[i].Text))
			{
				viewModel.IgnoredFiles.RemoveAt(i);
			}
		}
	}

	#endregion

	#region Events

	private void Rectangle_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
	{
		selectecRectangle = e.Source as Rectangle;

		LabelA.Visibility = selectecRectangle == SelectionBackground ? Visibility.Visible : Visibility.Collapsed;
		SliderA.Visibility = selectecRectangle == SelectionBackground ? Visibility.Visible : Visibility.Collapsed;

		Color currentColor = Color.FromArgb((byte)(selectecRectangle == SelectionBackground ? ((SolidColorBrush)selectecRectangle.Fill).Color.A : 255), ((SolidColorBrush)selectecRectangle.Fill).Color.R, ((SolidColorBrush)selectecRectangle.Fill).Color.G, ((SolidColorBrush)selectecRectangle.Fill).Color.B);

		SliderR.Value = currentColor.R;
		SliderG.Value = currentColor.G;
		SliderB.Value = currentColor.B;
		SliderA.Value = currentColor.A;

		ColorHex.Text = currentColor.ToString();

		ColorChooser.IsOpen = true;
	}

	private void ButtonResetColors_Click(object sender, RoutedEventArgs e)
	{
		FullMatchForeground.Fill = new SolidColorBrush(AppSettings.CurrentTheme.Default.FullMatchForeground);
		FullMatchBackground.Fill = new SolidColorBrush(AppSettings.CurrentTheme.Default.FullMatchBackground);

		PartialMatchForeground.Fill = new SolidColorBrush(AppSettings.CurrentTheme.Default.PartialMatchForeground);
		PartialMatchBackground.Fill = new SolidColorBrush(AppSettings.CurrentTheme.Default.PartialMatchBackground);

		DeletedForeground.Fill = new SolidColorBrush(AppSettings.CurrentTheme.Default.DeletedForeground);
		DeletedBackground.Fill = new SolidColorBrush(AppSettings.CurrentTheme.Default.DeletedBackground);

		NewForeground.Fill = new SolidColorBrush(AppSettings.CurrentTheme.Default.NewForeground);
		NewBackground.Fill = new SolidColorBrush(AppSettings.CurrentTheme.Default.NewBackground);

		IgnoredForeground.Fill = new SolidColorBrush(AppSettings.CurrentTheme.Default.IgnoredForeground);
		IgnoredBackground.Fill = new SolidColorBrush(AppSettings.CurrentTheme.Default.IgnoredBackground);

		MovedFromBackground.Fill = new SolidColorBrush(AppSettings.CurrentTheme.Default.MovedFromdBackground);
		MovedToBackground.Fill = new SolidColorBrush(AppSettings.CurrentTheme.Default.MovedToBackground);

		SelectionBackground.Fill = new SolidColorBrush(AppSettings.CurrentTheme.Default.SelectionBackground);

		LineNumber.Fill = new SolidColorBrush(AppSettings.CurrentTheme.Default.LineNumberColor);
		CurrentDiff.Fill = new SolidColorBrush(AppSettings.CurrentTheme.Default.CurrentDiffColor);
		Snake.Fill = new SolidColorBrush(AppSettings.CurrentTheme.Default.SnakeColor);
	}

	private void ButtonResetFont_Click(object sender, RoutedEventArgs e)
	{
		ComboBoxFont.Text = DefaultSettings.Font;
		TextBoxFontSize.Text = DefaultSettings.FontSize.ToString();
	}

	private void ButtonOk_Click(object sender, RoutedEventArgs e)
	{
		CleanIgnores();
		DialogResult = true;
	}

	private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
	{
		byte alpha = (byte)(selectecRectangle == SelectionBackground ? (byte)SliderA.Value : 255);

		if (Keyboard.IsKeyDown(Key.LeftCtrl))
		{
			SliderR.Value = e.NewValue;
			SliderG.Value = e.NewValue;
			SliderB.Value = e.NewValue;
		}

		Color newColor = Color.FromArgb(alpha, (byte)SliderR.Value, (byte)SliderG.Value, (byte)SliderB.Value);
		ColorHex.Text = newColor.ToString();
		selectecRectangle.Fill = new SolidColorBrush(newColor);

		SliderR.Background = new LinearGradientBrush(Color.FromArgb(alpha, 0, newColor.G, newColor.B), Color.FromArgb(alpha, 255, newColor.G, newColor.B), 0);
		SliderG.Background = new LinearGradientBrush(Color.FromArgb(alpha, newColor.R, 0, newColor.B), Color.FromArgb(alpha, newColor.R, 255, newColor.B), 0);
		SliderB.Background = new LinearGradientBrush(Color.FromArgb(alpha, newColor.R, newColor.G, 0), Color.FromArgb(alpha, newColor.R, newColor.G, 255), 0);
		SliderA.Background = new LinearGradientBrush(Color.FromArgb(0, newColor.R, newColor.G, newColor.B), Color.FromArgb(255, newColor.R, newColor.G, newColor.B), 0);
	}

	#endregion

}
