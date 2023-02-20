using System.Collections.Generic;
using PlayerRoles;
using PluginAPI.Core;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Mistaken.CustomClasses.API;


public class YamlCustomClass : CustomClass
{
    public YamlCustomClass(Player? player) : base(player)
    {
    }

    public override uint Id { get; }
    public override string Name { get; }
    public override string DisplayName { get; }
    public override SpawnData SpawnData { get; } = new SpawnData();
    public override string Description { get; }
    public override int MaxCount { get; }
    public override RoleTypeId Role { get; }
    public override Dictionary<ItemType, ushort> Ammo { get; set; }
    public override ItemType[] Inventory { get; set; }
    public override Vector3[] SpawnPositions { get; } = new[] { Vector3.zero };
    public override Misc.PlayerInfoColorTypes Color { get; }
    public override bool SetLatestUnitName { get; }
}