# 当前接续状态

更新时间：2026-10-21 21:00（北京时间）。TR-004/005 done；TR-006 场景层/TR-008 phase1+2 已并入；TR-013 字体**竖直基线**修正完成（run-13 候选在设备上）。

## 一句话进度

汉化链路全通：187 条译文定稿（TR-004）→ 全量字符集字体图集真机 pass（TR-005 v2，run-04）→ 菜单/设置接入中（TR-006）。设备装的是 run-04 候选。

## 2026-10-21 恢复段已完成

- TR-004 attempt 2 收口并**验收 done**（D-026）：zh-CN.csv 187/187 逐 key 对齐、结构 0 错、父裁决落地、files.sha256 17 条重算一致；validate_zhcn.py exit 0（over_limit 11 条按裁决保留标注）。task-4 completed。
- 全量字符集生成：run-20261021-04/charset-full.txt（436 汉字 + 16 标点 = attempt2 的 426 + 强制 6 + 术语独有 10）。
- 字体 v2：三套图集按全量字符集重生成（547/701/547 条目），场景保持试点 4 条中文；构建 run-04 **25cc5fbfaf96…**（207,086,613 B），安装成功。
- 真机 pass：菜单「开始/选项/重置」渲染完整（menu_52s.png）、启动视频回归正常（video_15s.png）；采证后 force-stop，未注入触摸。证据 run-20261021-04/（validation.json、apply-log.json、README、screenshots、logs）。
- 已知 errata（记录级）：attempt2 charset 的 ASCII 节声明 32 实为 31 字符；validate-output.txt 是 UTF-16LE；files.sha256 是“size 在前”格式——均不影响数据层。

## TR-006 进行中（localization）

- 范围：MainMenu.unity（菜单按钮/弹窗 tl1-3/确认框/About/TOS 标题）+ 设置相关 TextMesh，按 zh-CN.csv 逐 unique_key 接入；只改 m_Text 值，不动结构/字号/对象名。
- 边界：不构建、不碰设备、不碰字体图集（父 Agent 独占）；改完交父 Agent 构建 run-05 + 真机截图。
- 写租约：Assets/Levels/MainMenu.unity 等场景文件（见 board.json file_leases）。

## 待人工操作（设备旁）

1. **F04**：打开游戏（当前 run-04），点「开始」进教程，看提示是否为「滑动变身」，回我一声即可（或截图）。
2. TR-003 四项：听启动视频声音 / 播放中跳过一次 / Home 后台恢复 / Play→变身→飞行→死亡复活冒烟。
（禁止 adb input 注入，全部需真人手指。）

## 版本与回退

- candidate：run-04 25cc5fbf…（Builds 固定路径同名；不可变副本 run-20261021-04/candidate）。
- 回退链：run-03（6a094eee…）→ run-02（62564f93…）→ run-01（3173464c…）。字体/场景原件备份 run-20261020-03/backup。
- 构建 Development+debug 签名，仅本机测试。

## 原生卡与调度

| 卡 | 原生任务 | 状态 | 负责人 |
|---|---|---|---|
| TR-001 | task-1 | done | lead |
| TR-002 | task-2 | done | localization |
| TR-003 | task-3 | waiting_device（人工×4） | runtime |
| TR-004 | task-4 | done（attempt 2） | localization |
| TR-005 | task-5 | partial_pass（F04 待人工） | lead |
| TR-006 | task-6 | running（YAML 接入） | localization |

其余 backlog。子 Agent 认领不标 done；父 Agent 验收。

## 注意事项

- 构建前冻结源写入、核实无 Unity 进程；构建后先另存 run 目录再测试。
- 历史 7e16… APK 副本仍未定位，不可声称可回退它。
- 设备是用户正在用的手机：采证用冷启动+截图，完事 force-stop 交还。


## 2026-10-21 19:18 增量（TR-006 验收 + TR-008 开工）

- TR-006 attempt 1 独立复核：14 行 diff 全为 m_Text、fileID 归属正确、UTF-8 无 BOM、CRLF/行数/文档数不变、试点三项保留；备份/改后哈希与报告一致。
- run-05 候选 82a59ae9…（207,086,613 B）已装设备（拉取 base.apk 同哈希）；19:11:50 冷启动采证：主菜单「开始/选项/重置」完整、视频 30.347s 回归 pass；APK level1 内 14 条新串烘焙命中。证据 Analysis/TotemRunner/runs/run-20261021-05/（validation.json/README/screenshots/logcat-utf8）。
- 坑记录：PowerShell 重定向 screencap/logcat 会损坏 PNG 且 logcat 是 UTF-16LE——采证脚本必须用 Node execFileSync 二进制捕获或显式转码。
- 已知边界（预期行为）：弹窗/锁提示/Options 值/难度值运行时被 ChooseLevelGUI.cs 英文覆盖 → TR-008 修。
- TR-008 attempt 1 已派 localization（phase 1）：ChooseLevelGUI.cs 11 处显示串 + MainMenu 2731 序章 + 2738/2741 商店；phase 2 只诊断 config 名（关卡/成就 name 是否查找键、descr 与 $levelName 关系、GUIText/Arial 替代方案）。租约已写 board.json。
- 设备现停在主菜单（run-05），未注入触摸；待人工：L04 子页面（点 (i)/奖杯/重置看弹窗）+ TR-003 四项（声音/跳过/后台/玩法冒烟）。
- 回退链：TR-006 前 MainMenu=run-20261021-05/backup；run-04 APK=25cc5fbf…；run-01=3173464c…。


