# 工程标准

目标规范。当前仓库中的代码可能仍处于标准制定前的草案状态，不构成规范依据。发布前允许为落实本规范进行 breaking change。产品名、显示 ID、Def 前缀见 [naming.md](naming.md)。身份行为见 [pawn.md](pawn.md)。事件目录见 [incidents.md](incidents.md)。存档键和卸载归属见 [save-ownership.md](save-ownership.md)。修 bug 见 [bug-handling.md](bug-handling.md)。跨版本决策写 [adr/](adr/)。注释规则见仓库根 `Agents.md`。

## 规范优先级

发生冲突时按以下顺序解释：

1. 本页工程标准
2. 具体领域活文档，如 `pawn.md`、`naming.md`、`incidents.md`、`save-ownership.md`
3. 已接受 ADR
4. [bug-handling.md](bug-handling.md) 的修复范围与门禁
5. 当前实现、旧脚手架和历史代码

文档描述目标架构。M0 的程序集、命名空间和 API 边界已经按本页落地。以后的实现继续服从本页，不得为了匹配更旧的草案降低标准。

## 仓库分类

| 位置 | 用途 |
| --- | --- |
| `Source/{Layer}` | 实现源码，目录与实现命名空间对应 |
| `Api/` 或 API 项目目录 | 稳定对外契约源码，构建 `HungerAndHavoc.Api.dll` |
| `Guard/Source` | 冲突提示程序集，命名空间 `HungerAndHavoc.Guard` |
| `1.6/` | 当前版本 Def、程序集、Patch 和发布内容 |
| `Languages/` | 跨版本 Keyed / DefInjected |
| `docs/` | 活文档与 ADR |
| `scripts/` | 检查、构建和部署脚本 |
| `tmp/` | 临时文件，已 gitignore，不当源码或发布输入 |

已有层是 Core、Identity、Generation、Incidents、Pawn、Narrative、Trade、Data、Tests。World、Patches、UI 等有第一个类型再建模，不建空目录。不要再建 `Source/Behavior`。IrisMenus 绘制留在 `Source/Pawn/Compat`，和注册一起在缺少 IrisMenus.dll 时排除。不建 `Source/Settings` 或 `Source/UI`：设置数据仍在 `Core`，函数求值留在对应领域。`Source/Data` 使用 `HungerAndHavoc.Data`，只放经历和特质的静态记录与选择，不引用存档或设置。

## 分层与程序集

目标程序集边界：

- `HungerAndHavoc.Api.dll`：稳定公开契约，不引用实现程序集
- `HungerAndHavoc.dll`：RimWorld 实现，引用 API 程序集，包含 Identity、Core、Incidents、Generation、Pawn、Narrative、Trade 和运行时实现
- `HungerAndHavocGuard.dll`：独立冲突提示程序集，不作为业务 API
- 测试程序集：仅测试用途，不作为模组运行时依赖

依赖方向只能是实现依赖 API。其它模组只引用 `HungerAndHavoc.Api.dll`。

`HungerAndHavoc.Api.dll` 可以引用 `Assembly-CSharp` 与 `UnityEngine.CoreModule`，`Private=False`。API **不得**引用 `HungerAndHavoc.dll`、Harmony、Guard、其它模组程序集。公开方法可以使用 `Pawn`、`ThingDef` 等基础游戏类型。公开表面 **不得**出现 `CompRHAH_Pawn`、`Hediff_RHAH_Mark`、`RHAH_Race`、Job、Worker、DefOf、实现命名空间类型。

`Source/{Layer}/Foo.cs` 的命名空间必须是 `HungerAndHavoc.{Layer}`。`Source/Core` 使用 `HungerAndHavoc.Core`，`Source/Identity` 使用 `HungerAndHavoc.Identity`，`Source/Generation` 使用 `HungerAndHavoc.Generation`，`Source/Incidents` 使用 `HungerAndHavoc.Incidents`，`Source/Pawn` 使用 `HungerAndHavoc.Pawn`，`Source/Pawn/Compat` 使用 `HungerAndHavoc.Pawn.Compat`，`Source/Narrative` 使用 `HungerAndHavoc.Narrative`，`Source/Trade` 使用 `HungerAndHavoc.Trade`，`Source/Tests` 使用 `HungerAndHavoc.Tests`。API 类型使用 `HungerAndHavoc.Api`，并放在 API 项目目录。`RHAH_IrisMenusWidgets` 只画卡片、可拖拽份额条和只读份额柱，不保存设置，不引用领域求值。

