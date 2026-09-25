# Pawn 身份

来源标记是 Hediff `RHAH_HungerMark`，存档数据在 Identity 的 `CompRHAH_Pawn`。其它模组只引用独立的 `HungerAndHavoc.Api.dll`，使用 `HungerAndHavoc.Api` 中的 `RHAH_Api`、`IRHAH_Pawn`、`RHAH_PawnSnapshot`、闸门与 Seed。不要扫描 Hediff、Backstory 或 Comp。`CompRHAH_Pawn` 不实现 `IRHAH_Pawn`。

`RHAH_Api` 查询与标记返回 `IRHAH_Pawn`（`RHAH_PawnSnapshot`，sealed 不可变副本）。只读成员：`SourceIncidentDisplayId`、`SpawnBatchId`、`RelationshipGroupId`、`Role`、`Lifecycle`、`HasBeenFed`、`LeaveAfterGameTick`、`CarriesPlague`、`AttitudeAtArrival`、`Attitude`、`ParentPawnLoadId`、`ChildPawnLoadIds`（`IReadOnlyList<int>`）、`IsReleased`、`IsActiveVisitor`。`AttitudeAtArrival` 不随批次反应改写；`Attitude` 是当前态度，战斗和区外进食读它。`IsReleased` 为 `Lifecycle == Released`；`IsActiveVisitor` 为未 Released 且未 Dead。快照是拷贝，后续 Comp 变化不写回已发出的快照。改状态走 `SetLifecycle` / `SetGate` / `SetExtra` / `ReleaseToColony` / `TryMarkOrigin`。事件与行为参数使用 `IRHAH_Pawn`，不得传 Comp。

## 生命周期

`Arriving → SeekingFood → Fed → Leaving → Released | Dead`

`ReleaseToColony` 把访客变成殖民地相关 pawn，标记仍在。

角色 `RatkinYoung` 表示幼年鼠族。代码和 API 不使用含义不清的 `Egg` 或 `RatkinEgg`。

存档字段（Comp，新键使用 camelCase；正式发布后存档键冻结）：

- `spawnBatchId`：同一批次生成的 pawn。
- `relationshipGroupId`：同一事件中的家庭或关系组。
- `carriesPlague`：到达时是否携带鼠疫。康复后仍为 true，当前是否患病看 `RHAH_Plague`。
- `attitudeAtArrival`：到达时态度快照，不随批次反应改写。
- `attitude`：当前态度。缺省或旧档为 Neutral 时，读档后回退到 `attitudeAtArrival`。闸门读这个值。
- `parentPawnLoadId` / `childPawnLoadIds`：Pawn 存档 Load ID 关系。
- `droppedChildLoadIds`：母亲离场时已经放下的孩子 Load ID。开关关闭后不再新增。
- `gateOverrides`：本 Pawn 的行为闸门覆盖。
- `extraData`：其它模组使用的私有键值数据。

## 闸门

`RHAH_BehaviorGate`：Beg, Steal, Fight, LeaveAfterFed, EatOutsideRelief, FeedFromRelief, Gnaw, TailBite, Leash, Carry, JoinColony, Hire, Transfer, Imprison, DropOffChild, ExitMap。

`Hire` 与 `Transfer` 默认允许。检疫名单上的 pawn，`JoinColony`、`Hire`、`Transfer` 为 false，覆盖和行为策略不能放开这三项。

判定顺序：该 pawn 的 `gateOverrides` → `IRHAH_PawnBehavior`（后注册优先）→ `RHAH_PawnDefaults`。无来源标记时 `Allows` 为 false。检疫拒绝发生在这三层之后。

玩法必须问对应闸门，不能只看设置或角色：

- `Fight`：批次转敌对后，本模组安排的反击。默认只放行 `Siege` 或当前态度 `Hostile`。关闸后仍会离场，但不反击
- `DropOffChild`：母亲离场放下孩子。设置 `familyDropEnabled` 仍可整项关闭
- `TailBite`：囚犯咬幼年尾巴。默认关闭，设置或单只覆盖打开后才执行
- `Imprison`：原版俘虏进玩家囚犯名单时调用 `ReleaseToColony(Imprisoned)`。关闸后不捕获
- `Transfer`：原版交易把来客卖出或买进玩家派系。关闸后这笔角色交易不成交。检疫同样拒绝
- `Leash`：只公开查询和覆盖。牵引适配以后再接，当前没有玩法消费它