## 2026-10-21 19:35 增量（TR-008 phase1 验收 + phase2 派发）

- phase1 独立复核：ChooseLevelGUI.cs 12 行全为显示字面量（diff 逐行核对）；MainMenu 4 行 m_Text（2697 BlockTitle/2731 LevelLabel→序章，2738 mStore/2741 btnBuyFull→商店）。
- run-06 候选 28a765aa…（207,087,065 B）已装设备并拉取同哈希；19:29:58 冷启动：主菜单「开始/商店/选项/重置」全中文、视频 30.378s 回归、0 Unity 错误；APK 内 商店 在 level1、.cs 串在 global-metadata.dat。
- 运行时弹窗/选项/锁提示需人工点按（未注入输入）；章节标题/关卡名由 ReadAllLevelData 从配置覆盖，仍英文 → phase2。
- phase2 已派 localization：20 份 GameSettings（blockName Tutorial→序章、18 关卡名、52 成就 name+descr、descr 保 1 个 #、descr 内关卡名同批替换）+ 22 条 ShowMessageTrigger（保 $levelName）+ helpMessageText + GuiGameHud.cs:1258；备份 run-20261021-07/backup。
- 回退链：run-05 候选 82a59ae9…；TR-006 前 MainMenu 备份在 run-05/backup。


## 2026-10-21 19:52 增量（用户反馈四项 → run-08）

- 用户反馈：汉字整体偏下；局内变身没有按钮；鹰形态陀螺操控手感差；帧率低。
- 已修：字体基线（生成器 baseline=ascent，2048 图集，566 字并集，从原始备份重建）→ 真机主菜单截图已对齐；release 构建 + 60fps 上限（菜单/视频实测 60.2-60.7，Development 水印消失）。
- 待证：局内按钮缺失（材质/贴图都在，需游戏内截图定位）；陀螺手感（60fps 后复测；必要时区分“重力感应/重力感应2”语义或调 accelParam）；局内 FPS（有 [FPS] 日志，每 10s 一行）。
- 首次安装后启动偏慢（约 43s 到 Unity 初始化）——冷启动首次 dexopt/资源导入可能性大，二次启动应恢复；已记录，暂不处理。
- 回退链：run-08/font-apply-log.json 记录字体前后哈希；run-20261021-05/backup/MainMenu 与 run-07/backup 保存 TR-006/008 前状态；run-06/07 APK 均在各自 run 目录。


## 2026-10-21 20:35 run-10（字体重做 + HUD 绘制修复 + 陀螺调参）
- 字体：改幼圆(SIMYOU)+微描边，字高按英文大写 1.12× 重算（原 1.35× 偏大），CJK 基线 +33/+31/+16.2 单位分字体上移 → 菜单文字落在蓝条正中（run-09 截图）。
- HUD 按钮不显示：①旧 GuiTexture 着色器是固定管线 SetTexture 语法，GLES3 下不渲染 → 重写为 CG；②按钮在 OnPreRender 用 DrawMeshNow 立即绘制，2021 相机清屏后可能被抹 → 改到 OnPostRender。
- 陀螺（重力感应1）：GetDeviceAngle 加 70ms 低通；maxDelta 0.3→0.42、minDelta 0.05→0.07（满速 24°、死区 4°）。
- run-10 候选 efbde5cf… 已装机（187,947,951 B）；进入选关页正常，关卡名中文（沉睡森林）已生效。
- 待用户验证：进关后四个按钮是否出现；陀螺手感。

## 2026-10-21 20:35 run-11（用户 5 项反馈）
- 飞行升降抖动 → 重力感应1 改为**比例映射**（原棘轮累加会产生连续小幅加力→抖动）；死区 0.085 rad、满速 0.45 rad、输出 MoveTowards(6/s)、低通 0.12s。
- 升降按钮断触 → 触摸按 fingerId **锁存**（漂移不失联）；up/down 命中区 1.6×、暂停/幽灵 1.25×。
- 字体偏大 → CJK 1.12×→**1.0×** cap 对齐（tr011_fontgen.py，566 字）。
- 章节商店锁 → IsFreeVersion=false、IsBlockLocked=false、LoadRecords 强制 unlocked。
- 羽蛇/幽灵变身次数 → HUD 固定 x 999、不扣宝石、门槛恒真（确定"羽蛇神"=左下灵体变身按钮，截图确认图标为蛇形）。
- 候选 a4096bd7…（187,873,699 B）已装；用户正在玩教程（截图见 run-11/screenshots a50/a66，按钮已渲染，FPS 59-60）。
- 待用户反馈：抖动是否消失、断触是否解决、字号是否合适、章节是否全开、变身计数是否显示 999。


## 2026-10-21 20:45 增量（用户：字号再小 + 换圆体 → run-12）

