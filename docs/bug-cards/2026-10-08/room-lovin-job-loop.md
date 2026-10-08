# 乱起在同一 tick 里反复重开爱爱

| 字段 | 内容 |
| --- | --- |
| 复现 | 1.0.4 开发版存档。`普通囚犯-3` 携带乱起，与米莉拉 `Milira_Race20939` 同处室内，床为 `ADH_bed_double_A_A_FURNITURE39618`。Player.log 从 `Job_519400` 起同一 tick 连续 `started 10 jobs in one tick`，`jobGiver=RimWorld.JobGiver_DoLovin`，日志在此截断，游戏主线程不再往下走。上一份 Player-prev.log 同一对从 `Job_510250` 起已经刷过一轮 |
| 期望 / 实际 | 人已经躺在目标床上、且冷却结束时才发一次爱爱 / 原版给不出爱爱时补丁仍强制发 Lovin，人没躺上这张床，Job 立刻失败，ThinkTree 同一 tick 再问一次 |
| 版本 | 1.0.4 开发版 |
| 级别 | S1 |
| 根因 | `RHAH_RoomLovinPatch` 不看 `canLovinTick`，也不要求发起者已经躺在将要使用的那张床上。原版 `JobDriver_Lovin.CanBeginNowWhileLyingDown` 在人没躺上目标床时立刻失败，失败不写冷却 |
| 最小修复 | 只在冷却已过、发起者醒着且已躺在非医疗床、伴侣也在这张床上时补 Job。成功与失败都把双方 `canLovinTick` 推到原版年龄曲线之后 |
| 明确不做 | 不改受孕几率、不改早熟高育速产多崽、不改原版自己发起的爱爱、不给没躺下的人补走到床边的 Job |
| Language | 无 |
| 存档 | 无新键。沿用原版 `Pawn_MindState.canLovinTick` |
| 归属表 | 否 |
| 回归 | 规则测试覆盖冷却中、人没躺下、伴侣不在这张床、医疗床拒绝，以及允许时写入的最小冷却 |
| 后续债 | 游戏内仍需看正常结束只判定一次受孕 |