实现目录中未被 Verse 反射、XML 或 Def 创建要求的类型默认 `internal`。因 Verse 需要跨程序集创建而必须 `public` 的类型，只是反射入口，不因此成为稳定 API。

## 稳定 API

API 程序集的公开类型采用白名单，当前目标包括：

- `RHAH_Api`
- `IRHAH_Pawn`
- `RHAH_PawnSnapshot`
- `IRHAH_PawnBehavior`
- `RHAH_PawnBehaviors`
- `RHAH_PawnSeed`
- `RHAH_BehaviorGate`
- `RHAH_PawnRole` / `RHAH_Lifecycle` / `RHAH_ReleaseReason` / `RHAH_Attitude`
- `RHAH_Api` 的经历与特质查询：`IsOwnedHistory`、`IsOwnedTrait`、`TryGetHistory`、`TryGetTrait`、`CopyHistoryIds`、`CopyTraitIds`。参数是显示 ID 或 defName，不返回 Backstory、Trait 或 Data 记录
`IRHAH_Pawn` 是只读快照。只读态度有两个：`AttitudeAtArrival` 是到达时快照，`Attitude` 是当前态度。批次反应只改当前态度。快照、事件和 `IRHAH_PawnBehavior` 不得返回或接收 `CompRHAH_Pawn`、Hediff、私有 Job 或其它实现对象。

状态修改只通过 `RHAH_Api`：`SetLifecycle`、`SetGate`、`SetExtra`、`ReleaseToColony`、`TryMarkOrigin`。API 事件参数必须使用稳定类型或基础游戏类型，不得暴露实现程序集类型。

鼠族判定通过 `RHAH_Api.RegisterRatkinMatcher` 注册。不得公开或把 `RHAH_Race.Register` 作为跨模组契约。

改公开 API 签名、成员语义、事件顺序、闸门优先级或存活周期时，必须先更新本页、[pawn.md](pawn.md) 和对应 ADR，再修改实现。

## 字段、属性与存档

普通代码：

- 类型、方法、属性、事件、枚举、常量：PascalCase
- 接口：`I` 前缀
- 私有/静态字段、局部变量、参数：camelCase，不加 `_` / `m_`
- Seed / DTO 的公开成员：PascalCase
- 不可变目录项优先使用 PascalCase 属性
- 一个文件一个主类型；同一概念的相关枚举可共用文件

存档规则适用于 Comp、Settings、GameComponent、MapComponent 及其它持久化类型：

- 新存档键默认使用 camelCase
- 新字段默认值必须与 `Scribe_*.Look` 默认值一致
- 集合在 `PostLoadInit` 补齐空集合，且必须定义 null 与空集合的语义
- 计算状态使用 PascalCase 属性，不写入存档
- 存档键是格式契约，不自动等同于 C# 字段名
- 发布前允许重命名、删除或重建实验性存档键，但必须在修复卡写明“迁移”或“破坏性重建”，禁止默默改名
- 首次正式发布后，存档键冻结；字段重命名必须兼容旧键或执行显式迁移
- 迁移必须说明旧键、新键、默认值、旧存档行为和失败处理
- 当前键、XML 类型名和卸载动作登记在 [save-ownership.md](save-ownership.md)

“首次正式发布”指第一个面向用户发布的稳定版本，默认以 `1.0.0` 或更高版本作为冻结边界。

## 命名与格式

