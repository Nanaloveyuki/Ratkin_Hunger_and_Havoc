# LYP 自定义访客商队分配不生效

状态：已实现，未执行验证

| 字段 | 内容 |
| --- | --- |
| 复现 | 启用本机 LYP 1.6，生成 I-012/I-038。来自源码对照；卸载清理已运行隔离复现，其它尚未游戏内复现 |
| 期望 / 实际 | 应牵已有幼年来客；实际旅行 API 不接受本项目 Lord/PawnKind |
| 版本 | 本仓库 1.0.0，2026-09-30 当前工作树；保留用户并发改动 |
| 级别 | S2 |
| 根因 | `TryLeashArrivals` 对 I-012/I-038 置 `travel` 后调用 `LeadYourPetApi.TryAssignTravelChildren`。该方法只处理旧鼠灾 `MouseDisaster_TraderRatkinAdult` / `MouseDisaster_BeggarRatkinChild`，否则要求 `LordJob_TradeWithColony` 或 `LordJob_VisitColony` 并另生成幼年。本项目商队进 `LordJob_RHAH_Visitor`，PawnKind 是 `RHAH_PawnKind_Ratkin`，已有 `RatkinYoung` 因此不会被牵 |
| 最小修复 | Source/Pawn/Compat/RHAH_LeashBridge.cs、RHAH_LeashPlan.cs；使用 LYP 已公开逐对绑定 API 给已有孩子分配照护者，不补丁外部私有类型、不额外生成人物 |
| 明确不做 | 不部署，不修改外部 Mod，不改包名/API/事件显示 ID，不夹带重构 |
| Language | 预计无；若新增玩家可见字符串必须同时补中英 Keyed |
| 存档 | 默认无新键、不破坏重建；保留已有记录。若必须新增持久化类型，动手前在本卡明确并登记 |
| 归属表 | 无新增类型时保持；卸载清理动作或新持久化类型必须更新 save-ownership.md |
| 回归 | I-012/I-038 绑定已有幼年；无 LYP 安全跳过；离场清理；不重复链接。实现已完成。子代理已运行隔离冒烟 `/tmp/rhah-travel-leash-smoke.csproj` 并通过；仓库 `dotnet test` 未由子代理执行，主代理统一执行并记录 |
| 后续债 | 本卡外相邻问题不纳入；游戏内验证边界明确报告 |
