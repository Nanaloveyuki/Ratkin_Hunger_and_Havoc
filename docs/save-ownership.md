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

- `HungerAndHavoc.Identity.Hediff_RHAH_Mark`
- `HungerAndHavoc.Identity.CompRHAH_Pawn`
- `HungerAndHavoc.Identity.CompProperties_RHAH_Pawn`

Scribe 默认值必须等于字段默认值。集合在 `PostLoadInit` 补空集合；null 与空集合加载后语义相同，都是空集合。`SetExtra(key, null)` 删除键。

## 游戏存档键

`RHAH_Settings` 是全局 `ModSettings`，不写入 `.rws`，见下方非存档表。

### CompRHAH_Pawn

类型名：`HungerAndHavoc.Identity.CompRHAH_Pawn`。字段名不是存档键；本表键名与字段一致，camelCase。

| 字段 | 存档键 | 默认值 | 集合 | 说明 |
| --- | --- | --- | --- | --- |
| sourceIncidentDisplayId | sourceIncidentDisplayId | null | 否 | 取代旧键 `sourceIncidentId`；破坏性重建，不读旧键 |
| spawnBatchId | spawnBatchId | 0 | 否 | |
| relationshipGroupId | relationshipGroupId | 0 | 否 | |
| role | role | Unspecified | 否 | `RHAH_PawnRole` |
| lifecycle | lifecycle | Arriving | 否 | `RHAH_Lifecycle` |
| hasBeenFed | hasBeenFed | false | 否 | |
| leaveAfterGameTick | leaveAfterGameTick | -1 | 否 | |
| carriesPlague | carriesPlague | false | 否 | |
| attitudeAtArrival | attitudeAtArrival | Neutral | 否 | `RHAH_Attitude` |
| attitude | attitude | Neutral | 否 | 当前 `RHAH_Attitude`。旧档缺键且到达态度不是 Neutral 时回退到 `attitudeAtArrival` |
| parentPawnLoadId | parentPawnLoadId | 0 | 否 | |
| childPawnLoadIds | childPawnLoadIds | 空集合 | 是 | `PostLoadInit` 补 `List<int>`；null 与空集合语义相同 |
| gateOverrides | gateOverrides | 空集合 | 是 | `PostLoadInit` 补字典；null 与空集合语义相同 |
| extraData | extraData | 空集合 | 是 | `PostLoadInit` 补字典；null 与空集合语义相同；`SetExtra(key, null)` 删除键 |
| droppedChildLoadIds | droppedChildLoadIds | 空集合 | 是 | 已放下的孩子 Load ID。`PostLoadInit` 补空列表 |

计算属性不入档：`IsReleased`、`IsActiveVisitor`、`RoleLabelKey`。

### LordJob_RHAH_Visitor

类型名：`HungerAndHavoc.Pawn.LordJob_RHAH_Visitor`。

| 字段 | 存档键 | 默认值 | 集合 | 说明 |
| --- | --- | --- | --- | --- |
| faction | faction | null | 否 | 访客 Lord 使用的态度派系引用 |
| waitSpot | waitSpot | IntVec3.Invalid | 否 | 寻食集合点 |
| familyRole | familyRole | Unspecified | 否 | `RHAH_PawnRole` |


### ChoiceLetter_RHAH_Request

类型名：`HungerAndHavoc.Incidents.ChoiceLetter_RHAH_Request`。

| 字段 | 存档键 | 默认值 | 集合 | 说明 |
| --- | --- | --- | --- | --- |
| choiceId | choiceId | 0 | 否 | 对应 `RHAH_ChoiceRecord.id` |
| mapId | mapId | 0 | 否 | |
| kind | kind | None | 否 | `RHAH_RequestKind` |
| site | site | None | 否 | `RHAH_IntelSiteKind` |
| amount | amount | 0 | 否 | |
| expireTick | expireTick | -1 | 否 | |

### ChoiceLetter_RHAH_Visitors

类型名：`HungerAndHavoc.Incidents.ChoiceLetter_RHAH_Visitors`。

| 字段 | 存档键 | 默认值 | 集合 | 说明 |
| --- | --- | --- | --- | --- |
| choiceId | choiceId | 0 | 否 | 对应 `RHAH_ChoiceRecord.id` |
| choice | choice | Visitors | 否 | `RHAH_ChoiceKind` |

### RHAH_ChoiceRecord

类型名：`HungerAndHavoc.Incidents.RHAH_ChoiceRecord`。嵌在 `openChoices` 里。

