using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

namespace BeiMuDairy.OrderSystem
{
    public class SystemManager
    {
        /// <summary>
        /// 初始化系统数据（创建默认用户、渠道、配送人员等）
        /// </summary>
        public static bool InitializeSystemData()
        {
            try
            {
                // 检查是否已初始化
                if (IsSystemInitialized())
                {
                    return true;
                }

                // 使用事务初始化数据
                using (SqlConnection connection = new SqlConnection(DBHelper.connectionString))
                {
                    connection.Open();
                    SqlTransaction transaction = connection.BeginTransaction();

                    try
                    {
                        // 创建默认渠道
                        CreateDefaultChannels(connection, transaction);

                        // 创建默认配送人员
                        CreateDefaultDeliveryStaff(connection, transaction);

                        // 创建默认奶品种类
                        CreateDefaultMilkTypes(connection, transaction);

                        // 创建默认管理员用户
                        CreateDefaultAdminUser(connection, transaction);

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 检查系统是否已初始化
        /// </summary>
        private static bool IsSystemInitialized()
        {
            try
            {
                // 检查是否有管理员用户
                string sql = "SELECT COUNT(*) FROM Users WHERE Role = 'Admin'";
                int count = Convert.ToInt32(DBHelper.ExecuteScalar(sql));
                return count > 0;
            }
            catch (Exception)
            {
                // 数据库可能还未创建或连接失败
                return false;
            }
        }

        /// <summary>
        /// 创建默认渠道
        /// </summary>
        private static void CreateDefaultChannels(SqlConnection connection, SqlTransaction transaction)
        {
            string sql = "INSERT INTO Channels (ChannelName, Description, IsActive) VALUES (@ChannelName, @Description, 1)";

            SqlCommand command = new SqlCommand(sql, connection, transaction);

            // 线上订单
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@ChannelName", "线上订单");
            command.Parameters.AddWithValue("@Description", "通过线上平台下单的客户");
            command.ExecuteNonQuery();

            // 线下订单
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@ChannelName", "线下订单");
            command.Parameters.AddWithValue("@Description", "通过线下门店或推广下单的客户");
            command.ExecuteNonQuery();

            // 其他单位福利
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@ChannelName", "其他单位福利");
            command.Parameters.AddWithValue("@Description", "企业或单位团购的福利订单");
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// 创建默认配送人员
        /// </summary>
        private static void CreateDefaultDeliveryStaff(SqlConnection connection, SqlTransaction transaction)
        {
            string sql = "INSERT INTO DeliveryStaff (StaffName, Phone, IsActive) VALUES (@StaffName, @Phone, 1)";

            SqlCommand command = new SqlCommand(sql, connection, transaction);

            // 创建默认配送人员
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@StaffName", "配送员1");
            command.Parameters.AddWithValue("@Phone", "13800138001");
            command.ExecuteNonQuery();

            command.Parameters.Clear();
            command.Parameters.AddWithValue("@StaffName", "配送员2");
            command.Parameters.AddWithValue("@Phone", "13800138002");
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// 创建默认奶品种类
        /// </summary>
        private static void CreateDefaultMilkTypes(SqlConnection connection, SqlTransaction transaction)
        {
            string sql = "INSERT INTO MilkTypes (TypeName, Unit, DefaultPrice, IsActive) VALUES (@TypeName, @Unit, @DefaultPrice, 1)";

            SqlCommand command = new SqlCommand(sql, connection, transaction);

            // 纯牛奶
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@TypeName", "纯牛奶");
            command.Parameters.AddWithValue("@Unit", "瓶");
            command.Parameters.AddWithValue("@DefaultPrice", 5.00);
            command.ExecuteNonQuery();

            // 酸奶
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@TypeName", "酸奶");
            command.Parameters.AddWithValue("@Unit", "瓶");
            command.Parameters.AddWithValue("@DefaultPrice", 6.50);
            command.ExecuteNonQuery();

            // 高钙奶
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@TypeName", "高钙奶");
            command.Parameters.AddWithValue("@Unit", "瓶");
            command.Parameters.AddWithValue("@DefaultPrice", 6.00);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// 创建默认管理员用户
        /// </summary>
        private static void CreateDefaultAdminUser(SqlConnection connection, SqlTransaction transaction)
        {
            string sql = "INSERT INTO Users (Username, Password, Role, FullName, IsActive, CreatedTime) VALUES (@Username, @Password, @Role, @FullName, 1, GETDATE())";

            SqlCommand command = new SqlCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("@Username", "admin");
            command.Parameters.AddWithValue("@Password", "admin123"); // 实际项目中应该加密存储
            command.Parameters.AddWithValue("@Role", "Admin");
            command.Parameters.AddWithValue("@FullName", "系统管理员");
            command.ExecuteNonQuery();

            // 创建默认统计员
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@Username", "statistician");
            command.Parameters.AddWithValue("@Password", "stat123");
            command.Parameters.AddWithValue("@Role", "Statistician");
            command.Parameters.AddWithValue("@FullName", "统计员");
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// 备份数据库
        /// </summary>
        public static bool BackupDatabase(string backupPath)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["BeiMuDairy.OrderSystem.Properties.Settings.OrderSystemConnectionString"].ConnectionString;
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
                string databaseName = builder.InitialCatalog;

                string sql = string.Format("BACKUP DATABASE [{0}] TO DISK = @BackupPath WITH FORMAT, INIT", databaseName);
                SqlParameter[] parameters = { new SqlParameter("@BackupPath", backupPath) };

                return DBHelper.ExecuteNonQuery(sql, parameters) > 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 生成备份文件路径
        /// </summary>
        public static string GenerateBackupFilePath()
        {
            string backupFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backup");
            if (!Directory.Exists(backupFolder))
            {
                Directory.CreateDirectory(backupFolder);
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = string.Format("OrderSystem_Backup_{0}.bak", timestamp);
            return Path.Combine(backupFolder, fileName);
        }

        /// <summary>
        /// 获取系统信息
        /// </summary>
        public static Dictionary<string, string> GetSystemInfo()
        {
            Dictionary<string, string> systemInfo = new Dictionary<string, string>();

            systemInfo.Add("系统名称", "北牧乳业订单管理与统计系统");
            systemInfo.Add("版本", "1.0.0");
            systemInfo.Add("数据库连接", DBHelper.CheckConnection() ? "正常" : "异常");
            systemInfo.Add("当前用户", Form1.CurrentFullName);
            systemInfo.Add("用户角色", Form1.CurrentRole);
            systemInfo.Add("登录时间", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            // 获取系统统计数据
            try
            {
                string sql = "SELECT COUNT(*) FROM Customers";
                systemInfo.Add("客户总数", DBHelper.ExecuteScalar(sql).ToString());

                sql = "SELECT COUNT(*) FROM Orders WHERE IsActive = 1";
                systemInfo.Add("有效订单数", DBHelper.ExecuteScalar(sql).ToString());

                sql = "SELECT COUNT(*) FROM DeliveryStaff WHERE IsActive = 1";
                systemInfo.Add("配送人员数", DBHelper.ExecuteScalar(sql).ToString());
            }
            catch (Exception)
            {
                systemInfo.Add("统计数据", "获取失败");
            }

            return systemInfo;
        }

        /// <summary>
        /// 验证数据库连接
        /// </summary>
        public static bool TestDatabaseConnection(string connectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 更新数据库连接字符串
        /// </summary>
        public static bool UpdateConnectionString(string connectionString)
        {
            try
            {
                // 先测试连接是否有效
                if (!TestDatabaseConnection(connectionString))
                {
                    return false;
                }

                // 使用ConfigurationManager而不是Properties.Settings来更新连接字符串
                try
                {
                    // 获取配置文件
                    System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                    // 更新连接字符串
                    config.ConnectionStrings.ConnectionStrings["BeiMuDairy.OrderSystem.Properties.Settings.OrderSystemConnectionString"].ConnectionString = connectionString;

                    // 保存配置
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("connectionStrings");

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}