# 2026-09-24 成年照护者带离、离场 Duty 绕闸、断粮后游荡

| 字段 | 写什么 |
| --- | --- |
| 复现 | 0.1.0 新游戏。母亲或普通成年来客与走不动的幼年同批到达后离场。再看 `RHAH_VisitorLeave` 的后备 Job。最后让来客在赈灾区外找不到食物，等到断粮期限结束 |
| 期望 / 实际 | 期望能走的成年照护者带不能走的幼年离开；离场只经过 `ExitMap` 或已到期的 `LeaveAfterFed`；断粮期限结束后离开，而不是在等待点附近一直游荡。实际 `Carry` 只放行幼年角色，成年照护者发不出携带 Job；离场 Duty 在自有 Job 失败后仍执行 `JobGiver_ExitMapBest`；等待到期只是不再发等待 Job，寻食 Duty 继续游荡 |
| 版本 | 0.1.0 |
| 级别 | S1 |
| 根因 | `Carry` 被写成幼年角色闸门，和“同 Lord 里允许 Carry 的大人带出”相反。`RHAH_VisitorLeave` 把原版出口 Job 放在自有离场 Job 后面，绕过两个闸门。断粮截止只让 `WaitFood` 返回空，没有把未进食的活跃访客交给离场 |
| 最小修复 | 活跃且能自己离场的非幼年角色默认允许 `Carry`，幼年角色不允许。能走到出口的照护者先为同 Lord 里走不动的幼年发携带 Job，没有可带对象才自己离场。离场 Duty 去掉 `JobGiver_ExitMapBest`。断粮等待到期且仍未进食时，视为可以离场 |
| Language | 无 |
| 存档 | 无。`foodWaitUntilTick` 仍是 -1 表示未计时 |
| 归属表 | 否 |
| 回归 | 成年照护者 `Carry` 为 true，幼年为 false。离场 Duty 不再引用 `JobGiver_ExitMapBest`。断粮到期后放行离场，未到期或已进食不放行 |
| 后续债 | 无 |
