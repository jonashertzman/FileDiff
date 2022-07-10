using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileDiff;

public class DiffMapControl : Control
{

	#region Members

	private double dpiScale = 0;

	#endregion

	#region Constructors

	static DiffMapControl()
	{
		DefaultStyleKeyProperty.OverrideMetadata(typeof(DiffMapControl), new FrameworkPropertyMetadata(typeof(DiffMapControl)));
	}

	public DiffMapControl()
	{
		this.ClipToBounds = true;
	}

	#endregion

	#region Overrides

	protected override void OnRender(DrawingContext drawingContext)
	{
		Debug.Print("DiffMap OnRender");

		// Fill background
		drawingContext.DrawRectangle(AppSettings.ControlBackground, null, new Rect(0, 0, this.ActualWidth, this.ActualHeight));

		if (Lines.Count == 0)
			return;

		Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
		dpiScale = 1 / m.M11;

		double scrollableHeight = ActualHeight - (2 * RoundToWholePixels(SystemParameters.VerticalScrollBarButtonHeight));
		double lineHeight = scrollableHeight / Lines.Count;
		double lastHeight = -1;

		SolidColorBrush partialMatchBrush = BlendColors(AppSettings.PartialMatchBackground, AppSettings.PartialMatchForeground, .7);

		SolidColorBrush deletedBrush = BlendColors(AppSettings.DeletedBackground, AppSettings.DeletedForeground, .7);
		SolidColorBrush movedFromBrush = BlendColors(AppSettings.MovedFromdBackground, AppSettings.DeletedForeground, .7);

		SolidColorBrush newBrush = BlendColors(AppSettings.NewBackground, AppSettings.NewForeground, .7);
		SolidColorBrush movedToBrush = BlendColors(AppSettings.MovedToBackground, AppSettings.NewForeground, .7);

		SolidColorBrush lineBrush;

		for (int i = 0; i < Lines.Count; i++)
		{
			Line line = Lines[i];

			switch (line.Type)
			{
				case TextState.PartialMatch:
					lineBrush = partialMatchBrush;
					break;

				case TextState.New:
					lineBrush = newBrush;
					break;

				case TextState.MovedTo:
					lineBrush = movedToBrush;
					break;

				case TextState.Filler:
					lineBrush = deletedBrush;
					break;

				case TextState.MovedFiller:
					lineBrush = movedFromBrush;
					break;

				default:
					continue;
			}

			int sectionLength = 1;

			while (i + sectionLength < Lines.Count && line.Type == Lines[i + sectionLength].Type)
			{
				sectionLength++;
			}

			Rect rect = new Rect(RoundToWholePixels(1), (Math.Floor((i * lineHeight + SystemParameters.VerticalScrollBarButtonHeight) / dpiScale) * dpiScale), ActualWidth - RoundToWholePixels(2), Math.Ceiling(Math.Max((lineHeight * sectionLength), 1) / dpiScale) * dpiScale);

			if (rect.Bottom > lastHeight)
			{
				drawingContext.DrawRectangle(lineBrush, null, rect);

				lastHeight = rect.Bottom;
			}

			i += sectionLength - 1;
		}
	}

	#endregion

	#region Dependency Properties

	public static readonly DependencyProperty LinesProperty = DependencyProperty.Register("Lines", typeof(ObservableCollection<Line>), typeof(DiffMapControl), new FrameworkPropertyMetadata(new ObservableCollection<Line>(), FrameworkPropertyMetadataOptions.AffectsRender));

	public ObservableCollection<Line> Lines
	{
		get { return (ObservableCollection<Line>)GetValue(LinesProperty); }
		set { SetValue(LinesProperty, value); }
	}


	public static readonly DependencyProperty UpdateTriggerProperty = DependencyProperty.Register("UpdateTrigger", typeof(int), typeof(DiffMapControl), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

	public int UpdateTrigger
	{
		get { return (int)GetValue(UpdateTriggerProperty); }
		set { SetValue(UpdateTriggerProperty, value); }
	}

	#endregion

	#region Methods

	private SolidColorBrush BlendColors(SolidColorBrush brush1, SolidColorBrush brush2, double blendFactor)
	{
		byte r = (byte)((brush1.Color.R * blendFactor) + brush2.Color.R * (1 - blendFactor));
		byte g = (byte)((brush1.Color.G * blendFactor) + brush2.Color.G * (1 - blendFactor));
		byte b = (byte)((brush1.Color.B * blendFactor) + brush2.Color.B * (1 - blendFactor));

		SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(r, g, b));
		brush.Freeze();

		return brush;
	}

	private double RoundToWholePixels(double x)
	{
		return Math.Round(x / dpiScale) * dpiScale;
	}

	#endregion

}
