using Core;
using Event;
using UnityEngine;
using Resource.ResourceModules;
using System.Collections.Generic;

namespace Data
{
    public enum PhaseType
    {
        Phase1,
        Phase2,
        Phase3,
        Phase4
    }

    public enum PlayerAge
    {
        Child,
        Adult,
        Elder
    }
    
    public partial class DataManager : MonoBehaviour
    {
        //-------------------------------------------------------------------------------------
        // Configs
        //-------------------------------------------------------------------------------------
        
        public int maxPlatformCountPerLayer = 5;
        public int initPlatformCount = 50;

        private readonly Dictionary<PhaseType, PlayerAge> _ages = new() // phase -> age
        {
            { PhaseType.Phase1, PlayerAge.Child },
            { PhaseType.Phase2, PlayerAge.Child },
            { PhaseType.Phase3, PlayerAge.Adult },
            { PhaseType.Phase4, PlayerAge.Elder }
        };

        private readonly Dictionary<PhaseType, MainResource> _aItems = new() // phase -> aItem
        {
            { PhaseType.Phase1, MainResource.AItem1 },
            { PhaseType.Phase2, MainResource.AItem2 },
            { PhaseType.Phase3, MainResource.AItem3 },
            { PhaseType.Phase4, MainResource.None }
        };

        private readonly Dictionary<PhaseType, MainResource> _bItems = new() // phase -> bItem
        {
            { PhaseType.Phase1, MainResource.BItem1 },
            { PhaseType.Phase2, MainResource.BItem2 },
            { PhaseType.Phase3, MainResource.BItem3 },
            { PhaseType.Phase4, MainResource.None }
        };
        
        private readonly Dictionary<PhaseType, int> _phaseRequiredAItemCounts = new()
        {
            { PhaseType.Phase1, 15 },
            { PhaseType.Phase2, 15 },
            { PhaseType.Phase3, 15 },
            { PhaseType.Phase4, -1 }
        };

        private readonly Dictionary<PhaseType, float> _durationDict = new()
        {
            { PhaseType.Phase1, 30 },
            { PhaseType.Phase2, 30 },
            { PhaseType.Phase3, 30 },
            { PhaseType.Phase4, 30 }
        };
        
        
        //-------------------------------------------------------------------------------------
        // Lifecycle
        //-------------------------------------------------------------------------------------
        
        private readonly Dictionary<MainResource, int> _gatheredItemCounts = new();
        private readonly Dictionary<MainResource, bool> _bItemSpawnRecord = new();
        public readonly Dictionary<Vector2Int, MainResource> MapItemDict = new();
        public readonly List<Vector2Int> MapPlatforms = new();
        public readonly List<Vector2Int> ValidPlatforms = new();
        
        private readonly System.Random _random = new();
        
        public bool CanOperate { get; set; }
        public PhaseType CurrentPhase { get; private set; }
        
        public int CurGenerateX { get; set; }
        public int CurGenerateY { get; set; }
        
        public int PlayerPosX { get; set; }
        public int PlayerPosY { get; set; }
        
        public float Timer { get; set; }
        
        public void Init()
        {
            CanOperate = false;
            CurrentPhase = PhaseType.Phase1;
            
            MapItemDict.Clear();
            
            _gatheredItemCounts.Clear();
            
            _bItemSpawnRecord.Clear();
            _bItemSpawnRecord.Add(MainResource.BItem1, false);
            _bItemSpawnRecord.Add(MainResource.BItem2, false);
            _bItemSpawnRecord.Add(MainResource.BItem3, false);
            
            PlayerPosX = maxPlatformCountPerLayer / 2;
            PlayerPosY = 0;
            
            CurGenerateX = maxPlatformCountPerLayer / 2;
            CurGenerateY = 0;
            
            MapPlatforms.Clear();
            MapPlatforms.Add(new Vector2Int(CurGenerateX, CurGenerateY));
            ValidPlatforms.Clear();
            ValidPlatforms.Add(new Vector2Int(CurGenerateX, CurGenerateY));
            
            ExtendMap(initPlatformCount, out _, out _);

            Timer = GetCurPhaseDuration();
        }
        
