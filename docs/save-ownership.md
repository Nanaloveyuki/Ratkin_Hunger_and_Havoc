# 存档与卸载归属

本页是存档键和卸载归属的唯一清单。改 `Scribe_*.Look`、可序列化类型名、`hediffClass` / `workerClass` / XML `Class=`，或新增会进 `.rws` 的 Def，必须同步本页。流程见 [bug-handling.md](bug-handling.md)。

本页登记当前存档契约，不是旧草案。不把本页当成下一次改键或迁命名空间的理由。本模组不读旧鼠灾档。

卸载动作只有 `Remove` 或 `Replace`。导出器尚未实现。没有登记的持久化产物视为漏了卸载保护。

## 破坏性重建（1.0.0 前）

M0 工程对齐对实验性存档格式做 **破坏性重建**，不读旧键、不兼容旧 XML 类型名、不提供迁移。含本模组身份标记的旧档加载后，旧类型节点与旧键视为未知或不完整。

旧契约（废弃，不读）：

- 存档键 `sourceIncidentId`
- `HungerAndHavoc.Hediff_HungerMark`
- `HungerAndHavoc.Api.CompHungerPawn`
- `HungerAndHavoc.Api.CompProperties_HungerPawn`

新契约类型名（与 XML `hediffClass` / Comp `Class` 同步）：

- `HungerAndHavoc.Identity.Hediff_HungerMark`
- `HungerAndHavoc.Identity.CompHungerPawn`
- `HungerAndHavoc.Identity.CompProperties_HungerPawn`

Scribe 默认值必须等于字段默认值。集合在 `PostLoadInit` 补空集合；null 与空集合加载后语义相同，都是空集合。`SetExtra(key, null)` 删除键。

## 游戏存档键

`HungerAndHavocSettings` 是全局 `ModSettings`，不写入 `.rws`，见下方非存档表。

### CompHungerPawn

类型名：`HungerAndHavoc.Identity.CompHungerPawn`。字段名不是存档键；本表键名与字段一致，camelCase。

| 字段 | 存档键 | 默认值 | 集合 | 说明 |
| --- | --- | --- | --- | --- |
| sourceIncidentDisplayId | sourceIncidentDisplayId | null | 否 | 取代旧键 `sourceIncidentId`；破坏性重建，不读旧键 |
| spawnBatchId | spawnBatchId | 0 | 否 | |
| relationshipGroupId | relationshipGroupId | 0 | 否 | |
| role | role | Unspecified | 否 | `HungerPawnRole` |
| lifecycle | lifecycle | Arriving | 否 | `HungerLifecycle` |
| hasBeenFed | hasBeenFed | false | 否 | |
| leaveAfterGameTick | leaveAfterGameTick | -1 | 否 | |
| carriesPlague | carriesPlague | false | 否 | |
| attitudeAtArrival | attitudeAtArrival | Neutral | 否 | `HungerAttitude` |
| attitude | attitude | Neutral | 否 | 当前 `HungerAttitude`。旧档缺键且到达态度不是 Neutral 时回退到 `attitudeAtArrival` |
| parentPawnLoadId | parentPawnLoadId | 0 | 否 | |
| childPawnLoadIds | childPawnLoadIds | 空集合 | 是 | `PostLoadInit` 补 `List<int>`；null 与空集合语义相同 |
| gateOverrides | gateOverrides | 空集合 | 是 | `PostLoadInit` 补字典；null 与空集合语义相同 |
| extraData | extraData | 空集合 | 是 | `PostLoadInit` 补字典；null 与空集合语义相同；`SetExtra(key, null)` 删除键 |

计算属性不入档：`IsReleased`、`IsActiveVisitor`、`RoleLabelKey`。

### LordJob_RHAH_Visitor

类型名：`HungerAndHavoc.Pawn.LordJob_RHAH_Visitor`。

| 字段 | 存档键 | 默认值 | 集合 | 说明 |
| --- | --- | --- | --- | --- |
| faction | faction | null | 否 | 访客 Lord 使用的态度派系引用 |
| waitSpot | waitSpot | IntVec3.Invalid | 否 | 寻食集合点 |
| familyRole | familyRole | Unspecified | 否 | `HungerPawnRole` |


## 可序列化类型

| 类型 | 出现位置 | 卸载 |
| --- | --- | --- |
| HungerAndHavoc.Identity.Hediff_HungerMark | Hediff `Class` / `hediffClass` | Remove，随身份 Hediff 删除 |
| HungerAndHavoc.Identity.CompHungerPawn | HediffComp `Class` | Remove，随身份 Hediff 删除 |
| HungerAndHavoc.Identity.CompProperties_HungerPawn | Def XML `Class` | 不单独出现在 `.rws` |
| HungerAndHavoc.Pawn.LordJob_RHAH_Visitor | Lord `lordJob` | Remove。卸载后该 Lord 必须消失，pawn 回原版 ThinkTree |
| HungerAndHavoc.Pawn.JobDriver_RHAH_Beg | Job `driverClass` | Remove |
| HungerAndHavoc.Pawn.JobDriver_RHAH_Gnaw | Job `driverClass` | Remove |
| HungerAndHavoc.Pawn.ThinkNode_ConditionalRHAH_Visitor | ThinkTree XML `Class` | 不单独出现在 `.rws` |
| HungerAndHavoc.Pawn.JobGiver_RHAH_* | Duty / ThinkTree XML `Class` | 不单独出现在 `.rws` |
| HungerAndHavoc.Pawn.Area_RHAH_Relief | AreaManager `areas` | Remove。卸载后区域节点消失，格子不迁到家区 |

