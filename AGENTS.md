## 先读

后来的 agent 先打开本页，再按任务打开对应文档。不要在仓库外重新搜索这些路径，也不要把旧鼠灾仓库的源码拷进本仓库。

规范冲突时以 `docs/engineering.md` 为准。玩法只对照旧仓库，禁止拷贝其源码、存档字段或 `MouseDisaster` 命名。

## 本仓库

- 显示名：`鼠族: 饥与祸` / `Ratkin: Hunger and Havoc`
- `packageId`：`nanaloveyuki.ratkin.hungerandhavoc`
- Def / Keyed 前缀：`RHAH_`
- 命名空间：`HungerAndHavoc`
- 版本目录：`1.6/`。以后加 `1.7/`，不要把 Def 或程序集放到仓库根
- 部署目录：`/mnt/e/Apps/Steam/steamapps/common/RimWorld/Mods/RatkinHungerAndHavoc`
- 禁止写入：`/mnt/e/Apps/Steam/steamapps/common/RimWorld/Mods/RatkinGreatFamineYearContinued`

| 要查什么 | 路径 |
| --- | --- |
| 工程标准、程序集、public 例外 | `docs/engineering.md` |
| 名称、显示 ID、文件名 | `docs/naming.md` |
| 事件目录 `I-001`..`I-051` | `docs/incidents.md` |
| Pawn 身份、闸门、生命周期 | `docs/pawn.md` |
| 外部模组兼容 | `docs/compatibility.md` |
| 存档键与卸载归属 | `docs/save-ownership.md` |
| 修 bug 的范围 | `docs/bug-handling.md` |
| 路线与当前进度 | `docs/project-goals.md` |
| 决策记录 | `docs/adr/` |
| 面向玩家的说明 | `README.md` |
| 构建并部署 | `scripts/deploy.sh` |
| 结构检查 | `scripts/verify-scaffold.py` |

源码：

| 层 | 路径 |
| --- | --- |
| 稳定 API | `Source/Api/` → `HungerAndHavoc.Api.dll` |
| 实现 | `Source/` → `HungerAndHavoc.dll` |
| 入口与设置 | `Source/Core/ModEntry.cs`、`Source/Core/HungerAndHavocSettings.cs` |
| 事件目录 | `Source/Incidents/HungerIncidentCatalog.cs` |
| 访客与兼容 | `Source/Pawn/`、`Source/Pawn/Compat/` |
| IrisMenus 页面 | `Source/Pawn/Compat/RHAH_IrisMenusCompat.cs`、`Source/Pawn/Compat/RHAH_IrisMenusWidgets.cs` |
| 叙事状态 | `Source/Narrative/NarrativeState.cs` |
| Guard | `Guard/Source/` |
| 测试 | `Source/Tests/` |
| 当前 Def | `1.6/Defs/` |
| 中英 Keyed | `Languages/ChineseSimplified/Keyed/`、`Languages/English/Keyed/` |

玩家可见文案先读 `skill://rimworld-writing`。该技能的原版对照在 `/mnt/d/References/Rimworld/Vanilla/`。

## 本机路径

Windows 路径给资源管理器和 PowerShell。WSL 里用 `/mnt/...`。

| 用途 | Windows | WSL |
| --- | --- | --- |
| 游戏本体 | `E:\Apps\Steam\steamapps\common\RimWorld` | `/mnt/e/Apps/Steam/steamapps/common/RimWorld` |
| 已部署的本模组 | `E:\Apps\Steam\steamapps\common\RimWorld\Mods\RatkinHungerAndHavoc` | `/mnt/e/Apps/Steam/steamapps/common/RimWorld/Mods/RatkinHungerAndHavoc` |
| 已安装 IrisMenus | `E:\Apps\Steam\steamapps\common\RimWorld\Mods\IrisMenus` | `/mnt/e/Apps/Steam/steamapps/common/RimWorld/Mods/IrisMenus` |
| 游戏日志 | `C:\Users\miaom\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Player.log` | `/mnt/c/Users/miaom/AppData/LocalLow/Ludeon Studios/RimWorld by Ludeon Studios/Player.log` |
| 原版反编译 | `D:\References\Rimworld\Vanilla\Game` | `/mnt/d/References/Rimworld/Vanilla/Game` |
| 原版 DLC Def | `D:\References\Rimworld\Vanilla\DLCs` | `/mnt/d/References/Rimworld/Vanilla/DLCs` |
| NewRatkinPlus | `D:\References\Rimworld\Mods\NewRatkinPlus` | `/mnt/d/References/Rimworld/Mods/NewRatkinPlus` |
| NewRatkinPlus 简中 | `D:\References\Rimworld\Mods\NewRatkinPlus_zh` | `/mnt/d/References/Rimworld/Mods/NewRatkinPlus_zh` |
| 旧鼠灾，只对照玩法 |  | `/root/repos/Ratkin-Great-Famine-Year-Continued` |
| IrisMenus 源码与公开 API |  | `/root/repos/IrisMenus` |