`IRHAH_PawnBehavior`：

```csharp
bool? Allows(Pawn pawn, IRHAH_Pawn snapshot, RHAH_BehaviorGate gate);
bool? ShouldReleaseToColony(Pawn pawn, IRHAH_Pawn snapshot, RHAH_ReleaseReason reason);
```

单只覆盖：

```csharp
RHAH_Api.SetGate(pawn, RHAH_BehaviorGate.Leash, true);
```

全局：

```csharp
RHAH_PawnBehaviors.Register(new MyPolicy());
```

私有数据用 `SetExtra(pawn, "your.package.id:key", value)`。

## 运行时层

访客 AI 在 `Source/Pawn`，命名空间 `HungerAndHavoc.Pawn`。Identity 只管标记和闸门数据，不发 Job。

有 Lord 的访客走自有 `LordJob_RHAH_Visitor` + `DutyDef`。图只有赶路、寻食和离场。寻食 duty 在没有进食、乞讨、偷窃、啃咬或等待 Job 时，在等待点附近游荡，不走向地图出口。`ExitMap` 只在生命周期已经是 `Leaving` 时放行；吃饱离开仍问 `LeaveAfterFed`。`fedWanderEnabled` 开启时先闲逛 `fedWanderHours` 小时再走，默认 12，范围 1 到 48。关闭时离开时刻就是吃饱这一刻，下一轮离场直接走向出口。空派系不切原版防守或袭击。批次伤害和驱逐发 `RHAH_Leave`。无 Lord 回退用独立 `ThinkTreeDef`，`insertTag=Humanlike_PostDuty`，条件是 `RHAH_Api.IsVisitor`，不 xpath 改 `Humanlike.xml`，不按 `PawnKind` 分支。

不能自己走到出口的幼年访客由同 Lord 里允许 `Carry` 且能自己走到出口的成年照护者带出。`Carry` 默认只放行非幼年角色；幼年角色可以 `Leash`，但不能发出携带 Job。断粮等待到达 `foodWaitUntilTick` 后，仍未进食的活跃访客离场，不再停在寻食游荡。

招募、短工和长工仍保留来源标记，但停留期间不发乞讨、偷窃、啃咬、赈灾取食、等待和本模组离场。期限结束且不再倒地后，拒绝工作、休息、娱乐和任何非玩家强制任务，只保留被动近战反击、逃跑和进食。倒地期间计时暂停，这些限制先不生效。

乞讨不选睡着、躺在医疗床上、倒地、禁止接触，或还不能自己行动的殖民者。无幼童模组时年龄不足 4 岁，有幼童模组时不足 1 岁 47 天。当前目标不合适就换下一个。全部失败后闲逛，`begFailCooldownHours` 小时内不再乞讨，默认 3，范围 3 到 12。成功要同时掷中几率并且 `begAutoGiveEnabled` 开启。默认关闭。开启后从对方背包拿走一份 `disabledBegFoodDefNames` 允许的正餐，营养高的优先，放进乞讨者背包，放不下就丢在脚下。关掉、背包没有允许的食物，或同一人已经被乞讨过，都不算成功，继续换人。基础成功率 `begSuccessChancePercent` 默认 35，范围 0 到 100，每级社交再加 `begSocialBonusPercent`，默认 3，范围 0 到 20，合计不超过 100。成功给乞讨者 `RHAH_Thought_BeggingSucceeded`，心情 +3，不叠加。失败给乞讨者 `RHAH_Thought_BeggingRejected`，心情 -5，可无限叠加。同一殖民者第二次及以后被乞讨时，按 `begSlapChancePercent` 抽一巴掌，默认 50。抽中则昏迷 3 小时，头部没有瘀伤时加轻度瘀伤，已有则加重，瘀伤已到上限则头部中度流血，并给乞讨者 `RHAH_Thought_BeggingSlapped`，心情 -10，可无限叠加。重复乞讨不再算成功。JobGiver 第一行：非访客返回 null；再问 `RHAH_Api.Allows`。角色规则只在 `RHAH_PawnDefaults` 和闸门覆盖里。吃饱后不再乞讨、偷窃、啃咬或由本模组安排进食。啃树皮和墙只从原版饥饿觅食补上：食物比例低于 5%、当前没有 Job，并且 40 格内有可预订、可走到的树、植物或实心墙。寻食 duty 不主动发啃食。啃完只加营养和伤口，不把生命周期改成 `Fed`，也不因此离场。

