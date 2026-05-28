using System.Linq;
using RubbishPot.Core;
using UnityEngine;

namespace RubbishPot.Screen.Counter
{
    public class ChooseObserver : MonoBehaviour
    {
        [SerializeField] private ChooseUIController _chooseUIController;
        
        private void Start()
        {
            EventBus.Subscribe<ShowPlayerChoicesCommand>(ProceedShowCommand);
        }

        private void ProceedShowCommand(ShowPlayerChoicesCommand command)
        {
            var options = command.Choices.Select((s, i) => new ChooseUIController.ChooseOption(i, s, () => Choose(command.NodeId, i)));
            _chooseUIController.Show(options, _chooseUIController.EnableInput);
        }

        private void Choose(string id, int index)
        {
            _chooseUIController.DisableInput();
            _chooseUIController.PlayHideAnimation(index, () =>
            {
                var choiceHandler = InteractionHub.Get<IChoiceNodeHandler>();
                choiceHandler.SubmitChoice(id, index);
            });
        }
    }
}
