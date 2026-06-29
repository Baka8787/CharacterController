using UnityEngine;

namespace Project.Core.Blackboard
{
    /// <summary>
    /// 資料中心黑板：所有玩法模組唯一的讀寫窗口
    /// </summary>
    public class PlayerRuntimeData
    {
        // === 意圖區（每幀處理完由 Pipeline 在 LateUpdate 復位）===
        public IntentData Intent;

        // === 參數區（持續存在，每幀更新的連續數值）===
        public float MoveSpeed;
        public Vector2 MoveDirection;
        public float UpperBodyWeight;
        public Transform CameraTransform;

        // === 引用區 ===
        // public ItemInstance CurrentWeapon; // 留待後續擴充裝備系統
        public Transform AimTarget;
    }
}