`ReleaseToColony` 必须拆 Lord、清 duty、停访客 JobGiver。标记 Hediff 保留。

其它模组适配只进 `Source/Pawn/Compat/`。基底只暴露闸门、`IRHAH_PawnBehavior` 和 `RHAH_Api` 事件。禁止 Harmony 其它模组私有类型。原版缺口补丁登记在 [engineering.md](engineering.md)。

## 生成

`RHAH_PawnRequest.Profile` 是事件自己的生成配置，不进存档。四个开关各自独立，默认关闭，关闭时不改对应内容：

- `UseExplicitApparel` 与 `Apparel`：先清掉生成器给出的衣服，再穿列表中的装备。空列表表示不穿
- `UseExplicitBackstory` 与 `Childhood` / `Adulthood`：直接替换背景。两者都空时清掉背景。未满 20 岁不写成年背景
- `UseExplicitHealth` 与 `Hediffs`：只追加列表中的 Hediff，不删除生成器已有状态。空列表表示不追加
- `UseExplicitXenotype` 与 `Xenotype` / `XenotypeDefName`：指定异种。两者都空时不指定基因
生成请求未指定 `PawnKind` 时使用 `RHAH_PawnKind_Ratkin`，种族是 NewRatkinPlus 的 `Ratkin`。事件、难民营和交易都传这个 PawnKind。生成后读取该种族的 HAR 设置：头型不在 `headTypes` 里就重抽，发型不含 `styleTagsOverride` 的标签就重抽，`BeardDef` 或 `TattooDef` 的 `hasStyle` 为 false 时清成无胡须、无纹身。`apparelMode` 为 0 或 1 时清掉生成器的衣服，再按年龄重配。成年只穿 NewRatkinPlus 或原版及官方 DLC 的中世纪及以下、可制布或人皮、算衣物的衣服，以及带 `RHAH_GenerationExtension.allowRefugeeApparel` 的外部衣服。婴幼只穿连体衣、暖帽和遮阳帽，儿童只穿暖帽、遮阳帽和儿童部落服。`disabledRefugeeApparelDefNames` 里的衣服不进这套重配，默认空，之后新加入的衣服仍可生成。件数、品质、耐久和死者标记按旧鼠灾权重：品质只有极差、差、一般，耐久 10% 到 60%，25% 标成死者衣物。材料优先布，其次人皮。为 2 时同样重配。温度衣在普通衣服之前穿上，避免被挤掉。为 3 时清空。`UseExplicitApparel` 不走这套重配，仍按事件列表穿衣。调用方传入非鼠族 `PawnKind` 时不改头、发、胡须和纹身，衣服仍按上面的模式重配。`coldClothesEnabled` 打开且没有清空衣服、也没有显式衣服时，地图气温低于舒适下限或高于舒适上限就补一件温度衣。抗寒六档默认 8、12、20、28、40、56，抗热六档默认 8、12、20、28、36、44。选刚好盖住缺口的最低一档，单件关闭就跳过，都不够用最厚的一件。已经穿过温度衣不再换。滑条范围 0 到 100，改的是这件衣服实际隔热。名单按 `apparelListMode` 折叠：0 按来源模组，1 按名称首字母，2 按衣物在 `Apparel` 下的原版类别。
未设置显式背景时，生成后从 `Source/Data` 的经历表抽一条。自有特质在其后抽取，数量看 `maxOwnedTraits`，默认 1，最多 3。身份仍只看 `RHAH_HungerMark`。经历槽位用发育阶段，年龄上下限仍按每条记录。`pawnHistoriesEnabled` 或 `pawnTraitsEnabled` 关闭、单条被禁用、特质权重为 0、上限为 0 时跳过对应抽取。显式背景不改经历，特质仍抽。成年经历同时写入保底童年 `RHAH_History_Newborn`。幼年关联只影响抽取权重，不预写成年背景。`traitAgeFilter` 关闭后，幼年特质和成年特质可以互相抽到。`allowVanillaTraits` 关闭后，年龄特质上下限都是 0。
普通来客年龄用 `minGeneratedAge` 和 `maxGeneratedAge`。幼年角色未指定年龄时不走这组区间：`BeggarChild` 在 1 天到 2.9 岁，`RatkinYoung`、`ThiefChild` 和 `WildChild` 在 3 到 6.9 岁。`youngAgeFollowsRange` 不把这些角色抬成成年。母亲未指定年龄时也用普通区间，已指定的不改。无幼童模组且 `allowImmobileBabies` 关闭时，幼年角色不足 4 岁会抬到 4 岁。幼童模组启用或开关打开时不抬。`I-002` 与 `I-037` 整批是 `BeggarChild`。`I-003` 第一名是雌性 `Mother`，缺左耳、右腿和鼠尾，已在地图上时倒地，失血至少 0.7，其余是 `RatkinYoung`。`I-004` 第一名是雌性 `BeggarMother`，其余是 `BeggarChild`。`I-007` 与 `I-043` 是 `ThiefChild`。`I-009` 与 `I-041` 是 `WildChild`。`I-013`、`I-032`、`I-033`、`I-046` 和 `I-047` 是 `RatkinYoung`。`I-029` 与 `I-044` 是雌性 `BeggarMother`，生成后进入产程。其它事件保持各自角色。`genderMode` 只填未指定性别的请求。经历和特质菜单按来源、名称首字母或类别折叠。页内搜索和 IrisMenus 搜索都能定位到条目。
选择信里的投喂不改这份赈灾名单。右键交给来客时另看 `disabledGiveFoodDefNames`，默认空，表示当前能吃的食物都可以交。关掉的食物不出现在选项里。之后新加入的食物默认可交。名单按 `giveFoodListMode` 折叠：0 按来源模组，1 按名称首字母，2 按食物在 `Foods` 下的原版类别。没有类别时归入其他。
短工用 `shelterDays`，默认 5 天，范围 5 到 240。长工用 `hireDays`，默认 240 天，范围 5 到 2400。一年按 60 天。临时征募走短工期限。这三种停留在到期且不再倒地之前保持玩家派系，名字维持殖民者白名。外部把派系改走后，下一次停留检查拉回玩家派系。囚犯和奴隶不拉回。到期把派系清掉并改为离场，倒地不消耗剩余时间。

