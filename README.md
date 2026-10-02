# 鼠族: 饥与祸

<!-- 只放面向用户的说明!!! 
面向开发者/Agent的放在开发者贡献文档或 Agents.md
-->

RimWorld 1.6 模组。`packageId`：`nanaloveyuki.ratkin.hungerandhavoc`。

独立作品，不是“鼠灾-大荒年”的 Continued。Def 前缀 `RHAH_`，事件显示 ID 用 `I-`，剧情用 `N-` / `E-` / `J-` / `R-`。
仅卸载本模组前，在已加载存档的设置里先停用这个存档的新内容，再备份并导出卸载用副本。退出后只卸本模组，保留鼠族、Harmony 和 Biotech，然后加载 `RHAH-removed-*`。`RHAH-backup-*` 仍含本模组数据，加载它必须留着本模组。

## 中途加入 / Adding mid-save

支持在已有存档中加入本模组及所需前置。加载完成时会补齐缺失的五个态度派系，保留已有派系并固定它们对玩家的好感，不需要先推进游戏时间。不导入“鼠灾-大荒年”或其 Continued 的存档数据，也不能与这些模组同时启用。

This mod and its required dependencies can be added to an existing save. Missing attitude factions are created when loading finishes, and existing factions are kept without duplicates. Their goodwill toward the player is fixed before game time advances. Save data from Great Famine Year or its Continued versions is not imported; do not enable those mods alongside this one.

## 设置

原版模组设置页和 IrisMenus 总览页都有“重置本模组配置”。确认后恢复全部默认配置并保存；存档中的剧情进度、来客和仅此存档停用状态保留。

开发者事件页的 51 条事件均有“即时刷新”。直接生成到当前地图，跳过排队和世界地图行进，窗口暂停时也可用。商队劫掠直接生成袭击者，危险的鼠族安居点直接生成居民，不要求派出远行队。原有“触发”仍走正常流程；新内容和事件开关仍生效。

## 文案

50 条特质、121 条编号经历及两条基础经历已审校并同步中英。事件和穗音文案保留人物说话与情绪，价格、期限和人数不再写死默认值；必要风险另列提示。英文中的鼠蛋称为 young ratkin，不是蛋。

“易子而食”目前只能在食物替代开启、餐数大于 0 且库存充足时交餐，不能交付孩子；交餐也不会让孩子自动加入。原有选择信保存了正文，旧档中已经打开的信不会改成新稿。

## 结构

- `1.6/`：当前版本的程序集、Def、Patch
- `Guard/`：与旧鼠灾包冲突时只加载提示，不加载主体
- `docs/adr/`：设计决策
- `Source/`：SDK-style 工程，输出到 `1.6/Assemblies/`

其它模组请使用 `HungerAndHavoc.Api.RHAH_Api`，不要扫描经历或私有类型。

## 构建

退出 RimWorld 后：

```bash
scripts/deploy.sh
```

脚本做结构检查、Release 构建，并部署到游戏 `Mods/RatkinHungerAndHavoc`。游戏目录用 `RIMWORLD_DIR`。只检查仓库结构：

```bash
python3 scripts/verify-scaffold.py
```

