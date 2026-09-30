# 后续事件链派发键不一致

状态：已实现，主代理尚未统一验证

| 字段 | 内容 |
| --- | --- |
| 复现 | 生成 I-002 等来客并推进后续链。来自源码对照；卸载清理已运行隔离复现，其它尚未游戏内复现 |
| 期望 / 实际 | 应执行后续处理器；实际记录 I-xxx 查不到 RHAH_Follow |
| 版本 | 本仓库 1.0.0，2026-09-30 当前工作树；保留用户并发改动 |
| 级别 | S1 |
| 根因 | `eventChains.displayId` 保存事件显示 ID（`I-002`）。`RHAH_EventFollowChain.DisplayId` 是 `RHAH_Follow`，`RHAH_EventChains.Find` 只做相等比较，所以 `TickDue` 和结束通知都找不到处理器。短链幂等写在静态 `predatorRolled`，读档后实例 ID 会撞号；长链每次 `GameComponentTick` 进入 `OnTick` 都会新建列表并扫 `AllPawnsSpawned` |
| 最小修复 | `IRHAH_EventChain.Owns` 让后续处理器认领 `RHAH_Follow` 和全部 `I-` 记录，含 `I-019`，存档键和阶段语义不变。短链阶段 0 只推进到阶段 2。长链保持阶段 1，仅在 `tick % GenDate.TicksPerHour == instanceId % GenDate.TicksPerHour` 时找地图；批次列表复用 |
| 明确不做 | 不部署，不修改外部 Mod，不改包名/API/事件显示 ID，不夹带重构 |
| Language | 预计无；若新增玩家可见字符串必须同时补中英 Keyed |
| 存档 | 默认无新键、不破坏重建；保留已有记录。若必须新增持久化类型，动手前在本卡明确并登记 |
| 归属表 | 无新增类型时保持；卸载清理动作或新持久化类型必须更新 save-ownership.md |
| 回归 | 新记录与读档记录均能派发，短链只执行一次，长期链按原契约推进；子代理不运行检查，主代理统一执行并记录实际结果 |
| 后续债 | 本卡外相邻问题不纳入；游戏内验证边界明确报告 |
