using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.Loader;
using JetBrains.Annotations;
using MEC;
using Mistaken.API.Extensions;
using Mistaken.CustomClasses.API;
using Mistaken.Updater.API.Config;

namespace Mistaken.CustomClasses
{
    [UsedImplicitly]
    internal class PluginHandler : Plugin<Config>, IAutoUpdateablePlugin
    {
        public override string Author => "Mistaken Devs";
        public override string Name => "CustomClasses";
        public override string Prefix => "MCustomClasses";
        public override Version RequiredExiledVersion  => new Version(5, 2, 2);
        internal static PluginHandler Instance { get; set; }
        public static Dictionary<uint,Type> CustomClasses { get; set; }
        public override void OnEnabled()
        {
            Instance = this;
            CustomClasses = new Dictionary<uint, Type>();
            Loader.Plugins.Where(x=>x.Config.IsEnabled).ToList().ForEach(x =>
            {
                foreach (var type in x.Assembly.GetLoadableTypes())
                {
                    if (type.IsSubclassOf(typeof(CustomClass)) && !type.IsAbstract)
                    {
                        var customClass = (CustomClass)Activator.CreateInstance(type,null);
                        CustomClasses.Add(customClass.Id, type);
                    }
                }
            });
        }

        public override void OnDisabled()
        {
            Instance = null;
            CustomClasses = null;
        }

        public AutoUpdateConfig AutoUpdateConfig => new AutoUpdateConfig()
        {
            Url = "https://git.mistaken.pl/api/v4/projects/117",
            Type = SourceType.GITLAB,
        };
    }


}