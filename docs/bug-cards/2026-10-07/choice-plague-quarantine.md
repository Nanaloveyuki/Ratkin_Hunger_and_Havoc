# ChoiceLetter 鼠疫处置不应强制检疫

| 字段 | 内容 |
| --- | --- |
| 复现 | 生成携带鼠疫或进入地图检疫名单的 RHAH 来客，打开访客 ChoiceLetter，尝试加入、雇佣、送往盟友、拘捕或奴役；当前部分选项显示检疫禁用，拘捕和奴役在执行层失败 |
| 期望 / 实际 | 期望玩家可以直接收留、雇佣、转移、拘捕或奴役符合各自行为闸门的来客，疾病仍留在 Pawn 上并继续按现有规则传播与记录；实际检疫名单额外阻止玩家处置，且选项显示层与拘捕/奴役执行层不一致 |
| 版本 | 1.0.0，未正式发布 |
| 级别 | S2 |
| 根因 | 检疫观察名单被错误复用为玩家处置门禁：`RHAH_ApiHost` 在 `JoinColony`、`Hire`、`Transfer` 后强制拒绝，`ChoiceLetter_RHAH_Visitors` 对加入、雇佣、招募和盟友选项显示检疫禁用，`RHAH_VisitorBatch.CanConvert` 又无条件拒绝拘捕和奴役 |
| 最小修复 | 移除 ChoiceLetter 和访客转换路径对检疫名单的处置限制；移除 API 对 `JoinColony`、`Hire`、`Transfer` 的检疫强制拒绝，保留各自行为闸门、访客身份、本图、存活、精神状态、玩家派系、既有囚犯/奴隶和 Ideology 条件 |
| 明确不做 | 不删除鼠疫 Hediff、感染与传播；不删除地图观察名单、康复/死亡统计、回访安排或 N-007 叙事选择信；不改变鼠疫生成权重、严重度、存档类型名或事件显示 ID；不改原版疾病、交易或非 RHAH 来客 |
| Language | 删除不再生效的 `RHAH_Settings_PlagueQuarantine`、`RHAH_Settings_PlagueQuarantine_Tooltip`、`RHAH_Choice_Quarantine` 和文案中“检疫中不能转换”的描述；同步 ChineseSimplified、English、Japanese Keyed |
| 存档 | 不新增字段；删除设置 `plagueQuarantineBlocksJoin` 的读取与 UI，旧设置值忽略，不改变游戏存档键、ChoiceLetter 类型或叙事存档字段 |
| 归属表 | 删除已废弃的全局设置 `plagueQuarantineBlocksJoin` 登记；游戏存档归属不变 |
| 回归 | 检疫名单成员在行为闸门允许时可 `CanConvert`、Capture、Enslave；疾病仍存在；Join/Hire/Transfer 不再被检疫名单拒绝；精神状态、Imprison 闸门、身份、地图和玩家派系限制仍拒绝；ChoiceLetter 所有动作按点击时实时资格结算 |
| 后续债 | 未在本卡处理 Unity 信件排版和 N-007 剧情文本是否继续使用“检疫”一词；若玩法设计最终删除 N-007 故事线，另开事件与叙事重构任务 |

审查另确认 `I-032`、`I-046` 空投正文允许接纳，但 `ShowsJoin` 漏列二者。本次补齐永久收留选项，仍使用原有 JoinColony 闸门。无需新增 Language 或存档字段。

拘捕资格回归使用真实 Pawn/API 状态断言，不将未初始化原版运行时产生的异常当作转换成功。完整囚犯、奴隶和麻醉流程需游戏验证。
