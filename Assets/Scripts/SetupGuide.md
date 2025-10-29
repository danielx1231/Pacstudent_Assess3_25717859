# Unity 设置指南

## 1. PacStudent 设置

### 创建 PacStudent 对象：
1. 在场景中创建一个空的 GameObject，命名为 "PacStudent"
2. 添加以下组件：
   - `PacStudentController` 脚本
   - `Animator` 组件
   - `AudioSource` 组件
   - `DustParticleEffect` 脚本
   - `Collider2D` 组件（用于碰撞检测）

### 配置 PacStudentController：
- **Move Speed**: 5.0（移动速度）
- **Grid Size**: 1.0（网格大小）
- **Move Audio Source**: 拖入 AudioSource 组件
- **Moving Clip**: 拖入移动音效
- **Eating Clip**: 拖入吃豆子音效
- **Animator**: 拖入 Animator 组件
- **Dust Effect**: 拖入 DustParticleEffect 组件
- **Wall Layer Mask**: 设置为墙壁层
- **Pellet Layer Mask**: 设置为豆子层

## 2. Cherry 系统设置

### 创建 Cherry 控制器：
1. 创建空对象，命名为 "CherryController"
2. 添加 `CherryController` 脚本

### 配置 CherryController：
- **Cherry Prefab**: 创建樱桃预制体并拖入
- **Spawn Delay**: 5.0（生成延迟）
- **Move Speed**: 2.0（移动速度）
- **Level Center**: (0, 0)（关卡中心）
- **Level Size**: (20, 20)（关卡大小）
- **Sorting Order**: 10（渲染顺序）

### 创建樱桃预制体：
1. 创建 Sprite 对象
2. 添加 SpriteRenderer 组件
3. 设置樱桃图片
4. 添加 Collider2D 组件
5. 设置 Tag 为 "Cherry"
6. 保存为预制体

## 3. 关卡生成器设置

### 创建关卡生成器：
1. 创建空对象，命名为 "LevelGenerator"
2. 添加 `LevelGenerator` 脚本

### 配置 LevelGenerator：
- **Level Size**: (20, 20)（关卡大小）
- **Grid Size**: 1.0（网格大小）
- **Level Center**: (0, 0)（关卡中心）
- **Wall Prefab**: 创建墙壁预制体
- **Ghost Wall Prefab**: 创建幽灵墙预制体
- **Pellet Prefab**: 创建豆子预制体
- **Pac Student**: 拖入 PacStudent 对象
- **Cherry Controller**: 拖入 CherryController 对象

## 4. 游戏管理器设置

### 创建游戏管理器：
1. 创建空对象，命名为 "GameManager"
2. 添加 `GameManager` 脚本

### 配置 GameManager：
- **Level Generator**: 拖入 LevelGenerator 对象
- **Pac Student**: 拖入 PacStudent 对象
- **Cherry Controller**: 拖入 CherryController 对象
- **Audio Player**: 拖入 AudioPlayer 对象（如果存在）

## 5. 灰尘粒子效果设置

### 配置 DustParticleEffect：
- **Dust Particles**: 会自动创建 ParticleSystem
- **Emission Rate**: 50.0（发射率）
- **Particle Lifetime**: 0.5（粒子生命周期）
- **Particle Speed**: 2.0（粒子速度）
- **Particle Size**: 0.1（粒子大小）
- **Dust Color**: 设置灰尘颜色

## 6. 动画设置

### Animator 控制器设置：
1. 创建 Animator Controller
2. 添加 "Direction" 整数参数
3. 创建状态机，包含四个方向的状态
4. 设置过渡条件：
   - Direction = 0: 向上
   - Direction = 1: 向右
   - Direction = 2: 向下
   - Direction = 3: 向左

## 7. 音频设置

### 音频源配置：
1. 设置 AudioSource 组件：
   - **Play On Awake**: false
   - **Loop**: true（用于移动音效）
2. 准备音频文件：
   - 移动音效（循环播放）
   - 吃豆子音效（单次播放）

## 8. 层级设置

### 创建必要的层级：
1. 创建 "Wall" 层级
2. 创建 "Pellet" 层级
3. 创建 "PacStudent" 层级
4. 创建 "Cherry" 层级

### 设置碰撞检测：
1. 确保 PacStudent 的 Collider2D 设置为 Trigger
2. 设置适当的物理材质

## 9. 预制体创建

### 墙壁预制体：
1. 创建 Sprite 对象
2. 添加 SpriteRenderer
3. 设置墙壁图片
4. 添加 Collider2D
5. 设置层级为 "Wall"
6. 保存为预制体

### 豆子预制体：
1. 创建 Sprite 对象
2. 添加 SpriteRenderer
3. 设置豆子图片
4. 添加 Collider2D
5. 设置层级为 "Pellet"
6. 保存为预制体

### 幽灵墙预制体：
1. 创建 Sprite 对象
2. 添加 SpriteRenderer
3. 设置幽灵墙图片
4. 添加 Collider2D
5. 设置层级为 "Wall"
6. 保存为预制体

## 10. 测试和调试

### 测试步骤：
1. 运行游戏
2. 使用 WASD 键测试 PacStudent 移动
3. 检查动画是否正确播放
4. 检查音频是否正确播放
5. 检查灰尘粒子效果
6. 检查樱桃生成和移动
7. 检查豆子收集功能

### 常见问题：
- 如果移动不流畅，检查 Grid Size 设置
- 如果动画不播放，检查 Animator 参数设置
- 如果音频不播放，检查 AudioSource 配置
- 如果粒子效果不显示，检查 ParticleSystem 设置