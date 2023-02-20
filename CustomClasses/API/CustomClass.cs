using System;
using System.Collections.Generic;
using System.Linq;
using Interactables.Interobjects.DoorUtils;
using JetBrains.Annotations;
using MEC;
using Mistaken.API;
using Mistaken.API.GUI;
using Mistaken.CustomClasses.API.Enums;
using Mistaken.RoundLogger;
using PlayerRoles;
using PlayerStatsSystem;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using Respawning.NamingRules;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Mistaken.CustomClasses.API
{
    /// <summary>
    /// Base class for custom classes.
    /// </summary>
    [PublicAPI]
public abstract class CustomClass
{
    private Guid _internalId;

    /// <summary>
    /// Gets the unique ID of the class.
    /// </summary>
    public abstract uint Id { get; }
    /// <summary>
    /// Gets the name of the class.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the display name of the class.
    /// </summary>
    public abstract string DisplayName { get; }

    /// <summary>
    /// Gets the <see cref="SpawnData"/> of the class.
    /// </summary>
    public abstract SpawnData SpawnData { get; }

    /// <summary>
    /// Gets the description of the class.
    /// </summary>
    public abstract string Description { get; }
    
    
    
    /// <summary>
    /// Gets the maximum amount of players that can be this class.
    /// </summary>
    public abstract int MaxCount { get; }
    /// <summary>
    /// Gets the <see cref="RoleTypeId"/> of the class.
    /// </summary>
    public abstract RoleTypeId Role { get; }

    /// <summary>
    /// Sets the starting ammo of the class.
    /// </summary>
    public abstract Dictionary<ItemType, ushort> Ammo { get; set; }

    /// <summary>
    /// Sets the starting items of the class.
    /// </summary>
    public abstract ItemType[] Inventory { get; set; }
    
    /// <summary>
    /// Sets the potential spawn locations of the class.
    /// </summary>
    public abstract Vector3[] SpawnPositions { get; }

    /// <summary>
    /// Sets the color of the class.
    /// </summary>
    public abstract Misc.PlayerInfoColorTypes Color { get; }

    /// <summary>
    /// Sets the <see cref="KeycardPermissions"/> of the class.
    /// </summary>
    public virtual KeycardPermissions ClassPermissions { get; set; } = KeycardPermissions.None;

    /// <summary>
    /// Whether or not the class is in the latest unit.
    /// </summary>
    public abstract bool SetLatestUnitName { get; }

    // /// <summary>
    // /// Sets the <see cref="Vector3"/> scale of the class.
    // /// </summary>
    // public virtual Vector3 Scale { get; set; } = Vector3.one;
    
    /// <summary>
    /// Gets the <see cref="Player"/> playing as this class.
    /// </summary>
    [YamlIgnore]
    public Player Player { get; protected set; }

    /// <summary>
    /// Initializes the class and spawns the specified player as custom class.
    /// </summary>
    /// <param name="player">Player to spawn as this custom class</param>
    protected CustomClass(Player? player)
    {
        if (player == null)
        {
            Log.Debug($"Player is null, skipping creation of {Name}", PluginMain.Config.DebugOutput);
            return;
        }
        if(MaxCount > 0 && PluginMain.CustomClassInstances.Count(x => x.Value.Id == Id) >= MaxCount)
        {
            Log.Debug($"Max count of {Name} reached, skipping creation of {Name}", PluginMain.Config.DebugOutput);
            return;
        }
        _internalId = Guid.NewGuid();
        Log.Debug("Creating new instance of " + Name + " for " + player.Nickname, PluginMain.Config.DebugOutput);
        Player = player;
        //Log($"Spawning {Player.Nickname} as {Name}");
        Log.Debug("Setting player's role to " + Role, PluginMain.Config.DebugOutput);
        if (PluginMain.CustomClassInstances.ContainsKey(Player.PlayerId))
        {
            PluginMain.CustomClassInstances[Player.PlayerId].OnRemoved();
        }
        Log.Debug("Adding " + GetType().Name + " to instances", PluginMain.Config.DebugOutput);
        PluginMain.CustomClassInstances.Add(Player.PlayerId, this);
        Log.Debug("Setting player's role to " + Role, PluginMain.Config.DebugOutput);
        Player.SetRole(Role);
        Log.Debug("Clearing player's inventory", PluginMain.Config.DebugOutput);
        Player.ClearInventory();
        Log.Debug("Clearing player's inventory", PluginMain.Config.DebugOutput);
        if(Inventory is not null && Inventory.Length > 0)
            foreach(var item in Inventory)
                Player.AddItem(item);
        Log.Debug("Removing player's role from info area", PluginMain.Config.DebugOutput);
        Player.PlayerInfo.IsRoleHidden = true;
        Log.Debug("Setting ammo", PluginMain.Config.DebugOutput);
        if(Ammo is not null && Ammo.Count > 0)
            foreach (var ammoKv in Ammo)
            {
                Player.SetAmmo(ammoKv.Key, ammoKv.Value);
            }
        if (SetLatestUnitName)
        {
            if (player.RoleBase is HumanRole humanRole)
                humanRole.UnitNameId = (byte)UnitNameMessageHandler.ReceivedNames[humanRole.AssignedSpawnableTeam].Count;
        }
        Log.Debug("Setting permissions", PluginMain.Config.DebugOutput);
        // if(ClassPermissions != KeycardPermissions.None)
        //     Player.SetSessionVariable(SessionVarType.BUILTIN_DOOR_ACCESS, ClassPermissions);
        // CustomInfoHandler.Set(Player,$"cc-{Id}",$"<color={Misc.AllowedColors[Color]}>{DisplayName}</color>");
        // Player.SetGUI($"cc-{Id}-name",PseudoGUIPosition.BOTTOM,$"Grasz jako <color={Misc.AllowedColors[Color]}>{DisplayName}</color>");
        // Player.SetGUI($"cc-{Id}-desc",PseudoGUIPosition.MIDDLE,$"<size=150%><color={Misc.AllowedColors[Color]}>{DisplayName}</color></size>\n{Description}",15f);
        Timing.CallDelayed(0.5f, () =>
        {
            Player.Position = GetSpawnPosition();
            OnSpawned();
        });
        Log.Debug("Finished creating new instance of " + GetType().Name + " for " + player.Nickname, PluginMain.Config.DebugOutput);
        PluginAPI.Events.EventManager.RegisterEvents(this);
        Log.Debug("Subscribed to events", PluginMain.Config.DebugOutput);
    }

    // /// <summary>
    // /// Logs message to console and round log.
    // /// </summary>
    // /// <param name="message">A message to log.</param>
    // protected void LogInternal(string message)
    // {
    //     RLogger.Log("CUSTOMCLASSES",Name,message);
    // }
    
    /// <summary>
    /// Gets a random <see cref="Vector3"/> from <see cref="SpawnProperties"/>.
    /// </summary>
    /// <returns>The chosen spawn location.</returns>
    protected Vector3 GetSpawnPosition()
    {
        // if (SpawnPositions is null || SpawnPositions.Count() == 0)
        //     return Vector3.zero;
        //
        // if (SpawnPositions.StaticSpawnPoints.Count > 0)
        // {
        //     foreach ((float chance, Vector3 pos) in SpawnPositions.StaticSpawnPoints)
        //     {
        //         int r = Loader.Random.Next(100);
        //         if (r <= chance)
        //             return pos;
        //     }
        // }
        //
        // if (SpawnPositions.DynamicSpawnPoints.Count > 0)
        // {
        //     foreach ((float chance, Vector3 pos) in SpawnPositions.DynamicSpawnPoints)
        //     {
        //         int r = Loader.Random.Next(100);
        //         if (r <= chance)
        //             return pos;
        //     }
        // }
        //
        // if (SpawnPositions.RoleSpawnPoints.Count > 0)
        // {
        //     foreach ((float chance, Vector3 pos) in SpawnPositions.RoleSpawnPoints)
        //     {
        //         int r = Loader.Random.Next(100);
        //         if (r <= chance)
        //             return pos;
        //     }
        // }

        return Vector3.zero;
    }


    //TODO: Określić lepiej nazwę dla funkcji kiedy gracz (customowa klasa) zabije innego gracza
    /// <summary>
    /// Called when the player kills another player.
    /// </summary>
    /// <param name="target">Killed player.</param>
    /// <param name="damageHandler">Damage handler.</param>
    protected virtual void OnKill(Player target, DamageHandlerBase damageHandler)
    {
        
    }
    /// <summary>
    /// Called when the player dies.
    /// </summary>
    /// <param name="killer">Killer who killed player.</param>
    /// <param name="damageHandler">Damage Handler.</param>
    protected virtual void OnDied(Player killer, DamageHandlerBase damageHandler)
    {
        
    }
    /// <summary>
    /// Called when the player hurts another player.
    /// </summary>
    /// <param name="target">Victim.</param>
    /// <param name="damageHandler">Damage Handler.</param>
    protected virtual void OnDamageDealt(Player target, DamageHandlerBase damageHandler)
    {
    }
    /// <summary>
    /// Called when the player is hurt.
    /// </summary>
    /// <param name="attacker">Attacker.</param>
    /// <param name="damageHandler">Damage Handler.</param>
    protected virtual void OnDamageReceived(Player attacker, DamageHandlerBase damageHandler)
    {
    }

    /// <summary>
    /// Called when the player spawns.
    /// </summary>
    protected virtual void OnSpawned()
    {
        
    }
    [PluginEvent(ServerEventType.PlayerDamage)]
    private void OnInternalHurting(Player target, Player attacker, DamageHandlerBase handler)
    {
        if(Player.PlayerId == attacker?.PlayerId)
            OnDamageDealt(target,handler);
        else if(Player.PlayerId == target?.PlayerId)
            OnDamageReceived(attacker,handler);
    }
    [PluginEvent(ServerEventType.PlayerChangeRole)]
    private void OnInternalChangingRole(Player player, PlayerRoleBase oldRole, RoleTypeId newRole, RoleChangeReason reason)
    {
        if(Player.PlayerId == player.PlayerId && !Player.TemporaryData.Contains("preventCCRemove"))
            OnRemoved();
    }
    //TODO: Sprawdzić czy event się wykonuje, bo change role może odsubskrybować eventy
    [PluginEvent(ServerEventType.PlayerDeath)]
    private void OnInternalDied(Player player, Player attacker, DamageHandlerBase damageHandlerBase)
    {
        if (Player.PlayerId == player.PlayerId)
        {
            OnDied(attacker,damageHandlerBase);
            OnRemoved();
        }
        else if(Player.PlayerId == attacker?.PlayerId)
            OnKill(player,damageHandlerBase);
    }

    [PluginEvent(ServerEventType.PlayerLeft)]
    private void OnInternalDestroying(Player player)
    {
        if(Player.PlayerId == player.PlayerId)
            OnRemoved();
    }

    /// <summary>
    /// Called when the player is removed from role.
    /// </summary>
    protected internal virtual void OnRemoved()
    {
        //Log($"Removing {Player.Nickname} from {Name}");
        Log.Debug("Removing instance of " + GetType().Name + " for " + Player.Nickname, PluginMain.Config.DebugOutput);
        //CustomInfoHandler.Set(Player,$"cc-{Id}",null);
        //Player.SetGUI($"cc-{Id}-name",PseudoGUIPosition.BOTTOM,null);
        Player.PlayerInfo.IsRoleHidden = false;
        PluginAPI.Events.EventManager.UnregisterEvents(this);
        PluginMain.CustomClassInstances.Remove(Player.PlayerId);
        Log.Debug("Unsubscribed from events", PluginMain.Config.DebugOutput);
    }

}

}