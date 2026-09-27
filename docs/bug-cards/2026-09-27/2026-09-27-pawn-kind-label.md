# 角色栏显示 famine ratkin

| 字段 | 内容 |
| --- | --- |
| 复现 | 中文界面打开非玩家派系的本模组鼠族角色栏。例：4 岁、初鼠种、派系「饥与祸 失散的」 |
| 期望 / 实际 | 期望种类名是中文，和智人栏的「智人」一样。实际年龄后面是未翻译的 `famine ratkin`，看起来像 feminine ratkin |
| 版本 | 0.1.0 |
| 级别 | S3 |
| 根因 | `RHAH_PawnKind_Ratkin.label` 是英文 `famine ratkin`，没有中文 Def 正文。非玩家派系的 `Pawn.MainDesc` 会把种类名拼进角色栏 |
| 最小修复 | Def 正文改成「饥荒鼠族」。英文放到 `Languages/English/DefInjected/PawnKindDef` |
| 明确不做 | 不加 `labelFemale` / `labelMale`。不改派系名、异种名和生命阶段 |
| Language | 无 Keyed。英文 DefInjected 新增 `RHAH_PawnKind_Ratkin.label` |
| 存档 | 无。标签不是存档键 |
| 归属表 | 否 |
| 回归 | `RHAH_PawnKinds.xml` 的标签是「饥荒鼠族」，英文注入是 `famine ratkin` |
| 后续债 | 无 |
