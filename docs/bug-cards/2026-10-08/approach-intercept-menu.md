# 同格右键不出现拦截鼠族

| 字段 | 内容 |
| --- | --- |
| 复现 | 玩家远行队与 `RHAH_Approach` 停在同一个地表格，选中玩家远行队后右键该格。菜单里没有「拦截鼠族」，不是被别的条目挡住 |
| 期望 / 实际 | 至少一支玩家远行队和鼠族远行队同格时，右键该格出现「拦截鼠族」，点下去进入临时地图 / 右键菜单里没有这一项 |
| 版本 | 1.0.4 开发版 |
| 级别 | S2 |
| 根因 | 原版 `WorldSelector` 右键只对选中的那一支玩家远行队调用 `FloatMenuMakerWorld.ChoicesAtFor`，而后者只收集 `GenWorldUI.WorldObjectsUnderMouse`。玩家 `Caravan` 的 `expandingIconPriority` 是 100，`RHAH_Approach` 是 90。图标展开时排序把玩家队放在最前并 `Reverse`，鼠标下第一项是玩家队。玩家队的 `GetFloatMenuOptions` 不转发同格其它物体，鼠族队因此不会被问到 |
| 最小修复 | 给 `FloatMenuMakerWorld.ChoicesAtFor(Vector2, Caravan)` 加 Postfix。鼠标下已有同格玩家队、但没有这支鼠族队时，用这支鼠族队已有的 `GetFloatMenuOptions` 把「拦截鼠族」补进同一份菜单。仍要求同格、玩家控制、地面层，以及原有的地图生成条件 |
| 明确不做 | 不改图标优先级，不改选中逻辑，不增加追踪、预约或穿梭机拦截，不改临时地图生成事务 |
| Language | 无。继续用 `RHAH_Approach_Intercept` 与 `RHAH_Approach_InterceptBlocked` |
| 存档 | 无 |
| 归属表 | 否 |
| 回归 | 规则测试覆盖同格纳入、异格排除、停用排除、太空层排除、菜单里已经有这支鼠族队时不重复 |
| 后续债 | 真实游戏里同格右键、进入临时地图仍需在游戏中看一次 |
