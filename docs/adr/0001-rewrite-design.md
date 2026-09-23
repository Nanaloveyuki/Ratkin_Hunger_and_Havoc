---
status: implemented
date: 2026-09-20
---

# Ratkin: Hunger and Havoc 重写设计

在空仓库 `Ratkin_Hunger_and_Havoc` 中，把 `/root/repos/Ratkin-Great-Famine-Year-Continued` 作为**玩法规格**重写为独立新模组，而不是 Continued 移植。

旧仓库的问题不带到新仓库：`MouseDisaster` 全局前缀、`N-001` 同时占用剧情和事件、用童年/成年经历判定“是不是灾鼠”、二十多个 `GameComponent`、巨型 `MouseDisasterUtility` 分部类、旧式 csproj 手写 Compile 列表。

**默认假设：**

- 新 `packageId`，**不读旧档**，不迁移 `MouseDisaster_*` 存档字段。
- 玩法目标是 Continued 的功能面（事件、访客 AI、赈灾、鼠疫、穗音叙事），代码与 Def **重新实现**。
- 旧仓库只当行为对照，不拷贝进本仓库。
- 中文显示名 **鼠族: 饥与祸**；英文 **Ratkin: Hunger and Havoc**。

决策记录见 `docs/adr/`。本计划同步为 `docs/adr/0001-rewrite-design.md`。

## 1. 身份与包名

| 用途 | 取值 |
| --- | --- |
| Steam / About 名 | `鼠族: 饥与祸` / `Ratkin: Hunger and Havoc` |
| `packageId` | `nanaloveyuki.ratkin.hungerandhavoc` |
| 作者 | `Nanaloveyuki` |
| C# 命名空间 | `HungerAndHavoc` |
| 程序集 | `HungerAndHavoc.dll` |
| XML `workerClass` | `HungerAndHavoc.Incidents.IncidentWorker_*` |
| Def / 本地化前缀 | `RHAH_` |
| Guard 程序集 | `HungerAndHavocGuard.dll` |
| 设置分类键 | `RHAH_ModName` |
| Harmony Id | `nanaloveyuki.ratkin.hungerandhavoc` |

`packageId` 用作者 + 系列 + 产品；C# 命名空间保持短名，避免 XML 里写出 `Nanaloveyuki.Ratkin.HungerAndHavoc...`。前缀用 `RHAH_`（Ratkin Hunger And Havoc），比 `RHH_` 更不易和其它缩写撞车。

**冲突策略：**

- `incompatibleWith`：`lezhizhong.mouse.disaster.famine`、`nanaloveyuki.mouse.disaster.famine.continued`、`local.mousedisaster.greatfamine`
- Guard 始终加载；检测到上列任一启用则提示，且 `LoadFolders.xml` 不加载主体 `1.6/`。**不自动停用旧模组**（本模组不是替换包，旧档仍依赖旧包）。
- 本模组**不是** Continued，不声明对旧包的替换兼容

**依赖与加载序：** Harmony、`Solaris.RatkinRaceMod`、Biotech；`loadAfter` 再加可选 `Nanaloveyuki.IrisMenus`、`cyanobot.toddlers`、`nanaloveyuki.leadyourpet.continued`。

工程风格对齐 `Lead-Your-Pets`：SDK-style `net472` csproj、`RimWorldDir`、`scripts/build-and-deploy.ps1`、独立 Tests 项目。

## 2. 仓库布局

游戏内容按版本目录拆开，1.7 只加平行文件夹，不把 Def/程序集从根目录再迁一次。

