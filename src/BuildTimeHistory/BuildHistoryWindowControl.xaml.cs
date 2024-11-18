using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms.DataVisualization.Charting;
using Newtonsoft.Json;
using UserControl = System.Windows.Controls.UserControl;

namespace BuildTimeHistory
{
	public partial class BuildHistoryWindowControl : UserControl
	{
		public BuildHistoryWindowControl()
		{
			this.InitializeComponent();

			this.Loaded += BuildHistoryWindowControl_Loaded;
		}

		private void BuildHistoryWindowControl_Loaded(object sender, RoutedEventArgs e)
		{
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
			var SuccedCount = "Succeded (Count)";
			var cancelTime = "Cancelled (Time)";
			var FailedTime = "Failed (Time)";
			var SuccedTime = "Succeded (Time)";
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
			timeChart.Series[cancelTime].YAxisType = AxisType.Secondary;

			timeChart.Series.Add(new Series(FailedTime));
			timeChart.Series[FailedTime].IsValueShownAsLabel = true;
			timeChart.Series[FailedTime].XValueType = ChartValueType.String;
			timeChart.Series[FailedTime].ChartType = SeriesChartType.StackedColumn;
			timeChart.Series[FailedTime].YAxisType = AxisType.Secondary;

			timeChart.Series.Add(new Series(SuccedTime));
			timeChart.Series[SuccedTime].IsValueShownAsLabel = true;
			timeChart.Series[SuccedTime].XValueType = ChartValueType.String;
			timeChart.Series[SuccedTime].ChartType = SeriesChartType.StackedColumn;
			timeChart.Series[SuccedTime].YAxisType = AxisType.Secondary;

			timeChart.Series.Add(new Series(TotalTime));
			timeChart.Series[TotalTime].IsValueShownAsLabel = true;
			timeChart.Series[TotalTime].XValueType = ChartValueType.String;
			timeChart.Series[TotalTime].ChartType = SeriesChartType.Line;
			timeChart.Series[TotalTime].Color = Color.Transparent;
			timeChart.Series[TotalTime].IsVisibleInLegend = false;
			timeChart.Series[TotalTime].YAxisType = AxisType.Secondary;

			for (int i = 20; i >= 0; i--)
			{
				var path = BuildTimeHistoryPackage.GetHistoryFilePath(i);

				var date = DateTime.Now.AddDays(-i).ToString("d MMM");

				if (File.Exists(path))
				{
					var data = JsonConvert.DeserializeObject<HistoryRecord>(File.ReadAllText(path));

					countChart.Series[cancelCount].Points.AddXY(date, data.CancelCount);
					countChart.Series[cancelCount].Points[countChart.Series[cancelCount].Points.Count - 1].IsValueShownAsLabel = data.CancelCount > 0;

					countChart.Series[FailedCount].Points.AddXY(date, data.FailCount);
					countChart.Series[FailedCount].Points[countChart.Series[FailedCount].Points.Count - 1].IsValueShownAsLabel = data.FailCount > 0;

					countChart.Series[SuccedCount].Points.AddXY(date, data.SuccessCount);
					countChart.Series[SuccedCount].Points[countChart.Series[SuccedCount].Points.Count - 1].IsValueShownAsLabel = data.SuccessCount > 0;

					timeChart.Series[cancelTime].Points.AddXY(date, data.CancelBuildTime);
					timeChart.Series[cancelTime].Points[timeChart.Series[cancelTime].Points.Count - 1].IsValueShownAsLabel = data.CancelCount > 0;

					timeChart.Series[FailedTime].Points.AddXY(date, data.FailBuildTime);
					timeChart.Series[FailedTime].Points[timeChart.Series[FailedTime].Points.Count - 1].IsValueShownAsLabel = data.FailCount > 0;

					timeChart.Series[SuccedTime].Points.AddXY(date, data.SuccessBuildTime);
					timeChart.Series[SuccedTime].Points[timeChart.Series[SuccedTime].Points.Count - 1].IsValueShownAsLabel = data.SuccessCount > 0;

					countChart.Series[TotalCount].Points.AddXY(date, data.TotalCount);

					timeChart.Series[TotalTime].Points.AddXY(date, data.CalculatedTotalBuildTime);
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

			countChart.Legends.Add(new Legend("leg End"));
			timeChart.Legends.Add(new Legend("leg End"));

			countChart.Dock = System.Windows.Forms.DockStyle.Fill;
			timeChart.Dock = System.Windows.Forms.DockStyle.Fill;

			//chart.Palette = ChartColorPalette.SeaGreen;
			//chart.Palette = ChartColorPalette.Grayscale;

			//  chart.Titles.Add("Time spent building");

			countChart.ChartAreas.Add("MainChartArea"); // There must be one
			timeChart.ChartAreas.Add("MainChartArea"); // There must be one

			countChart.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
			countChart.ChartAreas[0].AxisX.MinorGrid.Enabled = false;
			countChart.ChartAreas[0].AxisX.LabelStyle.Angle = 45;
			countChart.ChartAreas[0].AxisX.Interval = 1;

			countChart.ChartAreas[0].AxisY.Title = "Times tried to build";
			countChart.ChartAreas[0].AxisY.MajorGrid.Enabled = false;

			timeChart.ChartAreas[0].AxisY2.Title = "Time spent building";
			timeChart.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
			timeChart.ChartAreas[0].AxisY2.MinorGrid.Enabled = true;
			timeChart.ChartAreas[0].AxisY2.MinorGrid.LineDashStyle = ChartDashStyle.Dot;

			countHost.Child = countChart;
			timeHost.Child = timeChart;

			this.grid1.Children.Add(countHost);
			this.grid2.Children.Add(timeHost);
		}
	}
}
