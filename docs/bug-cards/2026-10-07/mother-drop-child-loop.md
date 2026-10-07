# 2026-10-07 乞讨母亲抱婴儿离场循环报错

| 字段 | 写什么 |
| --- | --- |
| 复现 | 玩家反馈乞讨鼠族母亲抱着婴儿离场时循环报错。提供的栈从 `JobDriver_RHAH_DropChild` 进入 `Pawn_CarryTracker.TryDropCarriedThing`，报 `container tried to drop null which it didn't contain`。针对性回归构造孩子仍指向母亲的 holdingOwner、但母亲真实抱持容器为空或装着另一对象的状态，执行实际放下 toil |
| 期望 / 实际 | 期望离场时按家庭弃养开关放下当前实际抱着的已登记孩子，成功后记账并移出访客 Lord。实际用孩子的 `CarriedBy` 推断容器内容，空容器仍进入放下调用，失败后没有 dropped 标记，离场调度再次派发 |
| 版本 | 玩家反馈版本未提供；仓库当前 1.0.4 开发中，2026-10-07 |
| 级别 | S1 |
| 根因 | JobGiver 和 Driver 的抱持校验只依赖孩子的 holder 引用，不核对母亲 `carryTracker.CarriedThing`。提供的栈证明调用时真实容器为空；holder 与容器失配的具体产生者尚未确定。另有确定的本地生命周期错误：`RHAH_DropChild` 未关闭原版 `dropThingBeforeJob=true`，`Pawn_JobTracker.StartJob` 会在 toil 之前自行放下孩子，绕过成功记账和拆 Lord |
| 最小修复 | `RHAH_DropChild` 设置 `dropThingBeforeJob=false`，由本任务负责放下。派发和执行同时要求目标孩子是实际 `CarriedThing` 且 `CarriedBy` 指向母亲；不合格目标不调用放下，不记录弃养、不拆 Lord。仍只在原版放下成功、孩子已生成且无抱持者后记账和拆 Lord |
| 明确不做 | 不改离场条件、弃养开关、携出 Job、派系、API 或其它模组补丁。不修玩家当前其它地图生成与 smoke 问题。不靠限流、吞日志或伪造 dropped 标记终止循环 |
| Language | 无，未改玩家可见文案 |
| 存档 | 无，保留所有现有键、Def 名与 XML 类型名 |
| 归属表 | 不更新，未新增或改变持久化产物 |
| 回归 | 修复前，空容器回归实际进入 `ThingOwner.TryDrop(null)` 的报错路径，错误对象回归实际进入另一 Pawn 的落地路径，两项均失败。修复后新增 5 项覆盖真实双向抱持、holder 失配、缺目标或 tracker，以及空容器和错误对象下执行实际放下 toil 不记账、不拆 Lord；连同已有家庭与携出测试共 16 项通过。独立 Mono smoke 实际调用 JobGiver，验证空容器拒绝、真实已登记孩子获得任务、弃养关闭不派发；原版 XML loader 读取修改后的 flag，使用观测 driver 执行真实 `Pawn_JobTracker.StartJob` 至开工前放下阶段之后，确认孩子仍在容器中。结构检查和相关 LSP 诊断通过，测试命令完成主体与 API 构建。未验证真实游戏落地、完整 toil 调度与离场动画；无引擎环境不能运行这些路径 |
| 后续债 | 未收到反馈玩家的存档和完整补丁列表，无法确定 holder 与容器失配由哪个加载或兼容路径产生；本次保证本模组不从失配状态派发或执行放下 |
