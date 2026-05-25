using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Linq;

namespace RubbishPot.Core
{
    public class StateMachineEditorWindow : EditorWindow
    {
        private StateMachineGraphView _graphView;
        private PlotAsset _currentAsset;
        private Label _assetLabel;

        [MenuItem("Tools/State Machine Editor")]
        public static void Open() => GetWindow<StateMachineEditorWindow>("State Machine");

        private void OnEnable()
        {
            _graphView = new StateMachineGraphView(this)
            {
                style = { flexGrow = 1 }
            };

            rootVisualElement.Add(_graphView);
            _graphView.GenerateDefaultNodes();
            GenerateToolbar();
        }

        private void GenerateToolbar()
        {
            var toolbar = new UnityEditor.UIElements.Toolbar();

            _assetLabel = new Label(_currentAsset != null
                ? $"Active Asset: {_currentAsset.name}"
                : "No Asset Loaded (Double-click any PlotAsset)");
            _assetLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _assetLabel.style.paddingLeft = 5;
            _assetLabel.style.paddingRight = 10;
            toolbar.Add(_assetLabel);

            toolbar.Add(new Button(() => SaveData()) { text = "Save Asset" });
            toolbar.Add(new Button(() => FixAllPlotAssetsInProject()) { text = "Fix All Assets GUIDs" });
            
            rootVisualElement.Add(toolbar);
        }

        private void SaveData()
        {
            if (_currentAsset == null)
            {
                Debug.LogError("Невозможно сохранить: ни один PlotAsset не открыт в редакторе!");
                return;
            }

            _currentAsset.Data.Nodes.Clear();
            _currentAsset.Data.Edges.Clear();

            var uiNodes = _graphView.nodes.ToList().OfType<BaseGraphViewNode>();
            foreach (var uiNode in uiNodes)
            {
                uiNode.UpdatePosition();
                _currentAsset.Data.Nodes.Add(uiNode.GetRuntimeNode());
            }

            var edges = _graphView.edges.ToList().OfType<Edge>();
            foreach (var edge in edges)
            {
                if (edge.output?.node is BaseGraphViewNode outNode && edge.input?.node is BaseGraphViewNode inNode)
                {
                    var allOutputPorts = outNode.outputContainer.Query<Port>().ToList();
                    int portIndex = allOutputPorts.IndexOf(edge.output);

                    _currentAsset.Data.Edges.Add(new EdgeSaveData
                    {
                        OutputNodeID = outNode.GetRuntimeNode().ID,
                        InputNodeID = inNode.GetRuntimeNode().ID,
                        OutputPortIndex = portIndex,
                        PortName = edge.output.portName
                    });
                }
            }

            EditorUtility.SetDirty(_currentAsset);
            var so = new SerializedObject(_currentAsset);
            so.Update();
            so.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();

            Debug.Log($"<color=green>[SUCCESS]</color> Изменения успешно сохранены в ассет: {_currentAsset.name}");
        }

        public void LoadAsset(PlotAsset plotAsset)
        {
            if (plotAsset == null || plotAsset.Data == null) return;

            _currentAsset = plotAsset;

            // 1. Собираем все занятые GUID из ДРУГИХ ассетов в проекте
            var globalIds = new HashSet<string>();
            string[] guids = AssetDatabase.FindAssets("t:PlotAsset");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                PlotAsset otherAsset = AssetDatabase.LoadAssetAtPath<PlotAsset>(path);
                
                if (otherAsset != null && otherAsset != _currentAsset && otherAsset.Data != null)
                {
                    foreach (var n in otherAsset.Data.Nodes) 
                    {
                        globalIds.Add(n.ID);
                    }
                }
            }

            // 2. Проверяем текущий ассет на пересечение с глобальной базой
            FixAssetIDs(_currentAsset, globalIds);
            
            if (_assetLabel != null)
            {
                _assetLabel.text = $"Active Asset: {_currentAsset.name}";
            }

