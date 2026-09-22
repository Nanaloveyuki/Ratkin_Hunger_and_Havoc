# 外部模组兼容登记

本表是 Step 11 的兼容边界。跨模组代码只引用 `HungerAndHavoc.Api.dll`；不引用目标模组的私有类型、字段或方法。

| 目标 | 加载顺序 | 依赖 | 支持范围 | 失败策略 | 验证范围 |
| --- | --- | --- | --- | --- | --- |
| NewRatkinPlus | `loadAfter` | 必需 | 1.6，Ratkin 种族匹配与生成 | Ratkin 匹配器未注册时，来源标记请求安全失败；主体不创建无来源 Pawn | Ratkin 判定、`TryMarkOrigin`、交易带入幼年鼠族、Def 加载 |
| Lead Your Pet | `loadAfter` | 可选 | 1.6，牵引行为 | 不存在或 API 不可用时跳过适配；不反射私有类型 | `Leash` 闸门、释放顺序、幼年鼠族被牵引 |
| IrisMenus | `loadAfter` | 可选 | 1.6 公开 API：`MenuRegistry.RegisterSubItemListing` / `RegisterSearchProvider`，`MenuControls`，`MenuSearchEntry`。根分组 `RHAH` 下 14 个 SubItem：Overview、Events、Pawns、Narrative、Other、Compatibility、Diagnostics、Ending、Genes、Dev Events、Event Frequency、Pawn History、Developer、Experimental | 未安装、程序集缺失或 `modVersion` 不是 `1.6` 时不注册菜单，记一条日志，主体照常加载；原版设置窗口和 `HungerAndHavocScheduler.QueueDebugIncident` 保留。注册异常只禁用菜单。Ending、Genes、Pawn History 及频率函数尚无领域数据，页面只显示不可用，不写存档。Experimental 写入 `optimizeGeneration`。频率页的显示天数只留在菜单实例里 | 真实游戏里打开全部分类并返回；无 IrisMenus 时设置窗口仍保存 `enableNewContent` 和 `optimizeGeneration`；Dev Events 只调用现有调度入口 |

兼容策略统一使用 `HungerAndHavocApi` 的查询、闸门、行为策略、事件和 `TryMarkOrigin`。兼容适配实现只能放在 `Source/Pawn/Compat`。需要补原版或种族框架缺口时，必须另行登记 Harmony 目标、版本范围和失败策略。

不兼容的旧鼠灾包由 Guard 提示并阻止主体加载；不自动停用旧包，也不迁移旧存档。
