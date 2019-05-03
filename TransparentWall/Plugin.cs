using IPA;
using IPA.Config;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;

namespace TransparentWall
{
    class Plugin : IBeatSaberPlugin
    {
        private GameScenesManager _scenesManager;

        public static bool IsTranparentWall
        {
            get => config.Value.TransparentWallEnabled;
            set => config.Value.TransparentWallEnabled = value;
        }

        public static bool IsHMDOn
        {
            get => config.Value.HMD;
            set => config.Value.HMD = value;
        }

        public static bool IsCameraPlusOn
        {
            get => config.Value.CameraPlus;
            set => config.Value.CameraPlus = value;
        }

        public static bool IsLIVCameraOn
        {
            get => config.Value.LIV;
            set => config.Value.LIV = value;
        }

        public static List<string> ExcludedCams
        {
            get => config.Value.ExcludedCams.Split(',').ToList().Select(c => c.ToLower().Trim()).ToList();
            set => config.Value.ExcludedCams = string.Join(",", value);
        }

        internal static Ref<PluginConfig> config;
        internal static IConfigProvider configProvider;

        public void Init(IPALogger logger, [Config.Prefer("json")] IConfigProvider cfgProvider)
        {
            Logger.log = logger;
            configProvider = cfgProvider;

            config = cfgProvider.MakeLink<PluginConfig>((p, v) =>
            {
                if (v.Value == null || v.Value.RegenerateConfig)
                    p.Store(v.Value = new PluginConfig() { RegenerateConfig = false });
                config = v;
            });

        }
        public void OnApplicationStart()
        {
            Logger.log.Debug("OnApplicationStart");
        }

        public void OnApplicationQuit()
        {
            Logger.log.Debug("OnApplicationQuit");
            if (_scenesManager != null)
                _scenesManager.transitionDidFinishEvent -= SceneTransitionDidFinish;
        }

        private void SceneTransitionDidFinish()
        {
            if (SceneManager.GetActiveScene().name == "GameCore")
                new GameObject("TransparentWall").AddComponent<TransparentWall>();
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            
        }

        public void OnSceneUnloaded(Scene scene)
        {
            
        }

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {
            if (_scenesManager == null)
            {
                _scenesManager = Resources.FindObjectsOfTypeAll<GameScenesManager>().FirstOrDefault();

                if (_scenesManager != null)
                    _scenesManager.transitionDidFinishEvent += SceneTransitionDidFinish;
            }
        }

        public void OnUpdate()
        {
            
        }

        public void OnFixedUpdate()
        {
            
        }
    }
}
