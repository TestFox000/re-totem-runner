# Totem Runner 复现工程交接说明

目标接手模型：**DeepSeek V4.1 Flash-1017**  
目标设备：**Xiaomi 15，Android 16 / HyperOS 3.0，ARM64 only**  
当前目标：在保留原版玩法和奇幻气质的基础上，继续完成汉化嵌字、启动动画修复和整体体验打磨。

## 先读这些文件

1. 本文件 `DEEPSEEK_HANDOFF.md`
2. `README.txt`
3. `DSH\README.md`、`DSH\STATE.md`、`DSH\board.json`：团队协作、接续状态和任务卡索引
4. `DSH\RESOURCE_MAP.md`：汉化入口静态审查及尚未确认的范围
5. `Analysis\TotemRunner\` 下与当前卡片相关的恢复和验证报告
6. `Assets\Plugins\Assembly-CSharp-firstpass\IngameVideoPlayer.cs`
7. `ProjectSettings\EditorBuildSettings.asset`

不要重新从原 APK 开始逆向。这个工程已经完成了首个可玩的 ARM64 移植，先验证当前状态，再处理未完成项。

DSH 团队方案已补充：父 Agent + Runtime / Localization / QA 子角色，15 张带依赖的任务规格。它们尚未在 DSH 创建原生卡或执行。父 Agent先读 `DSH\PARENT_AGENT_PROMPT.txt`，按依赖分配卡片；不把模板当作验收结果。

## 工程位置和重要文件

- Unity 工程：`D:\User\Desktop\UnityAndroidTest\TotemRunner`
- 原始 APK（只读保存）：`D:\User\Desktop\com.chillingo.totemrunner.android.rowgplay_1.0.1.apk`
- 原始 Unity 导出基线：`D:\User\Desktop\UnityAndroidTest\Recovery\TotemRunner-Unity2021\ExportedProject`
- 分析和验证报告：`D:\User\Desktop\UnityAndroidTest\Analysis\TotemRunner`
- 构建脚本：`D:\User\Desktop\UnityAndroidTest\Tools\Build-TotemRunner.ps1`
- 安装脚本：`D:\User\Desktop\UnityAndroidTest\Tools\Run-TotemRunner.ps1`
- Unity：`D:\SoftWare\Unity 2021.3.45f2c1\Editor\Unity.exe`
- adb：`C:\Users\TestFox\AppData\Local\Android\Sdk\platform-tools\adb.exe`

不要覆盖原始 APK、`Recovery` 或 `Analysis`。修改前请先复制一份 `TotemRunner`，或者至少备份要改的资源和脚本。

## 如何把工程交给 DeepSeek

如果 DeepSeek 运行在同一台电脑并能访问本地文件，直接把下面这段路径和 `DEEPSEEK_HANDOFF_PROMPT.txt` 的内容发给它：

`D:\User\Desktop\UnityAndroidTest\TotemRunner`

要求它先打开本文件，再执行构建和真机验证。不要只发送 APK；APK 只能用于运行和对照，不能替代 Unity 工程。

如果 DeepSeek 在另一台电脑或只能接收上传文件，建议上传一个不含 Unity 缓存的压缩包。必须包含：

- `TotemRunner\Assets`
- `TotemRunner\Packages`
- `TotemRunner\ProjectSettings`
- `TotemRunner\README.txt`
- `TotemRunner\DEEPSEEK_HANDOFF.md`
- `TotemRunner\DEEPSEEK_HANDOFF_PROMPT.txt`
- `TotemRunner\DSH`（全部任务卡、父/子 Agent 提示词与接续状态）
- `Analysis\TotemRunner`
- `Tools\Build-TotemRunner.ps1`
- `Tools\Run-TotemRunner.ps1`
- `Tools\Export-TotemRunnerHandoff.ps1`
- `Tools` 顶层的恢复 `.py` / `.ps1` 脚本（导出器按扩展名纳入，排除下载工具/缓存；需重新分析原始导出时另带只读 `Recovery` 基线）
- `TotemRunner\Builds\TotemRunner-ARM64.apk`
- 原始 APK `com.chillingo.totemrunner.android.rowgplay_1.0.1.apk`（作为只读对照文件）

先暂停各 Agent写入及 Unity/构建，再在 PowerShell 生成交接包。导出器使用工作区内的新时间戳目录，不删除或覆盖既有交接包；原始 APK 默认从工作区上一级读取。先预检清单，再正式导出：

```powershell
& 'D:\User\Desktop\UnityAndroidTest\Tools\Export-TotemRunnerHandoff.ps1' -ListOnly
& 'D:\User\Desktop\UnityAndroidTest\Tools\Export-TotemRunnerHandoff.ps1'
```

不要把 `TotemRunner\Library`、`Logs` 或 `UserSettings` 放进压缩包；它们属于本机缓存，体积约 2.5 GB，DeepSeek 用同一 Unity 版本重新导入即可生成。

输出位于工作区 `Handoffs`，包含清单与校验值。这是传输快照，不代表游戏已验收。搬机后还需核对 `Assets\Editor\TotemRunnerBuild.cs` 内写定的 SDK/NDK/JDK 路径与 debug 签名；脚本参数只控制 Unity 路径。原件缺失时显式传 `-OriginalApkPath`。不要为了覆盖安装卸载应用或清空存档。

## 当前构建配置

- 包名：`local.totemrunner.restored`
- 应用名：`Totem Runner Restored`
- Unity：2021.3.45f2c1
- Android：IL2CPP、ARM64、GLES3
- minSdk：26；targetSdk：33
- 横屏
- 场景和顺序已恢复；`Tutorial` 是场景索引 3，`B01_Lv_01` 是场景索引 4
- 设备序列号：`1d16aa01`
- 设备型号：`24129PN74C`（Xiaomi 15）

原 APK SHA-256：

`108ffbdb8a383a798cc124347aaa391e4bf1a486b13d5794d09da677305c690e`

在启动视频改动前，最后一个已完成真机玩法验证的 APK SHA-256：

`7e16ebfe79fd503bea49b6951c634ea642515c7b6e92dc9631a5088652ab6fb0`

当前包含启动视频实验改动的 APK SHA-256：

`3173464cb67776e26211dbd93033e6bdd74a655ac0281317bc2fa013655ccf98`

当前候选大小 207,070,478 bytes。历史已验收包为 203,355,262 bytes，其独立 APK 副本尚未定位。旧 `restoration-status.json.latest_apk` 和 `final-apk-validation.json` 仍填历史哈希，虽然路径与当前候选相同；不要把旧验收结论套到新包。按 `DSH\STATE.md` 与 TR-001 先建立不可变归档，后续测试绑定 APK 哈希。

## 已完成的恢复工作

以下工作已经完成并经过真机验证，不要重复实现：

1. Unity 旧版 C# API 迁移、脚本编译和场景构建顺序恢复。
2. 缺失插件和运行时替代实现补齐。
3. 原版 shader、音频、材质、纹理、模型、相机和场景资源恢复。
4. 原版激活状态、Renderer 阴影、RGBA4444 纹理、TextMesh 颜色和字体度量修正。
5. 角色层级、`PlayerSounds`、CharacterController、触发器和粒子生命周期修正。
6. 教学关流程、鹰和野猪变身、飞行控制、碰撞、死亡、复活和关卡切换已验证。
7. 旧版 ARM32 工程已转换为可在 Xiaomi 15 ARM64 上运行的 Unity 2021 工程。

重要报告包括：

- `Analysis\TotemRunner\restoration-status.json`
- `Analysis\TotemRunner\final-apk-validation.json`
- `Analysis\TotemRunner\character-controller-recovery.json`
- `Analysis\TotemRunner\device-controllerfix.log`
- `Analysis\TotemRunner\device-playable-final.png`

此前真机反馈：教学提示、角色前进、鹰变身、野猪变身和飞行控制均已正常；菜单点击也正常。

## 当前状态和未完成项

### 1. 启动小动画：当前是实验改动，必须先验证

原版 `Assets\StreamingAssets\intro.mp4` 已完整保留：

- 大小：6,337,864 bytes
- SHA-256：`b1621ce5f83d9f64ff2125a9c4987eab00026dd7785904ab1b5600139e7b72f8`

在 Xiaomi 15 上，MediaCodec 能解码原视频，但旧的 `CameraNearPlane` 播放路径出现黑屏。当前 `IngameVideoPlayer.cs` 已尝试改成：

- `MaterialOverride` 渲染
- 独立 Quad 和 `Unlit/Texture` 材质
- 独立 layer 31 的正交相机
- 4:3 画面按屏幕比例 fit-inside，保留黑边
- 当前实验相机 `farClipPlane` 已改为 100

这个实验改动刚完成构建和安装，但最新截图尚未完成逐帧验收。接手后第一步应确认：启动时是否确实看到 Chillingo 标志和视频内容，而不是黑屏或过早跳过。若 Quad 方案仍失败，优先改为 `RenderTexture + RawImage` 或 Android 原生 Surface/TextureView 方案；不要删除原 `intro.mp4`。

启动验证必须满足：

1. 每次截图前确认当前前台窗口是 `local.totemrunner.restored`。
2. 记录 1、3、6、10、14、20 秒画面和 logcat。
3. 不要使用 `adb shell input` 注入触摸；本机已拒绝 `INJECT_EVENTS`，需要人工触摸。
4. 菜单出现后再进行玩法验证。

### 2. 汉化嵌字

用户要求的语气是：**贴近原版的奇幻语气**。不要做现代网游式、网络流行语式或过度直译的文本。

当前汉化尚未完成，主要工作面：

- 菜单、按钮、提示、计分和关卡文本：优先建立统一的中英对照表，再落到脚本或 TextAsset。
- 旧版 bitmap font：现有字体缺少完整汉字，需要增加 CJK 字体资源或运行时 fallback；不要直接拉伸旧英文字体。
- 教学图像文字：`Assets` 中约 14 个 tutorial 图片/文本资源仍含英文，需要按原版版式重绘或覆盖嵌字。
- 动态 UI：同时修正 TextMesh、Canvas、字号、描边、对齐和安全区，确保 Xiaomi 15 横屏不裁切。

建议先做一套术语表，例如：

- Totem：图腾
- Eagle：鹰形 / 化鹰（按上下文选一套）
- Boar：野猪形 / 化作野猪
- Tutorial：启程 / 教学关（菜单标题和叙述可不同）
- Run：奔行 / 疾奔

同一术语在菜单、教学和结算中必须保持一致。完成一小段后先在设备上看实际字形和换行，再批量替换。

### 3. 游戏体验

在不改变原版节奏的前提下检查：首次启动等待、视频跳过、按钮热区、返回逻辑、教学提示时机、变身反馈、音量平衡、死亡复活和横屏安全区。每项修改都要能在设备上复现和回归。

## 构建、安装和基本验证

在 PowerShell 中执行：

```powershell
& 'D:\User\Desktop\UnityAndroidTest\Tools\Build-TotemRunner.ps1'
& 'D:\User\Desktop\UnityAndroidTest\Tools\Run-TotemRunner.ps1'
```

构建产物：

`D:\User\Desktop\UnityAndroidTest\TotemRunner\Builds\TotemRunner-ARM64.apk`

检查设备连接和前台窗口：

```powershell
& 'C:\Users\TestFox\AppData\Local\Android\Sdk\platform-tools\adb.exe' devices
& 'C:\Users\TestFox\AppData\Local\Android\Sdk\platform-tools\adb.exe' -s 1d16aa01 shell dumpsys window | Select-String 'mCurrentFocus'
```

构建脚本里使用 `Start-Process ... .WaitForExit()` 是有意的，用来避免 Unity 授权辅助进程导致脚本挂起；不要改回只等待主进程的写法。

## 建议接手顺序

1. 按 DSH 父 Agent提示词读取状态，建立原生任务卡与 `board.json` 的 ID 映射。
2. TR-001 固定候选/历史证据与回退能力；并行 TR-002 只读清点汉化入口。
3. TR-003 验证/修复视频，TR-004/005 建立术语与中文真机试点；具体依赖见 board。
4. TR-006/007/008 分批完成菜单、教学图和剩余文本，TR-009 独立验收中文测试版。
5. TR-010/011/012/013 补齐存档/生命周期、全关卡和有证据的体验/性能改进。
6. TR-014/015 验收同一最终 APK 并冻结交付；父 Agent每轮更新状态，下一会话可直接接续。

## 交接约束

- 不要升级 Unity 版本，不要改变包名，不要删除原始资源。
- 不要把原 APK 当作可直接修改的工程；它只用于对照和校验。
- 不要重做已经在报告中完成的恢复项，除非回归测试证明它们退化。
- 每次改动前先复制文件，改动后记录文件路径、原因、构建结果和设备表现。
- 如果启动视频实验影响玩法，先核实可用备份；历史 APK 副本尚未定位，不能假称可回退。由 TR-001 先建立真实候选/源快照，再做修改。

## 可直接发送给 DeepSeek 的首条消息

```text
你是 DSH 中负责接手 Totem Runner Unity 2021 ARM64 工程的父 Agent。目标设备是 Xiaomi 15（Android 16 / HyperOS 3.0）。

