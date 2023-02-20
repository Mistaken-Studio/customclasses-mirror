using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mistaken.CustomClasses.API;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using UnityEngine;

namespace Mistaken.CustomClasses
{
    internal class PluginMain
    {
        [PluginConfig] public static Config Config;
        public static Dictionary<uint,(Type type, SpawnData spawnData)> CustomClasses { get; set; }
        public static Dictionary<int,CustomClass> CustomClassInstances { get; set; }
        [PluginEntryPoint("CustomClasses","1.0.0", "Base plugin for CustomClasses", "barwa")]
        public void Load()
        {
            Log.Info("AAAAAAAAAAAAAAAAAAAAA");
            Log.Info("Loading plugin", "CustomClasses");
            CustomClasses = new Dictionary<uint, (Type type, SpawnData spawnData)>();
            CustomClassInstances = new Dictionary<int, CustomClass>();
            Log.Info("Searching for custom classes", "CustomClasses");
            Directory.GetFiles(PluginHandler.Get(this).PluginDirectoryPath, "*.dll", SearchOption.AllDirectories).ToList().ForEach(x =>
            {
                try
                {
                    var assembly = Assembly.LoadFile(x);
                    foreach (var type in assembly.GetTypes())
                    {
                        if (!type.IsAbstract && type.IsSubclassOf(typeof(CustomClass)))
                        {
                            var cc = ((CustomClass)Activator.CreateInstance(type, new[] { (object)null }));
                            if (cc.Id == 0)
                            {
                                Log.Error($"CustomClass {cc.Name} has invalid ID");
                                continue;
                            }

                            CustomClasses.Add(cc.Id,(type,cc.SpawnData));
                        }
                            
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"Error while loading assembly {x}: {e}");
                }
            });
            Log.Info($"Found {CustomClasses.Count} custom classes", "CustomClasses");
            Log.Info("Enabling events", "CustomClasses");
            var serializer = new YamlDotNet.Serialization.SerializerBuilder()
                .WithTypeConverter(new Vector3YamlConverter()).Build();
            var cc = new YamlCustomClass(null);
            var yaml = serializer.Serialize(cc);
            Log.Info(yaml);
            new EventHandler();

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