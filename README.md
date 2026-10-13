# 鼠族: 饥与祸

<!-- 只放面向用户的说明!!! 
面向开发者/Agent的放在开发者贡献文档或 Agents.md
-->

RimWorld 1.6 模组。`packageId`：`nanaloveyuki.ratkin.hungerandhavoc`。

需要 NewRatkinPlus、Harmony、Biotech 和 Oxygen DSL Framework（`nanaloveyuki.oxygendsl`）。Oxygen DSL Framework 必须排在本模组前面。IrisMenus 用于分页设置。

独立作品，不是“鼠灾-大荒年”的 Continued。Def 前缀 `RHAH_`，事件显示 ID 用 `I-`，剧情用 `N-` / `J-` / `R-`。
仅卸载本模组前，在已加载存档的设置里先停用这个存档的新内容，再备份并导出卸载用副本。退出后只卸本模组，保留鼠族、Harmony、Biotech 和 Oxygen DSL Framework，然后加载 `RHAH-removed-*`。`RHAH-backup-*` 仍含本模组数据，加载它必须留着本模组。

## 中途加入 / Adding mid-save

支持在已有存档中加入本模组及所需前置。加载完成时会补齐缺失的五个态度派系，保留已有派系并固定它们对玩家的好感，不需要先推进游戏时间。不导入“鼠灾-大荒年”或其 Continued 的存档数据，也不能与这些模组同时启用。

This mod and its required dependencies can be added to an existing save. Missing attitude factions are created when loading finishes, and existing factions are kept without duplicates. Their goodwill toward the player is fixed before game time advances. Save data from Great Famine Year or its Continued versions is not imported; do not enable those mods alongside this one.

## 1.0.4 存档兼容

从 1.0.3 升级时，旧的单人投喂等待记录会在加载时自动转为新的等待名单，保留接收者、请求数量和等待期限，不需要手工编辑存档。再次保存只写新格式；原存档不会在加载时被覆盖。“仅此存档停用新内容”仍保留，也不会阻止已有记录迁移。

王国篇 K1～K5 初步合作已接入。仅在穗音下由设置页或 IrisMenus 手动开启，不自动触发。接受后的寄件与商队行动在切换叙事者后仍按约定期限继续；新的委托及穗音私人回信等她回来。调查使用实际文书与免费副本，公开、补发和护送按真实交接结果回复，第五幕不是终局。既有信任及普通 I/N 事件不变

## 事件链存档兼容 / Event-chain save compatibility

事件后续和捕食者的延迟到达统一由 Oxygen DSL Framework 执行；本模组不再运行旧事件链。已有饥与祸存档在加载时自动迁移，保留实例编号、阶段、开始时间、期限、已选结果和载荷，不重新抽取随机结果。旧的长事件保持原来的检查时点；已经完成短阶段的记录不会再执行一次。迁移成功后保存新格式，加载过程不覆盖原档。

旧记录损坏时保留原记录和待到达时间，暂停本模组事件链，并在日志和 Oxygen DSL Framework 的运行检查里显示原因。不要手工删记录来继续。已有的新格式记录优先，包括明确为空的记录；旧记录不会再次导入。

“仅此存档停用新内容”也会停下事件后续和尚未到达的捕食者，保留事件记录、已有来客、信件和世界地点。导出卸载用副本只移除饥与祸自己的事件记录，保留 Oxygen DSL Framework 和其它模组的数据。

Oxygen DSL Framework is required and must load before this mod. It is the only runtime for event follow-ups and delayed predator arrivals. Existing Hunger and Havoc saves migrate automatically, keeping instance IDs, stages, start times, deadlines, payloads, and selected results without another random roll. Long follow-ups keep their original check timing, and completed short stages do not run again. Successful migration saves only the new format; loading does not overwrite the original file.

Malformed legacy data is kept, including pending arrivals. Hunger and Havoc chains pause with a reason in the log and Oxygen DSL Framework's runtime inspection. Existing new-format state takes precedence, even when explicitly empty; legacy records are not imported again. Turning off new content also stops follow-ups and pending arrivals while keeping existing people, letters, places, and event records. Removal exports delete only Hunger and Havoc's event state. Keep Oxygen DSL Framework installed, along with the other required dependencies.

## 事件去同质化

自然事件与广播按 20 类选择，再决定人数、年龄、染疫等具体场景。旧事件身份仍保留；“难民接济（婴儿）”退出新事件池，已有请求和后续继续。

- “做个交易”：来客带着白银、零部件、药品等物资换食物。报价使用仓库里数量最多的合格食物，生食和熟食都可；按实际人数折算营养，最多 100 件。成交时当场交付实物，附赠情报可自行决定是否探索。升级前已打开的情报信仍按原报价和地点奖励处理。
- “路过的鼠族”：从入场边走到地图对边离开，不再到居住区停留乞讨。已有来客自动接上穿图路线；收留、囚禁或已下达离场令的成员保持原处理。
- “易子而食”：可选择实际殖民地孩子交接，或按设置交餐。成功后来客孩子加入殖民地，成人离开。患有鼠疫的孩子不能交接。
- 已登记的八条长后续有实际人物、选择与后果，包括鼠妈回访、旧窝记物、换子追问、空投认领、认亲依恋与孤儿自称身份。新配置默认开启遗弃孩子和流民商队两类（含鼠疫场景），其它长后续可在事件类设置中开启。升级保留已经保存的选择，包括全部短后续；既有长事件照常继续。

