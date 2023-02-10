using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mistaken.CustomClasses.API;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;

namespace Mistaken.CustomClasses
{
    internal class PluginMain
    {
        [PluginConfig] public static Config Config;
        public static Dictionary<uint,Type> CustomClasses { get; set; }
        public static Dictionary<int,CustomClass> CustomClassInstances { get; set; }
        [PluginEntryPoint("CustomClasses","1.0.0", "Base plugin for CustomClasses", "barwa")]
        public void Load()
        {
            CustomClasses = new Dictionary<uint, Type>();
            CustomClassInstances = new Dictionary<int, CustomClass>();
            Directory.GetFiles(PluginHandler.Get(this).PluginDirectoryPath, "*.dll", SearchOption.AllDirectories).ToList().ForEach(x =>
            {
                try
                {
                    var assembly = Assembly.LoadFile(x);
                    foreach (var type in assembly.GetTypes())
                    {
                        if (!type.IsAbstract && type.IsSubclassOf(typeof(CustomClass)))
                            CustomClasses.Add(((CustomClass)Activator.CreateInstance(type, new[] { (object)null })).Id,
                                type);
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"Error while loading assembly {x}: {e}");
                }
            });
        }
        [PluginUnload]
        public void Unload()
        {
            CustomClasses = null;
            foreach (var customClassInstance in CustomClassInstances)
            {
                customClassInstance.Value.OnRemoved();
                
            }
            CustomClassInstances = null;
            
        }
    }
}