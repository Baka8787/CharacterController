using UnityEngine;
using UnityEngine.InputSystem;
using Project.Core.Blackboard;

namespace Project.Core.Pipeline
{
    public class PlayerInputSource : MonoBehaviour, IInputSource
    {
        [Header("Unity New Input System Actions")]
        [Tooltip("設置改為 Action Type: Value, Control Type: Vector2")]
        public InputAction MoveAction;

        [Tooltip("設置改為 Action Type: Value, Control Type: Vector2")]
        public InputAction LookAction;

        [Tooltip("設置改為 Action Type: Button")]
        public InputAction JumpAction;

        [Tooltip("設置改為 Action Type: Button")]
        public InputAction RollAction;

        [Tooltip("設置改為 Action Type: Button")]
        public InputAction FireAction;

        private readonly InputData _inputData = new InputData();

        private void OnEnable()
        {
            MoveAction?.Enable();
            LookAction?.Enable();
            JumpAction?.Enable();
            RollAction?.Enable();
            FireAction?.Enable();
        }

        private void OnDisable()
        {
            MoveAction?.Disable();
            LookAction?.Disable();
            JumpAction?.Disable();
            RollAction?.Disable();
            FireAction?.Disable();
        }

        public InputData Sample()
        {
            // 1. 採樣連續二維向量數值
            _inputData.MoveInput = MoveAction != null ? MoveAction.ReadValue<Vector2>() : Vector2.zero;
            _inputData.LookInput = LookAction != null ? LookAction.ReadValue<Vector2>() : Vector2.zero;

            // 2. 採樣當幀是否按下（WasPressedThisFrame 完美契合單幀 Trigger 需求）
            _inputData.JumpButtonDown = JumpAction != null && JumpAction.WasPressedThisFrame();
            _inputData.RollButtonDown = RollAction != null && RollAction.WasPressedThisFrame();
            _inputData.FireButtonDown = FireAction != null && FireAction.WasPressedThisFrame();

            return _inputData;
        }
    }
}
