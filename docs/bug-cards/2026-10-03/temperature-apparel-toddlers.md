# 幼年温度衣被 Toddlers 移除穿戴许可

| 字段 | 内容 |
| --- | --- |
| 复现 | Player.log 中 I-002 生成期间，四名幼年来客尝试穿 RHAH_Cold_ThinHemp / RHAH_Cold_LayeredHemp，出现 `but is not allowed to.`；当前 Toddlers 设置为 BabyTribalwear |
| 期望 / 实际 | 启用温度衣且当前允许穿戴时，婴幼来客能穿温度衣；实际本模组声明 Baby, Child, Adult，Toddlers 将其归入 childClothes，因未在 WearableByBaby 中而移除 Baby 许可，生成代码未查最终 PawnCanWear 就调用 Wear |
| 版本 | 当前 1.0.0，未正式发布；RimWorld 1.6.4871 rev591，Toddlers 1.6 |
| 级别 | S2 |
| 根因 | 可选幼童模组的公开 Def 名单接入缺失；温度衣生成只检查身体部位和层兼容，没有遵守运行时最终年龄 / HAR 穿戴许可 |
| 最小修复 | 在现有 1.6/Patches/RHAH_TemperatureApparel.xml 里，仅当 Toddlers.DefListDef WearableByBaby 存在时幂等追加本模组 12 件温度衣；Source/Generation/RHAH_TemperatureApparel.cs 创建物品前查询 def.apparel.PawnCanWear(pawn, true) |
| 明确不做 | 不改 Toddlers 设置，不强制恢复 Baby 位，不 Harmony 外部私有方法，不扩大普通衣候选池，不修改第三方文件、旧档或其它模组的日志错误 |
| Language | 无，无玩家文案修改 |
| 存档 | 无新键、无迁移或破坏性重建；只修 Def 加载和实时生成判定 |
| 归属表 | 无新增持久化产物，不更新 |
| 回归 | Release 测试程序集构建成功，0 error、4 条既有 xUnit2013 warning；改动 C# 与两个新增测试文件的 LSP diagnostics 均为 OK。补丁回归修复前失败、修复后两项通过：有 Toddlers 保留原项且追加幂等，无 Toddlers 仍执行温度 Stat 补丁。一次针对性场景使用已安装 Toddlers.dll 的真实 ApplyApparelSettings 与实际 DefLists.xml：修复前 12 件温度衣全部丢失 Baby 许可，修复后 Baby/Child/Adult 均能穿，NoBabyApparel 仍拒绝 Baby；无 Error/Warning。临时宿主在衣物逻辑完成后的研究窗口访问处隔离 Unity 边界，不改服装许可逻辑 |
| 后续债 | 未启动游戏，未验证温度衣渲染及完整 Pawn 生成；日志中无栈的通用 DefOf、反射加载与语言汇总错误无法归因本模组，不猜测修改。临时宿主最初的 IL 截断与 ThingDef 引擎构造失败已修正，不计作游戏报错 |
