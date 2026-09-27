# 事件目录

显示 ID 是目录数据，不是列表下标。`I-001`..`I-051` 都有 IncidentDef 和 Worker。`baseChance` 为 0，不进入原版类别池。`RHAH_IncidentSchedule` 在讲述者每次检查时按正负两个平均天数抽池，再用 `RHAH_IncidentWeight` 选一条，交给现有 `QueueIncident`。入队保存当时的目标：地图用正数 `uniqueID`，商队用负数 `-ID`，0 表示未指定。执行只解析保存的目标，不再重新随机。目标暂时不可用时留在队列并继续尝试后面的事件；目标已销毁、目录或 Worker 缺失、Worker 确定失败时只丢掉这一项。旧档没有目标列表时按 0 补齐，仍走未指定的旧解析。频率页的线按这个平均天数画出接下来每一天至少发生一次的概率，横轴是第几天。显示天数只改图的宽度，范围 5 到 60，不进设置。开发者事件页也只入这条队列，不在菜单里 `TryExecute`，不掷权重。IrisMenus 强制暂停，游戏 tick 停着。排队后由 `GameComponentUpdate` 在暂停窗口都关掉的下一帧执行。

权重是 `family × season × plague × trust × target × 玩家生成权重 / 100`。族系数默认 Wild 1.4、Beggar 1、Thief 0.7、Trade 0.5、Siege 0.35、Aid 0.25、Special 0.2、Intel 0.12，可在频率页改，范围 0 到 5，0 不再抽这一类。春冬默认 ×1.1，鼠疫默认 ×0.5，同样可改。只有负池吃信任：`1 - clamp(trust, -100, 100) / 400`。正池不吃。玩家生成权重默认 100，范围 0 到 100，0 不抽。事件页关闭的条目同样为 0，调试触发也不接受。调试点写入 `IncidentParms.points`，默认取目录值，范围 1 到 10000。自然触发和调试触发都用这个点数决定人数；接济和情报的索取量按点数相对 300 缩放，仍夹在原上下限内。单人事件、交易商队和难民营人数不随点数变化。平均天数默认 15，范围 0 到 60，0 关闭该池。频率页用滑块和数字框分别设置正池、负池，并各画一条横线。横线是该池在接下来每一天至少发生一次的概率，不画单条事件的份额。显示天数范围 5 到 60，只改横轴长度。鼠标停在线上时显示这一天的概率。定居未满 1 天不抽。
接济和情报在生成后来信。信按选择类型写：接济和情报写出事件名、数量和物资名，婴儿不写份数，其余选择不套用索要模板。交付消耗对应物资；情报交付后再放原版 `ItemStash`、`Outpost` 或 `BanditCamp`。库存不足不结算。拒绝和一天超时按到达态度处理：敌对转敌对，偏敌对只离开，偏友好和友善离开，中立离开。玩家打伤时敌对和偏敌对转敌对，友善和偏友好离开，中立不改。投喂让最年长的在场来客等待，期限用 `noFoodWaitDays`，默认半天；到期或收到离开令就回到寻食，不再停在等待。忽视只关信。同一批次读档后不会再开第二封未结算的信。`I-051` 生成任务和临时安居点。每个安居点地图独立掷捕食概率，默认 10%。没有本地捕食者时生成外边的捕食者。关闭遵循难度时，外来捕食者饥饿且没有可接近的鼠族或尸体就离图。居住区内的鼠族尸体不可吃，已经加入玩家的鼠族不是强制目标。击倒和俘虏不算任务完成。
救济成功不在交付时计数。交付或接受只把这一批标成已答应。还活着的人后来全部离开原地图，或在原地入籍、吃饱、没病地住满五天，这一批才算一次。冻死、热死、饿死、病死、被其它派系或动物杀死、老死的人退出判定，不扣信任。没有活人且没有玩家直接杀死时，这一批关闭，不计成功。玩家派系的枪、近战、陷阱或其它外部暴力杀死任何一人，这一批立刻失败，信任 −10，每批一次。失败后其余人安置也不补记。监狱、奴役、拒绝、超时、驱逐不算成功。情报交付不算救济。结局只在穗音下触发，每档一个，标记是 `endingE01` 到 `endingE05`。规则见 [adr/0004-relief-and-endings.md](adr/0004-relief-and-endings.md)。
商队 `I-012` 与 `I-038` 都按目录人数生成，默认 3 人，并进入访客 Lord。`I-038` 仍携带鼠疫。两个开关默认都开。`traderIgnoresHarshEnvironment` 关闭后，太冷、太热或危险天气让访客 Lord 离场。`traderIgnoresEnclosedSpace` 关闭后，到不了地图边缘时他们离场。他们不使用原版交易 Lord。`I-013` 易子而食在 `childExchangeFoodSubstitution` 开启时允许玩家用简单餐代替婴幼儿，每个孩子 10 份。关闭后只能按原交易交出孩子。商人不卖食物。NPC 一方仍按原交易交出孩子。Lead Your Pet 不接入食物替代。亲子且不是出售囚犯时可以牵母亲绳。
来客选择里的投喂让最年长的在场来客原地等待，等待期限和断粮相同。殖民者右键这个来客时，只列出 `disabledGiveFoodDefNames` 没关掉、而且至少是糟糕食物的库存。名单为空时这些食物都可以交。这张名单不看来客自己能不能在赈灾区取食。
短工和长工只出现在可整批处理的来客信上，期限看设置。短工最短 5 天、最长 1 年，默认 5 天。长工最短 5 天、最长 10 年，默认 1 年。一年按 60 天。到期离开，倒地暂停。右键单只来客仍可收留为短工或雇佣为长工。
到达态度按事件单独保存，五档是敌对、偏敌对、中立、偏友好、友善。缺省用目录态度：`I-006`、`I-007`、`I-014`、`I-030`、`I-043`、`I-045` 敌对，`I-034` 与 `I-048` 偏敌对，`I-008` 到 `I-010`、`I-012` 和其余负池续作中立，其余原作与正池续作偏友好。当前叙事者是穗音且信任小于 0 时，生成再降一档，敌对保持敌对。其它叙事者不降。已经在场的来客不改写。

