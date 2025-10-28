# BeiMuDairy.OrderSystem

## 项目简介

北牧乳业订单管理系统是一个基于C#开发的PC端应用程序，用于管理客户订单、客户信息以及相关统计功能。该系统提供了友好的用户界面，方便操作人员进行订单的录入、查询、修改和删除等操作。

## 主要功能

- **订单管理**：创建、查看、编辑和删除订单
- **客户管理**：维护客户信息，包括添加、修改、查询客户数据
- **系统管理**：系统配置和用户管理
- **日志记录**：记录系统操作日志，便于追踪和审计
- **数据统计**：提供订单数据的统计和分析功能

## 项目结构

```
BeiMuDairy.OrderSystem/
├── App.config                # 应用程序配置文件
├── BeiMuDairy.OrderSystem.csproj  # 项目文件
├── CustomerManager.cs        # 客户管理类
├── DBHelper.cs               # 数据库操作辅助类
├── Database/                 # 数据库相关文件
│   └── CreateDatabase.sql    # 创建数据库脚本
├── Form1.cs                  # 主窗体
├── Form1.Designer.cs         # 主窗体设计器
├── InputBox.cs               # 输入框类
├── LogManager.cs             # 日志管理类
├── Models.cs                 # 数据模型类
├── OrderManager.cs           # 订单管理类
├── Program.cs                # 程序入口
├── Properties/               # 项目属性
├── SystemManager.cs          # 系统管理类
└── bin/                      # 编译输出目录
```

## 技术栈

- **开发语言**：C#
- **开发框架**：.NET Framework
- **数据库**：SQL Server
- **界面技术**：Windows Forms

## 使用说明

1. 确保已安装.NET Framework运行环境
2. 运行应用程序前，先执行Database目录下的CreateDatabase.sql脚本创建数据库
3. 根据实际环境配置App.config中的数据库连接字符串
4. 运行BeiMuDairy.OrderSystem.exe启动应用程序

## 注意事项

- 请确保有足够的权限访问数据库
- 系统操作会记录到日志文件中，请定期清理日志以避免占用过多磁盘空间
- 建议定期备份数据库以防止数据丢失

## 开发团队

南禾软件信息技术部

## 版权信息

© 2024 北牧乳业. 保留所有权利。