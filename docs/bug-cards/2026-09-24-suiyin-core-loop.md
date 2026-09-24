# 穗音信任、检疫选择、路费和记录计数

| 字段 | 内容 |
| --- | --- |
| 复现 | 新游戏，叙事者选穗音。完成托孤、交换、粮洞、使者或遗物选择后看事件权重和大型威胁；鼠疫来客进图后打开穗音信；凑满 8 类来客后看家园白银；把一条记录里的人安全送走后看结局种类计数 |
| 期望 / 实际 | 选择改过的信任立刻进入事件调度、威胁节奏和结局。鼠疫来客有留下、放行、暂缓三选，案子记下实际访客 ID。路费只发一次且金额与账本相同。完成的记录种类计入结局。实际选择只改账本，调度仍读旧信任；检疫信没有选项，访客列表一直空；路费只记数字；记录关闭不进结局计数。事件事实还把批次号当成 tick |
| 版本 | 0.1.0 |
| 级别 | S1 |
| 根因 | `NarrativeState.trust` 与 `SuiyinBook.Trust` 双写，选择路径不回写权威值。N007 只排队一封普通信。N003 在 `Note` 里置 `RewardPaid` 但不发放。`CloseJournal` 的成功结果没有调用 `NoteCompletedKind`。`SuiyinIncidentFact.Tick` 收的是 `SpawnBatchId` |
| 最小修复 | 账本是信任权威，状态在每次读写后对齐。N007 改成选择信并记录本批鼠疫访客。路费登记后由结局节拍发放一次。记录成功关闭时按种类计一次。事实分开保存 tick 和 batch。商队生成走同一事实入口 |
| 明确不做 | 不改事件目标、Pawn 生命周期、携带和离场。不重做检疫闸门、康复回访带入和结局门槛。不把暂缓变成第二套自动结算 |
| Language | 新增 `RHAH_Quarantine_Hold`、`RHAH_Quarantine_Release`、`RHAH_Quarantine_Defer`、`RHAH_Quarantine_Stale`，中英成对 |
| 存档 | 新 LetterDef `RHAH_QuarantineLetter` 与 `ChoiceLetter_RHAH_Quarantine.mapId`。`SuiyinN007Case` 增加 `choiceOpen`。`NarrativeState` 增加 `rewardDue`。无旧档迁移 |
| 归属表 | 是 |
| 回归 | `SuiyinRulesTests` 覆盖信任回写、检疫访客、路费只登记一次、记录种类只计一次 |
| 后续债 | 康复者十五天回访仍只认原 pawn，带不回地图时继续等。身份拒绝没有玩家入口 |
