-- 确保使用正确的数据库
USE OrderSystemDB;
GO

-- 检查表是否存在，如果存在则删除（仅用于开发环境）
IF OBJECT_ID('dbo.OperationLogs', 'U') IS NOT NULL DROP TABLE dbo.OperationLogs;
IF OBJECT_ID('dbo.CancellationRecords', 'U') IS NOT NULL DROP TABLE dbo.CancellationRecords;
IF OBJECT_ID('dbo.SuspensionRecords', 'U') IS NOT NULL DROP TABLE dbo.SuspensionRecords;
IF OBJECT_ID('dbo.Orders', 'U') IS NOT NULL DROP TABLE dbo.Orders;
IF OBJECT_ID('dbo.Customers', 'U') IS NOT NULL DROP TABLE dbo.Customers;
IF OBJECT_ID('dbo.MilkTypes', 'U') IS NOT NULL DROP TABLE dbo.MilkTypes;
IF OBJECT_ID('dbo.DeliveryStaff', 'U') IS NOT NULL DROP TABLE dbo.DeliveryStaff;
IF OBJECT_ID('dbo.Channels', 'U') IS NOT NULL DROP TABLE dbo.Channels;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

-- 用户表
CREATE TABLE [dbo].[Users](
    [UserId] [int] IDENTITY(1,1) NOT NULL,
    [Username] [nvarchar](50) NOT NULL,
    [Password] [nvarchar](50) NOT NULL,
    [Role] [nvarchar](20) NOT NULL, -- Admin, Statistician
    [FullName] [nvarchar](100) NULL,
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [CreatedTime] [datetime] NOT NULL DEFAULT GETDATE(),
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
([UserId] ASC)
);

-- 渠道表
CREATE TABLE [dbo].[Channels](
    [ChannelId] [int] IDENTITY(1,1) NOT NULL,
    [ChannelName] [nvarchar](100) NOT NULL,
    [Description] [nvarchar](200) NULL,
    [IsActive] [bit] NOT NULL DEFAULT 1,
 CONSTRAINT [PK_Channels] PRIMARY KEY CLUSTERED 
([ChannelId] ASC)
);

-- 配送人员表
CREATE TABLE [dbo].[DeliveryStaff](
    [StaffId] [int] IDENTITY(1,1) NOT NULL,
    [StaffName] [nvarchar](50) NOT NULL,
    [Phone] [nvarchar](20) NULL,
    [IsActive] [bit] NOT NULL DEFAULT 1,
 CONSTRAINT [PK_DeliveryStaff] PRIMARY KEY CLUSTERED 
([StaffId] ASC)
);

-- 奶品种类表
CREATE TABLE [dbo].[MilkTypes](
    [MilkTypeId] [int] IDENTITY(1,1) NOT NULL,
    [TypeName] [nvarchar](50) NOT NULL,
    [Unit] [nvarchar](20) NOT NULL, -- 瓶、盒等
    [DefaultPrice] [decimal](10,2) NULL,
    [IsActive] [bit] NOT NULL DEFAULT 1,
 CONSTRAINT [PK_MilkTypes] PRIMARY KEY CLUSTERED 
([MilkTypeId] ASC)
);

-- 客户表
CREATE TABLE [dbo].[Customers](
    [CustomerId] [int] IDENTITY(1,1) NOT NULL,
    [CustomerName] [nvarchar](100) NOT NULL,
    [Phone] [nvarchar](20) NOT NULL,
    [Address] [nvarchar](200) NOT NULL,
    [ChannelId] [int] NOT NULL,
    [StaffId] [int] NOT NULL,
    [OrderStatus] [nvarchar](20) NOT NULL DEFAULT '正常配送', -- 正常配送、停奶、退订
    [Remark] [nvarchar](500) NULL,
    [CreatedTime] [datetime] NOT NULL DEFAULT GETDATE(),
    [LastUpdatedTime] [datetime] NOT NULL DEFAULT GETDATE(),
 CONSTRAINT [PK_Customers] PRIMARY KEY CLUSTERED 
([CustomerId] ASC),
 CONSTRAINT [FK_Customers_Channels] FOREIGN KEY([ChannelId])
 REFERENCES [dbo].[Channels] ([ChannelId]),
 CONSTRAINT [FK_Customers_DeliveryStaff] FOREIGN KEY([StaffId])
 REFERENCES [dbo].[DeliveryStaff] ([StaffId])
);

