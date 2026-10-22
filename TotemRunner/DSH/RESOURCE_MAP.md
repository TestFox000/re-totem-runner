# 汉化入口静态审查参考

审查日期：2026-10-20。本次为子 Agent只读源码/资源清点，不是 Unity 导入或手机汉化验收。数量作为 TR-002 的核对起点，完成词条分类、UI 目视检查后才能冻结清单；不要每轮重扫。

## 已定位

| 范围 | 观察 | 工程内路径/消费者 |
|---|---|---|
| TextMesh | 739 实例，场景 736、Prefab 3；66 种原始 m_Text 值，含数字和占位，不能直接当 66 词条 | Assets/Levels/**/*.unity；Assets/GameObject/AchLinePrefab.prefab |
| 配置副本 | 20 份 GameSettings 文本配置相同；6 章节名、18 关名、52 成就名、51 非空描述 | 各场景；Assets/Scripts/Assembly-CSharp/GameSettings.cs |
| 动态覆盖 | 菜单锁定、操作/难度/音量、得分、结算与服务不可用文本可能在运行时覆盖场景 | ChooseLevelGUI.cs、GuiGameHud.cs、StatisticScreen.cs、CounterUI.cs、ScreenStore.cs |
| 消息触发 | 20 个 ShowMessageTrigger，18 处使用 $levelName，另两种英文提示各出现两次 | Assets/Scripts/Assembly-CSharp/ShowMessageTrigger.cs 与场景 |
| 教学图片 | 14 张 512×512 JPEG .jpg.bytes，由 25 个 TutorialMessage 与 1 个 TutorialMessageChoose 使用；死亡图复用于 18 场景 | Assets/TextAsset/*.jpg.bytes；TutorialMessage.cs → GUtils.LoadPictImage → LoadImage |
| 其他烘焙文字 | StampNewRecord.png、LoadingScreen.png 已确认有英文；其他 UI 图片仍需目视核对 | Assets/Texture2D/ |
| 字体 | 三套 bitmap Font 字形数 95/254/95，CJK 为 0；字体材质/Alpha8 atlas 与消费者绑定 | Assets/Font/*.asset 及关联 Font Material/atlas |
| 兼容 GUIText | 36 实例；兼容实现选指定字体或 Arial；需单独验证汉字，不假定系统字体自动可用 | Assets/Compatibility/GUIText.cs |

14 张教学图的名称：Boar、Eagle、Man、Death、Stars、Gems、Visilisk、Evil_Totem、Tutor_Eagle_Button、Tutor_Eagle_Tild、ControlButton1/3、ControlTild_01/03。按磁盘真实文件名映射，避免把斜杠记法当文件名。

## 必须保留的机器语义

- 不翻译 GameObject/场景/动画/按钮事件内部 ID、资源路径、PlayerPrefs 键。
- 保留 $levelName 运行时替换、成就描述 # 分行以及模板 /n、/t 的真实解析规则；在消费代码中核对，不盲改成常规转义。
- 去重翻译，按对象 fileID/字段/路径映射应用；20 份配置从同一词条源生成，避免各 Agent独立改出差异。
- .jpg.bytes 是运行时解码数据，不能按 TextAsset 直接替换英文字符串。保留尺寸/编码/比例和消费者假设。
- 需要多角色并行时：字体、新文本表、动态脚本、图片分别租约；场景/Prefab 由唯一集成者串行应用，其他人交映射或补丁建议。

## 尚待确认

66 个原始值中的真实可见集合、未按名称命中的图片文字、视频字幕是否有必要。LevelTaskManager 的 18 份 tasks 和 CutSceneController 的 cutScenes 当前为空，不能因代码中有模板就声称玩家会看到。

TR-002 输出应为：
id / category / source_path / object_file_id / field_or_key / original / context / consumer / placeholder_rules / translation / review_status / implementation_status / screenshot_or_waiver。

文本覆盖率分母取“已确认用户可见的唯一词条及图片清单”，对象覆盖另计 739 TextMesh、36 GUIText、20 配置副本、26 教学组件的分类率；两种分母不可混用。

