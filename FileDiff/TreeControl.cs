using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FileDiff;

public class TreeControl : Control
{

	#region Members

	private double itemHeight;
	private double dpiScale = 0;

	private List<FileItem> visibleItems = new List<FileItem>();

	#endregion

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
		drawingContext.DrawRectangle(AppSettings.FolderFullMatchBackground, null, new Rect(0, 0, this.ActualWidth, this.ActualHeight));

		if (Lines.Count == 0)
			return;

		Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
		dpiScale = 1 / m.M11;

		double itemMargin = 1 * dpiScale;

		itemHeight = Math.Ceiling(MeasureString("W").Height / dpiScale) * dpiScale + (2 * itemMargin);

		double textMargin = RoundToWholePixels(3);
		double expanderMargin = Math.Floor(itemHeight * .25);
		double handleWidth = RoundToWholePixels(4);

		double gridLine1 = RoundToWholePixels(AppSettings.NameColumnWidth + handleWidth - itemMargin);
		double gridLine2 = RoundToWholePixels(AppSettings.NameColumnWidth + AppSettings.SizeColumnWidth + (handleWidth * 2) - itemMargin);
		double gridLine3 = RoundToWholePixels(AppSettings.NameColumnWidth + AppSettings.SizeColumnWidth + AppSettings.DateColumnWidth + (handleWidth * 3) - itemMargin);

