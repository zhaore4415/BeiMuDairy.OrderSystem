USE OrderSystemDB;
GO

-- 查看表结构
SELECT column_name, data_type
FROM information_schema.columns
WHERE table_name = 'Users';
GO

-- 查看用户数据（包括密码）
SELECT UserId, Username, Password, Role, FullName, IsActive
FROM Users;
GO