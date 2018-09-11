using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FileDiff
{

	public class TreeControl : Control
	{
		private const double handleWidth = 4;
		private double itemHeight;
		private double dpiScale = 0;

		private List<FileItem> visibleItems = new List<FileItem>();

		#region Constructor

		static TreeControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeControl), new FrameworkPropertyMetadata(typeof(TreeControl)));
		}

		public TreeControl()
		{
			this.ClipToBounds = true;
		}

		#endregion

		#region Overrides

		protected override void OnRender(DrawingContext drawingContext)
		{
			Debug.Print("TreeControl OnRender");

			// Fill background
			drawingContext.DrawRectangle(AppSettings.FullMatchBackground, null, new Rect(0, 0, this.ActualWidth, this.ActualHeight));

			if (Lines.Count == 0)
				return;

			int itemMargin = 1;
			Pen selectionPen = new Pen(new SolidColorBrush(SystemColors.HighlightColor), itemMargin);

			Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
			dpiScale = 1 / m.M11;

			Typeface typeface = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);

			itemHeight = Math.Ceiling((MeasureString("W").Height + (2 * itemMargin)) / dpiScale) * dpiScale;

			visibleItems = new List<FileItem>();
			GetVisibleItems(Lines, visibleItems);

			VisibleLines = (int)(ActualHeight / itemHeight + 1);
			MaxVerialcalScroll = visibleItems.Count - VisibleLines + 1;
			VerticalOffset = Math.Min(VerticalOffset, MaxVerialcalScroll);

			for (int i = 0; i < VisibleLines; i++)
			{
				int lineIndex = i + VerticalOffset;

				if (lineIndex >= visibleItems.Count)
					break;

				FileItem line = visibleItems[lineIndex];

				if (line.Type == TextState.Filler)
					continue;

				drawingContext.PushTransform(new TranslateTransform(0, itemHeight * i));

				// Draw line background
				if (line.Type != TextState.FullMatch)
				{
					drawingContext.DrawRectangle(line.BackgroundBrush, null, new Rect(0, 0, Math.Max(this.ActualWidth, 0), itemHeight));
				}

				// Draw selection
				if (line == SelectedFile)
				{
					drawingContext.DrawRectangle(null, selectionPen, new Rect(.5, .5, Math.Max(this.ActualWidth - 1, 0), itemHeight - 1));
				}

				drawingContext.PushTransform(new TranslateTransform(-HorizontalOffset, 0));

				drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, AppSettings.NameColumnWidth, itemHeight)));

				// Draw folder expander
				if (line.IsFolder)
				{
					double expanderWidth = 5.5;
					drawingContext.DrawRectangle(Brushes.Transparent, new Pen(new SolidColorBrush(Colors.Black), 1), new Rect(expanderWidth + ((line.Level - 1) * itemHeight), expanderWidth, itemHeight - (expanderWidth * 2), itemHeight - (expanderWidth * 2)));
					if (line.Type != TextState.Ignored)
					{
						drawingContext.DrawLine(new Pen(new SolidColorBrush(Colors.Black), 1), new Point(expanderWidth + ((line.Level - 1) * itemHeight), itemHeight / 2), new Point(itemHeight - expanderWidth + ((line.Level - 1) * itemHeight), itemHeight / 2));
						if (!line.IsExpanded)
						{
							drawingContext.DrawLine(new Pen(new SolidColorBrush(Colors.Black), 1), new Point((itemHeight / 2) + ((line.Level - 1) * itemHeight), expanderWidth), new Point((itemHeight / 2) + ((line.Level - 1) * itemHeight), itemHeight - expanderWidth));
						}
					}
				}

				// Draw line text
				drawingContext.DrawText(new FormattedText(line.Name, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, this.FontSize, line.ForegroundBrush, null, TextFormattingMode.Display), new Point(line.Level * itemHeight, itemMargin));
				drawingContext.Pop();

				drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, AppSettings.NameColumnWidth + handleWidth + AppSettings.SizeColumnWidth, itemHeight)));
				drawingContext.DrawText(new FormattedText(line.Size, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, this.FontSize, line.ForegroundBrush, null, TextFormattingMode.Display), new Point(AppSettings.NameColumnWidth + handleWidth, itemMargin));
				drawingContext.Pop();

				drawingContext.DrawText(new FormattedText(line.Date, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, this.FontSize, line.ForegroundBrush, null, TextFormattingMode.Display), new Point(AppSettings.NameColumnWidth + AppSettings.SizeColumnWidth + (handleWidth * 2), itemMargin));

				drawingContext.Pop(); // Horizontal offset
				drawingContext.Pop(); // Line offset 
			}
		}

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			int lineIndex = (int)(e.GetPosition(this).Y / itemHeight) + VerticalOffset;

			if (lineIndex < visibleItems.Count)
			{
				// Item selected		
				if (e.GetPosition(this).X > (visibleItems[lineIndex].Level * itemHeight) - HorizontalOffset || !visibleItems[lineIndex].IsFolder)
				{
					if (visibleItems[lineIndex] != SelectedFile && visibleItems[lineIndex].Type != TextState.Filler)
					{
						SelectedFile = visibleItems[lineIndex];
						SelectionChanged?.Invoke(SelectedFile);
						UpdateTrigger++;
					}
				}

				// Folder expander clicked
				else if (e.LeftButton == MouseButtonState.Pressed && visibleItems[lineIndex].Type != TextState.Filler)
				{
					visibleItems[lineIndex].IsExpanded = !visibleItems[lineIndex].IsExpanded;

					visibleItems = new List<FileItem>();
					GetVisibleItems(Lines, visibleItems);
					UpdateTrigger++;
				}
			}
			this.Focus();
		}

		protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
		{
			int lineIndex = (int)(e.GetPosition(this).Y / itemHeight) + VerticalOffset;

			if (e.ChangedButton == MouseButton.Left && lineIndex < visibleItems.Count)
			{
				if (visibleItems[lineIndex].Type != TextState.Filler && visibleItems[lineIndex].CorrespondingItem.Type != TextState.Filler)
				{
					Process p = new Process();
					p.StartInfo.FileName = System.Reflection.Assembly.GetExecutingAssembly().Location;
					if (this.Name == "LeftFolder")
					{
						p.StartInfo.Arguments = $"\"{visibleItems[lineIndex].Path}\" \"{visibleItems[lineIndex].CorrespondingItem.Path}\"";
					}
					else
					{
						p.StartInfo.Arguments = $"\"{visibleItems[lineIndex].CorrespondingItem.Path}\" \"{visibleItems[lineIndex].Path}\"";
					}
					p.Start();
				}
			}
			base.OnMouseDoubleClick(e);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.Key == Key.PageUp)
			{
				VerticalOffset = Math.Max(0, VerticalOffset -= VisibleLines - 1);
			}
			else if (e.Key == Key.PageDown)
			{
				VerticalOffset = VerticalOffset += VisibleLines - 1;
			}
			else if (e.Key == Key.Home && Keyboard.Modifiers == ModifierKeys.Control)
			{
				VerticalOffset = 0;
			}
			else if (e.Key == Key.End && Keyboard.Modifiers == ModifierKeys.Control)
			{
				VerticalOffset = Lines.Count;
			}

			base.OnKeyDown(e);
		}

		#endregion

		#region Events

		public delegate void SelectionChangedEvent(FileItem file);
		public event SelectionChangedEvent SelectionChanged;

		#endregion

		#region Dependency Properties

		public static readonly DependencyProperty LinesProperty = DependencyProperty.Register("Lines", typeof(ObservableCollection<FileItem>), typeof(TreeControl), new FrameworkPropertyMetadata(new ObservableCollection<FileItem>(), FrameworkPropertyMetadataOptions.AffectsRender));

		public ObservableCollection<FileItem> Lines
		{
			get { return (ObservableCollection<FileItem>)GetValue(LinesProperty); }
			set { SetValue(LinesProperty, value); }
		}


		public static readonly DependencyProperty SelectedFileProperty = DependencyProperty.Register("SelectedFile", typeof(FileItem), typeof(TreeControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

		public FileItem SelectedFile
		{
			get { return (FileItem)GetValue(SelectedFileProperty); }
			set { SetValue(SelectedFileProperty, value); }
		}


		public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register("HorizontalOffset", typeof(int), typeof(TreeControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

		public int HorizontalOffset
		{
			get { return (int)GetValue(HorizontalOffsetProperty); }
			set { SetValue(HorizontalOffsetProperty, value); }
		}


		public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register("VerticalOffset", typeof(int), typeof(TreeControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

		public int VerticalOffset
		{
			get { return (int)GetValue(VerticalOffsetProperty); }
			set { SetValue(VerticalOffsetProperty, value); }
		}


		public static readonly DependencyProperty VisibleLinesProperty = DependencyProperty.Register("VisibleLines", typeof(int), typeof(TreeControl));

		public int VisibleLines
		{
			get { return (int)GetValue(VisibleLinesProperty); }
			set { SetValue(VisibleLinesProperty, value); }
		}


		public static readonly DependencyProperty MaxVerialcalScrollProperty = DependencyProperty.Register("MaxVerialcalScroll", typeof(int), typeof(TreeControl));

		public int MaxVerialcalScroll
		{
			get { return (int)GetValue(MaxVerialcalScrollProperty); }
			set { SetValue(MaxVerialcalScrollProperty, value); }
		}


		public static readonly DependencyProperty UpdateTriggerProperty = DependencyProperty.Register("UpdateTrigger", typeof(int), typeof(TreeControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

		public int UpdateTrigger
		{
			get { return (int)GetValue(UpdateTriggerProperty); }
			set { SetValue(UpdateTriggerProperty, value); }
		}

		#endregion

		#region Methods

		private void GetVisibleItems(ObservableCollection<FileItem> parent, List<FileItem> visibleItems)
		{
			foreach (FileItem fileItem in parent)
			{
				visibleItems.Add(fileItem);
				if (fileItem.IsFolder && fileItem.IsExpanded)
				{
					GetVisibleItems(fileItem.Children, visibleItems);
				}
			}
		}

		private Size MeasureString(string text)
		{
			FormattedText formattedText = new FormattedText(
				text,
				CultureInfo.CurrentCulture,
				FlowDirection.LeftToRight,
				new Typeface(this.FontFamily.ToString()),
				this.FontSize,
				Brushes.Black,
				new NumberSubstitution(),
				TextFormattingMode.Display);

			return new Size(formattedText.Width, formattedText.Height);
		}

		#endregion

	}
}
