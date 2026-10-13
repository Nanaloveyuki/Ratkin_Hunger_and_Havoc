# 自动化与低噪声调试

统一入口：`python3 scripts/rhah.py`，快捷入口：`make`。Python 只使用标准库；.NET SDK 由根目录 `global.json` 固定。Windows 游戏目录默认 `E:\Apps\Steam\steamapps\common\RimWorld`，WSL 使用 `/mnt/e/Apps/Steam/steamapps/common/RimWorld`。

## 常用命令

| 命令 | 行为 |
| --- | --- |
| `make doctor` | 查看工具链、游戏、ODF 与发布版本前置条件 |
| `make check` | 结构、Def、语言键、API 与存档归属静态门禁 |
| `make tools-test` | 离线运行自动化工具的行为测试 |
| `make build` | Release 构建 ODF、主体及其 API、Guard |
| `make test` | 构建并运行现有 net48 测试 |
| `make ci` | 工具测试、静态检查、完整构建和游戏契约测试 |
| `make package` | 完整 CI 通过后制作发布 zip 和 SHA-256 清单 |
| `make deploy` | 完整 CI 通过后安全同步到本机 Mods |
| `make report` | 最近一次流水线的状态、阶段和证据路径 |
| `make logs` | 增量查看 Player.log 中与本项目有关的内容 |
| `make carbon-status` | 独立测试进程、桥接与主线程状态 |
| `make carbon-tools` | 按需发现运行时工具，不展开全部 schema |
| `make carbon-events` | 增量事件、重复聚合及过滤计数 |
| `make carbon-smoke` | 对已经打开的测试地图运行只读项目场景 |
| `make mcp-config` | 生成项目适配器的 MCP 客户端配置 |

需要参数时直接调用 Python，例如：

```bash
python3 scripts/rhah.py ci --rimworld-dir /mnt/e/Apps/Steam/steamapps/common/RimWorld
python3 scripts/rhah.py test --tools-only
python3 scripts/rhah.py package --release-tag v1.1.0
python3 scripts/rhah.py logs --match 'HungerAndHavoc.*Exception|RHAH_' --limit 5
```

`build` 不是完整门禁；只有完整 CI 成功才允许部署或打包。没有绕过检查的发布参数。发布版要求 `About/About.xml` 的 `modVersion` 与主体项目 `Version` 一致，tag 必须是对应的 `v<版本>`；API 的独立契约版本、稳定 `AssemblyVersion` 不随内容版本强制改变。版本不一致时报告失败，不自动修改版本号。

环境覆盖：`RIMWORLD_DIR`（也接受 `RimWorldDir`）、`OXYGEN_DSL_DIR`、`CONFIGURATION`、`HARMONY_PATH`。ODF 默认使用同级 `Oxygen-Dsl-Framework` 仓库；完整 CI 先构建该仓库的 `Source/Mod/Oxygendsl.Mod.csproj`。不要将未经审核的外部 checkout 作为受保护 runner 的依赖。

## CI/CD

公共 GitHub Actions 在 push 和 PR 上执行 Actions schema 检查、离线工具测试与静态门禁。actionlint 固定为 1.7.7，自定义 runner 标签登记于 `.github/actionlint.yaml`。它不接触游戏安装、不下载或上传游戏 DLL，不启动游戏。分支保护应要求公共检查通过。

完整流水线通过 `workflow_dispatch` 手动触发，运行在标有 `rhah` 的 Linux x64 self-hosted runner 上，使用受保护的 `rhah-local` environment。它只 checkout 当前仓库默认分支，不接受任意 ref，也不在 PR 或 `pull_request_target` 上执行。该 runner 需要 Python 3、.NET 10.0.401、Mono、游戏程序集、Harmony、审核过的 ODF 和可选 IrisMenus 安装。

