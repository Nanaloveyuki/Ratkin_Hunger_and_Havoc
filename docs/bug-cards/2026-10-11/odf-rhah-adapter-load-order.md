# ODF 的 RHAH 行为适配器提前加载

| 字段 | 内容 |
| --- | --- |
| 复现 | 启用 ODF 和 RHAH，ODF 排在 RHAH 前；启动 RimWorld。ODF 的 LoadFolders 在 RHAH 已启用时立即扫描 Rhah/Assemblies/Oxygendsl.Rhah.dll，但 RHAH API 尚未加载 |
| 期望 / 实际 | 两个主体加载完成后注册行为适配器，Oxygen 闸门生效 / Verse.GetTypes 报 ReflectionTypeLoadException，TypeRef 01000011 无法解析，适配器未注册 |
| 版本 | 玩家报告 RHAH v1.0.6；该发布提交 af28301 的 API 与当前、本机部署和隔离安装 API 哈希一致；当前 ODF 0.0.1 同样受影响 |
| 级别 | S1：启动报错且行为联动失效；本次未观察到进程崩溃 |
| 根因 | ODF 框架先加载，却通过可选 LoadFolders 立即载入实现 RHAH API 接口的反向依赖程序集。Verse 在每个 DLL 载入后立即 GetTypes，未解析依赖的全局 fallback 返回 Assembly-CSharp；失败 TypeRef 在本进程中不能靠稍后加载 API 恢复。接口没有删除，适配器也不是仅旧构建出错 |
| 最小修复 | ODF 不再把 Rhah 加入 Verse 提前扫描目录；ODF 主体中的不引用 RHAH 的启动入口在静态启动完成后，按自身安装路径加载独立适配 DLL 并显式调用既有 Activate。删除适配 DLL 中已失效的自动启动入口；保持 ODF → RHAH 顺序和现有 API |
| 明确不做 | 不新增 API shim、不复制 RHAH API、不反转必需依赖、不改 IrisMenus 适配、不清玩家安装或存档；卸载 Lord/ThinkTree 警告与广播完整作业验收不在本修复范围 |
| Language | 无；新增的是开发日志，不是玩家 UI |
| 存档 | 无；不改任何存档字段、XML 类型或持久化归属 |
| 归属表 | 无需更新 |
| 回归 | 已失败的同 DLL 顺序烟测和真实游戏启动；修后覆盖 ODF 先加载、无 RHAH 启动、单次注册、脚本 allow/deny/abstain 与 pawn 显式覆盖优先 |
| 后续债 | 其它可选适配器独立按各自依赖顺序验收，不将本次结果泛化为全部外部模组兼容 |

## 修前证据

- `af28301:1.6/Assemblies/HungerAndHavoc.Api.dll` SHA-256：`2e938f6c7f537488883b7e3a07c3016e597fd196a68be9028da826f01964c6cf`，与当前、正式部署和隔离安装相同，包含 `IRHAH_PawnBehavior`
- 隔离 `Oxygendsl.Rhah.dll` 与当前 ODF 构建 SHA-256 均为 `4b48d815b989b70bb03db6ab47bd7b60fdb6a14b8c535da715f47a3b7291a9fb`
- `/mnt/c/RimCarbon14/rhah-adapter-loader-20261011/Player.log:39-47`：RimWorld 1.6.4871 在正常加载顺序中得到相同 `01000011` 异常
- 真实行为注册表仅包含 `RHAH_PawnCompatAdapter`，没有 `RhahBehaviorAdapter`；稍后查询后者仍得到 TypeLoadException
- 同组 DLL 的 Mono 顺序烟测：ODF 先载入失败，API 稍后加载仍失败；新进程 API 先载入成功

## 已执行验证

- ODF Release 构建零警告、零错误；既有 548 项行为检查通过；RHAH 结构、API 白名单、存档与语言门禁通过
- 永久 `Tests/AdapterLoading` 回归按实际发布 LoadFolders 扫描 ODF，API 随后才加载，真实类型解析和两次 Activate 仅注册一个策略通过；相同测试对修前安装失败于原 `01000011`，不是文本匹配或假接口
- 独立安装 `C:/RimCarbon14/rhah-adapter-fixed-game-20261011`，配置 `C:/RimCarbon14/rhah-adapter-fixed-20261011`：真实 RimWorld 1.6.4871 进程 792 正常 ODF→RHAH 顺序启动，日志无 ReflectionTypeLoadException / TypeLoadException，出现 `RHAH behavior adapter registered`
- 实际注册表依次包含本模组策略和 ODF 策略；重复 Activate 两次仍只有一个 ODF 适配器。加载双地图旧档 `adapter-fixture`，对真实标记访客 `Ratkin49530` 通过 ContentCompiler / NativeRuleRuntime 安装脚本，RHAH_Api.Allows 得到 deny=false、allow=true、abstain=默认 true；pawn 显式 allow 优先于脚本 deny，显式 deny 优先于脚本 allow。移除测试 provider、恢复样例规则并推进 120 tick
- 配置 `C:/RimCarbon14/odf-adapter-without-rhah-20261011`：仅停用 RHAH 及依赖它的示例，进程 35424 新游戏和 120 tick 通过；真实 AppDomain 程序集名单没有 `HungerAndHavoc.Api` 或 `Oxygendsl.Rhah`，日志无该类加载或适配初始化错误
- 两个测试进程均正常退出。共享旧隔离安装另有进程运行，所以复制了独立安装，未覆盖其它进程加载的 DLL、正式 Mods、原配置或玩家存档
- 证据：`tmp/odf-adapter-fix-receipts.jsonl`；两份配置目录的 `Player.log`；新 ODF Mod SHA-256 `c33a8d2f1185dbc7b654ca30e150622c889b0c2789a6848fbaed83035f63b40a`、适配 DLL `2c64c78194eb27708db484b5ab5adb9851ef8b2517a95b8e47ae97002285ee32`
- 更新须同时发布 ODF `LoadFolders.xml`、`Oxygendsl.Mod.dll`、`Rhah/Assemblies/Oxygendsl.Rhah.dll` 并完全重启；RHAH API 无改动。只更新 RHAH DLL 或只替换适配 DLL 不构成修复
