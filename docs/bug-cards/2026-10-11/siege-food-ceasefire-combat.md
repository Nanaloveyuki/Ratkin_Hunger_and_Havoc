# 围攻交粮停战与访客反击

| 字段 | 内容 |
| --- | --- |
| 复现 | v1.0.6 围攻批次敌对时点击送粮，交接前后观察派系与攻击任务；给等待者预置到期断粮期限或吃饱期限；等待中保存再加载 |
| 期望 / 实际 | 接受有效交接后本图本批活动访客转中立并保留 Lord，未交齐不按吃饱期限退出；无交接的敌对可战斗成人会按 Fight 选原版攻击任务 / 交接只登记名单，不切态度或派系；ReadyToLeave 无条件把历史断粮期限视为活动等待；访客调度没有战斗入口 |
| 版本 | 玩家反馈 v1.0.6（ODF 更新前），修复基于当前 ODF 仓库 |
| 级别 | S1 |
| 根因 | FoodHandoff 与批次当前态度缺少同一事务；ReadyToLeave 没区分未完成交接、已交齐和历史计时；Fight 只有许可、没有访客工作选择消费。原版 Pawn.SetFaction 会通知 ChangedFaction 并清 mind，必须沿用 PreserveForAttitudeChange 保留成员和 duty。Comp 当前态度 Neutral 的无条件旧档回退会把明确保存的停战覆盖回到敌对 |
| 最小修复 | 有效进入既有等待 toil 后，本图同批 Hostile 活动访客变 Neutral，非敌对态度不降级，保留到达快照、Lord 和生命周期；共同派系变化入口保留 duty 并同步 Lord 派系。待交接者只按本人截止离场，收齐后清历史断粮计时。访客工作及离场 duty 调用原版 AIFightEnemies，条件同时核对 Fight、实际敌对、成年、可行动与暴力能力；交接等待成员不反击，不建袭击 Lord。attitude 强制写既有键，Neutral 回退只用于缺键旧档 |
| 明确不做 | 不改敌对派系全局关系、不解除闸门、不直接写 Fed、不强制离场、不接管袭击或拦截 Lord、不改变 ODF、事件 ID、API 或存档键 |
| Language | 无，沿用现有中英提示 |
| 存档 | 无新键；foodReceivers、foodDef、foodCount、foodWaitUntilTick、attitude 和 Lord faction 原键原类型；attitude 使用 forceSave 保留明确 Neutral，加载仅缺失 attitude 时回退到到达态度 |
| 归属表 | 无新增归属；集成者更新已有态度回退语义 |
| 回归 | 独立 runner 保留每人交接计数/截止边界、收齐不结束他人等待、等待清名单、明确 Neutral 真实存读再存读、Fight 关闸/儿童/非敌对拒绝及友好不降级。真实 Lord 的 ReadyToLeave、批次停战成员与 duty 保留、允许成人攻击及暴力能力过滤由隔离游戏烟雾观察（原版 Lord 与 WorkTagIsDisabled 依赖 Unity，不能在无 Unity runner 假调用 native）。本代理不运行检查 |
| 烟雾 | 触发 I-014/I-030，保持敌对观察成人攻击；关闭 Fight 后不安排反击；送粮观察整批蓝名中立、旧 Lord 成员数不变、停下旧任务；仅交一人时另一人仍等待，交齐后实际进食才 Fed；等待中存读保持中立；儿童即使覆盖 Fight=true 也不安排成人攻击 |
| 后续债 | 无；集成者更新 pawn.md、incidents.md、save-ownership.md 的对应现有行为说明 |

## 已执行验证

- 真实 I-014 交接入口返回成功，当前态度转 Neutral、到达态度保留 Hostile，原 Lord 对象及成员保持，实际 toil 为 `LordToil_RHAH_WaitFood`
- 保存重读 `siege-neutral-waiting` 后仍为 Neutral；预置历史离场截止的未交齐成员 `ReadyToLeave=false`。给其中一人交齐后，该人等待结束，另一人仍等待，整批未错误结算完成
- 最终真实游戏把可战斗成人放在殖民者附近，恢复敌对及寻食 toil；访客战斗入口返回真实 `AttackMelee`，目标为该殖民者，推进 120 tick 后此访客实际运行 `AttackMelee`
- 真实闸门覆盖拒绝 Fight=false、Child、Baby；暴力工作标签禁用时拒绝战斗，恢复标签后保留原值。交接等待和非敌对拒绝由回归及真实状态观察覆盖
- 最终 Release 552 项回归通过，中英日结构检查通过；未测所有围攻事件的长期战斗结局
