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

	bool ShowAlpha
	{
		get
		{
			return selectedRectangle.In([SelectionBackground, WhiteSpaceForeground]);
		}
	}

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

		LabelA.Visibility = ShowAlpha ? Visibility.Visible : Visibility.Collapsed;
		SliderA.Visibility = ShowAlpha ? Visibility.Visible : Visibility.Collapsed;

		Color currentColor = Color.FromArgb((byte)(ShowAlpha ? ((SolidColorBrush)selectedRectangle.Fill).Color.A : 255), ((SolidColorBrush)selectedRectangle.Fill).Color.R, ((SolidColorBrush)selectedRectangle.Fill).Color.G, ((SolidColorBrush)selectedRectangle.Fill).Color.B);

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
		ColorTheme themeDefaults = AppSettings.Theme switch
		{
			Themes.Light => DefaultSettings.LightTheme,
			Themes.Dark => DefaultSettings.DarkTheme,
			_ => throw new NotImplementedException(),
		};

		// Folder diff colors
		FolderFullMatchForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.FolderFullMatchForeground));
		FolderFullMatchBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.FolderFullMatchBackground));

		FolderPartialMatchForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.FolderPartialMatchForeground));
		FolderPartialMatchBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.FolderPartialMatchBackground));

		FolderDeletedForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.FolderDeletedForeground));
		FolderDeletedBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.FolderDeletedBackground));

		FolderNewForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.FolderNewForeground));
		FolderNewBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.FolderNewBackground));

		FolderIgnoredForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.FolderIgnoredForeground));
		FolderIgnoredBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.FolderIgnoredBackground));

		// File diff colors
		FullMatchForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.FullMatchForeground));
		FullMatchBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.FullMatchBackground));

		PartialMatchForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.PartialMatchForeground));
		PartialMatchBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.PartialMatchBackground));

		DeletedForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.DeletedForeground));
		DeletedBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.DeletedBackground));

		NewForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.NewForeground));
		NewBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.NewBackground));

		IgnoredForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.IgnoredForeground));
		IgnoredBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.IgnoredBackground));

		MovedFromBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.MovedFromBackground));
		MovedToBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.MovedToBackground));

		WhiteSpaceForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.WhiteSpaceForeground));

		LineNumber.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.LineNumberColor));
		CurrentDiff.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.CurrentDiffColor));
		Snake.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.SnakeColor));

		SelectionBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.SelectionBackground));

		// UI colors
		WindowForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.NormalText));
		DisabledForeground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.DisabledText));

		WindowBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.WindowBackground));
		DialogBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.DialogBackground));

		ControlLightBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.ControlLightBackground));
		ControlDarkBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.ControlDarkBackground));

		BorderForegroundx.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.BorderLight));
		BorderDarkForegroundx.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.BorderDark));

		HighlightBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.HighlightBackground));
		HighlightBorder.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(themeDefaults.HighlightBorder));

		AttentionBackground.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString((themeDefaults.AttentionBackground)));
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
		byte alpha = (byte)(ShowAlpha ? (byte)SliderA.Value : 255);

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

	private void ColorHex_TextChanged(object sender, TextChangedEventArgs e)
	{
		if (selectedRectangle != null)
		{
			if (ColorHex.Text.ToBrush() != null)
			{
				ColorHex.Error = false;
				selectedRectangle.Fill = ColorHex.Text.ToBrush();
			}
			else
			{
				ColorHex.Error = true;
			}
		}
	}

	#endregion

}
