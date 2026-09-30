# 流民商队走近图标循环红字

| 字段 | 内容 |
| --- | --- |
| 复现 | Stable 1.0.0。世界里的流民商队走近图标每 tick 刷 `ArgumentOutOfRangeException`，引用 `ECC9DC5B`。栈顶是 `WorldPath.Peek` ← `ConsumeNextNode` ← `WorldObject_RHAH_Approach.Step` `[0x000c7]`，模块 `<b996c0455cf6473d95a58de0ddc908c8>`。异常打断 `WorldObjectsHolderTick` |
| 期望 / 实际 | 期望走近图标按一格推进，失败时离开路径并到达或停下，不打断后面的世界物体。实际该物体停在邻格后每 tick 越界 |
| 版本 | 玩家标注 Stable 1.0.0。抛异常的程序集 MVID 是 `b996c045-5cf6-473d-95a5-8de0ddc908c8`，即 `5b76034` 之前的构建。当前 1.0.0 是 `d7ebc6a9-d45c-4ca5-97be-bc75a7673f07` |
| 级别 | S1 |
| 根因 | 旧 `Step` 在 `NodesLeftCount >= 2` 时连调两次 `ConsumeNextNode`。原版一次调用丢掉当前格并取出下一格；邻格只剩当前格和殖民地，第二次 `Peek(1)` 越界。`5b76034` 已改成只消费一次。玩家这份程序集仍是修复前的，所以同一条路径继续每 tick 抛错 |
| 最小修复 | 不改移动、到达和存档。`TryConsumeNext` 在 `NodesLeftCount >= 2` 之外再要求 `NodeCount >= NodesLeftCount`，路径池里的游标坏掉时拒绝消费，不调用 `ConsumeNextNode` |
| 明确不做 | 不给世界物体 tick 加 try/catch。不改原版运输仓或穿梭机。不重发已经发出的 1.0.0 包 |
| Language | 无 |
| 存档 | 无 |
| 归属表 | 否 |
| 回归 | `AdjacentPathYieldsTheColonyOnce` 增加坏游标：节点被清空但 `Found` 仍为真时拒绝，且不抛越界 |
| 后续债 | 玩家需要换上含 `5b76034` 的构建。修复前的存档里如果走近图标已经卡在邻格，换包后应能继续走进殖民地 |
