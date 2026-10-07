# 拦截地图无鼠族、提前判胜与后续袭击

| 字段 | 内容 |
| --- | --- |
| 复现 | 玩家远行队与鼠族远行队同格后拦截；用户观察到地图没有鼠族、立刻收到胜利信及下一波袭击倒计时；重组远行队后再拦截受旧临时地图阻挡 |
| 期望 / 实际 | 同格才显示菜单，敌对鼠族从最外边缘穿越到对边离场，结束原事件并将穗音成功判定设为否、每批扣 2 信任，可再次拦截 / 原 Worker 使用各自地图解析和空投流程，原版战场提前判胜并启动袭击倒计时 |
| 版本 | 1.0.4 开发版 |
| 级别 | S1 |
| 根因 | 拦截复用原事件 Worker，不能保证事件在临时地图即时落地；父物体 AttackedNonPlayerCaravan 的 CaravansBattlefield.PostMapGenerate 固定启动 TimedDetectionRaids，CheckWonBattle 在无活动敌人时发胜利信；AnyMapParentAt 对已结束临时遭遇也一律拒绝 |
| 最小修复 | 独立 Encounter 父物体，无袭击组件和判胜；直接通过既有 PawnFactory 生成红名鼠族，独立 Lord 指定对边出口；终态结算失败及 -2 信任；结束临时地图可复用且无玩家成员后按原版规则删除；复用已有派系缓存 |
| 明确不做 | 不修改正常到家事件、事件编号、世界移动、其它模组补丁；不迁移已经生成的原版战场，不复制参考模组源码 |
| Language | 新临时地图 Def 正文及英日 DefInjected |
| 存档 | 新父物体 displayId、spawnBatchId、settled；新 Lord exitCell；不改旧键；既有战场照原版继续读取 |
| 归属表 | 新 Def 和父物体有地图时 Replace 为原版 Site，无地图时 Remove；Lord Remove；保留地图、殖民者及 retainedCaravanData |
| 回归 | 相关 44 项与全量 434 项测试通过；真实程序集隔离烟测通过 TrySpawn/CompleteInterception、I-032 即时边缘生成和对边 duty、连续两批同 tick 拦截、终态只扣二信任、无救济计数和通知、生成失败、玩家成员阻挡删图、同格允许/异格隐藏/销毁闸门、journal 与 Lord 出口 Scribe 重读；部署记录见下文 |
| 后续债 | 无 Unity 游戏进程中的实际画面、全模组列表与自动保存验证须由游戏内复测完成 |

烟测隔离 Unity 图形加载、完整 PawnFactory、站格与地形可达查询、玩家入图外部调用和攻击目标缓存；实际执行自有生成事务、边缘选点、Lord 创建及 duty 分配、父物体结算和 Scribe。Mono 输出 Unity 内部调用提示，未证明游戏画面或原版逐 tick 寻路。临时脚本验证后删除，不随模组发布。

`scripts/deploy.sh` 通过结构与中英日校验、API/主体/Guard Release 构建，127 个文件部署到 RatkinHungerAndHavoc 并逐文件 SHA-256 核对。游戏未运行。既有原版战场不迁移，需从拦截前存档复测新路径。
