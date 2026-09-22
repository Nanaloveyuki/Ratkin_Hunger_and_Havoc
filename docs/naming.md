# 命名表

| 用途 | 取值 |
| --- | --- |
| 中文名 | 鼠族: 饥与祸 |
| 英文名 | Ratkin: Hunger and Havoc |
| packageId | nanaloveyuki.ratkin.hungerandhavoc |
| 命名空间 | HungerAndHavoc |
| 程序集 | HungerAndHavoc.dll |
| API 程序集 | HungerAndHavoc.Api.dll |
| Guard 程序集 | HungerAndHavocGuard.dll |
| Def / Keyed 前缀 | RHAH_ |
| Harmony Id | nanaloveyuki.ratkin.hungerandhavoc |

## 显示 ID

| 前缀 | 用途 | 示例 |
| --- | --- | --- |
| I- | 事件 | I-001 |
| N- | 剧情节拍 | N-001、N-004-R |
| E- | 结局 | E-01 |
| J- | 记录 | J-001 |
| R- | 揭示 | R-01 |
| H-A / H-Y | 成年/幼年经历 | H-A001 |
| T- | 自有特质 | T-001 |

`defName` 用可读英文，例如 `RHAH_LargeRefugeeWave`，不要写成 `RHAH_I001`。

禁止再使用 `MouseDisaster`、`RHH_`、事件号 `N-011+`。

## 源码文件

`Source/Pawn` 及 Def XML 文件名用 `RHAH_` 短名，避免 `HungerAndHavocVisitor...` 膨胀。C# 类型名仍是合法标识符，XML `Class=` 写全名。

| 种类 | 文件 | 类型 |
| --- | --- | --- |
| Lord | `LordJob_RHAH_Visitor.cs` | `LordJob_RHAH_Visitor` |
| ThinkNode | `ThinkNode_ConditionalRHAH_Visitor.cs` | `ThinkNode_ConditionalRHAH_Visitor` |
| 总 JobGiver | `JobGiver_RHAH_Visitor.cs` | `JobGiver_RHAH_Visitor` |
| 角色 JobGiver | `JobGiver_RHAH_Beg.cs` 等同名文件 | `JobGiver_RHAH_Beg`、`Feed`、`Gnaw`、`Steal`、`Leave`、`WaitFood` |
| JobDriver | `JobDriver_RHAH_Beg.cs` | `JobDriver_RHAH_Beg`、`JobDriver_RHAH_Gnaw` |
| 闸门查询 | `RHAH_VisitorGate.cs` | `RHAH_VisitorGate` |
| 组管理 | `RHAH_VisitorGroup.cs` | `RHAH_VisitorGroup` |
| 态度 | `RHAH_AttitudeFactions.cs`、`RHAH_AttitudePolicy.cs`、`RHAH_BatchAttitude.cs` | 五个隐藏派系、批次反应、整批离场 |
| 赈灾 | `Area_RHAH_Relief.cs`、`RHAH_ReliefFood.cs` | 赈灾区和取食规则 |
| Def XML | `1.6/Defs/{Job,Duty,ThinkTree,Faction,Hediff}Defs/RHAH_*.xml` | `defName` 仍是 `RHAH_Beg` 这类可读短名 |

禁止目录或类型名 `Behavior`。其它模组适配放 `Source/Pawn/Compat/`，不进基底 Visitor/Duty。


代码分层、字段与方法原则见 [engineering.md](engineering.md)。
