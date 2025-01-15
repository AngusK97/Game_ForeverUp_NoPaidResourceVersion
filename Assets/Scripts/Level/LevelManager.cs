using System.Collections;
using UI;
using Core;
using Data;
using Event;
using Sound;
using Resource;
using UI.Views;
using UnityEngine;
using DG.Tweening;
using Resource.ResourceModules;
using System.Collections.Generic;
using Character;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace Level
{
    public class LevelManager : MonoBehaviour
    {
        [Header("Scene")]
        public Material normalSkyboxMat;
        public Material finalSkyboxMat;
        
        [Header("Platform")]
        public float layerDistance = 1.5f;
        public float platformDistance = 1.5f;

        public int dynamicGenerateCount = 20;
        public int dynamicGenerateThreshold = 10;
        
        [Header("Item")]
        public float itemPosYOffset = 0.3f;
        public float hideItemDelay = 0.1f;
        public float hideItemDuration = 0.5f;
        public int finalItemDistance = 4;

        [Header("Camera")]
        public float cameraDistance = 10f;
        public float cameraPosYOffset = 3f;
        public float cameraMoveRate = 0.1f;
        
        [Header("Sound")]
        public float bgmVolume = 0.5f;
        public float specialBgmVolume = 0.5f;
        public float itemVolume = 0.5f;
        public float itemSoundDelay = 0.2f;
        
        [Header("Player Death")]
        public float playerDieDuration = 1.5f;
        
        private readonly Dictionary<Vector2Int, Transform> _platformDict = new();
        private readonly Dictionary<Vector2Int, Transform> _itemDict = new();
        private readonly System.Random _random = new();
        
        private bool _initiated;
        private Camera _mainCamera;
        private FinalItem _finalItem;
        private GameObject _cityGo;
        private GameObject _aItemPrefab;
        private GameObject _bItemPrefab;
        private GameObject _platformPrefab;
        private DataManager _dataManager;
        private LevelHudView _hudView;
        private PlayerController _playerController;
        private MainResourceModule _mainResourceModule;

        
        //-------------------------------------------------------------------------------------
        // Lifecycle
        //-------------------------------------------------------------------------------------
        
        public void EnterLevel()
        {
            GameCore.UI.TransitionFadeOut(() =>
            {
                GameCore.Scene.LoadSceneAsync("Assets/GameResources/Scenes/LevelScene.scene").Completed += handle =>
                {
                    if (handle.Status != AsyncOperationStatus.Succeeded)
                    {
                        return;
                    }

                    GameCore.UI.CloseAll();
                    GameCore.UI.OpenUI(UILayer.First, UIName.LevelHud, null, hudView =>
                    {
                        _hudView = hudView as LevelHudView;
                        InitLevel();

                        var args = new PlotView.Args
                        {
                            PlotType = PlotType.Starting,
                            OnComplete = () =>
                            {
                                GameCore.UI.TransitionFadeIn(() =>
                                {
                                    GameCore.UI.CloseUI(UIName.Plot);
                                    _dataManager.CanOperate = true;
                                });
                            },
                        };
                        GameCore.UI.OpenUI(UILayer.Transition, UIName.Plot, args);
                    });
                };
            });
        }

        public void BackToMenu()
        {
            DeinitLevel();
            
            GameCore.UI.TransitionFadeOut(() =>
            {
                GameCore.Scene.LoadSceneAsync("Assets/GameResources/Scenes/MenuScene.scene").Completed += handle =>
                {
                    if (handle.Status != AsyncOperationStatus.Succeeded)
                    {
                        return;
                    }

                    GameCore.UI.CloseAll();
                    GameCore.UI.OpenUI(UILayer.First, UIName.Menu, null, _ =>
                    {
                        GameCore.UI.TransitionFadeIn();
                    });
                };
            });
        }

        public void EnterSpecialLevel()
        {
            DeinitLevel();
            
            GameCore.UI.TransitionFadeOut(() =>
            {
                var specialBgm = _mainResourceModule.GetRes<AudioClip>(MainResource.SpecialBgm);
                GameCore.Sound.Play(SoundLayer.Background, specialBgm, specialBgmVolume, 1f, true);
                
                GameCore.Scene.LoadSceneAsync("Assets/GameResources/Scenes/SpecialScene.scene").Completed += handle =>
                {
                    if (handle.Status != AsyncOperationStatus.Succeeded)
                    {
                        return;
                    }

                    GameCore.UI.CloseAll();
                    GameCore.UI.OpenUI(UILayer.First, UIName.SpecialHud, null, _ =>
                    {
                        GameCore.UI.TransitionFadeIn();
                    });
                };
            });
        }
        
        private void InitLevel()
        {
            _dataManager = GameCore.Data;
            _mainResourceModule = GameCore.Resource.GetResourceModule<MainResourceModule>(ResourceModuleName.Main);
            
            InitPlatformObjs();
            InitItemObjs();
            InitPlayer();
            InitFinalItem();
            InitCamera();
            UpdateSkyBox();
            UpdateBackground();
            PlayBgm();
            
            RegisterEvents();
            
            _initiated = true;
        }
        
        private void InitPlatformObjs()
        {
            _platformDict.Clear();
            _platformPrefab = _mainResourceModule.GetRes<GameObject>(MainResource.PlatformObj);
            CreatePlatforms(_dataManager.MapPlatforms);
        }

        private void InitItemObjs()
        {
            _itemDict.Clear();

            var curAItem = _dataManager.GetCurAItemType();
            var curBItem = _dataManager.GetCurBItemType();
            _aItemPrefab = _mainResourceModule.GetRes<GameObject>(curAItem);
            _bItemPrefab = _mainResourceModule.GetRes<GameObject>(curBItem);
            
            CreateItems(_dataManager.MapItemDict);
        }
        
        private void InitPlayer()
        {
            var resourceModule = GameCore.Resource.GetResourceModule<MainResourceModule>(ResourceModuleName.Main);
            var playerPrefab = resourceModule.GetRes<GameObject>(MainResource.PlayerObj);
            var playerObj = Instantiate(playerPrefab);
            _playerController = playerObj.GetComponent<PlayerController>();

            var position = GetPlatformPosition(_dataManager.PlayerPosX, _dataManager.PlayerPosY);
            _playerController.SetPos(position);
        }

        private void InitFinalItem()
        {
            var resourceModule = GameCore.Resource.GetResourceModule<MainResourceModule>(ResourceModuleName.Main);
            var finalItemPrefab = resourceModule.GetRes<GameObject>(MainResource.FinalItem);
            _finalItem = Instantiate(finalItemPrefab).GetComponent<FinalItem>();
            _finalItem.gameObject.SetActive(_dataManager.CurrentPhase == PhaseType.Phase4);
        }

        private void InitCamera()
        {
            _mainCamera = Camera.main;
            
            var playerPos = _playerController.GetPos();
            var targetPos = new Vector3(playerPos.x, playerPos.y, -cameraDistance);
            _mainCamera.transform.position = targetPos;
        }

        private void DeinitLevel()
        {
            _initiated = false;
            UnregisterEvents();
            StopAllCoroutines();
            StopBgm();
        }
        
        private void UpdateSkyBox()
        {
            RenderSettings.skybox = _dataManager.CurrentPhase == PhaseType.Phase4 ? finalSkyboxMat : normalSkyboxMat;
        }
        
        private void UpdateBackground()
        {
            if (_cityGo == null)
            {
                _cityGo = GameObject.Find("City");
            }
            _cityGo.SetActive(_dataManager.CurrentPhase != PhaseType.Phase4);
        }

        private void PlayBgm()
        {
            var bgmClip = _mainResourceModule.GetRes<AudioClip>(MainResource.LevelBgm);
            GameCore.Sound.Play(SoundLayer.Background, bgmClip, bgmVolume, 1f, true);
        }
        
        private void StopBgm()
        {
            GameCore.Sound.StopSound(SoundLayer.Background);
        }
        
        
        //-------------------------------------------------------------------------------------
        // Events
        //-------------------------------------------------------------------------------------

        private void RegisterEvents()
        {
            GameCore.Event.RegisterEvent(EventName.OnTimeOut, OnTimeOut);
            GameCore.Event.RegisterEvent(EventName.OnPhaseChanged, OnPhaseChanged);
            GameCore.Event.RegisterEvent(EventName.OnBItemCollected, OnCollectBItem);
        }
        
        private void UnregisterEvents()
        {
            GameCore.Event.UnRegisterEvent(EventName.OnTimeOut, OnTimeOut);
            GameCore.Event.UnRegisterEvent(EventName.OnPhaseChanged, OnPhaseChanged);
            GameCore.Event.UnRegisterEvent(EventName.OnBItemCollected, OnCollectBItem);
        }

        private void OnTimeOut(object sender, EventName type, BaseEventArgs eventargs)
        {
            StartCoroutine(OnTimeOut());
        }

        private IEnumerator OnTimeOut()
        {
            _dataManager.CanOperate = false;
            
            var isLastPhase = _dataManager.CurrentPhase == PhaseType.Phase4;
            if (isLastPhase)
            {
                _playerController.Die();
                yield return new WaitForSeconds(playerDieDuration);
            }

            var aItemType = _dataManager.GetCurAItemType();
            var curAItemCount = _dataManager.GetGatheredItemCount(aItemType);
            var requiredAItemCount = _dataManager.GetCurPhaseRequiredAItemCount();
            var collectedEnoughAItems = curAItemCount >= requiredAItemCount;
            
            GameCore.UI.TransitionFadeOut(() =>
            {
                var args = new PlotView.Args
                {
                    PlotType = isLastPhase ? PlotType.Final : PlotType.Ending,
                    EndingType = collectedEnoughAItems ? EndingType.Success : EndingType.Fail,
                    OnComplete = () =>
                    {
                        GameCore.UI.TransitionFadeIn(() =>
                        {
                            GameCore.UI.CloseUI(UIName.Plot);
                            _dataManager.CanOperate = true;
                        });
                    },
                };
                GameCore.UI.OpenUI(UILayer.Transition, UIName.Plot, args);
            });
        }
        
        private void OnCollectBItem(object sender, EventName type, BaseEventArgs eventargs)
        {
            _dataManager.CanOperate = false;
            
            if (_dataManager.GetCollectedBItemCount() >= 3)
            {
                EnterSpecialLevel();
                return;
            }
            
            GameCore.UI.TransitionFadeOut(() =>
            {
                var args = new PlotView.Args
                {
                    PlotType = PlotType.Ending,
                    EndingType = EndingType.Special,
                    OnComplete = () =>
                    {
                        GameCore.UI.TransitionFadeIn(() =>
                        {
                            GameCore.UI.CloseUI(UIName.Plot);
                            _dataManager.CanOperate = true;
                        });
                    },
                };
                GameCore.UI.OpenUI(UILayer.Transition, UIName.Plot, args);
            });
        }

        private void OnPlayerFall(bool jumpLeft)
        {
            _dataManager.CanOperate = false;
            _playerController.Fall(jumpLeft, () =>
            {
                var prevLayer = _dataManager.PlayerPosY;
                var newLater = prevLayer - 1 < 0 ? 0 : prevLayer - 1;
                _dataManager.TryGetValidPosIndex(newLater, out var newPosIndex);
                _dataManager.PlayerPosX = newPosIndex.x;
                _dataManager.PlayerPosY = newPosIndex.y;
                var newPos = GetPlatformPosition(newPosIndex.x, newPosIndex.y);
                _playerController.SetPos(newPos);

                _dataManager.Timer -= 3f;
                _hudView.RefreshCountdown();
                    
                _dataManager.CanOperate = true;
            });
        }
        
        private void OnPhaseChanged(object sender, EventName type, BaseEventArgs eventargs)
        {
            _finalItem.gameObject.SetActive(false);
            _playerController.Reset();
            UpdateSkyBox();
            UpdateBackground();
            
            if (_dataManager.CurrentPhase == PhaseType.Phase1)
            {
                // reset platforms
                foreach (var pair in _platformDict)
                {
                    var platformTransform = pair.Value;
                    platformTransform.gameObject.SetActive(false);
                }
                InitPlatformObjs();

                // reset items
                foreach (var pair in _itemDict)
                {
                    var itemTransform = pair.Value;
                    itemTransform.gameObject.SetActive(false);
                }
                InitItemObjs();
                
                // reset player
                var position = GetPlatformPosition(_dataManager.PlayerPosX, _dataManager.PlayerPosY);
                _playerController.SetPos(position);
                
                // reset camera
                var playerPos = _playerController.GetPos();
                var targetPos = new Vector3(playerPos.x, playerPos.y, -cameraDistance);
                _mainCamera.transform.position = targetPos;
            }
            else if (_dataManager.CurrentPhase == PhaseType.Phase4)
            {
                _finalItem.gameObject.SetActive(true);
                _dataManager.TryGetValidPosIndex(_dataManager.PlayerPosY + finalItemDistance, out var validPosIndex);
                var position = GetPlatformPosition(validPosIndex.x, validPosIndex.y);
                _finalItem.SetPos(position);
                
                foreach (var pair in _itemDict)
                {
                    var itemTransform = pair.Value;
                    itemTransform.gameObject.SetActive(false);
                }
            }
            else
            {
                // reset items
                foreach (var pair in _itemDict)
                {
                    var itemTransform = pair.Value;
                    itemTransform.gameObject.SetActive(false);
                }
                InitItemObjs();
            }
        }


        //-------------------------------------------------------------------------------------
        // Dynamic Generate
        //-------------------------------------------------------------------------------------
        
        private void ExtendMap(int layerCount)
        {
            _dataManager.ExtendMap(layerCount, out var newPlatforms, out var newItems);
            CreatePlatforms(newPlatforms);
            CreateItems(newItems);
        }
        
        private void CreatePlatforms(List<Vector2Int> newPlatforms)
        {
            foreach (var posIndex in newPlatforms)
            {
                var position = new Vector3(posIndex.x * platformDistance, posIndex.y * layerDistance, 0);
                var platform = Instantiate(_platformPrefab, position, Quaternion.identity);
                _platformDict.Add(posIndex, platform.transform);
            }
        }

        private void CreateItems(Dictionary<Vector2Int, MainResource> newItems)
        {
            foreach (var pair in newItems)
            {
                var posIndex = pair.Key;
                var itemType = pair.Value;
                var itemPrefab = _dataManager.IsAItem(itemType) ? _aItemPrefab : _bItemPrefab;
                var position = new Vector3(posIndex.x * platformDistance, posIndex.y * layerDistance + itemPosYOffset, 0);
                var item = Instantiate(itemPrefab, position, Quaternion.identity);
                _itemDict.Add(posIndex, item.transform);
            }
        }
        
        
        //-------------------------------------------------------------------------------------
        // Update
        //-------------------------------------------------------------------------------------
        
        public void OnUpdate(float deltaTime, float unscaledDeltaTime)
        {
            if (!_initiated)
            {
                return;
            }
            
            UpdateJump();
        }
        
        public void OnFixedUpdate()
        {
            if (!_initiated)
            {
                return;
            }
            
            UpdateCamera();
        }

        private void UpdateJump()
        {
            if (!_dataManager.CanOperate)
            {
                return;
            }
            
            if (_playerController.InCooldown())
            {
                return;
            }

            var jumpLeft = false;
            var jumpRight = false;
            var targetPosIndex = new Vector2Int(-1, -1);
            
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                jumpLeft = true;
                targetPosIndex = new Vector2Int(_dataManager.PlayerPosX - 1, _dataManager.PlayerPosY + 1);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                jumpRight = true;
                targetPosIndex = new Vector2Int(_dataManager.PlayerPosX + 1, _dataManager.PlayerPosY + 1);
            }
            
            if (!jumpLeft && !jumpRight)
            {
                return;
            }
            
            if (IsPlatform(targetPosIndex))
            {
                // player jump
                _dataManager.PlayerPosX = targetPosIndex.x;
                _dataManager.PlayerPosY = targetPosIndex.y;
                var targetPlatformPos = GetPlatformPosition(_dataManager.PlayerPosX, _dataManager.PlayerPosY);
                _playerController.Jump(targetPlatformPos);

                // extend map
                if (_dataManager.PlayerPosY >= _dataManager.CurGenerateY - dynamicGenerateThreshold)
                {
                    ExtendMap(dynamicGenerateCount);
                }
                
                // move final item
                if (_dataManager.CurrentPhase == PhaseType.Phase4)
                {
                    _dataManager.TryGetValidPosIndex(_dataManager.PlayerPosY + finalItemDistance, out var validPosIndex);
                    var validPos = GetPlatformPosition(validPosIndex.x, validPosIndex.y);
                    _finalItem.Move(validPos);
                }
                
                // collect item
                if (_dataManager.TryGetItemType(targetPosIndex, out var itemType))
                {
                    _dataManager.CollectItem(itemType);
                    _dataManager.RemoveMapItem(targetPosIndex);
                    
                    var itemTransform = _itemDict[targetPosIndex];
                    PlayItemDisappearAnim(itemTransform);
                    _itemDict.Remove(targetPosIndex);

                    StartCoroutine(PlayCollectItemSound(itemType));
                }
            }
            else
            {
                OnPlayerFall(jumpLeft);
            }
        }

        private void UpdateCamera()
        {
            if (!_dataManager.CanOperate)
            {
                return;
            }
            
            var mainCameraTransform = _mainCamera.transform;
            var cameraPos = mainCameraTransform.position;
            var playerPos = _playerController.GetPos();
            var targetPos = new Vector3(cameraPos.x, playerPos.y + cameraPosYOffset, -cameraDistance);
            mainCameraTransform.position = Vector3.Lerp(cameraPos, targetPos, cameraMoveRate);
        }

        private Vector3 GetPlatformPosition(int x, int y)
        {
            if (_platformDict.TryGetValue(new Vector2Int(x, y), out var platform))
            {
                return platform.position;
            }

            return Vector3.zero;
        }

        private bool IsPlatform(Vector2Int posIndex)
        {
            if (posIndex.x < 0 || posIndex.x >= _dataManager.maxPlatformCountPerLayer || posIndex.y < 0)
            {
                return false;
            }
            
            return _dataManager.HasPlatformData(posIndex);
        }
        
        private void PlayItemDisappearAnim(Transform itemTransform)
        {
            itemTransform.DOScale(Vector3.zero, hideItemDuration)
                .SetDelay(hideItemDelay)
                .SetEase(Ease.InOutBack)
                .OnComplete(() =>
                {
                    itemTransform.gameObject.SetActive(false);
                });
        }

        private IEnumerator PlayCollectItemSound(MainResource resource)
        { 
            AudioClip soundClip;
            if (_dataManager.IsAItem(resource))
            {
                var aItemSound1 = _mainResourceModule.GetRes<AudioClip>(MainResource.CollectAItemSound1);
                soundClip = aItemSound1;
            }
            else
            {
                var bItemSound = _mainResourceModule.GetRes<AudioClip>(MainResource.CollectBItemSound);
                soundClip = bItemSound;
            }
            
            yield return new WaitForSeconds(itemSoundDelay);
            
            GameCore.Sound.Play(SoundLayer.UI, soundClip, itemVolume);
        }
    }
}
