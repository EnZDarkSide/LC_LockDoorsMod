using BepInEx;
using HarmonyLib;

namespace LockDoorsMod
{
    [BepInPlugin(LDMPluginInfo.PLUGIN_GUID, LDMPluginInfo.PLUGIN_NAME, LDMPluginInfo.PLUGIN_VERSION)]
    public class LockingKeyModBase : BaseUnityPlugin
    {
        private static LockingKeyModBase instance;
        private readonly Harmony harmony = new(LDMPluginInfo.PLUGIN_GUID);

        private void Awake()
        {
            instance ??= this;
            LDMLogger.logger.LogInfo($"Lock Doors {LDMPluginInfo.PLUGIN_VERSION} :)");
            harmony.PatchAll();
        }
    }
}