# 选择信失败关窗与关闭错误窗口

| 字段 | 内容 |
| --- | --- |
| 复现 | 打开求援（Request）、使者（Envoy）、重逢（Revisit）、检疫（Quarantine）任意一封选择信，在点击前改变资格或状态（搬空库存、移动商队、记录被其它路径结算、病人离图、设置关停），再点击会失败的选项 |
| 期望 / 实际 | 期望按点击瞬间重新判定；失败时窗口和信都留着并给出原因。实际 `DiaOption.resolveTree = true` 先关窗，失败也关窗，有时只弹一条消息，玩家丢失上下文 |
| 版本 | 1.0.0，基线 60967e8，未正式发布 |
| 级别 | S2 |
| 根因 | RimWorld `DiaOption.Activate` 在 `action()` 前关闭窗口；GrainHole 虽延迟关闭，却按类型移除窗口，可能关掉其它信 |
| 最小修复 | Request、Envoy、Revisit、Quarantine 延迟关窗；包括 GrainHole 在内，成功移信后只调用该选项的 `dialog?.Close()`；失败补实时原因 |
| 明确不做 | 不改剧情、交易或生成顺序，不引入共享基类，不改信栈或存档结构；Visitors 见独立修复卡 |
| Language | 新增 `RHAH_Envoy_Stale`、`RHAH_Revisit_Stale` 中英；Quarantine 复用既有 `RHAH_Quarantine_Stale`；Request 复用既有 `RHAH_Choice_Stale` |
| 存档 | 无新字段、无迁移、无破坏性重建 |
| 归属表 | 不变（无新增可序列化类型或键） |
| 回归 | 原版 DiaOption.Activate 失败保留窗口的访客路径已运行；其余五封的逐路径窗口行为最终按用户要求静态审查验收，不声称运行通过或 Unity 画面验证 |
| 后续债 | 无 |

## 逐文件失败路径

- `ChoiceLetter_RHAH_Request.cs`：失败留窗留信；物资在点击前耗尽时提示既有 `RHAH_Choice_Short`，无效记录提示 `RHAH_Choice_Stale`。
- `ChoiceLetter_RHAH_Envoy.cs`：四个动作统一在包装器重查记录与剧情状态，无效时提示 `RHAH_Envoy_Stale`。交易库存检查仍走既有路径。
- `ChoiceLetter_RHAH_Revisit.cs`：`Pay`（`CanPay` 失败以及 `Choose` 失败）、`Kill`（`Choose` 失败）原本静默返回。`Act`（Leave/Kill）与内联 Pay 选项改为延迟关窗；`Pay` / `Kill` 失败播报新增 `RHAH_Revisit_Stale`。
- `ChoiceLetter_RHAH_Quarantine.cs`：`Choose` 已对 null / 非 Pending / `ChooseQuarantine` 失败播报 `RHAH_Quarantine_Stale`，但窗口已被 `resolveTree` 关闭。`Act` 改为延迟关窗，失败时留窗留信。
- `ChoiceLetter_RHAH_GrainHole.cs`：成功后由选项直接关闭所属窗口，不再按 `Dialog_NodeTree` 类型关闭其它窗口。
- 快照禁用保留：`Choices` 里基于当前物资/关系/检疫做出的 `Disable` 是合理物资与门禁提示，继续保留；真正决定执行的是点击时的实时检查，二者一致。

## 关闭窗口的判定

包装器在 action 前后各查一次 `Find.LetterStack.LettersListForReading.Contains(this)`，只有“这封信原本在栈内、执行后不在”时才关闭它自己的窗口：每个选项把自己的 `DiaOption` 传进包装器，关闭时调用 `option.dialog?.Close()`，指向当前这封选择信的所属对话窗。失败或同时打开着其它信件/窗口都不会被误关。RimWorld 里信只会经 `LetterStack.RemoveLetter` 与 `LetterStackUpdate`（`CanShowInLetterStack` 为 false，含超时）离开信栈，两种情形都被这个“前后对比”覆盖。

后续行为修复见 [choice-behavior-and-ambush.md](choice-behavior-and-ambush.md)：使者 Proof 成功从 Waiting 转 Checking 时保留信，但关闭已经展开的旧选项窗口；重开后不再提供 Proof。失败仍不按类型关闭其它窗口。

## 临时 smoke 方案

1. 触发求援信，交付前清空库存，点击提示 `RHAH_Choice_Short` 且窗口、信仍保留。
2. 使者信：点 `Trade` 前耗尽简单餐，点击应弹 `RHAH_Envoy_NoMeals` 且留窗；把记录置为已结算后点 `Drive`，应弹 `RHAH_Envoy_Stale`。
3. 重逢信：点 `Pay` 前花光白银，点击应弹 `RHAH_Revisit_NoSilver`；把记录置为已选后点 `Kill`，应弹 `RHAH_Revisit_Stale`。
4. 检疫信：记录被其它路径结算后点任一选项，应弹 `RHAH_Quarantine_Stale` 且留窗。
5. 成功路径：正常点击各选项，信从信栈消失的同时窗口关闭，且只关闭这一封所属窗口。