        public void ExtendMap(int layerCount, out List<Vector2Int> newPlatforms, 
            out Dictionary<Vector2Int, MainResource> newItems)
        {
            newPlatforms = new List<Vector2Int>();
            newItems = new Dictionary<Vector2Int, MainResource>();
            
            for (int i = 0; i < layerCount; i++)
            {
                var addExtraPlatform = false;
                var extraPlatformX = -1;
                
                if (CurGenerateX == 0)
                {
                    CurGenerateX += 1;
                }
                else if (CurGenerateX == maxPlatformCountPerLayer - 1)
                {
                    CurGenerateX -= 1;
                }
                else
                {
                    var generateLeft = _random.Next(0, 2) == 0;
                    var oldX = CurGenerateX;
                    CurGenerateX += generateLeft ? -1 : 1;
                    
                    addExtraPlatform = _random.Next(0, 2) == 0;
                    extraPlatformX = oldX + (generateLeft ? 1 : -1);
                }
                
                CurGenerateY += 1;
                
                var posIndex = new Vector2Int(CurGenerateX, CurGenerateY);
                AddMapPlatform(posIndex);
                newPlatforms.Add(posIndex);
                ValidPlatforms.Add(posIndex);
                
                if (addExtraPlatform)
                {
                    var extraPosIndex = new Vector2Int(extraPlatformX, CurGenerateY);
                    AddMapPlatform(extraPosIndex);
                    newPlatforms.Add(extraPosIndex);

                    if (CurrentPhase != PhaseType.Phase4)
                    {
                        var curAItem = GetCurAItemType();
                        var curBItem = GetCurBItemType();
                        var ifSpawnBItem = false;
                        if (!SpawnedBItem(curBItem))
                        {
                            ifSpawnBItem = _random.Next(0, 8) == 0;

                            if (ifSpawnBItem)
                            {
                                RecordBItemSpawned(curBItem);
                            }
                        }

                        var itemType = ifSpawnBItem ? curBItem : curAItem;
                        AddMapItem(extraPosIndex, itemType);
                        newItems.Add(extraPosIndex, itemType);
                    }
                }
            }
        }
        
        
        //-------------------------------------------------------------------------------------
        // Platform Data
        //-------------------------------------------------------------------------------------
        
        public void AddMapPlatform(Vector2Int posIndex)
        {
            MapPlatforms.Add(posIndex);
        }
        
        public bool HasPlatformData(Vector2Int posIndex)
        {
            return MapPlatforms.Contains(posIndex);
        }

        public bool TryGetValidPosIndex(int layer, out Vector2Int posIndex)
        {
            posIndex = Vector2Int.zero;

            foreach (var index in ValidPlatforms)
            {
                if (index.y == layer)
                {
                    posIndex = index;
                    return true;
                }
            }
            
            return false;
        }
        
        
        //-------------------------------------------------------------------------------------
        // Item Data
        //-------------------------------------------------------------------------------------
        
        public void AddMapItem(Vector2Int posIndex, MainResource itemType)
        {
            MapItemDict.Add(posIndex, itemType);
        }
        
        public void RemoveMapItem(Vector2Int posIndex)
        {
            MapItemDict.Remove(posIndex);
        }
        
        public bool TryGetItemType(Vector2Int posIndex, out MainResource itemType)
        {
            return MapItemDict.TryGetValue(posIndex, out itemType);
        }
        
        private void UpdateMapItemsAccordingToCurPhase()
        {
            if (CurrentPhase == PhaseType.Phase4)
            {
                MapItemDict.Clear();
                return;
            }
            
            var posIndexes = new List<Vector2Int>(MapItemDict.Keys);
            foreach (var posIndex in posIndexes)
            {
                var itemType = MapItemDict[posIndex];
                if (IsAItem(itemType))
                {
                    var curAItem = GetCurAItemType();
                    MapItemDict[posIndex] = curAItem;
                }
                else if (IsBItem(itemType))
                {
                    var curBItem = GetCurBItemType();
                    MapItemDict[posIndex] = curBItem;
                }
            }
        }
        
