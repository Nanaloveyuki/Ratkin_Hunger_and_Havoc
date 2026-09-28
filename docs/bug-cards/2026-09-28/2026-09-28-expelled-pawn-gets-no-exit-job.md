# 被驱逐的来客没有离图工作

| 字段 | 内容 |
| --- | --- |
| 复现 | 重新编译部署后，殖民者右键在场来客并完成「驱逐」。来客仍在寻食，没有倒地 |
| 期望 / 实际 | 期望这批人立刻接到走向地图出口的工作。实际 Lord 切了离场，当前工作被打断，思考树下一轮仍造不出离图 Job |
| 版本 | 0.1.0 |
| 级别 | S1 |
| 根因 | `ExitMap` 只在生命周期已经是 `Leaving` 时放行。`OrderLeave` 有 Lord 时只发 `RHAH_Leave` 就返回。转换会先结束所有工作，再等思考树。寻食中的人还没被标成 `Leaving`，离场 Job 在闸门处返回空 |
| 最小修复 | 先把要走的人标成 `Leaving`，再发 Lord 备忘。备忘之后当场 `TryCreate` 并 `StartJob`。倒地、昏迷或还不能自己走到出口的人仍不造 Job，状态保留 |
| 明确不做 | 不改五档反应。不改 `batchLeavesTogether`。不把倒地的人立刻抱出地图 |
| Language | 无 |
| 存档 | 无 |
| 归属表 | 否 |
| 回归 | `ManualExpel_StartsExitAfterOrder`。`OrderLeave` 在 `ReceiveMemo` 之后仍调用 `TryCreate` 和 `StartJob`，并且先标 `Leaving` |
| 后续债 | 无 |
