# 倒地鼠蛋被外部命令赶出地图

| 字段 | 内容 |
| --- | --- |
| 复现 | 幼童扩展启用后，本模组来客里不能自己走到出口的鼠蛋已经倒地。`RimtalkToddlerExpand` 的 `Patch_TravelingLord.ForcePawnLeaveMap` 或 `Patch_ToddlerCarrying.TryQueueExitMapJob` 对它 `StartJob(Goto, exitMapOnArrival: true)`。旧项目玩家反馈过同一现象 |
| 期望 / 实际 | 期望倒地且不能自己走到出口的来客留在地图上，由能走的成年照护者抱走。实际原版 `StartJob` 不看倒地，命令会让它自己走向出口 |
| 版本 | 0.1.0 |
| 级别 | S2 |
| 根因 | `JobGiver_ExitMap.TryGiveJob` 会拒绝非爬行倒地，但 `Pawn_JobTracker.StartJob` 不拒绝。外部模组绕过 JobGiver，直接下带 `exitMapOnArrival` 的 `Goto`。本模组自己的离场已经用 `CanWalkOut` 拦住，挡不住别人的 `StartJob` |
| 最小修复 | `RHAH_StayWorkPatch` 在 `StartJob` 前拒绝：本模组来客、`exitMapOnArrival`、且 `CanWalkOut` 为 false。能走、被抱着、非来客和其它 Job 不拦 |
| 明确不做 | 不反射 `RimtalkToddlerExpand` 或 Toddlers。不改生成年龄、乞讨年龄、赈灾取食、学步 Hediff 和多崽分娩。不拦 `Follow`、`Escort` 或原版 `ExitMap` duty |
| Language | 无 |
| 存档 | 无 |
| 归属表 | 不更新 |
| 回归 | `DownedVisitorDoesNotAcceptAnExternalExitOrder`。倒地或不能走的来客拒绝离图 Job，能走的来客、非来客和进食不拒绝 |
| 后续债 | 无成年照护者的整批幼年事件仍可能停在地图上。这次只禁止自己走出去，不补新的携带者 |
