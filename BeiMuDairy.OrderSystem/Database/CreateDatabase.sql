-- 创建数据库
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'OrderSystem')
BEGIN
    CREATE DATABASE OrderSystem;
END
GO

USE OrderSystem;
GO

-- 创建用户表
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE Users (
        UserId INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(50) NOT NULL UNIQUE,
        Password NVARCHAR(50) NOT NULL,
        Role NVARCHAR(20) NOT NULL, -- Admin, Statistician
        FullName NVARCHAR(100) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedTime DATETIME NOT NULL
    );
END
GO

-- 创建渠道表
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Channels' AND xtype='U')
BEGIN
    CREATE TABLE Channels (
        ChannelId INT IDENTITY(1,1) PRIMARY KEY,
        ChannelName NVARCHAR(50) NOT NULL,
        Description NVARCHAR(200),
        IsActive BIT NOT NULL DEFAULT 1
    );
END
GO

-- 创建配送人员表
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DeliveryStaff' AND xtype='U')
BEGIN
    CREATE TABLE DeliveryStaff (
        StaffId INT IDENTITY(1,1) PRIMARY KEY,
        StaffName NVARCHAR(50) NOT NULL,
        Phone NVARCHAR(20),
        IsActive BIT NOT NULL DEFAULT 1
    );
END
GO

-- 创建奶品种类表
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MilkTypes' AND xtype='U')
BEGIN
    CREATE TABLE MilkTypes (
        MilkTypeId INT IDENTITY(1,1) PRIMARY KEY,
        TypeName NVARCHAR(50) NOT NULL,
        Unit NVARCHAR(10) NOT NULL,
        DefaultPrice DECIMAL(10,2) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );
END
GO

-- 创建客户表
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Customers' AND xtype='U')
BEGIN
    CREATE TABLE Customers (
        CustomerId INT IDENTITY(1,1) PRIMARY KEY,
        CustomerName NVARCHAR(100) NOT NULL,
        Phone NVARCHAR(20) NOT NULL UNIQUE,
        Address NVARCHAR(500) NOT NULL,
        ChannelId INT NOT NULL,
        StaffId INT NOT NULL,
        OrderStatus NVARCHAR(20) NOT NULL DEFAULT '正常配送', -- 正常配送, 停奶中, 已退订
        Remark NVARCHAR(500),
        CreatedTime DATETIME NOT NULL,
        LastUpdatedTime DATETIME NOT NULL,
        FOREIGN KEY (ChannelId) REFERENCES Channels(ChannelId),
        FOREIGN KEY (StaffId) REFERENCES DeliveryStaff(StaffId)
    );
END
GO

-- 创建订单表
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Orders' AND xtype='U')
BEGIN
    CREATE TABLE Orders (
        OrderId INT IDENTITY(1,1) PRIMARY KEY,
        CustomerId INT NOT NULL,
        StartDate DATETIME NOT NULL,
        EndDate DATETIME,
        MilkTypeId INT NOT NULL,
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(10,2) NOT NULL,
        DiscountRate DECIMAL(5,2) NOT NULL DEFAULT 100, -- 百分比
        GiftMilkTypeId INT,
        GiftQuantity INT NOT NULL DEFAULT 0,
        GiftUnitPrice DECIMAL(10,2),
        DeductionAmount DECIMAL(10,2) NOT NULL DEFAULT 0,
        DeductionReason NVARCHAR(200),
        TotalAmount DECIMAL(10,2) NOT NULL,
        ActualAmount DECIMAL(10,2) NOT NULL,
        IsOriginal BIT NOT NULL, -- 是否为原始订单
        OriginalOrderId INT, -- 原始订单ID
        CreatedBy INT,
        CreatedTime DATETIME,
        ModifiedBy INT,
        ModifiedTime DATETIME,
        IsActive BIT NOT NULL DEFAULT 1,
        FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
        FOREIGN KEY (MilkTypeId) REFERENCES MilkTypes(MilkTypeId),
        FOREIGN KEY (GiftMilkTypeId) REFERENCES MilkTypes(MilkTypeId),
        FOREIGN KEY (CreatedBy) REFERENCES Users(UserId),
        FOREIGN KEY (ModifiedBy) REFERENCES Users(UserId)
    );
END
GO

-- 创建停奶记录表
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SuspensionRecords' AND xtype='U')
BEGIN
    CREATE TABLE SuspensionRecords (
        SuspensionId INT IDENTITY(1,1) PRIMARY KEY,
        CustomerId INT NOT NULL,
        SuspensionStartDate DATETIME NOT NULL,
        SuspensionEndDate DATETIME NOT NULL,
        Reason NVARCHAR(200) NOT NULL,
        CreatedBy INT NOT NULL,
        CreatedTime DATETIME NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
        FOREIGN KEY (CreatedBy) REFERENCES Users(UserId)
    );
