using Core;
using System;
using DG.Tweening;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace UI.Views
{
    public enum PlotType
    {
        Starting,
        Ending,
        Final
    }

    public enum EndingType
    {
        Success,
        Fail,
        Special
    }
    
    public class PlotView : UIBase
    {
        public class Args
        {
            public PlotType PlotType;
            public EndingType EndingType;
            public Action OnComplete;
        }
        
        [SerializeField] private CanvasGroup startingPlotCanvasGroup;
        [SerializeField] private CanvasGroup endingPlotCanvasGroup;
        [SerializeField] private CanvasGroup finalPlotCanvasGroup;
        
        private Args _args;
        private Coroutine _coroutine;
        
        /*
         * 有几种剧情展示流程？
         * 1、起始剧情：title 打印，随后 description 打印
         * 2、结束剧情：各个 TypeWriter 间隔一点时间依次开始打印
         * 3、最终剧情：每个 TypeWriter 逐个打印，期间间隔一段时间
         */
        

        //-------------------------------------------------------------------------------------
        // Lifecycle
        //-------------------------------------------------------------------------------------

        protected override void OnInit()
        {
            startingPlotCanvasGroup.alpha = 0;
            endingPlotCanvasGroup.alpha = 0;
            finalPlotCanvasGroup.alpha = 0;
            
            title.Clear();
            description.Clear();
            
            foreach (var typeWriter in endingTypeWriters)
            {
                typeWriter.Clear();
            }
            
            foreach (var typeWriter in finalTypeWriters)
            {
                typeWriter.Clear();
            }
        }

        protected override void OnShow()
        {
            _args = (Args)Data;
            
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
            }

            switch (_args.PlotType)
            {
                case PlotType.Starting:
                    _coroutine = StartCoroutine(ShowStartingPlot());
                    break;
                case PlotType.Ending:
                    _coroutine = StartCoroutine(ShowEndingPlot());
                    break;
                case PlotType.Final:
                    _coroutine = StartCoroutine(ShowFinalPlot());
                    break;
            }
        }

        protected override void OnClose()
        {
            StopAllCoroutines();
        }
        
        
        //-------------------------------------------------------------------------------------
        // Starting Plot
        //-------------------------------------------------------------------------------------

        [Header("Starting Plot")]
        [SerializeField] private TypeWriter title;
        [SerializeField] private TypeWriter description;
        [SerializeField] private float startDelay = 1.0f;
        [SerializeField] private float descriptionDelay = 0.5f;
        [SerializeField] private float startingCompleteDelay = 1.0f;
        
        private IEnumerator ShowStartingPlot()
        {
            var storyData = GameCore.Data.StoryDataDict[GameCore.Data.CurrentPhase];
            
            yield return new WaitForSeconds(startDelay);
            startingPlotCanvasGroup.alpha = 1f;
            title.ShowText(storyData.Title);
            yield return new WaitForSeconds(descriptionDelay);
            description.ShowText(storyData.Description);
            yield return new WaitForSeconds(startingCompleteDelay);
            _args.OnComplete?.Invoke();
        }
        
        
        //-------------------------------------------------------------------------------------
        // Ending Plot
        //-------------------------------------------------------------------------------------

        [Header("Ending Plot")]
        [SerializeField] private List<TypeWriter> endingTypeWriters;
        [SerializeField] private float endingTypeWriterInterval = 0.3f;
        [SerializeField] private float endingCompleteDelay = 1.0f;
        [SerializeField] private float endingPlotFadeDuration = 0.5f;

        private IEnumerator ShowEndingPlot()
        {
            var storyData = GameCore.Data.StoryDataDict[GameCore.Data.CurrentPhase];
            
            endingPlotCanvasGroup.alpha = 1f;
            
            var comments = _args.EndingType switch
            {
                EndingType.Success => storyData.SuccessComments,
                EndingType.Fail => storyData.FailComments,
                EndingType.Special => storyData.SpecialComments,
                _ => throw new ArgumentOutOfRangeException()
            };

            for (var i = 0; i < comments.Count; i++)
            {
                var commentStr = comments[i];
                endingTypeWriters[i].ShowText(commentStr);
                yield return new WaitForSeconds(endingTypeWriterInterval);
            }
            
            yield return new WaitForSeconds(endingCompleteDelay);
            GameCore.Data.ChangePhase();

            endingPlotCanvasGroup.DOFade(0f, endingPlotFadeDuration);
            yield return new WaitForSeconds(endingPlotFadeDuration);

            yield return ShowStartingPlot();
        }
        
        
        //-------------------------------------------------------------------------------------
        // Final Plot
        //-------------------------------------------------------------------------------------
        
        [Header("Final Plot")]
        [SerializeField] private List<TypeWriter> finalTypeWriters;
        [SerializeField] private float finalTypeWriterInterval = 1.0f;
        [SerializeField] private float finalCompleteDelay = 1.0f;
        [SerializeField] private float finalPlotFadeDuration = 0.5f;
        
        private IEnumerator ShowFinalPlot()
        {
            var storyData = GameCore.Data.StoryDataDict[GameCore.Data.CurrentPhase];
            
            finalPlotCanvasGroup.alpha = 1f;
            
            for (var i = 0; i < finalTypeWriters.Count; i++)
            {
                var commentStr = storyData.FailComments[i];
                yield return finalTypeWriters[i].ShowText(commentStr);
                yield return new WaitForSeconds(finalTypeWriterInterval);
            }
            
            yield return new WaitForSeconds(finalCompleteDelay);
            GameCore.Data.ChangePhase();
            
            finalPlotCanvasGroup.DOFade(0f, finalPlotFadeDuration);
            yield return new WaitForSeconds(finalPlotFadeDuration);

            yield return ShowStartingPlot();
        }
    }
}