using System;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

[CreateAssetMenu(fileName = "ReactionsConfig", menuName = "Reactions/Config")]
public class ReactionConfig : ScriptableObject
{
    [SerializeField] private List<ReactionRecord> _reactions = new List<ReactionRecord>();
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
    public class ReactionRecord
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
    
    [System.Serializable]

    public abstract class AbstractCondition : ISubscriber
    {
        public event Action<ReactionComponent> OnConditionReached;

        protected void ConditionReached(ReactionComponent target)
        {
            OnConditionReached?.Invoke(target);
        }
    }
    
    public class SpawnedCondition : AbstractCondition, ISpawnHandler
    {
        public void HandleSpawn(ReactionComponent target)
        {
            ConditionReached(target);
        }
    }
    
    public interface ISpawnHandler : ISubscriber
    {
        public void HandleSpawn(ReactionComponent target);
    }
    
    public class CollisionCondition : AbstractCondition, ICollisionHandler
    {
        [SerializeField] private ReactionConfig _collisionConfigToReact;
        
        public void HandleCollision(ReactionComponent target, ReactionComponent other)
        {
            if (_collisionConfigToReact == other.Config)
            {
                ConditionReached(target);
            }
        }
    }
    
    public interface ICollisionHandler : ISubscriber
    {
        public void HandleCollision(ReactionComponent target, ReactionComponent other);
    }
    
    [System.Serializable]
    public abstract class AbstractReaction
    {
        public abstract void Execute(ReactionComponent target);
    }
    
    public class DebugReaction : AbstractReaction
    {
        public override void Execute(ReactionComponent target)
        {
            "Debug successful, condition works fine".Log(Color.green);
        }
    }
}