尚无 GameComponent、MapComponent、Letter、Quest、Thing、WorldObject 的存档类型。新增时先加行。
### Generation runtime

| 类型 | 字段 | 存档键 | 卸载 |
| --- | --- | --- | --- |
| HungerAndHavoc.Core.GameComponent_HungerAndHavoc | active generation batches | activeGenerationBatches | Remove |
| HungerAndHavoc.Core.GameComponent_HungerAndHavoc | pending incident display IDs | pendingIncidentDisplayIds | Remove |
| HungerAndHavoc.Core.GameComponent_HungerAndHavoc | plague return load ID | plagueReturnLoadId | Remove |
| HungerAndHavoc.Core.GameComponent_HungerAndHavoc | plague return map ID | plagueReturnMapId | Remove |
| HungerAndHavoc.Core.GameComponent_HungerAndHavoc | plague return phase | plagueReturnPhase | Remove |
| HungerAndHavoc.Core.GameComponent_HungerAndHavoc | plague return due tick | plagueReturnDueTick | Remove |
| HungerAndHavoc.Core.GameComponent_HungerAndHavoc | plague return leave tick | plagueReturnLeaveTick | Remove |
| HungerAndHavoc.Core.MapComponent_HungerAndHavoc | visitor pawn load IDs | visitorPawnLoadIds | Remove |
| HungerAndHavoc.Core.MapComponent_HungerAndHavoc | food search ticks | foodSearchTicks | Remove |
| HungerAndHavoc.Core.MapComponent_HungerAndHavoc | plague quarantine load IDs | plagueQuarantineLoadIds | Remove |
| HungerAndHavoc.Core.MapComponent_HungerAndHavoc | plague recovered count | plagueRecovered | Remove |
| HungerAndHavoc.Core.MapComponent_HungerAndHavoc | plague death count | plagueDied | Remove |
| HungerAndHavoc.Core.MapComponent_HungerAndHavoc | plague last spread day | plagueLastSpreadDay | Remove |

生成队列、批次保护和全局调度属于唯一全局运行时组件；本图访客索引和寻食缓存属于唯一地图组件。地图拆除时由 MapComponent 随地图卸载，不能保留 Pawn 或 Map 引用。

## Def

