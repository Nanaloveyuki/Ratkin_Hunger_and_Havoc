# 舔地觅食心情漏译

| 字段 | 内容 |
| --- | --- |
| 复现 | 玩家反馈：“在地上找能吃的东西”工作结束后，心情显示 licked the floor |
| 期望 / 实际 | 简中显示中文心情名称与说明；实际 RHAH_Thought_ScavengedFilth 的 stages.0.label / description 仍是英文，简中 DefInjected 未包含它 |
| 版本 | 当前 1.0.0，未正式发布 |
| 级别 | S3 |
| 根因 | Def 基础正文未遵守中文约定；英文、日文只有顶层 description 注入，原版 Thought.Description 优先读 CurStage.description，所以只补顶层翻译不能覆盖阶段说明 |
| 最小修复 | 只将此 ThoughtDef 阶段名称和说明改为中文，在现有英文、日文 DefInjected 增加 stages.0.description 保留各语言说明；不创建重复简中覆盖 |
| 明确不做 | 不改觅食 Job、营养、心情数值、身份或相邻心情，不批量翻译 |
| Language | Def 阶段名称改为“舔了地上的东西”，说明“我舔了地上的一点东西。至少没吐出来。”；英文、日文新增 RHAH_Thought_ScavengedFilth.stages.0.description，保持已有译文 |
| 存档 | 无新键、无迁移或破坏性重建，defName 不变 |
| 归属表 | 不更新 |
| 回归 | 原版 DirectXmlToObject 加载实际 ThoughtDef，原版 DefInjectionPackage 注入英文、日文 stages.0.label / description 成功且零诊断；输出中文“舔了地上的东西 / 我舔了地上的一点东西。至少没吐出来。”，英文及日文保留各自说明。心情 -3、持续 1 天、堆叠 3 均未变。结构、双语和日语门禁通过；无固定文案永久测试 |
| 后续债 | 不启动游戏，不验证需求栏画面；玩家报告作为漏译依据，不要求再次复现 |
