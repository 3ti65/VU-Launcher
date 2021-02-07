using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using VULauncher.Commands;
using VULauncher.Models.Config;
using VULauncher.Models.Repositories.ServerFilesManagers;
using VULauncher.ViewModels.Common;
using VULauncher.ViewModels.ConsoleViewModels;
using VULauncher.ViewModels.Enums;
using VULauncher.ViewModels.Items;
using VULauncher.ViewModels.Items.Common;
using VULauncher.ViewModels.Items.Extensions;

namespace VULauncher.ViewModels
{
    public class ConsolesViewModel : ViewModel
    {
        private static readonly string _vuExe = "vu.exe";
        private static readonly string _vuCom = "vu.com";
        private static readonly string _vuClient = "Client";
        private static readonly string _vuServer = "Server";
        private VuConsoleViewModel _activeConsoleViewModel;

        public ObservableCollection<DockableDocumentViewModel> DockingViewModels { get; set; } = new ObservableCollection<DockableDocumentViewModel>();

        public bool IsClientRunning => DockingViewModels.Any(d => (d as VuConsoleViewModel)?.StartupType == StartupType.Client);
        public bool IsServerRunning => DockingViewModels.Any(d => (d as VuConsoleViewModel)?.StartupType == StartupType.Server);

        public VuConsoleViewModel ActiveConsoleViewModel // TODO: not used. is this needed for any docking stuff?
        {
            get => _activeConsoleViewModel;
            set => SetField(ref _activeConsoleViewModel, value);
        }

        public void Add(VuConsoleViewModel vuConsoleViewModel)
        {
            if (vuConsoleViewModel != null)
            {
                DockingViewModels.Add(vuConsoleViewModel);
            }
        }

        public void Remove(VuConsoleViewModel vuConsoleViewModel)
        {
            if (vuConsoleViewModel != null)
            {
                DockingViewModels.Remove(vuConsoleViewModel);
            }
        }

        public void StartVuConsole(ILaunchPresetItem launchPresetItem, StartupType startupType)
        {
            if (launchPresetItem == null) throw new ArgumentNullException(nameof(launchPresetItem));

            var concatenatedLaunchParameters = launchPresetItem.GetSelectedParameters().ConcatStartupStrings();
            var openConsoleInsideLauncher = launchPresetItem.OpenConsole;

            if (startupType == StartupType.Server)
            {
                OverwriteTxtFiles((ServerPresetItem)launchPresetItem);
            }

            new Thread(() => CreateVuConsoleViewModelAndGameProcess(startupType, launchPresetItem.Name, concatenatedLaunchParameters, openConsoleInsideLauncher)).Start();
        }

        private void OverwriteTxtFiles(ServerPresetItem serverPresetItem)
        {
            ModListManager.Instance.WriteModListFile(serverPresetItem.ModSelections);
            //MapListManager.Instance.WriteMapListFile(serverPresetItem.MapListPreset.MapSelections);
            StartupManager.Instance.WriteStartupFile(serverPresetItem.StartupPreset.StartupFileContent);
            BanListManager.Instance.WriteBanListFile(serverPresetItem.BanListPreset.BannedPlayers);
        }

        private void CreateVuConsoleViewModelAndGameProcess(StartupType startupType, string presetName, string concatenatedLaunchParameters, bool attach = false)
        {
            string environmentName = (startupType == StartupType.Client) ? _vuClient : _vuServer;
            string appName = (startupType == StartupType.Client) ? _vuExe : _vuCom;

            var vuConsoleViewModel = new VuConsoleViewModel($"{environmentName} - {presetName}", startupType);

            if (attach)
            {
                vuConsoleViewModel.CloseCommand = new RelayCommand(x =>
                {
                    if (vuConsoleViewModel.GameProcess != null && !vuConsoleViewModel.GameProcess.IsAlive())
                    {
                        Remove(vuConsoleViewModel);
                    }
                    else
                    {
                        MessageBoxResult result = MessageBox.Show($"Are you sure you want to close '{presetName}'?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Kill(vuConsoleViewModel);
                            });
                        }
                    }
                });

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Add(vuConsoleViewModel);
                });

                vuConsoleViewModel.IsSelected = true;
                ActiveConsoleViewModel = vuConsoleViewModel;
            }

            try
            {
                using (vuConsoleViewModel.GameProcess.Start(Configuration.VUInstallationDirectory, Path.Combine(Configuration.VUBinariesDirectory, appName), concatenatedLaunchParameters))
                {
                    if (attach)
                    {
                        StreamReader streamReader = vuConsoleViewModel.GameProcess.ReadData();
                        while (vuConsoleViewModel.GameProcess.IsAlive() && !streamReader.EndOfStream)
                        {
                            string output = streamReader.ReadLine();

                            if (output != string.Empty)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    vuConsoleViewModel.WriteLog(output);
                                });
                            }
                        }

                        vuConsoleViewModel.GameProcess.Exception();
                    }
                }
            }
            catch (Exception e)
            {
                if (attach)
                {
                    vuConsoleViewModel.WriteLog("The following exception was raised: ");
                    vuConsoleViewModel.WriteLog(e.Message);
                }
                else
                {
                    MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                if (attach)
                {
                    vuConsoleViewModel.WriteLog("Process is closed!");
                }
                else
                {
                    // meybe show error
                }
            }
        }

        public void Kill(VuConsoleViewModel vuConsoleViewModel)
        {
            if (vuConsoleViewModel != null)
            {
                Remove(vuConsoleViewModel);
                vuConsoleViewModel.GameProcess.Kill();
            }
        }
    }
}
