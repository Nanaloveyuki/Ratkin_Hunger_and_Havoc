# 设置绘制反复写盘

状态：已实现，验证未执行（子代理不运行 build/lint/test）

| 字段 | 内容 |
| --- | --- |
| 复现 | 打开原版设置或 IrisMenus Events/Dev Events/Genes，保持无操作。来自源码对照；卸载清理已运行隔离复现，其它尚未游戏内复现 |
| 期望 / 实际 | 应在保存生命周期写盘；实际每次绘制写全设置文件 |
| 版本 | 本仓库 1.0.0，2026-09-30 当前工作树；保留用户并发改动 |
| 级别 | S3 |
| 根因 | 已再次对照当前源码与原版/IrisMenus。`Dialog_ModSettings.PreClose` 与 `Dialog_Options.PreClose` 调 `Mod.WriteSettings()`；IrisMenus `RegisterSubItemListing` 未传 save 时保存委托是 `owner.WriteSettings`，`MenuSession.Select` 离页与 `MenuWindow` 关闭都会调用它。`Mod.WriteSettings` 在 `modSettings != null` 时调 `ModSettings.Write()`。修复前 `DoSettingsWindowContents`、`DrawEvents`、`DrawDevEvents`、`DrawGenes` 每次绘制都调用 `RHAH_Settings.Write()`，无操作也会全量写设置文件。控件本身改的是内存中的 `RHAH_Settings`，删除绘制写盘后仍由上述生命周期保存 |
| 最小修复 | Source/Core/ModEntry.cs、Source/Pawn/Compat/RHAH_IrisMenusCompat.cs；复用原版与 IrisMenus 已有保存生命周期，删除绘制内写盘 |
| 明确不做 | 不部署，不修改外部 Mod，不改包名/API/事件显示 ID，不夹带重构 |
| Language | 预计无；若新增玩家可见字符串必须同时补中英 Keyed |
| 存档 | 默认无新键、不破坏重建；保留已有记录。若必须新增持久化类型，动手前在本卡明确并登记 |
| 归属表 | 无新增类型时保持；卸载清理动作或新持久化类型必须更新 save-ownership.md |
| 回归 | `/tmp/rhah-settings-write-smoke.cs` 必须完整执行四个真实绘制入口。外部 IMGUI/翻译/IrisMenus 标题与卡片只打前缀跳过，绘制抛异常即失败，禁止吞掉后报通过。通过条件是：绘制结束后设置 XML 不存在；随后真实 `RHAH_Mod.WriteSettings` 写出 `Config/Mod_RHAHSmoke_RHAH_Mod.xml`，新 `GetSettings` 读回 `enableNewContent` 与 `I-001` 开关。未运行。游戏内仍未验证，主代理最终标 UI 未验 |
| 后续债 | 本卡外相邻问题不纳入；游戏内验证边界明确报告 |
