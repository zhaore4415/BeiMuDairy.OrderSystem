using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeiMuDairy.OrderSystem
{
    public class LogManager
    {
        /// <summary>
        /// 添加操作日志
        /// </summary>
        public static bool AddOperationLog(string operationType, string operationContent)
        {
            try
            {
                string sql = @"INSERT INTO OperationLogs (UserId, OperationType, OperationContent, IpAddress, OperationTime) 
                               VALUES (@UserId, @OperationType, @OperationContent, @IpAddress, GETDATE())";

                SqlParameter[] parameters = {
                    new SqlParameter("@UserId", Form1.CurrentUserId),
                    new SqlParameter("@OperationType", operationType),
                    new SqlParameter("@OperationContent", operationContent),
                    new SqlParameter("@IpAddress", "127.0.0.1") // 实际应获取真实IP
                };

                return DBHelper.ExecuteNonQuery(sql, parameters) > 0;
            }
            catch (Exception)
            {
                // 日志记录失败不应该影响主业务流程
                return false;
            }
        }

        /// <summary>
        /// 获取操作日志列表
        /// </summary>
        public static DataTable GetOperationLogs(DateTime? startDate = null, DateTime? endDate = null, 
                                               string operationType = "", int? userId = null)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append(@"SELECT ol.LogId, u.Username, u.FullName, ol.OperationType, ol.OperationContent, 
                                   ol.OperationTime, ol.IpAddress
                            FROM OperationLogs ol
                            LEFT JOIN Users u ON ol.UserId = u.UserId
                            WHERE 1=1");

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (startDate.HasValue)
            {
                sqlBuilder.Append(" AND ol.OperationTime >= @StartDate");
                parameters.Add(new SqlParameter("@StartDate", startDate.Value));
            }

            if (endDate.HasValue)
            {
                sqlBuilder.Append(" AND ol.OperationTime <= @EndDate");
                parameters.Add(new SqlParameter("@EndDate", endDate.Value));
            }

            if (!string.IsNullOrEmpty(operationType))
            {
                sqlBuilder.Append(" AND ol.OperationType LIKE @OperationType");
                parameters.Add(new SqlParameter("@OperationType", "%" + operationType + "%"));
            }

            if (userId.HasValue)
            {
                sqlBuilder.Append(" AND ol.UserId = @UserId");
                parameters.Add(new SqlParameter("@UserId", userId.Value));
            }

            sqlBuilder.Append(" ORDER BY ol.OperationTime DESC");

            return DBHelper.ExecuteQuery(sqlBuilder.ToString(), parameters.ToArray());
        }

        /// <summary>
        /// 获取订单修改记录
        /// </summary>
        public static DataTable GetOrderModificationLogs(int? orderId = null, int? customerId = null, 
                                                       DateTime? startDate = null, DateTime? endDate = null)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append(@"SELECT oml.LogId, oml.OrderId, c.CustomerName, oml.ModifyStartDate, oml.ModifyEndDate, 
                                   omt.TypeName AS OriginalMilkTypeName, oml.OriginalQuantity, oml.OriginalUnitPrice, oml.OriginalDiscountRate,
                                   nmt.TypeName AS NewMilkTypeName, oml.NewQuantity, oml.NewUnitPrice, oml.NewDiscountRate,
                                   oml.ModifyType, u.Username AS ModifiedBy, oml.ModifiedTime
                            FROM OrderModificationLogs oml
                            LEFT JOIN Customers c ON oml.CustomerId = c.CustomerId
                            LEFT JOIN MilkTypes omt ON oml.OriginalMilkTypeId = omt.MilkTypeId
                            LEFT JOIN MilkTypes nmt ON oml.NewMilkTypeId = nmt.MilkTypeId
                            LEFT JOIN Users u ON oml.ModifiedBy = u.UserId
                            WHERE 1=1");

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (orderId.HasValue)
            {
                sqlBuilder.Append(" AND oml.OrderId = @OrderId");
                parameters.Add(new SqlParameter("@OrderId", orderId.Value));
            }

            if (customerId.HasValue)
            {
                sqlBuilder.Append(" AND oml.CustomerId = @CustomerId");
                parameters.Add(new SqlParameter("@CustomerId", customerId.Value));
            }

            if (startDate.HasValue)
            {
                sqlBuilder.Append(" AND oml.ModifyStartDate >= @StartDate");
                parameters.Add(new SqlParameter("@StartDate", startDate.Value));
            }

            if (endDate.HasValue)
            {
                sqlBuilder.Append(" AND oml.ModifyEndDate <= @EndDate");
                parameters.Add(new SqlParameter("@EndDate", endDate.Value));
            }

            sqlBuilder.Append(" ORDER BY oml.ModifiedTime DESC");

            return DBHelper.ExecuteQuery(sqlBuilder.ToString(), parameters.ToArray());
        }

        /// <summary>
        /// 清理旧日志（保留指定天数的日志）
        /// </summary>
        public static int CleanupOldLogs(int daysToKeep = 90)
        {
            try
            {
                string sql = "DELETE FROM OperationLogs WHERE OperationTime < DATEADD(DAY, -@DaysToKeep, GETDATE())";
                SqlParameter[] parameters = { new SqlParameter("@DaysToKeep", daysToKeep) };
                return DBHelper.ExecuteNonQuery(sql, parameters);
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}