拒绝、一天超时和驱逐使用到达后的态度。敌对转敌对。偏敌对只离开。偏友好和友善离开。中立离开，不转敌对。玩家派系的外部暴力打伤时，敌对和偏敌对转敌对，偏友好和友善离开，中立不改。`batchTurnsHostile` 或 `batchLeavesTogether` 关闭后，对应的一半不执行。

`I-037` 的托孤和 `I-013` 交付后的交换照料只在当前叙事者是穗音时打开。其它叙事者仍结算选择，但不记这两条后续。

`Source/EventMgr` 管理可复用的事件链，不抽 `I-001`..`I-051`，也不推进 `N-`。目前没有事件注册链。每条事件在设置里有短链和长链，默认短链。开发者页可以改，但现在两种都只生成这一次。长链后续还没接。站点号沿用队列目标：地图是正数 `uniqueID`，商队是负数 `-ID`，0 是世界格。以后有事件注册后，生成成功才记一条开始实例。每 tick 只推进已到期的实例，到期原因是 `Expired`。新链仍在讲述者 1000 tick 检查点按 `CanStart` 抽取。阶段、出现物和主动结束由事件自己调用，管理器不决定玩法。实例写在 `GameComponent_RHAH_Game.eventChains`。

| 显示 ID | defName | 名称 | 旧 ID | Family | Origin | Category | Target | 设计 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| I-001 | RHAH_LargeRefugeeWave | 大批逃难者 | O-001 | Beggar | Original | Hunger | Map | [短](events/I-001.md) |
| I-002 | RHAH_AbandonedRatkinChildren | 幼年鼠族遗弃 | O-002 | Special | Original | Hunger | Map | [链](events/I-002-01.md) |
| I-003 | RHAH_ShatteredMother | 耗子分妈 | O-003 | Special | Original | Hunger | Map | [触发](events/I-003.md) |
| I-004 | RHAH_BeggarFamily | 乞讨的饥荒鼠族母亲 | O-004 | Beggar | Original | Hunger | Map | [触发](events/I-004.md) |
| I-005 | RHAH_BeggarGroup | 乞讨的鼠族队伍 | O-005 | Beggar | Original | Hunger | Map | [短](events/I-005.md) |
| I-006 | RHAH_ThiefRatkinGroup | 偷窃的鼠族 | O-006 | Thief | Original | Hunger | Map | [触发](events/I-006.md) |
| I-007 | RHAH_ThiefRatkinChildGroup | 偷窃的幼年鼠族 | O-007 | Thief | Original | Hunger | Map | [触发](events/I-007.md) |
| I-008 | RHAH_WildRatkinWandersIn | 游荡的野生鼠族 | O-008 | Wild | Original | Hunger | Map | [短](events/I-008.md) |
| I-009 | RHAH_WildRatkinChildWandersIn | 游荡的野生幼年鼠族 | O-009 | Wild | Original | Hunger | Map | [短](events/I-009.md) |
| I-010 | RHAH_WildRatkinGroupWandersIn | 游荡的野生鼠群 | O-010 | Wild | Original | Hunger | Map | [短](events/I-010.md) |
| I-011 | RHAH_FamineRefugees | 灾荒逃难者 | O-011 | Beggar | Original | Hunger | Map | [短](events/I-011.md) |
| I-012 | RHAH_RatkinTraderCaravan | 流民商队 | O-012 | Trade | Original | Hunger | Map | [链](events/I-012-01.md) |
| I-013 | RHAH_ChildExchange | 易子而食 | O-013 | Special | Original | Hunger | Map | [链](events/I-013-01.md) |
| I-014 | RHAH_BeggarSiege | 乞食围攻 | O-014 | Siege | Original | Hunger | Map | [短](events/I-014.md) |
| I-015 | RHAH_AidSimpleMeal | 难民接济（简单餐） | N-011 | Aid | Sequel | Hunger | Map | [触发](events/I-015.md) |
| I-016 | RHAH_AidFineMeal | 难民接济（精致餐） | N-012 | Aid | Sequel | Hunger | Map | [触发](events/I-016.md) |
| I-017 | RHAH_AidMedicine | 难民接济（药品） | N-013 | Aid | Sequel | Hunger | Map | [触发](events/I-017.md) |
| I-018 | RHAH_AidSilver | 难民接济（白银） | N-014 | Aid | Sequel | Hunger | Map | [触发](events/I-018.md) |
| I-019 | RHAH_AidBaby | 难民接济（婴儿） | N-015 | Aid | Sequel | Hunger | Map | [链](events/I-019-01.md) |
| I-020 | RHAH_IntelTreasureSimpleMeal | 情报（藏货，简单餐） | N-016 | Intel | Sequel | Hunger | Map | [短](events/I-020.md) |
| I-021 | RHAH_IntelTreasureHerbal | 情报（藏货，草药） | N-017 | Intel | Sequel | Hunger | Map | [短](events/I-021.md) |
| I-022 | RHAH_IntelTreasureSilver | 情报（藏货，白银） | N-018 | Intel | Sequel | Hunger | Map | [短](events/I-022.md) |
| I-023 | RHAH_IntelStructureSimpleMeal | 情报（建筑，简单餐） | N-019 | Intel | Sequel | Hunger | Map | [短](events/I-023.md) |
| I-024 | RHAH_IntelStructureHerbal | 情报（建筑，草药） | N-020 | Intel | Sequel | Hunger | Map | [短](events/I-024.md) |
| I-025 | RHAH_IntelStructureSilver | 情报（建筑，白银） | N-021 | Intel | Sequel | Hunger | Map | [短](events/I-025.md) |
| I-026 | RHAH_IntelSettlementSimpleMeal | 情报（据点，简单餐） | N-022 | Intel | Sequel | Hunger | Map | [短](events/I-026.md) |
| I-027 | RHAH_IntelSettlementHerbal | 情报（据点，草药） | N-023 | Intel | Sequel | Hunger | Map | [短](events/I-027.md) |
| I-028 | RHAH_IntelSettlementSilver | 情报（据点，白银） | N-024 | Intel | Sequel | Hunger | Map | [短](events/I-028.md) |
| I-029 | RHAH_LaboringRefugees | 待产流民 | N-025 | Beggar | Sequel | Hunger | Map | [触发](events/I-029.md) |
| I-030 | RHAH_StrongSiege | 强势围攻 | N-026 | Siege | Sequel | Hunger | Map | [短](events/I-030.md) |
| I-031 | RHAH_Passersby | 路过的鼠族 | N-027 | Beggar | Sequel | Hunger | Map | [短](events/I-031.md) |
| I-032 | RHAH_AirdropMistake | 空投失误 | N-028 | Special | Sequel | Hunger | Map | [链](events/I-032-01.md) |
| I-033 | RHAH_MisguidedKinship | 认错亲人 | N-029 | Special | Sequel | Hunger | Map | [链](events/I-033-01.md) |
| I-034 | RHAH_GreatFamine | 大灾荒 | N-030 | Thief | Sequel | Hunger | Map | [短](events/I-034.md) |
| I-035 | RHAH_CaravanMuggers | 商队劫掠 | N-031 | Thief | Sequel | Hunger | Caravan | [短](events/I-035.md) |
| I-036 | RHAH_PlagueWanderers | 鼠疫游荡者 | N-032 | Wild | Sequel | Plague | Map | [短](events/I-036.md) |
| I-037 | RHAH_PlagueAbandonedBabies | 鼠疫遗弃幼年鼠族 | N-033 | Special | Sequel | Plague | Map | [链](events/I-037-01.md) |
| I-038 | RHAH_PlagueTraderCaravan | 鼠疫商队 | N-034 | Trade | Sequel | Plague | Map | [触发](events/I-038.md) |
| I-039 | RHAH_PlaguePassersby | 鼠疫路过 | N-035 | Beggar | Sequel | Plague | Map | [短](events/I-039.md) |
| I-040 | RHAH_PlagueRefugees | 鼠疫逃难者 | N-036 | Beggar | Sequel | Plague | Map | [短](events/I-040.md) |
| I-041 | RHAH_PlagueOrphan | 鼠疫孤儿 | N-037 | Wild | Sequel | Plague | Map | [链](events/I-041-01.md) |
| I-042 | RHAH_PlagueBeggarGroup | 鼠疫乞讨队伍 | N-038 | Beggar | Sequel | Plague | Map | [短](events/I-042.md) |
| I-043 | RHAH_PlagueThiefGroup | 鼠疫偷窃队伍 | N-039 | Thief | Sequel | Plague | Map | [短](events/I-043.md) |
| I-044 | RHAH_PlagueLaboringRefugees | 鼠疫待产流民 | N-040 | Beggar | Sequel | Plague | Map | [触发](events/I-044.md) |
| I-045 | RHAH_PlagueStrongSiege | 鼠疫强势围攻 | N-041 | Siege | Sequel | Plague | Map | [短](events/I-045.md) |
| I-046 | RHAH_PlagueAirdropMistake | 鼠疫空投失误 | N-042 | Special | Sequel | Plague | Map | [触发](events/I-046.md) |
| I-047 | RHAH_PlagueMisguidedKinship | 鼠疫认错亲人 | N-043 | Special | Sequel | Plague | Map | [触发](events/I-047.md) |
| I-048 | RHAH_PlagueGreatFamine | 鼠疫大灾荒 | N-044 | Thief | Sequel | Plague | Map | [短](events/I-048.md) |
| I-049 | RHAH_PlagueRevenge | 鼠疫报复 | N-045 | Special | Sequel | Plague | Map | [短](events/I-049.md) |
| I-050 | RHAH_PlagueCaravanMuggers | 鼠疫商队劫掠 | N-046 | Thief | Sequel | Plague | Caravan | [短](events/I-050.md) |
| I-051 | RHAH_RefugeeMassacre | 危险的鼠族安居点 | N-047 | Special | Sequel | Hunger | Map | [短](events/I-051.md) |