            ClearAndPopulateGraph(_currentAsset.Data.Nodes, _currentAsset.Data.Edges);
            Debug.Log($"Successfully loaded asset: {plotAsset.name}");
        }

        private void ClearAndPopulateGraph(List<RuntimeNode> nodes, List<EdgeSaveData> edges)
        {
            var currentUiNodes = _graphView.nodes.ToList().OfType<BaseGraphViewNode>().ToList();
            foreach (var node in currentUiNodes) _graphView.RemoveElement(node);

            var currentEdges = _graphView.edges.ToList().OfType<Edge>().ToList();
            foreach (var edge in currentEdges) _graphView.RemoveElement(edge);

            if (!nodes.Any(n => n is RuntimeEntryNode))
                nodes.Add(new RuntimeEntryNode { Position = new Vector2(100, 200) });
            if (!nodes.Any(n => n is RuntimeExitNode))
                nodes.Add(new RuntimeExitNode { Position = new Vector2(600, 200) });

            var spawnedNodesMap = new Dictionary<string, BaseGraphViewNode>();
            foreach (var runtimeNode in nodes)
            {
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

                if (uiNode != null)
                {
                    _graphView.AddElement(uiNode);
                    spawnedNodesMap.Add(uiNode.NodeID, uiNode);
                }
            }

            foreach (var edgeData in edges)
            {
                if (spawnedNodesMap.TryGetValue(edgeData.OutputNodeID, out var outUiNode) &&
                    spawnedNodesMap.TryGetValue(edgeData.InputNodeID, out var inUiNode))
                {
                    var allOutputPorts = outUiNode.outputContainer.Query<Port>().ToList();
                    Port outputPort = null;

                    if (edgeData.OutputPortIndex >= 0 && edgeData.OutputPortIndex < allOutputPorts.Count)
                    {
                        outputPort = allOutputPorts[edgeData.OutputPortIndex];
                    }

                    if (outputPort == null && !string.IsNullOrEmpty(edgeData.PortName))
                    {
                        outputPort = allOutputPorts.FirstOrDefault(p => p.portName == edgeData.PortName);
                    }

                    if (outputPort == null)
                    {
                        outputPort = allOutputPorts.FirstOrDefault();
                    }

                    Port inputPort = inUiNode.inputContainer.Query<Port>().First();

                    if (outputPort != null && inputPort != null)
                    {
                        var newEdge = outputPort.ConnectTo(inputPort);
                        _graphView.AddElement(newEdge);
                    }
                }
            }
        }
        
        // Универсальный метод проверки и починки с учетом глобальной базы
        private bool FixAssetIDs(PlotAsset asset, HashSet<string> globalIds)
        {
            if (asset == null || asset.Data == null || asset.Data.Nodes.Count == 0) return false;

            bool needsFix = false;
            var localIds = new HashSet<string>();

            // Проверяем как внутренние дубликаты, так и глобальные (клоны)
            foreach (var node in asset.Data.Nodes)
            {
                if (string.IsNullOrEmpty(node.ID) || localIds.Contains(node.ID) || globalIds.Contains(node.ID))
                {
                    needsFix = true;
                    break;
                }
                localIds.Add(node.ID);
            }

            if (!needsFix) 
            {
                // Пополняем глобальную базу валидными ID этого ассета
                foreach (var id in localIds) globalIds.Add(id);
                return false;
            }

            Debug.Log($"<color=yellow>[ID Fixer]</color> Обнаружены клонированные/дублирующиеся GUID. Пересобираем ассет: '{asset.name}'...");

            var idMap = new Dictionary<string, string>();

            foreach (var node in asset.Data.Nodes)
            {
                string oldId = node.ID;
                string newId = System.Guid.NewGuid().ToString();

                // Запоминаем ремаппинг только для первой встречи старого ID
                if (!string.IsNullOrEmpty(oldId) && !idMap.ContainsKey(oldId))
                {
                    idMap.Add(oldId, newId);
                }

                node.ID = newId;
                globalIds.Add(newId); // Сразу кидаем новый ID в глобальную базу
            }

            foreach (var edge in asset.Data.Edges)
            {
                if (idMap.TryGetValue(edge.OutputNodeID, out string newOutputId))
                {
                    edge.OutputNodeID = newOutputId;
                }
                if (idMap.TryGetValue(edge.InputNodeID, out string newInputId))
                {
                    edge.InputNodeID = newInputId;
                }
            }

            EditorUtility.SetDirty(asset);
            var so = new SerializedObject(asset);
            so.Update();
            so.ApplyModifiedProperties();
            AssetDatabase.SaveAssetIfDirty(asset);
            
            Debug.Log($"<color=green>[ID Fixer SUCCESS]</color> Ассет '{asset.name}' вылечен и отвязан от оригиналов!");
            return true;
        }
        
