using System;
using HarmonyLib;

namespace CameraThingy.Utils;

[HarmonyPatch]
public class CameraThingyServer
{
    /// <summary>
    ///     All requests, put in order to the corresponding events. These are "sent" to the server
    /// </summary>
    public static void RPC_CameraThingyRequestAdminSync(long sender, ZPackage pkg)
    {
        ZNetPeer peer = ZNet.instance.GetPeer(sender);
        if (peer != null)
        {
            string str = peer.m_rpc.GetSocket().GetHostName();
            if (!ZNet.instance.IsDedicated() && ZNet.instance.IsServer())
            {
                CameraThingyPlugin.isAdmin = true;
                CameraThingyPlugin.CameraThingyLogger.LogDebug(
                    $"Local Play Detected setting Admin:{CameraThingyPlugin.isAdmin}");
            }

            //string str = ((ZSteamSocket)peer.m_socket).GetPeerID().m_SteamID.ToString();
            if (ZNet.instance.m_adminList == null || !ZNet.instance.m_adminList.Contains(str))
                return;
            CameraThingyPlugin.CameraThingyLogger.LogInfo($"Admin Detected: {str}");
            ZRoutedRpc.instance.InvokeRoutedRPC(sender, "CameraThingyEventAdminSync", pkg);
        }
        else
        {
            ZPackage zpackage = new();
            zpackage.Write("You aren't an Admin!");
            ZRoutedRpc.instance.InvokeRoutedRPC(sender, "CameraThingyBadRequestMsg", zpackage);
        }
    }

    /// <summary>
    ///     All events, put in order to the corresponding requests. These are "received" from the server
    ///     put logic here that you want to happen on the client AFTER getting the information from the server.
    /// </summary>
    public static void RPC_CameraThingyEventAdminSync(long sender, ZPackage pkg)
    {
    }
}

[HarmonyPatch]
public class ServerRPC_Registrations
{
    [HarmonyPatch(typeof(Game), nameof(Game.Start))]
    [HarmonyPrefix]
    public static void Prefix()
    {
        if (!ZNet.m_isServer) return;
        ZRoutedRpc.instance.Register("CameraThingyRequestAdminSync",
            new Action<long, ZPackage>(CameraThingyServer.RPC_CameraThingyRequestAdminSync));
        ZRoutedRpc.instance.Register("CameraThingyEventAdminSync",
            new Action<long, ZPackage>(CameraThingyServer.RPC_CameraThingyEventAdminSync));
    }
}