using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features.Spawn;
using UnityEngine;

namespace Mistaken.CustomClasses.API.Interfaces
{
    /// <summary>
    /// Interface that defines all CustomClasses.
    /// </summary>
    public interface ICustomClass
    {
        /// <summary>
        /// Gets the name of the class.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets the display name of the class.
        /// </summary>
        public string DisplayName { get; }
        /// <summary>
        /// Gets the unique ID of the class.
        /// </summary>
        public uint Id { get; }
        /// <summary>
        /// Gets the description of the class.
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// Gets and sets the MaxHealth of the class.
        /// </summary>
        public int MaxHealth { get; set; }
        /// <summary>
        /// Gets the <see cref="RoleType"/> of the class.
        /// </summary>
        public RoleType Role { get; }
        /// <summary>
        /// Sets the starting ammo of the class.
        /// </summary>
        public Dictionary<AmmoType, ushort> Ammo { get; set; }
        /// <summary>
        /// Sets the starting items of the class.
        /// </summary>
        public ItemType[] Inventory { get; set; }
        /// <summary>
        /// Sets the starting custom items of the class.
        /// </summary>
        public string[] CustomItems { get; set; }
        /// <summary>
        /// Sets the potential spawn locations of the class.
        /// </summary>
        public SpawnProperties SpawnPositions { get; }
        /// <summary>
        /// Sets the color of the class.
        /// </summary>
        public Misc.PlayerInfoColorTypes Color { get; }
        /// <summary>
        /// Sets the <see cref="KeycardPermissions"/> of the class.
        /// </summary>
        public KeycardPermissions ClassPermissions { get; set; }
        /// <summary>
        /// Whether or not the class is in the latest unit.
        /// </summary>
        public bool SetLatestUnitName { get; }
        /// <summary>
        /// Sets the <see cref="Vector3"/> scale of the class.
        /// </summary>
        public Vector3 Scale { get; set; }
    }
}