工作流文件为 `.github/workflows/static.yml` 和 `local.yml`。在 GitHub 仓库设置中登记 runner，为 `rhah-local` 和发布用的 `rhah-release` environment 设置审批人及允许的默认分支。`rhah-local` 变量使用 `RIMWORLD_DIR`、`OXYGEN_DSL_DIR`、`HARMONY_PATH`；运行时烟雾另设 `CARBON_PROFILE` 为已准备的独立 profile。源码中的工作流不会自动注册 runner 或创建远端审批规则。WSL runner 必须能够执行 `powershell.exe`，部署时用它检查 Windows 游戏进程

完整工作流可选择 `ci`、`package`、`deploy`、`release`。`runtime_smoke` 只允许与 `deploy` 一起启用，确保游戏观察的是本次部署；它启动独立 profile、创建新测试地图、执行只读场景，最后只停止该 profile 拥有的进程。已有测试游戏仍运行时拒绝接管。未启用时不声称通过运行时验收。`release` 要求既有 `v<版本>` tag 正好指向默认分支本次 checkout 的 HEAD，完整门禁成功后才经 `rhah-release` 发布 GitHub Release

发布与部署共用本地门禁。发布产物只包含 `About`、语言、Guard、`1.6`、Biotech、纹理、加载配置与许可证等白名单内容；三个本模组 DLL 必须存在。第三方程序集、PDB、源码、存档、发现令牌和原始测试日志不得入包或公共 artifact。需要 Steam Workshop 发布时，仍通过游戏的 Workshop 上传流程，不在 CI 中保存 Steam 登录凭据。

部署只写 `Mods/RatkinHungerAndHavoc` 对应的目标，拒绝游戏运行中、进程检查失败、链接目标、未知身份或其它模组身份。同步前再次检查进程，逐文件核对 SHA-256；清理范围限于本模组的白名单发布内容。不会自动退出游戏或写入旧鼠灾目录。原来的 `scripts/deploy.sh` 与 `scripts/build-and-deploy.ps1` 转发到同一实现，不维护第二套复制清单。

## RimCarbon14

适配依据本机 `/root/repos/csharp/RimCarbon14` 的真实 Agent 与协议。调用已安装的 `Mods/RimCarbon14/Tools/win-x64/RimCarbon14.Agent.exe`；没有给 RHAH 增加运行时依赖、Harmony 补丁或新的 HTTP 服务。

工具发现使用 RimCarbon14 原协议的认证 `GET /v1/tools`，避开上游 Agent CLI 对工具数组的错误索引。WSL 通过只读 `scripts/carbon/read-tools.ps1` 在 Windows 本地请求 `127.0.0.1`；不将宿主 IP 或外网地址替换为桥接端点。Discovery 协议、端口和认证先校验，禁止代理与重定向；token 由 helper 本地读取，不进入命令行。call、场景和进程控制仍交给真实 Agent

默认测试 profile：`E:\RHAHAutomation\RatkinHungerAndHavoc\profile`，WSL 对应 `/mnt/e/RHAHAutomation/RatkinHungerAndHavoc/profile`。这是独立存档目录，不是 `.rws` 文件。`prepare` 不覆盖已有配置，不修改玩家日常 profile；进程控制只允许适配器标记的独立 profile，Agent 再按 PID、启动时间和 executable 校验所有权。

第一次准备与实测：

```bash
python3 scripts/rhah.py carbon doctor
python3 scripts/rhah.py carbon prepare --minimal
python3 scripts/rhah.py carbon start
python3 scripts/rhah.py carbon run scripts/carbon/rhah-new-test.json
python3 scripts/rhah.py carbon run
python3 scripts/rhah.py carbon stop
```

`rhah-new-test.json` 显式创建测试殖民地并捕图；默认 `rhah-smoke.json` 只读检查已有地图、本模组及其 API/依赖加载。两者不是所有事件、王国篇、卸载或第三方兼容性的完整验收。改玩法时另写有结果断言的实际场景，不能以 `accepted`、方法没抛异常或截图文件非空代替玩法结果。

进程启动与配置复制属于显式操作，不随查询自动执行。游戏需要真实渲染窗口才能捕图；不要在玩家用鼠标时执行 desktop input。存档和实例切换后重新获取对象 handle。