        private void FixAllPlotAssetsInProject()
        {
            string[] guids = AssetDatabase.FindAssets("t:PlotAsset");
            var globalIds = new HashSet<string>();
            int fixedCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                PlotAsset asset = AssetDatabase.LoadAssetAtPath<PlotAsset>(path);
                
                if (asset != null)
                {
                    // Пропускаем через проверку по нарастающей глобальной базе
                    if (FixAssetIDs(asset, globalIds))
                    {
                        fixedCount++;
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"<color=cyan>[Bulk Fixer]</color> Проверка завершена. Вылечено клонов: {fixedCount}");
        }
    }

    public class StateMachineGraphView : GraphView
    {
        public EditorWindow Window { get; private set; }

        public StateMachineGraphView(EditorWindow window)
        {
            Window = window;

            Insert(0, new GridBackground());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            nodeCreationRequest = context =>
            {
                var provider = ScriptableObject.CreateInstance<StateMachineSearchWindowProvider>();
                provider.Init(this);
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), provider);
            };

            graphViewChanged = OnGraphViewChanged;
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.edgesToCreate != null)
            {
                foreach (var edge in graphViewChange.edgesToCreate)
                {
                    if (edge.output?.node is ParallelUiNode parallelNode)
                    {
                        schedule.Execute(() => parallelNode.CleanUpPorts()).ExecuteLater(10);
                    }
                    else if (edge.output?.node is GroupUiNode groupNode)
                    {
                        schedule.Execute(() => groupNode.CleanUpPorts()).ExecuteLater(10);
                    }
                }
            }

            if (graphViewChange.elementsToRemove != null)
            {
                var removedEdges = graphViewChange.elementsToRemove.OfType<Edge>().ToList();

                foreach (var edge in removedEdges)
                {
                    if (edge.output?.node is ParallelUiNode parallelNode)
                    {
                        schedule.Execute(() => parallelNode.CleanUpPorts()).ExecuteLater(50);
                    }
                    else if (edge.output?.node is GroupUiNode groupNode)
                    {
                        schedule.Execute(() => groupNode.CleanUpPorts()).ExecuteLater(50);
                    }
                }
            }

            return graphViewChange;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList()
                .Where(endPort => endPort.direction != startPort.direction && endPort.node != startPort.node).ToList();
        }

        public void GenerateDefaultNodes()
        {
            var existingNodes = nodes.ToList().OfType<BaseGraphViewNode>().ToList();
            if (existingNodes.Any(n => n.GetRuntimeNode() is RuntimeEntryNode)) return;

            var entryRuntime = new RuntimeEntryNode();
            var entryUI = new EntryUiNode(entryRuntime);
            entryUI.SetPosition(new Rect(100, 200, 100, 150));
            AddElement(entryUI);

            var exitRuntime = new RuntimeExitNode();
            var exitUI = new ExitUiNode(exitRuntime);
            exitUI.SetPosition(new Rect(600, 200, 100, 150));
            AddElement(exitUI);
        }
    }
}