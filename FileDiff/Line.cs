using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace FileDiff
{
	public class Line : INotifyPropertyChanged
	{

		#region Members

		private int hash;
		private int hashNoWhitespace;

		#endregion

		#region Constructor

		public Line()
		{

		}

		#endregion

		#region Overrides

		public override string ToString()
		{
			return $"{LineIndex}  {Text}  {MatchingLineIndex}";
		}

		public override int GetHashCode()
		{
			return AppSettings.IgnoreWhiteSpace ? hashNoWhitespace : hash;
		}

		#endregion

		#region Properties

		private string text = "";
		public string Text
		{
			get { return text; }
			set
			{
				text = value;
				TrimmedText = value.Trim();

				hash = value.GetHashCode();

				string textNoWhitespace = Regex.Replace(value, @"\s+", "");
				hashNoWhitespace = textNoWhitespace.GetHashCode();
				IsWhitespaceLine = textNoWhitespace == "";

				TextSegments.Clear();
				TextSegments.Add(new TextSegment(value, Type));

				OnPropertyChanged(nameof(Text));
			}
		}

		public string TrimmedText { get; set; }

		private ObservableCollection<TextSegment> textSegments = new ObservableCollection<TextSegment>();
		public ObservableCollection<TextSegment> TextSegments
		{
			get { return textSegments; }
			set { textSegments = value; OnPropertyChanged(nameof(TextSegments)); }
		}

		public bool IsWhitespaceLine { get; set; }

		public List<object> Characters
		{
			get
			{
				List<object> list = new List<object>();

				foreach (char c in text.ToCharArray())
				{
					list.Add(c);
				}
				return list;
			}
		}

		public List<object> TrimmedCharacters
		{
			get
			{
				List<object> list = new List<object>();

				foreach (char c in TrimmedText.ToCharArray())
				{
					list.Add(c);
				}
				return list;
			}
		}

		private int? lineindex;
		public int? LineIndex
		{
			get { return lineindex; }
			set { lineindex = value; OnPropertyChanged(nameof(LineIndex)); }
		}

		private TextState type;
		public TextState Type
		{
			get { return type; }
			set
			{
				type = value;
				TextSegments.Clear();
				TextSegments.Add(new TextSegment(Text, value));
				OnPropertyChanged(nameof(Type));
			}
		}

		public SolidColorBrush BackgroundBrush
		{
			get
			{
				switch (type)
				{
					case TextState.Deleted:
						return AppSettings.DeletedBackground;
					case TextState.New:
						return AppSettings.NewBackground;
					case TextState.PartialMatch:
						return AppSettings.PartialMatchBackground;

					default:
						return AppSettings.FullMatchBackground;
				}
			}
		}

		public SolidColorBrush ForegroundBrush
		{
			get
			{
				switch (type)
				{
					case TextState.Deleted:
						return AppSettings.DeletedForeground;
					case TextState.New:
						return AppSettings.NewForeground;
					case TextState.PartialMatch:
						return AppSettings.PartialMatchForeground;

					default:
						return AppSettings.FullMatchForeground;
				}
			}
		}

		public int? MatchingLineIndex { get; set; }

		#endregion

		#region Metods

		internal double CharacterPosition(int characterIndex)
		{
			double position = 0;
			int i = 0;

			foreach (TextSegment textSegment in TextSegments)
			{
				if (textSegment.RenderedText != null)
				{
					foreach (double x in textSegment.RenderedText.AdvanceWidths)
					{
						if (i++ == characterIndex)
						{
							return position;
						}
						position += x;
					}
				}
			}
			return position;
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
}