一键部署用 `scripts/deploy.sh`。它在 WSL 里做结构检查、Release 构建，并把 About、Languages、Guard、1.6、Biotech 同步到本机 `Mods/RatkinHungerAndHavoc`，按 SHA-256 核对。游戏进程 `RimWorldWin64` 存在时拒绝覆盖。游戏目录用 `RIMWORLD_DIR` 或 `RimWorldDir`，缺省是上面的 `/mnt/e/Apps/...`。`scripts/build-and-deploy.ps1` 默认指向另一台机器的 `D:\Appdata\...`，本机 Windows 也没有 Python 和 .NET SDK，不要用它部署。

IrisMenus 公开 API 在 `/root/repos/IrisMenus/Source/MenuRegistry.cs` 和 `MenuControls.cs`。接入说明是同仓库的 `guide.md` 与 `guide_agents.md`。它的 About 没有 `modVersion`，用 `supportedVersions` 的 1.6 判断。

## 注释

需要注释时用短中文，句末不加标点。

例：`建议使用 Ratkin Young 字段而非 Ratkin Egg 字段来表示鼠蛋`

只在意图、约束或之后会丢的事实上注释。

## Bug fix

先按 `docs/bug-handling.md` 写修复卡，再改代码。

同一次改动只包含根因路径、它的回归测试，以及这次修复迫使更新的 Language、存档键和归属表。

不要顺手改存档键、挪命名空间、扩大 API 或格式化无关文件。

玩家可见字符串必须同时有 ChineseSimplified 和 English Keyed。Def 正文保持中文，英文走 DefInjected。

存档键和 XML 类型名是契约。1.0.0 前的破坏性重建必须写在修复卡上。新的持久化 Def 或类型登记到 `docs/save-ownership.md`。

## 当前进度

版本 `0.1.0`。M0 到 M3 的目录、生成、访客、调度和 IrisMenus 页面已经落地。访客按五个态度派系活动，伤害和驱逐改整批态度并离场。赈灾区限制取食。携带鼠疫的来客进入检疫，检疫中不能加入、雇佣或转移。`NarrativeState` 保存计数、结局计算，以及 `N-001`..`N-010` 和 `R-01` 的开关、开始和期限。事件会记下事实，但还不会推进剧情。事件频率、基因页、121 条经历和 50 条特质已落地。结局开关还没有。

已落地：

- `HungerAndHavoc.Api.dll` 与实现分离。其它模组只引用 API
- `IHungerPawn` / `HungerPawnSnapshot`。API 不传 Comp、Hediff 或 Job
- `CompHungerPawn` 在 `HungerAndHavoc.Identity`。存档键 `sourceIncidentDisplayId`
- 事件目录 `I-001`..`I-051`，Def 在 `1.6/Defs/IncidentDefs/`
- 访客 Lord、Job、Duty 和 ThinkTree 在 `Source/Pawn/` 与 `1.6/Defs/`
- `GameComponent_HungerAndHavoc`、`MapComponent_HungerAndHavoc`
- IrisMenus 1.6 的 15 个 SubItem。可选依赖，缺失时不注册
- Guard 与 `LoadFolders.xml` 在旧鼠灾包启用时跳过主体

不要改 `packageId`、显示名、Harmony Id、Guard 冲突列表，也不要改已登记的 51 个事件显示 ID。

不要再建 `Source/Behavior`。Generation、Narrative、Pawn、Data 已有类型，不要为了规划再建空目录。

## 验证

```
python3 scripts/verify-scaffold.py
dotnet build Source/Api/HungerAndHavoc.Api.csproj -p:RimWorldDir=/mnt/e/Apps/Steam/steamapps/common/RimWorld
dotnet build Source/HungerAndHavoc.csproj -p:RimWorldDir=/mnt/e/Apps/Steam/steamapps/common/RimWorld
dotnet build Guard/Source/HungerAndHavocGuard.csproj -p:RimWorldDir=/mnt/e/Apps/Steam/steamapps/common/RimWorld
dotnet test Source/Tests/HungerAndHavoc.Tests.csproj -p:RimWorldDir=/mnt/e/Apps/Steam/steamapps/common/RimWorld
```

部署用 `scripts/deploy.sh`，不要在游戏运行时覆盖。IrisMenus.dll 是 net48 可选引用，`Private=False`，不要打进 `1.6/Assemblies/`。

不要提交 `bin/`、`obj/`、pdb、`1.6/Assemblies/0Harmony.dll`。Harmony 是模组依赖，不是本仓库程序集。