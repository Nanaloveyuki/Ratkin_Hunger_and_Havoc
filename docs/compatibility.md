# 外部模组兼容登记

本表是 Step 11 的兼容边界。跨模组代码只引用 `HungerAndHavoc.Api.dll`；不引用目标模组的私有类型、字段或方法。

| 目标 | 加载顺序 | 依赖 | 支持范围 | 失败策略 | 验证范围 |
| --- | --- | --- | --- | --- | --- |
| NewRatkinPlus | `loadAfter` | 必需 | 1.6，Ratkin 种族匹配与生成 | Ratkin 匹配器未注册时，来源标记请求安全失败；主体不创建无来源 Pawn | Ratkin 判定、`TryMarkOrigin`、交易带入幼年鼠族、Def 加载 |
| Lead Your Pet | `loadAfter` | 可选 | 1.6，牵引行为 | 不存在或 API 不可用时跳过适配；不反射私有类型 | `Leash` 闸门、释放顺序、幼年鼠族被牵引 |
| IrisMenus | `loadAfter` | 可选 | 1.6，菜单入口 | 菜单未注册时保留 API/调试入口；不阻止主体加载 | 事件触发入口、来源查询、缺失模组安全加载 |

兼容策略统一使用 `HungerAndHavocApi` 的查询、闸门、行为策略、事件和 `TryMarkOrigin`。兼容适配实现只能放在 `Source/Pawn/Compat`。需要补原版或种族框架缺口时，必须另行登记 Harmony 目标、版本范围和失败策略。

不兼容的旧鼠灾包由 Guard 提示并阻止主体加载；不自动停用旧包，也不迁移旧存档。
