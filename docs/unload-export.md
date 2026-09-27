# 卸载导出规划

规划。导出器尚未实现。本页不是已落地行为，也不改 [save-ownership.md](save-ownership.md) 里的卸载动作。动作仍以那一页为准。这里只写将来要做的事，以及开工前必须先改归属表的缺口。

旧项目的玩家流程只作对照。不拷贝其源码、存档字段或 `MouseDisaster` 命名。本模组不读旧鼠灾档。

## 玩家流程

已加载存档时，设置里提供两步。无存档时两步都不可用。

1. **仅此存档停用新内容。** 状态写进 `GameComponent_RHAH_Game`，不改 `RHAH_Settings`。已有来客、信件和世界物体留着。玩家再保存一次，停用才进原档。
2. **备份并导出卸载用副本。** 暂停。先把当前世界存成独立备份，再对序列化副本做 XML 清理。原档不覆盖。正在运行的世界不清理，保持暂停。
3. 成功后退出。只卸本模组，保留鼠族、Harmony、Biotech。重启后加载 `RHAH-removed-*`。确认副本能加载、运行、保存、再加载，再决定是否删旧档。
4. `RHAH-backup-*` 仍含本模组数据。加载备份必须留着本模组。导出失败时备份不是清理副本。

`RHAH_Settings.enableNewContent` 是全局开关，不能当作“仅此存档停用”。

## 明确不做

- 现在不写清理器、不建新目录、不加设置按钮
- 不把导出夹进事件修复或存档键重建
- 不扫描、不转换旧鼠灾存档
- 不删其它模组的特质、种族、基因、Hediff
- 不改全局 `ModSettings`。卸模组后设置文件可以残留
- 不猜测删除未知的本模组 Def 或类型。遇到就中止
- 半成品不改名为可卸载副本

## 所有权

运行时计划只认三样：

- 本包 `ModContentPack.AllDefs`
- 全名以 `HungerAndHavoc.` 开头的序列化类型
- `packageId` `nanaloveyuki.ratkin.hungerandhavoc`

不认 `MouseDisaster`、旧 packageId、旧类型名。`HungerAndHavoc.Guard` 无存档类型，不清理。

`Replace` 的替代 Def 必须已经加载，且不属于本模组。替代 Def 缺失就中止，不挑一个“看起来接近”的。

## 按当前归属表可以删除的

这些行已经是 `Remove`，且删除后不留下必须解析的本模组 Def：

- 身份 Hediff `RHAH_HungerMark` 及其 `CompRHAH_Pawn`。来源、角色、生命周期、`extraData` 一起消失。其它模组写在 `extraData` 里的键不保留
- 本模组其它 Hediff、记忆、精神状态、基因条目、特质、本模组衣物、观音土、观音土账单
- 引用这些东西的当前 Job、排队 Job、预约。Scribe 字典的 keys 与 values 保持等长
- 访客 Lord。Pawn 回到原版 ThinkTree
- 选择信、本模组 LetterDef、进行中的 `RHAH_RefugeeMassacre` 任务及其尚未生成的世界物体
- `Area_RHAH_Relief`。格子不并入家区
- `RHAH_Approach` 整段删除。还在路上的事件不再生成。它没有 Pawn
- `GameComponent_RHAH_Game` 与各地图的 `MapComponent_RHAH_Map` 整段删除。生成队列里还没落地的角色随队列取消。叙事计数、穗音深存档、事件链随游戏组件删除
- 清理副本的 meta 去掉本包。不改备份的 meta

二次清理同一份副本必须得到同一份 XML。

## 开工前必须改归属表

下面几行现在是 `Remove`，按字面导出后副本无法加载，或和“留下鼠族、保留鼠族前置”冲突。实现前先改 [save-ownership.md](save-ownership.md)，写出替代 Def。没改表就不写清理器。

