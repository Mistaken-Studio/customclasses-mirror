using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.DamageHandlers;
using Exiled.API.Features.Spawn;
using Exiled.Events.EventArgs;
using Exiled.Loader;
using JetBrains.Annotations;
using MEC;
using Mistaken.API;
using Mistaken.API.Extensions;
using Mistaken.API.GUI;
using Mistaken.CustomClasses.API.Interfaces;
using Mistaken.RoundLogger;
using UnityEngine;
using CustomInfoHandler = Mistaken.API.Handlers.CustomInfoHandler;

namespace Mistaken.CustomClasses.API
{
    [PublicAPI]
public abstract class CustomClass : ICustomClass
{
    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public abstract string DisplayName { get; }

    /// <inheritdoc />
    public abstract uint Id { get; }

    /// <inheritdoc />
    public abstract string Description { get; }

    /// <inheritdoc />
    public virtual int MaxHealth { get; set; }

    /// <inheritdoc />
    public abstract RoleType Role { get; }

    /// <inheritdoc />
    public abstract Dictionary<AmmoType, ushort> Ammo { get; set; }

    /// <inheritdoc />
    public abstract ItemType[] Inventory { get; set; }

    /// <inheritdoc />
    public virtual string[] CustomItems { get; set; } = Array.Empty<string>();

    /// <inheritdoc />
    public abstract SpawnProperties SpawnPositions { get; }

    /// <inheritdoc />
    public abstract Misc.PlayerInfoColorTypes Color { get; }

    /// <inheritdoc />
    public virtual KeycardPermissions ClassPermissions { get; set; } = KeycardPermissions.None;

    /// <inheritdoc />
    public abstract bool SetLatestUnitName { get; }

    /// <inheritdoc />
    public virtual Vector3 Scale { get; set; } = Vector3.one;
    /// <summary>
    /// Gets the <see cref="Player"/> playing as this class.
    /// </summary>
    public Player Player { get; protected set; }

    /// <summary>
    /// Dictionary of all the instances of <see cref="CustomClass"/>.
    /// </summary>
    public static Dictionary<int,CustomClass> Instances { get; set; }

