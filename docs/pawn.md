# Pawn 身份

来源标记是 Hediff `RHAH_HungerMark`，存档数据在 Identity 的 `CompRHAH_Pawn`。其它模组只引用独立的 `HungerAndHavoc.Api.dll`，使用 `HungerAndHavoc.Api` 中的 `RHAH_Api`、`IRHAH_Pawn`、`RHAH_PawnSnapshot`、闸门与 Seed。不要扫描 Hediff、Backstory 或 Comp。`CompRHAH_Pawn` 不实现 `IRHAH_Pawn`。

`RHAH_Api` 查询与标记返回 `IRHAH_Pawn`（`RHAH_PawnSnapshot`，sealed 不可变副本）。只读成员：`SourceIncidentDisplayId`、`SpawnBatchId`、`RelationshipGroupId`、`Role`、`Lifecycle`、`HasBeenFed`、`LeaveAfterGameTick`、`CarriesPlague`、`AttitudeAtArrival`、`ParentPawnLoadId`、`ChildPawnLoadIds`（`IReadOnlyList<int>`）、`IsReleased`、`IsActiveVisitor`。`IsReleased` 为 `Lifecycle == Released`；`IsActiveVisitor` 为未 Released 且未 Dead。快照是拷贝，后续 Comp 变化不写回已发出的快照。改状态走 `SetLifecycle` / `SetGate` / `SetExtra` / `ReleaseToColony` / `TryMarkOrigin`。事件与行为参数使用 `IRHAH_Pawn`，不得传 Comp。

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

有 Lord 的访客走自有 `LordJob_RHAH_Visitor` + `DutyDef`。图只有赶路、寻食和离场。空派系不切原版防守或袭击。批次伤害和驱逐发 `RHAH_Leave`。无 Lord 回退用独立 `ThinkTreeDef`，`insertTag=Humanlike_PostDuty`，条件是 `RHAH_Api.IsVisitor`，不 xpath 改 `Humanlike.xml`，不按 `PawnKind` 分支。

不能自己走到出口的幼年访客由同 Lord 里允许 `Carry` 的大人带出。

JobGiver 第一行：非访客返回 null；再问 `RHAH_Api.Allows`。角色规则只在 `RHAH_PawnDefaults` 和闸门覆盖里。吃饱后不再乞讨、偷窃、啃咬或由本模组安排进食。

`ReleaseToColony` 必须拆 Lord、清 duty、停访客 JobGiver。标记 Hediff 保留。

其它模组适配只进 `Source/Pawn/Compat/`。基底只暴露闸门、`IRHAH_PawnBehavior` 和 `RHAH_Api` 事件。禁止 Harmony 其它模组私有类型。原版缺口补丁登记在 [engineering.md](engineering.md)。

## 生成

`RHAH_PawnRequest.Profile` 是事件自己的生成配置，不进存档。四个开关各自独立，默认关闭，关闭时不改对应内容：

- `UseExplicitApparel` 与 `Apparel`：先清掉生成器给出的衣服，再穿列表中的装备。空列表表示不穿
- `UseExplicitBackstory` 与 `Childhood` / `Adulthood`：直接替换背景。两者都空时清掉背景。未满 20 岁不写成年背景
- `UseExplicitHealth` 与 `Hediffs`：只追加列表中的 Hediff，不删除生成器已有状态。空列表表示不追加
- `UseExplicitXenotype` 与 `Xenotype` / `XenotypeDefName`：指定异种。两者都空时不指定基因
未设置显式背景时，生成后从 `Source/Data` 的经历表抽一条。自有特质在其后抽取，最多一条。身份仍只看 `RHAH_HungerMark`。经历槽位用发育阶段，年龄上下限仍按每条记录。`pawnHistoriesEnabled` 或 `pawnTraitsEnabled` 关闭、单条被禁用、特质权重为 0 时跳过对应抽取。显式背景不改经历，特质仍抽。成年经历同时写入保底童年 `RHAH_History_Newborn`。幼年关联只影响抽取权重，不预写成年背景。

`optimizeGeneration` 默认开启，登记在 IrisMenus 实验页和原版设置窗口。开启时跳过关系、头衔、随机装备、成瘾、食物和世界角色重装。事件没有显式基因时，按基因页权重抽取已启用异种。权重合计为 0、生物科技未开或 Def 丢失时，依次尝试 `RK_XenoType_Ratkin` 和 `RHAH_Xenotype_Ratkin`。两者都不存在时保持原版默认。关闭优化不取消事件 Profile，也不取消经历和特质抽取。`RHAH_` 基因开关只在异种套上后追加，冲突则跳过。
地图事件默认分帧：排队事件每 64 tick 执行一条。`staggerGeneration` 关闭后连续执行。广播只从 `BroadcastEligible` 且未被单独关闭的事件里抽。

商队伏击先生成未入场的 pawn，再交给原版商队地图。
