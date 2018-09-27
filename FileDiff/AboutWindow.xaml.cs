using System.Diagnostics;
using System.Windows;

namespace FileDiff
{
	/// <summary>
	/// Interaction logic for AboutWindow1.xaml
	/// </summary>
	public partial class AboutWindow : Window
	{

		public AboutWindow()
		{
			InitializeComponent();
			Owner = App.Current.MainWindow;
		}

		private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}

	}
}
