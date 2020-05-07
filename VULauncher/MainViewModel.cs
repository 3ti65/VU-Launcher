﻿using System.Collections.Generic;
using System.Linq;
using VULauncher.Commands;
using VULauncher.Models.Setup;
using VULauncher.ViewModels;
using VULauncher.ViewModels.Common;
using VULauncher.ViewModels.Enums;

namespace VULauncher
{
    public class MainViewModel : ViewModel
    {
        private ActiveViewType _activeViewType;
        private ViewModel _currentViewModel;

        public ConsolesViewModel ConsolesViewModel { get; set; }
        public SettingsViewModel SettingsViewModel { get; set; }
        public ConfigViewModel ConfigViewModel { get; set; }

        public RelayCommand StartClientPresetCommand { get; }
        public RelayCommand StartServerPresetCommand { get; }
       
        public RelayCommand SaveTabCommand { get; }
        public RelayCommand SaveAllTabsCommand { get; }
        public RelayCommand DiscardChangesTabCommand { get; }
        public RelayCommand DiscardChangesAllTabsCommand { get; }

        public MainViewModel()
        {
            LocalSetup.VerifyOrCreate();

            ConsolesViewModel = new ConsolesViewModel();
            SettingsViewModel = new SettingsViewModel();
            ConfigViewModel = new ConfigViewModel();

            SaveTabCommand = new RelayCommand(x => SaveTab(), x => CanSaveTab);
            SaveAllTabsCommand = new RelayCommand(x => SaveAllTabs(), x => CanSaveAllTabs);
            DiscardChangesTabCommand = new RelayCommand(x => DiscardChangesTab(), x => CanDiscardChangesTab);
            DiscardChangesAllTabsCommand = new RelayCommand(x => DiscardChangesAllTabs(), x => CanDiscardChangesAllTabs);

            StartClientPresetCommand = new RelayCommand(x => StartClientPreset(), x => CanStartClientPreset);
            StartServerPresetCommand = new RelayCommand(x => StartServerPreset(), x => CanStartServerPreset);

            ActiveViewType = ActiveViewType.Console;
        }

        private void StartClientPreset()
        {
            ActiveViewType = ActiveViewType.Console;
            ConsolesViewModel.StartVuConsole(SettingsViewModel.ClientPresetsViewModel.SelectedPreset, StartupType.Client);
        }

        private void StartServerPreset()
        {
            ActiveViewType = ActiveViewType.Console;
	        ConsolesViewModel.StartVuConsole(SettingsViewModel.ServerPresetsViewModel.SelectedPreset, StartupType.Server);
        }

        private void SaveTab()
        {
            SettingsViewModel.SaveTab();
        }

        private void SaveAllTabs()
        {
            SettingsViewModel.SaveAllTabs();
        }

        private void DiscardChangesTab()
        {
            SettingsViewModel.DiscardChangesTab();
        }

        private void DiscardChangesAllTabs()
        {
            SettingsViewModel.DiscardChangesAllTabs();
        }

        private bool CanSaveTab => CurrentViewModel == SettingsViewModel && SettingsViewModel.CurrentTabViewModel.IsDirty;
        private bool CanSaveAllTabs => CurrentViewModel == SettingsViewModel && SettingsViewModel.IsDirty;
        private bool CanDiscardChangesTab => CurrentViewModel == SettingsViewModel && SettingsViewModel.CurrentTabViewModel.IsDirty;
        private bool CanDiscardChangesAllTabs => CurrentViewModel == SettingsViewModel && SettingsViewModel.IsDirty;

        private bool CanStartClientPreset
        {
            get => SettingsViewModel.ClientPresetsViewModel.SelectedPreset?.ClientParamsPreset != null && (!ConsolesViewModel.IsClientRunning || 
                                                                                                           ConsolesViewModel.IsClientRunning && SettingsViewModel.ClientPresetsViewModel.SelectedPreset.HasMultiParameterSelected);
        }

        private bool CanStartServerPreset
        {
            get => SettingsViewModel.ServerPresetsViewModel.SelectedPreset?.ServerParamsPreset != null && !ConsolesViewModel.IsServerRunning;
        } 

        public ActiveViewType ActiveViewType
        {
            get => _activeViewType;
            
            set
            {
                if (SetField(ref _activeViewType, value))
                {
                    CurrentViewModel = value switch
                    {
                        ActiveViewType.Console => ConsolesViewModel,
                        ActiveViewType.Settings => SettingsViewModel,
                        ActiveViewType.Config => ConfigViewModel,
                        _ => CurrentViewModel
                    };
                }
            } 
        }

        public ViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set => SetField(ref _currentViewModel, value);
        }
    }
}
