# 鼠族: 饥与祸

<!-- 只放面向用户的说明!!! 
面向开发者/Agent的放在开发者贡献文档或 Agents.md
-->

RimWorld 1.6 模组。`packageId`：`nanaloveyuki.ratkin.hungerandhavoc`。

独立作品，不是“鼠灾-大荒年”的 Continued。Def 前缀 `RHAH_`，事件显示 ID 用 `I-`，剧情用 `N-` / `E-` / `J-` / `R-`。

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

