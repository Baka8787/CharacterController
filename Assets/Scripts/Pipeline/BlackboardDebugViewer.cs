using UnityEngine;
using Project.Core.Blackboard;

namespace Project.Core.Pipeline
{
    public class BlackboardDebugViewer : MonoBehaviour
    {
        [SerializeField] private CharacterPipelineRunner runner;

        private void OnGUI()
        {
            if (runner == null) return;

            PlayerRuntimeData data = runner.RuntimeData;
            if (data == null) return;

            GUI.backgroundColor = new Color(0, 0, 0, 0.85f);
            GUILayout.BeginArea(new Rect(30, 30, 420, 400), "【黑板數據流監視器 v0.2】", GUI.skin.box);
            GUILayout.Space(25);

            // 1. 意圖區
            GUILayout.Label("<color=yellow><b>=== 1. 意圖區 (Intent Data - 單帧即逝) ===</b></color>");
            GUILayout.Label($"  Jump Requested : {FormatBool(data.Intent.JumpRequested)}");
            GUILayout.Label($"  Roll Requested : {FormatBool(data.Intent.RollRequested)}");
            GUILayout.Label($"  Fire Requested : {FormatBool(data.Intent.FireRequested)}");
            GUILayout.Space(10);

            // 2. 參數區
            GUILayout.Label("<color=yellow><b>=== 2. 參數區 (Parameter Data - 每帧連續) ===</b></color>");
            GUILayout.Label($"  Move Direction : {data.MoveDirection}");
            GUILayout.Label($"  Move Speed Mag : {data.MoveSpeed:F4}");
            GUILayout.Label($"  Upper Body Wt  : {data.UpperBodyWeight:F2}");
            GUILayout.Label($"  Camera Pivot   : {(data.CameraTransform != null ? "<color=lime>已綁定</color>" : "<color=red>未綁定</color>")}");
            GUILayout.Space(10);

            // 3. 引用區 (v0.2 新增監視)
            GUILayout.Label("<color=yellow><b>=== 3. 引用區 (Reference Data - 唯讀防寫) ===</b></color>");
            string weaponStatus = data.CurrentWeapon != null ? $"<color=lime>{data.CurrentWeapon.GetType().Name}</color>" : "<color=grey>空手 (Null)</color>";
            GUILayout.Label($"  Current Weapon : {weaponStatus}");
            GUILayout.Label($"  Aim Target     : {(data.AimTarget != null ? "<color=lime>已聚焦</color>" : "<color=grey>無目標</color>")}");

            GUILayout.Space(15);
            GUILayout.Label("<color=orange><i>* 順序安全檢查：Intent 於 LateUpdate 正常解構清空中。</i></color>");

            GUILayout.EndArea();
        }

        private string FormatBool(bool value)
        {
            return value ? "<color=lime><b>【TRUE - TRIGGERED】</b></color>" : "false";
        }
    }
}