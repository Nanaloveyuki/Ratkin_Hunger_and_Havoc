# 广播回应未进入广播所在据点

| 字段 | 内容 |
| --- | --- |
| 复现 | 玩家报告 ODF 更新前 1.0.6 中间歇出现广播后有鼠族到达信、当前殖民地没有鼠族。可证明边界：有 uniqueID=0 与另一张地图的存档，在首图通讯台广播；队列把首图编码成旧的“未指定”，执行时重新随机选地图，世界接近队伍又不尊重首图 0。没有玩家存档/日志，不能把此路径断言为玩家唯一原因 |
| 期望 / 实际 | 广播回应进入实际广播所在据点，真正到达信只在实际生成后发出 / 目标 0 被当成未指定，后续接近路径还会再次随机，可能在其它地图生成并发信 |
| 版本 | 玩家反馈为 ODF 前 1.0.6；修复基于当前已切换 ODF 的工作树 |
| 级别 | S2 |
| 根因 | `TargetId(false, 0)` 与 `UnspecifiedTargetId=0` 冲突；`RHAH_QueuedTarget.Resolve` 对 0 调用随机地图解析；`RHAH_Approach.PickMapId` 只尊重正数 savedId，`Home` 再调用一次选择。ODF 已让后续链和捕食者支持非负站点，但没有修复普通事件队列/世界队伍这条独立路径 |
| 最小修复 | 仍用已有 `pendingIncidentTargetIds`，仅为明确指定的首图增加 `int.MinValue` 编码，保留正图 ID、负商队 ID、旧 0 未指定语义；解码后按 0 精确查图。世界队伍只接受队列已经解析出的目标地图，不再另抽据点；世界队伍检查文字也接受 mapId=0。加目标、队列分批消费和真实 Scribe 存读回归 |
| 明确不做 | 不回退 ODF，不增加重试或常驻扫描；不改 Visitor/Food/Trade/SaveCleanup；不改公共签名、事件显示 ID、Def、存档键/默认值/XML 类型；不修改原本已有的一小时 Waiting 行为；不把无证据的异常解释成实际玩家根因 |
| Language | 无。广播 `RHAH_Broadcast_Queued` 中英都只说已发出/等回应，接近信中英都说仍在路上；实际到达选择信由 Facts 在整批生成与 Lord 成功后发送 |
| 存档 | 没有新字段、键或类型，不迁移或覆盖原档。已有队列目标整数扩展一个原本不能表示有效商队的值 `int.MinValue`，只用于明确 map0；旧队列 0 无法恢复原意，继续保持旧“未指定”解析。世界队伍 `mapId` 继续存原始非负 ID，`arrivalPending`、`costLeft`、`nextTileId`、批次和 ODF receipt 均不改变 |
| 归属表 | 集成者更新已有 `pendingIncidentTargetIds` 值域说明；归属仍随游戏组件 Remove，不新增清理产物 |
| 回归 | 永久 net48 测试覆盖显式 map0/正图/负商队/旧0/非法方向解码、精确目标不存在时不换图、未就绪首图保留等待、旧队列缺目标补0、真实 Scribe 保存剩余队列及批次；依赖 Unity 原生 cctor 的世界队伍旅行/待到达存读、真实 Worker 失败无信和目标销毁由下方真实游戏烟测覆盖，不在无 Unity 的独立 runner 中替换生产行为 |
| 后续债 | 没有这次新增的债；没有原玩家存档与 Player.log，真实 intermittency 的其它路径仍不能确认 |

## 已有修复覆盖

- `2026-10-09/visitor-arrival-destination.md` 修的是已经生成的来客在地图边缘徘徊：分离入场格与居住区停留格，旧 Lord 目的地刷新，倒地/不能走的孩子不阻塞集合。它不处理队列目标 ID、世界队伍到达或消息提交。
- 当前 `WorldObject_RHAH_Approach.Arrive` 已保存 `arrivalPending`，Waiting 时保存一小时剩余等待；成功或终止才销毁队伍，异常不整批重跑。不是本次新增重试。
- 广播候选是目录 `BroadcastEligible` 原作事件，不包含空投事件。`RHAH_PawnFactory.Create` 对有地图请求必须实际 `Spawned` 才返回成功；`RHAH_IncidentFacts.Submit` 整批创建、家庭绑定和 Lord 成功后才登记叙事事实、开选择信、提交 ODF 后续。返回 false 的路径不发到达选择信。
- 分帧仅按 64 tick 消费一条排队事件，不把单事件的 Pawn 拆成未保存的分批生成状态；保存发生在同步事件生成的前后，队列消费后不再重放该项。

