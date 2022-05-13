using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ServerSync;
using UnityEngine;

namespace CameraThingy
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class CameraThingyPlugin : BaseUnityPlugin
    {
        internal const string ModName = "CameraThingy";
        internal const string ModVersion = "1.0.0";
        internal const string Author = "Azumatt";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        private static Vector3 _camera1SaveLocation;
        private static Vector3 _camera2SaveLocation;
        private static Vector3 _camera3SaveLocation;

        private const float Playerheight = 0;

        internal static bool isAdmin = false;

        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource CameraThingyLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);

        private static readonly ConfigSync ConfigSync = new(ModGUID)
            { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        public void Awake()
        {
            ConfigSync.IsLocked = true;
            /* Camera 1 */
            savelocationhotKey = config("Camera 1", "SaveLocation Hotkey 1", KeyboardShortcut.Empty,
                new ConfigDescription(
                    "The hotkey to save the camera's location.",
                    new AcceptableShortcuts()), false);
            showcamerahotKey = config("Camera 1", "ShowCameraView Hotkey 1", KeyboardShortcut.Empty,
                new ConfigDescription("The hotkey to see through the camera you placed.", new AcceptableShortcuts()),
                false);
            cameraSavedLocation = config("Camera 1", "Camera 1 Location", new Vector3(0, 2, 0),
                new ConfigDescription("The currently saved location of the camera."),
                false);

            cameraSavedLocation.SettingChanged += (_, _) => { Config.Save(); };


            /* Camera 2 */
            savelocationhotKey2 = config("Camera 2", "SaveLocation Hotkey 2", KeyboardShortcut.Empty,
                new ConfigDescription(
                    "The hotkey to save the camera's location.",
                    new AcceptableShortcuts()), false);
            showcamerahotKey2 = config("Camera 2", "ShowCameraView Hotkey 2", KeyboardShortcut.Empty,
                new ConfigDescription("The hotkey to see through the camera you placed.", new AcceptableShortcuts()),
                false);
            cameraSavedLocation2 = config("Camera 2", "Camera 2 Location", new Vector3(0, 2, 0),
                new ConfigDescription("The currently saved location of the camera."),
                false);

            cameraSavedLocation2.SettingChanged += (_, _) => { Config.Save(); };


            /* Camera 3 */
            savelocationhotKey3 = config("Camera 3", "SaveLocation Hotkey 3", KeyboardShortcut.Empty,
                new ConfigDescription(
                    "The hotkey to save the camera's location.",
                    new AcceptableShortcuts()), false);
            showcamerahotKey3 = config("Camera 3", "ShowCameraView Hotkey 3", KeyboardShortcut.Empty,
                new ConfigDescription("The hotkey to see through the camera you placed.", new AcceptableShortcuts()),
                false);
            cameraSavedLocation3 = config("Camera 3", "Camera 3 Location", new Vector3(0, 2, 0),
                new ConfigDescription("The currently saved location of the camera."),
                false);

            cameraSavedLocation3.SettingChanged += (_, _) => { Config.Save(); };

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
        }

        private void Update()
        {
            /* TODO Optimize things, don't be a lazy piece of shit :)
             Copy and paste looks bad.*/

            if (!isAdmin) return;

            /* Camera 1 */
            if (savelocationhotKey.Value.IsDown())
            {
                _camera1SaveLocation = Player.m_localPlayer.transform.position;
                cameraSavedLocation.Value = _camera1SaveLocation;
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Saving Camera 1 location.");
            }

            if (showcamerahotKey.Value.IsDown())
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Jumping to Camera 1 location.");
                GameCamera camera = GameCamera.instance;
                camera.m_freeFly = true;
                Vector3 position = cameraSavedLocation.Value;
                camera.transform.position = position + Vector3.up * 8f - Vector3.forward * 5f;
                Quaternion rot = Quaternion.LookRotation(position - camera.transform.position);
                camera.m_freeFlyYaw = rot.y;
                camera.m_freeFlyPitch = 50f;
            }

            /* Camera 2 */
            if (savelocationhotKey2.Value.IsDown())
            {
                _camera2SaveLocation = Player.m_localPlayer.transform.position;
                cameraSavedLocation2.Value = _camera2SaveLocation;
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Saving Camera 2 location.");
            }

            if (showcamerahotKey2.Value.IsDown())
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Jumping to Camera 2 location.");
                GameCamera camera = GameCamera.instance;
                camera.m_freeFly = true;
                Vector3 position = cameraSavedLocation2.Value;
                camera.transform.position = position + Vector3.up * 8f - Vector3.forward * 5f;
                Quaternion rot = Quaternion.LookRotation(position - camera.transform.position);
                camera.m_freeFlyYaw = rot.y;
                camera.m_freeFlyPitch = 50f;
            }

            /* Camera 3 */
            if (savelocationhotKey3.Value.IsDown())
            {
                _camera3SaveLocation = Player.m_localPlayer.transform.position;
                cameraSavedLocation3.Value = _camera3SaveLocation;
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Saving Camera 3 location.");
            }

            if (showcamerahotKey3.Value.IsDown())
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Jumping to Camera 3 location.");
                GameCamera camera = GameCamera.instance;
                camera.m_freeFly = true;
                Vector3 position = cameraSavedLocation3.Value;
                camera.transform.position = position + Vector3.up * 8f - Vector3.forward * 5f;
                Quaternion rot = Quaternion.LookRotation(position - camera.transform.position);
                camera.m_freeFlyYaw = rot.y;
                camera.m_freeFlyPitch = 50f;
            }
        }

        private void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && GameCamera.instance.m_freeFly)
            {
                GameCamera.instance.m_freeFly = false;
            }

            if (GameCamera.instance != null && GameCamera.instance.m_freeFly && Player.m_localPlayer == null)
            {
                GameCamera.instance.m_freeFly = false;
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.LateUpdate))]
        public static class TurnOffReversePoint
        {
            private static bool Prefix(GameCamera __instance)
            {
                bool result = true;
                if (Player.m_localPlayer != null && GameCamera.instance != null && GameCamera.instance.m_freeFly)
                {
                    ZNet.instance.SetReferencePosition(GameCamera.instance.transform.position);
                    result = false;
                    Player.m_localPlayer.transform.position = new Vector3(Player.m_localPlayer.transform.position.x,
                        Playerheight, Player.m_localPlayer.transform.position.z);
                }

                return result;
            }
        }


        [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.CreateDestroyObjects))]
        public static class RewriteZDO
        {
            private static bool Prefix(ZNetScene __instance)
            {
                bool result = true;
                if (Player.m_localPlayer != null && GameCamera.instance != null && GameCamera.instance.m_freeFly)
                {
                    Vector2i zone = ZoneSystem.instance.GetZone(ZNet.instance.GetReferencePosition());
                    __instance.m_tempCurrentObjects.Clear();
                    __instance.m_tempCurrentDistantObjects.Clear();
                    ZDOMan.instance.FindSectorObjects(zone, ZoneSystem.instance.m_activeArea,
                        ZoneSystem.instance.m_activeDistantArea, __instance.m_tempCurrentObjects,
                        __instance.m_tempCurrentDistantObjects);
                    if (!__instance.m_tempCurrentObjects.Contains(
                            ZDOMan.instance.GetZDO(Player.m_localPlayer.GetZDOID())))
                    {
                        __instance.m_tempCurrentObjects.Add(ZDOMan.instance.GetZDO(Player.m_localPlayer.GetZDOID()));
                    }

                    __instance.CreateObjects(__instance.m_tempCurrentObjects, __instance.m_tempCurrentDistantObjects);
                    __instance.RemoveObjects(__instance.m_tempCurrentObjects, __instance.m_tempCurrentDistantObjects);
                    result = false;
                }
                else
                {
                    result = true;
                }

                return result;
            }
        }

        private void OnDestroy()
        {
            Config.Save();
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                CameraThingyLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                CameraThingyLogger.LogError($"There was an issue loading your {ConfigFileName}");
                CameraThingyLogger.LogError("Please check your config entries for spelling and format!");
            }
        }


        #region ConfigOptions

        /* Camera 1 */
        private static ConfigEntry<KeyboardShortcut> savelocationhotKey = null!;
        private static ConfigEntry<KeyboardShortcut> showcamerahotKey = null!;

        private static ConfigEntry<Vector3> cameraSavedLocation = null!;

        /* Camera 2 */
        private static ConfigEntry<KeyboardShortcut> savelocationhotKey2 = null!;
        private static ConfigEntry<KeyboardShortcut> showcamerahotKey2 = null!;

        private static ConfigEntry<Vector3> cameraSavedLocation2 = null!;

        /* Camera 3 */
        private static ConfigEntry<KeyboardShortcut> savelocationhotKey3 = null!;
        private static ConfigEntry<KeyboardShortcut> showcamerahotKey3 = null!;
        private static ConfigEntry<Vector3> cameraSavedLocation3 = null!;

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        private class ConfigurationManagerAttributes
        {
            public bool? Browsable = false;
        }

        class AcceptableShortcuts : AcceptableValueBase
        {
            public AcceptableShortcuts() : base(typeof(KeyboardShortcut))
            {
            }

            public override object Clamp(object value) => value;
            public override bool IsValid(object value) => true;

            public override string ToDescriptionString() =>
                "# Acceptable values: " + string.Join(", ", KeyboardShortcut.AllKeyCodes);
        }

        #endregion
    }
}