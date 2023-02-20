using System;
using System.Linq;
using Mistaken.CustomClasses.API.Enums;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;

namespace Mistaken.CustomClasses;

public class EventHandler
{
    public EventHandler()
    {
        PluginAPI.Events.EventManager.RegisterEvents(this);
    }
    ~EventHandler()
    {
        PluginAPI.Events.EventManager.UnregisterEvents(this);
    }
    [PluginEvent(ServerEventType.RoundStart)]
    void OnRoundStart()
    {
        var allPlayers = Player.GetPlayers();
        foreach (var kValueTuple in PluginMain.CustomClasses.Where(x=>x.Value.spawnData.SpawnStage == SpawnStage.RoundStart))
        {
            var player = allPlayers.Where(x => kValueTuple.Value.spawnData.SpawnCondition?.Invoke(x) ?? false);
            var customClass = (CustomClass)Activator.CreateInstance(kValueTuple.Value.type, new[] { player });
        }
    }
}