`optimizeGeneration` 默认开启，登记在 IrisMenus 实验页和原版设置窗口。开启时跳过关系、头衔、随机装备、成瘾、食物和世界角色重装。事件没有显式基因时，按基因页权重抽取已启用异种。权重合计为 0、生物科技未开或 Def 丢失时，依次尝试 `RK_XenoType_Ratkin` 和 `RHAH_Xenotype_Ratkin`。两者都不存在时保持原版默认。关闭优化不取消事件 Profile，也不取消经历和特质抽取。`RHAH_` 基因开关只在异种套上后追加，冲突则跳过。基因页的已启用异种和可加入异种按来源模组的显示名分组，组内保持原顺序。没有 `modContentPack` 或名称为空时归入未知来源。
繁殖基因不进随机基因包。开关打开后，多崽按 `litterMin`、`litterPeak`、`litterMax` 抽分娩数量，默认 2、4、6，范围 1 到 12。概率从下限升到峰，再降到上限。峰的相对几率是 1，两端按到峰的距离下降。早熟用 `fertileMinAge`，默认 1，范围 1 到 14。双方生理年龄达到后不再被原版生育阶段拦住。高育用 `fertilityPercent`，默认 300，范围 100 到 1000，乘在双方怀孕几率上。乱起让携带者主动接爱爱工作，爱爱结束后按原版 5% 再乘双方怀孕几率判定，哺乳期不另拦，双方仍要满 1 岁。速产用 `gestationDays`，默认 5.661，最短 3 天，最长不超过原版下限 5.661，也不超过该种族自己的孕期。
地图事件默认分帧：游戏走动时，排队事件每 64 tick 执行一条。`staggerGeneration` 关闭后连续执行。开发者触发在菜单强制暂停期间不走 tick，暂停窗口关闭后的下一帧立即生成，不受这 64 tick 限制。广播只从 `BroadcastEligible` 且未被单独关闭的事件里抽。

商队伏击先生成未入场的 pawn，再交给原版商队地图。
商队来客在恶劣环境或封闭房间里不走寻食离场。两项分别看 `traderIgnoresHarshEnvironment` 和 `traderIgnoresEnclosedSpace`，默认都开启。Lead Your Pet 不改这条。
