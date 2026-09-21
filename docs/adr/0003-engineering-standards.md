# ADR 0003: 工程标准

- Status: Accepted
- Date: 2026-09-20

## Context

脚手架已经有产品命名、事件目录和身份行为设计，但缺少统一的工程标准。此前的代码属于标准制定前的草案框架，不能作为目标架构依据。尤其是 `CompHungerPawn`、命名空间、API 边界、存档键和检查脚本之间存在相互矛盾。

需要一套可以承受 breaking change 的目标规范，明确稳定 API 与实现细节的边界，并覆盖构建、测试、发布、Harmony 联动和存档兼容。

## Decision

1. 活规范写在 [../engineering.md](../engineering.md)。`Agents.md` 只保留仓库指针和注释规则。产品名、显示 ID 和 Def 前缀仍只在 [../naming.md](../naming.md) 定义。
2. 当前草案实现不构成标准依据。发布前允许为落实本 ADR 进行 breaking change。
3. 对外契约使用独立的 `HungerAndHavoc.Api.dll`。实现程序集依赖 API 程序集，API 程序集不得反向依赖实现或游戏程序集。其它模组只依赖 API 程序集。
4. 稳定 API 使用公开类型白名单。`IHungerPawn` 是只读快照；查询、事件和行为扩展不得暴露 `CompHungerPawn`、Hediff、Job 或其它实现对象。
5. `CompHungerPawn`、`Hediff_HungerMark`、`HungerRaceExtension`、Worker、JobDriver、JobGiver、GameComponent 和 MapComponent 属于实现或 Verse 反射入口，不属于稳定 API。因反射必须 public 的类型需要登记例外。
6. 路径、命名空间和程序集职责必须一致。文件夹不再作为绕过程序集边界的理由。
7. 新存档键默认 camelCase。首次正式发布前允许重建实验性存档格式；首次正式发布后存档键冻结，后续变化必须兼容读取或显式迁移。
8. Harmony 默认只补原版或种族框架缺口。其它模组补丁默认禁止，例外必须登记目标、原因、兼容范围和失败策略。
9. 构建、测试、API、存档、XML、Def、语言键和关键补丁检查属于 CI 阻断级门禁。检查脚本缺失某项规则不代表实现符合标准。

本 ADR 修正此前把 `CompHungerPawn` 当作 Api 契约的草案方向。身份载体仍是 Hediff + Comp，但外部模组只能通过独立 API 程序集访问稳定快照和命令。M0 按 `Agents.md` 的规范解释执行。

## Consequences

- 实现必须先迁移程序集、命名空间和 API 边界，再补齐具体业务行为。
- 需要新增 API 项目、公开快照、稳定事件参数和鼠族判定注册入口。
- 存档字段变更在正式发布前可以重建；正式发布后必须维护迁移路径。
- 检查脚本和测试需要扩展为契约门禁，不能继续验证被本 ADR 淘汰的旧设计。
- 后续公开 API、存档格式和跨模组兼容变化必须先更新 `engineering.md` 与相关 ADR。
