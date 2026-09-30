# 商队伏击缺少攻击 Lord

状态：实现及拒绝无效输入回归通过，完整 Lord 创建受 Unity 宿主限制

| 字段 | 内容 |
| --- | --- |
| 复现 | 触发 I-035/I-050 并成功生成遭遇地图。来自源码对照；卸载清理已运行隔离复现，其它尚未游戏内复现 |
| 期望 / 实际 | 应有攻击及撤退群体控制；实际仅生成 Pawn 没有 Lord |
| 版本 | 本仓库 1.0.0，2026-09-30 当前工作树；保留用户并发改动 |
| 级别 | S1 |
| 根因 | `TrySpawnCaravanAmbush` 生成未入场攻击者后只调用 `SetupCaravanAttackMap`。原版 `IncidentWorker_Ambush.DoExecute` 和 `IncidentWorker_CaravanDemand.ActionFight` 都在地图生成并把同一批敌人放入后，用 `LordMaker.MakeNewLord` 建 `LordJob_AssaultColony(faction, canKidnap: true, canTimeoutOrFlee: false, sappers: false, useAvoidGridSmart: false, canSteal: true, breachers: false, canPickUpOpportunisticWeapons: false)`。这里没有这一步，攻击者留在地图上但没有 Lord，不会集体袭击，也不会因绑架或偷窃撤退 |
| 最小修复 | `Source/Trade/TradeEventRouter.cs`。地图非空后调用 `TryCreateAssaultLord`，Lord 派系与全部攻击者派系相同，参数与原版伏击一致。空列表、空地图、空派系、已有 Lord 或派系不一致时不建 Lord。建出的 Lord 不是袭击 Lord 或没有收齐这批人时，摘掉成员引用并从 `lordManager.lords` 移除，再 `Cleanup` 销毁攻击者。不改 Core 点数 |
| 明确不做 | 不部署，不修改外部 Mod，不改包名/API/事件显示 ID，不夹带重构 |
| Language | 预计无；若新增玩家可见字符串必须同时补中英 Keyed |
| 存档 | 默认无新键、不破坏重建；保留已有记录。若必须新增持久化类型，动手前在本卡明确并登记 |
| 归属表 | 无新增类型时保持；卸载清理动作或新持久化类型必须更新 save-ownership.md |
| 回归 | 真实 TryCreateAssaultLord 的派系不一致、空地图、空派系、空列表回归通过。成功创建尝试运行后被 Lord 静态材质初始化阻断，调用 Unity 原生资源加载；该引擎集成用例已移出无引擎单元套件，不伪造成功。完整地图生成、袭击、绑架和偷窃需游戏内验证 |
| 后续债 | 不改 I-035/I-050 到达态度。Core 点数由主代理处理。游戏内尚未确认遭遇地图上的袭击、绑架、偷窃和失败清人 |