- 字体换 **ZCOOL KuaiLe（站酷快乐体）**：566 字全覆盖（PIL 位图法核验 0 缺字），替换 SIMYOU 幼圆；CJK 字高 1.0×→**0.9×大写高**，描边 4%→2%。
- 生成器 TotemRunner/DSH/tools/tr012_fontgen.py（由 tr011 参数化改动）；三套图集 2048²，entries 661/815/661。
- 候选 run-12 **1615b9be…**（187,828,295 B）已装设备；20:40:15 冷启动进教程关（记住上次关卡），局内 FPS 58.8–60.6、0 Unity 错误。菜单页未截到（无输入注入）。
- 回退：run-12/backup 六文件；或 run-11 a4096bd7…。
- 用户已确认：章节全开 ✅、按钮不断触 ✅。

## 2026-10-21 21:00 增量（TR-013 字体竖直修正，run-13）

- 现象：用户连续三轮反馈"字偏下/依旧移位"。三个不同字形尺寸的构建（run-08 cap×1.3、run-09 cap×1.12、run-12 cap×0.9）实测**偏移恒为 +75 px**（蓝板中心 → 汉字 ink 中心），与字形大小无关 ⇒ 常数型错误。
- 根因：Unity TextMesh 的 `vert.y` 就是**字形顶边相对基线的高度**；图集生成器写成 `(cap_center + h/2) - ascent`，多减一个 ascent ⇒ 每字下沉**一个字身**。2.5 px/font-unit × 30.0 units(=ascent 29.578) = 75 px，三点互证。
- 修法：新增 `TotemRunner/DSH/tools/tr013_fontgen.py`（tr012 + 竖直公式去掉 `- ascent`，其余不变：ZCOOL 快乐体 / 0.9× 大写高 / 2% 描边 / 566 字），三套字体从**原件**重建后写入工程。
- 真机验收（候选 **5bc0708c…**，187,828,315 B，已装、base.apk 同哈希）：主菜单四个按钮文字中心 vs 蓝板中心 = **-0.5 / +1.5 / +2.0 px**（商店那行底板检测被背景污染未计）；局内 60.5–60.7 fps；当前进程 0 条 Unity 异常。
- 证据：`Analysis/TotemRunner/runs/run-20261021-13/`（README / screenshots/menu_final.png / logs/logcat-now.txt / candidate）。
- 坑记录：设备 `input tap/keyevent` 被 MIUI 以 INJECT_EVENTS 拒绝；`monkey` 随机事件可用但会误触（本机曾误启动 com.tencent.tmgp.cf）。**采证不要注入触摸**，等关卡自然结束回主菜单（约 3–4 分钟）或请用户操作。
- 新增待办：局内引导弹窗仍英文——"THE POWER OF COMBINED GEMS WILL RELEASE THE ANCIENT SPIRIT / BLINDING THE DARK ONES WITH ITS SHINING" + "Buy Gems"（不在 TR-004 187 条内，需补翻 + 新增汉字入图集）。
- 回退：`run-13/backup/` 6 个字体/贴图文件覆盖回工程后重建；APK 回 run-12 `1615b9be…`。

## 2026-10-21 21:15 增量（局内宝石引导弹窗中文化 + 文案统一 + 灵体无限次，run-14）

- 用户指令：局内宝石引导弹窗补译；并反馈选项页 "OK" 字体不统一、重置后灵体变身局外显示 999 但局内需重新攒宝石才可用。
- 弹窗：Death.jpg（死亡/宝石引导图）英文 4 行清除后重绘中文——聚合宝石之力 / 将释放远古之灵 / 以其辉光 / 致盲黑暗之物（沿用 TR-004 已定稿译文）；STHUPO 30px 紫渐变+描边，行框按原图实测（y 36-53/69-86/326-343/359-376）。
- 文案：18 个场景 `Buy Gems`→`购买宝石`；MainMenu 两处 `OK`→`确定`（Options 的 ControlsExit 与 About/Tos 的 Ok）。所用汉字均在现有 566 字图集内，未重建字体。
- 灵体(羽蛇神)：GuiGameHud.UpdateCrystalProgress 改为恒启用 + 计数 999 + 进度 1（不再依赖局内宝石）；不扣宝石/不改存档。
- 构建：候选 **d91af7a0…**（187,846,003 B），已装机并冷启动；主菜单中文、FPS 60.0–60.7、0 Unity 异常。
- APK 内容核验：`购买宝石` 命中 18 个 level split；`确定` 命中 level1；`Buy Gems`/`m_Text: OK` 0 命中；新 JPEG 前 400 字节命中 sharedassets3.assets.split7。
- 证据：`Analysis/TotemRunner/runs/run-20261021-14/`（README / apply-log.json / assets.sha256.json / work/Death_cn2.png / candidate）。
- 待人工：进关看死亡弹窗中文、选项页 确定、局内灵体按钮 x999 可直接使用。
- 回退：run-14/backup 两个文件（Death.jpg.bytes、GuiGameHud.cs）覆盖重建；场景文案反向替换；APK 回 run-13 `5bc0708c…`。

