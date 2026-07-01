using UnityEngine;
using Project.Core.Blackboard;

namespace Project.Core.Pipeline
{
    public class CharacterPipelineRunner : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private MonoBehaviour inputSourceComponent;
        [SerializeField] private Transform playerCamera;

        private IInputSource _inputSource;
        private PlayerRuntimeData _runtimeData;

        public PlayerRuntimeData RuntimeData => _runtimeData;

        private void Awake()
        {
            _inputSource = inputSourceComponent as IInputSource;
            if (_inputSource == null)
            {
                Debug.LogError($"[{gameObject.name}] inputSourceComponent 沒有實作 IInputSource 介面！", this);
            }

            _runtimeData = new PlayerRuntimeData
            {
                CameraTransform = playerCamera != null ? playerCamera : Camera.main?.transform
            };
        }

        private void Update()
        {
            if (_inputSource == null) return;

            // =================================================================
            // 執行順序符合 v0.2 規格書 3.0 節定義：
            // =================================================================

            // 【順序 1】InputPipeline - 採樣裝置原始輸入
            InputData inputData = _inputSource.Sample();

            // 【順序 2】Intent Processor - 寫入當帧意圖
            ProcessIntents(inputData);

            // 【順序 3】Parameter Processor - 更新黑板連續參數
            ProcessParameters(inputData);

            // 【順序 4】狀態機 Tick (預留位置，後續實作接上)
            // 讀取黑板中的 Intent，讀完即可視為被狀態機消耗
            // _stateMachine.Tick(_runtimeData, Time.deltaTime);

            // 【順序 5】AnimationFacade 同步 (預留位置，後續實作接上)
            // _animationFacade.Sync(_runtimeData);
        }

        private void LateUpdate()
        {
            // 【順序 6】MotionDriver 位移表現更新 (預留位置，後續實作在 LateUpdate 執行以避免滑步)
            // _motionDriver.UpdateMovement(_runtimeData);

            // =================================================================
            // ⚠️ v0.2 順序脆弱點防禦線：
            // IntentData.Reset() 必須嚴格排在【順序 2 寫入】與【順序 4 讀取】之後。
            // 目前安置於 LateUpdate 結尾，確保整個 Update 週期內的模組都能讀取到意圖。
            // =================================================================
            _runtimeData.Intent.Reset();
        }

        /// <summary>
        /// Intent Processor 邏輯（當前內嵌於 Runner，重構訊號：超過 10-15 行時抽離）
        /// </summary>
        private void ProcessIntents(InputData input)
        {
            if (input.JumpButtonDown) _runtimeData.Intent.JumpRequested = true;
            if (input.RollButtonDown) _runtimeData.Intent.RollRequested = true;
            if (input.FireButtonDown) _runtimeData.Intent.FireRequested = true;

            // 除錯 log 保持不變
            if (input.JumpButtonDown) Debug.Log("<color=lime>[Intent] 跳躍意圖已被黑板捕獲！</color>");
            if (input.RollButtonDown) Debug.Log("<color=cyan>[Intent] 翻滾意圖已被黑板捕獲！</color>");
            if (input.FireButtonDown) Debug.Log("<color=orange>[Intent] 開火意圖已被黑板捕獲！</color>");
        }

        /// <summary>
        /// Parameter Processor 邏輯（當前內嵌於 Runner）
        /// </summary>
        private void ProcessParameters(InputData input)
        {
            _runtimeData.MoveDirection = input.MoveInput;
            _runtimeData.MoveSpeed = input.MoveInput.magnitude;
            _runtimeData.UpperBodyWeight = input.MoveInput != Vector2.zero ? 0.5f : 0.0f;
        }
    }
}