		Typeface typeface = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);

		Pen selectionPen = new Pen(AppSettings.HighlightBorder, itemMargin);
		selectionPen.Freeze();
		GuidelineSet selectionGuide = CreateGuidelineSet(selectionPen);

		Pen expanderPen = new Pen(new SolidColorBrush(AppSettings.FolderFullMatchForeground.Color), 1 * dpiScale);
		expanderPen.Freeze();
		GuidelineSet expanderGuide = CreateGuidelineSet(expanderPen);

		UpdateVisibleItems();

		VisibleLines = (int)(ActualHeight / itemHeight + 1);
		MaxVerticalScroll = visibleItems.Count - VisibleLines + 1;
		VerticalOffset = Math.Min(VerticalOffset, MaxVerticalScroll);


		for (int i = 0; i < VisibleLines; i++)
		{
			int lineIndex = i + VerticalOffset;

			if (lineIndex >= visibleItems.Count)
				break;

			FileItem line = visibleItems[lineIndex];


			// Line Y offset
			drawingContext.PushTransform(new TranslateTransform(0, itemHeight * i));
			{
				if (line.Type != TextState.Filler)
				{
					// Draw line background
					if (line.Type != TextState.FullMatch)
					{
						drawingContext.DrawRectangle(line.BackgroundBrush, null, new Rect(0, 0, Math.Max(this.ActualWidth, 0), itemHeight));
					}

					// Horizontal offset
					drawingContext.PushTransform(new TranslateTransform(-HorizontalOffset, 0));
					{

						// Name column
						drawingContext.PushClip(new RectangleGeometry(new Rect(textMargin, 0, AppSettings.NameColumnWidth - textMargin * 2, itemHeight)));
						{

							// Draw folder expander
							if (line.IsFolder)
							{
								drawingContext.PushGuidelineSet(expanderGuide);
								{
									Rect expanderRect = new Rect(expanderMargin + ((line.Level - 1) * itemHeight), expanderMargin, expanderMargin * 2, expanderMargin * 2);

									drawingContext.DrawRectangle(Brushes.Transparent, expanderPen, expanderRect);
									if (line.Type != TextState.Ignored)
									{
										drawingContext.DrawLine(expanderPen, new Point(expanderRect.Left, expanderMargin * 2), new Point(expanderRect.Right, expanderMargin * 2));
										if (!line.IsExpanded)
										{
											drawingContext.DrawLine(expanderPen, new Point(expanderRect.Left + expanderMargin, expanderMargin), new Point(expanderRect.Left + expanderMargin, expanderMargin * 3));
										}
									}
								}
								drawingContext.Pop();
							}
							drawingContext.DrawText(new FormattedText(line.Name, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, this.FontSize, line.ForegroundBrush, null, TextFormattingMode.Display, dpiScale), new Point(line.Level * itemHeight, itemMargin));
						}
						drawingContext.Pop();

						// Size column
						if (!line.IsFolder)
						{
							drawingContext.PushClip(new RectangleGeometry(new Rect(gridLine1 + textMargin, 0, AppSettings.SizeColumnWidth - textMargin * 2, itemHeight)));
							{
								string sizeText = line.Size.ToString("N0");
								drawingContext.DrawText(new FormattedText(sizeText, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, this.FontSize, line.ForegroundBrush, null, TextFormattingMode.Display, dpiScale), new Point(gridLine1 + AppSettings.SizeColumnWidth - textMargin - MeasureString(sizeText).Width - 1, itemMargin));
							}
							drawingContext.Pop();
						}

						// Date column
						drawingContext.PushClip(new RectangleGeometry(new Rect(gridLine2 + textMargin, 0, AppSettings.DateColumnWidth - textMargin * 2, itemHeight)));
						{
							drawingContext.DrawText(new FormattedText(line.Date.ToString("g"), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, this.FontSize, line.ForegroundBrush, null, TextFormattingMode.Display, dpiScale), new Point(gridLine2 + textMargin, itemMargin));
						}
						drawingContext.Pop();

					}
					drawingContext.Pop(); // Horizontal offset
				}

				// Draw selection
				if (line == SelectedFile)
				{
					drawingContext.DrawRectangle(AppSettings.SelectionBackground, null, new Rect(0, 0, this.ActualWidth, itemHeight));
					drawingContext.PushGuidelineSet(selectionGuide);
					{
						drawingContext.DrawLine(selectionPen, new Point(-1, 0), new Point(this.ActualWidth, 0));
						drawingContext.DrawLine(selectionPen, new Point(-1, itemHeight - 1 * dpiScale), new Point(this.ActualWidth, itemHeight - 1 * dpiScale));
					}
					drawingContext.Pop();
				}
			}
			drawingContext.Pop(); // Line Y offset 
		}

		// Draw grid lines
		if (AppSettings.DrawGridLines)
		{
			Pen gridPen = new Pen(new SolidColorBrush(Color.FromArgb(20, 0, 0, 0)), RoundToWholePixels(1));
			gridPen.Freeze();
			GuidelineSet gridGuide = CreateGuidelineSet(gridPen);

			drawingContext.PushTransform(new TranslateTransform(-HorizontalOffset, 0));
			{
				drawingContext.PushGuidelineSet(gridGuide);
				{
					drawingContext.DrawLine(gridPen, new Point(gridLine1, -1), new Point(gridLine1, this.ActualHeight));
					drawingContext.DrawLine(gridPen, new Point(gridLine2, -1), new Point(gridLine2, this.ActualHeight));
					drawingContext.DrawLine(gridPen, new Point(gridLine3, -1), new Point(gridLine3, this.ActualHeight));
				}
			}
			drawingContext.Pop();
		}
	}

	protected override void OnMouseDown(MouseButtonEventArgs e)
	{
		int lineIndex = (int)(e.GetPosition(this).Y / itemHeight) + VerticalOffset;

		if (lineIndex < visibleItems.Count && Lines.Count > 0)
		{
			// Item selected		
			if (e.GetPosition(this).X > (visibleItems[lineIndex].Level * itemHeight) - HorizontalOffset || !visibleItems[lineIndex].IsFolder)
			{
				Select(visibleItems[lineIndex]);
			}

			// Folder expander clicked
			else if (e.LeftButton == MouseButtonState.Pressed && visibleItems[lineIndex].Type != TextState.Filler)
			{
				visibleItems[lineIndex].IsExpanded = !visibleItems[lineIndex].IsExpanded;

				UpdateVisibleItems();
				UpdateTrigger++;
			}
		}
		this.Focus();
	}

	protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
	{
		int lineIndex = (int)(e.GetPosition(this).Y / itemHeight) + VerticalOffset;

		if (e.ChangedButton == MouseButton.Left && lineIndex < visibleItems.Count && Lines.Count > 0)
		{
			if (visibleItems[lineIndex].Type != TextState.Filler && visibleItems[lineIndex].CorrespondingItem.Type != TextState.Filler)
			{
				using Process p = new Process();

				p.StartInfo.FileName = Environment.ProcessPath;

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


	public static readonly DependencyProperty MaxVerticalScrollProperty = DependencyProperty.Register("MaxVerticalScroll", typeof(int), typeof(TreeControl));

	public int MaxVerticalScroll
	{
		get { return (int)GetValue(MaxVerticalScrollProperty); }
		set { SetValue(MaxVerticalScrollProperty, value); }
	}


	public static readonly DependencyProperty UpdateTriggerProperty = DependencyProperty.Register("UpdateTrigger", typeof(int), typeof(TreeControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

	public int UpdateTrigger
	{
		get { return (int)GetValue(UpdateTriggerProperty); }
		set { SetValue(UpdateTriggerProperty, value); }
	}

	#endregion

	#region Methods

	internal void Init()
	{
		VerticalOffset = 0;
		HorizontalOffset = 0;
	}

	public void Select(FileItem item)
	{
		ExpandParents(item);
		UpdateVisibleItems();
		MaxVerticalScroll = visibleItems.Count - VisibleLines + 1;
		MoveItemIntoView(item);
		SelectedFile = item;
		SelectionChanged?.Invoke(SelectedFile);
		UpdateTrigger++;
	}

	private static GuidelineSet CreateGuidelineSet(Pen pen)
	{
		GuidelineSet guidelineSet = new GuidelineSet();
		guidelineSet.GuidelinesX.Add(pen.Thickness / 2);
		guidelineSet.GuidelinesY.Add(pen.Thickness / 2);
		guidelineSet.Freeze();

		return guidelineSet;
	}

	private void UpdateVisibleItems()
	{
		void FindVisibleItems(ObservableCollection<FileItem> parent, List<FileItem> items)
		{
			foreach (FileItem fileItem in parent)
			{
				items.Add(fileItem);
				if (fileItem.IsFolder && fileItem.IsExpanded)
				{
					FindVisibleItems(fileItem.Children, items);
				}
			}
		}

		visibleItems = new List<FileItem>();
		FindVisibleItems(Lines, visibleItems);
	}

	private void MoveItemIntoView(FileItem item)
	{
		int itemIndex = visibleItems.IndexOf(item);
		if (itemIndex < VerticalOffset)
		{
			VerticalOffset = itemIndex;
		}
		else if (itemIndex > VerticalOffset + VisibleLines - 2)
		{
			VerticalOffset = itemIndex - (VisibleLines - 2);
		}
	}

	private void ExpandParents(FileItem item)
	{
		if (item.Parent != null)
		{
			ExpandParents(item.Parent);
			item.Parent.IsExpanded = true;
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
			TextFormattingMode.Display,
			dpiScale);

		return new Size(formattedText.Width, formattedText.Height);
	}

	private double RoundToWholePixels(double x)
	{
		return Math.Round(x / dpiScale) * dpiScale;
	}

	#endregion

}