```bash
python3 scripts/rhah.py carbon tools --query objects
python3 scripts/rhah.py carbon tools --name objects_types
python3 scripts/rhah.py carbon call game_state --select result.data.status
python3 scripts/rhah.py carbon events --all --limit 10
python3 scripts/rhah.py carbon operation <operation-id> --instance <original-instance-id>
python3 scripts/rhah.py carbon config
```

工具必须先查真实 schema，再提供精确参数。长调用显式设置超时；写调用失败或 acknowledgement 丢失后查 operation，不自动重放。事件 cursor 按实例隔离；`lost` 或截断计数意味着观察不完整，不能据此声称没有错误。

适配器接受 `CARBON_AGENT`、`CARBON_PROFILE`、`CARBON_SOURCE_DIR`，以及命令行 `--agent`、`--profile`、`--game-root`、`--evidence-dir`。WSL 自动转换传给 Windows companion 的 profile、游戏、参数文件与场景路径。`carbon config` 输出标准 MCP 配置；客户端只接入这个精简适配器即可，避免再同时加载上游全部工具定义。

## 输出与证据

默认只把阶段摘要、有限错误片段、操作状态和证据路径送进上下文。Carbon 结果限制递归深度、数组项数、字符串长度和总响应大小；省略部分带计数。需要某个值时用 `--select` 精确取 JSON 路径，需要工具参数时只查一个 schema。截图和完整结果保存在文件，不向文本摘要塞 base64。

流水线原始输出与 `run.json` 在 `tmp/automation/`；`latest.json` 指向最近的报告，不抹掉较早失败。Carbon 的原始响应和场景证据也留在本地，token 做脱敏。不要提交或批量打开这些目录，不将原始日志上传公共 CI artifact。

Player.log 默认扫描最近 256 KiB，此后按 cursor 读取新内容；记录扫描范围、未扫描历史与剩余字节。日志轮转或覆盖会重置 cursor。过滤保留本项目及 ODF 相关诊断、聚合重复堆栈，启动模组大清单默认省略；其它模组诊断报告数量，需追查时使用 `--all` 或 `--match`。过滤后的证据仍可能包含存档内容，按本地敏感数据处理。

Agent 工作习惯：先看 `doctor` 或上次 `report`，只运行受影响门禁一次；失败后按报告里的具体文件和行范围读证据。不要为了找一句错误反复展开整个 Player.log、所有运行时类型、所有 tool schema 或全部 trace。自动化摘要不等于完整验收证明，最终交付只报告实际执行的检查与场景。

## 本次验证记录（2026-10-13）

- Actions workflow 经 actionlint 1.7.7 校验通过；71 项标准库工具行为测试通过。MCP 实际 stdio 初始化、7 工具发现和 doctor 调用通过，工具定义响应约 3.4 KiB，doctor 摘要约 1.3 KiB
- 独立 profile 实际准备、启动和正常停止通过。已部署模组的只读场景 7 步通过，新测试地图及捕图场景 10 步通过；WSL Windows helper、单工具 schema、game_state 投影和增量事件已执行。真实 1280×720 截图在 `tmp/automation/carbon/scenario-2c9e548faeb740fd967d56e7652cb130/real_rendered_capture.png`。没有覆盖玩家日常 profile，没有使用 force 停进程
- 打包白名单、ZIP 哈希和部署同步在可丢弃目录中用实际已安装 DLL 烟雾通过；游戏运行时部署被拒绝，退出后同步通过。临时发布包已删除。这只证明自动化工具，不是当前源码发布包验收
- 当前完整流水线如实失败：`tmp/automation/20261013T090909Z-package-02ef6a80/run.json` 记录王国篇新增翻译、DefInjected 与存档归属缺失；`tmp/automation/20261013T090941Z-test-150a3115/run.json` 记录主体 12 条编译错误，包括 Caravan 命名空间冲突和到达动作缺少原版 override。ODF 构建成功，主体失败后未执行后续 net48 测试。未修改这些同时进行的玩法改动，未绕过门禁部署当前源码
- 远端 Actions、runner 注册、environment 审批与 GitHub Release 未执行，需要仓库管理员按上面的配置启用
