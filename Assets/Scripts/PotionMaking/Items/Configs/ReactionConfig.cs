using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ReactionsConfig", menuName = "Reactions/Config")]
public partial class ReactionConfig : ScriptableObject
{
    [SerializeField] private List<ReactionRecord> _reactions = new();
    public EventBus EventBus { get; } = new EventBus();

    private void OnEnable()
    {
        foreach (var record in _reactions)
        {
            record.Init(EventBus);
        }
    }

    private void OnDisable()
    {
        foreach (var record in _reactions)
        {
            record.Init(EventBus);
        }
    }

    [System.Serializable]
    private class ReactionRecord
    {
        [SerializeReference] [SubclassSelector] private AbstractCondition _condition;
        [SerializeReference] [SubclassSelector] private List<AbstractReaction> _reactions;

        public void Init(EventBus eventBus)
        {
            eventBus.Subscribe(_condition);
            _condition.OnConditionReached += ConditionReached;
        }
        
        public void Stop(EventBus eventBus)
        {
            eventBus.Unsubscribe(_condition);
            _condition.OnConditionReached += ConditionReached;
        }

        public void ConditionReached(ReactionComponent target)
        {
            foreach (var reaction in _reactions)
            {
                reaction.Execute(target);
            }
        }
    }
}