- Allman 括号、显式类型、块命名空间
- 标识符使用英语
- 需要注释时使用短中文，句末无标点；注释只解释意图、约束或非显然事实
- Def / Keyed 使用 `RHAH_`，规则见 [naming.md](naming.md)
- XML `workerClass` 使用 `HungerAndHavoc.Incidents.IncidentWorker_*`
- 禁止 `MouseDisaster`、`RHH_`、`RatkinEgg`、`RHAH_PawnRole.Egg`
- 不以 XML Doc、EditorConfig、StyleCop 或 nullable 作为当前强制前提；若未来启用，必须通过 ADR 统一

## 方法、运行时与性能

- 方法应有单一主要责任；状态转换、校验、事件通知和持久化协同可以保留在同一事务边界内
- 禁止新增无法命名职责、无法独立测试或承担多个领域决策的万能 Utility
- 模组协作只走稳定 API，不把私有方法、字段、反射路径或 XML 实现细节当合约
- 访客 JobGiver 只问 `RHAH_Api.Allows`，不在 JobGiver 中复制角色规则
- 全局运行时组件默认由 `GameComponent_RHAH_Game` 与 `MapComponent_RHAH_Map` 承担；增加其它全局组件必须登记职责、生命周期和存档范围
- Tick 热路径包括每 tick 或高频批量执行的 Pawn、Map、组件和 Job 查询
- Tick 热路径默认避免 LINQ、闭包、装箱、重复字符串拼接和临时集合；使用 `for`、缓存和可复用缓冲区时必须保持可读性
- 性能约束以代码审查、分配分析或基准结果验证，不以机械行数或圈复杂度阈值替代判断

## Harmony 与外部联动

- Harmony 默认只补原版或种族框架的明确缺口
- 其它模组补丁默认禁止
- 必须联动时，例外登记必须包含目标模组、目标类型或方法、原因、补丁类型、加载顺序、兼容范围和失败策略
- 目标类型不存在、签名变化或前置条件不满足时，不得静默产生错误行为
- 优先使用 Prefix/Postfix；使用 Transpiler 必须说明无法采用更小补丁的原因
- 补丁不得把私有实现升级为对外契约

## 构建、测试与发布

- 每个游戏版本使用独立目录、Def、程序集和发布配置，不把版本内容放在仓库根目录
- TargetFramework、RimWorld 参考程序集、Harmony 版本和 API 依赖必须可复现并记录在项目配置或发布说明中
- 源码、文档、检查脚本和必要的许可证文件可提交；`bin/`、`obj/`、PDB、临时文件和本地游戏目录不作为源码提交
- 发布包只包含目标版本所需的 `About/`、版本目录、语言、程序集、补丁和许可证内容
- 发布前必须通过构建、单元测试、脚手架检查、XML/Def/语言键检查、API 白名单检查和存档契约检查
- API、存档、事件目录、闸门优先级、XML 类型名和关键 Harmony 目标属于阻断级测试范围
- 检查失败即阻止合并或发布；风格和性能建议可以单独报告，但不得掩盖契约失败

## 例外登记

| 例外 | 原因 | 必须记录 |
| --- | --- | --- |
| Verse/XML 反射所需的 public 类型 | 游戏运行时跨程序集创建 | 类型、创建入口、所在程序集、为何不能 internal |
| 其它模组 Harmony | 无稳定 API 可用的联动缺口 | 模组、目标类型、原因、补丁方式、版本范围、失败策略 |
| 发布后的存档键迁移 | 保留用户存档兼容性 | 旧键、新键、迁移步骤、默认值和失败处理 |
| 其它 public 实现类型 | 具体框架或运行时要求 | 类型、依赖方、替代 API 和移除条件 |

例外必须记录在本页或对应 ADR。没有登记的例外视为违反标准。实现类型默认 `internal`。下表是 M0 因 XML / Verse 反射必须保持 `public` 的实现类型，不属于稳定 API，其它模组不得依赖。

