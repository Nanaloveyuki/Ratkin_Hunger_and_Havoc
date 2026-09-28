# 鼠蛋被收成同一种发型

| 字段 | 内容 |
| --- | --- |
| 复现 | 新游戏触发带未满 4 岁来客的事件，例如鼠蛋遗弃。金鼠等成年任务在 0.2 后发型已分开 |
| 期望 / 实际 | 期望鼠蛋和满 4 岁的鼠族一样，按性别从 `RK_Style` 发型里重抽。实际未满 4 岁一律写成无发，头发节点再把图形清掉，看起来全是同一种 |
| 版本 | 0.2 |
| 级别 | S2 |
| 根因 | `ApplyHair` 在未满 4 岁时直接写成 `HairDefOf.Bald`。`RHAH_RatkinHairGraphicPatch` 再按同一年龄把 `PawnRenderNode_Hair.GraphicFor` 清成空。NewRatkinPlus 的 `Ratkin` 没有婴儿专用发型，`RK_Style` 对婴儿阶段同样可用。原版只藏人类婴儿阶段的头发，不该按 4 岁再藏一次 |
| 最小修复 | 去掉无发捷径和头发图形补丁。未满 4 岁也按性别从带标签的发型里重抽。缺年龄时不把发型收成无发 |
| 明确不做 | 不改已生成 pawn 的存档发型。不新增鼠蛋专用发型，也不改头型、衣服或发色 |
| Language | 无 |
| 存档 | 无。`hairDef` 仍是原版字段，旧档保持生成时写下的值 |
| 归属表 | 否 |
| 回归 | `RHAH_RatkinAppearanceTests.YoungRatkinKeepTheSameHairPool`：未指定性别时男女发型和中性发型都留下，无标签发型仍丢掉 |
| 后续债 | 无 |
