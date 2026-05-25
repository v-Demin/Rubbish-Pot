using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace RubbishPot.Core
{
    public class GameFlowService: IInitializable, IGameFlowService
    {
        private Queue<Action> _plotSequence = new ();

        public void Initialize()
        {
            _plotSequence.Enqueue(() => StartPlot("Test"));
            _plotSequence.Enqueue(() => StartPlot("Test2"));
            _plotSequence.Enqueue(() => StartPlot("Test3"));

            EventBus.Subscribe<NodeCompletedCommand>(cmd =>
            {
                LoadNext();
            });
        }
        
        public void EntryNext()
        {
            LoadNext();
        }

        private void LoadNext()
        {
            DOVirtual.DelayedCall(0.01f, () =>
            {
                if (_plotSequence.Count > 0)
                {
                    _plotSequence.Dequeue().Invoke();
                }
                else
                {
                    Debug.Log("Scenario completed");
                }
            });
        }
        
        private void StartPlot(string name)
        {
            var plot = PlotFactory.Create(name);
            plot.Execute();
        }
    }

    public interface IGameFlowService
    {
        public void EntryNext();
    }
}
