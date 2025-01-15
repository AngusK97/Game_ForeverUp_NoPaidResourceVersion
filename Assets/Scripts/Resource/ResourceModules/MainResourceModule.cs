using System;

namespace Resource.ResourceModules
{
    public enum MainResource
    {
        None,
        
        PlayerObj,
        PlatformObj,
        
        AItem1,
        BItem1,
        AItem2,
        BItem2,
        AItem3,
        BItem3,
        FinalItem,
        
        LevelBgm,
        SpecialBgm,
        
        CollectAItemSound1,
        CollectBItemSound,
        
        JumpSound,
    }

    public class MainResourceModule : BaseResourceModule
    {
        public override ResourceModuleName GetName()
        {
            return ResourceModuleName.Main;
        }

        protected override void RecordAllResourceInfos(Action loadedCallback = null)
        {
            AddResourceInfo(ResourceType.GameObject, (int) MainResource.PlayerObj, "Assets/GameResources/Prefab/Character/Player.prefab");
            AddResourceInfo(ResourceType.GameObject, (int) MainResource.PlatformObj, "Assets/GameResources/Prefab/Platform/Platform.prefab");
            AddResourceInfo(ResourceType.GameObject, (int) MainResource.AItem1, "Assets/GameResources/Prefab/Item/AItem1.prefab");
            AddResourceInfo(ResourceType.GameObject, (int) MainResource.BItem1, "Assets/GameResources/Prefab/Item/BItem1.prefab");
            AddResourceInfo(ResourceType.GameObject, (int) MainResource.AItem2, "Assets/GameResources/Prefab/Item/AItem2.prefab");
            AddResourceInfo(ResourceType.GameObject, (int) MainResource.BItem2, "Assets/GameResources/Prefab/Item/BItem2.prefab");
            AddResourceInfo(ResourceType.GameObject, (int) MainResource.AItem3, "Assets/GameResources/Prefab/Item/AItem3.prefab");
            AddResourceInfo(ResourceType.GameObject, (int) MainResource.BItem3, "Assets/GameResources/Prefab/Item/BItem3.prefab");
            AddResourceInfo(ResourceType.GameObject, (int) MainResource.FinalItem, "Assets/GameResources/Prefab/Item/FinalItem.prefab");
            AddResourceInfo(ResourceType.AudioClip, (int) MainResource.LevelBgm, "Assets/GameResources/Sound/bgm_level.mp3");
            AddResourceInfo(ResourceType.AudioClip, (int) MainResource.SpecialBgm, "Assets/GameResources/Sound/bgm_special.mp3");
            AddResourceInfo(ResourceType.AudioClip, (int) MainResource.CollectAItemSound1, "Assets/GameResources/Sound/collect_a_item1.mp3");
            AddResourceInfo(ResourceType.AudioClip, (int) MainResource.CollectBItemSound, "Assets/GameResources/Sound/collect_b_item.mp3");
            AddResourceInfo(ResourceType.AudioClip, (int) MainResource.JumpSound, "Assets/GameResources/Sound/jump.wav");
        }

        public T GetRes<T>(MainResource name)
        {
            return GetResource<T>((int)name);
        }
    }
}