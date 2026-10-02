# 默认衣物准入池

| 字段 | 内容 |
| --- | --- |
| 复现 | 默认设置生成来客并启用第三方鼠族衣物模组 |
| 期望 / 实际 | 只抽原版部落装、NewRatkinPlus 原始指定清单内的衣物和幼年基本衣；实际允许全部官方中世纪衣物、来源不明幼年衣物及外部 opt-in |
| 版本 | 1.0.0，基线 60967e8，未正式发布 |
| 级别 | S2 |
| 根因 | 来源与科技等级不是指定衣物池，allowRefugeeApparel 绕过默认边界；普通衣在温度衣之前清空全场并阻挡温度衣 |
| 最小修复 | 默认准入使用代码内固定的 NewRatkinPlus 1.6 原始 55 项 defName 白名单，且来源必须为 `solaris.ratkinracemod`；原版成人只留官方 Core/DLC 来源的 `Apparel_TribalA`，幼年只留同样官方来源的 `Apparel_BabyOnesie`、`Apparel_WarmerHat`、`Apparel_SunHat`、`Apparel_KidTribal`。全部还须是可制材、遮裸体且 ≤ 中世纪的已启用衣物，并通过年龄和身体部位检查。不读 HAR 当前可穿池，不保存 `ratkinApparel`，不抓取补丁时快照，第三方 opt-in 不准入。生成器衣物先清空，再穿温度衣，再尝试普通衣；普通衣只在兼容且无需脱掉任何已有衣物时穿上，不因已有温度衣而跳过整池 |
| 明确不做 | 不重写旧档已有衣物；不改品质材质概率或年龄范围；不改显式衣列表契约；不恢复 HAR 当前 Def 的可写清单；不删除曾公开供 XML 创建的 `RHAH_GenerationExtension.allowRefugeeApparel` 字段，但它不再参与候选判定 |
| Language | `RHAH_Settings_RefugeeApparel_Quote` 与 `_Tooltip` 中英描述固定来源和名单，保留温度衣先穿、普通衣只兼容叠穿的真实顺序 |
| 存档 | 无新存档键，无迁移或破坏性重建。没有 `ratkinApparel` 字段、存储或 XML 快照；现存 `allowRefugeeApparel` 是不再参与准入的旧 Def XML 标志，不进 `.rws` |
| 归属表 | 更新衣装模式、禁用列表语义及 `RHAH_GenerationExtension` 类型说明：默认池固定，不会随第三方衣物加入而扩展 |
| 回归 | 衣物策略回归通过；真实 XML 经原版继承解析后，实际 Candidate / AppendCandidates / TryWear / Apply 与 Wear 离线 smoke 通过：第三方扩池及旧 opt-in 拒绝，温度衣保留，兼容叠穿，冲突不替换。之后按用户要求停止追加运行，最终按静态审查验收 |
| 验证边界 | 离线隔离绘制通知；本机只有儿童部落服，另外三个幼年 Def 未加载，不伪造为实跑。未验证 Unity 画面 |
