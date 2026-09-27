# 殖民者手动驱逐来客无效

| 字段 | 内容 |
| --- | --- |
| 复现 | 新游戏。殖民者选中后右键在场来客，点「驱逐{名字}」。来客当前态度为中立，生命周期仍是寻食或到达 |
| 期望 / 实际 | 期望这批人按到达后的态度离开：中立离开且不转敌对。实际菜单能点，人留在原地继续寻食 |
| 版本 | 0.1.0 |
| 级别 | S1 |
| 根因 | `TryShift` 把 `forcedAway` 取反后交给 `React`。中立的手动驱逐落到 `None`，函数直接返回，不发 `RHAH_Leave`。即便发了，`JobGiver_RHAH_Leave` 在生命周期还不是 `Leaving` 时只认吃饱或断粮到期，寻食中的驱逐令造不出离图 Job |
| 最小修复 | 驱逐按 `forcedAway` 原样反应。离场 Job 在已有离开令时放行，再切到 `Leaving`。伤害仍按原契约：中立不改 |
| 明确不做 | 不改五档反应表。不改 `batchTurnsHostile` 和 `batchLeavesTogether`。不给不能自己走到出口的幼年补新的携带者 |
| Language | 无 |
| 存档 | 无 |
| 归属表 | 不更新 |
| 回归 | `ManualExpel_LeavesNeutralWithoutTurningHostile`。中立驱逐是 `Leave`，中立受击仍是 `None`。离开令在未吃饱、未到断粮期限时也算该走 |
| 后续债 | 无 |
