using UnityEngine;
using Project.Core.Blackboard;

namespace Project.Core.Pipeline
{
    public class CharacterPipelineRunner : MonoBehaviour
    {
        [Header("Setup")]
        [Tooltip("掛載了 PlayerInputSource 的 GameObject")]
        [SerializeField] private MonoBehaviour inputSourceComponent;
        [SerializeField] private Transform playerCamera;

        private IInputSource _inputSource;
        private PlayerRuntimeData _runtimeData;

        // 提供給 Debug 視窗讀取的外部介面
        public PlayerRuntimeData RuntimeData => _runtimeData;

        private void Awake()
        {
            _inputSource = inputSourceComponent as IInputSource;
            if (_inputSource == null)
            {
                Debug.LogError($"[{gameObject.name}] inputSourceComponent 沒有實作 IInputSource 介面！", this);
            }

            // 初始化黑板與基礎相機引用
            _runtimeData = new PlayerRuntimeData();
            _runtimeData.CameraTransform = playerCamera != null ? playerCamera : Camera.main?.transform;
        }

        private void Update()
        {
            if (_inputSource == null) return;

            // 【順序 1】InputPipeline - 採樣原始輸入裝置
            InputData inputData = _inputSource.Sample();

            // 【順序 2】Intent Processor - 將原始輸入轉化為當幀意圖並送入黑板
            ProcessIntents(inputData);

            // 【順序 3】Parameter Processor - 處理每幀更新的連續表現參數
            ProcessParameters(inputData);

            // 【順序 4】分層狀態機 Tick - (第 3 週以後實作接上)
            // _stateMachine.Tick(_runtimeData);

            // 【順序 5】AnimationFacade 同步 - (第 3 週以後實作接上)
        }

        private void LateUpdate()
        {
            // 【順序 6】MotionDriver 物理/位移表現更新 - (後續實作接上)

            // 【管線終點】每幀結尾徹底復位意圖，防止未被狀態機消耗的意圖殘留到下一幀
            _runtimeData.Intent.Reset();
        }

        private void ProcessIntents(InputData input)
        {
            // 意圖轉換：當裝置觸發按下，立刻標記黑板的意圖
            if (input.JumpButtonDown) _runtimeData.Intent.JumpRequested = true;
            if (input.RollButtonDown) _runtimeData.Intent.RollRequested = true;
            if (input.FireButtonDown) _runtimeData.Intent.FireRequested = true;

            // 學習性測試用的 Console 輸出，因為 OnGUI 可能因幀率落差抓不到極短暫的 true
            if (input.JumpButtonDown) Debug.Log("<color=lime>[Intent] 跳躍意圖已被黑板捕獲！</color>");
            if (input.RollButtonDown) Debug.Log("<color=cyan>[Intent] 翻滾意圖已被黑板捕獲！</color>");
            if (input.FireButtonDown) Debug.Log("<color=orange>[Intent] 開火意圖已被黑板捕獲！</color>");
        }

        private void ProcessParameters(InputData input)
        {
            // 連續參數計算
            _runtimeData.MoveDirection = input.MoveInput;
            _runtimeData.MoveSpeed = input.MoveInput.magnitude;

            // 演示測試：動態計算一個模擬的上半身權重
            _runtimeData.UpperBodyWeight = input.MoveInput != Vector2.zero ? 0.5f : 0.0f;
        }
    }
}