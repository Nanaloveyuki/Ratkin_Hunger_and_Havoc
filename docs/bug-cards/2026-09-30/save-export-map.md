# 卸载地图归属和未知类型保护

状态：已实现，验证未执行

| 字段 | 内容 |
| --- | --- |
| 复现 | 有已生成难民营或记录地点时导出；含未知 HungerAndHavoc 类型的副本清理。来自源码对照；卸载清理已运行隔离复现，其它尚未游戏内复现 |
| 期望 / 实际 | 应保留地图及父地点并清除自有生成器引用，未知类型中止；实际 generatorDef 报错、父地点误删、未知类型通过 |
| 版本 | 本仓库 1.0.0，2026-09-30 当前工作树；保留用户并发改动 |
| 级别 | S0 |
| 根因 | `Map.ExposeData` 把生成器写在地图 `generatorDef`，父地点写在 `maps/li/mapInfo/parent`，值是 `WorldObject_<ID>`。清理只把世界物体上的 `map`/`mapParent` 子节点当已生成地图，真实父引用被当成无人地点删掉；自有 `generatorDef` 留在地图上，卸模组后 `Scribe_Defs` 解析失败。未知 `HungerAndHavoc.*` 类型不在已登记类集合里，收尾扫描会放行 |
| 最小修复 | `RHAH_SaveCleanup` 用 `mapInfo/parent` 认地图并保留父地点；只通过 `MapGeneratorReplacements` 把该地图的自有 `generatorDef` 换成已加载且不属于本模组的 `Site.mapGenerator`。原版 `Site` 为空时 `RequireDef<MapGeneratorDef>("Encounter")`，并检查 `modContentPack` 与 `AllDefs`。未知 `HungerAndHavoc.*` 在任何删除或替换前中止 |
| 明确不做 | 不部署，不修改外部 Mod，不改包名/API/事件显示 ID，不夹带重构。不删除有人地图，不改外部地图的生成器或物品，不把生成器替换放进通用 `Replacements` |
| Language | 无 |
| 存档 | 无新键、不破坏重建；保留已有记录 |
| 归属表 | 必须更新。`RHAH_RefugeeCamp` 与 `RHAH_RecordSite` 的 MapGeneratorDef 会作为地图 `generatorDef` 写入 `.rws`，动作是 Replace，不是“不单独出现” |
| 回归 | `Source/Tests/RHAH_SaveCleanupTests.cs` 覆盖真实父引用、生成器替换、二次清理、未知类型中止和外部地图不动。隔离冒烟把 `Source/Core/RHAH_SaveCleanup.cs` 与 `/tmp/RHAH_SaveExportMapSmoke.cs` 编译进同一个 exe，不引用 `HungerAndHavoc.dll`，从而直接调用 internal `Clean`。成功路径保留外部 `Base_Player`；自有生成器留在已删除地点上是单独的预期失败。两项都未执行 |
| 后续债 | 本卡外相邻问题不纳入；游戏内验证边界明确报告 |
