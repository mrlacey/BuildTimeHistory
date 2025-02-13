using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms.DataVisualization.Charting;
using Humanizer;
using Newtonsoft.Json;
using UserControl = System.Windows.Controls.UserControl;

namespace BuildTimeHistory
{
	// TODO: It would be good to have a nice way to test this. 🤷
	public partial class BuildHistoryWindowControl : UserControl
	{
		private const double MilliSecondsPerMinute = 60000;

		public BuildHistoryWindowControl()
		{
			this.InitializeComponent();

			this.Loaded += BuildHistoryWindowControl_Loaded;
		}

#pragma warning disable VSTHRD100 // Avoid async void methods
		private async void BuildHistoryWindowControl_Loaded(object sender, RoutedEventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
		{
			if (await SponsorDetector.IsSponsorAsync())
			{
				this.SponsorshipPrompt.Visibility = Visibility.Collapsed;
			}

			System.Windows.Forms.Integration.WindowsFormsHost countHost =
				new System.Windows.Forms.Integration.WindowsFormsHost();

			System.Windows.Forms.Integration.WindowsFormsHost timeHost =
				new System.Windows.Forms.Integration.WindowsFormsHost();

			var countChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
			var timeChart = new System.Windows.Forms.DataVisualization.Charting.Chart();

			countChart.Series.Clear();
			timeChart.Series.Clear();

			var cancelCount = "Cancelled (Count)";
			var FailedCount = "Failed (Count)";
			var SuccedCount = "Succeeded (Count)";
			var cancelTime = "Cancelled (Time)";
			var FailedTime = "Failed (Time)";
			var SuccedTime = "Succeeded (Time)";
			var TotalCount = "Total count";
			var TotalTime = "Total time";

			countChart.Series.Add(new Series(cancelCount));
			countChart.Series[cancelCount].IsValueShownAsLabel = true;
			countChart.Series[cancelCount].XValueType = ChartValueType.String;
			countChart.Series[cancelCount].ChartType = SeriesChartType.StackedColumn;

			countChart.Series.Add(new Series(FailedCount));
			countChart.Series[FailedCount].IsValueShownAsLabel = true;
			countChart.Series[FailedCount].XValueType = ChartValueType.String;
			countChart.Series[FailedCount].ChartType = SeriesChartType.StackedColumn;

			countChart.Series.Add(new Series(SuccedCount));
			countChart.Series[SuccedCount].IsValueShownAsLabel = true;
			countChart.Series[SuccedCount].XValueType = ChartValueType.String;
			countChart.Series[SuccedCount].ChartType = SeriesChartType.StackedColumn;

			// TODO: Review how to show totals so the labels always appear above the stacked other data
			// By using a line, the label may be lower than the point if points either side are higher
			countChart.Series.Add(new Series(TotalCount));
			countChart.Series[TotalCount].IsValueShownAsLabel = true;
			countChart.Series[TotalCount].XValueType = ChartValueType.String;
			countChart.Series[TotalCount].ChartType = SeriesChartType.Line;
			countChart.Series[TotalCount].Color = Color.Transparent;
			countChart.Series[TotalCount].IsVisibleInLegend = false;

			timeChart.Series.Add(new Series(cancelTime));
			timeChart.Series[cancelTime].IsValueShownAsLabel = true;
			timeChart.Series[cancelTime].XValueType = ChartValueType.String;
			timeChart.Series[cancelTime].ChartType = SeriesChartType.StackedColumn;
			//timeChart.Series[cancelTime].YAxisType = AxisType.Secondary;

			timeChart.Series.Add(new Series(FailedTime));
			timeChart.Series[FailedTime].IsValueShownAsLabel = true;
			timeChart.Series[FailedTime].XValueType = ChartValueType.String;
			timeChart.Series[FailedTime].ChartType = SeriesChartType.StackedColumn;
			//timeChart.Series[FailedTime].YAxisType = AxisType.Secondary;

			timeChart.Series.Add(new Series(SuccedTime));
			timeChart.Series[SuccedTime].IsValueShownAsLabel = true;
			timeChart.Series[SuccedTime].XValueType = ChartValueType.String;
			timeChart.Series[SuccedTime].ChartType = SeriesChartType.StackedColumn;
			//timeChart.Series[SuccedTime].YAxisType = AxisType.Secondary;

			timeChart.Series.Add(new Series(TotalTime));
			timeChart.Series[TotalTime].IsValueShownAsLabel = true;
			timeChart.Series[TotalTime].XValueType = ChartValueType.String;
			timeChart.Series[TotalTime].ChartType = SeriesChartType.Line;
			timeChart.Series[TotalTime].Color = Color.Transparent;
			timeChart.Series[TotalTime].IsVisibleInLegend = false;
			//timeChart.Series[TotalTime].YAxisType = AxisType.Secondary;

			double maxTime = 0;

			// TODO: Test with no data
			for (int i = 20; i >= 0; i--)
			{
				var path = BuildTimeHistoryPackage.GetHistoryFilePath(i);

				var date = DateTime.Now.AddDays(-i).ToString("d MMM");

				if (File.Exists(path))
				{
					var data = JsonConvert.DeserializeObject<HistoryRecord>(await AsyncFileIo.ReadAllTextAsync(path));

					countChart.Series[cancelCount].Points.AddXY(date, data.CancelCount);
					countChart.Series[cancelCount].Points[countChart.Series[cancelCount].Points.Count - 1].IsValueShownAsLabel = data.CancelCount > 0 && data.CancelCount != data.TotalCount;

					countChart.Series[FailedCount].Points.AddXY(date, data.FailCount);
					countChart.Series[FailedCount].Points[countChart.Series[FailedCount].Points.Count - 1].IsValueShownAsLabel = data.FailCount > 0 && data.FailCount != data.TotalCount;

					countChart.Series[SuccedCount].Points.AddXY(date, data.SuccessCount);
					countChart.Series[SuccedCount].Points[countChart.Series[SuccedCount].Points.Count - 1].IsValueShownAsLabel = data.SuccessCount > 0 && data.SuccessCount != data.TotalCount;

					// TODO: Be smarter about including point labels based on the relative space available (so they don't overlap)
					timeChart.Series[cancelTime].Points.AddXY(date, data.CancelBuildTime / MilliSecondsPerMinute);
					//timeChart.Series[cancelTime].Points[timeChart.Series[cancelTime].Points.Count - 1].IsValueShownAsLabel = data.CancelCount > 0;

					if (data.CancelBuildTime > 0 && data.CancelBuildTime != data.CalculatedTotalBuildTime)
					{
						timeChart.Series[cancelTime].Points[timeChart.Series[cancelTime].Points.Count - 1].Label = data.CancelBuildTime.ToSmartTimeString();
					}
					else
					{
						timeChart.Series[cancelTime].Points[timeChart.Series[cancelTime].Points.Count - 1].IsValueShownAsLabel = false;
					}

					timeChart.Series[FailedTime].Points.AddXY(date, data.FailBuildTime / MilliSecondsPerMinute);
					//timeChart.Series[FailedTime].Points[timeChart.Series[FailedTime].Points.Count - 1].IsValueShownAsLabel = data.FailCount > 0;

					if (data.FailBuildTime > 0 && data.FailBuildTime != data.CalculatedTotalBuildTime)
					{
						timeChart.Series[FailedTime].Points[timeChart.Series[FailedTime].Points.Count - 1].Label = data.FailBuildTime.ToSmartTimeString();
					}
					else
					{
						timeChart.Series[FailedTime].Points[timeChart.Series[FailedTime].Points.Count - 1].IsValueShownAsLabel = false;
					}

					timeChart.Series[SuccedTime].Points.AddXY(date, data.SuccessBuildTime / MilliSecondsPerMinute);
					//timeChart.Series[SuccedTime].Points[timeChart.Series[SuccedTime].Points.Count - 1].IsValueShownAsLabel = data.SuccessCount > 0;

					if (data.SuccessBuildTime > 0 && data.SuccessBuildTime != data.CalculatedTotalBuildTime)
					{
						timeChart.Series[SuccedTime].Points[timeChart.Series[SuccedTime].Points.Count - 1].Label = data.SuccessBuildTime.ToSmartTimeString();
					}
					else
					{
						timeChart.Series[SuccedTime].Points[timeChart.Series[SuccedTime].Points.Count - 1].IsValueShownAsLabel = false;
					}

					countChart.Series[TotalCount].Points.AddXY(date, data.TotalCount);
					countChart.Series[TotalCount].Points[countChart.Series[TotalCount].Points.Count - 1].Label = $"{data.TotalCount}";

					if (data.CalculatedTotalBuildTime > maxTime)
					{
						maxTime = data.CalculatedTotalBuildTime;
					}

					timeChart.Series[TotalTime].Points.AddXY(date, data.CalculatedTotalBuildTime / MilliSecondsPerMinute);
					timeChart.Series[TotalTime].Points[timeChart.Series[TotalTime].Points.Count - 1].Label = $"{data.CalculatedTotalBuildTime.ToSmartTimeString()}";
				}
				else
				{
					countChart.Series[cancelCount].Points.AddXY(date, 0);
					countChart.Series[cancelCount].Points[countChart.Series[cancelCount].Points.Count - 1].IsValueShownAsLabel = false;
					countChart.Series[FailedCount].Points.AddXY(date, 0);
					countChart.Series[FailedCount].Points[countChart.Series[FailedCount].Points.Count - 1].IsValueShownAsLabel = false;
					countChart.Series[SuccedCount].Points.AddXY(date, 0);
					countChart.Series[SuccedCount].Points[countChart.Series[SuccedCount].Points.Count - 1].IsValueShownAsLabel = false;

					timeChart.Series[cancelTime].Points.AddXY(date, 0);
					timeChart.Series[cancelTime].Points[timeChart.Series[cancelTime].Points.Count - 1].IsValueShownAsLabel = false;
					timeChart.Series[FailedTime].Points.AddXY(date, 0);
					timeChart.Series[FailedTime].Points[timeChart.Series[FailedTime].Points.Count - 1].IsValueShownAsLabel = false;
					timeChart.Series[SuccedTime].Points.AddXY(date, 0);
					timeChart.Series[SuccedTime].Points[timeChart.Series[SuccedTime].Points.Count - 1].IsValueShownAsLabel = false;

					countChart.Series[TotalCount].Points.AddXY(date, 0);

					timeChart.Series[TotalTime].Points.AddXY(date, 0);
					timeChart.Series[TotalTime].Points[timeChart.Series[TotalTime].Points.Count - 1].IsValueShownAsLabel = false;
				}
			}

			countChart.Legends.Add(new Legend("count legend"));
			timeChart.Legends.Add(new Legend("timing legend"));

			countChart.Dock = System.Windows.Forms.DockStyle.Fill;
			timeChart.Dock = System.Windows.Forms.DockStyle.Fill;

			//chart.Palette = ChartColorPalette.SeaGreen;
			//chart.Palette = ChartColorPalette.Grayscale;

			//  chart.Titles.Add("Time spent building");

			countChart.ChartAreas.Add("CountChartArea"); // There must be one
			timeChart.ChartAreas.Add("TimeChartArea"); // There must be one

			countChart.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
			countChart.ChartAreas[0].AxisX.MinorGrid.Enabled = false;
			countChart.ChartAreas[0].AxisX.LabelStyle.Angle = 45;
			countChart.ChartAreas[0].AxisX.Interval = 1;

			countChart.ChartAreas[0].AxisY.Title = "Times tried to build";
			countChart.ChartAreas[0].AxisY.MajorGrid.Enabled = false;

			// TODO: Make the time title smarter about whether showing mins, secs, etc.
			var timeClassifier = " (mins)";
			//var timeClassifier = " (seconds)";

			//if (TimeSpan.FromMilliseconds(maxTime).TotalHours >= 1)
			//{
			//	timeClassifier = " (hours)";
			//}
			//else if(TimeSpan.FromMilliseconds(maxTime).TotalMinutes >= 1)
			//{
			//	timeClassifier = " (mins)";
			//}

			timeChart.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
			timeChart.ChartAreas[0].AxisX.MinorGrid.Enabled = false;
			timeChart.ChartAreas[0].AxisX.LabelStyle.Angle = 45;
			timeChart.ChartAreas[0].AxisX.Interval = 1;

			//timeChart.ChartAreas[0].AxisY.Enabled = AxisEnabled.True;
			//timeChart.ChartAreas[0].AxisY.MinorGrid.Enabled = true;
			//timeChart.ChartAreas[0].AxisY.MinorGrid.LineDashStyle = ChartDashStyle.Dot;

			timeChart.ChartAreas[0].AxisY.Title = "Time spent building " + timeClassifier;
			timeChart.ChartAreas[0].AxisY.MajorGrid.Enabled = false;

			countHost.Child = countChart;
			timeHost.Child = timeChart;

			this.grid1.Children.Add(countHost);
			this.grid2.Children.Add(timeHost);
		}

		private void JoinThemClicked(object sender, RoutedEventArgs e)
		{
			System.Diagnostics.Process.Start("https://github.com/sponsors/mrlacey");
		}
	}
}