| 类型 | 创建入口 | 所在程序集 | 为何不能 internal |
| --- | --- | --- | --- |
| `HungerAndHavoc.Identity.Hediff_RHAH_Mark` | HediffDef `hediffClass` | `HungerAndHavoc.dll` | Verse 按 XML 全名跨程序集创建 Hediff |
| `HungerAndHavoc.Identity.CompRHAH_Pawn` | `HediffCompProperties.compClass` | `HungerAndHavoc.dll` | Verse 按 `compClass` 创建 HediffComp；类型名写入 `.rws` |
| `HungerAndHavoc.Identity.CompProperties_RHAH_Pawn` | Def XML `Class=` | `HungerAndHavoc.dll` | Verse 按 XML `Class` 反序列化 CompProperties |
| `HungerAndHavoc.Identity.RHAH_RaceExtension` | ThingDef `modExtensions` XML `Class` | `HungerAndHavoc.dll` | Verse 按 DefModExtension XML `Class` 创建 |
| `HungerAndHavoc.Core.RHAH_Mod` | Verse 扫描 `Mod` 子类 | `HungerAndHavoc.dll` | 模组入口必须可被 Verse 发现并构造 |
| `HungerAndHavoc.Core.RHAH_Settings` | `Mod.GetSettings<T>()` | `HungerAndHavoc.dll` | Verse 按类型参数创建 `ModSettings` |
| `HungerAndHavoc.Core.RHAH_DefOf` | `[DefOf]` 静态字段 | `HungerAndHavoc.dll` | `DefOfHelper` 反射绑定公开静态 Def 字段 |
| `HungerAndHavoc.Core.HarmonyBootstrap` | `[StaticConstructorOnStartup]` | `HungerAndHavoc.dll` | Verse 启动扫描公开静态构造入口 |
| `HungerAndHavoc.Pawn.LordJob_RHAH_Visitor` | Lord `lordJob` / `LordMaker` | `HungerAndHavoc.dll` | Verse 按类型创建并存档 Lord |
| `HungerAndHavoc.Pawn.JobDriver_RHAH_Beg` | JobDef `driverClass` | `HungerAndHavoc.dll` | Verse 按 XML 全名创建 JobDriver |
| `HungerAndHavoc.Pawn.JobDriver_RHAH_Gnaw` | JobDef `driverClass` | `HungerAndHavoc.dll` | Verse 按 XML 全名创建 JobDriver |
| `HungerAndHavoc.Pawn.ThinkNode_ConditionalRHAH_Visitor` | ThinkTree / Duty XML `Class` | `HungerAndHavoc.dll` | Verse 按 XML 全名创建 ThinkNode |
| `HungerAndHavoc.Pawn.JobGiver_RHAH_Visitor` | ThinkTree / Duty XML `Class` | `HungerAndHavoc.dll` | Verse 按 XML 全名创建 ThinkNode |
| `HungerAndHavoc.Pawn.JobGiver_RHAH_Beg` | Duty XML `Class` | `HungerAndHavoc.dll` | Verse 按 XML 全名创建 ThinkNode |
| `HungerAndHavoc.Pawn.JobGiver_RHAH_Steal` | Duty XML `Class` | `HungerAndHavoc.dll` | Verse 按 XML 全名创建 ThinkNode |
| `HungerAndHavoc.Pawn.JobGiver_RHAH_Gnaw` | Duty XML `Class` | `HungerAndHavoc.dll` | Verse 按 XML 全名创建 ThinkNode |
| `HungerAndHavoc.Pawn.JobGiver_RHAH_Feed` | Duty XML `Class` | `HungerAndHavoc.dll` | Verse 按 XML 全名创建 ThinkNode |
| `HungerAndHavoc.Pawn.JobGiver_RHAH_Leave` | Duty XML `Class` | `HungerAndHavoc.dll` | Verse 按 XML 全名创建 ThinkNode |
| `HungerAndHavoc.Pawn.JobGiver_RHAH_WaitFood` | 访客调度直接调用 | `HungerAndHavoc.dll` | 与其它 JobGiver 一样必须 public，Verse 可按类型创建 |
| `HungerAndHavoc.Pawn.Compat.RHAH_PawnCompatStartup` | `[StaticConstructorOnStartup]` | `HungerAndHavoc.dll` | Verse 启动扫描公开静态构造入口 |
| `HungerAndHavoc.Pawn.Area_RHAH_Relief` | `AreaManager` 深存档 | `HungerAndHavoc.dll` | Verse 按 XML 全名创建 `Area` 并写入 `.rws` |
| `HungerAndHavoc.Pawn.Designator_AreaRHAH_Relief` | Zone `specialDesignatorClasses` 基类 | `HungerAndHavoc.dll` | 指定器基类必须可被 Verse 反射 |
| `HungerAndHavoc.Pawn.Designator_AreaRHAH_ReliefExpand` | Zone `specialDesignatorClasses` | `HungerAndHavoc.dll` | Verse 按 XML 全名创建指定器 |
| `HungerAndHavoc.Pawn.Designator_AreaRHAH_ReliefClear` | Zone `specialDesignatorClasses` | `HungerAndHavoc.dll` | Verse 按 XML 全名创建指定器 |
| `HungerAndHavoc.Pawn.JobGiver_RHAH_WaitFood` | 访客 ThinkTree 调度调用 | `HungerAndHavoc.dll` | 与其它 JobGiver 一样由 Verse 按公开类型创建 |
| `HungerAndHavoc.Pawn.RHAH_VisitorExpelMenu` | `FloatMenuMakerMap` 扫描 `FloatMenuOptionProvider` 子类 | `HungerAndHavoc.dll` | 原版只实例化公开子类。菜单只对在场来客提供驱逐，不进入 API |
| `HungerAndHavoc.Incidents.ChoiceLetter_RHAH_Request` | Letter `letterClass` | `HungerAndHavoc.dll` | Verse 按公开类型创建并存档选择信 |
| `HungerAndHavoc.Incidents.ChoiceLetter_RHAH_Visitors` | Letter `letterClass` | `HungerAndHavoc.dll` | Verse 按公开类型创建并存档选择信 |
| `HungerAndHavoc.Pawn.JobDriver_RHAH_DropChild` | JobDef `driverClass` | `HungerAndHavoc.dll` | Verse 按 XML 全名创建 JobDriver |
| `HungerAndHavoc.Pawn.JobDriver_RHAH_MotherFeed` | JobDef `driverClass` | `HungerAndHavoc.dll` | Verse 按 XML 全名创建 JobDriver |
| `HungerAndHavoc.Pawn.JobDriver_RHAH_Scavenge` | JobDef `driverClass` | `HungerAndHavoc.dll` | Verse 按 XML 全名创建 JobDriver |
| `HungerAndHavoc.Pawn.JobDriver_RHAH_TailBite` | JobDef `driverClass` | `HungerAndHavoc.dll` | Verse 按 XML 全名创建 JobDriver |
| `HungerAndHavoc.Pawn.JobGiver_RHAH_DropChild` | 访客调度直接调用 | `HungerAndHavoc.dll` | 与其它 JobGiver 一样必须 public |
| `HungerAndHavoc.Pawn.JobGiver_RHAH_MotherFeed` | 访客调度直接调用 | `HungerAndHavoc.dll` | 与其它 JobGiver 一样必须 public |
| `HungerAndHavoc.Pawn.JobGiver_RHAH_Scavenge` | 囚犯调度直接调用 | `HungerAndHavoc.dll` | 与其它 JobGiver 一样必须 public |
| `HungerAndHavoc.Pawn.JobGiver_RHAH_TailBite` | 囚犯调度直接调用 | `HungerAndHavoc.dll` | 与其它 JobGiver 一样必须 public |
| `HungerAndHavoc.Pawn.RHAH_BroadcastMenu` | `FloatMenuMakerMap` 扫描 `FloatMenuOptionProvider` 子类 | `HungerAndHavoc.dll` | 原版只实例化公开子类 |
| `HungerAndHavoc.Incidents.RHAH_RequestKind` | 选择信存档字段 | `HungerAndHavoc.dll` | Scribe 需要公开枚举，不属于 API |
| `HungerAndHavoc.Incidents.RHAH_IntelSiteKind` | 选择信存档字段 | `HungerAndHavoc.dll` | Scribe 需要公开枚举，不属于 API |
| `HungerAndHavoc.Incidents.RHAH_ChoiceKind` | 选择信存档字段 | `HungerAndHavoc.dll` | Scribe 需要公开枚举，不属于 API |
| `HungerAndHavoc.Incidents.RHAH_ChoiceAction` | 选择记录存档字段 | `HungerAndHavoc.dll` | Scribe 需要公开枚举，不属于 API |
| `HungerAndHavoc.Incidents.RHAH_ChoiceRecord` | `openChoices` 深存档 | `HungerAndHavoc.dll` | Scribe 按公开类型读写，不属于 API |
| `HungerAndHavoc.Incidents.RHAH_PredatorRecord` | `predators` 深存档 | `HungerAndHavoc.dll` | Scribe 按公开类型读写，不属于 API |
| `HungerAndHavoc.Pawn.Hediff_RHAH_ClaySatiety` | HediffDef `hediffClass` | `HungerAndHavoc.dll` | Verse 按 XML 全名跨程序集创建 Hediff |
| `HungerAndHavoc.Pawn.CompProperties_RHAH_Clay` | ThingDef XML `Class=` | `HungerAndHavoc.dll` | Verse 按 XML `Class` 反序列化 CompProperties |
| `HungerAndHavoc.Pawn.Comp_RHAH_Clay` | `CompProperties.compClass` | `HungerAndHavoc.dll` | Verse 按 `compClass` 创建 ThingComp；类型名写入 `.rws` |
| `HungerAndHavoc.Pawn.ThoughtWorker_RHAH_YoungInNeed` | ThoughtDef `workerClass` | `HungerAndHavoc.dll` | Verse 按 XML 全名创建 ThoughtWorker |
| `HungerAndHavoc.Pawn.ThoughtWorker_RHAH_NearbyDisease` | ThoughtDef `workerClass` | `HungerAndHavoc.dll` | Verse 按 XML 全名创建 ThoughtWorker |
| `HungerAndHavoc.Pawn.Compat.RHAH_IrisMenusCompat` 所在文件对 `IrisMenus` 的编译引用 | IrisMenus 1.6 公开 `MenuRegistry.RegisterSubItemListing` | `HungerAndHavoc.dll` 引用，`Private=False`，不随包发布 | 可选依赖。`ModLister` 未启用或 `modVersion` 不是 `1.6` 时不注册页面。类型保持 `internal`，不进入 API 程序集。`RHAH_IrisMenusWidgets.cs` 使用同一条编译排除 |
| `HungerAndHavoc.Pawn.Compat.RHAH_LeashBridge` 对 `LeadYourPet.LeadYourPetApi` 的运行时查找 | 门面留在 `LeadYourPet.dll`，没有独立 API DLL | `HungerAndHavoc.dll`，类型 `internal` | 可选依赖。不编译引用对方程序集。只在 `nanaloveyuki.leadyourpet.continued` 已启用时按公开方法名调用。类型或签名缺失记一条日志后跳过，不反射 `LeadYourPetGameComponent` |

