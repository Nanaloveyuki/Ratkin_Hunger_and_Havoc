# 事件目录

显示 ID 是目录数据，不是列表下标。`I-001`..`I-051` 都有 IncidentDef 和 Worker。`baseChance` 为 0，不进入原版类别池。`RHAH_IncidentSchedule` 在讲述者每次检查时先求正负两池当天的平均间隔，再用 `Rand.MTBEventOccurs` 决定是否抽池，抽中后用 `RHAH_IncidentWeight` 选一条，交给现有 `QueueIncident`。间隔来自该池的算术式。默认为 `averageDays`，也就是原来的平均天数。式子可读 `day`、`averageDays`、`trust`、`season`，可用 `+ - * /`、括号、`min`、`max`、`abs`、`clamp`。`day` 是定居后天数，`season` 为 0 到 3。空式、超长、语法错误、除以 0 或非有限结果都回到 `averageDays`。求出的天数仍夹在 0 到 60，0 关闭该池。失败不重试、不补抽。入队保存当时的目标：地图用正数 `uniqueID`，商队用负数 `-ID`，0 表示未指定。执行只解析保存的目标，不再重新随机。目标暂时不可用时留在队列并继续尝试后面的事件；目标已销毁、目录或 Worker 缺失、Worker 确定失败时只丢掉这一项。旧档没有目标列表时按 0 补齐，仍走未指定的旧解析。奥德赛空间层和生物群系 `OuterSpaceBiome` 的家园默认不抽地图事件，也不跑地图事件链；`spaceApproachEnabled` 打开后才列入。地图事件出队后不立刻生成 Pawn。`RHAH_Approach` 在目标家园外 4 到 6 个可走格生成一个没有 Pawn 的世界物体，平地每天 3 格，道路和地形仍按原版难度乘。家园在岛上、4 格内没有可走陆地时，改从对岸至少 4 格外的最近陆地出发，并允许穿过海洋。多个玩家家园时，已保存的地图还在就用它，否则在现有家园里随机一个。上述太空家园默认不参加这次选择。没有合格家园时不创建远行队，改为直接在地图生成。已经存在的太空远行队寻路失败时取消，不挡住穿梭机和运输舱选格。物体点击不挡住下层世界物体。

队伍到达后只有生成成功才完成事件。目标暂时不可用或 Worker 返回失败时，队伍保留，每游戏小时再试一次；到达阶段和剩余等待写入存档，读档后继续同一目标，不再重新走路。目标销毁、目录或 Worker 缺失、当前存档已停用新内容时取消队伍。Worker 抛异常可能已生成部分角色，记录错误并终止，不自动重跑以免重复生成。

玩家远行队与鼠族远行队在同一地表格时才显示拦截菜单。拦截在地图绘制初始化完成后直接生成敌对鼠族，不执行原事件 Worker，因此空投、索取、分娩和后续事件链不进入临时地图。人数沿用事件点数与人数上限；幼年成员至少四岁，保证能穿图。鼠族从地图最外缘入场，各自以 `TravelOrWait` 向可达的对边出口移动；进入出口 10 格半径后才切换离图 duty，出生边缘不能提前离场。倒地成员不阻挡其余成员离场。玩家原库存保留、入图后征召。成功后消费世界队伍，避免在家园再生成一次。

临时父物体 `RHAH_Interception` 使用 `Encounter`，只带重组远行队组件，没有原版战场判胜和袭击倒计时。最后一名活动鼠族离场、死亡或归入玩家后，本批穗音救济与完成判定保持否，信任减二，每批一次；死亡、捕获和离图不另算逐人信任。批次在既有 `journalCases` 中以 `id=0` 保存失败标记，终态写 `closed`，重读不重复扣除。事件不计入穗音经历种类，不开启 N 后续。已结束拦截地图可复用；玩家成员和入境运输器都不存在时，地图和父物体按原版规则移除。既有原版战场不迁移，旧倒计时继续由原版管理。

