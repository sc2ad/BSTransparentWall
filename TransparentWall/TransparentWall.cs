using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TransparentWall
{
    public class TransparentWall : MonoBehaviour
    {
        public static int WallLayer = 25;
        public static int MoveBackLayer = 27;
        public static string LIVCam_Name = "MainCamera";
        private static List<string> _excludedCams = Plugin.ExcludedCams;
        public static List<int> LayersToMask = new List<int> { WallLayer, MoveBackLayer };
        public static List<string> livNames = new List<string> { "MenuMainCamera", "MainCamera", "LIV Camera" };

        private BeatmapObjectSpawnController _beatmapObjectSpawnController;

        private void Start()
        {
            if (!Plugin.IsTranparentWall)
                return;
            try
            {
                if (Resources.FindObjectsOfTypeAll<BeatmapObjectSpawnController>().Count() > 0)
                    _beatmapObjectSpawnController = Resources.FindObjectsOfTypeAll<BeatmapObjectSpawnController>().First();
                if (Resources.FindObjectsOfTypeAll<MoveBackWall>().Count() > 0)
                    MoveBackLayer = Resources.FindObjectsOfTypeAll<MoveBackWall>().First().gameObject.layer;
                if (_beatmapObjectSpawnController != null)
                {
                    _beatmapObjectSpawnController.obstacleDiStartMovementEvent += HandleObstacleDidStartMovementEvent;
                }

                setupCams();
            }
            catch (Exception ex)
            {
                Logger.log.Error(ex);
            }
        }

        private void OnDestroy()
        {
            if (_beatmapObjectSpawnController != null)
            {
                _beatmapObjectSpawnController.obstacleDiStartMovementEvent -= HandleObstacleDidStartMovementEvent;
            }
        }
        private void setupCams()
        {
            _excludedCams = Plugin.ExcludedCams;
            StartCoroutine(setupCamerasCoroutine());
        }

        private IEnumerator<WaitForEndOfFrame> setupCamerasCoroutine()
        {
            yield return new WaitForEndOfFrame();

            StandardLevelScenesTransitionSetupDataSO manager = FindObjectsOfType<StandardLevelScenesTransitionSetupDataSO>().First(x => x != null);
            GameScenesManagerSO.SceneInfoSceneSetupDataPair[] pairs = manager.sceneInfoSceneSetupDataPairs;
            var pair = pairs.First(x => x.data != null);
            GameplayCoreSceneSetupData setupData = (GameplayCoreSceneSetupData)pair.data;

            Camera mainCamera = Camera.main;

            if (Plugin.IsHMDOn && setupData.gameplayModifiers.noFail)
                mainCamera.cullingMask &= ~(1 << WallLayer);
            else
                mainCamera.cullingMask |= (1 << WallLayer);

            try
            {
                FindObjectsOfType<LIV.SDK.Unity.LIV>().Where(x => livNames.Contains(x.name)).ToList().ForEach(l =>
                {
                    if(Plugin.IsLIVCameraOn)
                        LayersToMask.ForEach(i => { l.SpectatorLayerMask &= ~(1 << i); });
                });
                FindObjectsOfType<Camera>().Where(c => (c.name.ToLower().EndsWith(".cfg"))).ToList().ForEach(c => {
                    if (_excludedCams.Contains(c.name.ToLower()))
                        LayersToMask.ForEach(i => { c.cullingMask |= (1 << i); });
                    else
                    {
                        if (Plugin.IsCameraPlusOn)
                            LayersToMask.ForEach(i => { c.cullingMask &= ~(1 << i); });
                        else
                            LayersToMask.ForEach(i => { c.cullingMask |= (1 << i); });
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.log.Error(ex);
            }
        }

        public virtual void HandleObstacleDidStartMovementEvent(BeatmapObjectSpawnController obstacleSpawnController, ObstacleController obstacleController)
        {
            try
            {
                StretchableObstacle _stretchableObstacle = ReflectionUtil.GetPrivateField<StretchableObstacle>(obstacleController, "_stretchableObstacle");
                StretchableCube _stretchableCoreOutside = ReflectionUtil.GetPrivateField<StretchableCube>(_stretchableObstacle, "_stretchableCore");
                //MeshRenderer _meshRenderer = ReflectionUtil.GetPrivateField<MeshRenderer>(_stretchableCoreOutside, "_meshRenderer");
                //MeshRenderer _meshRenderer2 = ReflectionUtil.GetPrivateField<MeshRenderer>(_stretchableCoreInside, "_meshRenderer");
                _stretchableCoreOutside.gameObject.layer = WallLayer;
            }
            catch (Exception ex)
            {
                Logger.log.Error(ex);
            }
        }
    }
}
