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
            System.Windows.Forms.Integration.WindowsFormsHost host =
                new System.Windows.Forms.Integration.WindowsFormsHost();

            var chart = new System.Windows.Forms.DataVisualization.Charting.Chart();

            chart.Series.Clear();

            var cancelCount = "Cancelled (Count)";
            var FailedCount = "Failed (Count)";
            var SuccedCount = "Succeded (Count)";
            var cancelTime = "Cancelled (Time)";
            var FailedTime = "Failed (Time)";
            var SuccedTime = "Succeded (Time)";
            var TotalCount = "Total count";
            var TotalTime = "Total time";

            chart.Series.Add(new Series(cancelCount));
            chart.Series[cancelCount].IsValueShownAsLabel = true;
            chart.Series[cancelCount].XValueType = ChartValueType.String;
            chart.Series[cancelCount].ChartType = SeriesChartType.StackedColumn;

            chart.Series.Add(new Series(FailedCount));
            chart.Series[FailedCount].IsValueShownAsLabel = true;
            chart.Series[FailedCount].XValueType = ChartValueType.String;
            chart.Series[FailedCount].ChartType = SeriesChartType.StackedColumn;

            chart.Series.Add(new Series(SuccedCount));
            chart.Series[SuccedCount].IsValueShownAsLabel = true;
            chart.Series[SuccedCount].XValueType = ChartValueType.String;
            chart.Series[SuccedCount].ChartType = SeriesChartType.StackedColumn;

            chart.Series.Add(new Series(TotalCount));
            chart.Series[TotalCount].IsValueShownAsLabel = true;
            chart.Series[TotalCount].XValueType = ChartValueType.String;
            chart.Series[TotalCount].ChartType = SeriesChartType.Line;
            chart.Series[TotalCount].Color = Color.Transparent;
            chart.Series[TotalCount].IsVisibleInLegend = false;

            chart.Series.Add(new Series(cancelTime));
            chart.Series[cancelTime].IsValueShownAsLabel = true;
            chart.Series[cancelTime].XValueType = ChartValueType.String;
            chart.Series[cancelTime].ChartType = SeriesChartType.StackedColumn;
            chart.Series[cancelTime].YAxisType = AxisType.Secondary;

            chart.Series.Add(new Series(FailedTime));
            chart.Series[FailedTime].IsValueShownAsLabel = true;
            chart.Series[FailedTime].XValueType = ChartValueType.String;
            chart.Series[FailedTime].ChartType = SeriesChartType.StackedColumn;
            chart.Series[FailedTime].YAxisType = AxisType.Secondary;

            chart.Series.Add(new Series(SuccedTime));
            chart.Series[SuccedTime].IsValueShownAsLabel = true;
            chart.Series[SuccedTime].XValueType = ChartValueType.String;
            chart.Series[SuccedTime].ChartType = SeriesChartType.StackedColumn;
            chart.Series[SuccedTime].YAxisType = AxisType.Secondary;

            chart.Series.Add(new Series(TotalTime));
            chart.Series[TotalTime].IsValueShownAsLabel = true;
            chart.Series[TotalTime].XValueType = ChartValueType.String;
            chart.Series[TotalTime].ChartType = SeriesChartType.Line;
            chart.Series[TotalTime].Color = Color.Transparent;
            chart.Series[TotalTime].IsVisibleInLegend = false;
            chart.Series[TotalTime].YAxisType = AxisType.Secondary;

            for (int i = 20; i >= 0; i--)
            {
                var path = BuildTimeHistoryPackage.GetHistoryFilePath(i);

                var date = DateTime.Now.AddDays(-i).ToString("d MMM");

                if (File.Exists(path))
                {
                    var data = JsonConvert.DeserializeObject<HistoryRecord>(File.ReadAllText(path));

                    chart.Series[cancelCount].Points.AddXY(date, data.CancelCount);
                    chart.Series[cancelCount].Points[chart.Series[cancelCount].Points.Count - 1].IsValueShownAsLabel = data.CancelCount > 0;

                    chart.Series[FailedCount].Points.AddXY(date, data.FailCount);
                    chart.Series[FailedCount].Points[chart.Series[FailedCount].Points.Count - 1].IsValueShownAsLabel = data.FailCount > 0;

                    chart.Series[SuccedCount].Points.AddXY(date, data.SuccessCount);
                    chart.Series[SuccedCount].Points[chart.Series[SuccedCount].Points.Count - 1].IsValueShownAsLabel = data.SuccessCount > 0;

                    chart.Series[cancelTime].Points.AddXY(date, data.CancelBuildTime);
                    chart.Series[cancelTime].Points[chart.Series[cancelTime].Points.Count - 1].IsValueShownAsLabel = data.CancelCount > 0;

                    chart.Series[FailedTime].Points.AddXY(date, data.FailBuildTime);
                    chart.Series[FailedTime].Points[chart.Series[FailedTime].Points.Count - 1].IsValueShownAsLabel = data.FailCount > 0;

                    chart.Series[SuccedTime].Points.AddXY(date, data.SuccessBuildTime);
                    chart.Series[SuccedTime].Points[chart.Series[SuccedTime].Points.Count - 1].IsValueShownAsLabel = data.SuccessCount > 0;

                    chart.Series[TotalCount].Points.AddXY(date, data.TotalCount);
                    chart.Series[TotalTime].Points.AddXY(date, data.CalculatedTotalBuildTime);
                }
                else
                {
                    chart.Series[cancelCount].Points.AddXY(date, 0);
                    chart.Series[cancelCount].Points[chart.Series[cancelCount].Points.Count - 1].IsValueShownAsLabel = false;
                    chart.Series[FailedCount].Points.AddXY(date, 0);
                    chart.Series[FailedCount].Points[chart.Series[FailedCount].Points.Count - 1].IsValueShownAsLabel = false;
                    chart.Series[SuccedCount].Points.AddXY(date, 0);
                    chart.Series[SuccedCount].Points[chart.Series[SuccedCount].Points.Count - 1].IsValueShownAsLabel = false;

                    chart.Series[cancelTime].Points.AddXY(date, 0);
                    chart.Series[cancelTime].Points[chart.Series[cancelTime].Points.Count - 1].IsValueShownAsLabel = false;
                    chart.Series[FailedTime].Points.AddXY(date, 0);
                    chart.Series[FailedTime].Points[chart.Series[FailedTime].Points.Count - 1].IsValueShownAsLabel = false;
                    chart.Series[SuccedTime].Points.AddXY(date, 0);
                    chart.Series[SuccedTime].Points[chart.Series[SuccedTime].Points.Count - 1].IsValueShownAsLabel = false;

                    chart.Series[TotalCount].Points.AddXY(date, 0);

                    chart.Series[TotalTime].Points.AddXY(date, 0);
                    chart.Series[TotalTime].Points[chart.Series[TotalTime].Points.Count - 1].IsValueShownAsLabel = false;
                }
            }

            chart.Legends.Add(new Legend("leg End"));

            chart.Dock = System.Windows.Forms.DockStyle.Fill;

            //chart.Palette = ChartColorPalette.SeaGreen;
            //chart.Palette = ChartColorPalette.Grayscale;

            //  chart.Titles.Add("Time spent building");

            chart.ChartAreas.Add("MainChartArea"); // There must be one

            chart.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            chart.ChartAreas[0].AxisX.MinorGrid.Enabled = false;
            chart.ChartAreas[0].AxisX.LabelStyle.Angle = 45;
            chart.ChartAreas[0].AxisX.Interval = 1;

            chart.ChartAreas[0].AxisY.Title = "Times tried to build";
            chart.ChartAreas[0].AxisY.MajorGrid.Enabled = false;

            chart.ChartAreas[0].AxisY2.Title = "Time spent building";
            chart.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
            chart.ChartAreas[0].AxisY2.MinorGrid.Enabled = true;
            chart.ChartAreas[0].AxisY2.MinorGrid.LineDashStyle = ChartDashStyle.Dot;

            host.Child = chart;

            this.grid1.Children.Add(host);
        }
    }
}