原版 Harmony 例外不进上表。`RHAH_IncidentSchedulePatch` 是 `internal`，Postfix `Storyteller.StorytellerTick`。原版讲述者没有本模组事件池，`baseChance` 保持 0。补丁只在 1000 tick 检查点入队，不改类别权重，不替换袭击。

`RHAH_AttitudeHarmPatch` 是 `internal`，Postfix `Thing.PreApplyDamage`。原版伤害只改单只 pawn 的好感，不会按生成批次改态度。补丁只接收玩家派系实施者的外部暴力，调用批次离场或敌对关系，不创建袭击 Lord。目标缺失时不注册。

`RHAH_ClayEatThingPatch` 与 `RHAH_ClayEatDefPatch` 是 `internal`，Postfix `FoodUtility.WillEat` 的 Thing 和 ThingDef 重载。观音土十五天吃满三块后原版仍把它当食物。补丁只在目标是 `RHAH_GuanyinTu` 且饱腹窗口未过时返回 false，不改其它食物。
`RHAH_TraitColor` 是 `internal`，Postfix `Trait.LabelCap`。原版特质名没有本模组颜色。补丁只给 `RHAH_Trait_` 前缀上色，已有颜色标签时不改。
`RHAH_CaravanLeavePatch` 是 `internal`，Prefix `Transition.CheckSignal`。原版交易 Lord 会因危险温度、异常天气、危险状况或到不了地图边缘离图。补丁只拦本模组商队：环境离图看 `traderIgnoresHarshEnvironment`，封闭空间看 `traderIgnoresEnclosedSpace`。其它 Lord 不改。
`RHAH_CaravanFoodSalePatch` 是 `internal`，Postfix `TraderKindDef.WillTrade`。原版商人可以卖食物。补丁只在交易对象是本模组商队且物品提供营养时返回 false。玩家用食物换孩子不走这条。
`RHAH_ThreatTempoPatch` 是 `internal`，Prefix `StorytellerComp_RandomMain.ChooseRandomCategory`。原版权重不看本模组信任。补丁只在当前叙事者是 `RHAH_Suiyin`、目标是玩家家园、信任不是 0 时，把大型威胁权重乘以 `1 - clamp(trust, -100, 100) / 400`，并把 13 天补发阈值除以同一系数。其它叙事者、任务袭击、商队和本模组事件池不改。目标或属性缺失时走原版。
`RHAH_PredationFoodPatch` 是 `internal`，Prefix `JobGiver_GetFood.TryGiveJob`。原版觅食不认识安居点的强制目标。补丁只改 `MapComponent_RHAH_Map` 正在跟踪、且地图父对象是 `WorldObject_RHAH_RefugeeCamp` 的野生捕食者。其它地图走原版。
`RHAH_PredationFleePatch` 是 `internal`，Prefix `JobGiver_ReactToCloseMeleeThreat.TryGiveJob`。关闭反击后，被本特殊情况追猎的安居点鼠族改为逃跑。默认反击，不能使用暴力的人不变。其它威胁走原版。
`RHAH_CaptureGatePatch` 是 `internal`，Prefix `Pawn_GuestTracker.CapturedBy`。原版俘虏不看来源闸门。补丁只在捕获方是玩家且目标是本模组来源时问 `Imprison`，拒绝则不捕获，通过则 `ReleaseToColony(Imprisoned)`。其它俘虏走原版。
`RHAH_TradePawnGatePatch` 是 `internal`，Prefix `Tradeable_Pawn.ResolveTrade`。原版角色买卖不看来源闸门。补丁只检查这笔要成交的本模组来源，任一 `Transfer` 被拒则整笔不成交，通过则 `ReleaseToColony(ModRequest)`。物品交易和其它角色走原版。