| 字段 | 存档键 | 默认值 | 集合 | 说明 |
| --- | --- | --- | --- | --- |
| Id | id | 0 | 否 | |
| DisplayId | displayId | 空字符串 | 否 | `PostLoadInit` 把 null 补成空字符串 |
| MapId | mapId | 0 | 否 | |
| BatchId | batchId | 0 | 否 | |
| Kind | kind | None | 否 | `RHAH_RequestKind` |
| Site | site | None | 否 | `RHAH_IntelSiteKind` |
| Choice | choice | None | 否 | `RHAH_ChoiceKind` |
| Amount | amount | 0 | 否 | |
| ExpireTick | expireTick | -1 | 否 | |
| Settled | settled | None | 否 | `RHAH_ChoiceAction` |
| PawnLoadIds | pawnLoadIds | 空集合 | 是 | `PostLoadInit` 补 `List<int>`；null 与空集合语义相同 |

### WorldObject_RHAH_RefugeeCamp

类型名：`HungerAndHavoc.Incidents.WorldObject_RHAH_RefugeeCamp`。

| 字段 | 存档键 | 默认值 | 集合 | 说明 |
| --- | --- | --- | --- | --- |
| residents | residents | 空集合 | 是 | 居民引用。`PostLoadInit` 补空列表；null 与空集合语义相同 |
| cleared | cleared | false | 否 | |
| nextCheck | nextCheck | -1 | 否 | 下次检查 tick |


## 可序列化类型

| 类型 | 出现位置 | 卸载 |
| --- | --- | --- |
| HungerAndHavoc.Identity.Hediff_RHAH_Mark | Hediff `Class` / `hediffClass` | Remove，随身份 Hediff 删除 |
| HungerAndHavoc.Identity.CompRHAH_Pawn | HediffComp `Class` | Remove，随身份 Hediff 删除 |
| HungerAndHavoc.Identity.CompProperties_RHAH_Pawn | Def XML `Class` | 不单独出现在 `.rws` |
| HungerAndHavoc.Pawn.LordJob_RHAH_Visitor | Lord `lordJob` | Remove。卸载后该 Lord 必须消失，pawn 回原版 ThinkTree |
| HungerAndHavoc.Pawn.JobDriver_RHAH_Beg | Job `driverClass` | Remove |
| HungerAndHavoc.Pawn.JobDriver_RHAH_Gnaw | Job `driverClass` | Remove |
| HungerAndHavoc.Pawn.JobDriver_RHAH_DropChild | Job `driverClass` | Remove |
| HungerAndHavoc.Pawn.JobDriver_RHAH_MotherFeed | Job `driverClass` | Remove |
| HungerAndHavoc.Pawn.JobDriver_RHAH_Scavenge | Job `driverClass` | Remove |
| HungerAndHavoc.Pawn.JobDriver_RHAH_TailBite | Job `driverClass` | Remove |
| HungerAndHavoc.Incidents.ChoiceLetter_RHAH_Request | Letter `letterClass` | Remove |
| HungerAndHavoc.Incidents.ChoiceLetter_RHAH_Visitors | Letter `letterClass` | Remove |
| HungerAndHavoc.Incidents.WorldObject_RHAH_RefugeeCamp | WorldObject `Class` | Remove。居民引用随营地删除，不替换成原版 Site |
| HungerAndHavoc.Pawn.ThinkNode_ConditionalRHAH_Visitor | ThinkTree XML `Class` | 不单独出现在 `.rws` |
| HungerAndHavoc.Pawn.JobGiver_RHAH_* | Duty / ThinkTree XML `Class` | 不单独出现在 `.rws` |
| HungerAndHavoc.Pawn.Area_RHAH_Relief | AreaManager `areas` | Remove。卸载后区域节点消失，格子不迁到家区 |
| HungerAndHavoc.Pawn.Hediff_RHAH_ClaySatiety | Hediff `Class` / `hediffClass` | Remove，随饱腹 Hediff 删除 |
| HungerAndHavoc.Pawn.Comp_RHAH_Clay | ThingComp `Class` | Remove，随观音土物品删除 |
| HungerAndHavoc.Pawn.CompProperties_RHAH_Clay | Def XML `Class` | 不单独出现在 `.rws` |

Letter 与 Quest 的类型见上表。WorldObject `RHAH_RefugeeCamp` 已登记。GameComponent 与 MapComponent 的键在下一节。
### Generation runtime

