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


穗音深存档类型从 `HungerAndHavoc.Narrative.Suiyin*` 迁到 `HungerAndHavoc.Storyteller.Suiyin.Suiyin*`。破坏性重建，不读旧类型名，不提供迁移。受影响的是 `SuiyinMember`、`SuiyinN004Case`、`SuiyinN005Case`、`SuiyinN006Case`、`SuiyinN007Case`、`SuiyinN008Case`、`SuiyinN009Case`、`SuiyinJournalCase`、`SuiyinNotice`。`NarrativeState` 仍是 `HungerAndHavoc.Narrative.NarrativeState`，存档键不变。

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
| stayKind | stayKind | 0 | 否 | 0 无，1 收留，2 雇佣 |
| stayRemainingTicks | stayRemainingTicks | 0 | 否 | 倒地时冻结的剩余 tick |
| foodWaitUntilTick | foodWaitUntilTick | -1 | 否 | 断粮等待截止。找到食物后回到 -1 |
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
| foodReceiver | foodReceiver | null | 否 | 等待玩家亲手交食物的来客 |
| foodDef | foodDef | null | 否 | 第一次交货后锁定的食物 |
| foodCount | foodCount | 0 | 否 | 需要的份数。0 表示没有在等 |


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

### ChoiceLetter_RHAH_GrainHole

类型名：`HungerAndHavoc.Incidents.ChoiceLetter_RHAH_GrainHole`。

| 字段 | 存档键 | 默认值 | 集合 | 说明 |
| --- | --- | --- | --- | --- |
| mapId | mapId | 0 | 否 | 洞所在地图 |
| followUp | followUp | false | 否 | false 是入口四选，true 是诱饵到期后的追踪或停止 |

### ChoiceLetter_RHAH_Envoy

类型名：`HungerAndHavoc.Incidents.ChoiceLetter_RHAH_Envoy`。

| 字段 | 存档键 | 默认值 | 集合 | 说明 |
| --- | --- | --- | --- | --- |
| mapId | mapId | 0 | 否 | 使者所在地图 |
| pawnId | pawnId | 0 | 否 | 使者 thingID |

### ChoiceLetter_RHAH_Revisit

类型名：`HungerAndHavoc.Incidents.ChoiceLetter_RHAH_Revisit`。

| 字段 | 存档键 | 默认值 | 集合 | 说明 |
| --- | --- | --- | --- | --- |
| caseId | caseId | 0 | 否 | 对应 `SuiyinN004Case.id` |

### ChoiceLetter_RHAH_Quarantine

类型名：`HungerAndHavoc.Incidents.ChoiceLetter_RHAH_Quarantine`。

| 字段 | 存档键 | 默认值 | 集合 | 说明 |
| --- | --- | --- | --- | --- |
| mapId | mapId | 0 | 否 | 待选检疫所在地图 |
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
| HungerAndHavoc.Incidents.ChoiceLetter_RHAH_GrainHole | Letter `letterClass` | Remove |
| HungerAndHavoc.Incidents.ChoiceLetter_RHAH_Envoy | Letter `letterClass` | Remove |
| HungerAndHavoc.Incidents.ChoiceLetter_RHAH_Revisit | Letter `letterClass` | Remove |
| HungerAndHavoc.Incidents.ChoiceLetter_RHAH_Quarantine | Letter `letterClass` | Remove |
| HungerAndHavoc.Pawn.ThinkNode_ConditionalRHAH_Visitor | ThinkTree XML `Class` | 不单独出现在 `.rws` |
| HungerAndHavoc.Pawn.JobGiver_RHAH_* | Duty / ThinkTree XML `Class` | 不单独出现在 `.rws` |
| HungerAndHavoc.Pawn.Area_RHAH_Relief | AreaManager `areas` | Remove。卸载后区域节点消失，格子不迁到家区 |
| HungerAndHavoc.Pawn.Hediff_RHAH_ClaySatiety | Hediff `Class` / `hediffClass` | Remove，随饱腹 Hediff 删除 |
| HungerAndHavoc.Pawn.Comp_RHAH_Clay | ThingComp `Class` | Remove，随观音土物品删除 |
| HungerAndHavoc.Pawn.CompProperties_RHAH_Clay | Def XML `Class` | 不单独出现在 `.rws` |
| HungerAndHavoc.Generation.RHAH_GenerationExtension | ThingDef `modExtensions` XML `Class` | 不单独出现在 `.rws` |
| HungerAndHavoc.Pawn.StatPart_RHAH_TemperatureApparel | StatDef `parts` XML `Class` | 不单独出现在 `.rws` |
| HungerAndHavoc.Incidents.RHAH_PredatorRecord | 捕食者深存档，嵌在 `predators` | Remove。随地图组件删除，不替换成原版动物 |
| HungerAndHavoc.EventMgr.RHAH_EventChainRecord | 事件链深存档，嵌在 `eventChains` | Remove。随游戏组件删除，不替换成原版事件 |

### RHAH_PredatorRecord

类型名：`HungerAndHavoc.Incidents.RHAH_PredatorRecord`。嵌在 `predators` 里。

| 字段 | 存档键 | 默认值 | 集合 | 说明 |
| --- | --- | --- | --- | --- |
| pawn | pawn | null | 否 | 活跃捕食者引用 |
| outside | outside | false | 否 | 本地捕食者为 false，外边生成的为 true |
| nextSearchTick | nextSearchTick | -1 | 否 | 下次允许搜索的 tick。空闲地图不因这个值扫描 |

### RHAH_EventChainRecord

类型名：`HungerAndHavoc.EventMgr.RHAH_EventChainRecord`。嵌在 `eventChains` 里。

