using System;
using System.Collections.Generic;
using UnityEngine;

namespace RubbishPot.Core
{
    [Serializable]
    public abstract class RuntimeNode 
    {
        public string ID = Guid.NewGuid().ToString();
        public Vector2 Position;
    }
    
    [Serializable] public class RuntimeEntryNode : RuntimeNode { }
    [Serializable] public class RuntimeExitNode : RuntimeNode { }
    [Serializable] public class RuntimePhraseNode : RuntimeNode { public string Text; }
    [Serializable] public class RuntimeAnimationNode : RuntimeNode { public string AnimationName; }
    [Serializable] public class RuntimeGroupNode : RuntimeNode { }
    [Serializable] public class RuntimeParallelNode : RuntimeNode { }
    [Serializable] public class RuntimeChoiceNode : RuntimeNode { public List<string> Choices = new(); }
    [Serializable] public class RuntimeMakeOrderNode : RuntimeNode { public string OrderID; }
    [Serializable] public class RuntimeStorageNode : RuntimeNode { public string DataKey; }
    [Serializable] public class RuntimeHandOverItemNode : RuntimeNode { }
    [Serializable] public class RuntimeCancelOrderNode : RuntimeNode { }
    [Serializable] public class RuntimeAddKnowledgeNode : RuntimeNode { }
}