| 类型 | 字段 | 存档键 | 卸载 |
| --- | --- | --- | --- |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | active generation batches | activeGenerationBatches | Remove |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | pending incident display IDs | pendingIncidentDisplayIds | Remove |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | pending incident points | pendingIncidentPoints | Remove。与显示 ID 等长。旧档缺列表时按当前调试点补齐 |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | plague return load ID | plagueReturnLoadId | Remove |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | plague return map ID | plagueReturnMapId | Remove |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | plague return phase | plagueReturnPhase | Remove |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | plague return due tick | plagueReturnDueTick | Remove |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | plague return leave tick | plagueReturnLeaveTick | Remove |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | open choices | openChoices | Remove |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | next choice id | nextChoiceId | Remove |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | broadcast cooldown tick | broadcastCooldownUntilTick | Remove |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | generation cursor | generationCursor | Remove |
| HungerAndHavoc.Core.MapComponent_RHAH_Map | visitor pawn load IDs | visitorPawnLoadIds | Remove |
| HungerAndHavoc.Core.MapComponent_RHAH_Map | food search ticks | foodSearchTicks | Remove |
| HungerAndHavoc.Core.MapComponent_RHAH_Map | plague quarantine load IDs | plagueQuarantineLoadIds | Remove |
| HungerAndHavoc.Core.MapComponent_RHAH_Map | plague recovered count | plagueRecovered | Remove |
| HungerAndHavoc.Core.MapComponent_RHAH_Map | plague death count | plagueDied | Remove |
| HungerAndHavoc.Core.MapComponent_RHAH_Map | plague last spread day | plagueLastSpreadDay | Remove |
| HungerAndHavoc.Core.MapComponent_RHAH_Map | wall gnaw counts | wallGnawCounts | Remove |

生成队列、批次保护和全局调度属于唯一全局运行时组件；本图访客索引和寻食缓存属于唯一地图组件。地图拆除时由 MapComponent 随地图卸载，不能保留 Pawn 或 Map 引用。

## Def

| defName | 种类 | 卸载 |
| --- | --- | --- |
| RHAH_HungerMark | HediffDef | Remove。身份标记，不是伤病，不替换成原版 Hediff |
| RHAH_Plague | HediffDef | Remove。不替换成原版 Plague |
| RHAH_RefeedingSyndrome | HediffDef | Remove。不替换成原版 Hediff |
| RHAH_GnawedBark | HediffDef | Remove。不替换成原版 Hediff |
| RHAH_GnawedWall | HediffDef | Remove。不替换成原版 Hediff |
| RHAH_OvergnawedWall | HediffDef | Remove。不替换成原版 Hediff |
| RHAH_ClaySatiety | HediffDef | Remove。不替换成原版 Hediff |
| RHAH_GuanyinTu | ThingDef | Remove。不替换成原版食物 |
| RHAH_MakeGuanyinTu | RecipeDef | Remove |
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
| RHAH_DropChild | JobDef | Remove |
| RHAH_MotherFeed | JobDef | Remove |
| RHAH_Scavenge | JobDef | Remove |
| RHAH_TailBite | JobDef | Remove |
| RHAH_RefugeeMassacre | QuestScriptDef | Remove |
| RHAH_RefugeeCamp | WorldObjectDef / SitePartDef / MapGeneratorDef / GenStepDef | Remove。不替换成原版地点 |
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
| RHAH_History_* | BackstoryDef | Remove。不替换成原版背景 |
| RHAH_Trait_* | TraitDef | Remove。不替换成原版特质 |
| RHAH_Thought_EggKeeperYoung, RHAH_Thought_HungerRage, RHAH_Thought_FoodSnatcher, RHAH_Thought_PlagueDreadSick, RHAH_Thought_PlagueDreadNearby | ThoughtDef | Remove |
| RHAH_Thought_NightTerrors, RHAH_Thought_GrainGreed, RHAH_Thought_Chillblood, RHAH_Thought_FamineGloom, RHAH_Thought_AilingMother, RHAH_Thought_FamilyThief | ThoughtDef | Remove |
| HungerAndHavoc.Pawn.ThoughtWorker_RHAH_YoungInNeed | 无存档字段 | Remove |
| HungerAndHavoc.Pawn.ThoughtWorker_RHAH_NearbyDisease | 无存档字段 | Remove |

尚无 PawnKind、TraderKind、Site、Letter。出现 `Replace` 时必须写替代 Def，且替代 Def 不能属于本模组。

### Hediff_RHAH_ClaySatiety

类型名：`HungerAndHavoc.Pawn.Hediff_RHAH_ClaySatiety`。

| 字段 | 存档键 | 默认值 | 集合 | 说明 |
| --- | --- | --- | --- | --- |
| windowStartTick | windowStartTick | -1 | 否 | 当前十五天窗口起点。-1 表示没有窗口 |
| ingestionCount | ingestionCount | 0 | 否 | 本窗口已吃块数，读档后夹到 0..3 |

