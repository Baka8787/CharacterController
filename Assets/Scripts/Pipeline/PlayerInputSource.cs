using UnityEngine;
using UnityEngine.InputSystem;
using Project.Core.Blackboard;

namespace Project.Core.Pipeline
{
    public class PlayerInputSource : MonoBehaviour, IInputSource
    {
        [Header("Unity New Input System Actions")]
        public InputAction MoveAction;
        public InputAction LookAction;
        public InputAction JumpAction;
        public InputAction RollAction;
        public InputAction FireAction;

        // 遵循 0.1 命名規範：私有欄位使用 _camelCase
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
            _inputData.MoveInput = MoveAction != null ? MoveAction.ReadValue<Vector2>() : Vector2.zero;
            _inputData.LookInput = LookAction != null ? LookAction.ReadValue<Vector2>() : Vector2.zero;

            _inputData.JumpButtonDown = JumpAction != null && JumpAction.WasPressedThisFrame();
            _inputData.RollButtonDown = RollAction != null && RollAction.WasPressedThisFrame();
            _inputData.FireButtonDown = FireAction != null && FireAction.WasPressedThisFrame();

            return _inputData;
        }
    }
}