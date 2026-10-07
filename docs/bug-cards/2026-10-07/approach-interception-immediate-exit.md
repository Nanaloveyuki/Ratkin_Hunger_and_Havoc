# 拦截鼠族在出生边缘立即离图

| 字段 | 内容 |
| --- | --- |
| 复现 | 玩家同格拦截鼠族远行队；用户观察到生成的 pawn 直接离开地图，没有穿图 |
| 期望 / 实际 | 鼠族从出生边缘进入地图，穿到对边再离场 / 出生后立即离图 |
| 版本 | 1.0.4 开发版，上轮拦截修复后 |
| 级别 | S1 |
| 根因 | Lord 起始使用 ExitMapNearDutyTarget，原版 JobGiver 生成 exitMapOnArrival=true 的 Goto；JobDriver_Goto 在移动途中检查当前格，出生最外缘属于离图格，目标在对边也会立即 ExitMap |
| 最小修复 | 单个无存档数据 LordToil 按每名 pawn 的位置分配 duty；距对边出口超过原版穿图到达半径 10 格时使用 TravelOrWait，进入该半径后才使用 ExitMapNearDutyTarget；倒地成员不阻挡其余成员到达后离图 |
| 明确不做 | 不改出生边缘、人数、敌对派系、信任结算、世界移动、正常访客或原版 JobDriver；不新增全局 Harmony 拦截 |
| Language | 无 |
| 存档 | 不新增或改名字段；Lord 类型、exitCell 键及默认值不变；单 toil 索引仍为 0，无新 toilData |
| 归属表 | 既有 Lord Remove 规则不变 |
| 回归 | 修复前出生边缘、穿图中间与成员独立阶段断言失败；修复后通过。真实程序集烟测运行原版 JobGiver_GotoTravelDestination、JobGiver_ExitMapNearDutyTarget 与 JobDriver_Goto 的移动 preTick 和到达 action：旧 duty 首 tick 请求离图，新 duty 保留出生边缘和地图中央的 pawn，到达对边才请求离图；exitCell Scribe 存读后仍保持两阶段 |
| 后续债 | 无 Unity 进程，游戏画面和逐 tick 寻路仍需游戏内复测 |

烟测隔离地形可达、站格选取、Map.CanEverExit 和终端 TryExitMap（计数离图请求，避免触发 DLC、世界及 Unity）；实际执行自有 LordToil 的初始 duty 和周期更新、原版 JobGiver 的 Job 生成、原版 JobDriver_Goto 的离图条件与 Scribe。不证明真实游戏画面、逐 tick 寻路或世界 pawn 转移。

全量 441 项测试通过，包含 7 项穿图回归。`scripts/deploy.sh` 通过结构及中英日检查、API/主体/Guard Release 构建，127 个文件部署至 RatkinHungerAndHavoc 并逐文件 SHA-256 核对。游戏未运行。临时烟测脚本验证后删除，不随模组发布。
