# Pawn 身份

来源标记是 Hediff `RHAH_HungerMark`，存档数据在 Identity 的 `CompHungerPawn`。其它模组只引用独立的 `HungerAndHavoc.Api.dll`，使用 `HungerAndHavoc.Api` 中的 `HungerAndHavocApi`、`IHungerPawn`、`HungerPawnSnapshot`、闸门与 Seed。不要扫描 Hediff、Backstory 或 Comp。`CompHungerPawn` 不实现 `IHungerPawn`。

`HungerAndHavocApi` 查询与标记返回 `IHungerPawn`（`HungerPawnSnapshot`，sealed 不可变副本）。只读成员：`SourceIncidentDisplayId`、`SpawnBatchId`、`RelationshipGroupId`、`Role`、`Lifecycle`、`HasBeenFed`、`LeaveAfterGameTick`、`CarriesPlague`、`AttitudeAtArrival`、`ParentPawnLoadId`、`ChildPawnLoadIds`（`IReadOnlyList<int>`）、`IsReleased`、`IsActiveVisitor`。`IsReleased` 为 `Lifecycle == Released`；`IsActiveVisitor` 为未 Released 且未 Dead。快照是拷贝，后续 Comp 变化不写回已发出的快照。改状态走 `SetLifecycle` / `SetGate` / `SetExtra` / `ReleaseToColony` / `TryMarkOrigin`。事件与行为参数使用 `IHungerPawn`，不得传 Comp。

## 生命周期

`Arriving → SeekingFood → Fed → Leaving → Released | Dead`

`ReleaseToColony` 把访客变成殖民地相关 pawn，标记仍在。

角色 `RatkinYoung` 表示幼年鼠族。代码和 API 不使用含义不清的 `Egg` 或 `RatkinEgg`。

存档字段（Comp，新键使用 camelCase；正式发布后存档键冻结）：

- `spawnBatchId`：同一批次生成的 pawn。
- `relationshipGroupId`：同一事件中的家庭或关系组。
- `carriesPlague`：是否携带鼠疫。
- `attitudeAtArrival`：到达时态度快照，不表示当前动态态度。
- `parentPawnLoadId` / `childPawnLoadIds`：Pawn 存档 Load ID 关系。
- `gateOverrides`：本 Pawn 的行为闸门覆盖。
- `extraData`：其它模组使用的私有键值数据。

## 闸门

`HungerBehaviorGate`：Beg, Steal, Fight, LeaveAfterFed, EatOutsideRelief, FeedFromRelief, Gnaw, TailBite, Leash, Carry, JoinColony, Imprison, DropOffChild, ExitMap。

判定顺序：该 pawn 的 `gateOverrides` → `IHungerPawnBehavior`（后注册优先）→ `HungerPawnDefaults`。无来源标记时 `Allows` 为 false。

`IHungerPawnBehavior`：

```csharp
bool? Allows(Pawn pawn, IHungerPawn snapshot, HungerBehaviorGate gate);
bool? ShouldReleaseToColony(Pawn pawn, IHungerPawn snapshot, HungerReleaseReason reason);
```

单只覆盖：

```csharp
HungerAndHavocApi.SetGate(pawn, HungerBehaviorGate.Leash, true);
```

全局：

```csharp
HungerPawnBehaviors.Register(new MyPolicy());
```

私有数据用 `SetExtra(pawn, "your.package.id:key", value)`。

## 运行时层

访客 AI 在 `Source/Pawn`，命名空间 `HungerAndHavoc.Pawn`。Identity 只管标记和闸门数据，不发 Job。

有 Lord 的访客走自有 `LordJob_RHAH_Visitor` + `DutyDef`。无 Lord 回退用独立 `ThinkTreeDef`，`insertTag=Humanlike_PostDuty`，条件是 `HungerAndHavocApi.IsVisitor`，不 xpath 改 `Humanlike.xml`，不按 `PawnKind` 分支。

JobGiver 第一行：非访客返回 null；再问 `HungerAndHavocApi.Allows`。角色规则只在 `HungerPawnDefaults` 和闸门覆盖里。

`ReleaseToColony` 必须拆 Lord、清 duty、停访客 JobGiver。标记 Hediff 保留。

其它模组适配只进 `Source/Pawn/Compat/`。基底只暴露闸门、`IHungerPawnBehavior` 和 `HungerAndHavocApi` 事件。禁止 Harmony 其它模组私有类型。原版缺口补丁也放 Compat，并在 [engineering.md](engineering.md) 登记例外。

## 生成

`HungerPawnRequest.Profile` 是事件自己的生成配置，不进存档。四个开关各自独立，默认关闭，关闭时不改对应内容：

- `UseExplicitApparel` 与 `Apparel`：先清掉生成器给出的衣服，再穿列表中的装备。空列表表示不穿
- `UseExplicitBackstory` 与 `Childhood` / `Adulthood`：直接替换背景。两者都空时清掉背景。未满 20 岁不写成年背景
- `UseExplicitHealth` 与 `Hediffs`：只追加列表中的 Hediff，不删除生成器已有状态。空列表表示不追加
- `UseExplicitXenotype` 与 `Xenotype` / `XenotypeDefName`：指定异种。两者都空时不指定基因

`optimizeGeneration` 默认开启，登记在 IrisMenus 实验页和原版设置窗口。开启时跳过关系、头衔、随机装备、成瘾、食物和世界角色重装。事件没有显式基因时，尝试 `RK_XenoType_Ratkin`。生物科技未启用或 Def 不存在时保持原版默认。关闭优化不取消事件 Profile。

商队伏击先生成未入场的 pawn，再交给原版商队地图。地图事件仍在同一 tick 内生成并入场，不做分帧队列。
