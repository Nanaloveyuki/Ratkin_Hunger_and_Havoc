# 事件生成没有指向箭头

| 字段 | 内容 |
| --- | --- |
| 复现 | 新游戏或已有存档。调试触发会生成 pawn 并打开选择信的地图事件，例如 `I-005`、`I-015`。鼠标停在右上角信件上 |
| 期望 / 实际 | 期望和原版、旧鼠灾一样，悬停信件时地图上画出指向刚生成 pawn 的箭头。实际信件没有 `lookTargets`，悬停不画箭头，也没有跳转目标 |
| 版本 | 0.1.0 |
| 级别 | S2 |
| 根因 | `RHAH_IncidentFacts.SendLetter` 用三参数 `LetterMaker.MakeLetter`，不写 `ChoiceLetter.lookTargets`。原版 `LetterStack` 悬停时调用 `lookTargets.TryHighlight`，空目标不进 `TargetHighlighter` |
| 最小修复 | 发信前把本批已生成且未销毁的 pawn 放进 `lookTargets`。选择信和索取信同一条路径 |
| 明确不做 | 不给没有选择信的事件另写到达信。不改信件按钮、文案、超时和结算。不改穗音、粮洞、捕食和任务信已有的目标 |
| Language | 无 |
| 存档 | 无新键。`lookTargets` 是原版信件字段，随信件存档 |
| 归属表 | 否 |
| 回归 | `SendLetter` 把本批 pawn 交给 `LookTargets`，再交给两种选择信 |
| 后续债 | `I-001` 等不发选择信的事件仍没有箭头，因为本来就没有可悬停的信 |
