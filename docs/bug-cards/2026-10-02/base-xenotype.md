# 初鼠种基底候选兼容

| 字段 | 内容 |
| --- | --- |
| 复现 | 启用 RGE 两个 1.6 分支之一，基因页给其它异种正权重 |
| 期望 / 实际 | 初鼠种默认相对权重 100 与其余异种共存；实际 RGE 覆盖白名单后被 HAR 过滤 |
| 版本 | 1.0.0，基线 60967e8，未正式发布 |
| 级别 | S2 |
| 根因 | RGE RKRacePatch 替换白名单移除 RK_XenoType_Ratkin，权重算法本身正确。新增回归测试的独立宿主尚未加载 Prefs，原版 DeepProfiler 默认启用并在 DirectXmlToObject 的列表 / PostLoad 计时读取 Prefs.LogVerbose，空引用经原版错误日志暴露为 Unity LogError；这是宿主初始化问题，不是 XML 字段错误。`<success>Normal</success>` 是原版 PatchOperation 支持的继承字段，省略时语义相同 |
| 最小修复 | 本模组在 RGE 后只补回初鼠种白名单，不覆盖其它异种。回归测试只在各测试生命周期内关闭依赖 Prefs 的深度计时并恢复原值，保留真实 DirectXmlToObject / Apply，不截断或吞掉解析错误；继续尊重 HAR 许可与实际基因验证 |
| 明确不做 | 不修改外部模组，不改归一算法，不锁死玩家权重为 100，不绕过 HAR |
| Language | 基因页中英提示说明基底相对权重 |
| 存档 | 无新键、迁移或破坏性重建 |
| 归属表 | 不变 |
| 回归 | 真实 DirectXmlToObject / PatchOperation 已对 NRP 原始 XML 和 RGE 两分支的白名单替换操作执行通过：其它候选完整保留，初鼠种仅补一次，完整 XML 重放幂等，后续第三方候选保留，零解析诊断。永久测试对应路径通过；完整 Unity/HAR 载入未验证。最终按用户要求静态审查验收 |
| 后续债 | 完整 HAR/Unity 载入仍须实机确认 |