| 字段 | 存档键 | 默认值 | 集合 | 说明 |
| --- | --- | --- | --- | --- |
| id | id | 0 | 否 | 实例 ID。读档后 `nextEventChainId` 至少比最大 id 大 1 |
| displayId | displayId | 空字符串 | 否 | 注册键。`PostLoadInit` 把 null 补成空字符串。空键或 id 小于等于 0 的记录丢弃 |
| siteId | siteId | 0 | 否 | 地图为正数 uniqueID，商队为负数 -ID，0 为世界格 |
| stage | stage | 0 | 否 | 事件自己定义的阶段 |
| startedTick | startedTick | 0 | 否 | |
| deadlineTick | deadlineTick | -1 | 否 | -1 表示没有期限。读档后小于 -1 归 -1 |
| payload | payload | 空字符串 | 否 | 事件自己的出现物文本。`PostLoadInit` 把 null 补成空字符串 |
| closed | closed | false | 否 | |
| end | end | 0 | 否 | 0 未结束，1 完成，2 失败，3 到期，4 取消 |

Letter 与 Quest 的类型见上表。WorldObject `RHAH_RefugeeCamp` 已登记。GameComponent 与 MapComponent 的键在下一节。
### Generation runtime

| 类型 | 字段 | 存档键 | 卸载 |
| --- | --- | --- | --- |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | active generation batches | activeGenerationBatches | Remove |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | pending incident display IDs | pendingIncidentDisplayIds | Remove |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | pending incident points | pendingIncidentPoints | Remove。与显示 ID 等长。旧档缺列表时按当前调试点补齐 |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | pending incident target IDs | pendingIncidentTargetIds | Remove。与显示 ID 等长。地图为正数 uniqueID，商队为负数 -ID，0 为未指定。旧档缺列表时按 0 补齐。null 与空集合都是空队列 |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | plague return load ID | plagueReturnLoadId | Remove |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | plague return map ID | plagueReturnMapId | Remove |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | plague return phase | plagueReturnPhase | Remove |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | plague return due tick | plagueReturnDueTick | Remove |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | plague return leave tick | plagueReturnLeaveTick | Remove |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | open choices | openChoices | Remove |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | next choice id | nextChoiceId | Remove |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | broadcast cooldown tick | broadcastCooldownUntilTick | Remove |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | generation cursor | generationCursor | Remove |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | beg cooldown pawn ids | begCooldownPawnIds | Remove。与冷却 tick 等长。旧档缺列表时为空。null 与空集合相同 |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | beg cooldown ticks | begCooldownTicks | Remove。与 pawn id 等长。缺一边时按另一边截齐，短的一边补 -1 |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | begged pawn ids | beggedPawnIds | Remove。与殖民者 id 等长。旧档缺列表时为空 |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | begged colonist ids | beggedColonistIds | Remove。与乞讨者 id 等长。不等长时按短的一边截齐 |
| HungerAndHavoc.Core.MapComponent_RHAH_Map | visitor pawn load IDs | visitorPawnLoadIds | Remove |
| HungerAndHavoc.Core.MapComponent_RHAH_Map | food search ticks | foodSearchTicks | Remove |
| HungerAndHavoc.Core.MapComponent_RHAH_Map | plague quarantine load IDs | plagueQuarantineLoadIds | Remove |
| HungerAndHavoc.Core.MapComponent_RHAH_Map | plague recovered count | plagueRecovered | Remove |
| HungerAndHavoc.Core.MapComponent_RHAH_Map | plague death count | plagueDied | Remove |
| HungerAndHavoc.Core.MapComponent_RHAH_Map | plague last spread day | plagueLastSpreadDay | Remove |
| HungerAndHavoc.Core.MapComponent_RHAH_Map | wall gnaw counts | wallGnawCounts | Remove |
| HungerAndHavoc.Core.MapComponent_RHAH_Map | camp predation prey | predationPrey | Remove |
| HungerAndHavoc.Core.MapComponent_RHAH_Map | camp predators | predators | Remove |
| HungerAndHavoc.Core.MapComponent_RHAH_Map | camp predation rolled | predationRolled | Remove |
| HungerAndHavoc.Core.MapComponent_RHAH_Map | camp predation selected | predationSelected | Remove |
| HungerAndHavoc.Core.MapComponent_RHAH_Map | camp predation pending tick | predationPendingTick | Remove |
| HungerAndHavoc.Core.MapComponent_RHAH_Map | camp predation next tick | predationNextTick | Remove |
| HungerAndHavoc.Core.MapComponent_RHAH_Map | grain hole thing id | holeThingId | Remove |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | event chains | eventChains | Remove。旧档缺列表时为空。null 与空集合都是没有链 |
| HungerAndHavoc.Core.GameComponent_RHAH_Game | next event chain id | nextEventChainId | Remove。默认 1。读档后小于等于已有实例 id 时抬到最大 id 加 1 |

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
| RHAH_Heat_WetCloth, RHAH_Heat_MudCoat, RHAH_Heat_ReedWrap, RHAH_Heat_BarkWrap, RHAH_Heat_MudMantle, RHAH_Heat_MudShell | ThingDef | Remove。不替换成原版衣物 |
| RHAH_Cold_ThinHemp, RHAH_Cold_LayeredHemp, RHAH_Cold_StrawQuilt, RHAH_Cold_FurCloak, RHAH_Cold_SmokedBlanket, RHAH_Cold_HideWrap | ThingDef | Remove。不替换成原版衣物 |
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
| HungerAndHavoc.Narrative.NarrativeState | revealedCount, trust, rescued, lost, failed, suiyinEnabled, suiyinStarted, suiyinDeadlineTick, seenKinds, theftMaps, theftCounts, journalNoted, asidesSent, openingSent, progressSent, rewardClaimed, rewardPaid, rewardDue, envoyClue, relicClue, lastAsideTick, nextCaseId, aidCount, broadcastCount, expulsionCount, adultCount, completedKindCount, completedJournals, firstFactTick, nextAdultCheckTick, relicDone, endingE01, endingE02, endingE03, endingE04, endingE05, identityTier, identityRefused, entrustCases, exchangeCases, holeCases, quarantineCases, envoyCases, relicCase, journalCases, pendingNotices | 同上 | Remove。0.1.0 破坏性重建：结局计数与标记无旧档迁移。`rewardDue` 默认 0，读档后小于 0 归 0。`completedJournals` 为已计入结局的记录编号，空集合与 null 相同，读档后 `completedKindCount` 以它的数量为准 |
| HungerAndHavoc.Storyteller.Suiyin.SuiyinMember | loadId, presence, care, child, careTicks, missingSince | 嵌在案子里 | Remove。随叙事组件删除 |
| HungerAndHavoc.Storyteller.Suiyin.SuiyinN004Case | id, motherId, mapId, startedTick, outcome, mother, missingSince, revisit, revisitSeen, revisitDeadline, meetingCaravanId, rescueDueTick, rescuePaid, breakUntil, breakTrait, effectsApplied, children | 嵌在 entrustCases | Remove |
| HungerAndHavoc.Storyteller.Suiyin.SuiyinN005Case | id, mapId, startedTick, outcome, careClosed, children | 嵌在 exchangeCases | Remove |
| HungerAndHavoc.Storyteller.Suiyin.SuiyinN006Case | mapId, thefts, startedTick, ignoreUntil, baitUntil, hole, foodPresent, wood, baitStock, outcome, losses, nextLossTick | 嵌在 holeCases | Remove |
| HungerAndHavoc.Storyteller.Suiyin.SuiyinN007Case | mapId, startedTick, outcome, choiceOpen, returnDueTick, returnPawnId, returnDone, visitors | 嵌在 quarantineCases | Remove。`choiceOpen` 默认 false，true 表示还没把选择信放进队列 |
| HungerAndHavoc.Storyteller.Suiyin.SuiyinN008Case | mapId, pawnId, startedTick, deadline, checkUntil, presence, missingSince, outcome, mealsReady, proofAvailable | 嵌在 envoyCases | Remove |
| HungerAndHavoc.Storyteller.Suiyin.SuiyinN009Case | startedTick, deadline, mapPresent, playersInside, envoyHere, boxDestroyed, outcome, siteId, boxId, mapEntered | relicCase | Remove |
| HungerAndHavoc.Storyteller.Suiyin.SuiyinJournalCase | id, mapId, batchId, startedTick, delivered, driven, failed, empty, closed, counted, people | 嵌在 journalCases | Remove |
| HungerAndHavoc.Storyteller.Suiyin.SuiyinNotice | letter, arg, privateNotice | 嵌在 pendingNotices | Remove |
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
| RHAH_Gene_LargeLitter | GeneDef | Remove。不替换成原版基因 |
| RHAH_Gene_EarlyFertility | GeneDef | Remove。不替换成原版基因 |
| RHAH_Gene_HighFertility | GeneDef | Remove。不替换成原版基因 |
| RHAH_Gene_RoomFertility | GeneDef | Remove。不替换成原版基因 |
| RHAH_Gene_FastBirth | GeneDef | Remove。不替换成原版基因 |
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
| RHAH_Thought_BeggingSucceeded, RHAH_Thought_BeggingRejected, RHAH_Thought_BeggingSlapped | ThoughtDef | Remove |
| RHAH_Thought_MotherGoneBad, RHAH_Thought_MotherGoneGood, RHAH_Thought_AteMotherGuilt, RHAH_Thought_AteMotherFine, RHAH_Thought_MotherSorry, RHAH_Thought_MotherFine | ThoughtDef | Remove |
| RHAH_Thought_ChildDeadGlad, RHAH_Thought_ChildDeadSad, RHAH_Thought_ChildStarvedSad, RHAH_Thought_ChildStarvedGlad, RHAH_Thought_StealHurtBad, RHAH_Thought_StealHurtGood | ThoughtDef | Remove |
| RHAH_Thought_StealMine, RHAH_Thought_StealGot, RHAH_Thought_SoldAway, RHAH_Thought_SoldFed, RHAH_Thought_TradedAway, RHAH_Thought_TradedFed | ThoughtDef | Remove |
| RHAH_Thought_AidAgain, RHAH_Thought_AidEmpty, RHAH_Thought_FineGood, RHAH_Thought_FineKeep, RHAH_Thought_MedicineEnough, RHAH_Thought_MedicineNext | ThoughtDef | Remove |
| RHAH_Thought_SilverLight, RHAH_Thought_SilverHard, RHAH_Thought_TakenBad, RHAH_Thought_TakenFed, RHAH_Thought_ExtraMouth, RHAH_Thought_BornAlive | ThoughtDef | Remove |
| RHAH_Thought_FellBad, RHAH_Thought_FellGood, RHAH_Thought_WrongKinBad, RHAH_Thought_WrongKinGood, RHAH_Thought_PlagueMotherDeadBad, RHAH_Thought_PlagueMotherDeadGood | ThoughtDef | Remove |
| RHAH_Thought_PlagueLeft, RHAH_Thought_PlagueLived, RHAH_Thought_OrphanBad, RHAH_Thought_OrphanGood, RHAH_Thought_TraderSilent | ThoughtDef | Remove |
| RHAH_Thought_CleanBirth, RHAH_Thought_NextBirth, RHAH_Thought_BornSick, RHAH_Thought_DropSick, RHAH_Thought_DropLived, RHAH_Thought_WrongSick, RHAH_Thought_WrongFed | ThoughtDef | Remove |
| RHAH_Thought_ScavengedFilth, RHAH_Thought_TailBitten, RHAH_Thought_BitATail | ThoughtDef | Remove |
| HungerAndHavoc.Pawn.ThoughtWorker_RHAH_YoungInNeed | 无存档字段 | Remove |
| HungerAndHavoc.Pawn.ThoughtWorker_RHAH_NearbyDisease | 无存档字段 | Remove |

