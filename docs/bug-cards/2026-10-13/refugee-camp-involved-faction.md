# 安居点任务关联派系缺少部件

| 字段 | 内容 |
| --- | --- |
| 复现 | 玩家在 1.0.6 存档触发正常 I-051；完整日志 `日志.txt:796–834` 显示 Quest250 的 sponsor、residents、site 均有效，`QuestNode_RHAH_RefugeeCamp.RunInt` 调用 `AddInvolvedFaction` 后抛出 `Sequence contains no matching element`，外层仍记录到达成功。不要求玩家重复复现 |
| 期望 / 实际 | 正常生成清除安居点任务与站点、奖励和超时部件；实际登记委托派系时中断，后续部件和正文期限、好感变量未生成 |
| 版本 | 玩家报告 1.0.6；当前工作区仍含相同调用 |
| 级别 | S2，I-051 任务生成受阻 |
| 根因 | 自定义 QuestNode 直接作为任务根，没有提前创建 `QuestPart_InvolvedFactions`；原版 `AddInvolvedFaction` 用 `First` 查该部件，无匹配时在其 `?? new` 回退前抛异常。原版 `QuestNode.Run` 捕获错误，不能依靠外层返回非 null 判断节点生成完整 |
| 最小修复 | `Source/Incidents/RHAH_RefugeeCamp.cs` 使用原版 `GetFirstOrAddPart<QuestPart_InvolvedFactions>().factions.Add(sponsor)` 登记委托派系，创建或复用部件 |
| 明确不做 | 不补原版 Harmony，不吞异常，不改任务玩法、生成入口、其它任务或存档格式，不自动重建既有不完整任务；不使用即时刷新作为任务实测 |
| Language | 无，正文和翻译键不变 |
| 存档 | 无新字段、迁移或破坏性重建；仅生成既有原版任务部件 |
| 归属表 | 不更新，无新增本模组持久化类型或 Def |
| 回归 | 真实游戏程序集的临时定向 API 调用已证明新接口创建并挂接部件、复用已有部件且保留关联派系；本次另用 Carbon14 独立测试 profile 走正常 I-051 入口，核对任务正文、关联派系、生成/结局/超时部件及接受后的世界站点，实测结果见下节 |
| 后续债 | 已存入玩家存档的不完整任务未自动恢复，本次不猜测重建；本次实测不代表居民生成、清除战斗和所有任务结局的全流程验收 |

## 验证

- 当前工作区 `make build` 通过，证据 `tmp/automation/20261013T093033Z-build-2ae2a00e/run.json`
- 当前工作区 `make deploy` 被无关王国篇重复语言键、未登记存档键和三条日语占位符门禁阻止，证据 `tmp/automation/20261013T093005Z-deploy-f6ca426a/03-structure.log`；没有覆盖日常 `Mods/RatkinHungerAndHavoc`
- 为避免携带正在编辑的王国篇，实测使用 `afb8053` 的 1.0.6 发布基线加完全相同的单行修复；原节点文件与当前工作区修复前一致。该隔离基线结构检查通过，Release 主体/API 与 Guard 构建通过，552 条既有 net48 测试通过。这些测试是部署门禁，不作为本 bug 的运行时证明
- 部署到独立游戏根 `E:\RHAHAutomation\RatkinHungerAndHavoc\refugee-camp-game`，复用只读游戏与依赖目录；本模组发布文件独立复制并按 SHA-256 核对，共 126 文件，主体 SHA-256 `7f21e4f72e07be55ac7c15ccd18e6bec7406d23b083ab442cd2b27f7935e7314`
- Carbon14 独立 profile 为 `E:\RHAHAutomation\RatkinHungerAndHavoc\refugee-camp-profile`，游戏 1.6.4871，instance `dc55d432453a431d85ed259c7f7d16c7`。新测试地图上调用真实 `GameComponent_RHAH_Game.SpawnArrived("I-051", 300, 0)`，返回 Spawned；未设置 forced、未调用即时刷新
- Quest0 有 8 个部件：关联派系、站点生成、好感变更、三个结局、站点超时和延时。登记的唯一委托派系与奖励派系相同；奖励为 12，两个期限均为 900000 tick。三个结局分别监听 `Quest0.site.ResidentsCleared`、`Quest0.site.Destroyed` 和 `Quest0.OuterNodeCompleted`，前者 Success，后两者 Fail
- 任务界面截图确认正文已显示 15 天、12 好感及 2–4 成人 / 8–16 幼年人数范围，无未解析变量。接受前 NotYetAccepted 且站点未生成；调用原版 Accept 后任务 Ongoing，关联站点 Spawned=true 且实际出现在 WorldObjectsHolder，两个计时部件均 Enabled
- 核心证据：`tmp/automation/carbon/20261013T093858051058Z-ee50b643-response.json`（正常到达）、`20261013T093915040735Z-6533da34-response.json`（任务部件）、`20261013T094124502137Z-c8771fab-response.json`（接受后状态）、`20261013T094125339867Z-31a65c5d-response.json`（站点生成）、`20261013T094203265426Z-87a12503-response.json`（世界对象登记）
- 截图：`tmp/automation/carbon/169bbf990d8c4b70ac782b389d09b881-6de5b006650144da8514cf2aaa955fbc.png`（接受前）与 `02607a399c024977a449a0e2338c7263-97a90e25b0de4fa5944d938771b703fe.png`（接受后）。Carbon14 CLI 中文字符串输出有编码损失，中文正文以实际截图为准；未修改适配器
- 测试 profile 完整 Player.log 定向检查没有 Exception、Sequence contains 或 XML error；仅有已有按键冲突及 Unity fallback 提示。测试进程 PID 29348 已正常停止、未强制终止；未测试进图战斗、实际完成任务或 15 天超时，不声称这些流程通过
