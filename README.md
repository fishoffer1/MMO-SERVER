# MMO-SERVER

一个使用 **.NET 8 / C#** 构建的多人在线 RPG（MMO）游戏服务端，覆盖网络通信、账号与角色、地图与实体同步、战斗、怪物 AI、背包等核心系统，并配套 Unity 客户端。

> 目标：跑通「登录 → 选择角色 → 进入地图 → 移动同步 → 打怪掉落 → 聊天」的完整闭环。

---

## 技术栈

| 分类 | 技术 |
| --- | --- |
| 语言 / 运行时 | C# · .NET 8（`Nullable` / `ImplicitUsings` 开启） |
| 网络 | TCP 长连接框架 Summer（异步 Socket · 长度前缀拆包 · 发布订阅消息分发） |
| 通信协议 | Google Protobuf |
| 数据持久化 | FreeSql 3.5 + FreeSql.Repository + MySQL Provider |
| 配置序列化 | Newtonsoft.Json |
| 日志 | Serilog（Async / Console / File，按天滚动） |

---

## 功能特性

- **网络层**：TCP 长连接、心跳保活、粘包/分包处理、多工作线程消息分发
- **账号 / 角色**：注册登录、角色创建 / 删除 / 选择，数据落库
- **地图与同步**：多场景（Space）管理，实体进出场，位置与属性增量同步
- **战斗系统**：属性系统、技能释放与吟唱、弹道飞行、伤害结算（暴击 / 闪避）
- **怪物 AI**：基于有限状态机的「巡逻 / 追击 / 脱战返回」
- **背包系统**：物品、装备、消耗品、材料
- **配置驱动**：物品 / 技能 / 地图 / 刷怪 / 单位全部 JSON 配置，改数值无需重编译
- **聊天**：聊天消息转发

---

## 解决方案结构

```
MMO-SERVER/
├── Common/                  # 公共库
│   ├── Database/            # FreeSql 初始化与数据实体
│   └── Summer/              # 网络框架
│       ├── Core/            # DataStream 二进制编解码、类型缓存
│       ├── Network/         # TcpServer / Connection / SocketReceiver / MessageRouter
│       └── Proto/           # Protobuf 生成的消息类
├── GameServer/              # 游戏主逻辑（可执行程序）
│   ├── AI/                  # 怪物 AI（基于 FSM）
│   ├── Core/                # 游戏主循环、会话、数学与向量
│   ├── Data/                # JSON 数值配置
│   ├── Define/              # 配置结构定义
│   ├── Fight/               # 战斗：属性 / 技能 / 弹道 / 伤害
│   ├── InventorySystem/     # 背包与物品
│   ├── Mgr/                 # 各类管理器
│   ├── Model/               # 实体模型
│   └── Service/             # 业务服务层
└── NetClient/               # 命令行测试客户端
```

---

## 架构说明

服务端按 **接入层 → 服务层 → 管理器层 → 模型层** 分层：

```
                    客户端（Unity / NetClient）
                              │
                     Protobuf 二进制协议
                              ▼
┌─────────────────────────────────────────────────────────┐
│ GameServer                                              │
│                                                         │
│  Service 层   NetService / UserService / SpaceService    │
│               BattleService / ChatService               │
│       │ 订阅消息、编排流程                                │
│  Mgr 层       CharacterManager / MonsterManager /        │
│               SpaceManager / SpawnManager / SkillManager │
│               DataManager                                │
│       │                                                 │
│  Model 层     Entity / Actor / Character / Monster /     │
│               Space                                      │
│       │                                                 │
│  Fight 层     Attributes / Skill / Spell / Missile       │
│  AI 层        AIBase / MonsterAI（FSM）                   │
└─────────────────────────────────────────────────────────┘
            │                          │
        FreeSql + MySQL            Serilog
```

**消息流转**：`TcpServer` 异步接收字节流 → `SocketReceiver` 按长度前缀拆包 → 反序列化为 Protobuf 消息 → `MessageRouter` 按消息类型派发给已订阅的 `Service` 方法 → 业务处理 → 广播 / 回包。

