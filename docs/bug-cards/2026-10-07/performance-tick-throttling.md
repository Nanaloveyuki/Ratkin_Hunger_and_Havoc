# 性能：游戏组件 Tick 热路径

## 复现

启用本模组并进入有访客或叙事状态的存档，使用游戏内性能查看器观察 `GameComponent_RHAH_Game.GameComponentTick`、`RHAH_AttitudeFactions.LockGoodwill`、`PinPair` 和叙事运行时 Tick。

## 期望 / 实际

期望：没有到期工作时，游戏组件只承担轻量调度；稳定派系关系不应每个游戏 Tick 重复解析和重锁。

实际：`GameComponentTick` 每 Tick 调用多个已有 due 闸的运行时，并且 `LockGoodwill` 每 Tick 重复 `FirstFactionOfDef` 与关系查找。

## 版本

1.0.0，2026-10-07

## 级别

S3

## 根因

全局组件把所有运行时入口放在每 Tick 调用，派系解析缓存数组已存在但 `Resolve` 未使用；关系锁定没有游戏内时间闸。

## 最小修复

在 `RHAH_AttitudeFactions` 内按当前 `FactionManager` 缓存 5 个派系解析结果，并让游戏组件按游戏内小时执行关系锁定。保留每 Tick 的截止时间调度入口。

## 明确不做

不改事件频率、叙事状态语义、存档键、派系定义或外部模组补丁；不把已有精确截止 tick 的运行时改成小时轮询。

## Language

无

## 存档

无

## 归属表

不更新

## 回归

测试派系解析缓存仍返回对应 Def 的非玩家派系；验证结构检查、构建、测试，并在游戏性能查看器确认 `LockGoodwill` 调用频率下降。

## 后续债

若性能采样仍显示 `PinPair` 为热点，再单独评估关系对象缓存与外部派系变更通知的契约。
