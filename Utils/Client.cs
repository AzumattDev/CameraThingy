using System;
using HarmonyLib;

namespace CameraThingy.Utils;

[HarmonyPatch]
public class CameraThingyClient
{
    /// <summary>
    ///     All requests, put in order to the corresponding events. These are "sent" to the server
    /// </summary>
    public static void RPC_CameraThingyRequestAdminSync(long sender, ZPackage pkg)
    {
    }

    public static void RPC_CameraThingyBadRequestMsg(long sender, ZPackage pkg)
    {
        if (sender != ZRoutedRpc.instance.GetServerPeerID() || pkg == null || pkg.Size() <= 0)
            return;
        string str = pkg.ReadString();
        if (str == "")
            return;
        Chat.m_instance.AddString("Server", "<color=\"red\">" + str + "</color>", Talker.Type.Normal);
    }

    /// <summary>
    ///     All events, put in order to the corresponding requests. These are "received" from the server
    ///     put logic here that you want to happen on the client AFTER getting the information from the server.
    /// </summary>
    public static void RPC_CameraThingyEventAdminSync(long sender, ZPackage pkg)
    {
        CameraThingyPlugin.CameraThingyLogger.LogInfo("This account is an admin.");
        Chat.m_instance.AddString("[CameraThingy]", "<color=\"green\">" + "Admin permissions synced" + "</color>",
            Talker.Type.Normal);
        CameraThingyPlugin.isAdmin = true;
    }

    [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
    public static class PlayerOnSpawnPatch
    {
        private static void Prefix()
        {
            if (!ZNet.instance.IsDedicated() && ZNet.instance.IsServer())
            {
                CameraThingyPlugin.isAdmin = true;

                CameraThingyPlugin.CameraThingyLogger.LogInfo(
                    $"Local Play Detected setting Admin: {CameraThingyPlugin.isAdmin}");
            }

            if (ZRoutedRpc.instance == null || !ZNetScene.instance)
                return;
            if (!CameraThingyPlugin.isAdmin)
            {
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "CameraThingyRequestAdminSync",
                    new ZPackage());
            }
        }
    }
}

[HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Start))]
internal static class FejdStartup_Start_Patch
{
    private static void Postfix()
    {
        Console.SetConsoleEnabled(true);
    }
}

[HarmonyPatch]
internal static class ClientResetPatches
{
    [HarmonyPatch(typeof(Game), nameof(Game.Logout))]
    [HarmonyPatch(typeof(ZNet), nameof(ZNet.Disconnect))]
    [HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_Error))]
    private static void Prefix()
    {
        if (CameraThingyPlugin.isAdmin)
        {
            CameraThingyPlugin.isAdmin = false;
            CameraThingyPlugin.CameraThingyLogger.LogDebug($"Admin Status changed to: {CameraThingyPlugin.isAdmin}");
        }
    }
}

[HarmonyPatch(typeof(Game), nameof(Game.Start))]
static class ClientRPC_Registrations
{
    static void Prefix(Game __instance)
    {
        if (ZNet.m_isServer) return;
        ZRoutedRpc.instance.Register("CameraThingyRequestAdminSync",
            new Action<long, ZPackage>(CameraThingyClient.RPC_CameraThingyRequestAdminSync));
        ZRoutedRpc.instance.Register("CameraThingyEventAdminSync",
            new Action<long, ZPackage>(CameraThingyClient.RPC_CameraThingyEventAdminSync));
        ZRoutedRpc.instance.Register("CameraThingyBadRequestMsg",
            new Action<long, ZPackage>(CameraThingyClient.RPC_CameraThingyBadRequestMsg));
    }
}