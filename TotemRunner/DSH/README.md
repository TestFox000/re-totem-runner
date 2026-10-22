# DSH 持续完善方案

本方案按用户描述的“父 Agent 通过任务卡与子 Agent 协作”设计，采用通用字段；不依赖未经确认的 DSH API。DSH 原生任务卡与这里的文件一一映射，文件副本用于换会话、换模型和审计。

当前交付的是协作方案和待办卡，尚未替 DeepSeek 创建原生团队、执行卡片或验收游戏。

路径约定：本文及任务卡中的 reports/ 一律指 TotemRunner/DSH/reports/，不随终端当前目录改变；游戏运行证据另存工作区 Analysis/TotemRunner/runs/。导出器包含整个 DSH，因此卡片报告能随工程转交。

## 入口与事实边界

1. 先读上一层的 DEEPSEEK_HANDOFF.md、README.txt。
2. 读 STATE.md：最近状态、证据边界和下一批任务。
3. 读 board.json：任务状态、依赖、负责人、DSH 卡片 ID、文件租约与设备占用的唯一权威索引。
4. 按需读 RESOURCE_MAP.md、cards/、QUALITY_GATES.md 和相关历史报告，不要每轮重新扫描所有场景或加载整个 Analysis。
5. 父 Agent 使用 PARENT_AGENT_PROMPT.txt；子 Agent 使用 CHILD_AGENT_PROMPT.txt 和分配到的卡片。

工程路径是工作区下 TotemRunner。工作区根还有另一套 Unity 测试工程，请始终使用构建脚本指定的 TotemRunner，不要在外层工程改游戏。

## 团队结构

默认父 Agent 加三个子 Agent；若 DSH 并发限制更小，按依赖排队，角色可以复用。

| 角色 | 主要责任 | 交付 |
|---|---|---|
| 父 Agent / Integrator | 选里程碑、发卡、分配写权限、整合、独占构建和设备、验收、更新状态 | board、STATE、DECISIONS、候选版本与验收记录 |
| Runtime Agent | 启动视频、兼容问题、存档/生命周期修复、经确认的体验缺陷 | 有限文件修改、复现与实现报告 |
| Localization Agent | 文本清点、术语、奇幻语气、字体试点、图像嵌字与 UI 接入 | 文本表、字体/图片资源、翻译与版式证据 |
| QA Agent | 独立读代码和复核结果、制定操作脚本、核对证据、覆盖关卡 | review.md、测试清单、回归结果和缺陷卡建议 |

QA 默认只读工程，可写自己卡片的 reports 目录。字体与美术需要专门工作时，临时用 Typography/Art Agent 替换一个空闲席位，不无限扩展团队。实现者不能成为自己卡片的最终验收人。

## 父 Agent 的每轮流程

1. 恢复：读取 STATE、board、目标卡和最近报告；核实未结束进程、实际文件、候选 APK 哈希。
2. 选卡：只选依赖已 done 的任务，锁定本轮一个里程碑。最多两张代码/资源修改卡同时执行。
3. 发卡：映射到 DSH 原生任务，记录 dsh_task_id、owner、attempt 和 base_run_id；把允许修改文件与验收条目发给子 Agent。
4. 协作：子 Agent 可互相提问、提供证据和接口建议；写权限、依赖、范围和优先级变化都由父 Agent确认。
5. 整合：子 Agent 提交报告和修改清单，状态进入 review；QA 独立核对，父 Agent 在冻结源文件后串行构建。
6. 验证：测试证据必须绑定确切 APK 哈希。需要触摸时合并成一组人工操作，缺设备则记 waiting_device，继续不依赖设备的工作。
7. 收口：父 Agent检查对应质量门槛，通过后标 done，归档不可变产物，更新 STATE、DECISIONS，分配下一批。
8. 停止：本里程碑条件满足后交付；新的非阻断建议进入后续卡，不以“还能优化”为由无限扩张。

以上循环由当前 DSH 会话推进；未配置调度器时，不宣称会话关闭后仍在后台工作。

## 任务卡与状态

卡片规范在 TASK_TEMPLATE.md，预置 15 张卡在 cards/。原生卡最少映射：ID、标题、优先级、父卡、负责人、状态、依赖、输入、输出、允许写文件、验收、证据、下一步。DSH 没有依赖或附件字段时，用卡片正文链接表达。

状态含义：

- backlog：尚未满足依赖或未进入本轮。
- ready：依赖满足，可以分配；尚未开工。
- running：已分配负责人和独占范围，正在工作。
- review：提交实现和报告，等待独立复核。
- waiting_device：仍缺真机/人工操作证据；不能当作完成。
- blocked：明确缺少输入或有未解除冲突，写清原因与解除条件。
- done：父 Agent根据质量门槛和证据验收通过。

通常流程为 ready → running → review → done；审查失败回到 running 并递增 attempt。backlog → ready、waiting_device/blocked 的恢复与 done 的重开由父 Agent处理。重开必须写明新回归证据及受影响版本。

