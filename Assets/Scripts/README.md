# PacStudent 和 Cherry 移动系统

这个项目包含了实现 PacStudent 和 Cherry 移动系统的完整脚本集合。

## 脚本说明

### 1. PacStudentController.cs
PacStudent 的主要控制器，实现了基于网格的线性插值移动系统。

**主要功能：**
- 使用 WASD 键控制移动（上、左、下、右）
- 基于网格的线性插值移动，帧率无关
- 支持 `lastInput` 和 `currentInput` 系统，实现类似经典 Pac-Man 的移动感觉
- 集成动画和音频播放
- 支持灰尘粒子效果
- 与 GameManager 集成，处理得分和游戏状态

**设置要求：**
- 需要 Animator 组件用于动画控制
- 需要 AudioSource 用于移动音效
- 需要 DustParticleEffect 组件用于灰尘效果
- 需要设置 wallLayerMask 和 pelletLayerMask

### 2. CherryController.cs
樱桃生成和移动控制器。

**主要功能：**
- 每 5 秒生成一个新的樱桃
- 从关卡边界外的随机位置生成
- 直线移动穿过关卡中心
- 使用 SpriteMask 确保只在关卡内可见
- 支持与 PacStudent 的碰撞检测

**设置要求：**
- 需要樱桃预制体 (cherryPrefab)
- 需要设置关卡边界 (levelCenter, levelSize)
- 可选：SpriteMask 用于可见性控制

### 3. DustParticleEffect.cs
自定义灰尘粒子效果系统。

**主要功能：**
- 创建逼真的灰尘粒子效果
- 支持基于移动方向的粒子方向更新
- 可配置的粒子属性（颜色、大小、速度等）
- 支持渐变颜色和大小变化

**设置要求：**
- 自动创建 ParticleSystem 组件
- 可配置各种粒子参数

### 4. LevelGenerator.cs
关卡生成器，创建网格地图和视觉瓦片。

**主要功能：**
- 生成基于网格的关卡地图
- 支持不同类型的瓦片（墙壁、幽灵墙、豆子）
- 为其他脚本提供关卡数据
- 管理瓦片的实例化和销毁

**瓦片类型：**
- 0: 空瓦片（可通行）
- 1: 墙壁（不可通行）
- 2: 幽灵墙（特殊，从内部可通行）
- 3: 豆子（可收集）

### 5. GameManager.cs
游戏管理器，协调所有游戏组件。

**主要功能：**
- 管理游戏状态（得分、生命值）
- 协调各个控制器
- 处理游戏结束和重启
- 更新 UI 显示

## 使用说明

### 设置步骤：

1. **创建 PacStudent 对象：**
   - 添加 PacStudentController 脚本
   - 设置 Animator、AudioSource、DustParticleEffect 引用
   - 配置移动参数（速度、网格大小等）

2. **创建 Cherry 系统：**
   - 创建空对象，添加 CherryController 脚本
   - 设置樱桃预制体和关卡边界
   - 可选：添加 SpriteMask 用于可见性控制

3. **设置关卡：**
   - 创建空对象，添加 LevelGenerator 脚本
   - 设置瓦片预制体和关卡大小
   - 配置 PacStudent 和 Cherry 控制器的引用

4. **设置游戏管理：**
   - 创建空对象，添加 GameManager 脚本
   - 设置所有组件的引用
   - 配置 UI 元素（可选）

5. **创建灰尘效果：**
   - 在 PacStudent 对象上添加 DustParticleEffect 脚本
   - 在 PacStudentController 中引用该组件

### 重要注意事项：

- 确保所有 LayerMask 设置正确
- 网格大小必须与关卡生成器中的设置一致
- 音频剪辑需要正确分配给相应的 AudioSource
- 动画控制器需要包含 "Direction" 参数（0=上, 1=右, 2=下, 3=左）

## 移动算法说明

PacStudent 的移动系统实现了以下逻辑：

1. 收集玩家输入（WASD）
2. 存储最后输入到 `lastInput`
3. 如果不在移动中：
   - 尝试向 `lastInput` 方向移动
   - 如果可行，设置 `currentInput` 并开始移动
   - 如果不可行，尝试向 `currentInput` 方向移动
   - 如果都不可行，停止移动
4. 使用线性插值在网格位置间移动
5. 移动时播放动画和音效
6. 到达目标位置时检查是否吃到豆子

这个系统确保了类似经典 Pac-Man 的移动感觉，包括输入缓冲和连续移动。