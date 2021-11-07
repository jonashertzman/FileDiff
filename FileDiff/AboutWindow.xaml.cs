﻿using System.Windows;

namespace FileDiff;

public partial class AboutWindow : Window
{

	public AboutWindow()
	{
		InitializeComponent();
	}

	private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
	{
		Process.Start(new ProcessStartInfo(e.Uri.ToString()) { UseShellExecute = true });
		e.Handled = true;
	}

	private void Feedback_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
	{
		MainWindowViewModel viewModel = DataContext as MainWindowViewModel;

		string subject = Uri.EscapeDataString(viewModel.FullApplicationName);
		string body = Uri.EscapeDataString("Hello");

		string mailto = $"mailto:jonashertzmansoftware@gmail.com?Subject={subject}&Body={body}";

		Process.Start(new ProcessStartInfo(mailto) { UseShellExecute = true });

		e.Handled = true;
	}

}
