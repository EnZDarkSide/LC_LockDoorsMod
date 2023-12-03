using HarmonyLib;
using UnityEngine;

namespace LockDoorsMod;

[HarmonyPatch(typeof(KeyItem))]
internal class KeyItemPatch
{
    [HarmonyPatch(nameof(KeyItem.ItemActivate))]
    [HarmonyPrefix]
    static bool ItemActivatePatch(ref KeyItem __instance, bool used, bool buttonDown = true)
    {
        var playerHeldBy = __instance.playerHeldBy;
        var isOwner = __instance.IsOwner;

        if (playerHeldBy == null || !isOwner || !Physics.Raycast(
                new Ray(playerHeldBy.gameplayCamera.transform.position, playerHeldBy.gameplayCamera.transform.forward),
                out var hitInfo, 3f, 2816)) return false;

        var doorLock = hitInfo.transform.GetComponent<DoorLock>();
        if (doorLock == null || doorLock.isPickingLock) return false;

        // Waiting for dev to make this field public
        var isDoorOpened = (bool)typeof(DoorLock).GetField("isDoorOpened",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(doorLock);

        if (isDoorOpened)
        {
            LDMLogger.logger.LogInfo("The door is opened");
            return false;
        }

        ;

        if (!doorLock.isLocked)
        {
            doorLock.LockDoor();
            DoorLockPatch.LockDoorPatchSyncWithServer(doorLock.NetworkObjectId);
        }
        else
        {
            doorLock.UnlockDoorSyncWithServer();
        }

        __instance.playerHeldBy.DespawnHeldObject();

        return false;
    }
}