## 检查门禁

检查脚本和测试必须与本页保持一致，至少覆盖：

- API 程序集依赖方向与公开类型白名单
- 路径、命名空间和程序集职责一致
- 非 API public 类型的例外登记
- 存档键格式、默认值、迁移登记和集合初始化规则，并与 [save-ownership.md](save-ownership.md) 一致
- Def、Keyed、事件目录、XML workerClass、hediffClass 和 CompProperties 类型名
- 中英 Keyed 键集合对称、`Translate` 引用存在、英文 DefInjected 覆盖 Def 正文
- 可持久化 Def 与类型已登记卸载动作（Remove / Replace）
- 禁止旧前缀、旧产品名和含义不清的 Egg 身份名
- `IRHAH_Pawn`、`RegisterRatkinMatcher` 和稳定 API 事件签名
- 测试程序集只能通过 `InternalsVisibleTo` 访问内部实现，且不得成为运行时依赖
- 修 bug 时的范围、Language、存档和卸载门禁见 [bug-handling.md](bug-handling.md)

这些检查属于 CI 阻断级门禁。`scripts/verify-scaffold.py` 检查中英 Keyed 对称、源码里的 `Translate` 字面量、中文 Def 正文的英文 DefInjected、`Scribe_*.Look` 键是否出现在卸载归属表、Hediff XML 类型名和旧前缀。动态拼接的翻译键不在字面量扫描里，事件标签另按目录 defName 检查。