```
About/About.xml
LoadFolders.xml
LICENSE
README.md
NOTICE
Languages/{ChineseSimplified,English}/Keyed/   # 跨版本共用
Textures/
1.6/
  Assemblies/HungerAndHavoc.dll
  Defs/                   # 文件名 RHAH_*.xml
  Patches/
Guard/                    # 始终加载
Source/HungerAndHavoc.csproj   # 输出到 1.6/Assemblies/
  Core/
  Identity/
  Api/                    # 给其它模组的稳定入口
  Generation/
  Incidents/
  Behavior/
  Narrative/
  World/
  Patches/
  UI/
  Tests/
scripts/
docs/
  adr/                    # 决策记录（含本计划）
  naming.md
  incidents.md
  pawn.md
tmp/                      # 已 gitignore
```

`LoadFolders.xml`：

```xml
<v1.6>
  <li>Guard</li>
  <li IfModNotActive="lezhizhong.mouse.disaster.famine,nanaloveyuki.mouse.disaster.famine.continued,local.mousedisaster.greatfamine">/</li>
  <li IfModNotActive="...same...">1.6</li>
</v1.6>
```

1.7 到来时增加 `<v1.7>`：`Guard` + `/` + `1.7`，源码再按 API 差异决定是否拆 `Source/1.7`。

## 3. 事件命名

旧目录把 **O-001~O-014** 当原作事件、**N-011~N-047** 当续作事件，同时把 **N-001~N-010** 留给剧情。设置页、调试菜单、经历匹配、态度覆盖全部挤在同一套 `N-` 上。

新目录拆成互不重叠的 **显示 ID**。`defName` 可读，不把编号写进 Def 名。

| 种类 | 显示 ID | defName 示例 | 用途 |
| --- | --- | --- | --- |
| 事件 | `I-001` | `RHAH_LargeRefugeeWave` | 随机/调试可触发的 IncidentDef |
| 剧情节拍 | `N-001` | （信件/状态，通常无 IncidentDef） | 穗音章节 |
| 结局 | `E-01` | 设置与结局画面 | 年末/信任结局 |
| 记录 | `J-001` | 记录簿条目 | 旧 S-01~S-14 |
| 揭示 | `R-01` | 身份询问 | 旧 R-01 |
| 经历 | `H-A001` / `H-Y001` | `RHAH_History_A001` | 童年/成年 Backstory |
| 特质 | `T-001` | `RHAH_Trait_HardyLabor` | 自有特质 |

规则：

1. 显示 ID 是目录数据，不是列表下标；增删事件不改已有编号。
2. `I-` 从 `I-001` 起连续登记；原作 14 个占 `I-001`~`I-014`，续作从 `I-015` 起（危险的鼠族安居点进目录，不再插在 `N-047`）。
3. 剧情只使用 `N-` / `E-` / `J-` / `R-`，事件绝不再用 `N-`。
4. 回访分支用后缀：`N-004-R`、`N-007-R`。
5. 本地化键：`RHAH_Incident_LargeRefugeeWave_Label`，调试菜单显示 `I-001 大型流民冲击`。
6. 设置里的“原作/续作”是目录字段 `Origin: Original | Sequel`，不再靠 O/N 前缀推断。

### 3.1 事件目录（第一批对照）

`RHAH_IncidentCatalog` 每条记录：

- `DisplayId`（I-001）
- `DefName`（RHAH_LargeRefugeeWave）
- `Family`：Beggar / Thief / Wild / Aid / Intel / Plague / Siege / Trade / Special
- `Origin`：Original | Sequel
- `Category`：Hunger | Plague
- `Target`：Map | Caravan
- `BroadcastEligible`
- `DefaultAttitudePool`：Positive | Negative
- `DebugPoints`

Worker 只实现一次行为；鼠疫变体是同一 Worker + `infectsWithPlague` 请求，不再复制 9 套几乎相同的类。

旧 → 新（事件部分）：