    /// <summary>
    /// Initializes the class and spawns the specified player as custom class.
    /// </summary>
    /// <param name="player">Player to spawn as this custom class</param>
    protected CustomClass(Player player)
    {
        if (player == null)
        {
            Exiled.API.Features.Log.Debug($"Player is null, skipping creation of {Name}", PluginHandler.Instance.Config.DebugOutput);
            return;
        }
        Exiled.API.Features.Log.Debug("Creating new instance of " + Name + " for " + player.Nickname, PluginHandler.Instance.Config.DebugOutput);
        Player = player;
        Log($"Spawning {Player.Nickname} as {Name}");
        Exiled.API.Features.Log.Debug("Setting player's role to " + Role, PluginHandler.Instance.Config.DebugOutput);
        if (Instances.ContainsKey(Player.Id))
        {
            Instances[Player.Id].OnRemoved();
        }
        Exiled.API.Features.Log.Debug("Adding " + GetType().Name + " to instances", PluginHandler.Instance.Config.DebugOutput);
        Instances.Add(Player.Id, this);
        Exiled.API.Features.Log.Debug("Setting player's role to " + Role, PluginHandler.Instance.Config.DebugOutput);
        Player.SetRole(Role,SpawnReason.None);
        Exiled.API.Features.Log.Debug("Setting player's health to " + MaxHealth, PluginHandler.Instance.Config.DebugOutput);
        Player.ClearInventory();
        Exiled.API.Features.Log.Debug("Clearing player's inventory", PluginHandler.Instance.Config.DebugOutput);
        Player.AddItem(Inventory);
        Exiled.API.Features.Log.Debug("Adding items to player's inventory", PluginHandler.Instance.Config.DebugOutput);
        foreach (var customItem in CustomItems)
        {
            if(Exiled.CustomItems.API.Features.CustomItem.TryGet(customItem, out var item))
                item.Give(Player);
        }
        Exiled.API.Features.Log.Debug("Adding custom items to player's inventory", PluginHandler.Instance.Config.DebugOutput);
        Player.InfoArea &= ~PlayerInfoArea.Role;
        Exiled.API.Features.Log.Debug("Removing player's role from info area", PluginHandler.Instance.Config.DebugOutput);
        Exiled.API.Features.Log.Debug("Setting ammo", PluginHandler.Instance.Config.DebugOutput);
        foreach (var ammoKv in Ammo)
        {
            Player.SetAmmo(ammoKv.Key, ammoKv.Value);
        }
        Exiled.API.Features.Log.Debug("Setting player's health to " + MaxHealth, PluginHandler.Instance.Config.DebugOutput);
        Player.MaxHealth = MaxHealth;
        Exiled.API.Features.Log.Debug("Setting player's scale to " + Scale, PluginHandler.Instance.Config.DebugOutput);
        Player.Scale = Scale;
        if (SetLatestUnitName)
        {
            //TODO: Sprawdzić sensowność kodu
            var prevRole = Player.Role.Type;
            var old = Respawning.RespawnManager.CurrentSequence();
            Respawning.RespawnManager.Singleton._curSequence = Respawning.RespawnManager.RespawnSequencePhase.SpawningSelectedTeam;
            Player.Role.Type = this.Role == RoleType.None ? RoleType.ClassD : this.Role;
            Player.ReferenceHub.characterClassManager.NetworkCurSpawnableTeamType = 2;
            Player.UnitName = Respawning.RespawnManager.Singleton.NamingManager.AllUnitNames.Last().UnitName;
            Respawning.RespawnManager.Singleton._curSequence = old;
            if (this.Role == RoleType.None)
                Player.Role.Type = prevRole;
        }
        Exiled.API.Features.Log.Debug("Setting permissions", PluginHandler.Instance.Config.DebugOutput);
        if(ClassPermissions != KeycardPermissions.None)
            Player.SetSessionVariable(SessionVarType.BUILTIN_DOOR_ACCESS, ClassPermissions);
        CustomInfoHandler.Set(Player,$"cc-{Id}",$"<color={Misc.AllowedColors[Color]}>{DisplayName}</color>");
        Player.SetGUI($"cc-{Id}-name",PseudoGUIPosition.BOTTOM,$"Grasz jako <color={Misc.AllowedColors[Color]}>{DisplayName}</color>");
        Player.SetGUI($"cc-{Id}-desc",PseudoGUIPosition.MIDDLE,$"<size=150%><color={Misc.AllowedColors[Color]}>{DisplayName}</color></size>\n{Description}",15f);
        Timing.CallDelayed(0.5f, () =>
        {
            Player.Position = GetSpawnPosition();
            OnSpawned();
        });
        Exiled.API.Features.Log.Debug("Finished creating new instance of " + GetType().Name + " for " + player.Nickname, PluginHandler.Instance.Config.DebugOutput);
        #region Events
        Exiled.Events.Handlers.Player.Destroying += OnInternalDestroying;
        Exiled.Events.Handlers.Player.Died += OnInternalDied;
        Exiled.Events.Handlers.Player.ChangingRole += OnInternalChangingRole;
        Exiled.Events.Handlers.Player.Hurting += OnInternalHurting;
        #endregion
        Exiled.API.Features.Log.Debug("Subscribed to events", PluginHandler.Instance.Config.DebugOutput);
    }

    /// <summary>
    /// Logs message to console and round log.
    /// </summary>
    /// <param name="message">A message to log.</param>
    protected void Log(string message)
    {
        RLogger.Log("CUSTOMCLASSES",Name,message);
    }
    
    /// <summary>
    /// Gets a random <see cref="Vector3"/> from <see cref="SpawnProperties"/>.
    /// </summary>
    /// <returns>The chosen spawn location.</returns>
    protected Vector3 GetSpawnPosition()
    {
        if (SpawnPositions is null || SpawnPositions.Count() == 0)
            return Vector3.zero;

        if (SpawnPositions.StaticSpawnPoints.Count > 0)
        {
            foreach ((float chance, Vector3 pos) in SpawnPositions.StaticSpawnPoints)
            {
                int r = Loader.Random.Next(100);
                if (r <= chance)
                    return pos;
            }
        }

        if (SpawnPositions.DynamicSpawnPoints.Count > 0)
        {
            foreach ((float chance, Vector3 pos) in SpawnPositions.DynamicSpawnPoints)
            {
                int r = Loader.Random.Next(100);
                if (r <= chance)
                    return pos;
            }
        }

        if (SpawnPositions.RoleSpawnPoints.Count > 0)
        {
            foreach ((float chance, Vector3 pos) in SpawnPositions.RoleSpawnPoints)
            {
                int r = Loader.Random.Next(100);
                if (r <= chance)
                    return pos;
            }
        }

        return Vector3.zero;
    }


