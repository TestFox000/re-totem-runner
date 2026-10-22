# 项目结构说明

本工作区包含**两个 Unity 工程**与一些过程产物，为避免混淆，说明如下。

## 一、两个 Unity 工程

| 目录 | 作用 | 关键内容 |
|---|---|---|
| `UnityAndroidTest/`（根） | **构建/打包工程** | `Assets/Editor/AndroidBuilder.cs`（Android 构建脚本）、`Assets/Scenes/Main.unity`、`ProjectSettings/`、`Packages/` |
| `UnityAndroidTest/TotemRunner/` | **游戏本体工程** | `Assets/Levels/`（开场、主菜单、Block_01–04、Titan_Race、Stone_Enemy_Level）、`Assets/Scripts/`（全部游戏逻辑）、中文字体子集、DSH 自动化 |

> 两个工程都有各自的 `ProjectSettings/` 与 `Packages/`，是**独立的 Unity 工程**，不是包含关系。

## 二、过程产物（不入仓库）

| 目录 | 说明 |
|---|---|
| `Analysis/` | 逆向与调试过程产出（素材来源证据、JSON 转储），体积大 |
| `Recovery/` | 导出工程备份 |
| `TotemRunner/DSH/_proto/` | 字体图集比对用的临时图片 |
| `TotemRunner/DSH/reports/`、`cards/` | 自动化过程报告 |
| `Library/`、`Logs/`、`Temp/`、`Builds/`、`UserSettings/` | Unity 缓存 / 构建产物 / 编辑器设置 |

这些目录已在 `.gitignore` 中排除，但**文件仍在本地保留**。

## 三、关键文件

| 文件 | 说明 |
|---|---|
| `README.md` | 项目介绍（GitHub 展示用） |
| `LICENSE` | MIT 许可证 |
| `.gitignore` / `.gitattributes` | 版本控制规则 |
| `TotemRunner/DSH/STATE.md` | 当前任务与进度 |
| `TotemRunner/DSH/DECISIONS.md` | 关键设计决策记录 |
| `TotemRunner/DSH/QUALITY_GATES.md` | 质量门禁 |
| `Tools/` | 素材恢复与构建辅助脚本 |
