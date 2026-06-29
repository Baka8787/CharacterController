# Unity 動作遊戲架構學習路徑

## 學習目標

本學習計畫的目標並非快速完成一款遊戲，而是建立一套具備可維護性、可擴充性且適合展示於作品集的 Unity 動作遊戲架構。

重點包含：

* 建立清楚的資料流（Data Flow）
* 實作分層狀態機（Hierarchical State Machine）
* 邏輯與動畫解耦（Logic / Presentation Separation）
* 建立可擴充的角色控制架構
* 完成一個可展示的 Demo 專案

---

# 學習階段規劃

## 第一階段：地基建設

### 學習目標

建立完整資料流，先不要接動畫系統。

### 實作內容

* 建立專案架構
* 安裝 Unity New Input System
* 建立 `PlayerRuntimeData`（黑板）
* 建立 `InputData`
* 建立 `InputPipeline`

  * 採樣輸入
  * 將玩家意圖寫入黑板
* 使用 `Debug.Log` 或 `OnGUI` 顯示黑板內容
* 驗證資料流是否正確

### 本階段重點

理解：

> **玩家意圖（Intent）** 與 **角色狀態（State）** 必須分離。

不要急著製作動畫，先確認資料流設計沒有問題。

---

## 第二階段：分層狀態機（核心）

### 學習目標

完成 Full Body State Layer。

### 實作內容

* 設計 `BaseState`
* 建立 State Registry（狀態註冊表）
* 完成以下狀態：

  * Idle
  * Move
  * Jump
  * Roll
* 使用 ScriptableObject 配置：

  * 進入條件
  * 打斷規則
* 串接 Unity Animator（暫不使用 Animancer）

### 本階段亮點

ScriptableObject 配置 State Rule 是整套架構最值得展示的部分，也是作品集的重要特色。

---

## 第三階段：表現層解耦

### 學習目標

讓遊戲邏輯完全不直接操作動畫。

### 實作內容

* 建立 `AnimationFacadeBase`
* 將 Unity Animator 包裝成 Facade
* 建立 `MotionDriver`

  * LateUpdate 同步動畫 Root Motion
  * 解決滑步問題
* 建立上半身 Layer

  * 空手
  * 持槍

### 本階段重點

完成：

> **Gameplay Logic → Animation Facade → Animator**

而不是：

> **Gameplay Logic → Animator**

---

## 第四階段：仲裁器與打斷系統

### 學習目標

加入較進階的架構設計。

### 實作內容

* 建立 `InterruptProcessor`

  * 全域打斷
  * 上半身打斷
* 建立簡易 Action Arbiter

  * 管理請求優先權
  * 解決技能衝突

### 本階段亮點

這部分能充分展現：

* 系統設計能力
* 架構思維
* 可擴充性

也是整個專案最有特色的地方之一。

---

## 第五階段：裝備系統與物件池

### 學習目標

補足專案完整度。

### 實作內容

建立裝備系統：

* `ItemDefinition`
* `EquipmentDriver`

完成：

* 至少兩種武器
* 展示不同武器邏輯

建立物件池：

* `SimpleObjectPoolSystem`

應用於：

* 投射物
* 特效
* 可重複生成物件

---

## 第六階段：作品集打磨

### 學習目標

將專案整理成一份完整的作品集。

### 實作內容

#### README

撰寫：

* 專案介紹
* 系統架構
* 流程圖（Mermaid 或其他流程圖）
* 各模組設計理念

#### Demo 影片

錄製約 3–5 分鐘影片，介紹：

* 專案架構
* 核心設計理念
* 系統運作流程
* 解決了哪些問題
* 為什麼採用目前的架構

#### GitHub 維護

保持良好的 Commit 歷史：

* 每完成一個功能就 Commit
* Commit 訊息具描述性
* 避免一次提交大量內容

---

# 新手實作建議

## 1. 先求完成，再求漂亮

第一版可以接受程式碼不夠優雅。

重點是：

* 功能先完成
* 資料流跑通
* 再逐步重構

---

## 2. Animancer 可以先略過

先使用 Unity Animator，透過 `AnimationFacade` 包裝動畫系統。

未來若改用 Animancer，只需修改 Facade 內部即可，大幅降低切換成本。

---

## 3. 不要急著加入額外插件

例如：

* Final IK
* Cinemachine

這些屬於加分功能，建議核心架構完成後再視需求加入。

---

## 4. 建立小型測試場景

每完成一個子系統，就建立一個獨立測試場景，例如：

* Input Test
* State Machine Test
* Animation Test
* Equipment Test
* Object Pool Test

讓每個系統都能獨立驗證，降低整合時的除錯成本。

---

# 最終成果

完成本學習路徑後，應具備以下成果：

* Input Pipeline
* Runtime Blackboard
* 分層狀態機（Hierarchical State Machine）
* ScriptableObject 規則配置
* Logic / Animation 解耦
* Animation Facade
* Motion Driver
* Interrupt System
* Action Arbiter
* Equipment System
* Object Pool
* 完整 README
* Demo 展示影片
* 清晰的 GitHub Commit 歷史

最終完成的不只是角色控制器，而是一套具備良好擴充性與維護性的 Unity 動作遊戲架構，可作為求職作品集的重要展示專案。