请先阅读：
D:\User\Desktop\UnityAndroidTest\TotemRunner\DEEPSEEK_HANDOFF.md
D:\User\Desktop\UnityAndroidTest\TotemRunner\README.txt
D:\User\Desktop\UnityAndroidTest\TotemRunner\DSH\PARENT_AGENT_PROMPT.txt
D:\User\Desktop\UnityAndroidTest\TotemRunner\DSH\README.md
D:\User\Desktop\UnityAndroidTest\TotemRunner\DSH\STATE.md
D:\User\Desktop\UnityAndroidTest\TotemRunner\DSH\board.json

工程已经完成首个可玩的 ARM64 移植，菜单、Tutorial、鹰/野猪变身、飞行、碰撞、死亡和复活都已验证。请不要重新逆向原 APK，也不要升级 Unity 或覆盖原始资源。

创建或复用 Runtime、Localization、QA 子 Agent。按 DSH\cards 的 15 张任务规格建立原生卡映射，父 Agent维护 board 和 STATE，分配具体文件租约。子 Agent可互相协作，不能自行扩范围或标 done。实现后进入 review，由独立 QA 和父 Agent验收。

先执行 TR-001 固定候选与源快照，并行 TR-002 只读清点汉化。再按依赖处理启动视频、奇幻语气术语表、中文字体真机试点、菜单/教学图片/剩余文本、存档/全关卡与体验。

当前 APK 的 317346... 哈希是未验收启动实验版，历史玩法报告 7e16... 不适用于它；旧 APK 副本尚未定位。每次产物单独归档，测试绑定实际 APK 哈希；不要用 MediaCodec 日志代替可见画面。

构建：
& 'D:\User\Desktop\UnityAndroidTest\Tools\Build-TotemRunner.ps1'
安装：
& 'D:\User\Desktop\UnityAndroidTest\Tools\Run-TotemRunner.ps1'

设备序列号是 1d16aa01，重新核实连接。Unity/构建输出/设备独占，构建期间冻结修改；不要使用 adb shell input 注入触摸，不卸载清数据。需要人工操作时给出一组清晰步骤，卡保持 waiting_device，继续独立任务。每个里程碑和会话结束更新 STATE，以便下次接续。
```