    //TODO: Określić lepiej nazwę dla funkcji kiedy gracz (customowa klasa) zabije innego gracza
    /// <summary>
    /// Called when the player kills another player.
    /// </summary>
    /// <param name="target">Killed player.</param>
    /// <param name="damageHandler">Damage handler.</param>
    protected virtual void OnKill(Player target, CustomDamageHandler damageHandler)
    {
        
    }
    /// <summary>
    /// Called when the player dies.
    /// </summary>
    /// <param name="killer">Killer who killed player.</param>
    /// <param name="damageHandler">Damage Handler.</param>
    protected virtual void OnDied(Player killer, CustomDamageHandler damageHandler)
    {
        
    }
    /// <summary>
    /// Called when the player hurts another player.
    /// </summary>
    /// <param name="target">Victim.</param>
    /// <param name="damageHandler">Damage Handler.</param>
    protected virtual void OnDamageDealt(Player target, CustomDamageHandler damageHandler)
    {
    }
    /// <summary>
    /// Called when the player is hurt.
    /// </summary>
    /// <param name="attacker">Attacker.</param>
    /// <param name="damageHandler">Damage Handler.</param>
    protected virtual void OnDamageReceived(Player attacker, CustomDamageHandler damageHandler)
    {
    }

    /// <summary>
    /// Called when the player spawns.
    /// </summary>
    protected virtual void OnSpawned()
    {
        
    }
    private void OnInternalHurting(HurtingEventArgs ev)
    {
        if(!ev.IsAllowed)
            return;
        if(Player.Id == ev.Attacker?.Id)
            OnDamageDealt(ev.Target, ev.Handler);
        else if(Player.Id == ev.Target.Id)
            OnDamageReceived(ev.Attacker,ev.Handler);
    }
    private void OnInternalChangingRole(ChangingRoleEventArgs ev)
    {
        if(!ev.IsAllowed)
            return;
        if(Player.Id == ev.Player.Id && !Player.GetSessionVariable<bool>(SessionVarType.TALK))
            OnRemoved();
    }
    //TODO: Sprawdzić czy event się wykonuje, bo change role może odsubskrybować eventy
    private void OnInternalDied(DiedEventArgs ev)
    {
        if (Player.Id == ev.Target.Id)
        {
            OnDied(ev.Killer,ev.Handler);
            OnRemoved();
        }
        else if(Player.Id == ev.Killer?.Id)
            OnKill(ev.Target,ev.Handler);
    }

    private void OnInternalDestroying(DestroyingEventArgs ev)
    {
        if(Player.Id == ev.Player.Id)
            OnRemoved();
    }

    /// <summary>
    /// Called when the player is removed from role.
    /// </summary>
    protected virtual void OnRemoved()
    {
        Log($"Removing {Player.Nickname} from {Name}");
        Exiled.API.Features.Log.Debug("Removing instance of " + GetType().Name + " for " + Player.Nickname, PluginHandler.Instance.Config.DebugOutput);
        CustomInfoHandler.Set(Player,$"cc-{Id}",null);
        Player.SetGUI($"cc-{Id}-name",PseudoGUIPosition.BOTTOM,null);
        if(ClassPermissions != KeycardPermissions.None)
            Player.RemoveSessionVariable(SessionVarType.BUILTIN_DOOR_ACCESS);
        Player.InfoArea |= PlayerInfoArea.Role;
        Exiled.Events.Handlers.Player.Destroying -= OnInternalDestroying;
        Exiled.Events.Handlers.Player.Died -= OnInternalDied;
        Exiled.Events.Handlers.Player.ChangingRole -= OnInternalChangingRole;
        Exiled.Events.Handlers.Player.Hurting -= OnInternalHurting;
        Instances.Remove(Player.Id);
        Exiled.API.Features.Log.Debug("Unsubscribed from events", PluginHandler.Instance.Config.DebugOutput);
    }

}

}