        public bool SpawnedBItem(MainResource item)
        {
            return _bItemSpawnRecord.TryGetValue(item, out var isSpawned) && isSpawned;
        }
        
        public void RecordBItemSpawned(MainResource item)
        {
            if (_bItemSpawnRecord.ContainsKey(item))
            {
                _bItemSpawnRecord[item] = true;
            }
        }

        public void CollectItem(MainResource resource, int count = 1)
        {
            if (_gatheredItemCounts.ContainsKey(resource))
            {
                _gatheredItemCounts[resource] += count;
            }
            else
            {
                _gatheredItemCounts.Add(resource, count);
            }
            GameCore.Event.DispatchNow(this, EventName.OnItemCollected);

            if (IsBItem(resource))
            {
                GameCore.Event.DispatchNow(this, EventName.OnBItemCollected);   
            }
            
            Debug.LogError($"-----------------------");
            foreach (var pair in _gatheredItemCounts)
            {
                Debug.LogError($"{pair.Key}: {pair.Value}");
            }
        }
        
        public int GetCollectedBItemCount()
        {
            var count = 0;
            foreach (var pair in _gatheredItemCounts)
            {
                if (IsBItem(pair.Key))
                {
                    count += pair.Value;
                }
            }

            return count;
        }

        public MainResource GetCurAItemType()
        {
            var item = MainResource.AItem1;

            foreach (var pair in _aItems)
            {
                var requiredPhase = pair.Key;
                if (CurrentPhase >= requiredPhase)
                {
                    item = pair.Value;
                }
            }

            return item;
        }
        
        public MainResource GetCurBItemType()
        {
            var item = MainResource.BItem1;

            foreach (var pair in _bItems)
            {
                var requiredPhase = pair.Key;
                if (CurrentPhase >= requiredPhase)
                {
                    item = pair.Value;
                }
            }

            return item;
        }
        
        public int GetCurPhaseRequiredAItemCount()
        {
            var count = 15;

            foreach (var pair in _phaseRequiredAItemCounts)
            {
                var requiredPhase = pair.Key;
                if (CurrentPhase >= requiredPhase)
                {
                    count = pair.Value;
                }
            }

            return count;
        }
        
        public int GetGatheredItemCount(MainResource item)
        {
            if (_gatheredItemCounts.TryGetValue(item, out var count))
            {
                return count;
            }

            return 0;
        }

        public bool IsAItem(MainResource item)
        {
            return item != MainResource.None && _aItems.ContainsValue(item);
        }

        public bool IsBItem(MainResource item)
        {
            return item != MainResource.None && _bItems.ContainsValue(item);
        }

        
        //-------------------------------------------------------------------------------------
        // Phase Data
        //-------------------------------------------------------------------------------------
        
        public void TimeOut()
        {
            GameCore.Event.DispatchNow(this, EventName.OnTimeOut);
        }

        public void ChangePhase()
        {
            if (CurrentPhase < PhaseType.Phase4)
            {
                Debug.LogError($"ChangePhase: {CurrentPhase} -> {CurrentPhase + 1}");
                CurrentPhase++;
                Timer = GetCurPhaseDuration();
                UpdateMapItemsAccordingToCurPhase();
                GameCore.Event.DispatchNow(this, EventName.OnPhaseChanged);
            }
            else
            {
                Init();
                GameCore.Event.DispatchNow(this, EventName.OnPhaseChanged);
            }
        }
        
        public float GetCurPhaseDuration()
        {
            var duration = 10f;

            foreach (var pair in _durationDict)
            {
                var requiredPhase = pair.Key;
                if (CurrentPhase >= requiredPhase)
                {
                    duration = pair.Value;
                }
            }

            return duration;
        }
        
        
        //-------------------------------------------------------------------------------------
        // Age Data
        //-------------------------------------------------------------------------------------
        
        public PlayerAge GetCurAgeType()
        {
            var age = PlayerAge.Child;

            foreach (var pair in _ages)
            {
                var requiredPhase = pair.Key;
                if (CurrentPhase >= requiredPhase)
                {
                    age = pair.Value;
                }
            }

            return age;
        }
    }
}