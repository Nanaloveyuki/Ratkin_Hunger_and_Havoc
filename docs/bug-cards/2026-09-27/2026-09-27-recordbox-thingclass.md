# 旧箱子类型名解析失败，无法开局

| 字段 | 内容 |
| --- | --- |
| 复现 | 启用本模组后进主菜单。点新游戏到剧本页，或用 HugsLib 快速开图。已有存档同样在构造 `Game` 时停住 |
| 期望 / 实际 | 期望能进入剧本页并生成地图。实际 `RHAH_RecordBox` 的 `thingClass` 解析失败，`ReadingPolicyDatabase.GenerateStartingPolicies` 对空类型调用 `SameOrSubclassOf<Book>()`，空引用中断开局 |
| 版本 | 0.1.0 |
| 级别 | S0 |
| 根因 | `1.6/Defs/ThingDefs/RHAH_RecordBox.xml` 写 `HungerAndHavoc.Narrative.Building_RHAH_RecordBox`。类型在 `HungerAndHavoc.Storyteller.Suiyin`。Verse 把找不到的类型留成 null。原版 1.6 生成起始阅读策略时不判空 |
| 最小修复 | XML `thingClass` 改成实际全名。补公开类型例外和卸载归属。测试断言 Def 里的全名能在实现程序集里解析，且是 `Building` 子类 |
| 明确不做 | 不改箱子行为、不改 `RHAH_MigrationRecord`、不给原版 `GenerateStartingPolicies` 打补丁、不处理 Gagarin 因 `RHAH_Approach.xml` 变化而停缓存 |
| Language | 无 |
| 存档 | 无新键。类型名是 ThingDef 契约，箱子实例按 `Building` 写入地图，不另加字段 |
| 归属表 | 是。补 `RHAH_RecordBox`、`RHAH_MigrationRecord`，以及存放点已有但未登记的 `RHAH_RecordSite`、`RHAH_RecordLetter` |
| 回归 | `RHAH_RecordBox` 的 `thingClass` 解析到 `HungerAndHavoc.Storyteller.Suiyin.Building_RHAH_RecordBox` |
| 后续债 | 无 |
