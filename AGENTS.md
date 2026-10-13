# 项目原则

- 功能和修复遵循 YAGNI 和消融实验，先解决当前问题，不夹带无关功能、重构或格式化
- 避免不必要的 Smoke 和 Tests, 不要求做到完美, 只要求尽力, 不认识不熟悉的停下来问用户
- Code as documentation：实现、默认值、类型与字段以代码、Def 和配置为准；文档只保留约束、设计理由、未实施设计和必要操作说明，不维护实现进度副本
- 缺陷、规格、实施工作项和后续债统一用当前仓库 GitHub Issues，按 `docs/agents/issue-tracker.md` 与 `docs/agents/triage-labels.md` 操作
- 本页规定全项目原则和硬约束；子目录 `AGENTS.md` 只补当前目录特有的约束与注意事项，不重复上层规则。历史 ADR 保留决策理由，不凌驾于当前约束；设计和术语变更按 `docs/agents/domain.md` 处理
- 文档必须保持最新，必要时强烈建议删除过时内容、文档

# 项目约束

- 保持显示名 `鼠族: 饥与祸` / `Ratkin: Hunger and Havoc`、`packageId` 与 Harmony Id `nanaloveyuki.ratkin.hungerandhavoc`、Guard 冲突列表及既有 `I-001`..`I-051` 显示 ID 不变
- 命名空间使用 `HungerAndHavoc`，自有 Def / Keyed 使用 `RHAH_`；显示 ID 是稳定数据，不是列表下标，不与剧情编号混用
- 旧鼠灾仓库只对照玩法；不拷贝其源码、存档字段或 `MouseDisaster` 命名，不读取或转换旧鼠灾存档，不自动停用旧模组
- 游戏版本内容放在对应版本目录，当前为 `1.6/`；不把 Def 或程序集放到仓库根，不为规划建立空目录
- 已发布存档键和 XML 类型名是契约；变更须明确自动迁移、新旧共存优先级与失败处理，不能套用历史发布前的破坏性重建例外。新增或变更持久化键、类型和 Def 时同步 `docs/save-ownership.md` 的卸载归属
- 玩家可见文案遵循 `skill://rimworld-writing`；Keyed 中英日键及占位符对应，Guard 同样处理。Def 正文保留中文，英日通过 DefInjected，不删除基础文本来依赖语言回退
- 对外契约限 `HungerAndHavoc.Api.dll`；其它模组不依赖实现类型或私有反射路径。Harmony 补原版或种族框架的明确缺口，跨模组例外须记录公开目标、理由、兼容范围及失败策略
- Harmony、Oxygen DSL Framework、IrisMenus 等第三方程序集不随包发布；不提交 `bin/`、`obj/`、PDB 或临时证据，不上传游戏程序集、凭据、存档或原始日志
- 部署复用现有脚本与发布门禁；游戏运行时不覆盖，不绕过检查，不自动退出玩家游戏。构建失败只读取摘要及对应证据，不批量展开日志或工具 schema
- 本机部署只写 `/mnt/e/Apps/Steam/steamapps/common/RimWorld/Mods/RatkinHungerAndHavoc`；禁止写入 `/mnt/e/Apps/Steam/steamapps/common/RimWorld/Mods/RatkinGreatFamineYearContinued`。实测使用独立 profile，不覆盖玩家原档或日常配置