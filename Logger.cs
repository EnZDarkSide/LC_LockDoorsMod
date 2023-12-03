using BepInEx.Logging;

namespace LockDoorsMod;

public static class LDMLogger
{
    public static readonly ManualLogSource logger = Logger.CreateLogSource(PluginInfo.PLUGIN_GUID);
}