| RHAH_Suiyin | StorytellerDef | Remove。卸载后叙事者换成原版 Randy，不保留穗音定义 |
| RHAH_ChoiceRequest | LetterDef | Remove。选择信随本模组删除，不替换成原版信 |
| RHAH_ChoiceVisitors | LetterDef | Remove。选择信随本模组删除，不替换成原版信 |
| RHAH_QuarantineLetter | LetterDef | Remove。选择信随本模组删除，不替换成原版信 |
| RHAH_PawnKind_Ratkin | PawnKindDef | Remove。不替换成原版 PawnKind。已生成 pawn 的 kindDef 不迁移 |

尚无 TraderKind、Site。出现 `Replace` 时必须写替代 Def，且替代 Def 不能属于本模组。

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
| RHAH_Settings.litterMin | 全局 ModSettings，默认 2。多崽一次分娩的最少数量，范围 1 到 12 |
| RHAH_Settings.litterPeak | 全局 ModSettings，默认 4。多崽概率图的峰，夹在最少和最多之间 |
| RHAH_Settings.litterMax | 全局 ModSettings，默认 6。多崽一次分娩的最多数量，不低于最少 |
| RHAH_Settings.fertileMinAge | 全局 ModSettings，默认 1。早熟允许受孕的最小生理年龄，范围 1 到 14 |
| RHAH_Settings.fertilityPercent | 全局 ModSettings，默认 300。高育相对常人的怀孕几率，范围 100 到 1000 |
| RHAH_Settings.gestationDays | 全局 ModSettings，默认 5.661。速产最短孕期天数，范围 3 到原版下限 5.661 |
| RHAH_Settings.reliefEnabled | 全局 ModSettings，默认 true。关闭后访客不受赈灾区限制 |
| RHAH_Settings.allowEatOutsideRelief | 全局 ModSettings，默认 false。空值不是允许区外取食 |
| RHAH_Settings.ignoreReliefAfterFed | 全局 ModSettings，默认 false |
| RHAH_Settings.leaveAfterFed | 全局 ModSettings，默认 true |
| RHAH_Settings.disabledReliefFoodDefNames | 全局 ModSettings，默认空。空名单表示当前食物可用，不是全部禁用 |
| RHAH_Settings.disabledGiveFoodDefNames | 全局 ModSettings，默认空。空名单表示当前食物可以交给来客，不是全部禁止。不随赈灾区开关 |
| RHAH_Settings.giveFoodListMode | 全局 ModSettings，默认 0。0 按模组，1 按名称，2 按原版分类。只影响给予食物菜单 |
| RHAH_Settings.disabledBegFoodDefNames | 全局 ModSettings，默认空。空名单表示当前正餐可以被乞讨拿走，不是全部禁止。生食不进名单。不随给予食物名单 |
| RHAH_Settings.begFoodListMode | 全局 ModSettings，默认 0。0 按模组，1 按名称，2 按原版分类。只影响乞讨食物菜单 |
| RHAH_Settings.aidRequestsEnabled | 全局 ModSettings，默认 true |
| RHAH_Settings.intelTradesEnabled | 全局 ModSettings，默认 true |
| RHAH_Settings.visitorChoicesEnabled | 全局 ModSettings，默认 true |
| RHAH_Settings.foodGiveHintDismissed | 全局 ModSettings，默认 false。第一次投喂后玩家选择不再提示 |
| RHAH_Settings.traderIgnoresHarshEnvironment | 全局 ModSettings，默认 true。商队不因恶劣环境离图 |
| RHAH_Settings.traderIgnoresEnclosedSpace | 全局 ModSettings，默认 true。商队在封闭房间里不挖路离开 |
| RHAH_Settings.childExchangeFoodSubstitution | 全局 ModSettings，默认 true。易子而食玩家侧可用简单餐代替婴幼儿 |
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
| RHAH_Settings.incidentAttitudes | 全局 ModSettings，默认空字典。缺键用目录态度。值 0 到 4，依次是敌对、偏敌对、中立、偏友好、友善。越界读档后夹回 |
| RHAH_Settings.incidentLongChains | 全局 ModSettings，默认空名单。名单里的显示 ID 预留长链，缺席是短链。现在两种都只生成一次 |
| RHAH_Settings.refugeeCampEnabled | 全局 ModSettings，默认 true |
| RHAH_Settings.refugeePredationChancePercent | 全局 ModSettings，默认 10，范围 0 到 100。每个安居点地图独立掷一次 |
| RHAH_Settings.refugeePredationFightBack | 全局 ModSettings，默认 true。关闭后被追猎的安居点鼠族逃跑 |
| RHAH_Settings.outsidePredatorsFollowDifficulty | 全局 ModSettings，默认 false。开启后外来捕食者无目标时走原版觅食 |
| RHAH_Settings.pawnHistoriesEnabled | 全局 ModSettings，默认 true。关闭后新来客不抽本模组经历 |
| RHAH_Settings.pawnTraitsEnabled | 全局 ModSettings，默认 true。关闭后新来客不抽本模组特质 |
| RHAH_Settings.disabledHistoryDisplayIds | 全局 ModSettings，默认空。空名单表示经历可抽 |
| RHAH_Settings.disabledTraitDisplayIds | 全局 ModSettings，默认空。空名单表示特质可抽 |
| RHAH_Settings.traitWeights | 全局 ModSettings，默认空字典。缺键用目录概率乘 100。0 不抽 |
| RHAH_Settings.maxEventPawns | 全局 ModSettings，默认 30，范围 1 到 100。低于事件最低人数时保留最低人数。母子固定组合不拆 |
| RHAH_Settings.minGeneratedAge | 全局 ModSettings，默认 0。普通来客年龄下限 |
| RHAH_Settings.maxGeneratedAge | 全局 ModSettings，默认 50。普通来客年龄上限，不超过 100 |
| RHAH_Settings.youngAgeFollowsRange | 全局 ModSettings，默认 false。打开后幼年角色使用普通年龄区间。关闭时乞讨幼崽 1 天到 2.9 岁，其它幼年 3 到 6.9 岁。已指定年龄不改 |
| RHAH_Settings.allowImmobileBabies | 全局 ModSettings，默认 false。打开后无幼童模组也可以生成 4 岁以下。幼童模组启用时不抬龄 |
| RHAH_Settings.genderMode | 全局 ModSettings，默认 0。0 随机，1 按比例，2 女性，3 男性 |
| RHAH_Settings.femaleSharePercent | 全局 ModSettings，默认 50，范围 0 到 100。只在按比例时使用 |
| RHAH_Settings.apparelMode | 全局 ModSettings，默认 0。0 和 1 都重配平民衣装，2 重配后再补温度衣，3 不穿衣 |
| RHAH_Settings.apparelListMode | 全局 ModSettings，默认 0。0 按来源，1 按名称，2 按类别。只影响菜单 |
| RHAH_Settings.disabledRefugeeApparelDefNames | 全局 ModSettings，默认空。空名单表示平民衣装都可生成。新加入的衣服默认可生成 |
| RHAH_Settings.maxOwnedTraits | 全局 ModSettings，默认 1，范围 0 到 3。0 不抽本模组特质 |
| RHAH_Settings.allowVanillaTraits | 全局 ModSettings，默认 true。关闭后新来客不获得随年龄出现的原版特质 |
| RHAH_Settings.traitAgeFilter | 全局 ModSettings，默认 true。关闭后幼年特质和成年特质不再按年龄分开 |
| RHAH_Settings.contentListMode | 全局 ModSettings，默认 0。0 按来源，1 按名称，2 按类别。只影响菜单 |
| RHAH_Settings.reliefFoodScoreBonus | 全局 ModSettings，默认 0.1。赈灾区食物额外加分，0 到 1 |
| RHAH_Settings.fedWanderEnabled | 全局 ModSettings，默认 true。开启后吃饱先闲逛再离开，关闭后立刻走向出口。仍受 leaveAfterFed 控制 |
| RHAH_Settings.fedWanderHours | 全局 ModSettings，默认 12，范围 1 到 48。吃饱后闲逛的小时数 |
| RHAH_Settings.waitWhenNoFood | 全局 ModSettings，默认 true。关闭后找不到食物直接离开 |
| RHAH_Settings.noFoodWaitDays | 全局 ModSettings，默认 0.5，范围 0 到 5 |
| RHAH_Settings.shelterDays | 全局 ModSettings，默认 5，范围 5 到 240。短工，1 年按 60 天 |
| RHAH_Settings.hireDays | 全局 ModSettings，默认 240，范围 5 到 2400。长工，1 年按 60 天 |
| RHAH_Settings.coldClothesEnabled | 全局 ModSettings，默认 true。气温超出舒适范围时给新来客一件温度衣 |
| RHAH_Settings.temperatureApparelInsulation | 全局 ModSettings。温度衣 defName 到隔热，范围 0 到 100，缺省用 Def 默认值 |
| RHAH_Settings.disabledTemperatureApparelDefNames | 全局 ModSettings。关闭的温度衣 defName，默认空 |
| RHAH_Settings.minimumEventTemperature | 全局 ModSettings，默认 -35。地图事件下限 |
| RHAH_Settings.maximumEventTemperature | 全局 ModSettings，默认 70。地图事件上限。商队不受限 |
| RHAH_Settings.countEndingsWithoutNarrator | 全局 ModSettings，遗留键。结局只在穗音下计数，这个开关不再读 |
| RHAH_Settings.endingsWithoutNarrator | 全局 ModSettings，遗留键。结局只在穗音下触发，这个开关不再读 |
| RHAH_Settings.endingAidGoal | 全局 ModSettings，默认 99，范围 1 到 999 |
| RHAH_Settings.endingBroadcastGoal | 全局 ModSettings，默认 3，范围 1 到 99 |
| RHAH_Settings.endingExpulsionLimit | 全局 ModSettings，默认 3，范围 0 到 99 |
| RHAH_Settings.endingAdultGoal | 全局 ModSettings，默认 100，范围 1 到 500 |
| RHAH_Settings.endingWaitDays | 全局 ModSettings，默认 30，范围 0 到 120 |
| RHAH_Settings.endingE01 | 全局 ModSettings，默认 true |
| RHAH_Settings.endingE02 | 全局 ModSettings，默认 true |
| RHAH_Settings.endingE03 | 全局 ModSettings，默认 true |
| RHAH_Settings.endingE04 | 全局 ModSettings，默认 true |
| RHAH_Settings.endingE05 | 全局 ModSettings，默认 true |
| RHAH_Settings.endingIdentity | 全局 ModSettings，默认 true。只控制穗音身份询问 |
| RHAH_Settings.plagueEnabled | 全局 ModSettings，默认 true。关闭后新来客不感染，也不再传播 |
| RHAH_Settings.plagueSpreadChancePerCarrier | 全局 ModSettings，默认 0.005，范围 0 到 1 |
| RHAH_Settings.plagueSpreadChanceCap | 全局 ModSettings，默认 0.30，范围 0 到 1 |
| RHAH_Settings.plagueSpreadDayInterval | 全局 ModSettings，默认 3，范围 1 到 30 |
| RHAH_Settings.plagueSpreadHour | 全局 ModSettings，默认 6，范围 0 到 23 |
| RHAH_Settings.plagueBloodPumpingSkipPercent | 全局 ModSettings，默认 120，范围 0 到 300 |
| RHAH_Settings.plagueQuarantineBlocksJoin | 全局 ModSettings，默认 true。关闭后检疫不挡加入、雇佣、转移 |
| RHAH_Settings.plagueReturnEnabled | 全局 ModSettings，默认 true |
| RHAH_Settings.plagueReturnDelayDays | 全局 ModSettings，默认 15，范围 0 到 60 |
| RHAH_Settings.plagueReturnStayDays | 全局 ModSettings，默认 1，范围 0 到 15 |
| RHAH_Settings.beggingEnabled | 全局 ModSettings，默认 true |
| RHAH_Settings.begAutoGiveEnabled | 全局 ModSettings，默认 false。开启后乞讨成功才从对方背包拿走一份允许乞讨的食物 |
| RHAH_Settings.begSuccessChancePercent | 全局 ModSettings，默认 35，范围 0 到 100。对同一殖民者第一次乞讨的基础成功率 |
| RHAH_Settings.begSocialBonusPercent | 全局 ModSettings，默认 3，范围 0 到 20。乞讨者每级社交额外增加的成功率 |
| RHAH_Settings.begFailCooldownHours | 全局 ModSettings，默认 3，范围 3 到 12。合适的人都讨不到后闲逛这么多小时才再乞讨 |
| RHAH_Settings.begSlapChancePercent | 全局 ModSettings，默认 50，范围 0 到 100。同一殖民者第二次及以后被乞讨时抽巴掌的几率 |
| RHAH_Settings.stealingEnabled | 全局 ModSettings，默认 true |
| RHAH_Settings.fightingEnabled | 全局 ModSettings，默认 true |
| RHAH_Settings.gnawingEnabled | 全局 ModSettings，默认 true |
| RHAH_Settings.batchTurnsHostile | 全局 ModSettings，默认 true |
| RHAH_Settings.batchLeavesTogether | 全局 ModSettings，默认 true |
| RHAH_Settings.suiYinThreatTempo | 全局 ModSettings，默认 true。关闭后穗音不再按信任改大型威胁节奏 |
| RHAH_Settings.weightWild | 全局 ModSettings，默认 1.4，范围 0 到 5 |
| RHAH_Settings.weightBeggar | 全局 ModSettings，默认 1 |
| RHAH_Settings.weightThief | 全局 ModSettings，默认 0.7 |
| RHAH_Settings.weightTrade | 全局 ModSettings，默认 0.5 |
| RHAH_Settings.weightSiege | 全局 ModSettings，默认 0.35 |
| RHAH_Settings.weightAid | 全局 ModSettings，默认 0.25 |
| RHAH_Settings.weightSpecial | 全局 ModSettings，默认 0.2 |
| RHAH_Settings.weightIntel | 全局 ModSettings，默认 0.12 |
| RHAH_Settings.weightSeason | 全局 ModSettings，默认 1.1。春冬系数 |
| RHAH_Settings.weightPlague | 全局 ModSettings，默认 0.5。鼠疫事件系数 |
| RHAH_Settings.requestMinSimple | 全局 ModSettings，默认 6。简单餐索取下限，0 到 200 |
| RHAH_Settings.requestMaxSimple | 全局 ModSettings，默认 28。简单餐索取上限，不低于下限 |
| RHAH_Settings.requestMinFine | 全局 ModSettings，默认 4。精致餐索取下限 |
| RHAH_Settings.requestMaxFine | 全局 ModSettings，默认 18。精致餐索取上限 |
| RHAH_Settings.requestMinMedicine | 全局 ModSettings，默认 2。药品索取下限 |
| RHAH_Settings.requestMaxMedicine | 全局 ModSettings，默认 10。药品索取上限 |
| RHAH_Settings.requestMinSilver | 全局 ModSettings，默认 80。白银索取下限 |
| RHAH_Settings.requestMaxSilver | 全局 ModSettings，默认 1200。白银索取上限 |
| RHAH_Settings.requestMinHerbal | 全局 ModSettings，默认 3。草药索取下限 |
| RHAH_Settings.requestMaxHerbal | 全局 ModSettings，默认 15。草药索取上限 |
| RHAH_Settings.requestDays | 全局 ModSettings，默认 1，范围 0 到 30。接济和情报信的期限 |
| RHAH_Settings.requestPointScale | 全局 ModSettings，默认 300。索取量相对这个点数缩放 |
| RHAH_Settings.foodPerChild | 全局 ModSettings，默认 10。易子而食每个孩子的简单餐 |
| RHAH_Settings.foodPerVisitor | 全局 ModSettings，默认 1。投喂按人数乘的份数 |
| RHAH_Settings.maxFoodRequest | 全局 ModSettings，默认 12。一次投喂的份数上限 |
| RHAH_Settings.envoyMealCost | 全局 ModSettings，默认 6。使者成交的简单餐 |
| RHAH_Settings.campMinAdults | 全局 ModSettings，默认 2。安居点成人下限 |
| RHAH_Settings.campMaxAdults | 全局 ModSettings，默认 4。安居点成人上限 |
| RHAH_Settings.campMinChildren | 全局 ModSettings，默认 8。安居点幼年下限 |
| RHAH_Settings.campMaxChildren | 全局 ModSettings，默认 16。安居点幼年上限 |
| RHAH_Settings.campGoodwill | 全局 ModSettings，默认 12。清掉安居点的好感 |
| RHAH_Settings.campDays | 全局 ModSettings，默认 15。安居点天数 |
| RHAH_Settings.campHuts | 全局 ModSettings，默认 4。安居点棚屋 |
| RHAH_Settings.holeWoodCost | 全局 ModSettings，默认 20。封粮洞的木材 |
| RHAH_Settings.holeCleanPortions | 全局 ModSettings，默认 5。清粮洞取走的食物 |
| RHAH_Settings.holeLossRange | 全局 ModSettings，默认 12。粮洞取食距离 |
| RHAH_Settings.holeMaxLosses | 全局 ModSettings，默认 3。粮洞最多损失次数 |
| RHAH_Settings.apparelAwfulPercent | 全局 ModSettings，默认 35。破旧衣几率 |
| RHAH_Settings.apparelPoorPercent | 全局 ModSettings，默认 50。较差衣几率，与破旧合计不超过 100 |
| RHAH_Settings.apparelMinDurabilityPercent | 全局 ModSettings，默认 10。新衣耐久下限 |
| RHAH_Settings.apparelMaxDurabilityPercent | 全局 ModSettings，默认 60。新衣耐久上限 |
| RHAH_Settings.apparelCorpsePercent | 全局 ModSettings，默认 25。尸衣几率 |
| RHAH_Settings.apparelClothPercent | 全局 ModSettings，默认 65。布料相对人皮的几率 |
| RHAH_Settings.apparelMaxPieces | 全局 ModSettings，默认 4。成年来客最多衣物 |
| RHAH_Settings.begFailMood | 全局 ModSettings，默认 -5。乞讨被拒心情 |
| RHAH_Settings.begSuccessMood | 全局 ModSettings，默认 3。乞讨成功心情 |
| RHAH_Settings.begSlapMood | 全局 ModSettings，默认 -10。被抽耳光心情 |
| RHAH_Settings.begSlapKnockoutHours | 全局 ModSettings，默认 3。耳光击晕小时 |
| RHAH_Settings.begBruiseSeverity | 全局 ModSettings，默认 4。第一下瘀伤 |
| RHAH_Settings.begBruiseStep | 全局 ModSettings，默认 4。瘀伤加重 |
| RHAH_Settings.begBruiseMax | 全局 ModSettings，默认 16。瘀伤改割伤的上限 |
| RHAH_Settings.barkNutrition | 全局 ModSettings，默认 0.2。啃树皮营养 |
| RHAH_Settings.barkDamage | 全局 ModSettings，默认 2。啃树皮伤害 |
| RHAH_Settings.wallNutrition | 全局 ModSettings，默认 0.5。啃墙营养 |
| RHAH_Settings.wallDamage | 全局 ModSettings，默认 5。啃墙伤害 |
| RHAH_Settings.clayMaxBites | 全局 ModSettings，默认 3。观音土窗口口数 |
| RHAH_Settings.clayWindowDays | 全局 ModSettings，默认 15。观音土窗口天数 |
| RHAH_Settings.claySeverityPerBite | 全局 ModSettings，默认 0.33。每口饱腹 |
| RHAH_Settings.childHungryPercent | 全局 ModSettings，默认 30。母亲喂食的饥饿线 |
| RHAH_Settings.prisonerHungryPercent | 全局 ModSettings，默认 20。囚犯舔污和咬尾的饥饿线 |
| RHAH_Settings.tailBiteAge | 全局 ModSettings，默认 3。可咬的最大年龄 |
| RHAH_Settings.scavengeNutrition | 全局 ModSettings，默认 0.15。舔污营养 |
| RHAH_Settings.tailNutrition | 全局 ModSettings，默认 0.35。咬尾营养 |
| RHAH_Settings.tailFailDamage | 全局 ModSettings，默认 4。咬尾失败伤害 |
| RHAH_Settings.satisfiedFoodPercent | 全局 ModSettings，默认 82。吃饱线 |
| RHAH_Settings.refeedMalnutrition | 全局 ModSettings，默认 0.4。再吃的营养不良 |
| RHAH_Settings.plagueSeverityMax | 全局 ModSettings，默认 0.1。新感染严重度上限 |
| RHAH_Settings.followPredatorPercent | 全局 ModSettings，默认 10。短链捕食几率 |
| RHAH_Settings.followBirthWatchDays | 全局 ModSettings，默认 3。出生观察天数 |
| RHAH_Settings.followPlagueBirthDays | 全局 ModSettings，默认 5。鼠疫出生观察天数 |
| RHAH_Settings.followMotherReturnDays | 全局 ModSettings，默认 15。鼠疫母亲回归天数 |
| RHAH_Settings.followLongReturnYears | 全局 ModSettings，默认 2。长链额外年数 |
| RHAH_Settings.followAdultAge | 全局 ModSettings，默认 14。后续心情和长链年龄 |
| RHAH_Settings.followMoodScalePercent | 全局 ModSettings，默认 100。后续心情缩放 |
| RHAH_Settings.endingTrustFloor | 全局 ModSettings，默认 50。丰年信任 |
| RHAH_Settings.endingHopeTrust | 全局 ModSettings，默认 75。希望之城信任 |
| RHAH_Settings.endingHaltTrust | 全局 ModSettings，默认 -75。崩盘信任 |
| RHAH_Settings.endingLowKinds | 全局 ModSettings，默认 6。低结局需要的成功种类 |
| RHAH_Settings.endingThreatDays | 全局 ModSettings，默认 13。大型威胁基准天数。当前威胁补丁仍读叙事者自己的间隔 |
| RHAH_Settings.trustKill | 全局 ModSettings，默认 -10。放逐回访杀死 |
| RHAH_Settings.trustCaptive | 全局 ModSettings，默认 -5。俘虏，按人记 |
| RHAH_Settings.trustEntrustGood | 全局 ModSettings，默认 5。托孤安顿 |
| RHAH_Settings.trustEntrustCaptive | 全局 ModSettings，默认 -5。托孤被俘 |
| RHAH_Settings.trustEntrustStory | 全局 ModSettings，默认 -2。托孤只听故事 |
| RHAH_Settings.trustEntrustRegret | 全局 ModSettings，默认 -3。托孤后悔 |
| RHAH_Settings.trustEntrustBanished | 全局 ModSettings，默认 -1。托孤放逐 |
| RHAH_Settings.trustExchange | 全局 ModSettings，默认 3。交换照料完成 |
| RHAH_Settings.trustHoleOpen | 全局 ModSettings，默认 1。封上粮洞 |
| RHAH_Settings.trustHoleIgnore | 全局 ModSettings，默认 -1。清掉粮洞 |
| RHAH_Settings.trustHoleBait | 全局 ModSettings，默认 3。粮洞追踪 |
| RHAH_Settings.trustQuarantineStay | 全局 ModSettings，默认 1。检疫后留下 |
| RHAH_Settings.trustQuarantineRecover | 全局 ModSettings，默认 2。检疫后离开 |
| RHAH_Settings.trustQuarantineFail | 全局 ModSettings，默认 -2。检疫破裂 |
| RHAH_Settings.trustEnvoyFail | 全局 ModSettings，默认 -2。赶走使者 |
| RHAH_Settings.trustRelicFail | 全局 ModSettings，默认 -2。毁掉记录 |
| RHAH_Settings.trustHold | 全局 ModSettings，默认 -1。扣下，按人记 |
| RHAH_Settings.trustDeliver | 全局 ModSettings，默认 1。交付，按人记 |
| RHAH_Settings.trustLeave | 全局 ModSettings，默认 1。安全离开，按人记 |
| RHAH_Settings.trustExpel | 全局 ModSettings，默认 -1。驱逐，按人记 |
| RHAH_Settings.narrativeRewardSilver | 全局 ModSettings，默认 300。记录来信白银 |
| RHAH_Settings.narrativeRescueCost | 全局 ModSettings，默认 250。放逐赎回 |
| RHAH_Settings.narrativeRescueReward | 全局 ModSettings，默认 2500。赎回后的空投 |
| RHAH_Settings.narrativeRelicTake | 全局 ModSettings，默认 200。取走记录 |
| RHAH_Settings.narrativeRelicHand | 全局 ModSettings，默认 100。交出记录 |
| RHAH_Settings.narrativeTrustBonusPercent | 全局 ModSettings，默认 25。信任满值时的银两加成 |
| RHAH_Settings.narrativeCareDays | 全局 ModSettings，默认 5。安置天数 |
| RHAH_Settings.narrativeMissingDays | 全局 ModSettings，默认 1。失踪天数 |
| RHAH_Settings.narrativeObserveDays | 全局 ModSettings，默认 30。救济观察上限 |
| RHAH_Settings.narrativeHoleIgnoreDays | 全局 ModSettings，默认 3。粮洞忽视天数 |
| RHAH_Settings.narrativeEnvoyWaitDays | 全局 ModSettings，默认 3。使者等待 |
| RHAH_Settings.narrativeEnvoyCheckDays | 全局 ModSettings，默认 1。使者核对 |
| RHAH_Settings.narrativeRelicDays | 全局 ModSettings，默认 15。记录地点开放天数 |
| RHAH_Settings.narrativeReturnDays | 全局 ModSettings，默认 15。康复回访 |
| RHAH_Settings.narrativeRevisitYears | 全局 ModSettings，默认 4。放逐回访年数 |
| RHAH_Settings.narrativeAsideCooldownDays | 全局 ModSettings，默认 3。插话间隔 |
| RHAH_Settings.narrativeAsideCutoff | 全局 ModSettings，默认 -75。插话关闭的信任 |
| RHAH_Settings.narrativeAdultYears | 全局 ModSettings，默认 18。使者年龄 |
| RHAH_Settings.narrativeTheftKinds | 全局 ModSettings，默认 2。粮洞需要的偷窃种类 |
| RHAH_Settings.narrativeProgressKinds | 全局 ModSettings，默认 3。记录来信种类 |
| RHAH_Settings.narrativeRewardKinds | 全局 ModSettings，默认 8。白银来信种类 |
| RHAH_Settings.narrativeEnvoyKinds | 全局 ModSettings，默认 5。使者种类 |
| RHAH_Settings.narrativeRelicKinds | 全局 ModSettings，默认 8。记录地点种类 |
| HungerAndHavoc.Guard.* | Guard 始终加载，无存档类型 |
| RHAH_Mod / HarmonyBootstrap / RHAH_Runtime | 运行时入口，无 ExposeData |

## 卸载边界

- 按本页识别所有权，不删除其它模组的特质、种族、基因或 Hediff
- 原档不覆盖；失败则中止，不把半成品当成可卸载副本
- 未知本模组 Def 或类型引用必须中止，不猜测删除其所属节点
- 清理交叉引用时保持 Scribe 字典 keys/values 同步
- 全局 ModSettings 不改；卸模组后设置文件可残留
