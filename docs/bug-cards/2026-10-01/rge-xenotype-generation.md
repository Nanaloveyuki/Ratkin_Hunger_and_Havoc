# RGE 下生成智人异种伪鼠

状态：正式修复已实现，结构检查、Release 构建、348 项测试与真实程序集隔离烟雾通过；未部署，完整游戏生成待验证

| 字段 | 内容 |
| --- | --- |
| 复现 | 启用 NewRatkinPlus、Biotech、HAR 与 Ratkin Gene Expanded，保持饥与祸默认异种权重，开启任一本模组基因并生成事件来客。RGE 官方和非官方 1.6 分支都排除初鼠种。真实方法隔离烟雾观察到 `race=Ratkin; xenotype=Baseliner; genes=RHAH_Gene_ThinRations`；尚未运行完整 Unity 地图生成 |
| 期望 / 实际 | 来客应保有鼠族种族和允许的完整异种基因，再追加本模组基因；实际 HAR 拒绝初鼠种后异种字段为空，显示智人且只有本模组基因，没有错误日志 |
| 版本 | 本仓库 1.0.0，2026-10-01 当前工作树；本机 HAR 1.6 与 RGE 的 `1.6` / `1.6_unofficial` 内容 |
| 级别 | S1 |
| 根因 | `RHAH_XenotypeResolver` 固定默认和全零回退 `RK_XenoType_Ratkin`，不检查 HAR 的公开 `RaceRestrictionSettings.CanUseXenotype`。RGE 替换 Ratkin 的白名单排除它和 `RHAH_Xenotype_Ratkin`；HAR `SetXenotypePrefix` 返回 false，跳过原版安装。原版空字段 getter 返回 Baseliner。独立 `RHAH_PawnKind_Ratkin` 没有 RGE 修改的基础 PawnKind 异种池，关闭优化也会走空池。工厂未核对安装结果就追加本模组基因 |
| 最小修复 | 通过兼容桥缓存 HAR 公开许可方法，不编译引用或修改外部程序集。按请求种族过滤权重候选与兜底。默认初鼠种不可用时读取已加载 `RatkinColonist.xenotypeSet` 的真实权重，不写死 RGE 名称或分支权重；关闭优化仍使用种族本底池。显式配置不偷偷换异种。无允许候选、显式异种缺失/被禁用、生成种族改变、选定异种或基因未安装时记错误并清理新 Pawn，返回现有 GenerationFailed，不追加自有基因、不登记来源 |
| 明确不做 | 不放宽 HAR 白名单，不 Harmony RGE，不改 packageId/API/事件 ID，不继承基础 PawnKind 的其它年龄装备字段，不批量修改既有伪鼠，不部署，不夹带繁殖路径重构 |
| Language | 修改中英 `RHAH_Menu_Genes_Note` / `RHAH_Menu_Genes_Fallback`，说明种族允许的本底异种回退。日志不走 Keyed |
| 存档 | 无新键、迁移或破坏性重建；保留原异种启用名单与存储权重，禁止项只在运行时过滤 |
| 归属表 | 无新增持久化类型或 Def，保持 `save-ownership.md` |
| 回归 | `python3 scripts/verify-scaffold.py` 通过；主体/API 和 Guard Release 构建均 0 警告、0 错误；全套 348 项测试通过，新异种回归 3 项覆盖禁止候选后的概率边界、零权重及内外源完整性。隔离烟雾读取两分支真实 XML 白名单与基础池，运行新 Release DLL 和真实 HAR 的许可/SetXenotypePrefix；原鼠族默认和全零、RGE 各概率区间、默认/全零/关闭优化、玩家正权重、显式允许/禁止/缺失/空值、空许可池、缺失默认合法兜底、无 Biotech、种族被改及安装不完整检查均通过。真实 `Pawn_GeneTracker.SetXenotype` 安装简化耳尾基因后通过检查，再追加 RHAH，观察两版均保持 `race=Ratkin` 且耳尾与自有基因共存。只在临时 DLL 副本清空宿主静态启动与 `HediffSet.DirtyCache` 绘制缓存；生产方法未改写。没有运行完整 PawnGenerator、地图入场/失败销毁或全套 RGE 基因/渲染，不能当作游戏集成已通过。临时程序已移除 |
| 后续债 | 玩家旧存档中的伪鼠不自动修复；实际 Pawn Def 为 Human 的其它模组路径不在本卡范围 |
