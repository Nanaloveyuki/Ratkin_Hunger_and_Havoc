# 事件目录

显示 ID 是目录数据，不是列表下标。`I-001`..`I-051` 都有 IncidentDef 和 Worker。`baseChance` 为 0，不进入原版类别池。`HungerIncidentSchedule` 在讲述者每次检查时按正负两个平均天数抽池，再用 `HungerIncidentWeight` 选一条，交给现有 `QueueIncident`。调试排队仍走 `HungerAndHavocScheduler.QueueDebugIncident`，不掷权重。地图事件只打玩家家园，`I-035` 与 `I-050` 只打玩家商队。

权重是 `family × season × plague × trust × target`。族权重：Wild 1.4、Beggar 1、Thief 0.7、Trade 0.5、Siege 0.35、Aid 0.25、Special 0.2、Intel 0.12。春冬 ×1.1，鼠疫 ×0.5。只有负池吃信任：`1 - clamp(trust, -100, 100) / 400`。正池不吃。平均天数默认 15，范围 0 到 60，0 关闭该池。定居未满 1 天不抽。

| 显示 ID | defName | 旧 ID | Family | Origin | Category | Target |
| --- | --- | --- | --- | --- | --- | --- |
| I-001 | RHAH_LargeRefugeeWave | O-001 | Beggar | Original | Hunger | Map |
| I-002 | RHAH_AbandonedRatkinChildren | O-002 | Special | Original | Hunger | Map |
| I-003 | RHAH_ShatteredMother | O-003 | Special | Original | Hunger | Map |
| I-004 | RHAH_BeggarFamily | O-004 | Beggar | Original | Hunger | Map |
| I-005 | RHAH_BeggarGroup | O-005 | Beggar | Original | Hunger | Map |
| I-006 | RHAH_ThiefRatkinGroup | O-006 | Thief | Original | Hunger | Map |
| I-007 | RHAH_ThiefRatkinChildGroup | O-007 | Thief | Original | Hunger | Map |
| I-008 | RHAH_WildRatkinWandersIn | O-008 | Wild | Original | Hunger | Map |
| I-009 | RHAH_WildRatkinChildWandersIn | O-009 | Wild | Original | Hunger | Map |
| I-010 | RHAH_WildRatkinGroupWandersIn | O-010 | Wild | Original | Hunger | Map |
| I-011 | RHAH_FamineRefugees | O-011 | Beggar | Original | Hunger | Map |
| I-012 | RHAH_RatkinTraderCaravan | O-012 | Trade | Original | Hunger | Map |
| I-013 | RHAH_ChildExchange | O-013 | Special | Original | Hunger | Map |
| I-014 | RHAH_BeggarSiege | O-014 | Siege | Original | Hunger | Map |
| I-015 | RHAH_AidSimpleMeal | N-011 | Aid | Sequel | Hunger | Map |
| I-016 | RHAH_AidFineMeal | N-012 | Aid | Sequel | Hunger | Map |
| I-017 | RHAH_AidMedicine | N-013 | Aid | Sequel | Hunger | Map |
| I-018 | RHAH_AidSilver | N-014 | Aid | Sequel | Hunger | Map |
| I-019 | RHAH_AidBaby | N-015 | Aid | Sequel | Hunger | Map |
| I-020 | RHAH_IntelTreasureSimpleMeal | N-016 | Intel | Sequel | Hunger | Map |
| I-021 | RHAH_IntelTreasureHerbal | N-017 | Intel | Sequel | Hunger | Map |
| I-022 | RHAH_IntelTreasureSilver | N-018 | Intel | Sequel | Hunger | Map |
| I-023 | RHAH_IntelStructureSimpleMeal | N-019 | Intel | Sequel | Hunger | Map |
| I-024 | RHAH_IntelStructureHerbal | N-020 | Intel | Sequel | Hunger | Map |
| I-025 | RHAH_IntelStructureSilver | N-021 | Intel | Sequel | Hunger | Map |
| I-026 | RHAH_IntelSettlementSimpleMeal | N-022 | Intel | Sequel | Hunger | Map |
| I-027 | RHAH_IntelSettlementHerbal | N-023 | Intel | Sequel | Hunger | Map |
| I-028 | RHAH_IntelSettlementSilver | N-024 | Intel | Sequel | Hunger | Map |
| I-029 | RHAH_LaboringRefugees | N-025 | Beggar | Sequel | Hunger | Map |
| I-030 | RHAH_StrongSiege | N-026 | Siege | Sequel | Hunger | Map |
| I-031 | RHAH_Passersby | N-027 | Beggar | Sequel | Hunger | Map |
| I-032 | RHAH_AirdropMistake | N-028 | Special | Sequel | Hunger | Map |
| I-033 | RHAH_MisguidedKinship | N-029 | Special | Sequel | Hunger | Map |
| I-034 | RHAH_GreatFamine | N-030 | Thief | Sequel | Hunger | Map |
| I-035 | RHAH_CaravanMuggers | N-031 | Thief | Sequel | Hunger | Caravan |
| I-036 | RHAH_PlagueWanderers | N-032 | Wild | Sequel | Plague | Map |
| I-037 | RHAH_PlagueAbandonedBabies | N-033 | Special | Sequel | Plague | Map |
| I-038 | RHAH_PlagueTraderCaravan | N-034 | Trade | Sequel | Plague | Map |
| I-039 | RHAH_PlaguePassersby | N-035 | Beggar | Sequel | Plague | Map |
| I-040 | RHAH_PlagueRefugees | N-036 | Beggar | Sequel | Plague | Map |
| I-041 | RHAH_PlagueOrphan | N-037 | Wild | Sequel | Plague | Map |
| I-042 | RHAH_PlagueBeggarGroup | N-038 | Beggar | Sequel | Plague | Map |
| I-043 | RHAH_PlagueThiefGroup | N-039 | Thief | Sequel | Plague | Map |
| I-044 | RHAH_PlagueLaboringRefugees | N-040 | Beggar | Sequel | Plague | Map |
| I-045 | RHAH_PlagueStrongSiege | N-041 | Siege | Sequel | Plague | Map |
| I-046 | RHAH_PlagueAirdropMistake | N-042 | Special | Sequel | Plague | Map |
| I-047 | RHAH_PlagueMisguidedKinship | N-043 | Special | Sequel | Plague | Map |
| I-048 | RHAH_PlagueGreatFamine | N-044 | Thief | Sequel | Plague | Map |
| I-049 | RHAH_PlagueRevenge | N-045 | Special | Sequel | Plague | Map |
| I-050 | RHAH_PlagueCaravanMuggers | N-046 | Thief | Sequel | Plague | Caravan |
| I-051 | RHAH_RefugeeMassacre | N-047 | Special | Sequel | Hunger | Map |
