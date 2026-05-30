using System;
using System.Collections.Generic;
using Submodules.Common.Tools.SubclassSelector;
using UnityEngine;

namespace RubbishPot.Core
{
    [Serializable]
    public abstract class RuntimeNode 
    {
        public string ID = Guid.NewGuid().ToString();
        public Vector2 Position;
        
        [SubClassSelector] [SerializeReference] public List<ISubCommand> SubCommands = new();
    }
    
    [Serializable] public class RuntimeEntryNode : RuntimeNode { }
    [Serializable] public class RuntimeExitNode : RuntimeNode { }
    [Serializable] public class RuntimePhraseNode : RuntimeNode
    {
        public string SpeakerCharacterID;
        public string SpeakerVariant;
        
        public string ActiveBackgroundVariant;
        
        [TextArea] public string Text;
    }
    
    [Serializable] public class RuntimeChoiceNode : RuntimeNode { public List<string> Choices = new(); }
    [Serializable] public class RuntimeMakeOrderNode : RuntimeNode { public string OrderID; }
    [Serializable] public class RuntimeCancelOrderNode : RuntimeNode { }
}
