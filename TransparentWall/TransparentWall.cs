using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TransparentWall
{
    public class TransparentWall : MonoBehaviour
    {
        public static int MoveBackLayer = 27;
        public static string LIVCam_Name = "MainCamera";
        private static List<string> _excludedCams = Plugin.ExcludedCams;
        public static List<int> LayersToMask = new List<int> { Plugin.WallLayer, MoveBackLayer };
        public static List<string> livNames = new List<string> { "MenuMainCamera", "MainCamera", "LIV Camera" };

        private BeatmapObjectSpawnController _beatmapObjectSpawnController;

        private void Start()
        {
            if (!Plugin.IsTranparentWall)
            {
                Logger.log.Debug("Transparent wall is disabled!");
                return;
            }
            try
            {
                if (Resources.FindObjectsOfTypeAll<BeatmapObjectSpawnController>().Count() > 0)
                {
                    Logger.log.Debug("Found BeatmapObjectSpawnController!");
                    _beatmapObjectSpawnController = Resources.FindObjectsOfTypeAll<BeatmapObjectSpawnController>().First();
                }
                if (Resources.FindObjectsOfTypeAll<MoveBackWall>().Count() > 0)
                {
                    MoveBackLayer = Resources.FindObjectsOfTypeAll<MoveBackWall>().First().gameObject.layer;
                    Logger.log.Debug("Found MoveBackWall! Using MoveBackLayer=" + MoveBackLayer);
                }
                if (_beatmapObjectSpawnController != null)
                {
                    Logger.log.Debug("Setup obstacleDidStartMovementEvent hook!");
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
            Logger.log.Debug("Destroyed Transparent Wall Handler!");
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

        private IEnumerator setupCamerasCoroutine()
        {
            Logger.log.Debug("Setting up cameras coroutine!");
            yield return new WaitForEndOfFrame();

            StandardLevelScenesTransitionSetupDataSO manager = FindObjectsOfType<StandardLevelScenesTransitionSetupDataSO>().First(x => x != null);
            GameScenesManagerSO.SceneInfoSceneSetupDataPair[] pairs = manager.sceneInfoSceneSetupDataPairs;
            var pair = pairs.First(x => x.data != null);
            GameplayCoreSceneSetupData setupData = (GameplayCoreSceneSetupData)pair.data;

            Camera mainCamera = Camera.main;

            if (Plugin.IsHMDOn && setupData.gameplayModifiers.noFail)
            {
                mainCamera.cullingMask &= ~(1 << Plugin.WallLayer);
            }
            else
            {
                mainCamera.cullingMask |= (1 << Plugin.WallLayer);
            }
            Logger.log.Debug("Setting culling mask to: " + mainCamera.cullingMask);

            try
            {
                FindObjectsOfType<LIV.SDK.Unity.LIV>().Where(x => livNames.Contains(x.name)).ToList().ForEach(l =>
                {
                    if (Plugin.IsLIVCameraOn)
                    {
                        LayersToMask.ForEach(i => 
                        {
                            Logger.log.Debug("Masking layer: " + i + " by converting: " + l.SpectatorLayerMask + " to: " + (l.SpectatorLayerMask & ~(1 << i)));
                            l.SpectatorLayerMask &= ~(1 << i);
                        });
                    }
                });
                FindObjectsOfType<Camera>().Where(c => (c.name.ToLower().EndsWith(".cfg"))).ToList().ForEach(c => {
                    if (_excludedCams.Contains(c.name.ToLower()))
                    {
                        LayersToMask.ForEach(i => 
                        {
                            Logger.log.Debug("Masking layer: " + i + " by converting: " + c.cullingMask + " to: " + (c.cullingMask | (1 << i)));
                            c.cullingMask |= (1 << i);
                        });
                    }
                    else
                    {
                        if (Plugin.IsCameraPlusOn)
                        {
                            LayersToMask.ForEach(i => 
                            {
                                Logger.log.Debug("Masking layer: " + i + " by converting: " + c.cullingMask + " to: " + (c.cullingMask & ~(1 << i)));
                                c.cullingMask &= ~(1 << i);
                            });
                        }
                        else
                        {
                            LayersToMask.ForEach(i => 
                            {
                                Logger.log.Debug("Masking layer: " + i + " by converting: " + c.cullingMask + " to: " + (c.cullingMask | (1 << i)));
                                c.cullingMask |= (1 << i);
                            });
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.log.Error(ex);
            }
            yield break;
        }

        public virtual void HandleObstacleDidStartMovementEvent(BeatmapObjectSpawnController obstacleSpawnController, ObstacleController obstacleController)
        {
            try
            {
                StretchableObstacle _stretchableObstacle = ReflectionUtil.GetPrivateField<StretchableObstacle>(obstacleController, "_stretchableObstacle");
                StretchableCube _stretchableCore = ReflectionUtil.GetPrivateField<StretchableCube>(_stretchableObstacle, "_stretchableCore");
                Transform _obstacleCore = ReflectionUtil.GetPrivateField<Transform>(_stretchableObstacle, "_obstacleCore");
                //MeshRenderer _meshRenderer = ReflectionUtil.GetPrivateField<MeshRenderer>(_stretchableCoreOutside, "_meshRenderer");
                //MeshRenderer _meshRenderer2 = ReflectionUtil.GetPrivateField<MeshRenderer>(_stretchableCoreInside, "_meshRenderer");
                Logger.log.Debug("Setting stretchableCore layer to: " + Plugin.WallLayer + " from: " + _stretchableCore.gameObject.layer);
                Logger.log.Debug("Setting obstacleCore layer to: " + Plugin.WallLayer + " from: " + _obstacleCore.gameObject.layer);
                Logger.log.Debug("ObstacleController has layer: " + obstacleController.gameObject.layer);
                Logger.log.Debug("_stretchableObstacle has layer: " + _stretchableObstacle.gameObject.layer);
                _stretchableCore.gameObject.layer = Plugin.WallLayer;
                _obstacleCore.gameObject.layer = Plugin.WallLayer;
            }
            catch (Exception ex)
            {
                Logger.log.Error(ex);
            }
        }
    }
}
