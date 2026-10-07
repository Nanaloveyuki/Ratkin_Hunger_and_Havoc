# 2026-10-07 同批来客只能交食一人

| 字段 | 写什么 |
| --- | --- |
| 复现 | 同一事件来了两名以上饥饿鼠族，选择信点投喂，或殖民者右键其中一人点投喂。再右键同批另一人点投喂 |
| 期望 / 实际 | 期望每人都能单独交到允许的食物，吃不了或交不了时有明确提示，吃饱只在本人实际进食后成立。实际选择信只让最年长者等待整批份数，其他人右键只调用 `RHAH_Feeding.TryComplete`，饥饿时静默返回 false，没有交食 |
| 版本 | 当前 1.0.3（1.3），修复进入 1.0.4（1.4），2026-10-07 |
| 级别 | S1 |
| 根因 | 按人等待已经登记，但右键仍把 `job.lord` 设成整批访客 Lord，并沿用原版 `JobDriver_GiveToPawn.DetermineNumToHaul`。它用 `ItemCountLeftToCollect` 把整个 Lord 正在搬运的数量从当前接收者的剩余量里扣掉，`<= 0` 时立刻 `Succeeded`。第一人还在送时，第二人的任务还没拿起食物就被结束。旅途中点投喂也进不了等待：`RHAH_WaitFood` 只从 `seek` 转入 |
| 最小修复 | 请求仍按人保存，每人份数仍用 `foodPerVisitor`。选择信和右键都给当前可交食的人单独开等待。旅途和寻找都可转入已有的 `RHAH_WaitFood`。右键仍用原版 `GiveToPawn` 与 `disabledGiveFoodDefNames`。本模组已登记的接收者在原版 `ItemCountLeftToCollect` 按整个 Lord 扣搬运量之前，改成只扣这个接收者自己的搬运量；原版乞讨和其他接收者不改。不能交时显示原因。`TryComplete` 仍只由实际进食调用，饥饿者不标 `Fed` |
| 明确不做 | 不改广播、倒计时、赈灾取食名单和乞讨食物名单。不改吃饱阈值、再喂综合征和吃饱后离开。不把整批份数继续堆给一个人。不新增交食 Job。不改原版乞讨者和其他模组的 `GiveToPawn` 整 Lord 扣量 |
| Language | 中英日新增 `RHAH_Choice_FeedBlocked`、`RHAH_Choice_FeedNoVisitor`、`RHAH_Choice_FeedNotHungry`。`RHAH_Choice_FeedWaiting` 增加 `{0}`，表示这次开始等待的人数。本轮不改语言 |
| 存档 | 1.0.4 在访客 Lord 的 LoadingVars 字段读取前，将旧 `foodReceiver` XML 引用包装为 `foodReceivers/li`，之后只写新键。已有新键时不覆盖。原 foodDef、foodCount 和 Pawn 等待截止不变；空引用迁为空列表。不修改磁盘原档、不增加 tick 扫描。已增加旧键读入、保存新键、重读和原文件不变回归 |
| 归属表 | 已同步 `save-ownership.md`，`foodReceiver` 改为 `foodReceivers`，动作仍是 Remove |
| 回归 | 全套 431 项测试通过，新增 8 种旧引用、空引用、缺键和新旧共存迁移场景，均验证读旧、存新、重读与不改原文件。独立实际 Scribe smoke 验证停用存档仍迁移、接收者与食物 Def 引用恢复、7 份请求保留。前一轮实际 Harmony smoke 验证第二人原版剩余 -2、补丁后 2；第一人仍为 0，整批未完成。结构检查与主体构建通过。未验证游戏界面 |
| 后续债 | `pawn.md`、`incidents.md` 与 `save-ownership.md` 已同步。真实游戏中交食、进食与离场仍需观察 |
