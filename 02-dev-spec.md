# CharacterController 開發規格文件（API / 資料結構）

> 狀態：草稿 v0.3
> 最後更新：2026-06-29
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
        Arbitration/       # ArbiterPipeline, IArbiter, ArbiterData
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
    public IntentData Intent;

    // === 參數區（持續存在，每帧更新）===
    public float MoveSpeed;
    public Vector2 MoveDirection;
    public float UpperBodyWeight;
    public Transform CameraTransform;

    // === 仲裁區（由 ArbiterPipeline 每帧寫入，各表現層 Controller 唯讀）===
    public ArbiterData Arbitration;

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
| Arbitration | struct ArbiterData | ArbiterPipeline | IK/Audio/表情等 Controller | 每帧由仲裁管線統一覆寫，Controller 只讀不寫 |

> **規範**：新增欄位時，必須同時填寫「誰寫入」「誰讀取」，避免日後出現多處寫入同一欄位造成競爭。
>
> **⚠️ ref struct 相容性警語**：`InputData` 已升版為 `ref struct`（見 1.3 節），絕對不能成為 `PlayerRuntimeData` 的欄位。黑板只持有處理後的 `IntentData`（一般 struct）與各參數數值，這個邊界必須維持，否則編譯直接失敗。

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

### 1.3 InputData（已升版為 ref struct，v0.2 → v0.3）

> **版本說明**：v0.1 使用可變 class，`Sample()` 回傳同一物件參考，存在鬼影資料風險（詳見 `01-design-doc.md` Trade-off 表）。v0.3 起升版為 `ref struct`，同步修改 `IInputSource` 簽名（見 2.1 節）。

```csharp
public ref struct InputData
{
    public Vector2 MoveInput;
    public Vector2 LookInput;
    public bool JumpButtonDown;
    public bool RollButtonDown;
    public bool FireButtonDown;
}
```

**使用限制（必讀）**：
- 只能存活在 stack，不能被 `class` 持有為欄位
- 不能裝箱（boxing）、不能用於 `async` 方法或 `yield return` 迭代器
- **`PlayerRuntimeData` 永遠不能持有 `InputData`**——只能持有處理後轉換成一般型別的結果（`IntentData`、`MoveDirection` 等）
- 這是破壞性升版，已同步修改 `IInputSource`（2.1 節）、`PlayerInputSource`、`CharacterPipelineRunner` 三處

**升版後務必執行**：用 Unity Profiler 量測升版前後的 GC Alloc 差異，截圖存入 `/docs/profiler/` 並將結論補回 `01-design-doc.md` Trade-off 表對應欄位。

### 1.4 ArbiterData（仲裁旗標，結構體）

> **實作時機**：第四階段，狀態機完成後再實作。目前先定義好結構，預留黑板欄位。

```csharp
public struct ArbiterData
{
    // 輸入封鎖：true 時 InputPipeline 仍採樣，但 Intent Processor 不應寫入意圖
    // （由 InputSourceBase.IsBlocked 或 CharacterPipelineRunner 判斷）
    public bool BlockInput;

    // IK 封鎖：true 時 IK Controller 跳過本帧更新
    public bool BlockIK;

    // 音頻封鎖：true 時 Audio Controller 跳過本帧播放請求
    public bool BlockAudio;

    // 表情封鎖：true 時 表情 Controller 跳過本帧更新
    public bool BlockExpression;

    // TODO: 視實際需要新增更細粒度的旗標
    // 注意：旗標越多黑板越肥，新增前先評估能否合併到現有旗標
}
```

| 旗標 | 誰寫入 | 誰讀取 | 典型觸發情境 |
|---|---|---|---|
| BlockInput | ArbiterPipeline | CharacterPipelineRunner / InputSourceBase | 死亡、被定身 CC、過場動畫 |
| BlockIK | ArbiterPipeline | IK Controller | 死亡、角色不可見、LOD 降級 |
| BlockAudio | ArbiterPipeline | Audio Controller | 死亡、靜音降級、LOD 降級 |
| BlockExpression | ArbiterPipeline | 表情 Controller | 死亡、頭部被遮擋 |

---

## 2. 核心介面定義

### 2.1 IInputSource

> **v0.3 升版**：配合 `InputData` 改為 `ref struct`，簽名從回傳值改為 `ref` 寫入參數。

```csharp
public interface IInputSource
{
    void FetchRawInput(ref InputData data);
}
```
- 職責：單純把輸入裝置/AI決策寫入既有的 `InputData` 記憶體，不碰黑板、不碰狀態機。
- 預期實作：`PlayerInputSource`, `AIInputSource`
- **呼叫端範例**（`CharacterPipelineRunner.Update()` 內）：
```csharp
InputData inputData = default;
_inputSource.FetchRawInput(ref inputData);
```

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

### 2.4 IArbiter（仲裁器介面，規劃中）

> **實作時機**：第四階段。目前先定義介面草案，實作留待後續。

