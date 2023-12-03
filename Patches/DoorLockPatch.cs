using System;
using HarmonyLib;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;

namespace LockDoorsMod;

[JsonObject]
internal class LockDoorData
{
    [JsonProperty] public ulong networkObjectId { get; set; }

    [JsonProperty] public ulong clientId { get; set; }
}

[HarmonyPatch(typeof(DoorLock))]
public class DoorLockPatch
{
    [HarmonyPatch(nameof(DoorLock.Awake))]
    [HarmonyPrefix]
    private static void Awake()
    {
        Broadcaster.GetString += CloseDoorReceived;
    }

    private static void CloseDoorReceived(string data, string signature)
    {
        var dataDeserialized = JsonConvert.DeserializeObject<LockDoorData>(data);

        // Hierarchy: Door -> Cube(DoorLock)

        LDMLogger.logger.LogInfo($"[BROADCAST_RECEIVE] Got message \n data: {data} \n signature: {signature}");
        switch (signature)
        {
            case "lock_door":
                var doorComponent = FindObjectById(Convert.ToUInt64(dataDeserialized.networkObjectId));
                var doorLock = doorComponent.GetComponentInChildren<DoorLock>();
                LDMLogger.logger.LogInfo(
                    $"[BROADCAST_RECEIVE] found object with id {dataDeserialized.networkObjectId}: {doorComponent} \n {doorLock} \n doorLockNID: {doorLock.NetworkObjectId}");
                doorLock.LockDoor();
                break;
        }
    }

    [HarmonyPatch(nameof(DoorLock.LockDoor))]
    [HarmonyPrefix]
    private static bool LockDoorPatch(ref DoorLock __instance, ref bool ___isDoorOpened, ref bool ___isLocked,
        ref AudioSource ___doorLockSFX, ref AudioClip ___unlockSFX)
    {
        if (___isDoorOpened || ___isLocked)
        {
            LDMLogger.logger.LogInfo("The door is opened or locked. Can't lock it");
            return false;
        }

        var obj = ___doorLockSFX;
        obj.PlayOneShot(___unlockSFX);

        return true;
    }

    public static void LockDoorPatchSyncWithServer(ulong networkObjectId)
    {
        var data = new LockDoorData()
        {
            networkObjectId = networkObjectId,
            clientId = GameNetworkManager.Instance.localPlayerController.playerClientId
        };

        LDMLogger.logger.LogInfo("[BROADCAST] Locking the door");
        Broadcaster.BroadcastString(
            JsonConvert.SerializeObject(data, Formatting.None), "lock_door");
    }

    private static GameObject FindObjectById(ulong networkId)
    {
        return networkId == 0 ? null : NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkId].gameObject;
    }
}