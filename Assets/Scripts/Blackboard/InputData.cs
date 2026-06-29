using UnityEngine;

namespace Project.Core.Blackboard
{
    /// <summary>
    /// 承載自輸入裝置採樣來的原始資料（未經邏輯處理）
    /// </summary>
    public class InputData
    {
        public Vector2 MoveInput;
        public Vector2 LookInput;
        public bool JumpButtonDown;
        public bool RollButtonDown;
        public bool FireButtonDown;
    }
}
