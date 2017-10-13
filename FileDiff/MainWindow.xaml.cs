using System;
using System.IO;
using System.Windows;

namespace FileDiff
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		WindowData windowData = new WindowData();

		public MainWindow()
		{
			InitializeComponent();

			this.DataContext = this.windowData;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (Environment.GetCommandLineArgs().Length > 2)
			{
				windowData.LeftFile = Environment.GetCommandLineArgs()[1];
				windowData.RightFile = Environment.GetCommandLineArgs()[2];

				if (File.Exists(windowData.LeftFile) && File.Exists(windowData.RightFile))
				{
					CompareFiles();
				}
			}
		}

		private void CompareFiles()
		{
			int i = 1;
			foreach (string s in File.ReadAllLines(windowData.LeftFile))
			{
				windowData.LeftSide.Add(new Line() { Text = s, LineNumber = i++ });
			}

			i = 1;
			foreach (string s in File.ReadAllLines(windowData.RightFile))
			{
				windowData.RightSide.Add(new Line() { Text = s, LineNumber = i++ });
			}
		}
	}
}
