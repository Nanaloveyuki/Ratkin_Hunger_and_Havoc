# 卸载导出遗漏外部 Hediff 的思想阶段缓存

| 字段 | 内容 |
| --- | --- |
| 复现 | 玩家在 ODF 更新前的 1.0.6 已有存档导出卸载副本。世界 Pawn 与地图 Pawn 的外部 Hediff 保存 `SpokenThoughtTicks/keys/li=RHAH_Thought_GrainGreed_0`、`_1`、乞讨结果等思想阶段键，导出报 `Unresolved mod references` |
| 期望 / 实际 | 移除本模组思想引用并保留外部 Hediff 和其余缓存 / 清理器只能识别完整 Def 名，带阶段后缀的键在残留检查中导致导出中止 |
| 版本 | 玩家反馈 1.0.6；当前 ODF 更新后的源码仍保留此缺口 |
| 级别 | S1 |
| 根因 | `RHAH_SaveExport.BuildPlan` 只登记 Def 名；`RHAH_SaveCleanup.IsOwnedScalar` 不识别外部 `SpokenThoughtTicks` 用 `ThoughtDef.defName + "_" + stageIndex` 保存的字符串字典键 |
| 最小修复 | 从本包已加载 ThoughtDef 的实际阶段生成精确缓存键白名单；仅删除 Hediff 下 `SpokenThoughtTicks/keys/li` 的已知本模组阶段键，沿用 `Remove` 同步删除对应 values |
| 明确不做 | 不删除外部 Hediff，不按 `RHAH_` 前缀猜删，不把任意带后缀的字符串当作 Def，不放宽未知引用中止，不改 ODF provider 边界 |
| Language | 无 |
| 存档 | 无新键或迁移；只清理导出副本的外部思想缓存，不修改 live world 或原档 |
| 归属表 | 是，登记外部思想阶段缓存的 Remove 边界 |
| 回归 | 世界 Pawn 与地图 Pawn 的真实路径形状，多个相邻/交错本模组键移除后保留外部键及准确 tick 配对；源副本不变，二次清理不变；未知阶段键、其它位置的同名字符串仍中止 |
| 后续债 | 未取得玩家原始存档；玩家完整模组组合的卸载加载需用原档副本复测 |

## 已执行验证

- 最终 Release 回归 552 项通过，0 失败、0 跳过；结构与中英日键、占位符检查通过
- 对实际 `HungerAndHavoc.dll` 执行阶段缓存烟雾：外部 Hediff 保留，外部键对应 tick=20，本模组阶段移除，第二次清理不变
- 真实游戏 `BuildPlan` 生成 96 个实际思想阶段键；真实 `Export` 生成独立备份及 `RHAH-removed-20261011-082949-017c8f5b.rws`，导出 XML 无 `RHAH_`、`HungerAndHavoc.` 或本包 ID 残留
- 在隔离配置中仅停用 RHAH，保留鼠族、Harmony、Biotech 与 ODF，加载导出副本的两张地图并推进 120 tick；原档与玩家配置未覆盖，烟雾结束已恢复隔离配置并正常退出进程
- 验证边界：该完整导出档没有玩家外部语音 Hediff，阶段缓存场景由真实程序集烟雾及永久回归覆盖。卸载加载仍记录 3 条原版袭击 Lord trigger 索引和 5 条整数 think node 缓存缺失警告；能加载推进，但不能声称零警告。此项独立于本次字符串缓存遗漏，后续按独立修复卡处理
