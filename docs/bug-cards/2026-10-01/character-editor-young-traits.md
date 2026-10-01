# Character Editor 手动添加的幼年鼠族特性读档消失

报告：[#014](https://rimworld.naloveyuki.top/mod/rhah/bugs/88510ec9-a2df-48bd-be2e-4926e39c61f2)

状态：调查中，尚未定位根因；没有修改运行时代码、没有部署，不能标记已修复

| 字段 | 内容 |
| --- | --- |
| 复现 | 报告称 Character Editor 手动添加到鼠蛋/鼠宝的特性在退出游戏重进后消失。报告未附存档、日志、具体特性名称、角色年龄或模组列表。该现象以报告为准；尚未取得能复现它的输入 |
| 期望 / 实际 | 手动添加后保存的特性应在重新加载同一存档时保留；报告称重进后消失 |
| 版本 | 报告为 Stable 1.0.0 / RimWorld 1.6；调查为当前项目 1.0.0 工作树。本机游戏程序集 1.6.9676.17735，Character Editor 包声明 v1.6.3，不能视为报告者相同环境 |
| 级别 | 暂定 S2，编辑器兼容问题；若存档已损坏则需重评，当前无损坏证据 |
| 根因 | 未定位。当前项目没有清空或移除 TraitSet 条目的代码。原版 TraitSet.ExposeData 不按年龄过滤普通特性；加载时只清理 null/null-def、以及 sourceGene 不再提供相同特性与 degree 的条目。已安装 CharacterEditor.TraitTool.AddTrait 创建普通 Trait 并直接加入 allTraits、绑定 pawn，不设置 sourceGene，也没有年龄检查。HAR GainTrait 限制只作用于新增，原版反序列化不走 GainTrait；Toddlers 主程序集未发现特性读档过滤。这些证据不能排除报告者环境中的其它补丁 |
| 最小修复 | 等待具体特性及退出前保存的 .rws、重进后保存的 .rws 和本次 Player.log / 模组列表，确定条目未保存、加载被删或仅界面隐藏，再改根因路径。当前不添加无证据的保护补丁 |
| 明确不做 | 不做读档强制补回，不跳过原版基因特性清理，不补丁 Character Editor 私有实现，不改外部模组、不改 API、特性 Def 名称或存档键 |
| Language | 无 |
| 存档 | 无新增、迁移或破坏性重建；不修改玩家存档 |
| 归属表 | 不更新，无新增持久化产物 |
| 回归 | 已运行实际安装游戏程序集的 Scribe 保存、LoadingVars、ResolvingCrossRefs、PostLoadInit 隔离烟雾：合成 Ratkin Pawn 分别为 2 岁 Baby、6 岁 Child，各自 50 个 RHAH 特性均保持同一 Def / degree、pawn 引用和空 sourceGene。每个场景在独立 Mono 进程中执行。TraitDef 从当前 XML 名称建立，degree 数据简化；直接加入 allTraits 的方式与已检查的 Character Editor AddTrait 相同，但未运行编辑器 UI。临时游戏 DLL 副本只清空 ModsConfig 静态启动、重定向 Log.Error/Warning，并关闭 DeepProfiler；Scribe、TraitSet、Trait 方法未改写。宿主输出缺少 Unity 生命周期图标初始化警告，不能当作完整游戏、HAR、Toddlers 或报告者全部模组集成验证 |
| 后续债 | 唯一阻塞是报告者的具体输入与重启前后存档证据，不据此推断是谁删除特性 |
