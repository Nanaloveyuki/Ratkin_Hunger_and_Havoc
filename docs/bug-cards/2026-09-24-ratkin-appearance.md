# 鼠族生成外观不受 NewRatkinPlus 限制

| 字段 | 内容 |
| --- | --- |
| 复现 | 新游戏触发本模组地图事件、难民营居民或商队来客 |
| 期望 / 实际 | 期望使用鼠族头、`RK_Style` 发型、无胡须、无纹身，衣服只来自 NewRatkinPlus 的 `apparelList` 与 `whiteApparelList`。实际是智人头、任意发型、胡须、纹身和原版衣服 |
| 版本 | 0.1.0 |
| 级别 | S2 |
| 根因 | 事件、难民营和交易都用 `PawnKindDefOf.Colonist`。HAR 只在 `pawn.def` 为 `ThingDef_AlienRace` 时过滤头型与样式。NewRatkinPlus 的 `onlyUseRaceRestrictedApparel` 为 false，即使换成 `Ratkin`，生成器仍会穿非鼠族衣服 |
| 最小修复 | 缺省请求改用 `RHAH_PawnKind_Ratkin`。生成后按 NewRatkinPlus 的 `Ratkin` 种族设置重抽头、发、胡须、纹身，并清掉名单外的衣服。显式衣服仍覆盖这一步 |
| 明确不做 | 不改调用方传入的非空 `PawnKind`。不实现 ADR 里未落地的 `allowRefugeeApparel`。不改基因页、经历和特质 |
| Language | 无。`PawnKind` 标签只给调试，玩家信件不显示它 |
| 存档 | 新 Def `RHAH_PawnKind_Ratkin`。已生成 pawn 的 `kindDef` 不迁移 |
| 归属表 | 是。`Remove`，不替换成原版 PawnKind |
| 回归 | `RHAH_RatkinAppearanceTests`：缺省种族、名单过滤、显式衣服保留 |
| 后续债 | 难民营 `Equip` 仍会在过滤后换上原版 `Apparel_TribalA` 和原版木制武器。另开 |
