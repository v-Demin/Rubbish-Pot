using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RubbishPot.Core
{
    public class SubStateGraphView : GraphView
    {
        public StateMachineEditorWindow Window { get; private set; }
        private RuntimeSubState _activeSubState;
        private VisualElement _placeholderContainer;

        public SubStateGraphView(StateMachineEditorWindow window)
        {
            Window = window;

            Insert(0, new GridBackground());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            InitPlaceholderNotification();

            // Перехват изменений для сохранения Edges
            graphViewChanged = OnGraphViewChanged;

            nodeCreationRequest = context =>
            {
                if (_activeSubState == null)
                {
                    EditorUtility.DisplayDialog("Внимание", "Сначала выберите Сабстейт в верхней ноде!", "ОК");
                    return;
                }

                var provider = ScriptableObject.CreateInstance<SubStateSearchWindowProvider>();
                provider.Init(this, _activeSubState);
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), provider);
            };
        }

        private void InitPlaceholderNotification()
        {
            _placeholderContainer = new VisualElement
            {
                style = { position = Position.Absolute, width = new Length(100, LengthUnit.Percent), height = new Length(100, LengthUnit.Percent), justifyContent = Justify.Center, alignItems = Align.Center }
            };
            _placeholderContainer.pickingMode = PickingMode.Ignore;
            var label = new Label("Граф пуст. Нажмите на подсостояние в верхней ноде") { style = { fontSize = 15, unityFontStyleAndWeight = FontStyle.Bold, color = new Color(0.5f, 0.5f, 0.5f, 0.5f) } };
            label.pickingMode = PickingMode.Ignore;
            _placeholderContainer.Add(label);
            Add(_placeholderContainer);
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (_activeSubState == null) return graphViewChange;

            bool isDirty = false;

            // Обработка создания связей
            if (graphViewChange.edgesToCreate != null)
            {
                if (_activeSubState.Edges == null) _activeSubState.Edges = new List<EdgeSaveData>();
                foreach (var edge in graphViewChange.edgesToCreate)
                {
                    if (edge.output.node is BaseGraphViewNode outNode && edge.input.node is BaseGraphViewNode inNode)
                    {
                        var outPorts = outNode.outputContainer.Query<Port>().ToList();
                        int portIndex = outPorts.IndexOf((Port)edge.output);

                        _activeSubState.Edges.Add(new EdgeSaveData { 
                            OutputNodeID = outNode.GetRuntimeNode().ID, 
                            OutputPortIndex = portIndex, 
                            InputNodeID = inNode.GetRuntimeNode().ID 
                        });
                        isDirty = true;
                    }
                }
            }

            if (graphViewChange.elementsToRemove != null)
            {
                foreach (var element in graphViewChange.elementsToRemove)
                {
                    // Если это связь — вычищаем её из списка Edges
                    if (element is Edge edge && edge.output?.node is BaseGraphViewNode outNode && edge.input?.node is BaseGraphViewNode inNode)
                    {
                        var outPorts = outNode.outputContainer.Query<Port>().ToList();
                        int portIndex = outPorts.IndexOf((Port)edge.output);
            
                        _activeSubState.Edges?.RemoveAll(e => 
                                e.OutputNodeID == outNode.GetRuntimeNode().ID && 
                                e.OutputPortIndex == portIndex && 
                                e.InputNodeID == inNode.GetRuntimeNode().ID // Вот тут прямое приведение без GetComponent
                        );
                        isDirty = true;
                    }
                    // Если это нода — удаляем её и все связи, ведущие к ней/от неё
                    else if (element is BaseGraphViewNode uiNode)
                    {
                        var id = uiNode.GetRuntimeNode().ID;
                        _activeSubState.Nodes?.Remove(uiNode.GetRuntimeNode());
                        _activeSubState.Edges?.RemoveAll(e => e.OutputNodeID == id || e.InputNodeID == id);
                        isDirty = true;
                    }
                }
            }

            if (isDirty && Window?.CurrentAsset != null) EditorUtility.SetDirty(Window.CurrentAsset);
            return graphViewChange;
        }

        public void PopulateView(RuntimeSubState subState)
        {
            graphViewChanged -= OnGraphViewChanged;
            _activeSubState = subState;
            
            // Удаляем только элементы, которые мы сами создали
            var elementsToDelete = graphElements.Where(e => e is BaseGraphViewNode || e is Edge).ToList();
            foreach(var e in elementsToDelete) RemoveElement(e);

            if (_activeSubState == null) { _placeholderContainer.style.display = DisplayStyle.Flex; graphViewChanged += OnGraphViewChanged; return; }
            _placeholderContainer.style.display = DisplayStyle.None;

            if (_activeSubState.Nodes == null) _activeSubState.Nodes = new List<RuntimeNode>();
            if (_activeSubState.Nodes.Count == 0)
            {
                _activeSubState.Nodes.Add(new RuntimeEntryNode { ID = Guid.NewGuid().ToString(), Position = new Vector2(100, 200) });
                _activeSubState.Nodes.Add(new RuntimeExitNode { ID = Guid.NewGuid().ToString(), Position = new Vector2(600, 200) });
                if (Window?.CurrentAsset != null) EditorUtility.SetDirty(Window.CurrentAsset);
            }

            var uiNodesMap = new Dictionary<string, BaseGraphViewNode>();
            foreach (var runtimeNode in _activeSubState.Nodes)
            {
                var uiNode = CreateUiNodeInstance(runtimeNode);
                if (uiNode == null) continue;
                uiNode.SetPosition(new Rect(runtimeNode.Position, Vector2.zero));
                AddElement(uiNode);
                uiNodesMap[runtimeNode.ID] = uiNode;
            }

            if (_activeSubState.Edges != null)
            {
                foreach (var edgeData in _activeSubState.Edges)
                {
                    if (uiNodesMap.TryGetValue(edgeData.OutputNodeID, out var outN) && uiNodesMap.TryGetValue(edgeData.InputNodeID, out var inN))
                    {
                        var outPorts = outN.outputContainer.Query<Port>().ToList();
                        var inPorts = inN.inputContainer.Query<Port>().ToList();
                        
                        if (edgeData.OutputPortIndex >= 0 && edgeData.OutputPortIndex < outPorts.Count && inPorts.Count > 0)
                        {
                            AddElement(outPorts[edgeData.OutputPortIndex].ConnectTo(inPorts[0]));
                        }
                    }
                }
            }
            graphViewChanged += OnGraphViewChanged;
        }

        public override void AddToSelection(ISelectable s) { base.AddToSelection(s); OnSelectionChanged(); }
        public override void RemoveFromSelection(ISelectable s) { base.RemoveFromSelection(s); OnSelectionChanged(); }
        public override void ClearSelection() { base.ClearSelection(); OnSelectionChanged(); }

        private void OnSelectionChanged() => Window.UpdateInspector(selection.OfType<BaseGraphViewNode>().FirstOrDefault()?.GetRuntimeNode() ?? (object)_activeSubState);

        public override List<Port> GetCompatiblePorts(Port start, NodeAdapter adapter) => ports.ToList().Where(p => p.direction != start.direction && p.node != start.node).ToList();

        public void AddLocalNode(RuntimeNode node, Vector2 pos)
        {
            node.Position = pos;
            _activeSubState.Nodes.Add(node);
            var ui = CreateUiNodeInstance(node);
            ui.SetPosition(new Rect(pos, Vector2.zero));
            AddElement(ui);
            if (Window?.CurrentAsset != null) EditorUtility.SetDirty(Window.CurrentAsset);
        }

        private BaseGraphViewNode CreateUiNodeInstance(RuntimeNode n) => n switch
        {
            RuntimeEntryNode en => new EntryUiNode(en),
            RuntimeExitNode ex => new ExitUiNode(ex),
            RuntimePhraseNode p => new PhraseUiNode(p),
            RuntimeChoiceNode c => new ChoiceUiNode(c),
            RuntimeMakeOrderNode m => new MakeOrderUiNode(m),
            RuntimeCancelOrderNode c => new CancelOrderUiNode(c),
            _ => null
        };
    }
}