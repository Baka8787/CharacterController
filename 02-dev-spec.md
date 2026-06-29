# CharacterController 開發規格文件（API / 資料結構）

> 狀態：草稿 v0.1
> 最後更新：2026-06-28
> 用途：實作時的對照表，先定義介面再寫實作

---

## 0. 命名與檔案結構規範

### 0.1 命名規範
- 介面：`I` 前綴，例如 `IInputSource`
- 抽象基底類別：`Base` 後綴，例如 `BaseState`
- ScriptableObject 設定檔：`SO` 後綴，例如 `EquippableItemSO`
- 私有欄位：`_camelCase`，公開屬性：`PascalCase`

### 0.2 資料夾結構（建議）
```
Assets/
  _Project/
    Scripts/
      Core/
        Blackboard/        # RuntimeData, InputData
        Pipeline/          # InputPipeline, MainProcessorPipeline
        StateMachine/      # BaseState, 各層狀態
        Interrupt/         # InterruptProcessor, 攔截器
      Presentation/
        Animation/         # AnimationFacadeBase 及實作
        Motion/            # MotionDriver
        Audio/             # AudioDriver
      Equipment/
        Definitions/       # ItemDefinition, EquippableItemSO
        Runtime/           # ItemInstance, EquipmentDriver
      Pooling/
      Editor/              # 編輯器工具
    Prefabs/
    ScriptableObjects/
  Plugins/
```

---

## 1. 資料結構定義

### 1.1 RuntimeData（黑板）

```csharp
public class PlayerRuntimeData
{
    // === 意圖區（每帧處理完即復位）===
    public IntentData Intent;          // 結構體，見下方

    // === 參數區（持續存在，每帧更新）===
    public float MoveSpeed;
    public Vector2 MoveDirection;
    public float UpperBodyWeight;
    public Transform CameraTransform;

    // === 引用區 ===
    public ItemInstance CurrentWeapon;
    public Transform AimTarget;

    // TODO: 隨實作擴充，每新增欄位請同步更新此文件
}
```

| 欄位 | 型別 | 誰寫入 | 誰讀取 | 備註 |
|---|---|---|---|---|
| Intent | struct IntentData | InputPipeline | 狀態機 | 每帧結尾復位 |
| MoveSpeed | float | Parameter Processor | AnimationFacade | |
| CurrentWeapon | ItemInstance | EquipmentDriver | 多處 | 唯讀引用，禁止外部修改內容 |

> **規範**：新增欄位時，必須同時填寫「誰寫入」「誰讀取」，避免日後出現多處寫入同一欄位造成競爭。

### 1.2 IntentData（意圖，結構體）

```csharp
public struct IntentData
{
    public bool JumpRequested;
    public bool RollRequested;
    public bool FireRequested;
    // 結構體 + 整體覆寫 = 復位時零 GC
}
```

### 1.3 InputData

```csharp
public class InputData
{
    public Vector2 MoveInput;
    public Vector2 LookInput;
    public bool JumpButtonDown;
    // ...
}
```

---

## 2. 核心介面定義

### 2.1 IInputSource

```csharp
public interface IInputSource
{
    InputData Sample();
}
```
- 職責：單純把輸入裝置/AI決策轉成 `InputData`，不碰黑板、不碰狀態機。
- 預期實作：`PlayerInputSource`, `AIInputSource`

### 2.2 BaseState

```csharp
public abstract class BaseState
{
    public abstract bool CanEnter(PlayerRuntimeData data);
    public abstract void OnEnter(PlayerRuntimeData data);
    public abstract void OnTick(PlayerRuntimeData data, float deltaTime);
    public abstract void OnExit(PlayerRuntimeData data);
    public abstract bool CanBeInterruptedBy(BaseState other);
}
```
- 職責：定義單一狀態的生命週期與互斥規則。
- 注意：`OnTick` 內不可直接呼叫動畫播放 API，必須透過 `AnimationFacadeBase`。

### 2.3 AnimationFacadeBase

```csharp
public abstract class AnimationFacadeBase : MonoBehaviour
{
    public abstract void Play(string stateKey, float transitionDuration = 0.2f);
    public abstract void SetLayerWeight(int layer, float weight);
    public abstract void SetParameter(string key, float value);
    // event/callback 介面視實作需求增補
}
```
- 職責：隔離底層動畫系統（Animator / Animancer）差異。
- 實作範例：`AnimatorFacade : AnimationFacadeBase`

### 2.4 IPlayerIKSource（若範圍內含 IK）

```csharp
public interface IPlayerIKSource
{
    void SetIKTarget(IKTargetType type, Transform target, float weight);
}
```

---

## 3. Pipeline 處理順序規格

> 管線化架構最容易出 bug 的地方是「處理順序」，明確寫下來避免日後改順序時忘記為什麼這樣排

| 順序 | 處理器 | 輸入 | 輸出 | 備註 |
|---|---|---|---|---|
| 1 | InputPipeline | 裝置原始輸入 | InputData | |
| 2 | Intent Processor | InputData | RuntimeData.Intent | |
| 3 | Parameter Processor | RuntimeData | RuntimeData（更新參數） | |
| 4 | 狀態機 Tick | RuntimeData | 狀態切換 | 讀 Intent，讀完即可視為消耗 |
| 5 | AnimationFacade 同步 | RuntimeData/狀態 | 動畫播放 | |
| 6 | MotionDriver（LateUpdate） | 動畫結果 | Transform 位移 | 必須在 LateUpdate，避免滑步 |

---

## 4. 狀態進入/打斷規則表（範例，待擴充）

| 狀態 | 所屬層 | 可從哪些狀態進入 | 可被誰打斷 | 備註 |
|---|---|---|---|---|
| Idle | FullBody | Move, Land | Move, Jump, Roll | |
| Move | FullBody | Idle | Jump, Roll | |
| Jump | FullBody | Idle, Move | （空中不可被地面動作打斷） | |
| Roll | FullBody | Move | （翻滾中不可打斷，無敵幀） | |

---

## 5. 待補充規格清單

- [ ] 仲裁器（Arbiter）優先級表
- [ ] 裝備系統 ItemDefinition 欄位規格
- [ ] 音效事件命名規範
- [ ] 物件池 Key 規則

---

## 6. 修訂紀錄

| 日期 | 版本 | 變更內容 |
|---|---|---|
| 2026-06-28 | v0.1 | 初版骨架建立 |
