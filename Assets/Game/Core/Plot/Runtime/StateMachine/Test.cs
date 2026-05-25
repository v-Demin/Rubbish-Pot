namespace RubbishPot.Core
{
    using UnityEngine;
    using System.Collections;

    public class Test : MonoBehaviour
    {
        [Header("Настройки")]
        [SerializeField] private string _plotName; // Имя файла в папке Resources

        private GraphRuntimeInterpreter _interpreter;
        private GraphController _controller;

        private bool _waitingForChoice = false;
        private bool _canCancelOrder = false;
        private ShowPlayerChoicesCommand _currentChoiceCommand;
        private string _currentCancelNodeId;

        private void Start()
        {
            if (string.IsNullOrEmpty(_plotName))
            {
                Debug.LogError("Забыли написать имя файла графа в Инспекторе!");
                return;
            }

            SubscribeToAllEvents();

            Debug.Log("<color=green>=== ЗАПУСК ТЕСТОВОГО ГРАФА ===</color>");
            Debug.Log("<color=gray>[ИНФО] Управление в тесте:\nЦифры 1, 2, 3, 4 — Выбор ответа\nКлавиша ESC — Отмена заказа</color>");

            // Фабрика собирает всё сама, возвращает готовые движки и ID точки входа
            var (interpreter, controller, entryId) = PlotFactory.Create(_plotName);
            _interpreter = interpreter;
            _controller = controller;

            if (!string.IsNullOrEmpty(entryId))
            {
                _interpreter.Execute(entryId);
            }
            else
            {
                Debug.LogError("В графе отсутствует EntryNode!");
            }
        }

        private void Update()
        {
            // 1. Логика интерактивного выбора
            if (_waitingForChoice && _currentChoiceCommand != null)
            {
                int selectedIndex = -1;
        
                if (Input.GetKeyDown(KeyCode.Alpha1)) selectedIndex = 0;
                else if (Input.GetKeyDown(KeyCode.Alpha2)) selectedIndex = 1;
                else if (Input.GetKeyDown(KeyCode.Alpha3)) selectedIndex = 2;
                else if (Input.GetKeyDown(KeyCode.Alpha4)) selectedIndex = 3;

                if (selectedIndex >= 0 && selectedIndex < _currentChoiceCommand.Choices.Count)
                {
                    _waitingForChoice = false;
                    string choiceNodeId = _currentChoiceCommand.NodeId;
            
                    Debug.Log($"<color=yellow>[КЛАВИАТУРА]</color> Выбран вариант [{selectedIndex + 1}]");
            
                    // Взаимодействуем со специфичным интерфейсом конкретного типа ноды!
                    var choiceHandler = InteractionHub.Get<IChoiceNodeHandler>();
                    if (choiceHandler != null)
                    {
                        choiceHandler.SubmitChoice(choiceNodeId, selectedIndex);
                    }
                    else
                    {
                        Debug.LogError("Активная Choice-нода не зарегистрирована в хабе!");
                    }
            
                    _currentChoiceCommand = null;
                }
            }

            // 2. Обработка отмены заказа (Клавиша ESC)
            if (_canCancelOrder && Input.GetKeyDown(KeyCode.Escape))
            {
                _canCancelOrder = false;
                Debug.Log("<color=red>[КЛАВИАТУРА]</color> Нажат ESC! Активирована принудительный отказ от заказа.");
                
                // Взаимодействуем с интерфейсом ноды отмены
                var cancelHandler = InteractionHub.Get<ICancelOrderNodeHandler>();
                if (cancelHandler != null)
                {
                    cancelHandler.ExecuteCancel(_currentCancelNodeId);
                }
            }
        }

        private void SubscribeToAllEvents()
        {
            EventBus.Subscribe<PlayPhraseCommand>(cmd => {
                Debug.Log($"<color=cyan>[PLAY PHRASE]</color> Node ID: {cmd.NodeId} | Текст: \"{cmd.Text}\"");
                
                // Авто-прокрутка через интерфейс ноды фразы
                StartCoroutine(InvokeRoutine(() => {
                    var phraseHandler = InteractionHub.Get<IPhraseNodeHandler>();
                    phraseHandler?.CompletePhrase(cmd.NodeId);
                }, 2.0f));
            });

            EventBus.Subscribe<PlayAnimationCommand>(cmd => {
                Debug.Log($"<color=orange>[ANIMATION]</color> Node ID: {cmd.NodeId} | Запуск анимации: {cmd.AnimationName}");
            });

            EventBus.Subscribe<ShowPlayerChoicesCommand>(cmd => {
                Debug.Log($"<color=yellow>[PLAYER CHOICES]</color> Node ID: {cmd.NodeId} | Ожидание выбора (Нажми 1-{cmd.Choices.Count}):");
                for (int i = 0; i < cmd.Choices.Count; i++)
                {
                    Debug.Log($" [{i + 1}] -> {cmd.Choices[i]}");
                }
                
                _currentChoiceCommand = cmd;
                _waitingForChoice = true;
            });

            EventBus.Subscribe<MakeOrderCommand>(cmd => {
                Debug.Log($"<color=magenta>[MAKE ORDER]</color> Node ID: {cmd.NodeId} | Геймплей запущен для заказа: {cmd.OrderId}");
            });

            EventBus.Subscribe<SetStorageDataCommand<int>>(cmd => {
                Debug.Log($"<color=white>[STORAGE INJECT]</color> Node ID: {cmd.NodeId} | Ключ: {cmd.Key} | Значение: {cmd.Value}");
            });

            EventBus.Subscribe<ToggleCancelOrderButtonCommand>(cmd => {
                _canCancelOrder = cmd.IsVisible;
                _currentCancelNodeId = cmd.NodeId;
                Debug.Log($"<color=red>[UI CONTROL]</color> Доступность отмены заказа: {cmd.IsVisible}");
            });

            EventBus.Subscribe<NodeCompletedCommand>(cmd => {
                Debug.Log($"<color=green>=== [GRAPH FINISHED] ===</color> Работа завершена на Exit-ноде: {cmd.NodeId}");
            });
        }

        private void OnDestroy()
        {
            // Здесь должна быть нормальная очистка, но в рамках теста оставляем пустую отписку, 
            // так как в нашей шине это безопасно
        }

        private IEnumerator InvokeRoutine(System.Action action, float delay)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }
    }
}