# 远行队劫掠没有袭击通知

| 字段 | 内容 |
| --- | --- |
| 复现 | 在 v1.0.6 旧档或新游戏派出玩家远行队，触发 I-035 商队劫掠或 I-050 鼠疫商队劫掠；另在开发者事件页使用两事件的“立刻生成到当前地图” |
| 期望 / 实际 | 成功建立袭击遭遇后立即收到威胁信，可定位实际袭击者和遭遇地图 / 地图和袭击 Lord 已建立，但没有袭击通知，玩家可能直到战斗结束才发现 |
| 版本 | 玩家反馈为 ODF 更新前 v1.0.6；修复基于当前 ODF 事件链实现 |
| 级别 | S2 |
| 根因 | `TradeEventRouter.TrySpawnCaravanAmbush` 的成功尾端仅调用 ODF `NoteStarted` 后返回。原版 `SetupCaravanAttackMap(..., true)` 的第三参仅发送亲属相遇信，不发送袭击信；原版 `IncidentWorker_Ambush.DoExecute` 在生成地图、入场和建 Lord 后另发事件信，并对新生成的敌对地图调用 `Notify_GeneratedPotentiallyHostileMap` |
| 最小修复 | 在两条入场路径共用的成功尾端发送原版 `ThreatBig` 威胁信，复用当前事件 `LabelKey`，正文新增一个适用于远行队和调试地图的袭击键；`lookTargets` 使用已经入场的实际袭击者，不引用已经入图的远行队世界物体。远行队生成遭遇地图后补原版敌对地图暂停通知；所有失败提前返回仍不发袭击信，ODF `NoteStarted` 保留 |
| 明确不做 | 不改事件频率、点数、人数、双向敌对锁定、攻击/偷窃/绑架 Lord 参数、鼠疫和检疫、访客/广播/卸载路径；不增加索取选择、自动跳镜头、重试或存档状态 |
| Language | 复用 `RHAH_Incident_CaravanMuggers_Label` / `RHAH_Incident_PlagueCaravanMuggers_Label`；新增 `RHAH_CaravanAmbush_LetterText`，中英同步，并按现有日语键对称约定补日语。既有接近通知承诺还在路上，访客/偷窃正文不声明正在战斗，均不能准确替代袭击正文 |
| 存档 | 无新增/改名字段、存档键或类型；信件为原版 StandardLetter，不增加本模组持久化类型；既有 ODF 开始记录不改 |
| 归属表 | 否；无新本模组存档所有物 |
| 回归 | 永久回归 `RHAH_AmbushLetterTests.AmbushLetterTargetsAttackersOnTheirActualMap`：真实 LetterMaker 构造 ThreatBig 信，验证地图 A 为家园、地图 B 为遭遇时，目标仍为 B 的实际攻击者，信不误定位家园或远行队。真实游戏烟测步骤见下文；不加翻译文本、源码 wiring 或 mock echo 测试 |
| 后续债 | 无；未把开发者地图烟雾算作完整远行队路径，实际执行覆盖见下文 |

## 原版证据

- `/mnt/d/References/Rimworld/Vanilla/Game/Managed/Assembly-CSharp/RimWorld/Planet/CaravanIncidentUtility.cs`：40–57 行，建立地图、入场；53–55 行只处理亲属通知。
- 同目录 `RimWorld/IncidentWorker_Ambush.cs`：64–95 行，建 Lord 后发标准事件信；新生成地图调用敌对地图通知。
- 同目录 `Verse/LetterStack.cs`：48–51、75–91 行，原版威胁信处理声音、自动暂停偏好、降速、存档与归档，不需要本模组再造通知状态。

## 可执行烟雾验证

1. 加载包含当前必需 ODF 的游戏，派出有可行动成员的玩家远行队，分别触发 I-035/I-050。地图和袭击 Lord 建好后应出现一封对应事件标题的红色袭击信；新遭遇地图暂停，正文说明袭击、抢夺与绑架风险。
2. 在家园仍为当前地图时悬停该信，目标箭头应指向遭遇地图上的本批袭击者；点击定位应跳到实际遭遇地图而非家园或已经入图的远行队图标。解暂停后确认攻击、偷窃和绑架行为未改。
3. 分别对两事件使用“立刻生成到当前地图”；成功入场并建 Lord 后应收到同类威胁信，定位本图实际袭击者，不创建远行队伏击地图。I-050 沿用现有携病与检疫开关。
4. 在无玩家远行队时通过正常远行队目标触发，或用无法生成鼠族的配置使生成失败；应无袭击成功信、无新增 ODF 开始实例。已有亲属通知不是袭击成功回执。
5. 在 ODF 调试面板核对每次成功仍有 I-035/I-050 的一次 `NoteStarted` 实例；保存重读后通知不由后续链重发。中英及日语分别确认正文无裸键，标题沿用事件名。

## 文档归并

集成者更新 `docs/incidents.md` 的 I-035/I-050 袭击例外及即时刷新说明：成功建立袭击后发威胁信，定位实际袭击者；远行队新遭遇沿用原版敌对地图暂停通知。`pawn.md`、`save-ownership.md`、`unload-export.md` 无本切片迫使的更新。

## 已执行验证

- 在真实 RimWorld 1.6.4871 中执行 I-050 开发者地图入场，收到 `ThreatBig` 袭击信；原版袭击成员实际运行 `AttackMelee`
- 原版创建玩家远行队，移到可生成遭遇的格后执行 I-035 正常远行队伏击；生成地图 1，游戏暂停，新增一封 `ThreatBig` 信，7 个目标均在实际遭遇地图 1
- 当前格不能生成遭遇时返回 false，未创建成功通知；成功信仍由实际入场和 Lord 成功尾端发送
- 永久 LetterMaker 目标回归及其它最终 Release 回归共 552 项通过；中英日语言检查通过。没有将所有语言实际界面及长期战斗结局计为已测
