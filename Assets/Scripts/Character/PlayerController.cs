using System;
using System.Collections.Generic;
using Core;
using Data;
using DG.Tweening;
using Event;
using Resource;
using Resource.ResourceModules;
using Sound;
using UnityEngine;

namespace Character
{
    public class PlayerController : MonoBehaviour
    {
        [Serializable]
        public struct PlayerModel
        {
            public PlayerAge age;
            public GameObject model;
        }
        
        public Animator animator;
        public List<PlayerModel> playerModels;
        
        [Header("Jump")]
        public float jumpInterval = 0.5f;
        public float jumpDuration = 0.2f;
        public float jumpPower = 1.5f;
        public float jumpAnimSpeed = 5f;

        [Header("Fall")]
        public Transform leftFallPoint;
        public Transform rightFallPoint;
        public float fallDuration = 0.8f;
        public float fallPower = 1.5f;
        public float fallAnimSpeed = 3f;
        
        [Header("Death")]
        public float dieAnimSpeed = 1f;
        
        [Header("Sound")]
        public float jumpSoundVolume = 0.15f;
    
        private float _jumpTimer;
        private AudioClip _jumpSound;

        
        //-------------------------------------------------------------------------------------
        // Lifecycle
        //-------------------------------------------------------------------------------------
        
        private void Start()
        {
            _jumpSound = GameCore.Resource
                .GetResourceModule<MainResourceModule>(ResourceModuleName.Main)
                .GetRes<AudioClip>(MainResource.JumpSound);
            _jumpTimer = jumpInterval;
            UpdateModel();
            RegisterEvents();
        }

        private void OnDestroy()
        {
            UnRegisterEvents();
        }

        private void Update()
        {
            _jumpTimer += Time.deltaTime;
        }

        
        //-------------------------------------------------------------------------------------
        // Events
        //-------------------------------------------------------------------------------------
        
        private void RegisterEvents()
        {
            GameCore.Event.RegisterEvent(EventName.OnPhaseChanged, UpdateModel);
        }
        
        private void UnRegisterEvents()
        {
            GameCore.Event.UnRegisterEvent(EventName.OnPhaseChanged, UpdateModel);
        }

        
        //-------------------------------------------------------------------------------------
        // Refresh Model
        //-------------------------------------------------------------------------------------
        
        private void UpdateModel(object sender = null, EventName type = EventName.None, BaseEventArgs eventArgs = null)
        {
            var age = GameCore.Data.GetCurAgeType();
            var model = playerModels.Find(m => m.age == age).model;
            playerModels.ForEach(m => m.model.SetActive(m.model == model));
        }

        
        //-------------------------------------------------------------------------------------
        // Actions
        //-------------------------------------------------------------------------------------

        public void Reset()
        {
            animator.Rebind();
            animator.speed = 1f;
            animator.Play("Idle");
        }

        public void Die()
        {
            animator.Rebind();
            animator.speed = dieAnimSpeed;
            animator.Play("Death");
        }
        
        public void Fall(bool left, Action onComplete = null)
        {
            GameCore.Sound.Play(SoundLayer.UI, _jumpSound, jumpSoundVolume);
            var targetPos = left ? leftFallPoint.position : rightFallPoint.position;
            PlayJumpAnim(fallAnimSpeed);
            transform.DOJump(targetPos, fallPower, 1, fallDuration).onComplete += () =>
            {
                onComplete?.Invoke();
            };
        }

        public void Jump(Vector3 targetPos, Action onComplete = null)
        {
            GameCore.Sound.Play(SoundLayer.UI, _jumpSound, jumpSoundVolume);
            ResetCooldown();
            PlayJumpAnim(jumpAnimSpeed);
            transform.DOJump(targetPos, jumpPower, 1, jumpDuration).onComplete += () =>
            {
                onComplete?.Invoke();
            };
        }
        
        private void PlayJumpAnim(float speed = 1f)
        {
            animator.Rebind();
            animator.speed = speed;
            animator.Play("Jump");
        }
    
        private void ResetCooldown()
        {
            _jumpTimer = 0;
        }
        
        public bool InCooldown()
        {
            return _jumpTimer < jumpInterval;
        }

        public Vector3 GetPos()
        {
            return transform.position;
        }
        
        public void SetPos(Vector3 pos)
        {
            transform.position = pos;
        }
    }
}
