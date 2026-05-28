using System;
using DG.Tweening;
using RubbishPot.Core;
using UnityEngine;

namespace RubbishPot.Screen.Counter
{
    public class PhraseObserver : MonoBehaviour
    {
        [SerializeField] private PhraseUIBubbleController _ui;
        
        private void Start()
        {
            EventBus.Subscribe<PlayPhraseCommand>(DisplayMessage);
            //[Todo]: дополнительная команда на закрыте интерфейса phrase
        }

        private void DisplayMessage(PlayPhraseCommand command)
        {
            DisplayMessageInner(command.Text, () =>
            {
                DOVirtual.DelayedCall(2f, () =>
                {
                    var phraseHandler = InteractionHub.Get<IPhraseNodeHandler>();
                    phraseHandler?.CompletePhrase(command.NodeId);
                });
            });
        }

        private void DisplayMessageInner(string text, Action onComplete)
        {
            _ui.DisplayText(text, EnableInput, () =>
            {
                EnableEnd();
                onComplete?.Invoke();
            });
        }
        
        private void EnableInput()
        {
            //[Todo]: при инпуте вызвать EnableEnd и форсированное прерывание отображения текста
        }

        private void EnableEnd()
        {
            
        }
    }
}