**解耦设计**：网络 I/O 与业务逻辑通过消息队列隔离，业务层只订阅自己关心的消息类型，不直接接触 Socket。

---

## 核心模块

### 网络框架（Common/Summer）

- `TcpServer`：基于 `Socket` 的异步监听与接入，向上暴露 `Connected` / `DataReceived` / `Disconnected` 事件
- `SocketReceiver`：处理 TCP 粘包与分包，保证 Protobuf 消息边界正确
- `MessageRouter`：单例消息分发器，`Subscribe<T>()` 按消息类型注册处理器，收到消息入队后由工作线程取出派发（默认启动 10 个工作线程）
- `Schedule`：中心计时器，承载周期任务与心跳检测（默认每 2 秒巡检一次）
- `DataStream` / `DataSerializer`：二进制编解码

### 地图与同步

`Space` 一个实例即一张地图，内部维护角色字典与演员字典，并各自持有 `MonsterManager`、`SpawnManager`、`FightMgr`。客户端定时上报 `SpaceEntitySyncRequest`（位置 / 方向），服务端校验后在场景内广播，实现多人可见的移动同步。

### 战斗系统

技能释放流程：`SpellRequest` → `Skill`（校验吟唱时间与冷却）→ `Missile` 弹道飞行 → 命中后结算 `Damage`（含暴击 `isCrit` / 闪避 `isMiss`）→ 广播伤害表现。属性由 `Attributes` 统一承载，供技能、装备、Buff 读写。

### 怪物 AI

泛型状态机 `FsmSystem<T>`，`MonsterAI` 注册三个状态：`walk`（游荡）/ `chase`（追击）/ `goback`（脱战返回），并结合视野范围与活动半径在每帧 `Update` 中驱动状态切换。

### 配置驱动

`GameServer/Data/*.json` 定义数值，由 `DataManager` 在启动时加载：

| 文件 | 说明 |
| --- | --- |
| `ItemDefine.json` | 物品定义 |
| `SkillDefine.json` | 技能定义 |
| `SpaceDefine.json` | 地图定义 |
| `SpawnDefine.json` | 刷怪定义 |
| `UnitDefine.json` | 单位（角色 / 怪物）定义 |

客户端与服务器共用同一套配置结构，保证双端数值一致。

---

## 快速开始

### 环境要求

- .NET 8 SDK
- MySQL 5.7 / 8.x

### 数据库

服务端使用 FreeSql 的 `UseAutoSyncStructure`，首次运行会依据实体自动建表。请先创建数据库：

```sql
CREATE DATABASE game DEFAULT CHARSET utf8;
```

数据库连接信息位于 `Common/Database/Db.cs`。**请将其改为本地配置或环境变量后再提交，不要提交真实账号密码。**

### 运行

```bash
# 1. 启动游戏服务端（默认监听 0.0.0.0:32510）
dotnet run --project GameServer

# 2. 或运行命令行测试客户端
dotnet run --project NetClient
```

启动后服务端会依次初始化：日志 → JSON 配置 → 网络服务 → 玩家服务 → 地图服务 → 战斗服务 → 中心计时器 → 聊天服务。

---

## 通信协议

全部消息使用 Protobuf 定义，主要协议包括：

| 协议 | 说明 |
| --- | --- |
| `HeartBeatRequest` / `HeartBeatResponse` | 心跳保活 |
| `NetActor` / `NetEntity` | 角色与实体数据 |
| `PropertyUpdate` / `PropertyUpdateResponse` | 属性增量更新 |
| `SpaceEntitySyncRequest` | 地图实体位置同步 |
| `Damage` / `DamageResponse` | 伤害结算与广播 |
| `SpellRequest` | 技能施放 |
| `ChatRequest` / `ChatResponse` | 聊天 |

---

## 配套项目

- 客户端（Unity + C#）：游戏表现与交互，通过同一套 Protobuf 协议与服务端通信

---

## 说明

- 第三方资源与商业插件不在本仓库范围内，请自行准备运行环境。
