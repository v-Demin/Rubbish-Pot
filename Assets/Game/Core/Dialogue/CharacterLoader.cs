using UnityEngine;
using Zenject;

namespace RubbishPot.Core
{
    public class CharacterLoader : MonoBehaviour
    {
        [Inject] private readonly DiContainer _container;
        [SerializeField] private CharacterPool _pool;
        
        private void OnEnable()
        {
            EventBus.Subscribe<PreloadScenarioResourcesCommand>(OnResourcesRequired);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PreloadScenarioResourcesCommand>(OnResourcesRequired);
        }

        private void OnResourcesRequired(PreloadScenarioResourcesCommand cmd)
        {
            foreach (var loadedCharacterData in cmd.Characters)
            {
                _pool.Load(t => _container.InstantiatePrefabResourceForComponent<Character>(loadedCharacterData.FormId(), t));
            }
        }
    }
}
