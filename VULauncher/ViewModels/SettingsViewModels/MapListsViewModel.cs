﻿using System.Linq;
using VULauncher.Models.Repositories;
using VULauncher.ViewModels.Common;
using VULauncher.ViewModels.Items;

namespace VULauncher.ViewModels.SettingsViewModels
{
    public class MapListsViewModel : PresetTabViewModel<MapListPresetItem>
    {
        public override string TabHeaderName { get; } = "MapLists";

        public MapListsViewModel()
        {
            Presets.AddRange(MapListPresetsRepository.Instance.MapListPresets);
            SelectedPreset = Presets.FirstOrDefault();

            RegisterChildItemCollection(Presets);
        }
    }
}