END
GO

-- 创建退订记录表
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CancellationRecords' AND xtype='U')
BEGIN
    CREATE TABLE CancellationRecords (
        CancellationId INT IDENTITY(1,1) PRIMARY KEY,
        CustomerId INT NOT NULL,
        CancellationDate DATETIME NOT NULL,
        Reason NVARCHAR(200) NOT NULL,
        CreatedBy INT NOT NULL,
        CreatedTime DATETIME NOT NULL,
        FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
        FOREIGN KEY (CreatedBy) REFERENCES Users(UserId)
    );
END
GO

-- 创建操作日志表
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='OperationLogs' AND xtype='U')
BEGIN
    CREATE TABLE OperationLogs (
        LogId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        OperationType NVARCHAR(50) NOT NULL,
        OperationContent NVARCHAR(MAX) NOT NULL,
        IpAddress NVARCHAR(50) NOT NULL,
        OperationTime DATETIME NOT NULL,
        FOREIGN KEY (UserId) REFERENCES Users(UserId)
    );
END
GO

-- 创建订单修改日志表
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='OrderModificationLogs' AND xtype='U')
BEGIN
    CREATE TABLE OrderModificationLogs (
        LogId INT IDENTITY(1,1) PRIMARY KEY,
        OrderId INT NOT NULL,
        OriginalOrderId INT NOT NULL,
        CustomerId INT NOT NULL,
        ModifyStartDate DATETIME NOT NULL,
        ModifyEndDate DATETIME NOT NULL,
        OriginalMilkTypeId INT NOT NULL,
        OriginalQuantity INT NOT NULL,
        OriginalUnitPrice DECIMAL(10,2) NOT NULL,
        OriginalDiscountRate DECIMAL(5,2) NOT NULL,
        NewMilkTypeId INT NOT NULL,
        NewQuantity INT NOT NULL,
        NewUnitPrice DECIMAL(10,2) NOT NULL,
        NewDiscountRate DECIMAL(5,2) NOT NULL,
        ModifyType NVARCHAR(20) NOT NULL,
        ModifiedBy INT NOT NULL,
        ModifiedTime DATETIME NOT NULL,
        FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
        FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
        FOREIGN KEY (OriginalMilkTypeId) REFERENCES MilkTypes(MilkTypeId),
        FOREIGN KEY (NewMilkTypeId) REFERENCES MilkTypes(MilkTypeId),
        FOREIGN KEY (ModifiedBy) REFERENCES Users(UserId)
    );
END
GO

-- 创建索引以提高查询性能
CREATE INDEX IX_Customers_Phone ON Customers(Phone);
CREATE INDEX IX_Customers_ChannelId ON Customers(ChannelId);
CREATE INDEX IX_Customers_StaffId ON Customers(StaffId);
CREATE INDEX IX_Orders_CustomerId ON Orders(CustomerId);
CREATE INDEX IX_Orders_IsActive ON Orders(IsActive);
CREATE INDEX IX_OperationLogs_UserId ON OperationLogs(UserId);
CREATE INDEX IX_OperationLogs_OperationTime ON OperationLogs(OperationTime);
GO

-- 插入初始数据
IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'admin')
BEGIN
    INSERT INTO Users (Username, Password, Role, FullName, IsActive, CreatedTime)
    VALUES ('admin', 'admin123', 'Admin', '系统管理员', 1, GETDATE());
END
GO

IF NOT EXISTS (SELECT * FROM Users WHERE Username = 'statistician')
BEGIN
    INSERT INTO Users (Username, Password, Role, FullName, IsActive, CreatedTime)
    VALUES ('statistician', 'stat123', 'Statistician', '统计员', 1, GETDATE());
END
GO

-- 插入默认渠道
IF NOT EXISTS (SELECT * FROM Channels)
BEGIN
    INSERT INTO Channels (ChannelName, Description, IsActive)
    VALUES ('线上订单', '通过线上平台下单的客户', 1),
           ('线下订单', '通过线下门店或推广下单的客户', 1),
           ('其他单位福利', '企业或单位团购的福利订单', 1);
END
GO

-- 插入默认配送人员
IF NOT EXISTS (SELECT * FROM DeliveryStaff)
BEGIN
    INSERT INTO DeliveryStaff (StaffName, Phone, IsActive)
    VALUES ('配送员1', '13800138001', 1),
           ('配送员2', '13800138002', 1);
END
GO

-- 插入默认奶品种类
IF NOT EXISTS (SELECT * FROM MilkTypes)
BEGIN
    INSERT INTO MilkTypes (TypeName, Unit, DefaultPrice, IsActive)
    VALUES ('纯牛奶', '瓶', 5.00, 1),
           ('酸奶', '瓶', 6.50, 1),
           ('高钙奶', '瓶', 6.00, 1);
END
GO

PRINT '数据库创建完成！';
GO