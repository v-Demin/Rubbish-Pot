using UnityEngine;

namespace RubbishPot.Core
{
    public class CharacterCommandResolver : MonoBehaviour
    {
        [SerializeField] private CharacterPool _pool;
        
        private void OnEnable()
        {
            EventBus.Subscribe<PlayShowAnimationCommand>(OnPlayShowCommand);
            EventBus.Subscribe<PlayEmotionAnimationCommand>(OnPlayEmotionCommand);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayShowAnimationCommand>(OnPlayShowCommand);
            EventBus.Unsubscribe<PlayEmotionAnimationCommand>(OnPlayEmotionCommand);
        }
        
        private void OnPlayShowCommand(PlayShowAnimationCommand cmd)
        {
            _pool.GetCharacter(cmd.CharacterInfo.FormId()).PlayShow(cmd.Animation);
        }

        private void OnPlayEmotionCommand(PlayEmotionAnimationCommand cmd)
        {
            if (cmd.Fast)
            {
                _pool.GetCharacter(cmd.CharacterInfo.FormId()).PlayEmotionFast(cmd.Animation);
            }
            else
            {
                _pool.GetCharacter(cmd.CharacterInfo.FormId()).PlayEmotion(cmd.Animation);
            }
        }
    }
}
