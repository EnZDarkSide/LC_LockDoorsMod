using System;
using HarmonyLib;

namespace LockDoorsMod;

[HarmonyPatch(typeof(HUDManager))]
public class Broadcaster
{
    public static Action<string, string> GetString = (_, _) => { };

    public static void BroadcastString(string data, string signature)
    {
        HUDManager.Instance.AddTextToChatOnServer("<size=0>ENZDATA/" + data + "/" + signature + "/" +
                                                  GameNetworkManager.Instance.localPlayerController.playerClientId +
                                                  "/</size>");
    }

    [HarmonyPatch(typeof(HUDManager), "AddChatMessage")]
    [HarmonyPostfix]
    internal static void AddChatMessagePatch(string chatMessage, string nameOfUserWhoTyped = "")
    {
        var strArray1 = chatMessage.Split('/');
        if (!chatMessage.StartsWith("<size=0>ENZDATA"))
            return;

        if (GameNetworkManager.Instance.localPlayerController.playerClientId.ToString() == strArray1[3])
            return;

        GetString(strArray1[1], strArray1[2]);
    }
}