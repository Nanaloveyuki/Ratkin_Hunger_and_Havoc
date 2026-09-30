# 检疫逐 tick 扫全人口

状态：已实现，验证未执行

| 字段 | 内容 |
| --- | --- |
| 复现 | 一条检疫记录离开 Pending/Defer 后持续运行。来自源码对照；卸载清理已运行隔离复现，其它尚未游戏内复现 |
| 期望 / 实际 | 慢状态按小时检查；实际每 tick 重建世界人口索引 |
| 版本 | 本仓库 1.0.0，2026-09-30 当前工作树；保留用户并发改动 |
| 级别 | S3 |
| 根因 | `GameComponent` 每 tick 调用 `RHAH_Quarantine.Tick`。选择通知后无条件进入 `Watch`，`RHAH_PawnIndex.Find` 在 tick 变化时重建 `PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead`。终态记录没有提前返回 |
| 最小修复 | 选择通知仍每 tick 执行。人口观察只在 `GenDate.TicksPerHour` 的整点进入。`Pending`、`Defer`、`Broken`、`RecoveredLeft`、`RecoveredStayed`、`AllDead`、`Missing` 不扫描。`Release` 与 `Quarantine` 仍按小时观察 |
| 明确不做 | 不部署，不修改外部 Mod，不改包名/API/事件显示 ID，不夹带重构 |
| Language | 预计无；若新增玩家可见字符串必须同时补中英 Keyed |
| 存档 | 默认无新键、不破坏重建；保留已有记录。若必须新增持久化类型，动手前在本卡明确并登记 |
| 归属表 | 无新增类型时保持；卸载清理动作或新持久化类型必须更新 save-ownership.md |
| 回归 | 非观察 tick 不改访客在场状态；观察 tick 才推进。终态和暂缓不观察。人口枚举次数由 /tmp Harmony 冒烟计数，生产代码不留计数。子代理不运行检查，主代理统一执行并记录实际结果 |
| 后续债 | 本卡外相邻问题不纳入；游戏内验证边界明确报告 |