## 永久回归

- `RHAH_IncidentScheduleTests.SavedTargetsKeepTheirOriginalMapOrCaravan`：正图、负商队、显式首图哨兵及非法方向值
- `RHAH_BroadcastArrivalTests.ExplicitBroadcastTargetKeepsTheConsoleMapWhileUnavailable`：首图 0 与正图均精确查询；地图未就绪时等待，不因另图存在而改目标
- `MissingExplicitTargetDoesNotRedirectToAnotherHome`：原目标消失，队列终止而不是改往另一图
- `LegacyUnspecifiedZeroWaitsWithoutUsableMaps` / `LegacyQueueWithoutTargetsKeepsUnspecifiedSemanticsAfterResave`：旧 0 与缺失目标列表不被重新解释成首图
- `FirstMapQueueAndRemainingBatchRoundTripWithoutReplayingConsumedItem`：调用既有消费入口 `DropPending` 后，实际 Scribe 保存/载入/再存/重读保留首图目标、点数、剩余项和已登记批次，消费过的项不会重放；完整 Worker 分帧执行由真实游戏烟测覆盖
- `InvalidGenerationCannotPublishArrivalChoiceOrNarrativeFacts`：非法生成上下文没有选择或登记批次
- 初次集成独立 runner 暴露 `WorldObject` 的 `MaterialPropertyBlock` cctor 与日志堆栈原生依赖；这些行为转由真实 RimWorld 执行，不增加生产 suppress、Harmony 替代、假逻辑或假成功

切片代理未运行检查；集成者统一执行结果见下文


## 集成后可执行烟测

1. 使用含两处玩家据点（至少首图 `uniqueID=0`）的存档，关闭其它可广播事件，只启用 I-005；在首图通讯台完成广播。记录队列明确首图编码，切换当前查看地图也不能改变接近队伍的目标。
2. 队伍到达前保存并重读：广播回执仅表示已发出，接近信仍是路上状态，没有已经到达选择信。等待抵达；选择信 `lookTargets` 的鼠族都实际在首图，另一据点没有这一批；悬停/跳转目标能看到已生成访客。
3. 在首图不可满足事件条件（例如 I-005 温度不允许）时到达，确认没有到达选择信、队伍保留；在这个待到达阶段保存重读，剩余小时等待与目标不变。恢复原条件后仅生成一次、仅开一封信，随后世界队伍消失；不增加新重试机制。
4. 排两项不同事件，打开分帧生成，在只消费第一项之后保存重读；第一批不重复生成，第二项的点数和目标继续原值。
5. 在已生成首图访客的存档保存重读，确认原选择信、访客批次和 ODF 后续实例各只有一份；对照上述历史修复确认他们向居住区停留点移动而不是藏在入场边缘。
6. 队伍抵达前删除原 map0，只留下另一家园：`SpawnArrived("I-005", 260f, 0)` 必须 Terminal，队伍销毁且另一家园没有新访客/到达信；不能重新抽目标

## 已执行验证

- 永久队列目标及真实 Scribe 回归纳入最终 Release 552 项，全通过；包含显式 map0、正图、商队、旧0、目标缺失、剩余队列与存读边界
- 真实游戏通过接近队伍入口 `TrySend("I-005", 300, 0)` 创建 map0 队伍，行进阶段存读保持目标。新增另一地图并切换当前图，仍不改目的地
- 温度故意不满足时调用真实到达入口，队伍保留、`arrivalPending=true`、剩余等待 2500 tick，没有新增到达选择信；保存重读保持这些状态
- 恢复温度后真实生成 5 名访客到 map0，仅新增一封到达选择信，队伍销毁；再次存读，信的 5 个目标仍全部在 map0，另一地图没有该批访客
- 让原 map0 无法按保存 ID 解析后，到达队伍终止，地图 1 未生成该批访客；日志明确记录 `Saved map is gone. map=0`
- 边界：本次集成烟雾直接驱动接近队伍入口，未重新操作通讯台完整作业。缺少玩家原档，不能断言该目标碰撞覆盖所有间歇原因
- 隔离游戏保留了旧 ODF 可选 `Oxygendsl.Rhah` 适配程序集，其 `IRHAH_PawnBehavior` 类型加载报错；核心 ODF 与本模组事件链已加载。该外部程序集兼容问题独立于五条反馈，未修改其它仓库或扩大 API
