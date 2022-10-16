﻿using System;
using System.Linq;
using Exiled.API.Features;
using JetBrains.Annotations;
using MEC;
using Mistaken.Updater.API.Config;

namespace Mistaken.CustomClasses
{
    [UsedImplicitly]
    internal class PluginHandler : Plugin<Config>, IAutoUpdateablePlugin
    {
        public override string Author { get; } = "Mistaken Devs";
        public override string Name { get; } = "CustomClasses";
        public override string Prefix { get; } = "MCustomClasses";
        public override Version RequiredExiledVersion { get; } = new Version(5, 2, 2);
        internal static PluginHandler Instance { get; set; }
        public override void OnEnabled()
        {
            Instance = this;
        }

        public override void OnDisabled()
        {
            Instance = null;
        }

        public AutoUpdateConfig AutoUpdateConfig { get; } = new AutoUpdateConfig()
        {
            Url = "https://git.mistaken.pl/api/v4/projects/117",
            Type = SourceType.GITLAB,
        };
    }


}