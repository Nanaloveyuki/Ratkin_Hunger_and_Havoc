# 温度衣被标成可熔炼

| 字段 | 写什么 |
| --- | --- |
| 复现 | 0.1.0 加载本模组，看 Player.log 配置错误 |
| 期望 / 实际 | 期望湿布、泥壳、麻布、毛皮等温度衣不可熔炼。实际 12 件各报两次 `is smeltable but does not give anything for smelting` |
| 版本 | 0.1.0 |
| 级别 | S2 |
| 根因 | 父节点 `ApparelNoQualityBase` 默认 `smeltable=true`。这 12 件没有 `costList`、`stuff` 或 `smeltProducts`，熔炼没有产物 |
| 最小修复 | `1.6/Defs/ThingDefs/RHAH_TemperatureApparel.xml` 的 `RHAH_TemperatureApparelBase` 设 `smeltable=false` |
| 明确不做 | 不补熔炼产物。不改隔热、生成或交易 |
| Language | 无 |
| 存档 | 无。Def 名不变，只改非持久字段 |
| 归属表 | 否 |
| 回归 | 基类含 `<smeltable>false</smeltable>`。游戏重载后这 12 条配置错误消失 |
| 后续债 | 无 |
