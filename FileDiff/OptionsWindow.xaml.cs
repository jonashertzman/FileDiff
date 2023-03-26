using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FileDiff;

public partial class OptionsWindow : Window
{

	#region Members

	Rectangle selectedRectangle;

	#endregion

	#region Constructor

	public OptionsWindow()
	{
		InitializeComponent();

		Utils.HideMinimizeAndMaximizeButtons(this);

		foreach (FontFamily family in Fonts.SystemFontFamilies.OrderBy(x => x.Source))
		{
			ComboBoxFont.Items.Add(new ComboBoxItem { Content = family.Source });
		}

		foreach (string name in Enum.GetNames(typeof(Themes)))
		{
			ComboBoxTheme.Items.Add(new ComboBoxItem { Content = name });
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
		selectedRectangle = e.Source as Rectangle;

		LabelA.Visibility = selectedRectangle == SelectionBackground ? Visibility.Visible : Visibility.Collapsed;
		SliderA.Visibility = selectedRectangle == SelectionBackground ? Visibility.Visible : Visibility.Collapsed;

		Color currentColor = Color.FromArgb((byte)(selectedRectangle == SelectionBackground ? ((SolidColorBrush)selectedRectangle.Fill).Color.A : 255), ((SolidColorBrush)selectedRectangle.Fill).Color.R, ((SolidColorBrush)selectedRectangle.Fill).Color.G, ((SolidColorBrush)selectedRectangle.Fill).Color.B);

		SliderR.Value = currentColor.R;
		SliderG.Value = currentColor.G;
		SliderB.Value = currentColor.B;
		SliderA.Value = currentColor.A;

		ColorHex.Text = currentColor.ToString();

		ColorChooser.IsOpen = true;
		SliderR.Focus();
	}

	private void ButtonResetColors_Click(object sender, RoutedEventArgs e)
	{
		var Default = AppSettings.Theme switch
		{
			Themes.Light => DefaultSettings.LightTheme,
			Themes.Dark => DefaultSettings.DarkTheme,
			_ => throw new NotImplementedException(),
		};

		// Diff colors
		FullMatchForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.FullMatchForeground));
		FullMatchBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.FullMatchBackground));

		PartialMatchForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.PartialMatchForeground));
		PartialMatchBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.PartialMatchBackground));

		DeletedForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.DeletedForeground));
		DeletedBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.DeletedBackground));

		NewForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.NewForeground));
		NewBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.NewBackground));

		IgnoredForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.IgnoredForeground));
		IgnoredBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.IgnoredBackground));

		MovedFromBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.MovedFromBackground));
		MovedToBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.MovedToBackground));

		//LineNumber.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.LineNumberColor));
		//CurrentDiff.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.CurrentDiffColor));
		//Snake.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.SnakeColor));

		SelectionBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.SelectionBackground));

		// GUI colors
		WindowForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.NormalText));
		DisabledForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.DisabledText));

		WindowBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.WindowBackground));
		DialogBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.DialogBackground));

		ControlLightBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.ControlLightBackground));
		ControlDarkBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.ControlDarkBackground));

		BorderForegroundx.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.BorderLight));
		BorderDarkForegroundx.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.BorderDark));

		HighlightBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.HighlightBackground));
		HighlightBorder.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Default.HighlightBorder));



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
		byte alpha = (byte)(selectedRectangle == SelectionBackground ? (byte)SliderA.Value : 255);

		if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
		{
			SliderR.Value = e.NewValue;
			SliderG.Value = e.NewValue;
			SliderB.Value = e.NewValue;
		}

		Color newColor = Color.FromArgb(alpha, (byte)SliderR.Value, (byte)SliderG.Value, (byte)SliderB.Value);
		ColorHex.Text = newColor.ToString();
		selectedRectangle.Fill = new SolidColorBrush(newColor);

		SliderR.Background = new LinearGradientBrush(Color.FromArgb(alpha, 0, newColor.G, newColor.B), Color.FromArgb(alpha, 255, newColor.G, newColor.B), 0);
		SliderG.Background = new LinearGradientBrush(Color.FromArgb(alpha, newColor.R, 0, newColor.B), Color.FromArgb(alpha, newColor.R, 255, newColor.B), 0);
		SliderB.Background = new LinearGradientBrush(Color.FromArgb(alpha, newColor.R, newColor.G, 0), Color.FromArgb(alpha, newColor.R, newColor.G, 255), 0);
		SliderA.Background = new LinearGradientBrush(Color.FromArgb(0, newColor.R, newColor.G, newColor.B), Color.FromArgb(255, newColor.R, newColor.G, newColor.B), 0);
	}

	private void Border_PreviewKeyDown(object sender, KeyEventArgs e)
	{
		bool controlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

		if (e.Key == Key.C && controlPressed)
		{
			WinApi.CopyTextToClipboard(ColorHex.Text);

			e.Handled = true;
			return;
		}
		else if (e.Key == Key.V && controlPressed)
		{
			string colorString = Clipboard.GetText();

			SolidColorBrush newBrush = colorString.ToBrush();
			if (newBrush != null)
			{
				SliderR.Value = newBrush.Color.R;
				SliderG.Value = newBrush.Color.G;
				SliderB.Value = newBrush.Color.B;
				SliderA.Value = newBrush.Color.A;

				e.Handled = true;
				return;
			}
		}
		e.Handled = false;
	}

	#endregion

}