```csharp
public interface IArbiter
{
    /// <summary>
    /// 依目前狀態機狀態，將仲裁結果寫入 data.Arbitration
    /// </summary>
    void Arbitrate(PlayerRuntimeData data);
}
```
- 職責：接收黑板（含當前狀態資訊），統一計算並覆寫 `data.Arbitration` 仲裁旗標。
- `ArbiterPipeline` 持有 `List<IArbiter>`，每帧逐一呼叫，支援多個仲裁器疊加（例如「死亡仲裁器」和「LOD 仲裁器」各自獨立）。
- 不該做：不該直接呼叫任何 Controller 的方法，只能寫黑板旗標。

### 2.5 IPlayerIKSource（若範圍內含 IK）

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
| 1 | InputPipeline | 裝置原始輸入 | InputData | `ref struct`，stack-only |
| 2 | Intent Processor | InputData | RuntimeData.Intent | 若 `Arbitration.BlockInput == true` 則跳過寫入 |
| 3 | Parameter Processor | RuntimeData | RuntimeData（更新參數） | |
| 4 | 狀態機 Tick | RuntimeData | 狀態切換 | 讀 Intent，讀完即可視為消耗 |
| 4.5 | ArbiterPipeline Tick | RuntimeData（含當前狀態） | RuntimeData.Arbitration | 第四階段接入，狀態機 Tick 之後立即執行 |
| 5 | AnimationFacade 同步 | RuntimeData/狀態 | 動畫播放 | |
| 6 | MotionDriver（LateUpdate） | 動畫結果 | Transform 位移 | 必須在 LateUpdate，避免滑步 |
| 7 | IntentData.Reset()（LateUpdate 末） | — | RuntimeData.Intent 清空 | 必須在所有讀取方完成後才執行 |

> **⚠️ 順序脆弱點 1**：`IntentData.Reset()` 必須排在狀態機 Tick（順序 4）之後，且在同帧的 LateUpdate 末執行。若不小心提前，意圖會在被讀取前就被清空。
>
> **⚠️ 順序脆弱點 2**：`ArbiterPipeline Tick`（順序 4.5）必須在狀態機 Tick（順序 4）**之後**，才能讀到本帧最新的狀態結果；且必須在 AnimationFacade（順序 5）**之前**，確保動畫層能讀到本帧的仲裁旗標。

---

## 3.1 Pipeline 處理器介面（規劃中／重構候選，尚未實作）

> 目前 Intent/Parameter 處理邏輯以 private method 形式內嵌在 `CharacterPipelineRunner`（`ProcessIntents` / `ProcessParameters`）。地基階段邏輯量小，先求資料流跑通，暫不抽介面。完整 Trade-off 見 `01-design-doc.md` 第 5 節。

**重構訊號**：當 `ProcessIntents` 或 `ProcessParameters` 任一個方法內的 if-else 判斷超過約 10-15 行，視為該執行此重構的時間點。

**規劃草案**：
```csharp
public interface IIntentProcessor
{
    void Process(ref InputData input, ref IntentData intent);
    // 注意：InputData 已為 ref struct，這裡必須用 ref 參數，不能用回傳值
}

public interface IParameterProcessor
{
    void Process(ref InputData input, PlayerRuntimeData data);
}
```

- `CharacterPipelineRunner` 屆時改為持有 `List<IIntentProcessor>` 與 `List<IParameterProcessor>`，在 `Update()` 中逐一呼叫。
- 目的：新增一個處理器時不需要修改 `CharacterPipelineRunner` 本體。
- **注意**：`InputData` 已改為 `ref struct`，介面方法簽名必須用 `ref` 參數傳遞，不能用回傳值或一般參數。

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

- [ ] 仲裁器（Arbiter）優先級表（多個 IArbiter 同時封鎖同一旗標時的合併規則）
- [ ] 裝備系統 ItemDefinition 欄位規格
- [ ] 音效事件命名規範
- [ ] 物件池 Key 規則
- [ ] Pipeline 處理器抽介面執行（見 3.1 節規劃草案）
- [ ] ArbiterData 旗標粒度最終決策（單一 BlockAll vs 各 Controller 各自一個旗標）
- [ ] Profiler GC Alloc 量測結果補回 Trade-off 表（InputData ref struct 升版後執行）

---

## 6. 修訂紀錄

| 日期 | 版本 | 變更內容 |
|---|---|---|
| 2026-06-28 | v0.1 | 初版骨架建立 |
| 2026-06-29 | v0.2 | 補充 InputData ref struct 重構規劃、IInputSource 替代簽名、Pipeline 處理器抽介面規劃草案，並標注黑板與 ref struct 的相容性限制 |
| 2026-06-29 | v0.3 | InputData 正式升版為 ref struct；IInputSource 簽名改為 void FetchRawInput(ref InputData)；新增 ArbiterData 結構定義（1.4）、IArbiter 介面（2.4）、資料夾結構補 Arbitration/、Pipeline 順序表加入 ArbiterPipeline 步驟（4.5）及兩處順序脆弱點警語；IIntentProcessor 簽名同步改為 ref 參數 |