Totem Runner 本地还原移植工程

工作工程：D:\User\Desktop\UnityAndroidTest\TotemRunner
Unity：2021.3.45f2c1（本机已安装版本）
目标：小米 15 / Android 16 / HyperOS 3 / ARM64，OpenGL ES 3
应用包名：local.totemrunner.restored

原件是 Unity 3.5.4f1 + ARM32 的 Totem Runner 1.0.1。
原 APK SHA-256：108ffbdb8a383a798cc124347aaa391e4bf1a486b13d5794d09da677305c690e
原 APK 保持原样。反编译代码与原始导出基线保存在工作区 Analysis 和 Recovery 目录。

已恢复：21 个场景和原始 build 顺序（Tutorial 索引 3）、原版逻辑、598 个网格、137 个蒙皮网格、207 个 Legacy Animation、694 个材质、13 个原始 ShaderLab 源码、91 段声音、3 个位图字体和原版 intro.mp4。
同时修复了自动导出遗漏的激活状态、顶点布局、三角形条带、材质属性、字距和纹理 wrap 参数。
旧粒子和 GUIText 使用现代兼容组件；旧 ARM32 平台插件已改为托管接口。在线购买、社交和评分接口返回不可用。

构建：在 PowerShell 执行：
& 'D:\User\Desktop\UnityAndroidTest\Tools\Build-TotemRunner.ps1'

安装并启动已构建的 APK：
& 'D:\User\Desktop\UnityAndroidTest\Tools\Run-TotemRunner.ps1'
此脚本默认使用本次连接的小米设备 serial 1d16aa01，可用 -DeviceSerial 指定其他已授权设备。

APK：TotemRunner\Builds\TotemRunner-ARM64.apk
构建日志：Analysis\TotemRunner\unity-android-build.log
当前验证状态：Analysis\TotemRunner\restoration-status.json
各项源数据核对、恢复统计及局限均记录在 Analysis\TotemRunner 的 JSON 中。

真机验证（2026-10-20）：首个可玩移植版已安装到本次连接的小米 15，应用名称 Totem Runner Restored。
用户确认教学提示、变鹰、变野猪和飞行控制正常；日志记录 Tutorial 完成后进入 B01_Lv_01。第一关中已记录危险触发、死亡动画后回到出生点继续前进；本轮捕获日志没有 Unity Error，先前重生空引用没有再出现。
原始 APK 保持不变。当前安装包为 ARM64 / GLES3 开发测试版，包含诊断日志。

本轮补充修复：59 个相机投影模式、193 张旧 Android RGBA4444 纹理解码、739 个 TextMesh 透明颜色、3 套字体的缩放/字距倍率、18 个角色移动组件启用状态、1,330 个原始凹形触发器兼容，以及旧粒子暂停/重启/销毁生命周期。
原生专项验证：碰撞 20/20、粒子 7/7、字体 444 个字距及真实渲染、角色移动修复后重新加载验证均通过。

操作：从 Play 进入。教学关开始会隐藏变身控件，到对应教学点才开放；提示出现后按提示点按并松手继续。解锁后按住左半屏变野猪，按住右半屏变鹰，松手回人形。飞行可按原版教学选择按钮或倾斜模式。

验证边界：其余关卡、Boss、存档跨重启和原版手感/粒子视觉尚未全部验收。在线购买、社交、评分服务不可用。凹网格触发器保留原几何，通过查询兼容旧回调；其时间粒度差异记录在 concave-trigger-conversion.json。
