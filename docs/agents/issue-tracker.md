# 工作项：GitHub

规格和实施工作项使用 `Nanaloveyuki/Ratkin_Hunger_and_Havoc` 的 GitHub Issues。机制访谈留在对应领域文档，形成规格或实施工作项时才发布 Issue。

## 操作

使用 `gh issue create/view/list/edit/close`，发布前从本仓库解析远程。前置依赖使用 GitHub 原生 `blocked_by`，目标为阻塞项的数据库 ID；不可用时在正文注明 `Blocked by: #编号`。已经由规格拆出的实施工作项按前置依赖推进，不重复分流。

外部 PR 不作为分流入口。本轮只记录流程配置，不创建远程 Issues 或标签
