# 2026-09-25 来客自动入籍未证实，灾荒商人不弹信

| 字段 | 写什么 |
| --- | --- |
| 复现 | 反馈 1：事件生成的鼠族在几秒钟后自动成为殖民地成员，无法选择是否接纳。反馈 2：触发 `I-012` 灾荒商人，观察信件和殖民者栏。当前 `Player.log` 没有这次事件记录 |
| 期望 / 实际 | 期望来客保持态度派系，玩家从选择信决定是否接纳。期望 `I-012` 弹出商队到达或交易提示，商人不加入殖民地。实际反馈称鼠族数秒后入籍，且 `I-012` 不弹信、人直接留下。代码只确认第二点的不弹信 |
| 版本 | 0.1.0 |
| 级别 | S1 |
| 根因 | 反馈 1 的可修路径：态度派系在旧档或中途启用时可能不存在。`Resolve` 返回 null 后，`RHAH_IncidentFacts.Submit`、`TradeEventRouter.TrySpawnTraderCaravan` 和 `RHAH_Envoy` 用 `?? Faction.OfPlayer`。原版 `Pawn.IsColonist` 对人类只看玩家派系，所以一生成就进殖民者栏。地图事件默认每 64 tick 才出队，约一秒，玩家看到的是出现后很快入籍。已改为 `Require`，玩家派系或仍缺失则生成失败。`EnsureAll` 在锁好感前补齐五个态度派系。反馈 2 已确认：`IncidentWorker_RatkinTraderCaravan` 走 `RHAH_IncidentFacts.Submit`。`SpecFor("I-012")` 是 `ChoiceKind.None`，`OffersVisitorControl` 和 `OffersBatchControl` 都不含 `I-012`，`OpenChoice` 直接返回，不发信，也不建 `LordJob_TradeWithColony`。人已生成，挂 `LordJob_RHAH_Visitor`。`I-038` 走 `TradeEventRouter.TrySpawnTraderCaravan`，同样不发信、不开交易。旧鼠灾商人用原派系并开商队，找不到非玩家派系就拒绝生成。反馈 1 未证实：Lord、Job、ThinkTree、信件超时和 `pawn.guest` 都不会在数秒内 `SetFaction(Faction.OfPlayer)`。入籍只发生在选择信的加入、短工、长工，或右键收留。超时是一天后离场。生成请求在态度派系解析失败时会 `?? Faction.OfPlayer`，那是生成当下就是殖民者，不是数秒后。正常开局 `RHAH_Faction_Neutral` 有 `requiredCountAtGameStart=1`，不应解析失败。五个态度派系 `rescueesCanJoin=false`，原版被救者倒地恢复也不会收编 |
| 最小修复 | 已去掉玩家派系回退，并在锁好感前补齐态度派系。`I-012` 与 `I-038` 保持中立态度派系，建原版商队 Lord 并发到达信。不要把它们加进 `OffersVisitorControl`，否则会发来客选择信而不是商队信。态度派系解析失败时事件失败并回滚，不要回退到玩家派系。不要在生成、Lord 到达或信超时里加入籍 |
| 明确不做 | 不补旧鼠灾的售子商队、库存和交易种类。不改已有来客选择信的按钮和结算。不改收留、雇佣到期后的派系清理。不改已在图上的来客。反馈 1 在拿到带事件的存档或日志前不改入籍路径 |
| Language | 无。若商队到达信需要新文案，再补中英键 |
| 存档 | 无新键。已在图上的来客不改写 |
| 归属表 | 否 |
| 回归 | `SpecFor("I-012")` 与 `SpecFor("I-038")` 仍是 `ChoiceKind.None`，`OffersVisitorControl` 和 `OffersBatchControl` 为 false。两条事件成功后派系是 `RHAH_Faction_Neutral`，不是玩家派系，且会发商队到达信。派系解析返回 null 时事件失败，不生成玩家派系 pawn。已有来客信未选择时派系不变，`Timeout` 后离场且仍不是玩家派系 |
| 日志补充 | 玩家日志补充：I-001 已生成，但原版报 `non-downed Baby famine ratkin`，生命阶段是婴儿。鼠族 `HumanlikeBaby` 到 4 岁且 `alwaysDowned`。本模组按人类 3 岁切阶段，3 到 4 岁被标成儿童，生成时撞上永久倒地阶段。已改为婴儿到 4 岁、儿童到 12 岁，会走路下限同步到 4 岁，婴儿阶段允许倒地。另一反馈：入籍后仍像敌对派系搬走家具和物品。原版偷窃包含可拆卸建筑。释放只拆 Lord 和 duty，进行中的偷窃 Job 不停止。`NotifyReleased` 现在同时 `StopAll`。 |
| 后续债 | 五个隐藏态度派系在部分存档里可能没有实例。反馈 1 仍缺可复现存档，不能把“数秒后入籍”写成已定位 |
