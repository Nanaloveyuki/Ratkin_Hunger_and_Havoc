# 生育判定错误绑定所有 Toil

状态：实现已落地，主体构建通过；完整生育流程尚未游戏内验证

| 字段 | 内容 |
| --- | --- |
| 复现 | 携带乱起基因开始爱爱，观察走到床前、被中断和正常结束。来自源码对照；卸载清理已运行隔离复现，其它尚未游戏内复现 |
| 期望 / 实际 | 应只在成功完成时额外判定一次；实际每个 Toil Cleanup 都判定 |
| 版本 | 本仓库 1.0.0，2026-09-30 当前工作树；保留用户并发改动 |
| 级别 | S1 |
| 根因 | 原版 `JobDriver.Cleanup` 先执行 `globalFinishActions`，再调用当前 Toil 的 `Cleanup`。修复前 `RHAH_RoomBirthPatch.Finish` 给 `MakeNewToils` 返回的每个 Toil 都调用 `AddFinishAction(TryRoomConception)`。走到床前和躺下中断都会跑 Toil Cleanup，因此每次都判定。成功结束时 `Pawn_JobTracker.CleanupCurrentJob` 仍持有 `curJob`，伴侣在 `targetA`；`curJob` 要等 driver `Cleanup` 返回后才置空 |
| 最小修复 | `RHAH_RoomBirthPatch.Postfix` 为 driver 登记全局结束动作；重复构造时按委托 Method 找到并替换原槽位，不使用弱表，不包装 Toil。只有 `JobCondition.Succeeded` 调用真实 `TryRoomConception`。正常耗尽 Toil 时，原版最后 Cleanup 先于成功结束动作。其它生育补丁不改 |
| 明确不做 | 不部署，不修改外部 Mod，不改包名/API/事件显示 ID，不夹带重构 |
| Language | 预计无；若新增玩家可见字符串必须同时补中英 Keyed |
| 存档 | 默认无新键、不破坏重建；保留已有记录。若必须新增持久化类型，动手前在本卡明确并登记 |
| 归属表 | 无新增类型时保持；卸载清理动作或新持久化类型必须更新 save-ownership.md |
| 回归 | 删除拦截整个 TryRoomConception 并计数的伪回归。实际冒烟已尝试编译、运行；编译通过，但 PregnancyUtility 静态初始化触发 PawnRelationDefOf 和 Unity Debug 原生调用，无引擎宿主无法继续。未将该失败报告为通过；正常完成后的额外受孕仍需游戏内验证 |
| 后续债 | 游戏内仍需看携带乱起走到床前、中途中断和正常结束是否只在正常结束额外判定一次。本卡不处理怀孕几率、基因遗传和其它生育补丁 |
