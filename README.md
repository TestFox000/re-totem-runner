# TotemRunner
利用AI组合技术栈+REA逆向+软件逆向工程，使这个已经死去的小游戏如今能够继续游玩，这真的是我很喜欢的一款小游戏

————————————————————————————————————————————————————————————————————————————————————————————————————————————————————
> 《图腾跑酷》移动端动作小游戏，基于 **Unity 2021 LTS** 开发，面向 Android（IL2CPP / ARM64），带中文界面与中文字体裁剪方案。

**工程版本**：m_EditorVersion: 2021.3.45f2c1，m_EditorVersionWithRevision: 2021.3.45f2c1 (8058fe21db2f)

## 特性

- **移动端优先**：Android 平台，IL2CPP 后端，ARM64 目标，触摸拖拽操作
- **中文界面**：内置中文文案表，按唯一 key 导入到 TextMesh，只改文本值，不改动层级 / 物体名 / 组件结构
- **中文字体裁剪**：字体子集化 + 纹理图集（font atlas），只保留游戏实际用到的汉字，显著缩小包体
- **模块化场景**：开场动画、主菜单、关卡（Block_01–04 / Titan_Race / Stone_Enemy_Level）
- **自动化构建**：附带一套脚本化的构建 / 设备验证流水线

## 技术栈

| 项 | 说明 |
|---|---|
| 引擎 | Unity 2021 LTS |
| 平台 | Android |
| 后端 | IL2CPP |
| 目标架构 | ARM64 |
| 字体 | 中文子集化 + 纹理图集 |

## 目录结构

```
UnityAndroidTest/
├── TotemRunner/          # 主工程（Unity 工程根目录）
│   ├── Assets/           # 场景、脚本、材质、网格、音频等游戏资源
│   │   ├── Levels/       # 主菜单 + 各关卡场景
│   │   ├── Scripts/      # 游戏代码
│   │   ├── Plugins/
│   │   └── ...
│   ├── Packages/         # 包管理器清单
│   ├── ProjectSettings/  # 工程设置
│   ├── Builds/           # 构建产物（不入仓库）
│   ├── Library/ Logs/    # Unity 缓存与日志（不入仓库）
│   ├── UserSettings/     # 个人编辑器设置（不入仓库）
│   └── DSH/              # 构建 / 验证自动化脚本与状态
├── Analysis/             # 逆向与调试产出（证据 / 素材来源），体积大，默认不入仓库
├── Recovery/             # 导出工程备份，默认不入仓库
└── Tools/                # 宿主工具
```

> 说明：Analysis/ 与 Recovery/ 体积较大且属于过程产物 / 备份，默认已在 .gitignore 中排除，需要时可单独归档。

## 快速开始

### 环境要求

1. Unity Hub 安装 Unity 2021 LTS（小版本见工程内 ProjectSettings/ProjectVersion.txt）
2. 安装 Android SDK / NDK / JDK（Unity Hub 的 SDK 组件即可）
3. 本机已配置 ADB（Android Platform Tools）

### 打开工程

```
# 用 Unity Hub 打开
unityhub://open/project/<工程路径>
```

### 构建与运行

1. File → Build Settings，切换到 Android，架构选 ARM64
2. 点击 Build，等待生成 APK
3. 安装到设备：

```
adb install -r <生成的APK路径>/TotemRunner.apk
```

## 构建流水线（可选）

TotemRunner/DSH/ 下提供脚本化的构建与设备验证流程，可用于重复出包与回归验证。参考：

- STATE.md：当前任务与状态
- DECISIONS.md：关键设计决策
- QUALITY_GATES.md：质量门禁

> 该目录为开发期辅助工具，非游戏运行时依赖。

## 本地化说明

- 文案表：中文文案以唯一 key 组织，导入场景中已有的 TextMesh 组件，只改 m_Text，不触碰结构
- 字体：使用子集化后的中文字体与纹理图集，避免打包全量 CJK 字形导致的体积膨胀

## 当前状态

| 模块 | 状态 |
|---|---|
| 开场动画 | 已验证 |
| 主菜单（含中文界面） | 已验证 |
| 关卡 / 战斗 | 待真机验证 |
| Android 构建链路 | 已打通（IL2CPP / ARM64） |

## 贡献

欢迎提 Issue 与 PR。提交前请确认：

1. 未引入 Library/、Logs/、Builds/ 等生成物
2. 中文文案改动同时更新文案表
3. 新字体 / 新字符需重新生成字体子集与纹理图集

## 许可证

见 LICENSE（MIT）。

## 素材版权提示

请在正式发布前确认第三方素材（模型、贴图、音频、字体）的授权情况；如素材非自有，请勿随代码一并开源发布。