| defName | 种类 | 卸载 |
| --- | --- | --- |
| RHAH_HungerMark | HediffDef | Remove。身份标记，不是伤病，不替换成原版 Hediff |
| RHAH_Plague | HediffDef | Remove。不替换成原版 Plague |
| RHAH_RefeedingSyndrome | HediffDef | Remove。不替换成原版 Hediff |
| RHAH_LargeRefugeeWave | IncidentDef | Remove |
| RHAH_ThiefRatkinGroup | IncidentDef | Remove |
| RHAH_AbandonedRatkinChildren | IncidentDef | Remove |
| RHAH_ShatteredMother | IncidentDef | Remove |
| RHAH_BeggarFamily | IncidentDef | Remove |
| RHAH_BeggarGroup | IncidentDef | Remove |
| RHAH_ThiefRatkinChildGroup | IncidentDef | Remove |
| RHAH_WildRatkinWandersIn | IncidentDef | Remove |
| RHAH_WildRatkinChildWandersIn | IncidentDef | Remove |
| RHAH_WildRatkinGroupWandersIn | IncidentDef | Remove |
| RHAH_FamineRefugees | IncidentDef | Remove |
| RHAH_AidSimpleMeal, RHAH_AidFineMeal, RHAH_AidMedicine, RHAH_AidSilver, RHAH_AidBaby | IncidentDef | Remove |
| RHAH_IntelTreasureSimpleMeal, RHAH_IntelTreasureHerbal, RHAH_IntelTreasureSilver | IncidentDef | Remove |
| RHAH_IntelStructureSimpleMeal, RHAH_IntelStructureHerbal, RHAH_IntelStructureSilver | IncidentDef | Remove |
| RHAH_IntelSettlementSimpleMeal, RHAH_IntelSettlementHerbal, RHAH_IntelSettlementSilver | IncidentDef | Remove |
| RHAH_LaboringRefugees, RHAH_StrongSiege, RHAH_Passersby, RHAH_AirdropMistake, RHAH_MisguidedKinship | IncidentDef | Remove |
| RHAH_GreatFamine, RHAH_CaravanMuggers, RHAH_PlagueWanderers, RHAH_PlagueAbandonedBabies, RHAH_PlagueTraderCaravan, RHAH_PlaguePassersby | IncidentDef | Remove |
| RHAH_PlagueCaravanMuggers | IncidentDef | Remove |
| RHAH_PlagueRefugees, RHAH_PlagueOrphan, RHAH_PlagueBeggarGroup, RHAH_PlagueThiefGroup, RHAH_PlagueLaboringRefugees | IncidentDef | Remove |
| RHAH_PlagueStrongSiege, RHAH_PlagueAirdropMistake, RHAH_PlagueMisguidedKinship, RHAH_PlagueGreatFamine, RHAH_PlagueRevenge | IncidentDef | Remove |
| RHAH_RefugeeMassacre | IncidentDef | Remove |
| RHAH_ChildExchange | IncidentDef | Remove |
| HungerAndHavoc.Narrative.NarrativeState | revealedCount, trust, rescued, lost, failed, suiyinEnabled, suiyinStarted, suiyinDeadlineTick | revealedCount, trust, rescued, lost, failed, suiyinEnabled, suiyinStarted, suiyinDeadlineTick | Remove |
| RHAH_BeggarSiege | IncidentDef | Remove |
| RHAH_Beg | JobDef | Remove |
| RHAH_Gnaw | JobDef | Remove |
| RHAH_VisitorSeek | DutyDef | Remove |
| RHAH_VisitorLeave | DutyDef | Remove |
| RHAH_VisitorFallback | ThinkTreeDef | Remove |
| RHAH_Gene_ThinRations | GeneDef | Remove。不替换成原版基因 |
| RHAH_Xenotype_Ratkin | XenotypeDef | Remove。不替换成原版异种 |
| RHAH_XenotypeIcon_Ratkin | XenotypeIconDef | Remove |
| RHAH_Faction_Hostile | FactionDef | Remove。隐藏空派系，不替换成原版派系 |
| RHAH_Faction_LeaningHostile | FactionDef | Remove |
| RHAH_Faction_Neutral | FactionDef | Remove |
| RHAH_Faction_LeaningFriendly | FactionDef | Remove |
| RHAH_Faction_Friendly | FactionDef | Remove |

尚无 PawnKind、Backstory、TraderKind、Site、Thing、Letter。出现 `Replace` 时必须写替代 Def，且替代 Def 不能属于本模组。

## 非存档

这些不进游戏 `.rws`，卸载导出器也不清理。

| 名称 | 说明 |
| --- | --- |
| nanaloveyuki.ratkin.hungerandhavoc | packageId。清理副本的 meta 里应去掉本包，但不在运行时存档字段中 |
| HungerAndHavocSettings.enableNewContent | 全局 ModSettings，默认 true |
| HungerAndHavocSettings.optimizeGeneration | 全局 ModSettings，默认 true。关闭后不套权重异种，事件 Profile 仍生效 |
| HungerAndHavocSettings.positiveIncidentDays | 全局 ModSettings，默认 15。正池平均天数，0 关闭，上限 60 |
| HungerAndHavocSettings.negativeIncidentDays | 全局 ModSettings，默认 15。负池平均天数，0 关闭，上限 60 |
| HungerAndHavocSettings.xenotypeWeights | 全局 ModSettings，默认空字典。缺键用登记建议权重。空字典不是全部禁用 |
| HungerAndHavocSettings.enabledXenotypeDefNames | 全局 ModSettings，默认空。玩家加入的外部异种 defName |
| HungerAndHavocSettings.enabledGeneDefNames | 全局 ModSettings，默认空。只允许 `RHAH_` 基因在生成后追加 |
| HungerAndHavocSettings.reliefEnabled | 全局 ModSettings，默认 true。关闭后访客不受赈灾区限制 |
| HungerAndHavocSettings.allowEatOutsideRelief | 全局 ModSettings，默认 false。空值不是允许区外取食 |
| HungerAndHavocSettings.ignoreReliefAfterFed | 全局 ModSettings，默认 false |
| HungerAndHavocSettings.leaveAfterFed | 全局 ModSettings，默认 true |
| HungerAndHavocSettings.disabledReliefFoodDefNames | 全局 ModSettings，默认空。空名单表示当前食物可用，不是全部禁用 |
| HungerAndHavoc.Guard.* | Guard 始终加载，无存档类型 |
| HungerAndHavocMod / HarmonyBootstrap / HungerAndHavocRuntime | 运行时入口，无 ExposeData |

## 卸载边界

- 按本页识别所有权，不删除其它模组的特质、种族、基因或 Hediff
- 原档不覆盖；失败则中止，不把半成品当成可卸载副本
- 未知本模组 Def 或类型引用必须中止，不猜测删除其所属节点
- 清理交叉引用时保持 Scribe 字典 keys/values 同步
- 全局 ModSettings 不改；卸模组后设置文件可残留
