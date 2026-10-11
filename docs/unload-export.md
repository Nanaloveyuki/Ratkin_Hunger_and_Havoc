# 卸载导出

已落地。卸载动作以 [save-ownership.md](save-ownership.md) 为准。本页只写玩家流程和清理器实际做的事。

旧项目的玩家流程只作对照。不拷贝其源码、存档字段或 `MouseDisaster` 命名。本模组不读旧鼠灾档。

## 玩家流程

已加载存档时，原版设置页和 IrisMenus「存档与卸载」页都提供两步。无存档时两步都不可用。

1. **仅此存档停用新内容。** 状态写进 `GameComponent_RHAH_Game.newContentDisabled`，不改 `RHAH_Settings.enableNewContent`。ODF 中本模组的 provider 同时设为 Disabled，停止事件后续和待到达实例，但保留实例、已有来客、信件和世界物体。玩家再保存一次，停用才进原档。
2. **备份并导出卸载用副本。** 暂停，先把当前状态存成独立备份，再把停用标志写入内存并禁用本 provider；备份保留点击导出前的停用状态。随后保存并清理序列化副本。原档不覆盖。正在运行的世界不清理，保持暂停。
3. 成功后退出。只卸本模组，保留鼠族、Harmony、Biotech 和 Oxygen DSL Framework。重启后加载 `RHAH-removed-*`。确认副本能加载、运行、保存、再加载，再决定是否删旧档。
4. `RHAH-backup-*` 仍含本模组数据。加载备份必须留着本模组。导出失败时备份不是清理副本。

事件链只由 ODF 执行，不保留旧 RHAH 引擎。已有饥与祸存档在加载时自动迁移旧记录，保持 ID、阶段、开始时间、期限、payload、已选结果和长事件检查相位，不重新抽取随机结果。迁移失败时暂停并显示原因，原 `eventChains`、`nextEventChainId` 和地图 `predationPendingTick` 保留且只在失败时存回。已存在的 ODF provider 分区（包括显式空分区）优先；`MigrationStatus=failed:...` 的诊断分区继续保留失败原记录。详细转换见 [save-ownership.md](save-ownership.md)。


## 明确不做

- 不扫描、不转换旧鼠灾存档
- 不删其它模组的特质、种族、基因、Hediff
- 不改全局 `ModSettings`。卸模组后设置文件可以残留
- 不猜测删除未知的本模组 Def 或类型。遇到 `RHAH_` 标量、`HungerAndHavoc.` 类型或已删除对象的残留引用就中止
- 半成品不改名为可卸载副本

## 所有权

RHAH 运行时清理计划按以下来源识别本模组所有物；ODF provider 分区通过框架公开 API 单独移除：

- 本包 `ModContentPack.AllDefs`
- 实现程序集里的类型全名
- `packageId` `nanaloveyuki.ratkin.hungerandhavoc`

不认 `MouseDisaster`、旧 packageId、旧类型名。`HungerAndHavoc.Guard` 无存档类型，不清理。

`Replace` 的替代 Def 必须已经加载，且不属于本模组。替代 Def 缺失就中止，不挑一个“看起来接近”的。

ODF 根组件、根 `eventChainSchemaVersion` 和框架类型属于 Oxygen DSL Framework，不属于 RHAH。清理只调用 `Oxygendsl.Mod.Events.OxygenEventChainHost.RemoveProviderState(document, "nanaloveyuki.ratkin.hungerandhavoc")`，删序列化副本的 `eventChainProviders` 中这个 provider 的整项。保留其它 provider（包括空分区）的数据，以及 ODF save meta；不移除 ODF 模组、不改 live runtime。框架 API 返回移除的分区数，供导出计数使用。
其它清理扫描跳过整个 ODF 根组件；其它 provider 的 payload 即使含 `RHAH_` 文本、对象引用或未知类型名，也不替换、不删除、不作为 RHAH 残留中止条件。



## 删除

这些行是 `Remove`，删除后不留下必须解析的本模组 Def：

- 身份 Hediff `RHAH_HungerMark` 及其 `CompRHAH_Pawn`。来源、角色、生命周期、`extraData` 一起消失。其它模组写在 `extraData` 里的键不保留
- 本模组其它 Hediff、记忆、精神状态、本模组基因条目、特质、本模组衣物、观音土、观音土账单
- 引用这些东西的当前 Job、排队 Job、预约。Scribe 字典的 keys 与 values 保持等长
- 访客 Lord。Pawn 回到原版 ThinkTree
- 选择信、本模组 LetterDef、进行中的 `RHAH_RefugeeMassacre` 任务及其尚未生成的世界物体
- `Area_RHAH_Relief`。格子不并入家区
- `RHAH_Approach` 整段删除。还在路上的事件不再生成。它没有 Pawn
- `GameComponent_RHAH_Game`、`NarrativeState` 与各地图的 `MapComponent_RHAH_Map` 整段删除。生成队列里还没落地的角色随队列取消。叙事计数、穗音深存档和失败迁移的旧事件链随组件删除
- ODF 中仅删除 RHAH provider 分区，连同事件链、待到达实例、receipts 和迁移诊断；ODF 根组件及其它 provider 保留
- 清理副本的 meta 去掉本包。不改备份的 meta

二次清理同一份副本必须得到同一份 XML。

## 替换

