using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RubbishPot.Core
{

[CreateAssetMenu(fileName = "StateMachineSearchWindowProvider", menuName = "Scriptable Objects/StateMachineSearchWindowProvider")]
public class StateMachineSearchWindowProvider : ScriptableObject, ISearchWindowProvider
{
    private StateMachineGraphView _graphView;
    private Texture2D _indentationIcon;

    public void Init(StateMachineGraphView graphView)
    {
        _graphView = graphView;
        _indentationIcon = new Texture2D(1, 1);
        _indentationIcon.SetPixel(0, 0, Color.clear);
        _indentationIcon.Apply();
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        return new List<SearchTreeEntry>
        {
            new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
            
            new SearchTreeGroupEntry(new GUIContent("Dialogue"), 1),
            new SearchTreeEntry(new GUIContent("Phrase Node", _indentationIcon)) { level = 2, userData = typeof(RuntimePhraseNode) },
            new SearchTreeEntry(new GUIContent("Choice Node", _indentationIcon)) { level = 2, userData = typeof(RuntimeChoiceNode) },
            new SearchTreeEntry(new GUIContent("Animation Node", _indentationIcon)) { level = 2, userData = typeof(RuntimeAnimationNode) },

            new SearchTreeGroupEntry(new GUIContent("Logic & Sync"), 1),
            new SearchTreeEntry(new GUIContent("Parallel Node", _indentationIcon)) { level = 2, userData = typeof(RuntimeParallelNode) },
            new SearchTreeEntry(new GUIContent("Group Node (Sync)", _indentationIcon)) { level = 2, userData = typeof(RuntimeGroupNode) },
            
            new SearchTreeGroupEntry(new GUIContent("Game Mechanics"), 1),
            new SearchTreeEntry(new GUIContent("Make Order Node", _indentationIcon)) { level = 2, userData = typeof(RuntimeMakeOrderNode) },
            new SearchTreeEntry(new GUIContent("Cancel Order Node", _indentationIcon)) { level = 2, userData = typeof(RuntimeCancelOrderNode) },
            new SearchTreeEntry(new GUIContent("Storage Node (DI)", _indentationIcon)) { level = 2, userData = typeof(RuntimeStorageNode) },
            new SearchTreeEntry(new GUIContent("Hand Over Item Node", _indentationIcon)) { level = 2, userData = typeof(RuntimeHandOverItemNode) },
            new SearchTreeEntry(new GUIContent("Add Knowledge Node", _indentationIcon)) { level = 2, userData = typeof(RuntimeAddKnowledgeNode) }
        };
    }

    public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
    {
        var runtimeType = (Type)searchTreeEntry.userData;
        var runtimeNode = (RuntimeNode)Activator.CreateInstance(runtimeType);

        // Фабрика строго маппит рантайм-объекты на их кастомные UI классы
        BaseGraphViewNode uiNode = runtimeNode switch
        {
            RuntimeEntryNode n => new EntryUiNode(n),
            RuntimeExitNode n => new ExitUiNode(n),
            RuntimePhraseNode n => new PhraseUiNode(n),
            RuntimeAnimationNode n => new AnimationUiNode(n),
            RuntimeGroupNode n => new GroupUiNode(n),
            RuntimeParallelNode n => new ParallelUiNode(n),
            RuntimeChoiceNode n => new ChoiceUiNode(n),
            RuntimeMakeOrderNode n => new MakeOrderUiNode(n),
            RuntimeStorageNode n => new StorageUiNode(n),
            RuntimeHandOverItemNode n => new HandOverItemUiNode(n),
            RuntimeCancelOrderNode n => new CancelOrderUiNode(n),
            RuntimeAddKnowledgeNode n => new AddKnowledgeUiNode(n),
            _ => null
        };

        if (uiNode == null) return false;

        var windowRoot = _graphView.panel.visualTree;
        var windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot, context.screenMousePosition - _graphView.Window.position.position);
        var graphMousePosition = _graphView.contentViewContainer.WorldToLocal(windowMousePosition);

        uiNode.SetPosition(new Rect(graphMousePosition, Vector2.zero));
        _graphView.AddElement(uiNode);
        return true;
    }
}
}