| 旧 | 新 | 名称 |
| --- | --- | --- |
| O-001 | I-001 | 大型流民冲击 |
| O-002 | I-002 | 鼠蛋遗弃 |
| O-003 | I-003 | 耗子分妈 |
| O-004 | I-004 | 乞讨的灾荒鼠妈 |
| O-005 | I-005 | 乞讨的鼠族队伍 |
| O-006 | I-006 | 偷窃的鼠族 |
| O-007 | I-007 | 偷窃的鼠蛋 |
| O-008 | I-008 | 游荡的野生鼠族 |
| O-009 | I-009 | 游荡的野生鼠蛋 |
| O-010 | I-010 | 游荡的野生鼠群 |
| O-011 | I-011 | 灾荒逃难者 |
| O-012 | I-012 | 流民商队 |
| O-013 | I-013 | 易子而食 |
| O-014 | I-014 | 鼠灾围攻 |
| N-011~N-015 | I-015~I-019 | 接济五件 |
| N-016~N-024 | I-020~I-028 | 情报九件 |
| N-025~N-031 | I-029~I-035 | 待产/强势围攻/路过/空投/认亲/大灾荒/商队劫掠 |
| N-032~N-046 | I-036~I-050 | 对应鼠疫变体 |
| N-047 | I-051 | 危险的鼠族安居点 |

剧情编号保持 `N-001`~`N-010`、`E-01`~`E-05`、`R-01`，记录改为 `J-001`~`J-014`，避免再和事件抢 `S-`/`N-`。

## 4. Pawn 逻辑

旧判定把**生成来源**、**访客 AI**、**叙事身份**绑在 Backstory 上。经历一改就丢身份；招募后仍被当成事件鼠；角色则靠二十多个仅作标签的 Hediff。

### 4.1 身份分层

| 层 | 载体 | 含义 |
| --- | --- | --- |
| 种族 | 鼠族 `ThingDef` | 是否鼠族（不靠名字模糊匹配作为唯一依据） |
| 来源 | `RHAH_HungerMark` + `CompRHAH_Pawn` | 本模组（或经 API 标记的其它模组）生成过，随 pawn 存档 |
| 访客 | `comp.IsActiveVisitor` | 仍走事件 AI / Lord / 五个固定态度派系 |
| 角色 | `comp.Role` | Beggar、Thief、Mother、RatkinYoung、Trader、Wild、Siege、Plague、Labor、Envoy… |
| 经历 | BackstoryDef | 只是文本和技能，**不参与身份判定** |
| 状态 | 少量 Hediff | 鼠疫、已饱食、再喂养、啃树皮、雇佣计时等**有效果**的状态 |

不用种族 `ThingDef` 补丁挂 ThingComp：NewRatkinPlus / HAR 会改种族 Def，额外 comps 容易丢或冲突。身份 Hediff 跟着 pawn 走，跨种族子类型、交易、俘虏都还在。

判定入口走公开 API（其它模组不要自己翻 Hediff）：

- `RHAH_Api.IsOrigin(pawn)`
- `RHAH_Api.IsVisitor(pawn)`
- `RHAH_Api.IsRatkin(pawn)`
- `RHAH_Api.IsRatkinYoung(pawn)`
- 基因/特质/叙事统计用 origin；JobGiver/离场/乞食只用 visitor

招募、囚禁、奴隶、玩家派系加入时调用 `ReleaseToColony()`：停访客 AI，**保留标记** 供穗音信件和统计。

### 4.2 `CompRHAH_Pawn` 存档字段

```
sourceIncidentDisplayId // "I-005"
spawnBatchId           // same generation batch
relationshipGroupId    // family/relationship group within an event
role
lifecycle           // Arriving | SeekingFood | Fed | Leaving | Released | Dead
hasBeenFed
leaveAfterGameTick
carriesPlague
attitudeAtArrival
attitude              // 当前态度 到达后可按批次改
behaviorFlags       // 本模组内建开关
gateOverrides       // per-pawn RHAH_BehaviorGate overrides
extraData           // Dictionary<string,string>，其它模组私有状态
parentPawnLoadId / childPawnLoadIds
```

