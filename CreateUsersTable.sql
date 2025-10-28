USE OrderSystemDB;
GO

-- 创建用户表（不使用IDENTITY）
CREATE TABLE [dbo].[Users](
    [UserId] [int] NOT NULL,
    [Username] [nvarchar](50) NOT NULL,
    [Password] [nvarchar](50) NOT NULL,
    [Role] [varchar](20) NOT NULL,
    [FullName] [varchar](100) NULL,
    [IsActive] [bit] NOT NULL,
    [CreatedTime] [datetime] NOT NULL,
 PRIMARY KEY CLUSTERED (UserId)
);

-- 插入默认管理员用户
INSERT INTO [dbo].[Users](UserId, Username, Password, Role, FullName, IsActive, CreatedTime)
VALUES (1, 'admin', '123456', 'Admin', '系统管理员', 1, GETDATE());
GO