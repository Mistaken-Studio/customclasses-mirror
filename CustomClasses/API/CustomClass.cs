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
using UnityEngine;
using CustomInfoHandler = Mistaken.API.Handlers.CustomInfoHandler;

namespace Mistaken.CustomClasses.API;

[PublicAPI]
public abstract class CustomClass : ICustomClass
{
    public abstract string Name { get; }
    public abstract string DisplayName { get; }
    public abstract uint Type { get; }
    public abstract string Description { get; }
    public virtual int MaxHealth { get; set; }
    public abstract RoleType Role { get; }
    public abstract Dictionary<AmmoType, ushort> Ammo { get; set; }
    public abstract ItemType[] Inventory { get; set; }
    public abstract SpawnProperties SpawnPositions { get; }
    public abstract Misc.PlayerInfoColorTypes Color { get; }
    public virtual KeycardPermissions ClassPermissions { get; set; } = KeycardPermissions.None;
    public abstract bool SetLatestUnitName { get; }
    public virtual Vector3 Scale { get; set; } = Vector3.one;
    public Player Player { get; protected set; }

    public static Dictionary<int,CustomClass> Instances { get; } = new Dictionary<int, CustomClass>();
    public CustomClass(Player player)
    {
        Log.Debug("Creating new instance of " + GetType().Name + " for " + player.Nickname, PluginHandler.Instance.Config.DebugOutput);
        Player = player;
        if (Instances.ContainsKey(player.Id))
        {
            Instances[player.Id].OnRemoved();
        }
        Log.Debug("Adding " + GetType().Name + " to instances", PluginHandler.Instance.Config.DebugOutput);
        Instances.Add(Player.Id, this);
        Player.SetRole(Role,SpawnReason.None);
        Player.ClearInventory();
        player.AddItem(Inventory);
        Log.Debug("Setting ammo", PluginHandler.Instance.Config.DebugOutput);
        foreach (var ammoKv in Ammo)
        {
            Player.SetAmmo(ammoKv.Key, ammoKv.Value);
        }
        Player.MaxHealth = MaxHealth;
        player.Scale = Scale;
        if (SetLatestUnitName)
        {
            //TODO: Sprawdzić sensowność kodu
            var prevRole = player.Role.Type;
            var old = Respawning.RespawnManager.CurrentSequence();
            Respawning.RespawnManager.Singleton._curSequence = Respawning.RespawnManager.RespawnSequencePhase.SpawningSelectedTeam;
            player.Role.Type = this.Role == RoleType.None ? RoleType.ClassD : this.Role;
            player.ReferenceHub.characterClassManager.NetworkCurSpawnableTeamType = 2;
            player.UnitName = Respawning.RespawnManager.Singleton.NamingManager.AllUnitNames.Last().UnitName;
            Respawning.RespawnManager.Singleton._curSequence = old;
            if (this.Role == RoleType.None)
                player.Role.Type = prevRole;
        }
        Log.Debug("Setting permissions", PluginHandler.Instance.Config.DebugOutput);
        if(ClassPermissions != KeycardPermissions.None)
            player.SetSessionVariable(SessionVarType.BUILTIN_DOOR_ACCESS, ClassPermissions);
        CustomInfoHandler.Set(Player,$"cc-{Type}",$"<color={Misc.AllowedColors[Color]}>{DisplayName}</color>");
        Player.SetGUI($"cc-{Type}-name",PseudoGUIPosition.BOTTOM,$"Grasz jako <color={Misc.AllowedColors[Color]}>{DisplayName}</color>");
        Player.SetGUI($"cc-{Type}-desc",PseudoGUIPosition.MIDDLE,$"<size=150%><color={Misc.AllowedColors[Color]}>{DisplayName}</color></size>\n{Description}",15f);
        Timing.CallDelayed(0.5f, () =>
        {
            Player.Position = GetSpawnPosition();
            OnSpawned();
        });
        Log.Debug("Finished creating new instance of " + GetType().Name + " for " + player.Nickname, PluginHandler.Instance.Config.DebugOutput);
        #region Events
        Exiled.Events.Handlers.Player.Destroying += OnInternalDestroying;
        Exiled.Events.Handlers.Player.Died += OnInternalDied;
        Exiled.Events.Handlers.Player.ChangingRole += OnInternalChangingRole;
        Exiled.Events.Handlers.Player.Hurting += OnInternalHurting;
        #endregion
        Log.Debug("Subscribed to events", PluginHandler.Instance.Config.DebugOutput);
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
    protected virtual void OnKilledAnotherPlayer(Player target, CustomDamageHandler damageHandler)
    {
        
    }
    protected virtual void OnDied(Player killer, CustomDamageHandler damageHandler)
    {
        
    }
    protected virtual void OnDamageDealt(Player target, CustomDamageHandler damageHandler)
    {
    }
    protected virtual void OnDamageReceived(Player attacker, CustomDamageHandler damageHandler)
    {
    }

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
            OnKilledAnotherPlayer(ev.Target,ev.Handler);
    }

    private void OnInternalDestroying(DestroyingEventArgs ev)
    {
        if(Player.Id == ev.Player.Id)
            OnRemoved();
    }

    protected virtual void OnRemoved()
    {
        Log.Debug("Removing instance of " + GetType().Name + " for " + Player.Nickname, PluginHandler.Instance.Config.DebugOutput);
        CustomInfoHandler.Set(Player,$"cc-{Type}",null);
        Player.SetGUI($"cc-{Type}-name",PseudoGUIPosition.BOTTOM,null);
        if(ClassPermissions != KeycardPermissions.None)
            Player.RemoveSessionVariable(SessionVarType.BUILTIN_DOOR_ACCESS);
        Exiled.Events.Handlers.Player.Destroying -= OnInternalDestroying;
        Exiled.Events.Handlers.Player.Died -= OnInternalDied;
        Exiled.Events.Handlers.Player.ChangingRole -= OnInternalChangingRole;
        Exiled.Events.Handlers.Player.Hurting -= OnInternalHurting;
        Instances.Remove(Player.Id);
        Log.Debug("Unsubscribed from events", PluginHandler.Instance.Config.DebugOutput);
    }
}