# 外部模组兼容登记

跨模组代码只引用 `HungerAndHavoc.Api.dll`；不引用目标模组的私有类型、字段或方法。

| 目标 | 加载顺序 | 依赖 | 支持范围 | 失败策略 | 验证范围 |
| --- | --- | --- | --- | --- | --- |
| NewRatkinPlus | `loadAfter` | 必需 | 1.6，Ratkin 种族匹配与生成 | Ratkin 匹配器未注册时，来源标记请求安全失败；主体不创建无来源 Pawn | Ratkin 判定、`TryMarkOrigin`、交易带入幼年鼠族、Def 加载 |
| Lead Your Pet | `loadAfter` | 可选 | 1.6，运行时查找 `LeadYourPet.LeadYourPetApi` 的公开方法。生成后牵幼年角色：`I-004` 亲子走母亲绳并记来源 1，其它原作亲属走普通绳，`I-013` 亲子且非出售囚犯走母亲绳并记来源 2，`I-012` 与 `I-038` 的商队 Lord 分配随行幼年。加入、俘虏、成交、收留、雇佣和整批离场清理链接。弃婴投放不接。食物替代、商人不卖食物、NPC 交孩子不改 | 未启用、类型缺失、公开签名不对或 `Available` 为 false 时跳过，记一条日志，访客照常生成。不引用 `LeadYourPet.dll`，不反射 `LeadYourPetGameComponent`。清理失败不阻断离场 | 无 LYP 时 `dotnet build` 与 `dotnet test` 通过。发布目录不出现 `LeadYourPet.dll` |
| IrisMenus | `loadAfter` | 可选 | 1.6 公开 API：`MenuRegistry.RegisterSubItemListing` / `RegisterSearchProvider`，`MenuControls`，`MenuSearchEntry`。根分组 `RHAH` 下 14 个 SubItem：Overview、Relief、Events、Pawns、Narrative、Other、Compatibility and diagnostics、Ending、Genes、Dev Events、Event Frequency、Pawn History、Developer、Experimental | 未安装、程序集缺失或 `modVersion` 不是 `1.6` 时不注册菜单，记一条日志，主体照常加载；原版设置窗口保留赈灾区开关。Relief 写入 `reliefEnabled`、`allowEatOutsideRelief`、`ignoreReliefAfterFed`、`leaveAfterFed`、`disabledReliefFoodDefNames`，不进 `.rws` | 无 IrisMenus 时设置窗口仍能改这五项 |
| 金鸢尾兰鼠族 `Ratkin_OA` | 无专用加载顺序 | 可选 | 1.6，异种 `Ratkin_OA` 挂在 NewRatkinPlus 的 `Ratkin` 白名单上。本模组不引用其程序集 | Def 不存在时不进权重表，不报错。玩家在基因页加入后才参与抽取，默认权重 0 | 生物科技开启且该异种已加载时，基因页可加入并保存权重 |
| 鼠鼠退化 | 无专用加载顺序 | 可选 | 不提供新人形种族或异种。手术目标仍回到 `Ratkin` | 不扫描其动物种族，不把肉鼠、松鼠蛋、仓鼠蛋加入异种表 | 退化后的动物不进入饥与祸基因抽取 |

兼容策略统一使用 `RHAH_Api` 的查询、闸门、行为策略、事件和 `TryMarkOrigin`。经历和特质用 `IsOwnedHistory`、`IsOwnedTrait`、`TryGetHistory`、`TryGetTrait`、`CopyHistoryIds`、`CopyTraitIds` 查询显示 ID 与 defName，不扫描 Backstory 或 TraitDef 前缀。兼容适配实现只能放在 `Source/Pawn/Compat`。需要补原版或种族框架缺口时，必须另行登记 Harmony 目标、版本范围和失败策略。

不兼容的旧鼠灾包由 Guard 提示并阻止主体加载；不自动停用旧包，也不迁移旧存档。
