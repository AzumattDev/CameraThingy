using System;
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
        internal const string ModVersion = "2.0.0";
        internal const string Author = "Azumatt";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

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
            _savelocationhotKey = config("Camera 1", "SaveLocation Hotkey 1", KeyboardShortcut.Empty,
                new ConfigDescription(
                    "The hotkey to save the camera's location.",
                    new AcceptableShortcuts()), false);
            _showcamerahotKey = config("Camera 1", "ShowCameraView Hotkey 1", KeyboardShortcut.Empty,
                new ConfigDescription("The hotkey to see through the camera you placed.", new AcceptableShortcuts()),
                false);
            _cameraSavedLocation = config("Camera 1", "Camera 1 Location", new Vector3(0, 2, 0),
                new ConfigDescription("The currently saved location of the camera."),
                false);

            _cameraSavedRotation = config("Camera 1", "Camera 1 Rotation", new Quaternion(),
                new ConfigDescription("The currently saved rotation of the camera."),
                false);
            _cameraSavedEuler = config("Camera 1", "Camera 1 Euler", new Vector3(0, 2, 0),
                new ConfigDescription("The currently saved euler of the camera."),
                false);

            _cameraSavedVectorUp = config("Camera 1", "Camera 1 VectorUP", new Vector3(0, 2, 0),
                new ConfigDescription("The currently saved yaw of the camera."),
                false);

            _cameraSavedLocation.SettingChanged += (_, _) => { Config.Save(); };


            /* Camera 2 */
            _savelocationhotKey2 = config("Camera 2", "SaveLocation Hotkey 2", KeyboardShortcut.Empty,
                new ConfigDescription(
                    "The hotkey to save the camera's location.",
                    new AcceptableShortcuts()), false);
            _showcamerahotKey2 = config("Camera 2", "ShowCameraView Hotkey 2", KeyboardShortcut.Empty,
                new ConfigDescription("The hotkey to see through the camera you placed.", new AcceptableShortcuts()),
                false);
            _cameraSavedLocation2 = config("Camera 2", "Camera 2 Location", new Vector3(0, 2, 0),
                new ConfigDescription("The currently saved location of the camera."),
                false);

            _cameraSavedRotation2 = config("Camera 2", "Camera 2 Rotation", new Quaternion(),
                new ConfigDescription("The currently saved rotation of the camera."),
                false);

            _cameraSavedEuler2 = config("Camera 2", "Camera 2 Euler", new Vector3(0, 2, 0),
                new ConfigDescription("The currently saved euler of the camera."),
                false);

            _cameraSavedVectorUp2 = config("Camera 2", "Camera 2 VectorUP", new Vector3(0, 2, 0),
                new ConfigDescription("The currently saved yaw of the camera."),
                false);

            _cameraSavedLocation2.SettingChanged += (_, _) => { Config.Save(); };


            /* Camera 3 */
            _savelocationhotKey3 = config("Camera 3", "SaveLocation Hotkey 3", KeyboardShortcut.Empty,
                new ConfigDescription(
                    "The hotkey to save the camera's location.",
                    new AcceptableShortcuts()), false);
            _showcamerahotKey3 = config("Camera 3", "ShowCameraView Hotkey 3", KeyboardShortcut.Empty,
                new ConfigDescription("The hotkey to see through the camera you placed.", new AcceptableShortcuts()),
                false);
            _cameraSavedLocation3 = config("Camera 3", "Camera 3 Location", new Vector3(0, 2, 0),
                new ConfigDescription("The currently saved location of the camera."),
                false);

            _cameraSavedRotation3 = config("Camera 3", "Camera 3 Rotation", new Quaternion(),
                new ConfigDescription("The currently saved rotation of the camera."),
                false);

            _cameraSavedEuler3 = config("Camera 3", "Camera 3 Euler", new Vector3(0, 2, 0),
                new ConfigDescription("The currently saved euler of the camera."),
                false);

            _cameraSavedVectorUp3 = config("Camera 3", "Camera 3 VectorUP", new Vector3(0, 2, 0),
                new ConfigDescription("The currently saved yaw of the camera."),
                false);

            _cameraSavedLocation3.SettingChanged += (_, _) => { Config.Save(); };

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
        }

        private void Update()
        {
            if (!isAdmin) return;

            Transform transform1 = GameCamera.instance.transform;
            /* Camera 1 */
            if (_savelocationhotKey.Value.IsDown() && Player.m_localPlayer.TakeInput())
            {
                CamUpdate(transform1, 1, true);
            }

            if (_showcamerahotKey.Value.IsDown() && Player.m_localPlayer.TakeInput())
            {
                CamUpdate(transform1, 1, false);
            }

            /* Camera 2 */
            if (_savelocationhotKey2.Value.IsDown() && Player.m_localPlayer.TakeInput())
            {
                CamUpdate(transform1, 2, true);
            }

            if (_showcamerahotKey2.Value.IsDown() && Player.m_localPlayer.TakeInput())
            {
                CamUpdate(transform1, 2, false);
            }


            /* Camera 3 */
            if (_savelocationhotKey3.Value.IsDown() && Player.m_localPlayer.TakeInput())
            {
                CamUpdate(transform1, 3, true);
            }

            if (_showcamerahotKey3.Value.IsDown() && Player.m_localPlayer.TakeInput())
            {
                CamUpdate(transform1, 3, false);
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

        private void CamUpdate(Transform camTransform, int cam, bool save)
        {
            string message = save ? "Saving" : "Jumping to";
            string endOfMessage = save ? "information." : "location.";
            Player.m_localPlayer.Message(MessageHud.MessageType.Center,
                $"{message} Camera {cam.ToString()} {endOfMessage}");
            switch (cam)
            {
                case 1 when save:
                    _cameraSavedLocation.Value = camTransform.position;
                    _cameraSavedRotation.Value = camTransform.rotation;
                    _cameraSavedEuler.Value = camTransform.eulerAngles;
                    _cameraSavedVectorUp.Value = camTransform.up;
                    break;
                case 1:
                    GameCamera.instance.m_freeFly = true;
                    camTransform.position = _cameraSavedLocation.Value;
                    camTransform.rotation = _cameraSavedRotation.Value;
                    camTransform.eulerAngles = _cameraSavedEuler.Value;
                    GameCamera.instance.m_freeFlyYaw = _cameraSavedEuler.Value.y;
                    GameCamera.instance.m_freeFlyPitch = _cameraSavedEuler.Value.x;
                    camTransform.up = _cameraSavedVectorUp.Value;
                    break;
                case 2 when save:
                    _cameraSavedLocation2.Value = camTransform.position;
                    _cameraSavedRotation2.Value = camTransform.rotation;
                    _cameraSavedEuler2.Value = camTransform.eulerAngles;
                    _cameraSavedVectorUp2.Value = camTransform.up;
                    break;
                case 2:
                    GameCamera.instance.m_freeFly = true;
                    camTransform.position = _cameraSavedLocation2.Value;
                    camTransform.rotation = _cameraSavedRotation2.Value;
                    camTransform.eulerAngles = _cameraSavedEuler2.Value;
                    GameCamera.instance.m_freeFlyYaw = _cameraSavedEuler2.Value.y;
                    GameCamera.instance.m_freeFlyPitch = _cameraSavedEuler2.Value.x;
                    camTransform.up = _cameraSavedVectorUp2.Value;
                    break;
                case 3 when save:
                    _cameraSavedLocation3.Value = camTransform.position;
                    _cameraSavedRotation3.Value = camTransform.rotation;
                    _cameraSavedEuler3.Value = camTransform.eulerAngles;
                    _cameraSavedVectorUp3.Value = camTransform.up;
                    break;
                case 3:
                    GameCamera.instance.m_freeFly = true;
                    camTransform.position = _cameraSavedLocation3.Value;
                    camTransform.rotation = _cameraSavedRotation3.Value;
                    camTransform.eulerAngles = _cameraSavedEuler3.Value;
                    GameCamera.instance.m_freeFlyYaw = _cameraSavedEuler3.Value.y;
                    GameCamera.instance.m_freeFlyPitch = _cameraSavedEuler3.Value.x;
                    camTransform.up = _cameraSavedVectorUp3.Value;
                    break;
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
                    Transform transform1 = Player.m_localPlayer.transform;
                    Vector3 position = transform1.position;
                    position = new Vector3(position.x,
                        Playerheight, position.z);
                    transform1.position = position;
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
        private static ConfigEntry<KeyboardShortcut> _savelocationhotKey = null!;
        private static ConfigEntry<KeyboardShortcut> _showcamerahotKey = null!;

        private static ConfigEntry<Vector3> _cameraSavedLocation = null!;
        private static ConfigEntry<Quaternion> _cameraSavedRotation = null!;
        private static ConfigEntry<Vector3> _cameraSavedEuler = null!;
        private static ConfigEntry<Vector3> _cameraSavedVectorUp = null!;

        /* Camera 2 */
        private static ConfigEntry<KeyboardShortcut> _savelocationhotKey2 = null!;
        private static ConfigEntry<KeyboardShortcut> _showcamerahotKey2 = null!;

        private static ConfigEntry<Vector3> _cameraSavedLocation2 = null!;
        private static ConfigEntry<Quaternion> _cameraSavedRotation2 = null!;
        private static ConfigEntry<Vector3> _cameraSavedEuler2 = null!;
        private static ConfigEntry<Vector3> _cameraSavedVectorUp2 = null!;

        /* Camera 3 */
        private static ConfigEntry<KeyboardShortcut> _savelocationhotKey3 = null!;
        private static ConfigEntry<KeyboardShortcut> _showcamerahotKey3 = null!;
        private static ConfigEntry<Vector3> _cameraSavedLocation3 = null!;
        private static ConfigEntry<Quaternion> _cameraSavedRotation3 = null!;
        private static ConfigEntry<Vector3> _cameraSavedEuler3 = null!;
        private static ConfigEntry<Vector3> _cameraSavedVectorUp3 = null!;

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