using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "ReactionsConfig", menuName = "Reactions/Config")]
public class ReactionConfig : ScriptableObject
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
            record.Stop(EventBus);
        }
    }

    private void OnValidate()
    {
        foreach (var reactionRecord in _reactions)
        {
            reactionRecord.Validate();
        }
    }

    [System.Serializable]
    private class ReactionRecord
    {
        [SerializeReference] [SubclassSelector] private AbstractCondition _condition;
        [SerializeReference] [SubclassSelector(nameof(ReactionFilter))] private List<AbstractReaction> _reactions = new ();
        private EventBus _localEventBus = new ();
        
        public void Init(EventBus eventBus)
        {
            if (_condition == null) return;
            _condition.Init(_localEventBus);
            _reactions.ForEach(r => _localEventBus.Subscribe(r));
            
            eventBus.Subscribe(_condition);
        }

        public void Validate()
        {
            if (_condition == null) return;
            foreach (var reaction in _reactions)
            {
                reaction?.InformAboutConnection(_condition.Connection);
            }
        }

        private bool ReactionFilter(AbstractReaction reaction)
        {
            if (_condition == null) return false;
            if (reaction == null) return false;
            return (int)reaction.Connection <= (int)_condition.Connection;
        }
        
        public void Stop(EventBus eventBus)
        {
            if (_condition == null) return;
            eventBus.Unsubscribe(_condition);
        }
    }
}