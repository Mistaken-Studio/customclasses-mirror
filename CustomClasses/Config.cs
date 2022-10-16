using Exiled.API.Interfaces;

namespace Mistaken.CustomClasses;

internal class Config : IConfig
{
    public bool IsEnabled { get; set; } = true;
    public bool DebugOutput { get; set; } = false;
}