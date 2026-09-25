# 中文经历显示英文，标题不是 Mod 加形容词名词

| 字段 | 内容 |
| --- | --- |
| 复现 | 中文游戏，生成带本模组经历的角色，打开经历 |
| 期望 / 实际 | 期望中文描述，标题为 `Mod` 加形容词名词；实际描述是源 Def 英文，标题是无前缀的名词加动词 |
| 版本 | 0.1.0 |
| 级别 | S2 |
| 根因 | 中英 DefInjected 把正文写进 `baseDescription`。源 Def 已有 `description`，`baseDesc` 不会覆盖。中文 `title` 也不是 `Mod` 加形容词名词 |
| 最小修复 | `Languages/ChineseSimplified/DefInjected/BackstoryDef/RHAH_Histories.xml`、`Languages/English/DefInjected/BackstoryDef/RHAH_Histories.xml` |
| 明确不做 | 不改源 Def 英文、不改经历技能、不改短标题长度策略以外的正文措辞 |
| Language | 123 条 `RHAH_History_*.description`；中文 `title` 与 `titleShort` 改为 `Mod` 加形容词名词 |
| 存档 | 无。经历仍按 `BackstoryDef` defName 保存 |
| 归属表 | 否 |
| 回归 | 中英注入键与 Def 一一对应，描述键为 `description`，中文标题均为 `Mod ` 开头 |
| 后续债 | 无 |