-- 订单表
CREATE TABLE [dbo].[Orders](
    [OrderId] [int] IDENTITY(1,1) NOT NULL,
    [CustomerId] [int] NOT NULL,
    [StartDate] [date] NOT NULL,
    [EndDate] [date] NULL,
    [MilkTypeId] [int] NOT NULL,
    [Quantity] [int] NOT NULL,
    [UnitPrice] [decimal](10,2) NOT NULL,
    [DiscountRate] [decimal](5,2) NOT NULL DEFAULT 1.00,
    [GiftMilkTypeId] [int] NULL,
    [GiftQuantity] [int] NOT NULL DEFAULT 0,
    [GiftUnitPrice] [decimal](10,2) NULL,
    [DeductionAmount] [decimal](10,2) NOT NULL DEFAULT 0.00,
    [DeductionReason] [nvarchar](200) NULL,
    [TotalAmount] [decimal](10,2) NOT NULL,
    [ActualAmount] [decimal](10,2) NOT NULL,
    [IsOriginal] [bit] NOT NULL DEFAULT 1,
    [OriginalOrderId] [int] NULL,
    [ModifiedBy] [int] NULL,
    [ModifiedTime] [datetime] NULL,
    [IsActive] [bit] NOT NULL DEFAULT 1,
 CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED 
([OrderId] ASC),
 CONSTRAINT [FK_Orders_Customers] FOREIGN KEY([CustomerId])
 REFERENCES [dbo].[Customers] ([CustomerId]),
 CONSTRAINT [FK_Orders_MilkTypes] FOREIGN KEY([MilkTypeId])
 REFERENCES [dbo].[MilkTypes] ([MilkTypeId]),
 CONSTRAINT [FK_Orders_GiftMilkTypes] FOREIGN KEY([GiftMilkTypeId])
 REFERENCES [dbo].[MilkTypes] ([MilkTypeId]),
 CONSTRAINT [FK_Orders_OriginalOrders] FOREIGN KEY([OriginalOrderId])
 REFERENCES [dbo].[Orders] ([OrderId]),
 CONSTRAINT [FK_Orders_Users] FOREIGN KEY([ModifiedBy])
 REFERENCES [dbo].[Users] ([UserId])
);

-- 停奶记录表
CREATE TABLE [dbo].[SuspensionRecords](
    [SuspensionId] [int] IDENTITY(1,1) NOT NULL,
    [CustomerId] [int] NOT NULL,
    [SuspensionStartDate] [date] NOT NULL,
    [SuspensionEndDate] [date] NOT NULL,
    [Reason] [nvarchar](200) NULL,
    [CreatedBy] [int] NOT NULL,
    [CreatedTime] [datetime] NOT NULL DEFAULT GETDATE(),
    [IsActive] [bit] NOT NULL DEFAULT 1,
 CONSTRAINT [PK_SuspensionRecords] PRIMARY KEY CLUSTERED 
([SuspensionId] ASC),
 CONSTRAINT [FK_SuspensionRecords_Customers] FOREIGN KEY([CustomerId])
 REFERENCES [dbo].[Customers] ([CustomerId]),
 CONSTRAINT [FK_SuspensionRecords_Users] FOREIGN KEY([CreatedBy])
 REFERENCES [dbo].[Users] ([UserId])
);

-- 退订记录表
CREATE TABLE [dbo].[CancellationRecords](
    [CancellationId] [int] IDENTITY(1,1) NOT NULL,
    [CustomerId] [int] NOT NULL,
    [CancellationDate] [date] NOT NULL,
    [Reason] [nvarchar](500) NOT NULL,
    [CreatedBy] [int] NOT NULL,
    [CreatedTime] [datetime] NOT NULL DEFAULT GETDATE(),
 CONSTRAINT [PK_CancellationRecords] PRIMARY KEY CLUSTERED 
([CancellationId] ASC),
 CONSTRAINT [FK_CancellationRecords_Customers] FOREIGN KEY([CustomerId])
 REFERENCES [dbo].[Customers] ([CustomerId]),
 CONSTRAINT [FK_CancellationRecords_Users] FOREIGN KEY([CreatedBy])
 REFERENCES [dbo].[Users] ([UserId])
);

-- 操作日志表
CREATE TABLE [dbo].[OperationLogs](
    [LogId] [int] IDENTITY(1,1) NOT NULL,
    [UserId] [int] NOT NULL,
    [OperationType] [nvarchar](50) NOT NULL,
    [OperationContent] [nvarchar](max) NOT NULL,
    [OperationTime] [datetime] NOT NULL DEFAULT GETDATE(),
    [IpAddress] [nvarchar](50) NULL,
 CONSTRAINT [PK_OperationLogs] PRIMARY KEY CLUSTERED 
([LogId] ASC),
 CONSTRAINT [FK_OperationLogs_Users] FOREIGN KEY([UserId])
 REFERENCES [dbo].[Users] ([UserId])
);

-- 插入默认数据
INSERT INTO [dbo].[Users]([Username], [Password], [Role], [FullName], [IsActive])
VALUES ('admin', '123456', 'Admin', '系统管理员', 1);

INSERT INTO [dbo].[Channels]([ChannelName], [Description], [IsActive])
VALUES ('线上订单', '通过线上平台下单的客户', 1),
       ('线下订单', '通过线下门店下单的客户', 1),
       ('其他单位福利', '单位福利订单', 1);

INSERT INTO [dbo].[DeliveryStaff]([StaffName], [Phone], [IsActive])
VALUES ('张三', '13800138001', 1),
       ('李四', '13800138002', 1);

INSERT INTO [dbo].[MilkTypes]([TypeName], [Unit], [DefaultPrice], [IsActive])
VALUES ('纯牛奶', '瓶', 5.00, 1),
       ('酸奶', '瓶', 6.00, 1),
       ('高钙奶', '瓶', 7.00, 1);
GO