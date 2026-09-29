# 走近图标在邻格每 tick 越界，运输仓和穿梭机停住

| 字段 | 内容 |
| --- | --- |
| 复现 | Beta 0.2.2 读档。地图事件的 `RHAH_Approach` 已经走到殖民地邻格。世界 tick 刷 `ArgumentOutOfRangeException`，引用 `4436BF6B`。运输仓和穿梭机发不出去。开发者模式删掉该世界物体后恢复 |
| 期望 / 实际 | 期望邻格走入殖民地并生成事件，后面的世界物体照常 tick。实际 `Step` 在两格路径上第二次取节点时越界，异常打断 `WorldObjectsHolderTick`，排在后面的运输仓和穿梭机本 tick 不推进 |
| 版本 | 0.1.0，玩家日志为 Beta 0.2.2 |
| 级别 | S1 |
| 根因 | 原版 `WorldPath` 反序存放，`ConsumeNextNode` 一次丢掉当前格并取出下一格。`Step` 在 `NodesLeftCount >= 2` 时连调两次。邻格只剩当前格和殖民地，第二次 `Peek(1)` 越界。更长的路径则跳过真正的下一格 |
| 最小修复 | `RHAH_ApproachRules.TryConsumeNext` 只调用一次 `ConsumeNextNode`。不足两格、失败路径和共享 `NotFound` 返回 false，调用方仍按原规则决定是否归还路径池 |
| 明确不做 | 不改移动耗时、出海、到达生成和存档键。不给世界物体 tick 加 try/catch。不改原版运输仓或穿梭机 |
| Language | 无 |
| 存档 | 无 |
| 归属表 | 否 |
| 回归 | `AdjacentPathYieldsTheColonyOnce`。两格路径取出殖民地且只消费一次；三格路径取出中间格；已到达、空路径和 `NotFound` 拒绝 |
| 后续债 | 无 |
