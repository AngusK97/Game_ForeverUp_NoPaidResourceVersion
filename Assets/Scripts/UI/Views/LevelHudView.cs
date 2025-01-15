using Core;
using Data;
using Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace UI.Views
{
    public class LevelHudView : UIBase
    {
        [SerializeField] private Button quitBtn;
        [SerializeField] private TextMeshProUGUI itemCount;
        [SerializeField] private TextMeshProUGUI countDown;
        
        private Coroutine _countdownCoroutine;
        private DataManager _dataManager;


        //-------------------------------------------------------------------------------------
        // Lifecycle
        //-------------------------------------------------------------------------------------

        protected override void OnShow()
        {
            _dataManager = GameCore.Data;
            RefreshUI();
            StartCountdown();
            RegisterEvents();
        }

        protected override void OnClose()
        {
            UnregisterEvents();
            StopAllCoroutines();
        }


        //-------------------------------------------------------------------------------------
        // Events
        //-------------------------------------------------------------------------------------

        private void RegisterEvents()
        {
            quitBtn.onClick.AddListener(QuiteGame);
            GameCore.Event.RegisterEvent(EventName.OnItemCollected, RefreshUI);
            GameCore.Event.RegisterEvent(EventName.OnPhaseChanged, OnPhaseChanged);
        }

        private void UnregisterEvents()
        {
            quitBtn.onClick.RemoveListener(QuiteGame);
            GameCore.Event.UnRegisterEvent(EventName.OnItemCollected, RefreshUI);
            GameCore.Event.UnRegisterEvent(EventName.OnPhaseChanged, OnPhaseChanged);
        }

        private void QuiteGame()
        {
            GameCore.Level.BackToMenu();
        }
        
        private void OnPhaseChanged(object sender = null, EventName type = EventName.None, BaseEventArgs eventArgs = null)
        {
            RefreshUI();
            StartCountdown();
        }


        //-------------------------------------------------------------------------------------
        // Refresh UI
        //-------------------------------------------------------------------------------------

        private void RefreshUI(object sender = null, EventName type = EventName.None, BaseEventArgs eventArgs = null)
        {
            RefreshItemCount();
        }

        private void RefreshItemCount()
        {
            if (_dataManager.CurrentPhase == PhaseType.Phase4)
            {
                itemCount.text = $"0/1";
                return;
            }
            
            var aItemType = _dataManager.GetCurAItemType();
            var curAItemCount = _dataManager.GetGatheredItemCount(aItemType);
            var requiredAItemCount = _dataManager.GetCurPhaseRequiredAItemCount();
            itemCount.text = $"{curAItemCount}/{requiredAItemCount}";
        }
        
        private void StartCountdown()
        {
            if (_countdownCoroutine != null)
            {
                StopCoroutine(_countdownCoroutine);
            }

            _countdownCoroutine = StartCoroutine(Countdown());
        }
        
        private System.Collections.IEnumerator Countdown()
        {
            countDown.text = $"{_dataManager.Timer}";
            
            while (_dataManager.Timer > 0)
            {
                while (!_dataManager.CanOperate)
                {
                    yield return null;
                }
                
                yield return new WaitForSeconds(1);
                _dataManager.Timer--;
                RefreshCountdown();
            }
            
            GameCore.Data.TimeOut();
        }

        public void RefreshCountdown()
        {
            countDown.text = $"{_dataManager.Timer}";
        }
    }
}