## 非存档

这些不进游戏 `.rws`，卸载导出器也不清理。

| 名称 | 说明 |
| --- | --- |
| nanaloveyuki.ratkin.hungerandhavoc | packageId。清理副本的 meta 里应去掉本包，但不在运行时存档字段中 |
| RHAH_Settings.enableNewContent | 全局 ModSettings，默认 true |
| RHAH_Settings.optimizeGeneration | 全局 ModSettings，默认 true。关闭后不套权重异种，事件 Profile 仍生效 |
| RHAH_Settings.positiveIncidentDays | 全局 ModSettings，默认 15。正池平均天数，0 关闭，上限 60 |
| RHAH_Settings.negativeIncidentDays | 全局 ModSettings，默认 15。负池平均天数，0 关闭，上限 60 |
| RHAH_Settings.xenotypeWeights | 全局 ModSettings，默认空字典。缺键用登记建议权重。空字典不是全部禁用 |
| RHAH_Settings.enabledXenotypeDefNames | 全局 ModSettings，默认空。玩家加入的外部异种 defName |
| RHAH_Settings.enabledGeneDefNames | 全局 ModSettings，默认空。只允许 `RHAH_` 基因在生成后追加 |
| RHAH_Settings.reliefEnabled | 全局 ModSettings，默认 true。关闭后访客不受赈灾区限制 |
| RHAH_Settings.allowEatOutsideRelief | 全局 ModSettings，默认 false。空值不是允许区外取食 |
| RHAH_Settings.ignoreReliefAfterFed | 全局 ModSettings，默认 false |
| RHAH_Settings.leaveAfterFed | 全局 ModSettings，默认 true |
| RHAH_Settings.disabledReliefFoodDefNames | 全局 ModSettings，默认空。空名单表示当前食物可用，不是全部禁用 |
| RHAH_Settings.aidRequestsEnabled | 全局 ModSettings，默认 true |
| RHAH_Settings.intelTradesEnabled | 全局 ModSettings，默认 true |
| RHAH_Settings.visitorChoicesEnabled | 全局 ModSettings，默认 true |
| RHAH_Settings.familyDropEnabled | 全局 ModSettings，默认 true |
| RHAH_Settings.motherFeedEnabled | 全局 ModSettings，默认 true |
| RHAH_Settings.prisonerScavengeEnabled | 全局 ModSettings，默认 true |
| RHAH_Settings.tailBiteEnabled | 全局 ModSettings，默认 false |
| RHAH_Settings.broadcastEnabled | 全局 ModSettings，默认 true |
| RHAH_Settings.broadcastCooldownDays | 全局 ModSettings，默认 3，范围 0 到 10 |
| RHAH_Settings.staggerGeneration | 全局 ModSettings，默认 true |
| RHAH_Settings.disabledIncidentDisplayIds | 全局 ModSettings，默认空。空名单表示事件可用 |
| RHAH_Settings.incidentDebugPoints | 全局 ModSettings，默认空字典。缺键用目录调试点。范围 1 到 10000 |
| RHAH_Settings.incidentWeights | 全局 ModSettings，默认空字典。缺键为 100，表示目录权重。0 不抽，上限 100。空字典不是全部禁用 |
| RHAH_Settings.refugeeCampEnabled | 全局 ModSettings，默认 true |
| RHAH_Settings.pawnHistoriesEnabled | 全局 ModSettings，默认 true。关闭后新来客不抽本模组经历 |
| RHAH_Settings.pawnTraitsEnabled | 全局 ModSettings，默认 true。关闭后新来客不抽本模组特质 |
| RHAH_Settings.disabledHistoryDisplayIds | 全局 ModSettings，默认空。空名单表示经历可抽 |
| RHAH_Settings.disabledTraitDisplayIds | 全局 ModSettings，默认空。空名单表示特质可抽 |
| RHAH_Settings.traitWeights | 全局 ModSettings，默认空字典。缺键用目录概率乘 100。0 不抽 |
| HungerAndHavoc.Guard.* | Guard 始终加载，无存档类型 |
| RHAH_Mod / HarmonyBootstrap / RHAH_Runtime | 运行时入口，无 ExposeData |

## 卸载边界

- 按本页识别所有权，不删除其它模组的特质、种族、基因或 Hediff
- 原档不覆盖；失败则中止，不把半成品当成可卸载副本
- 未知本模组 Def 或类型引用必须中止，不猜测删除其所属节点
- 清理交叉引用时保持 Scribe 字典 keys/values 同步
- 全局 ModSettings 不改；卸模组后设置文件可残留
