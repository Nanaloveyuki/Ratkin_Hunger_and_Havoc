# IrisMenus 更新后 RHAH 子页面未注册

| 字段 | 内容 |
| --- | --- |
| 复现 | 玩家截图在 IrisMenus 中只看到 RHAH 整页原版设置。安装 IrisMenus 1.1.1（`supportedVersions=1.6`）并加载 RHAH |
| 期望 / 实际 | 注册 RHAH 全部子页面与搜索提供器 / `TryRegister` 因 `modVersion=1.1.1` 不等于 `1.6` 提前返回，宿主只能显示手动适配的原版设置页 |
| 版本 | 玩家反馈 1.0.6；当前 RHAH 源码与已安装 IrisMenus 1.1.1 |
| 级别 | S2 |
| 根因 | RHAH 将 IrisMenus 的模组发布版本 `modVersion` 与 RimWorld 游戏版本 `supportedVersions` 混为一谈；IrisMenus 现有公开注册 API 仍保留，宿主无须修改 |
| 最小修复 | 仅按 IrisMenus `supportedVersions` 的 RimWorld 1.6 判断游戏兼容，发布版本只用于日志及诊断展示；沿用现有公开注册调用和异常报告 |
| 明确不做 | 不改 IrisMenus 注册/导航机制，不增加动态注册或反射，不重命名 page ID，不改绘制布局，不重新设计设置页 |
| Language | 无 |
| 存档 | 无，设置和宿主导航 ID 保持不变 |
| 归属表 | 否 |
| 回归 | 隔离真实游戏加载已安装 IrisMenus 1.1.1，检查注册 18 个 RHAH 子页面；打开宿主窗口，检查分类导航和页内搜索。IrisMenus 不启用时仍使用原版设置 |
| 后续债 | 无 |

## 已执行验证

- 真实 IrisMenus 1.1.1 启动日志为 `[RHAH] IrisMenus 1.1.1 pages registered.`，运行时注册列表包含 18 个 RHAH 子页面
- 实际打开 IrisMenus，展开 RHAH 分类并进入总览，取得分类导航、搜索框与总览绘制截图 `57291464b3ec4b5a884ef89828197b74.png`
- 最终 Release 构建、552 项回归与结构检查通过。未测 FPS，未将搜索框可见算作每个搜索结果都已操作
