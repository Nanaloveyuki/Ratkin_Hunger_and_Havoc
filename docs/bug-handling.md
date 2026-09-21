# Bug 处理

修 bug 前先填修复卡，再改代码。流程以本页为准。严重级别仍用 [project-goals.md](project-goals.md) 的 S0–S3。存档键和卸载归属以 [save-ownership.md](save-ownership.md) 为准。

Language、存档键、卸载归属这三项，功能开发只要碰到玩家文案、持久化或 Def，同样要过。

## 修复卡

必填后再动手：

| 字段 | 写什么 |
| --- | --- |
| 复现 | 步骤、存档/新游戏、相关事件或 pawn |
| 期望 / 实际 | 各一句 |
| 版本 | 模组版本或提交 |
| 级别 | S0–S3 |
| 根因 | 哪条契约或状态错了，不是崩溃栈顶 |
| 最小修复 | 准备改的路径 |
| 明确不做 | 这次故意留下的相邻债 |
| Language | 无 / 新增或修改的键 |
| 存档 | 无 / 新字段 / 迁移 / 破坏性重建 |
| 归属表 | 是否更新 [save-ownership.md](save-ownership.md) |
| 回归 | 失败测试或手工步骤 |
| 后续债 | 单独跟进的项 |

## 防修过头

只改根因路径、回归测试，以及这次修复强迫带上的 Language / 存档键 / 归属表。

这次不要做：

- 顺手迁命名空间、改 API、重排分层
- 默默改存档键或 XML `Class` / `hediffClass` / `workerClass`
- 补无关功能、顺手翻译整份文案、整文件格式化
- 把工程标准迁移塞进行为修复

根因本身就是契约错误时，契约改动就是修复；仍然不要夹带其它重构。相邻债写进修复卡，另开任务。

## 防没修到位

- 先定位根因。只加 null 判断或 `try/catch` 不算修好，除非 null 就是契约
- 能测的先写会失败的测试，再改实现
- 状态问题要覆盖存读；文案覆盖中英；身份覆盖访客和已释放；事件覆盖其 Target（Map / Caravan）
- 临时保护必须写移除条件
- API、事件目录、生命周期、结局、存档键修复后，同步改对应文档

## Language

玩家能看见的字符串走 Keyed，前缀 `RHAH_`。

- `Languages/ChineseSimplified/Keyed` 与 `Languages/English/Keyed` 键集合必须相同，占位符 `{0}` 数量一致
- Guard 的中英 Keyed 同样成对
- Def 正文留中文；英文靠 `Languages/English/DefInjected`。不要删 Def 基础文本，RimWorld 不会按字段回退到 English
- 不要求中文 DefInjected，只要 Def 正文已经是中文
- 新增 `HungerPawnRole` 必须补 `RHAH_Role_*` 中英键
- 代码里不要写死给玩家看的中文或英文
- About、README、日志、存档键、Def 名、packageId 不是 Language 系统

## 存档

本模组不读旧鼠灾档。这里的旧档只指本模组之前版本的 `.rws`。

- 1.0.0 前可以重建实验性键，但修复卡必须写明“迁移”或“破坏性重建”，禁止默默改名
- 1.0.0 起存档键冻结；改键必须读旧写新，并登记迁移
- 新字段的 `Scribe_*.Look` 默认值必须等于字段默认值；集合在 `PostLoadInit` 补空，并写明 null 与空集合的语义
- 存档键、`hediffClass`、`workerClass`、XML `Class=` 都是契约，不等于 C# 字段名
- 改了键、默认值、类型名或可序列化类型，必须同步 [save-ownership.md](save-ownership.md)

## 卸载保护

目标与旧模组一致：当前存档停用新内容 → 备份原档 → 导出 XML 清理副本 → 退出后再卸本模组，保留鼠族等前置。原档不覆盖。未知本模组引用必须中止，不猜测删除。不碰其它模组数据，不清理全局 `ModSettings`。

导出器尚未实现。现在每次新增会进存档的 Def、可序列化类型或键，都必须先写入 [save-ownership.md](save-ownership.md)，动作只允许 `Remove` 或 `Replace`。没有登记的持久化产物视为漏了卸载保护。

## 检查

当前先人工对照本页和 [save-ownership.md](save-ownership.md)。`scripts/verify-scaffold.py` 已检查中英 Keyed 键集合对称、占位符数量、Comp 存档键和 Hediff XML 类型名。尚未自动检查：`Translate` 缺键、英文 DefInjected 覆盖、卸载归属表与代码同步。

现有命令：

```bash
python3 scripts/verify-scaffold.py
```

## 明确不做

- 不把旧鼠灾存档字段迁进本模组
- 不在行为修复里做工程标准迁移
- 本次文档不实现卸载导出器