| 对象 | 动作 |
| --- | --- |
| `RHAH_PawnKind_Ratkin` | 保留 pawn 的 thingID。`kindDef` 改成已加载、种族同为 `Ratkin`、不属于本模组的 PawnKind。按 defName 序选第一个。没有就中止，不换成人类 |
| `RHAH_Faction_*` | 派系 loadID 保留，Def 改成已加载的原版 `Ancients`。目标缺失或 `Ancients` 属于本模组就中止。玩家派系和其它模组派系不改 |
| `RHAH_History_*` | 同槽换成已加载、不属于本模组的 Backstory。按 defName 序选第一个，不按技能挑。背景带来的技能变化不保留。没有同槽背景就中止 |
| `RHAH_Xenotype_Ratkin` | 异种引用改成已加载的 `Baseliner`。本模组基因从基因列表删除，其它模组基因留下。`Baseliner` 未加载就中止 |
| `RHAH_Suiyin` | 换成已加载的原版 `Randy`。未加载就中止 |
| `RHAH_RefugeeCamp` | 没有地图、也没有居民：删整个世界物体。已有地图或居民：保留世界物体 ID、地图和居民，类名与 Def 改成原版 `Site`，去掉本模组部件和生成步骤。类名不是 `Site`、也不是本模组难民营类时中止，不删地图。同名地点部件单独删除 |
| `RHAH_RecordSite` | `worldObjectClass` 已是原版 `Site`。没有地图也没有居民：删整个世界物体。已有地图或居民：保留 ID、地图和居民，Def 改成原版 `Site`，去掉本模组地点部件。同名地点部件单独删除 |

已有地图按 `maps/li/mapInfo/parent` 的 `WorldObject_<ID>` 引用判定，不查世界物体自身的 `map` 字段。保留地点 ID 和地图父引用，并将对应地图的自有 `generatorDef` 替换为已加载、非本模组的 `Site.mapGenerator`；该字段为空时使用原版 `Encounter`。替代 Def 缺失时中止，不删除生成器字段，不改其它地图。

特质、本模组 Hediff、衣物、信件、任务保持 Remove。

## 文件与失败

- 备份名 `RHAH-backup-*`，副本名 `RHAH-removed-*`。已存在同名时换新名字，不覆盖玩家存档，也不覆盖上一次导出
- 清理副本先写入临时路径。写完并完成清理后才变成 `.rws`
- 未知的 `RHAH_` 标量、`HungerAndHavoc.` 类型、或已删除对象的残留引用：中止，记下路径和名字。未知本模组类型在任何删除或替换前检查
- 失败时设置和日志给出原因。不要把备份或半截临时文件当成清理副本
- 原档、运行中的世界、全局设置都不因为失败而改掉。停用标志如果已经写入内存，仍要等玩家自己再存一次才进原档

## 入口

- 原版设置页：只有 `Current.Game` 存在时两步可用。无存档时显示说明
- IrisMenus：单独一页 `removal`，缺 IrisMenus 时不注册。现有页面不挪
- 文案走 Keyed，中英成对。键是 `RHAH_Removal*`。确认文案写明：暂停、先备份当前状态再停用、不覆盖原档、运行中的世界不清理、未知引用会中止、成功后退出并只卸本模组，保留 Oxygen DSL Framework 和其它依赖

## 代码

- XML 清理在 `Source/Core/RHAH_SaveCleanup.cs`，`internal`，不进 API，不 Tick
- 停用标志和导出入口在 `GameComponent_RHAH_Game` 与 `Source/Core/RHAH_SaveExport.cs`。`newContentDisabled` 已登记，动作为 Remove，因为清理时整个组件会被删掉
- ODF 分区清理只用 `OxygenEventChainHost.RemoveProviderState`；不把 ODF 类型并入 RHAH 所有权计划，不自行删除框架根节点
- 设置按钮在 `Source/Core/ModEntry.cs`。IrisMenus 页在 `Source/Pawn/Compat/RHAH_IrisMenusCompat.cs`

## 验证

`RHAH_SaveCleanupTests` 用合成 `.rws` 做结构测试，不写玩家存档：

- 本模组组件、信件、Hediff、Lord、区域被删
- 非本模组组件和 pawn thingID 还在
- 字典 keys/values 等长
- 未知 `RHAH_` 标量中止
- 同一份 XML 再清一次不变
- 使用原版 `mapInfo/parent` 和 `generatorDef` 结构的已生成地点保留地图归属，替换生成器
- 未知 `HungerAndHavoc.*` 类型在修改副本前中止
- RHAH provider 分区删除，ODF 根组件、根 schema、其它 provider 和 ODF meta 保留；失败迁移的原记录随 RHAH 组件删除

2026-10-11 ODF 切换验收在独立 RimWorld 1.6.4871 中完成：真实导出备份先保存 Active 分区，再停用 live provider；已有来客、捕食者和叙事事实保留。清理副本只删 RHAH 分区，未知 owner 原 XML、ODF 根、schema 和 meta 保留。随后用不启用 RHAH 及其闸门样例的新进程，实际加载清理副本、运行 10 tick、保存、重新加载、再运行 2 tick。原玩家安装、配置和存档未覆盖。此前 2026-09-29 的验收不作为此次切换证据；本次原始回执见相邻 ODF 仓库的 `tmp/eventchain-acceptance/`。