权重是 `family × season × plague × trust × target × 玩家生成权重 / 100`。族系数默认 Wild 1.4、Beggar 1、Thief 0.7、Trade 0.5、Siege 0.35、Aid 0.25、Special 0.2、Intel 0.12，可在频率页改，范围 0 到 5，0 不再抽这一类。春冬默认 ×1.1，鼠疫默认 ×0.5，同样可改。只有负池吃信任：`1 - clamp(trust, -100, 100) / 400`。正池不吃。玩家生成权重默认 100，范围 0 到 100，0 不抽。事件页关闭的条目同样为 0，调试触发也不接受。调试点写入 `IncidentParms.points`，默认取目录值，范围 1 到 10000。自然触发和调试触发都用这个点数决定人数；接济和情报的索取量按点数相对 `requestPointScale` 缩放，默认 300，仍夹在设置里的上下限内。单人事件、交易商队和难民营人数不随点数变化。平均天数默认 15，范围 0 到 60，0 关闭该池。两池各有一条间隔式，默认 `averageDays`。频率页用滑块和数字框设置平均天数，再用文本框改式子，并各画一条曲线。曲线是该池在接下来每一天至少发生一次的概率，不画单条事件的份额。显示天数范围 5 到 360，写入 `frequencyWindowDays`，只改横轴长度。鼠标停在线上时显示这一天的概率。定居未满 1 天不抽。
接济和情报在生成后来信。特殊事件按显示 ID 选专用正文，其余复用适合的选择类别；I-013 的换子正文先于通用婴儿请求。实际人数、物资量和名称传入翻译；幼年正文根据已生成角色年龄选择，I-033/I-047 不满一岁不用直接台词，满 14 岁不用鼠蛋错音。答复期限和拒绝风险放独立提示，鼠疫提示读实际开关。选择信和索取信的 `lookTargets` 是本批已生成的 pawn，悬停时原版画出指向箭头。交付消耗对应物资；情报交付后再放原版 `ItemStash`、`Outpost` 或 `BanditCamp`。库存不足不结算。拒绝和 `requestDays` 天超时按到达态度处理，默认 1 天：敌对转敌对，偏敌对只离开，偏友好和友善离开，中立离开。玩家打伤时敌对和偏敌对转敌对，友善和偏友好离开，中立不改。投喂让每名合格的在场来客等待自己的食物，期限用 `noFoodWaitDays`，默认半天；到期或收到离开令就回到寻食，不再停在等待。忽视只关信。同一批次读档后不会再开第二封未结算的信。`I-051` 生成任务和临时安居点，任务正文使用 Slate 中实际的配置人数范围、期限和好感，人数写预计，不承诺精确数量。每个安居点地图独立掷捕食概率，默认 10%。没有本地捕食者时生成外边的捕食者。关闭遵循难度时，外来捕食者饥饿且没有可接近的鼠族或尸体就离图。居住区内的鼠族尸体不可吃，已经加入玩家的鼠族不是强制目标。击倒和俘虏不算任务完成。
救济成功不在交付时计数。交付或接受只把这一批标成已答应。还活着的人后来全部离开原地图，或在原地入籍、吃饱、没病地住满五天，这一批才算一次。冻死、热死、饿死、病死、被其它派系或动物杀死、老死的人退出判定，不扣信任。没有活人且没有玩家直接杀死时，这一批关闭，不计成功。玩家派系的枪、近战、陷阱或其它外部暴力杀死任何一人，这一批立刻失败，信任 −10，每批一次。失败后其余人安置也不补记。监狱、奴役、拒绝、超时、驱逐不算成功。情报交付不算救济。结局只在穗音下触发，每档一个，标记是 `endingE01` 到 `endingE05`。规则见 [adr/0004-relief-and-endings.md](adr/0004-relief-and-endings.md)。
商队 `I-012` 与 `I-038` 都按目录人数生成，默认 3 人，并进入访客 Lord。`I-038` 仍携带鼠疫。两个开关默认都开。`traderIgnoresHarshEnvironment` 关闭后，太冷、太热或危险天气让访客 Lord 离场。`traderIgnoresEnclosedSpace` 关闭后，到不了地图边缘时他们离场。他们不使用原版交易 Lord。`I-013` 易子而食当前只能在 `childExchangeFoodSubstitution` 开启、`foodPerChild` 大于 0 且库存充足时交出简单餐，每个孩子默认 10 份。`ChoiceLetter_RHAH_Request` 的孩子交付分支不可用，关闭替代或餐数为 0 时无法成交；`I-019` 同样无法交付婴儿。I-013 交餐不自动让孩子加入殖民地。这些是当前交易实现的限制，本次文案回填没有补上交换机制。商人不卖食物。Lead Your Pet 不接入食物替代。亲子且不是出售囚犯时可以牵母亲绳。
来客选择里的投喂按人保存等待名单，每人索取 `foodPerVisitor` 份，不把整批份数堆给最年长者。殖民者右键具体来客时，进入同一条交食流程，不直接写成吃饱；只列出 `disabledGiveFoodDefNames` 没关掉、而且至少是糟糕食物的库存。名单为空时这些食物都可以交。这张名单不看来客自己能不能在赈灾区取食。只有该人实际进食达到阈值后才标成 Fed；一个人收齐不结束其它人的等待。
短工和长工只出现在可整批处理的来客信上，期限看设置。短工最短 5 天、最长 1 年，默认 5 天。长工最短 5 天、最长 10 年，默认 1 年。一年按 60 天。到期离开，倒地暂停。右键单只来客仍可收留为短工或雇佣为长工。
选中短工、长工或临时征募时显示剩余离场时间，倒地显示暂停，永久加入不显示。通讯台“广播希望”在殖民者完成实际广播作业后入队；广播回执只表示已发出，不表示鼠族已经入图。当前存档停用新内容后不能再广播。
到达态度按事件单独保存，五档是敌对、偏敌对、中立、偏友好、友善。缺省用目录态度：`I-006`、`I-007`、`I-014`、`I-030`、`I-043`、`I-045` 敌对，`I-034` 与 `I-048` 偏敌对，`I-008` 到 `I-010`、`I-012` 和其余负池续作中立，其余原作与正池续作偏友好。当前叙事者是穗音且信任小于 0 时，生成再降一档，敌对保持敌对。其它叙事者不降。已经在场的来客不改写。

