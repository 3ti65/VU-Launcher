﻿using System;
using System.Collections.Generic;
using System.Text;
using VULauncher.Models.Entities.Common;

namespace VULauncher.Models.Entities
{
    public class ClientParamsPreset : PresetEntity
    {
        public List<ParameterSelection> ParameterSelections = new List<ParameterSelection>();
    }
}
