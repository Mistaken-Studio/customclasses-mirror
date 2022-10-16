using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Spawn;
using Mistaken.CustomClasses.API;

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
        public override RoleType Role => RoleType.ClassD;
        public override Dictionary<AmmoType, ushort> Ammo { get; set; }
        public override ItemType[] Inventory { get; set; }

        public override SpawnProperties SpawnPositions => new SpawnProperties()
        {
            RoleSpawnPoints = new List<RoleSpawnPoint>()
            {
                new RoleSpawnPoint()
                {
                    Chance = 100f,
                    Role = RoleType.Scientist
                }
            }
        };

        public override Misc.PlayerInfoColorTypes Color => Misc.PlayerInfoColorTypes.Cyan;
        public override bool SetLatestUnitName => false;
    }
}