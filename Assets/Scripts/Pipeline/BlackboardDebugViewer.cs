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

            // 畫出半透明黑色除錯底框
            GUI.backgroundColor = new Color(0, 0, 0, 0.85f);
            GUILayout.BeginArea(new Rect(30, 30, 400, 350), "【BBBNexus 復刻誌 - 黑板數據流監視器】", GUI.skin.box);
            GUILayout.Space(25);

            // 1. 意圖區
            GUILayout.Label("<color=yellow><b>=== 1. 意圖區 (Intent Data - 單幀即逝) ===</b></color>");
            GUILayout.Label($"  Jump Requested : {FormatBool(data.Intent.JumpRequested)}");
            GUILayout.Label($"  Roll Requested : {FormatBool(data.Intent.RollRequested)}");
            GUILayout.Label($"  Fire Requested : {FormatBool(data.Intent.FireRequested)}");
            GUILayout.Space(15);

            // 2. 參數區
            GUILayout.Label("<color=yellow><b>=== 2. 參數區 (Parameter Data - 每幀連續) ===</b></color>");
            GUILayout.Label($"  Move Direction : {data.MoveDirection}");
            GUILayout.Label($"  Move Speed Mag : {data.MoveSpeed:F4}");
            GUILayout.Label($"  Upper Body Wt  : {data.UpperBodyWeight:F2}");
            GUILayout.Label($"  Camera Pivot   : {(data.CameraTransform != null ? "<color=lime>已綁定</color>" : "<color=red>未綁定</color>")}");

            GUILayout.Space(20);
            GUILayout.Label("<color=grey>* 註：意圖在 LateUpdate 會被清空，若畫面上反應不及請看 Console 記錄。</color>");

            GUILayout.EndArea();
        }

        private string FormatBool(bool value)
        {
            return value ? "<color=lime><b>【TRUE - TRIGGERED】</b></color>" : "false";
        }
    }
} 