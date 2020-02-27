﻿using System.Linq;
using VULauncher.Models.Repositories;
using VULauncher.ViewModels.Common;
using VULauncher.ViewModels.Items;

namespace VULauncher.ViewModels.SettingsViewModels
{
    public class ServerParamsViewModel : PresetTabViewModel<ServerParamsPresetItem>
    {
        public override string TabHeaderName { get; } = "Server Params";

        public ServerParamsViewModel()
        {
            Presets.AddRange(ServerParamsPresetsRepository.Instance.ServerParamsPresets);
            SelectedPreset = Presets.FirstOrDefault();

            RegisterChildItemCollection(Presets);
        }
    }
}
