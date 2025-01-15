using System.Collections;
using Core;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Views
{
    public class SpecialHudView : UIBase
    {
        [SerializeField] private TypeWriter wisdom;
        [SerializeField] private string wisdomText;
        [SerializeField] private float wisdomDelay = 4f;

        [SerializeField] private CanvasGroup productionInfo;
        [SerializeField] private float productionInfoDelay = 8f;
        [SerializeField] private float productionInfoFadeDuration = 5f;

        [SerializeField] private Button quitBtn;

        protected override void OnInit()
        {
            wisdom.Clear();
            productionInfo.alpha = 0;
        }

        protected override void OnShow()
        {
            StartCoroutine(ShowWisdom());
            StartCoroutine(ShowProductionInfo());
            quitBtn.onClick.AddListener(BackToMenu);
        }

        protected override void OnClose()
        {
            quitBtn.onClick.RemoveListener(BackToMenu);
            StopAllCoroutines();
        }

        private IEnumerator ShowWisdom()
        {
            yield return new WaitForSeconds(wisdomDelay);
            wisdom.ShowText(wisdomText);
        }

        private IEnumerator ShowProductionInfo()
        {
            yield return new WaitForSeconds(productionInfoDelay);
            productionInfo.DOFade(1f, productionInfoFadeDuration);
        }

        private void BackToMenu()
        {
            GameCore.Data.Init();
            GameCore.Level.BackToMenu();
        }
    }
}