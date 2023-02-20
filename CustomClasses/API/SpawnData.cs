using System;
using Mistaken.CustomClasses.API.Enums;
using PluginAPI.Core;

namespace Mistaken.CustomClasses.API;

/// <summary>
/// Represents a spawn data of a <see cref="CustomClass"/>.
/// </summary>
public class SpawnData
{

    /// <summary>
    /// Gets the <see cref="SpawnStage"/> of the class.
    /// </summary>
    public SpawnStage SpawnStage { get; set; } = SpawnStage.None;

    /// <summary>
    /// Gets the spawn conditions of the class.
    /// </summary>
    public Predicate<Player>? SpawnCondition { get; set; }
}