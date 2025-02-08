using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace FileDiff;

public partial class ExceptionWindow : Window, INotifyPropertyChanged
{

	#region Constructor

	public ExceptionWindow()
	{
		InitializeComponent();
		DataContext = this;
	}

	#endregion

	#region Properties

	string exceptionType;
	public string ExceptionType
	{
		get { return exceptionType; }
		set { exceptionType = value; OnPropertyChanged(ExceptionType); }
	}

	string exceptionMessage;
	public string ExceptionMessage
	{
		get { return exceptionMessage; }
		set { exceptionMessage = value; OnPropertyChanged(ExceptionMessage); }
	}

	string source;
	public string Source
	{
		get { return source; }
		set { source = value; OnPropertyChanged(Source); }
	}

	string stackTrace;
	public string StackTrace
	{
		get { return stackTrace; }
		set { stackTrace = value; OnPropertyChanged(StackTrace); }
	}

	#endregion

	#region Events

	private void CloseButton_Click(object sender, RoutedEventArgs e)
	{
		this.Close();
	}

	private async void ReportButton_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			HttpClientHandler handler = new HttpClientHandler();
			handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

			HttpClient httpClient = new HttpClient(handler);

			CrashReportRequest cr = new()
			{
				ApplicationName = "FileDiff",
				BuildNumber = AppSettings.BuildNumber,
				ClientId = AppSettings.Id,
				ExceptionType = this.ExceptionType,
				ExceptionMessage = this.ExceptionMessage,
				Source = this.Source,
				StackTrace = this.StackTrace
			};

			string json = JsonSerializer.Serialize(cr);
			var content = new StringContent(json, Encoding.UTF8, "application/json");

			var response = await httpClient.PostAsync("https://a57ee9618f0a6ac2f5546d8b2704f9ec6.asuscomm.com:7133/api/CrashReport", content);
		}
		catch (Exception ex)
		{
			Debug.Print($"Crash report failed: {ex.Message}");
		}

		//this.Close();
	}

	private void OpenLogFileButton_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			using Process p = new();

			p.StartInfo.FileName = AppSettings.LogPath;
			p.StartInfo.ErrorDialog = true;
			p.StartInfo.UseShellExecute = true;
			p.Start();
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"Cannot open log file. {ex.Message}");
		}
	}

	#endregion

	#region INotifyPropertyChanged

	public event PropertyChangedEventHandler PropertyChanged;

	public void OnPropertyChanged(string name)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}

	#endregion

}
