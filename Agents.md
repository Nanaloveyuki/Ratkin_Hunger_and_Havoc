## References

Most in Windows, not WSL.

- Vanilla Game Decompile
  - DLCs: `D:\References\Rimworld\Vanilla\DLCs\`
  - Game: `D:\References\Rimworld\Vanilla\Game`
- Mods
  - NewRatkinPlus (RaceMod): `D:\References\Rimworld\Mods\NewRatkinPlus`
  - NewRatkinPlus Chinese Translate: `D:\References\Rimworld\Mods\NewRatkinPlus_zh`
- Old-Repo: `~/repos/Ratkin-Great-Famine-Year-Continued`

## This mod

- Display name: `鼠族: 饥与祸` / `Ratkin: Hunger and Havoc`
- `packageId`: `nanaloveyuki.ratkin.hungerandhavoc`
- Def / keyed prefix: `RHAH_`
- Namespace: `HungerAndHavoc`
- Game content lives under `1.6/` (add `1.7/` later; do not put Defs/assemblies at repo root)
- Decisions: `docs/adr/`
- Engineering standard: `docs/engineering.md`
- Bug process: `docs/bug-handling.md`
- Save / uninstall inventory: `docs/save-ownership.md`
- Play spec only (do not copy source): `/root/repos/Ratkin-Great-Famine-Year-Continued`
- Other mods must use `HungerAndHavoc.Api`, not Backstory or private jobs

## Comment

### How comment?

Use short and clear **Chinese** comment when you need comment somethings.

DO NOT USE USELESS **Punctuation Marks** IN THE SENTENCE END.

example:`建议使用 Ratkin Young 字段而非 Ratkin Egg 字段来表示鼠蛋`

NO Chinese Punctuation Marks

### When comment?

Actually You Real Need or Maybe Lost Memory, Or clarify the facts to the user

## Bug fix

Follow `docs/bug-handling.md`. Fill the fix card before editing.

Allowed in the same change: the root-cause path, its regression test, and Language / save-key / ownership updates that the fix forces.

Do not drive-by rename save keys, move namespaces, expand API, or format unrelated files.

Player-facing strings need matching ChineseSimplified and English Keyed keys. Def body text stays Chinese; English uses DefInjected.

Save keys and XML type names are contracts. Pre-1.0 rebuilds must be explicit on the fix card. Register new persisted Defs/types in `docs/save-ownership.md`.

## Current milestone

M0 契约基线已完成，版本 `0.1.0`。其它模组只引用 `HungerAndHavoc.Api.dll` 查询来源、访客、闸门、标记和释放，看不到 Comp / Hediff / Job。

规范以 `docs/engineering.md` 为准，冲突按该页优先级。玩法对照旧仓库，禁止拷贝旧源码。

### 已落地

- 独立 `HungerAndHavoc.Api.dll`，实现 → API，API 不引用实现
- `IHungerPawn` / `HungerPawnSnapshot`；API 事件不传 Comp
- `CompHungerPawn` 在 `HungerAndHavoc.Identity`；存档键 `sourceIncidentDisplayId`
- `RegisterRatkinMatcher` 取代对外的 `HungerRace.Register`
- 事件目录 `I-001`..`I-051`；中英 Keyed 对称
- Guard 冲突检测与 `LoadFolders.xml` 主体跳过

### 下一目标：M1

最小可玩闭环：一条原作求助事件（先 `I-001`）加一条敌对事件，含生成、行为、救济/冲突、离场和存档重载。

未做：IncidentDef / Worker、生成管线、访客 AI、`GameComponent` / `MapComponent`、Harmony 业务补丁、叙事/结局、经历/特质。

不要建空的 Generation、Behavior、Narrative、World、Patches、UI 目录。不要改 packageId、显示名、Harmony Id、Guard 冲突列表、事件目录 51 条显示 ID。

### 验证

```
python3 scripts/verify-scaffold.py
dotnet build Source/Api/HungerAndHavoc.Api.csproj -p:RimWorldDir=/mnt/e/Apps/Steam/steamapps/common/RimWorld
dotnet build Source/HungerAndHavoc.csproj -p:RimWorldDir=/mnt/e/Apps/Steam/steamapps/common/RimWorld
dotnet test Source/Tests/HungerAndHavoc.Tests.csproj -p:RimWorldDir=/mnt/e/Apps/Steam/steamapps/common/RimWorld
```

Windows 默认 RimWorld 目录：`D:\Appdata\Steam\steamapps\common\RimWorld`。

不要提交 `bin/`、`obj/`、pdb、`1.6/Assemblies/0Harmony.dll`。Harmony 是模组依赖，不是本仓库程序集。