事件设置同步为 20 类。升级时旧逐事件配置恢复为类默认值，其它设置保留。穗音来信已依据作者的小故事调整：她会讲宫墙外的见闻、日记、有限资助和自己的犹疑。普通来访记录可在 IrisMenus 主动查看；交换答复、封洞、使者交易和记录取舍等直接结果改为短消息，重要线索、照料结果与身份揭示继续来信。已有选择信保留打开时的正文。

鼠疫来客的病情、照料需求和传播风险并入主事件信，使用原事件的处置选项。独立检疫信、留观流程、专属信任结算和康复回访已移除；旧档加载时自动清理对应记录并解除检疫留驻，原文件保留。鼠疫疾病和传播仍按原规则运行。

Plague arrivals describe their illness, care needs, and transmission risk in the main event letter and use that event's existing choices. Separate quarantine letters, observation holds, quarantine trust outcomes, and recovery visits have been removed. Existing saves automatically discard those retired records and release quarantine holds without overwriting the original file. The disease and its spread retain their existing rules.



## 本次修复 / Current fixes

- 卸载导出会清掉其它模组语音缓存中引用的饥与祸心情，保留其它心情和语音记录。
- 商队劫掠发生后发送红色袭击信，定位遭遇地图上的袭击者。
- 敌对来客接受送粮等待后转为中立；收齐一人的份额时，其余来客继续等自己的食物。可战斗的敌对成年鼠族会攻击殖民者。
- 短工、长工和临时征募到期后恢复访客身份。因年龄还不能走路的孩子照常到期，有合格的同组照护者时由其带走；因伤病倒地的来客继续暂停期限。永久加入后清除临时期限。
- 短工、长工和临时征募吃饱后保留合同期限，按殖民地工作安排继续工作。已经永久加入的成员可正常工作，即使旧档还留着临时期限。
- 灾民收齐食物后会结束等待，优先吃自己背包里的合格食物。背包食物仍受取食种类和饮食限制。
- 广播回应固定前往广播所在据点，包括最早建立的地图。切换查看另一据点不会改变目的地。
- IrisMenus 1.1.1 可正常显示本模组的分类设置。

Removal exports clear Hunger and Havoc thought references from other mods' speech caches while keeping their other records. Caravan attacks send a red threat letter targeting the attackers on the encounter map. Hostile visitors become neutral when they accept a food handoff, and each recipient waits for their own share. Combat-capable hostile adult ratkin can attack colonists.

Temporary stays expire for children who cannot walk because of their age; an eligible adult in their group carries them away. Injury or illness still pauses the stay. Permanent joining clears the temporary deadline. Broadcast responses keep the broadcasting colony as their destination, including the first map. IrisMenus 1.1.1 displays the categorized settings again.

Meals preserve temporary workers' contract deadlines, and workers can continue their assigned work. Permanently joined colonists can work even if an old save retains an expired temporary contract. After a food handoff, visitors stop waiting and look for an acceptable meal in their own inventory. Food settings and dietary restrictions still apply.

原来的五条普通事件结局已停用，旧结局设置和预览入口移除。穗音信任、普通来信、奖励与身份揭示保留；旧档中的历史结局和已经收到的信件继续保存。王国篇 K1～K5 用于背景与初步合作，远期政治结局仍在规划中。

The five ordinary-event endings have been retired, along with their settings and previews. Suiyin's trust, ordinary notices, rewards, and identity letters remain. Existing ending history and received letters are kept. K1–K5 cover background and early cooperation; later political endings are still being planned.

## 设置

原版模组设置页和 IrisMenus 总览页都有“重置本模组配置”。确认后恢复全部默认配置并保存；存档中的剧情进度、来客和仅此存档停用状态保留。

开发者事件页保留原场景的“即时刷新”，退出新池的婴儿索取除外。直接生成到当前地图，跳过排队和世界地图行进，窗口暂停时也可用。商队劫掠直接生成袭击者，危险的鼠族安居点直接生成居民，不要求派出远行队。原有“触发”仍走正常流程；新内容和类开关仍生效。

## 文案

50 条特质、121 条编号经历及两条基础经历已审校并同步中英。事件和穗音文案保留人物说话与情绪，价格、期限和人数不再写死默认值；必要风险另列提示。英文中的鼠蛋称为 young ratkin，不是蛋。

中文 Keyed 的设置提示、来客信、长后续、穗音来信、结局和王国篇已清理空泛总结、指向不明的省略句和无来由的否定解释。玩法条件、数量与期限保留；“易子而食”的中文提示按现有孩子交接和食物替代方式说明。旧存档中已经打开的选择信仍保留原来的正文。

日语覆盖当前设置、事件、穗音来信与结局、特质、经历、心情、物品和 Biotech 基因，另有旧模组冲突提示。游戏语言选择日本語即可使用。鼠族沿用「ラットキン」，鼠蛋称为子供或子ラットキン，不译成卵。旧档中已经打开的选择信仍保留打开时的文字。

语言 XML 已按通用操作、设置、菜单、访客、事件、穗音叙事和王国篇分类。设置说明与故事正文分开，经历和心情另按内容拆分；既有键名与译文保留。菜单的标签与数值、分组数量、事件搜索分类和未知版本提示均使用中英日翻译。

“易子而食”的孩子选择与食物替代均可交接，原有选择信保存的正文继续保留。历史存档未记录的原母亲或交出孩子身份无法还原，对应后续会结束，不会用其它角色顶替。

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

脚本通过结构检查、Release 构建和测试后，部署到游戏 `Mods/RatkinHungerAndHavoc` 并核对 SHA-256。游戏目录用 `RIMWORLD_DIR`。开发自动化与 MCP 调试见 [自动化说明](docs/automation.md)。只检查仓库结构：

```bash
python3 scripts/verify-scaffold.py
```