健康面板身份句来自同一条 `RHAH_HungerMark` 的动态 label，不再为每个角色单独做一个空 Hediff。

### 4.3 其它模组可改的行为（预留）

默认行为全部经过闸门，不在 JobGiver 里写死。其它模组有三层、由近到远：

1. **单 pawn**：`RHAH_Api.SetGate(pawn, RHAH_BehaviorGate.Leash, true/false/null)` 写入 `gateOverrides`。牵绳、吞食、离场、加入等都能按只覆盖。检疫中的 `JoinColony`、`Hire`、`Transfer` 由疾病层强制拒绝，单 pawn 覆盖不能放开。
2. **全局策略**：`RHAH_PawnBehaviors.Register(IRHAH_PawnBehavior)`。后注册优先；返回 `null` 表示不管。Lead Your Pet、Toddlers、囚犯模组在 `StaticConstructorOnStartup` 里注册即可，不必 Harmony 我们的私有方法。
3. **事件**：`OriginMarked` / `ReleasedToColony` / `LifecycleChanged` / `GateQueried`。只观察也可以。

闸门枚举一开始就留齐，即使本期未实现对应 AI：

`Beg, Steal, Fight, LeaveAfterFed, EatOutsideRelief, FeedFromRelief, Gnaw, TailBite, Leash, Carry, JoinColony, Imprison, DropOffChild, ExitMap`

`TryMarkOrigin(pawn, seed)` 是给其它模组的生成入口：它们生成的鼠族若应计入饥与祸来源（牵引、交易带来的幼年鼠族等），调用后即有 Comp，而不要求走我们的 `PawnKindDef`。

公开类型放在 `HungerAndHavoc.Api`，不把 `internal` 实现细节暴露成契约。Harmony Id 稳定为 packageId。

详见 `docs/adr/0002-pawn-mod-compatibility.md`。

### 4.4 生成管线

`RHAH_PawnFactory.Create(RHAH_PawnRequest)`，一步一责：

1. 选 `PawnKindDef` + 年龄/性别/异种
2. `PawnGenerator.GeneratePawn`
3. `TryMarkOrigin`（身份从此成立）
4. 抽 `H-*` 经历（失败则保底 `RHAH_Newborn` / `RHAH_Refugee`）
5. 最多 1 条自有 `T-*` 特质
6. 重配平民衣装（官方 + `RHAH_GenerationExtension.allowRefugeeApparel`）
7. 饥饿、伤病、鼠疫
8. 分帧生成队列

### 4.5 运行时行为（Visitor only）

寻食、乞食、偷窃、饱食离场、态度五档、托孤、分帧生成等仍按玩法规格做，但全部问闸门。

`GameComponent` 收敛为两个：

- `GameComponent_RHAH_Game`（partial：调度、生成队列、叙事、卸载清理）
- `MapComponent_RHAH_Map`（寻食缓存、捕食、本图访客索引）

## 5. 实现顺序

1. **脚手架**：About、LoadFolders、`1.6/`、SDK csproj、Guard、adr、Catalog
2. **身份 + API**：`CompRHAH_Pawn`、闸门、`TryMarkOrigin`、一条最小事件 `I-005`
3. **访客 AI**（全部走闸门）
4. **原作 14 事件** `I-001`~`I-014`
5. **调度与设置**
6. **续作事件**（鼠疫变体复用 Worker）
7. **叙事**
8. **联动**：Lead Your Pet / Toddlers 走公开 API，不反向私有补丁

## 6. 明确不做

- 不保留 `MouseDisaster` 命名空间、Def 名、存档 tag
- 不把旧 `1.6/Source` 拷进本仓库
- 不在 v1 做旧档迁移器
- 不把经历/空 Hediff 当身份
- 不把剧情 ID 和事件 ID 放进同一前缀
- 不给种族 ThingDef 打身份 Comp 补丁
- 不自动禁用旧的鼠灾模组