DSH UI 可展示状态，但 board.json 由父 Agent单写；若 UI 与文件不同，先核实最后执行结果再同步，不凭界面自动宣布完成。

## 防冲突规则

- 文件租约采用具体路径；跨目录任务先清点，再由父 Agent批准路径清单。新发现文件不能偷偷顺手修改。
- Unity 的同一个 .unity/.prefab/.mat/.asset/.meta 文件只有一个写者；同场景不同对象也不并写。
- 场景/Prefab 的文字最终应用由父 Agent或其指定的唯一 Scenes Integrator 串行完成；菜单、教学与剩余文本卡先交对象/字段/词条映射，避免按界面分工后仍写到同一个场景。
- 运行时和汉化可能同时碰到 TextMesh、GUIText 或 Tutorial；遇到重叠就串行或由另一 Agent提交修改建议。
- 已有 .meta/GUID 不重建、不批量格式化 YAML。新资源与对应 .meta 一起交付；Unity 导入只在父 Agent授权时执行。
- board、STATE、DECISIONS 和任务规格由父 Agent单写。子 Agent只写租约范围和 reports/<卡号>/<尝试号>/。
- 同一工程仅一个 Unity Editor/批处理进程；正式构建前所有工程写者停笔，冻结 Assets、Packages、ProjectSettings，构建结束核查有无预期外变化。
- 构建脚本、Builds 输出、构建日志和同一台手机是共享资源，只由父 Agent调用或明确委托唯一操作者。当前脚本本身没有互斥机制，租约是调度约定；必须查进程和设备，不能只看锁文件。
- 不假设 Git 已建立；当前工程没有已确认的提交基线。先用不可变源快照/文件哈希建立回退能力，再决定是否引入本地 Git。
- 换会话发现遗留租约，先核实工作者/进程/未提交修改，再回收，不能按超时直接释放。

## 子 Agent 之间的协作消息

统一采用短消息，必须带卡号：

REQUEST TR-007 → TR-005：需要 tutorial.panel 的字号/行宽接口；相关文件……；若增加写范围请父 Agent确认。
ANSWER TR-005 → TR-007：已提供……；证据路径……；接口版本……。
BLOCKER TR-006 → Parent：现有 TextMesh 文件与 TR-007 重叠；建议先合并文本表，工程改动排队。
RESULT TR-003 → Parent：实现已提交；报告……；真机证据尚缺，不请求标 done。

对子 Agent 的请求不自动生成新的子任务或授权写文件。父 Agent把协作结论写入当前卡/DECISIONS，避免下一会话丢失。

## 持续维护与交付节奏

| 里程碑 | 必须得到的结果 |
|---|---|
| M0 可回退基线 | TR-001：候选/历史证据分离，源快照和 APK 归档，环境可复用 |
| M1 原版启动 + 中文试点 | TR-003 与 TR-005：视频真正可见可听；菜单一项与一条教学中文在真机可读 |
| M2 中文测试版 | TR-006/007/008/009：全部已清点用户文本完成接入和校对，基础玩法未回归 |
| M3 稳定游玩版 | TR-010～015：存档/生命周期、全关卡覆盖、必要体验修复、性能检查、冻结交付 |

文本和字体清点可与视频工作并行；进入后续阶段不要求无关文档工作阻塞，但每张卡的依赖必须满足。

每轮建议只处理一个可验证结果。完整关卡覆盖按章节拆卡；文件超过一个明确模块或一轮无法独立验收的卡先拆分。每个章节记录进入、机关/形态、死亡复活、完成推进，不以“场景加载成功”代替可通关。

同一缺陷两轮试改无改善，先停止修改并补诊断证据，再由父 Agent决定换方案。已通过项仅在代码/资源受影响或有新回归证据时重测。最终零阻断、零严重进度/存档损坏问题，允许已记录且用户接受的外部在线服务不可用。

## 版本、证据与接续

每轮使用唯一 run_id，例如 run-20261020-01（北京时间）。保存在工作区 Analysis/TotemRunner/runs/<run_id>/：

- source-manifest.json：Assets、Packages、ProjectSettings 的相对路径与 SHA-256、工具版本、源快照位置。
- build.log、apk.sha256、device-info.json、validation.json。
- screenshots/、logcat、人工操作结果（记录谁、何时、步骤、结果）。
- candidate APK 的不可变副本；已验证后再登记为 last_verified，不覆盖旧副本。

validation.json 的每个用例写 apk_sha256、source_run_id、test_id、status（pass/fail/not_run/blocked）、环境、证据路径和观察。Editor/静态检查与真机结果分栏。截图只能证明对应帧，按钮触摸、音频、流畅度需人工结果或录像；无法截取受保护内容时如实记录。

历史验证的 7e16… 版本与当前 317346… 实验版不能互相继承“已通过”。旧报告原样保留作为历史，不改写成当前版本已通过。

每次里程碑或会话结束更新 STATE：已验收卡、候选/稳定产物、进行中租约、阻塞、下一步第一条命令。详细代码只在源文件和卡片报告中保存，不把整段聊天当项目记忆。

