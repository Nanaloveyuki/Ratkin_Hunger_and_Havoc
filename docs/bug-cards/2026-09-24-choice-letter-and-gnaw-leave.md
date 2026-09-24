# 2026-09-24 选择信写成索要 I-00X，啃树皮后离图

| 字段 | 写什么 |
| --- | --- |
| 复现 | 0.1.0 在玩家地图触发任意会开选择信的事件，例如 `I-004`。再让来客靠近树或墙 |
| 期望 / 实际 | 期望来客信说明谁来了、能收留、雇佣、投喂、拒绝或忽视；物资信写出中文物资名和数量。啃树皮或墙只在饥饿低于 5%、空闲且能走到目标时发生，啃完留在地图上。实际所有选择信都是「他们要 0 份I-00X」，物资信把 def 名当物品名。啃食不看饥饿和当前 Job，啃完走喂食完成，整批随后离场 |
| 版本 | 0.1.0 |
| 级别 | S1 |
| 根因 | `SendLetter` 对所有选择共用 `RHAH_Choice_Text`。`Amount` 对 `None` 返回 0，`ThingDefName` 对 `None` 和 `Baby` 返回 null，于是回退成显示 ID。啃食 JobGiver 只排除已饱食，JobDriver 结束时调用 `RHAH_Feeding.TryComplete`。营养把食物抬过 82% 后标成 `Fed`，Lord 的 `AllReadyToLeave` 把整批切到离场 duty |
| 最小修复 | 按 `RHAH_ChoiceKind` 选文案。物资和婴儿用事件标签与数量；其余选择用各自说明。啃食要求食物比例低于 0.05、当前没有 Job，且现有可达搜索找到目标。啃完只加营养和伤，不再调用喂食完成 |
| 明确不做 | 不改各选择的按钮和结算。不改啃食营养、伤害和过度啃墙。不改吃饱后离开的停留天数。不为 51 个事件各写一封到达信 |
| Language | 修改 `RHAH_Choice_Label`、`RHAH_Choice_Text`。新增 `RHAH_Choice_Aid`、`RHAH_Choice_Intel`、`RHAH_Choice_Baby`、`RHAH_Choice_Abandoned`、`RHAH_Choice_Refugees`、`RHAH_Choice_ChildExchange`、`RHAH_Choice_Kinship`、`RHAH_Choice_Airdrop`、`RHAH_Choice_Visitors` 的 Label 与 Text，中英成对 |
| 存档 | 无 |
| 归属表 | 否 |
| 回归 | 无物资选择不再走数量模板；物资名来自事件标签。食物达到 5% 或已有 Job 时不啃。啃食驱动不再调用喂食完成 |
| 后续债 | 无 |
