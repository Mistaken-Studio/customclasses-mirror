using System;
using System.Collections.Generic;
using Mistaken.CustomClasses.API.Enums;
using PlayerRoles;
using PluginAPI.Core;
using UnityEngine;
using CustomClass = Mistaken.CustomClasses.API.CustomClass;

namespace Mistaken.CustomClasses.Tests
{
    public class DBoi : CustomClass
    {
        public DBoi(Player player) : base(player)
        {
        }

        public override string Name => "DBoi";
        public override string DisplayName => "DBoi";
        public override uint Id => 1;
        public override string Description => "DBoi";
        public override SpawnStage SpawnStage { get; } = SpawnStage.RoundStart;
        public override Predicate<Player>? SpawnCondition { get; } = player => player.Role == RoleTypeId.ClassD;
        public override int MaxCount { get; } = 1;
        public override RoleTypeId Role => RoleTypeId.ClassD;
        public override Dictionary<ItemType, ushort> Ammo { get; set; }
        public override ItemType[] Inventory { get; set; } = new[] { ItemType.KeycardO5 };


        public override Vector3 SpawnPositions { get; } = Vector3.zero;
        public override Misc.PlayerInfoColorTypes Color => Misc.PlayerInfoColorTypes.Cyan;
        public override bool SetLatestUnitName => false;
    }
}