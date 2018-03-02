using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileDiff
{

	public class DiffMapControl : Control
	{

		#region Members

		private Pen transpatentPen;
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

			transpatentPen = new Pen(Brushes.Transparent, 0);
		}

		#endregion

		#region Overrides

		protected override void OnRender(DrawingContext drawingContext)
		{
			Debug.Print("DiffMap OnRender");

			drawingContext.DrawRectangle(AppSettings.fullMatchBackgroundBrush, transpatentPen, new Rect(0, 0, this.ActualWidth, this.ActualHeight));

			Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
			dpiScale = 1 / m.M11;

			double scrollableHeight = ActualHeight - (2 * RoundToPixels(SystemParameters.VerticalScrollBarButtonHeight));
			double lineHeight = scrollableHeight / Lines.Count;
			double drawHeight = Math.Ceiling(Math.Max((lineHeight), 1) / dpiScale) * dpiScale;

			SolidColorBrush lineColor = new SolidColorBrush();

			double lastHeight = -1;

			for (int i = 0; i < Lines.Count; i++)
			{
				Line line = Lines[i];

				if (line.Type == TextState.FullMatch)
				{
					continue;
				}
				else if (line.Type == TextState.PartialMatch)
				{
					lineColor = Blend(AppSettings.partialMatchBackgroundBrush, AppSettings.partialMatchForegroundBrush);
				}
				else if (line.Type == TextState.Deleted || line.Type == TextState.Filler)
				{
					lineColor = Blend(AppSettings.deletedBackgroundBrush, AppSettings.deletedForegroundBrush);
				}
				else if (line.Type == TextState.New)
				{
					lineColor = Blend(AppSettings.newBackgrounBrush, AppSettings.newForegroundBrush);
				}

				Rect rect = new Rect(RoundToPixels(1), (Math.Floor((i * lineHeight + SystemParameters.VerticalScrollBarButtonHeight) / dpiScale) * dpiScale), ActualWidth, drawHeight);

				if (rect.Bottom > lastHeight)
				{
					drawingContext.DrawRectangle(lineColor, transpatentPen, rect);

					lastHeight = rect.Bottom;
				}
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

		public static SolidColorBrush Blend(SolidColorBrush color, SolidColorBrush color2)
		{
			byte r = (byte)((color.Color.R + color2.Color.R) / 2);
			byte g = (byte)((color.Color.G + color2.Color.G) / 2);
			byte b = (byte)((color.Color.B + color2.Color.B) / 2);

			return new SolidColorBrush(Color.FromRgb(r, g, b));
		}

		private double RoundToPixels(double x)
		{
			return Math.Round(x / dpiScale) * dpiScale;
		}

		#endregion

	}
}