| 现状 | 问题 | 建议，尚未生效 |
| --- | --- | --- |
| `RHAH_PawnKind_Ratkin`：Remove，且已生成 pawn 的 kindDef 不迁移 | 人还在，种类 Def 没了 | 保留 pawn 的 thingID。`kindDef` 改成仍会随 NewRatkinPlus 加载、种族同为 `Ratkin` 的 PawnKind。没有这种种类就中止。不换成人类 |
| `RHAH_Faction_*` 五个态度派系：Remove，不换成原版派系 | 派系上的人和派系引用都会悬空 | 派系 loadID 保留，Def 改成已加载且不属于本模组的派系。优先原版 `Ancients`。目标缺失就中止。玩家派系和其它模组派系不改 |
| `RHAH_History_*`：Remove，不换成原版背景 | 童年或成年槽仍指向本模组背景 | 同槽换成已加载、不属于本模组的 Backstory。按 defName 序选第一个，不按技能挑。背景带来的技能变化不保留。没有同槽背景就中止 |
| `RHAH_Xenotype_Ratkin`：Remove，不换成原版异种 | 异种 Def 仍挂在 pawn 上 | 异种引用改成已加载的 `Baseliner`。本模组基因从基因列表删除，其它模组基因留下。`Baseliner` 未加载就中止 |
| `RHAH_Suiyin`：动作列是 Remove，注释却写换成兰迪 | 动作和注释不一致，替代 defName 没登记 | 改成 Replace，目标写死已加载的原版兰迪 `RandyRandom`。未加载就中止 |
| `RHAH_RefugeeCamp`：Remove，不换成原版地点 | 已生成地图会失去父物体。同名的 WorldObjectDef、SitePartDef、MapGeneratorDef、GenStepDef 不能混删 | 没有地图、也没有居民：删整个世界物体。已有地图或居民：保留世界物体 ID、地图和居民，类名与 Def 改成原版 `Site`，去掉本模组部件和生成步骤。`Site` 不能接住已生成地图就中止，不删地图。同名地点部件单独处理，不把它当成世界物体 |

特质、本模组 Hediff、衣物、信件、任务保持 Remove。不要为了“好看”换成原版同名物。

## 文件与失败

- 备份名 `RHAH-backup-*`，副本名 `RHAH-removed-*`。已存在同名时换新名字，不覆盖玩家存档，也不覆盖上一次导出
- 先写入临时路径。写完并完成清理后才变成 `.rws`
- 未知的本模组 Def 名、`HungerAndHavoc.` 类型、或已删除对象的残留引用：中止，记下路径和名字
- 失败时设置和日志给出原因。不要把备份或半截临时文件当成清理副本
- 原档、运行中的世界、全局设置都不因为失败而改掉。停用标志如果已经写入内存，仍要等玩家自己再存一次才进原档

## 入口

- 原版设置页：只有 `Current.Game` 存在时显示两步
- IrisMenus：单独一页，缺 IrisMenus 时不注册。现有 15 个 SubItem 不挪
- 文案走 Keyed，中英成对。键用 `RHAH_Removal*`。确认文案写明：暂停、不覆盖原档、运行中的世界不清理、未知引用会中止、成功后退出并只卸本模组

## 验证

合成 `.rws` 做结构测试，不写玩家存档：

- 本模组组件、信件、Hediff、Lord、区域被删
- 非本模组组件和 pawn thingID 还在
- 字典 keys/values 等长
- 未知本模组标量中止
- 同一份 XML 再清一次不变

这些测试不能代替卸模组后的加载、运行、保存、再加载。那一步仍是手工验收。

## 将来的代码落点

不要为规划建空目录。真做时：

- XML 清理放 `Source/Core/`，`internal`，不进 API，不 Tick
- 停用标志和导出入口放在现有 `GameComponent_RHAH_Game`。新键登记进归属表，动作为 Remove，因为清理时整个组件会被删掉
- 设置按钮在 `Source/Core/ModEntry.cs`。IrisMenus 页仍在 `Source/Pawn/Compat/`

## 时机

0.1.0 不实现。归属表里的 Replace 还没定，存档键也还在 1.0.0 前的破坏性重建里。

开始写代码的条件：

1. 上一节那六行归属已经改成可执行的 Remove 或 Replace
2. 这次改动不夹带事件、叙事或设置重构
3. 合成存档测试先失败，再写清理器
