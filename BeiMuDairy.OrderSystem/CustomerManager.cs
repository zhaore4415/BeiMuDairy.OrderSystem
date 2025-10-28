using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeiMuDairy.OrderSystem
{
    public class CustomerManager
    {
        /// <summary>
        /// 添加新客户
        /// </summary>
        public static int AddCustomer(Customer customer)
        {
            string sql = @"INSERT INTO Customers (CustomerName, Phone, Address, ChannelId, StaffId, OrderStatus, Remark, CreatedTime, LastUpdatedTime) 
                           VALUES (@CustomerName, @Phone, @Address, @ChannelId, @StaffId, @OrderStatus, @Remark, GETDATE(), GETDATE()) 
                           SELECT @@IDENTITY";

            SqlParameter[] parameters = {
                new SqlParameter("@CustomerName", customer.CustomerName),
                new SqlParameter("@Phone", customer.Phone),
                new SqlParameter("@Address", customer.Address),
                new SqlParameter("@ChannelId", customer.ChannelId),
                new SqlParameter("@StaffId", customer.StaffId),
                new SqlParameter("@OrderStatus", customer.OrderStatus),
                new SqlParameter("@Remark", customer.Remark)
            };

            object result = DBHelper.ExecuteScalar(sql, parameters);
            int customerId = result != null ? Convert.ToInt32(result) : 0;
            
            // 添加日志记录
            string message = string.Format("客户ID: {0}，客户名称: {1}，操作类型: 添加客户", customerId, customer.CustomerName);
            //LogManager.AddOperationLog(message, username);
            
            return customerId;
        }

        /// <summary>
        /// 更新客户信息
        /// </summary>
        public static bool UpdateCustomer(Customer customer)
        {
            string sql = @"UPDATE Customers 
                           SET CustomerName = @CustomerName, Phone = @Phone, Address = @Address, 
                               ChannelId = @ChannelId, StaffId = @StaffId, OrderStatus = @OrderStatus, 
                               Remark = @Remark, LastUpdatedTime = GETDATE() 
                           WHERE CustomerId = @CustomerId";

            SqlParameter[] parameters = {
                new SqlParameter("@CustomerId", customer.CustomerId),
                new SqlParameter("@CustomerName", customer.CustomerName),
                new SqlParameter("@Phone", customer.Phone),
                new SqlParameter("@Address", customer.Address),
                new SqlParameter("@ChannelId", customer.ChannelId),
                new SqlParameter("@StaffId", customer.StaffId),
                new SqlParameter("@OrderStatus", customer.OrderStatus),
                new SqlParameter("@Remark", customer.Remark)
            };

            bool updateResult = DBHelper.ExecuteNonQuery(sql, parameters) > 0;
            
            // 添加日志记录
            string message = string.Format("客户ID: {0}，客户名称: {1}，操作类型: 修改客户", customer.CustomerId, customer.CustomerName);
            //LogManager.AddOperationLog(message, username);
            
            return updateResult;
        }

        /// <summary>
        /// 根据ID获取客户信息
        /// </summary>
        public static Customer GetCustomerById(int customerId)
        {
            string sql = @"SELECT c.*, ch.ChannelName, ds.StaffName 
                           FROM Customers c
                           LEFT JOIN Channels ch ON c.ChannelId = ch.ChannelId
                           LEFT JOIN DeliveryStaff ds ON c.StaffId = ds.StaffId
                           WHERE c.CustomerId = @CustomerId";

            SqlParameter[] parameters = { new SqlParameter("@CustomerId", customerId) };
            DataTable dt = DBHelper.ExecuteQuery(sql, parameters);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                return new Customer
                {
                    CustomerId = Convert.ToInt32(row["CustomerId"]),
                    CustomerName = row["CustomerName"].ToString(),
                    Phone = row["Phone"].ToString(),
                    Address = row["Address"].ToString(),
                    ChannelId = Convert.ToInt32(row["ChannelId"]),
                    ChannelName = row["ChannelName"].ToString(),
                    StaffId = Convert.ToInt32(row["StaffId"]),
                    StaffName = row["StaffName"].ToString(),
                    OrderStatus = row["OrderStatus"].ToString(),
                    Remark = row["Remark"] != DBNull.Value ? row["Remark"].ToString() : "",
                    CreatedTime = Convert.ToDateTime(row["CreatedTime"]),
                    LastUpdatedTime = Convert.ToDateTime(row["LastUpdatedTime"])
                };
            }

            return null;
        }

        /// <summary>
        /// 查询客户列表
        /// </summary>
        public static DataTable GetCustomers(string searchName = "", string searchPhone = "", string searchAddress = "", 
                                             int? channelId = null, int? staffId = null, string orderStatus = "")
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append(@"SELECT c.CustomerId, c.CustomerName, c.Phone, c.Address, 
                                    ch.ChannelName, ds.StaffName, c.OrderStatus, c.CreatedTime
                               FROM Customers c
                               LEFT JOIN Channels ch ON c.ChannelId = ch.ChannelId
                               LEFT JOIN DeliveryStaff ds ON c.StaffId = ds.StaffId
                               WHERE 1=1");

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(searchName))
            {
                sqlBuilder.Append(" AND c.CustomerName LIKE @CustomerName");
                parameters.Add(new SqlParameter("@CustomerName", "%" + searchName + "%"));
            }

            if (!string.IsNullOrEmpty(searchPhone))
            {
                if (searchPhone.StartsWith("*"))
                {
                    // 按尾号查询
                    sqlBuilder.Append(" AND c.Phone LIKE @Phone");
                    parameters.Add(new SqlParameter("@Phone", "%" + searchPhone.Substring(1)));
                }
                else
                {
                    sqlBuilder.Append(" AND c.Phone = @Phone");
                    parameters.Add(new SqlParameter("@Phone", searchPhone));
                }
            }

            if (!string.IsNullOrEmpty(searchAddress))
            {
                sqlBuilder.Append(" AND c.Address LIKE @Address");
                parameters.Add(new SqlParameter("@Address", "%" + searchAddress + "%"));
            }

            if (channelId.HasValue)
            {
                sqlBuilder.Append(" AND c.ChannelId = @ChannelId");
                parameters.Add(new SqlParameter("@ChannelId", channelId.Value));
            }

            if (staffId.HasValue)
            {
                sqlBuilder.Append(" AND c.StaffId = @StaffId");
                parameters.Add(new SqlParameter("@StaffId", staffId.Value));
            }

            if (!string.IsNullOrEmpty(orderStatus))
            {
                sqlBuilder.Append(" AND c.OrderStatus = @OrderStatus");
                parameters.Add(new SqlParameter("@OrderStatus", orderStatus));
            }

            sqlBuilder.Append(" ORDER BY c.CreatedTime DESC");

            return DBHelper.ExecuteQuery(sqlBuilder.ToString(), parameters.ToArray());
        }

        /// <summary>
        /// 更新客户订单状态
        /// </summary>
        public static bool UpdateCustomerStatus(int customerId, string status)
        {
            string sql = @"UPDATE Customers SET OrderStatus = @OrderStatus, LastUpdatedTime = GETDATE() 
                           WHERE CustomerId = @CustomerId";

            SqlParameter[] parameters = {
                new SqlParameter("@CustomerId", customerId),
                new SqlParameter("@OrderStatus", status)
            };

            return DBHelper.ExecuteNonQuery(sql, parameters) > 0;
        }

        /// <summary>
        /// 检查客户是否已存在
        /// </summary>
        public static bool IsCustomerExists(string phone, int? excludeCustomerId = null)
        {
            string sql = "SELECT COUNT(*) FROM Customers WHERE Phone = @Phone";
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@Phone", phone));

            if (excludeCustomerId.HasValue)
            {
                sql += " AND CustomerId != @CustomerId";
                parameters.Add(new SqlParameter("@CustomerId", excludeCustomerId.Value));
            }

            int count = Convert.ToInt32(DBHelper.ExecuteScalar(sql, parameters.ToArray()));
            return count > 0;
        }

        /// <summary>
        /// 批量导入客户信息
        /// </summary>
        public static ImportValidationResult ImportCustomers(List<Customer> customers)
        {
            ImportValidationResult result = new ImportValidationResult();

            foreach (Customer customer in customers)
            {
                try
                {
                    // 验证手机号格式
                    if (!System.Text.RegularExpressions.Regex.IsMatch(customer.Phone, @"^1[3-9]\d{9}$"))
                    {
                        result.ErrorMessages.Add(string.Format("客户 {0} 手机号格式不正确：{1}", customer.CustomerName, customer.Phone));
                        result.ErrorCount++;
                        continue;
                    }

                    // 验证必填字段
                    if (string.IsNullOrEmpty(customer.CustomerName) || string.IsNullOrEmpty(customer.Address))
                    {
                        result.ErrorMessages.Add(string.Format("客户 {0} 的姓名或地址不能为空", customer.CustomerName));
                        result.ErrorCount++;
                        continue;
                    }

                    // 检查是否已存在
                    if (IsCustomerExists(customer.Phone))
                    {
                        result.ErrorMessages.Add(string.Format("客户手机号 {0} 已存在", customer.Phone));
                        result.ErrorCount++;
                        continue;
                    }

                    // 添加客户
                    AddCustomer(customer);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.ErrorMessages.Add(string.Format("导入客户 {0} 失败：{1}", customer.CustomerName, ex.Message));
                    result.ErrorCount++;
                }
            }

            result.IsValid = result.ErrorCount == 0;
            return result;
        }
    }
}