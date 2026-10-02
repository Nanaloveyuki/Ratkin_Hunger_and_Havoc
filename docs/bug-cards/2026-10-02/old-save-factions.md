# 中途加入存档初始化

| 字段 | 内容 |
| --- | --- |
| 复现 | 不含本模组派系的游戏加入本模组，加载完成尚未普通 tick 时调用地图/商队事件 |
| 期望 / 实际 | 加载完成即有五派系且不重复；目前只在 tick 惰性补齐，少数必需派系事件仅 Resolve |
| 版本 | 1.0.0，基线 60967e8，未正式发布 |
| 级别 | S2 |
| 根因 | 初始化时点与必需派系查询契约不一致；五个 Def 均 hidden=true，原版 Faction.HasGoodwill 为 !Hidden && !temporary，CanChangeGoodwillFor 因此拒绝 TryAffectGoodwillWith，原有 LockGoodwill 无法把敌对派系的初始 -80 固定到 -100 或恢复已有关系；未证实当前玩家存档缺派系 |
| 最小修复 | GameComponent.FinalizeInit 调现有 LockGoodwill 补齐派系；复用 PinPair 固定双向玩家关系；商队劫掠、营地居民与营地任务的必需生成入口改 Require |
| 明确不做 | 不导入旧鼠灾字段，不假定暂停必然缺派系，不改既有派系身份 |
| Language | About/README 中英中途加入说明按实现更新 |
| 存档 | 沿用原版派系存档，无迁移或破坏性重建 |
| 归属表 | 不变 |
| 回归 | 实际 FinalizeInit / FactionGenerator 离线 smoke 通过：全缺补齐、部分已有保留、重复不创建、双向关系固定、Resolve 不创建，商队劫掠和营地必需入口补派系。新增测试宿主 DefOf 初始化问题已修，按用户要求不再重跑，最终静态审查验收 |
| 后续债 | 离线 smoke 使用最小世界与 Def，未覆盖完整游戏加载、原生世界网格、启用 Ideology 的派系生成、营地任务/站点/地图或 Pawn 生成；完整中途加入场景仍须实机确认 |
