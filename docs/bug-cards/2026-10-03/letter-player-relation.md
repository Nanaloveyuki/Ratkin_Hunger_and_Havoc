# 打开访客信查询玩家自身关系

| 字段 | 内容 |
| --- | --- |
| 复现 | 玩家点击 RHAH 信件时报告派系关系错误；Player.log 第 2033 行记录 I-002 生成，2034 行为 `Tried to get relation between faction PlayerColony and itself.`。打开未结算访客选择信会在构造“送往盟友”选项时遍历全部派系 |
| 期望 / 实际 | 玩家派系不能作为盟友接收方，打开信不应查询自身外交关系；实际排除玩家的规则执行前已经读取 PlayerRelationKind，触发原版 RelationWith 自关系错误 |
| 版本 | 当前 1.0.0 源码，未正式发布；游戏日志为 RimWorld 1.6.4871 rev591 |
| 级别 | S2 |
| 根因 | RHAH_ChoiceRuntime.AcceptsAlly 将 faction.IsPlayer 与 faction.PlayerRelationKind 等作为同一次方法调用的参数；C# 参数先求值，IsAllyDestination 内部的 !player 短路无法保护外部的关系查询 |
| 最小修复 | Source/Incidents/RHAH_ChoiceRuntime.cs：在读取外交关系前拒绝玩家派系；保留现有盟友资格、据点要求与最小 loadID 选择规则 |
| 明确不做 | 不给玩家建立自身关系，不 suppress 日志，不改信件选项、派系好感、公开 API、相邻衣物报错；不部署或启动整局游戏 |
| Language | 无 |
| 存档 | 无新键、无迁移或破坏性重建；既有信件和选择记录直接使用修复后的实时筛选 |
| 归属表 | 不更新，无新增持久化类型或键 |
| 回归 | Release 测试项目构建成功，0 error、4 条既有 xUnit2013 warning。Mono 运行真实 FactionManager / WorldObjectsHolder / AllyDestination：修复前捕获 PlayerRelationKind → RelationWith 的自身关系错误；修复后玩家唯一派系的回归通过，无新 Error。仅追加一次直接 smoke：玩家与一个没有据点的盟友共存时目的地仍为 null、HasAllyDestination=false、外交关系仍为 Ally，无游戏错误。临时 runner 把 Log.Error 转为失败，未屏蔽错误 |
| 后续债 | 未启动游戏、未部署，未验证 Unity 信件画面和有据点盟友的最小 loadID 选择；这些选择规则未改。未处理本日志中其它模组异常或相邻服装问题 |
