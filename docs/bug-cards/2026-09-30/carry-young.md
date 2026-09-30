# 清醒不能步行幼年来客携出失败

状态：实现已按监督修正，验证未执行。开始 Lord 在 `MakeNewToils` 枚举时捕获，不放在 preInit。`lordCaptured` 保证第一次就是 null 也不再重读。`Downed` 测试通过 `Pawn_HealthTracker.healthState` 设置。主代理运行 `dotnet test Source/Tests/HungerAndHavoc.Tests.csproj -p:RimWorldDir=/mnt/e/Apps/Steam/steamapps/common/RimWorld --filter RHAH_CarryYoungTests`，以及 `dotnet run --project /tmp/rhah-carry-young-smoke/rhah-carry-young-smoke.csproj`。游戏内携出、出口 `ExitMap` 和旧档 `CarryDownedPawnToExit` 尚未运行

| 字段 | 内容 |
| --- | --- |
| 复现 | 同 Lord 成年照护者离场，目标幼年清醒未倒地但不能走出。来自源码对照；卸载清理已运行隔离复现，其它尚未游戏内复现 |
| 期望 / 实际 | 应携出幼年；实际原版 Kidnap 驱动拒绝清醒未倒地目标 |
| 版本 | 本仓库 1.0.0，2026-09-30 当前工作树；保留用户并发改动 |
| 级别 | S1 |
| 根因 | 当前 `JobGiver_RHAH_Leave.CarryDependent` 对同 Lord、不能 `CanWalkOut` 的来客发 `JobDefOf.CarryDownedPawnToExit`。该 Def 的 `driverClass` 是 `JobDriver_Kidnap`。`JobDriver_Kidnap.MakeNewToils` 在基类 `JobDriver_TakeAndExitMap` 之前加 `FailOn(() => Takee == null || (!Takee.Downed && Takee.Awake()))`。目标清醒且未倒地时，走向目标的第一个 Toil 就会以 Incompletable 结束，照护者随后自己 `Goto` 离图，幼年留在地图。倒地目标能通过这个失败条件，所以倒地路径本身不是缺口。`ReleaseToColony` 只对目标 `NotifyReleased`：`Lord.RemovePawn` 不结束别人的 Job，`pawn.jobs.StopAll()` 只停目标自己的 Job。照护者换 Lord 时 `DetachFromOldLords` 同样只 `RemovePawn`，不结束其当前携出。因此新驱动必须在每个 Toil 前拒绝：目标已不是访客、目标 Lord 不再等于开 Job 时的 Lord、或目标已由别人携带 |
| 最小修复 | Source/Pawn/JobGiver_RHAH_Leave.cs 与最小必要携出 Job/Def；复用原版搬运离图流程，采用适合照护的目标契约，不强制倒地或跳过安全检查 |
| 明确不做 | 不部署，不修改外部 Mod，不改包名/API/事件显示 ID，不夹带重构 |
| Language | 新增玩家可见工作报告。Def 正文 `reportString` 为中文；英文 `Languages/English/DefInjected/JobDef/RHAH_Jobs.xml` 增加 `RHAH_CarryYoung.reportString`。另增中英 Keyed `RHAH_Job_CarryYoung_Report`，占位符 `{0}` 一个 |
| 存档 | 新 JobDef `RHAH_CarryYoung`，新驱动 `HungerAndHavoc.Pawn.JobDriver_RHAH_CarryYoung`。无新 `Scribe_*.Look` 键，驱动无新增字段。Job 目标仍是原版 Target A/B。旧档里已经发出的 `CarryDownedPawnToExit` 保留原版驱动，读档后自然完成或失败，不迁移、不改写 |
| 归属表 | 主代理更新。类型 `HungerAndHavoc.Pawn.JobDriver_RHAH_CarryYoung`：Job `driverClass`，Remove。Def `RHAH_CarryYoung`：JobDef，Remove |
| 回归 | 先写 `Source/Tests/RHAH_CarryYoungTests.cs`，调用真实 `JobDriver_RHAH_CarryYoung`：清醒不能走接受，倒地接受，已由别人携带、非访客、Lord 改变和预约失败拒绝，不写 `Downed=true`。另备 `/tmp` 冒烟，引用真实驱动。子代理不运行；主代理统一执行 |
| 后续债 | 本卡外相邻问题不纳入；游戏内验证边界明确报告 |