`I-035`、`I-050` 是袭击例外：不使用可配置的中立到达态度或信任降档，到达与当前态度固定敌对。远行队与调试地图共享入场前的双向敌对锁定；调试已有地图另刷新原版攻击目标缓存。攻击 Lord 保留偷窃、绑架且不超时撤退，不由访客离场替换。

拒绝、一天超时和驱逐使用到达后的态度。敌对转敌对。偏敌对只离开。偏友好和友善离开。中立离开，不转敌对。玩家派系的外部暴力打伤时，敌对和偏敌对转敌对，偏友好和友善离开，中立不改。`batchTurnsHostile` 或 `batchLeavesTogether` 关闭后，对应的一半不执行。

访客信的攻击是独立的显式敌对动作，不复用拒绝；先改当前态度再问 Fight。驱逐只统计实际新离场人数，真实伤害才写 hurt，拒绝和超时不伪装主动驱逐。首个记录成员离图后仍从本图本批合格成员处理，不操作已加入、雇佣、囚奴或远处成员。

`I-013` 交付后的交换照料只在当前叙事者是穗音时打开。`I-037` 的托孤入口还要求批次里有成年母亲和孩子，但该事件目前只生成孩子，普通触发不能满足入口；文案中的母亲动作保留为托付背景，不新增现场角色。其它叙事者仍结算选择，但不记这两条后续。

`Source/EventMgr` 管理可复用的事件链，不抽 `I-001`..`I-051`，也不推进 `N-`。生成成功后记一条 `eventChains` 实例，payload 是显示 ID、批次和站点。短链是阶段 0：下一次到期检查按 `followPredatorPercent` 给这批人排一次野生捕食者，默认 10%，用安居点那条到达，不重复掷。长链是阶段 1，只在开发者页把 `I-002`、`I-012`、`I-013`、`I-019`、`I-032`、`I-033`、`I-037`、`I-041` 设为长链时打开。原批次当前在场的相关角色达到 `followAdultAge`（默认 14 岁）后发一封后续通知并结束，不生成回来探望的母亲、商人或认领者；正文不因达到可配置阈值就断言成年。触发心情按文档的年龄、救济、出生和特质条件写，不另开状态机。站点号沿用队列目标。到期原因仍是 `Expired`。新链仍在讲述者 1000 tick 检查点按 `CanStart` 抽取。阶段、出现物和主动结束由事件自己调用。

后续处理器按 `Owns` 接收存档里的 `I-xxx`，不改显示 ID。短链处理后进入阶段 2，不重复投掷；长链阶段保持 1，按实例 ID 错开每游戏小时一次检查，阶段不承载 tick。排队事件的点数传入实际 IncidentParms，商队直达与地图回退都保留该值。

物资请求足额检查后反向消费实际库存，跨完整堆不跳项，不足不部分扣费；情报先验证并构造未发布站点，扣费成功才发布站点与结算。站点失败留信留窗且不扣库存；关闭选择开关不阻止既有请求超时。检疫选择通知仍逐 tick 处理，人口观察仅在游戏整点执行，终态及推迟态不扫描。记录地点期限前保留；到期仅空置、未进图、无地图、无人且箱子未毁时移除。

使者核对后保留或恢复原案件选择信，Checking 期限读 `checkUntil`；实时核对原使者、原地图和期限，失效终态归还本事件持有的两项离场闸门。检疫信绑定地图与病例开始 tick；留驻同时持有 LeaveAfterFed、ExitMap，放行安排实际离场但不清疾病检疫名单。全死、失联宽限已过、康复加入均可结案；重叠持有最后一个解除时才恢复原覆盖。

粮洞封洞使用配置成本（含免费）及实际木材库存，Pending 到期无论洞是否仍在都关闭；诱饵洞毁结为 Gone，追踪站点失败保留原信，损失距离读配置。重逢信固定发信时相遇者、原 Tile、地图与目标持有商队；答复实时核对原玩家商队仍在相遇地点，不远程扣银或杀人，不以另一名孩子替代失效目标。重逢期限仍只限制首次相遇，不额外限制已发信答复。

开发者事件页每条事件保留“触发”，另有“即时刷新”。即时入口同步调用 Worker，不入队、不创建 `RHAH_Approach`，优先用当前地图，无当前地图时沿用地图解析。点数来自该事件配置，仍遵守全局新内容、仅此存档停用与事件开关。`I-035`、`I-050` 在选定地图边缘直接放置袭击者并建立袭击 Lord，不寻找玩家远行队或创建伏击地图；鼠疫携带者继续进入检疫。`I-051` 复用安居点居民生成与防守 Lord，直接在选定地图放置居民，不创建任务、站点或棚屋，也不改殖民地建筑。自然触发不变。暂停中连续生成采用地图内尚未登记的正数批次 ID，避免同 tick 的后一次刷新被当作重复批次拒绝。

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
