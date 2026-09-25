# 2026-09-24 日志报错与生成后立刻离图

| 字段 | 写什么 |
| --- | --- |
| 复现 | 0.1.0 加载本模组，看 Player.log。再在玩家地图触发来客事件，观察刚生成的鼠族 |
| 期望 / 实际 | 期望加载无本模组 XML / Config 报错，来客和其他鼠族一起在地图逗留并寻食。实际 `RHAH_ChoiceRequest` / `RHAH_ChoiceVisitors` 找不到父节点 `NeutralEvent`；`RHAH_PawnKind_Ratkin` 缺初始抵抗和意志；寻食失败的来客因 `ExitMap` 默认放行立刻走向出口 |
| 版本 | 0.1.0 |
| 级别 | S1 |
| 根因 | `NeutralEvent` 是 LetterDef 的 `defName`，没有同名抽象父级。人类 PawnKind 必须声明 `initialResistanceRange` 和 `initialWillRange`。活跃访客的 `ExitMap` 默认 true，寻食 JobGiver 在乞讨、偷窃、啃咬和等待都失败时直接发离场 Job |
| 最小修复 | 信件去掉不存在的 `ParentName`，自己声明中性事件外观。PawnKind 补 0 抵抗和 0 意志。活跃访客默认关闭 `ExitMap`，只在生命周期已经是 Leaving 时放行；饱食离开仍走 `LeaveAfterFed` |
| 明确不做 | 不改批次伤害和驱逐已经发出的 `RHAH_Leave`。不把寻食失败改成永久逗留。不改 PawnKind 衣着和战斗力 |
| Language | 无 |
| 存档 | 无。Def 字段不是存档键 |
| 归属表 | 否 |
| 回归 | 信件 XML 不再引用 `NeutralEvent` 父级；PawnKind 声明两段区间；活跃访客 `ExitMap` 为 false，Leaving 为 true |
| 后续债 | 无 |
