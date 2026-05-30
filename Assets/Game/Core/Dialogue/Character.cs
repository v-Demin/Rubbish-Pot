using UnityEngine;

namespace RubbishPot.Core
{
    public class Character : MonoBehaviour
    {
        private static readonly int FEmotion = Animator.StringToHash("f_Emotion");
        private static readonly int TStartTransition = Animator.StringToHash("t_StartTransition");
        private static readonly int FShowingType = Animator.StringToHash("f_ShowingType");
        private static readonly int TShow = Animator.StringToHash("t_Show");

        [SerializeField] private string _id;
        [SerializeField] private string _variant;
        [SerializeField] private Animator _animator;
        [SerializeField] private AnimationEventReceiver _receiver;
        
        public string Id => $"{_id}_{_variant}";

        private bool _shown;
        
        public void PlayShow(ShowType showType)
        {
            _animator.SetFloat(FShowingType, (int)showType);
            _animator.SetTrigger(TShow);
        }

        public void PlayEmotionFast(AnimationState state)
        {
            _animator.SetFloat(FEmotion, (int)state);
        }
        
        public void PlayEmotion(AnimationState state)
        {
            _animator.SetTrigger(TStartTransition);
            _receiver.RegisterEvent("Switch", () => PlayEmotionFast(state));
        }
        
        public enum ShowType
        {
            Instant = 0,
            Alpha = 1
        }
        
        public enum AnimationState
        {
            Smile = 0,
            Shy = 1,
            Thinking = 2,
        }
    }
}
