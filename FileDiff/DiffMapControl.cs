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
			double dpiScale = 1 / m.M11;

			double scrollableHeight = ActualHeight - (2 * SystemParameters.VerticalScrollBarButtonHeight);
			double lineHeight = scrollableHeight / Lines.Count;

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
					lineColor = AppSettings.partialMatchBackgroundBrush;
				}
				else if (line.Type == TextState.Deleted)
				{
					lineColor = AppSettings.deletedBackgroundBrush;
				}
				else if (line.Type == TextState.New)
				{
					lineColor = AppSettings.newBackgrounBrush;
				}
				else if (line.Type == TextState.Filler)
				{
					lineColor = AppSettings.deletedBackgroundBrush;
				}

				Rect rect = new Rect(Math.Round(1 / dpiScale) * dpiScale, (Math.Floor(i * lineHeight / dpiScale) * dpiScale) + SystemParameters.VerticalScrollBarButtonHeight, ActualWidth, Math.Ceiling(Math.Max(lineHeight, 1) / dpiScale) * dpiScale);

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

		#endregion

	}
}
