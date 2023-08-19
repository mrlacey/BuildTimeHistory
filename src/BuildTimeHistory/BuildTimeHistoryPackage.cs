using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json;
using Task = System.Threading.Tasks.Task;

namespace BuildTimeHistory
{
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(BuildTimeHistoryPackage.PackageGuidString)]
    public sealed class BuildTimeHistoryPackage : AsyncPackage, IVsSolutionEvents, IVsUpdateSolutionEvents2
    {
        public const string PackageGuidString = "c0e7666d-0fc1-4e88-9c61-0468227a9922";

        private IVsSolution2 solution;
        private IVsSolutionBuildManager2 sbm;
        private uint updateSolutionEventsCookie;
        private uint solutionEventsCookie;

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            solution = ServiceProvider.GlobalProvider.GetService(typeof(SVsSolution)) as IVsSolution2;
            if (solution != null)
            {
                solution.AdviseSolutionEvents(this, out solutionEventsCookie);
            }

            sbm = ServiceProvider.GlobalProvider.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager2;
            sbm?.AdviseUpdateSolutionEvents(this, out updateSolutionEventsCookie);

            await OutputPane.Instance.WriteAsync($"{Vsix.Name} v{Vsix.Version}");

            await SponsorRequestHelper.CheckIfNeedToShowAsync();

            var (latestRecord, daysAgo) = await GetMostRecentDaysRecordAsync();

            if (daysAgo > int.MinValue && latestRecord.TotalCount > 0)
            {
                var sb = new StringBuilder();

                if (daysAgo == 0)
                {
                    sb.Append("Today's build summary: ");
                }
                else if (daysAgo == 1)
                {
                    sb.Append("Yesterday's build summary: ");
                }
                else
                {
                    sb.Append($"Build summary for {DateTime.Now.AddDays(daysAgo):dddd, MMMM d} : ");
                }

                sb.Append($"{latestRecord.TotalCount} builds ");

                if (latestRecord.HasMultipleDifferentResults)
                {
                    sb.Append($"({latestRecord.SuccessCount} successful, {latestRecord.FailCount} failed, {latestRecord.CancelCount} cancelled) ");
                }

                sb.AppendLine($"taking a total of {TimeSpan.FromMilliseconds(latestRecord.TotalBuildTime).Humanize()}");

                await OutputPane.Instance.WriteAsync(sb.ToString());
            }
            else
            {
                await OutputPane.Instance.WriteAsync("No previous history available.");
            }
        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        readonly Stopwatch _buildTimer = new Stopwatch();

        public int UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            _buildTimer.Restart();

            return VSConstants.S_OK;
        }

        public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            ProcessFinish(fSucceeded == 1, fCancelCommand == 1);

            return VSConstants.S_OK;
        }

        public int UpdateSolution_StartUpdate(ref int pfCancelUpdate)
        {
            return VSConstants.S_OK;
        }

        public int UpdateSolution_Cancel()
        {
            return VSConstants.S_OK;
        }

        public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int UpdateProjectCfg_Begin(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int UpdateProjectCfg_Done(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, int fSuccess, int fCancel)
        {
            return VSConstants.S_OK;
        }

        private void ProcessFinish(bool wasSuccessful, bool wasCancelled)
        {
            // If a build was started before the extension package loaded the time won't have been started.
            bool includeTimeInHistory = _buildTimer.IsRunning;

            // Stop the timer before switching thread
            _buildTimer.Stop();

#pragma warning disable VSTHRD102 // Implement internal logic asynchronously - can't make IVsUpdateSolutionEvents2 methods async
            ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                try
                {
                    var buildDuration = _buildTimer.ElapsedMilliseconds;

                    var todaysRecord = await GetTodaysRecordAsync();

                    var sb = new StringBuilder();

                    sb.Append($"{DateTime.Now.ToShortTimeString()}> ");

                    if (wasSuccessful)
                    {
                        sb.AppendLine($"Build completed successfully after {TimeSpan.FromMilliseconds(buildDuration).Humanize()}");
                        todaysRecord.SuccessCount++;
                    }
                    else
                    {
                        if (wasCancelled)
                        {
                            sb.AppendLine($"Build was cancelled after {TimeSpan.FromMilliseconds(buildDuration).Humanize()}");
                            todaysRecord.CancelCount++;
                        }
                        else
                        {
                            sb.AppendLine($"Build failed after {TimeSpan.FromMilliseconds(buildDuration).Humanize()}");
                            todaysRecord.FailCount++;
                        }
                    }

                    if (includeTimeInHistory)
                    {
                        todaysRecord.TotalBuildTime += buildDuration;
                    }
                    else
                    {
                        await OutputPane.Instance.WriteAsync("** Build time is unavailable and won't be added to the cumulative history.");
                    }

                    await SaveTodaysRecordAsync(todaysRecord);

                    sb.Append($"Today's build summary: ");
                    sb.Append($"{todaysRecord.TotalCount} build{(todaysRecord.TotalCount > 1 ? "s" : string.Empty)} ");

                    if (todaysRecord.HasMultipleDifferentResults)
                    {
                        sb.Append($"({todaysRecord.SuccessCount} successful, {todaysRecord.FailCount} failed, {todaysRecord.CancelCount} cancelled) ");
                    }

                    sb.AppendLine($"taking a total of {TimeSpan.FromMilliseconds(todaysRecord.TotalBuildTime).Humanize()}");

                    await OutputPane.Instance.WriteAsync(sb.ToString());
                }
                catch (Exception exc)
                {
                    System.Diagnostics.Debug.WriteLine(exc.Message);
                    await OutputPane.Instance.WriteAsync(exc.Message);
                }
            });
