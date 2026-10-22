# 通用任务卡模板

## 调度字段（在 board.json 中维护）

ID / DSH 原生卡 ID / 标题 / 里程碑 / 优先级 / 状态 / owner / attempt / base_run_id / 依赖 / 资源租约。

文件租约最少记录 path、task_id、owner、session_id、acquired_at、base_sha256；共享资源同时记录 process_id，先核查真实进程再回收。

## 卡片正文

- 问题与复现：……
- 唯一可验证目标：……
- 输入：具体文件、历史报告、前置卡成果，不只写“看聊天”。
- 允许修改：列具体文件；初期不确定路径时只读清点，由父 Agent补批后执行。
- 排除项：本卡不需要调整的核心机制或其他模块。
- 输出：代码/资源改动清单与报告位置。
- 验收用例：引用 QUALITY_GATES 中 ID，明确预期行为和测试环境。
- 回退：源快照/文件备份/既有候选，禁止用删除存档当回退。
- 停止或升级条件：同一问题连续两次无改善，或发现租约冲突时先诊断/上报。
- 交给下一张卡：接口/术语版本/缺失证据和下一步。

## 子 Agent报告格式

保存 TotemRunner/DSH/reports/<卡号>/<attempt>/RESULT.md（相对于工作区，禁止随当前终端目录推断）：

1. 基线：base_run_id、任务版本、输入哈希、实际阅读证据。
2. 改动：文件相对路径、前后 SHA-256、目的；新增资源含 .meta。
3. 已做验证：命令、退出码、日志路径；静态/Editor/真机分开。
4. 未做验证：not_run / blocked、原因、所需动作。
5. 结果：实现是否完成、是否请求 review；不自行宣布 done。
6. 风险/回退：会影响哪些用例、如何恢复，不覆盖别人的文件。
7. 接续：一条可执行的下一步和必要协作消息。

QA 在同目录写 REVIEW.md，列观察、证据、问题、建议 pass/fail/waiting_device；父 Agent最终验收。

## 最小验证记录示例（模板，未执行）

```json
{
  "task_id": "TR-003",
  "source_run_id": null,
  "apk_sha256": null,
  "test_id": "V01",
  "method": "physical_device",
  "status": "not_run",
  "device": "Xiaomi 15",
  "evidence": [],
  "observed": "尚未执行",
  "next_action": "先完成 TR-001 并分配设备租约"
}
```

不要把模板的预期结果复制成真实测试结论。

