using System.Collections.Generic;
using NaughtyAttributes;
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
        [SerializeReference] [SubclassSelector] [OnValueChanged(nameof(ValidateCondition))] private AbstractCondition _condition;
        [SerializeReference] [SubclassSelector(nameof(ReactionFilter))] private List<AbstractReaction> _reactions;

        public void Init(EventBus eventBus)
        {
            eventBus.Subscribe(_condition);
            _condition.OnConditionReached += ConditionReached;
        }

        private void ValidateCondition()
        {
            if(_condition.Connection == IConnectinable.ConnectionType.Solo)
                _reactions.RemoveAll(r => r.Connection == IConnectinable.ConnectionType.Duo);
        }

        private bool ReactionFilter(AbstractReaction reaction)
        {
            if (_condition.Connection == IConnectinable.ConnectionType.Ambivalent)
            {
                return true;
            }
            
            if (reaction.Connection == IConnectinable.ConnectionType.Ambivalent)
            {
                return true;
            }

            return _condition.Connection == reaction.Connection;
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