#pragma warning restore VSTHRD102 // Implement internal logic asynchronously
        }

        private async Task SaveTodaysRecordAsync(HistoryRecord todaysRecord)
        {
            try
            {
                var path = GetHistoryFilePath();

                var dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllText(path, JsonConvert.SerializeObject(todaysRecord));
            }
            catch (Exception exc)
            {
                await OutputPane.Instance.WriteAsync("Failed to load history file to save todays data");
                await OutputPane.Instance.WriteAsync(exc.Message);
                await OutputPane.Instance.WriteAsync(exc.Source);
                await OutputPane.Instance.WriteAsync(exc.StackTrace);
            }
        }

        private string GetHistoryFilePath(int daysPast = 0)
        {
            var dateOfInterest = DateTime.Now.AddDays(-daysPast);

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BuildTimerHistory",
                dateOfInterest.Year.ToString(),
                dateOfInterest.Month.ToString(),
                $"{dateOfInterest.Day}.data");
        }

        private async Task<(HistoryRecord, int)> GetMostRecentDaysRecordAsync()
        {
            try
            {
                // Quick easy approach of all possible history is only looking at the last 100 days
                for (int i = 0; i < 100; i++)
                {
                    var path = GetHistoryFilePath(i);

                    if (File.Exists(path))
                    {
                        return (JsonConvert.DeserializeObject<HistoryRecord>(File.ReadAllText(path)), i);
                    }
                }
            }
            catch (Exception exc)
            {
                await OutputPane.Instance.WriteAsync("Failed to load history file for today");
                await OutputPane.Instance.WriteAsync(exc.Message);
                await OutputPane.Instance.WriteAsync(exc.Source);
                await OutputPane.Instance.WriteAsync(exc.StackTrace);
            }

            return (HistoryRecord.CreateNew(), int.MinValue);
        }

        private async Task<HistoryRecord> GetTodaysRecordAsync()
        {
            try
            {
                var path = GetHistoryFilePath();

                if (File.Exists(path))
                {
                    return JsonConvert.DeserializeObject<HistoryRecord>(File.ReadAllText(path));
                }
            }
            catch (Exception exc)
            {
                await OutputPane.Instance.WriteAsync("Failed to load history file for today");
                await OutputPane.Instance.WriteAsync(exc.Message);
                await OutputPane.Instance.WriteAsync(exc.Source);
                await OutputPane.Instance.WriteAsync(exc.StackTrace);
            }

            return HistoryRecord.CreateNew();
        }
    }
}
