# 事件鼠族穿着名单外的原版衣物

| 字段 | 内容 |
| --- | --- |
| 复现 | 新游戏触发本模组地图事件、难民营居民或商队来客，查看身上衣服 |
| 期望 / 实际 | 期望只穿 NewRatkinPlus 与原版的平民衣，品质停在极差、差、一般。实际会留下生成器给出的其它模组衣服，难民营还会再换上 `Apparel_TribalA` |
| 版本 | 0.1.0 |
| 级别 | S2 |
| 根因 | NewRatkinPlus 的 `onlyUseRaceRestrictedApparel` 为 false。旧的过滤只在鼠族名单模式脱掉名单外衣服，不重配，也不限制品质。难民营 `Equip` 在过滤后又强制穿部落服 |
| 最小修复 | 鼠族衣着和原版衣着都清掉生成器衣服，再按年龄重配。来源只留 NewRatkinPlus、原版 Core 和官方 DLC 的中世纪及以下可制材衣物，幼年用固定衣帽。品质、耐久、死者标记和布料权重按旧项目。外部衣服要用 `RHAH_GenerationExtension.allowRefugeeApparel` |
| 明确不做 | 不改武器。不改温度衣的选择和隔热。不清洗已经在场的殖民者、囚犯和世界角色 |
| Language | 无。衣物选项的现有键仍表示同一组模式 |
| 存档 | 新 XML 类型 `HungerAndHavoc.Generation.RHAH_GenerationExtension`。不进 `.rws`，已生成衣服不迁移 |
| 归属表 | 是。类型登记为不单独出现在 `.rws`。`apparelMode` 说明改为重配 |
| 回归 | `RHAH_ApparelPolicyTests`：来源、年龄池、品质上限、材料 |
| 后续债 | 无 |
