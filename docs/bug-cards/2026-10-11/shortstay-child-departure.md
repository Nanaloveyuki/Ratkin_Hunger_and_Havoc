# 到期短工孩子恢复访客身份与离场

| 字段 | 内容 |
| --- | --- |
| 复现 | 收留包含幼年来客的短工批次，推进至期限；对年龄导致 alwaysDowned 的孩子观察期限暂停；加载已有仍为玩家派系的 Leaving 活动访客孩子；把短工永久加入后推进原期限 |
| 期望 / 实际 | 非永久招募的临时成员到期恢复其当前态度访客派系，与同批到期成员共用照护 Lord；正常收养或 Released 幼年殖民者保持原状 / 所有 Downed（包含原版年龄倒地）无限暂停；到期清成 null 派系并拆 Lord，孩子失去照护组；永久释放没有清 stayKind，旧期限仍能被运行时执行 |
| 版本 | 玩家反馈 v1.0.6（ODF 更新前），修复基于当前 ODF 仓库 |
| 级别 | S1 |
| 根因 | 临时身份把医疗倒地与年龄不能行走当成同一暂停条件；到期没有回到态度派系和批次照护组；临时计时与终态 Released 缺少清理归属。原版 ShouldBeDowned 包括 CurLifeStage.alwaysDowned，Moving worker 对该阶段直接返回 0，所以健康婴儿的零移动能力不能当作医疗倒地证据。临时玩家派系仍是活动标记，进食钩子会覆盖停留期限；逐索引扫描 AllPawnsSpawned 时原版 SetFaction 注销/重新登记会跳过成员 |
| 最小修复 | 临时计时只对医疗原因倒地暂停；暂停期间滚动既有期限以保存剩余时间，年龄倒地不暂停。到期恢复态度访客派系、Leaving，清既有 stayKind/计时与本模组衣物锁，加入同批访客离场 Lord。小时检查用池化稳定名单含抱持孩子，避免 SetFaction 改列表跳人；临时停留进食不结算访客 Fed、不覆盖期限。历史已清 stayKind 的玩家派系 Leaving 活动访客恢复相同路径；Released、Dead、囚奴及无标记者不处理。永久 Released 时清临时计时 |
| 明确不做 | 不按年龄把玩家所有孩子驱离，不重新激活 Released，不猜测永久收养记录，不造照护者，不绕过 Carry/ExitMap，不改变医疗倒地暂停规则、ODF 或存档键 |
| Language | 修改既有中英日 `RHAH_Settings_ShelterDays_Tooltip`、`RHAH_Settings_HireDays_Tooltip`、`RHAH_Stay_InspectPaused`，说明医疗倒地暂停与年龄不能行走正常到期；不新增键 |
| 存档 | 无新字段/键；复用 lifecycle、stayKind、leaveAfterGameTick、stayRemainingTicks、attitude 和 shortStayLockedApparel。旧档活跃短工按已有期限继续；旧档玩家派系 Leaving 仅修复身份不重写终态；永久 Released 以终态优先 |
| 归属表 | 无新增归属；集成者补充永久释放清理临时计时语义 |
| 回归 | 独立 runner 添加年龄倒地/医疗倒地政策边界、医疗暂停与到期边界、Released 优先且永久释放清计时、进食不覆盖期限、合法旧档恢复与囚奴排除边界。真实到期派系与共享 Lord 恢复、旧档 Leaving 卡住成员恢复由隔离游戏烟雾覆盖，原版 Lord 依赖 Unity 不在独立 runner 假调用。本代理不运行检查 |
| 烟雾 | 新档收留成年人和幼年来客，期限前白名且无访客任务，期限到恢复正确访客派系并蓝名离场；年龄不能行走的孩子由同组 Carry 合格成人携出，医疗倒地保持暂停；加载旧档玩家派系 Leaving 孩子恢复；永久加入及普通非本模组殖民者不受影响 |
| 后续债 | 无；集成者同步 pawn.md、save-ownership.md 既有归属语义 |

## 已执行验证

- 真实 I-004 家庭短工复现原版婴儿 Moving=0 导致错误暂停；补正能力判断后加载同一 `baby-expiry-before-capacity-fix`，健康婴儿 `PausesStay=false`
- 调用真实到期入口后，家庭三名成员均为 Leaving，并恢复同一个访客 Lord；成人取得真实 `RHAH_CarryYoung`
- 推进 600 tick 后，这三名到期成员全部离开地图，原殖民者与其它鼠族仍在。没有创建虚假照护者或瞬移孩子
- 永久 Released、医疗暂停、旧档活动 Leaving 恢复和进食不覆盖期限由最终 552 项回归中的行为与边界测试覆盖；没有把独立 runner 当成 Unity 移动验证
- 医疗暂停和年龄不能行走的中英日提示已同步，结构与占位符检查通过
