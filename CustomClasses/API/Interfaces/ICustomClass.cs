using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features.Spawn;
using UnityEngine;

namespace Mistaken.CustomClasses.API.Interfaces;

public interface ICustomClass
{
    public string Name { get; }
    public string DisplayName { get; }
    public uint Type { get; }
    public string Description { get; }
    public int MaxHealth { get; set; }
    public RoleType Role { get; }
    public Dictionary<AmmoType, ushort> Ammo { get; set; }
    public ItemType[] Inventory { get; set; } //TODO: Dodać wsparcie dla customowych itemów, potencjalnie wparcie dla exiledowych
    public SpawnProperties SpawnPositions { get; }
    public Misc.PlayerInfoColorTypes Color { get; }
    public KeycardPermissions ClassPermissions { get; set; }
    public bool SetLatestUnitName { get; }
    public Vector3 Scale { get; set; }
}