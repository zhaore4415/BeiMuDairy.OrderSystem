using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeiMuDairy.OrderSystem
{
    public class OrderManager
    {
        /// <summary>
        /// 添加新订单
        /// </summary>
        public static int AddOrder(Order order)
        {
            // 计算订单金额
            CalculateOrderAmounts(order);

            string sql = @"INSERT INTO Orders (CustomerId, StartDate, EndDate, MilkTypeId, Quantity, UnitPrice, 
                           DiscountRate, GiftMilkTypeId, GiftQuantity, GiftUnitPrice, DeductionAmount, DeductionReason, 
                           TotalAmount, ActualAmount, IsOriginal, CreatedBy, CreatedTime, IsActive) 
                           VALUES (@CustomerId, @StartDate, @EndDate, @MilkTypeId, @Quantity, @UnitPrice, 
                           @DiscountRate, @GiftMilkTypeId, @GiftQuantity, @GiftUnitPrice, @DeductionAmount, @DeductionReason, 
                           @TotalAmount, @ActualAmount, 1, @CreatedBy, GETDATE(), 1) 
                           SELECT @@IDENTITY";

            SqlParameter[] parameters = {
                new SqlParameter("@CustomerId", order.CustomerId),
                new SqlParameter("@StartDate", order.StartDate),
                new SqlParameter("@EndDate", order.EndDate.HasValue ? (object)order.EndDate.Value : DBNull.Value),
                new SqlParameter("@MilkTypeId", order.MilkTypeId),
                new SqlParameter("@Quantity", order.Quantity),
                new SqlParameter("@UnitPrice", order.UnitPrice),
                new SqlParameter("@DiscountRate", order.DiscountRate),
                new SqlParameter("@GiftMilkTypeId", order.GiftMilkTypeId.HasValue ? (object)order.GiftMilkTypeId.Value : DBNull.Value),
                new SqlParameter("@GiftQuantity", order.GiftQuantity),
                new SqlParameter("@GiftUnitPrice", order.GiftUnitPrice.HasValue ? (object)order.GiftUnitPrice.Value : DBNull.Value),
                new SqlParameter("@DeductionAmount", order.DeductionAmount),
                new SqlParameter("@DeductionReason", string.IsNullOrEmpty(order.DeductionReason) ? DBNull.Value : (object)order.DeductionReason),
                new SqlParameter("@TotalAmount", order.TotalAmount),
                new SqlParameter("@ActualAmount", order.ActualAmount),
                new SqlParameter("@CreatedBy", Form1.CurrentUserId)
            };

            object result = DBHelper.ExecuteScalar(sql, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        /// <summary>
        /// 修改订单
        /// </summary>
        public static bool ModifyOrder(Order newOrder, DateTime modifyStartDate, DateTime modifyEndDate)
        {
            try
            {
                // 检查是否为已完成配送的时间段
                bool isPastDelivery = modifyStartDate < DateTime.Now.Date;
                string modifyType = isPastDelivery ? "补录修改" : "正常修改";

                // 计算新订单金额
                CalculateOrderAmounts(newOrder);

                // 使用事务处理订单修改
                using (SqlConnection connection = new SqlConnection(DBHelper.connectionString))
                {
                    connection.Open();
                    SqlTransaction transaction = connection.BeginTransaction();

                    try
                    {
                        // 获取原订单信息
                        Order originalOrder = GetOrderById(newOrder.OrderId);
                        if (originalOrder == null)
                        {
                            throw new Exception("订单不存在");
                        }

                        // 标记原订单为无效
                        string updateSql = "UPDATE Orders SET IsActive = 0 WHERE OrderId = @OrderId";
                        SqlCommand updateCommand = new SqlCommand(updateSql, connection, transaction);
                        updateCommand.Parameters.AddWithValue("@OrderId", newOrder.OrderId);
                        updateCommand.ExecuteNonQuery();

                        // 插入新的修改后订单
                        string insertSql = @"INSERT INTO Orders (CustomerId, StartDate, EndDate, MilkTypeId, Quantity, UnitPrice, 
                                           DiscountRate, GiftMilkTypeId, GiftQuantity, GiftUnitPrice, DeductionAmount, DeductionReason, 
                                           TotalAmount, ActualAmount, IsOriginal, OriginalOrderId, ModifiedBy, ModifiedTime, IsActive) 
                                           VALUES (@CustomerId, @StartDate, @EndDate, @MilkTypeId, @Quantity, @UnitPrice, 
                                           @DiscountRate, @GiftMilkTypeId, @GiftQuantity, @GiftUnitPrice, @DeductionAmount, @DeductionReason, 
                                           @TotalAmount, @ActualAmount, 0, @OriginalOrderId, @ModifiedBy, GETDATE(), 1)";

                        SqlCommand insertCommand = new SqlCommand(insertSql, connection, transaction);
                        insertCommand.Parameters.AddWithValue("@CustomerId", newOrder.CustomerId);
                        insertCommand.Parameters.AddWithValue("@StartDate", newOrder.StartDate);
                        insertCommand.Parameters.AddWithValue("@EndDate", newOrder.EndDate.HasValue ? (object)newOrder.EndDate.Value : DBNull.Value);
                        insertCommand.Parameters.AddWithValue("@MilkTypeId", newOrder.MilkTypeId);
                        insertCommand.Parameters.AddWithValue("@Quantity", newOrder.Quantity);
                        insertCommand.Parameters.AddWithValue("@UnitPrice", newOrder.UnitPrice);
                        insertCommand.Parameters.AddWithValue("@DiscountRate", newOrder.DiscountRate);
                        insertCommand.Parameters.AddWithValue("@GiftMilkTypeId", newOrder.GiftMilkTypeId.HasValue ? (object)newOrder.GiftMilkTypeId.Value : DBNull.Value);
                        insertCommand.Parameters.AddWithValue("@GiftQuantity", newOrder.GiftQuantity);
                        insertCommand.Parameters.AddWithValue("@GiftUnitPrice", newOrder.GiftUnitPrice.HasValue ? (object)newOrder.GiftUnitPrice.Value : DBNull.Value);
                        insertCommand.Parameters.AddWithValue("@DeductionAmount", newOrder.DeductionAmount);
                        insertCommand.Parameters.AddWithValue("@DeductionReason", string.IsNullOrEmpty(newOrder.DeductionReason) ? DBNull.Value : (object)newOrder.DeductionReason);
                        insertCommand.Parameters.AddWithValue("@TotalAmount", newOrder.TotalAmount);
                        insertCommand.Parameters.AddWithValue("@ActualAmount", newOrder.ActualAmount);
                        insertCommand.Parameters.AddWithValue("@OriginalOrderId", newOrder.OrderId);
                        insertCommand.Parameters.AddWithValue("@ModifiedBy", Form1.CurrentUserId);
                        insertCommand.ExecuteNonQuery();

                        // 记录订单修改日志
                        string logSql = @"INSERT INTO OrderModificationLogs (OrderId, OriginalOrderId, CustomerId, 
                                          ModifyStartDate, ModifyEndDate, OriginalMilkTypeId, OriginalQuantity, OriginalUnitPrice, 
                                          OriginalDiscountRate, NewMilkTypeId, NewQuantity, NewUnitPrice, NewDiscountRate, 
                                          ModifyType, ModifiedBy, ModifiedTime) 
                                          VALUES (@OrderId, @OriginalOrderId, @CustomerId, @ModifyStartDate, @ModifyEndDate, 
                                          @OriginalMilkTypeId, @OriginalQuantity, @OriginalUnitPrice, @OriginalDiscountRate, 
                                          @NewMilkTypeId, @NewQuantity, @NewUnitPrice, @NewDiscountRate, @ModifyType, @ModifiedBy, GETDATE())";

                        SqlCommand logCommand = new SqlCommand(logSql, connection, transaction);
                        logCommand.Parameters.AddWithValue("@OrderId", newOrder.OrderId);
                        logCommand.Parameters.AddWithValue("@OriginalOrderId", newOrder.OrderId);
                        logCommand.Parameters.AddWithValue("@CustomerId", newOrder.CustomerId);
                        logCommand.Parameters.AddWithValue("@ModifyStartDate", modifyStartDate);
                        logCommand.Parameters.AddWithValue("@ModifyEndDate", modifyEndDate);
                        logCommand.Parameters.AddWithValue("@OriginalMilkTypeId", originalOrder.MilkTypeId);
                        logCommand.Parameters.AddWithValue("@OriginalQuantity", originalOrder.Quantity);
                        logCommand.Parameters.AddWithValue("@OriginalUnitPrice", originalOrder.UnitPrice);
                        logCommand.Parameters.AddWithValue("@OriginalDiscountRate", originalOrder.DiscountRate);
                        logCommand.Parameters.AddWithValue("@NewMilkTypeId", newOrder.MilkTypeId);
                        logCommand.Parameters.AddWithValue("@NewQuantity", newOrder.Quantity);
                        logCommand.Parameters.AddWithValue("@NewUnitPrice", newOrder.UnitPrice);
                        logCommand.Parameters.AddWithValue("@NewDiscountRate", newOrder.DiscountRate);
                        logCommand.Parameters.AddWithValue("@ModifyType", modifyType);
                        logCommand.Parameters.AddWithValue("@ModifiedBy", Form1.CurrentUserId);
                        logCommand.ExecuteNonQuery();

                        transaction.Commit();

                        // 记录系统操作日志
                        string operationContent = string.Format("{0}客户订单：客户ID={1}，订单ID={2}，修改时间段={3}至{4}", modifyType, newOrder.CustomerId, newOrder.OrderId, modifyStartDate.ToString("yyyy-MM-dd"), modifyEndDate.ToString("yyyy-MM-dd"));
                        LogManager.AddOperationLog("订单修改", operationContent);

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
        /// 计算订单金额
        /// </summary>
        public static void CalculateOrderAmounts(Order order)
        {
            // 计算总金额（牛奶单价 × 数量 × 折扣率）
            order.TotalAmount = order.UnitPrice * order.Quantity * order.DiscountRate / 100;
            
            // 计算实付金额（总金额 - 减免金额）
            order.ActualAmount = order.TotalAmount - order.DeductionAmount;
            
            // 确保实付金额不为负数
            if (order.ActualAmount < 0)
            {
                order.ActualAmount = 0;
            }
        }

        /// <summary>
        /// 根据ID获取订单信息
        /// </summary>
        public static Order GetOrderById(int orderId)
        {
            string sql = @"SELECT o.*, c.CustomerName, m.TypeName AS MilkTypeName, gm.TypeName AS GiftMilkTypeName 
                           FROM Orders o
                           LEFT JOIN Customers c ON o.CustomerId = c.CustomerId
                           LEFT JOIN MilkTypes m ON o.MilkTypeId = m.MilkTypeId
                           LEFT JOIN MilkTypes gm ON o.GiftMilkTypeId = gm.MilkTypeId
                           WHERE o.OrderId = @OrderId";

            SqlParameter[] parameters = { new SqlParameter("@OrderId", orderId) };
            DataTable dt = DBHelper.ExecuteQuery(sql, parameters);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                return new Order
                {
                    OrderId = Convert.ToInt32(row["OrderId"]),
                    CustomerId = Convert.ToInt32(row["CustomerId"]),
                    CustomerName = row["CustomerName"] != DBNull.Value ? row["CustomerName"].ToString() : "",
                    StartDate = Convert.ToDateTime(row["StartDate"]),
                    EndDate = row["EndDate"] != DBNull.Value ? Convert.ToDateTime(row["EndDate"]) : (DateTime?)null,
                    MilkTypeId = Convert.ToInt32(row["MilkTypeId"]),
                    MilkTypeName = row["MilkTypeName"] != DBNull.Value ? row["MilkTypeName"].ToString() : "",
                    Quantity = Convert.ToInt32(row["Quantity"]),
                    UnitPrice = Convert.ToDecimal(row["UnitPrice"]),
                    DiscountRate = Convert.ToDecimal(row["DiscountRate"]),
                    GiftMilkTypeId = row["GiftMilkTypeId"] != DBNull.Value ? Convert.ToInt32(row["GiftMilkTypeId"]) : (int?)null,
                    GiftMilkTypeName = row["GiftMilkTypeName"] != DBNull.Value ? row["GiftMilkTypeName"].ToString() : "",
                    GiftQuantity = Convert.ToInt32(row["GiftQuantity"]),
                    GiftUnitPrice = row["GiftUnitPrice"] != DBNull.Value ? Convert.ToDecimal(row["GiftUnitPrice"]) : (decimal?)null,
                    DeductionAmount = Convert.ToDecimal(row["DeductionAmount"]),
                    DeductionReason = row["DeductionReason"] != DBNull.Value ? row["DeductionReason"].ToString() : "",
                    TotalAmount = Convert.ToDecimal(row["TotalAmount"]),
                    ActualAmount = Convert.ToDecimal(row["ActualAmount"]),
                    IsOriginal = Convert.ToBoolean(row["IsOriginal"]),
                    OriginalOrderId = row["OriginalOrderId"] != DBNull.Value ? Convert.ToInt32(row["OriginalOrderId"]) : (int?)null,
                    ModifiedBy = row["ModifiedBy"] != DBNull.Value ? Convert.ToInt32(row["ModifiedBy"]) : (int?)null,
                    ModifiedTime = row["ModifiedTime"] != DBNull.Value ? Convert.ToDateTime(row["ModifiedTime"]) : (DateTime?)null,
                    IsActive = Convert.ToBoolean(row["IsActive"])
                };
            }

            return null;
        }

        /// <summary>
        /// 获取客户的当前有效订单
        /// </summary>
        public static Order GetCustomerCurrentOrder(int customerId)
        {
            string sql = @"SELECT o.*, c.CustomerName, m.TypeName AS MilkTypeName, gm.TypeName AS GiftMilkTypeName 
                           FROM Orders o
                           LEFT JOIN Customers c ON o.CustomerId = c.CustomerId
                           LEFT JOIN MilkTypes m ON o.MilkTypeId = m.MilkTypeId
                           LEFT JOIN MilkTypes gm ON o.GiftMilkTypeId = gm.MilkTypeId
                           WHERE o.CustomerId = @CustomerId AND o.IsActive = 1 
                           ORDER BY o.StartDate DESC";

            SqlParameter[] parameters = { new SqlParameter("@CustomerId", customerId) };
            DataTable dt = DBHelper.ExecuteQuery(sql, parameters);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                return new Order
                {
                    OrderId = Convert.ToInt32(row["OrderId"]),
                    CustomerId = Convert.ToInt32(row["CustomerId"]),
                    CustomerName = row["CustomerName"] != DBNull.Value ? row["CustomerName"].ToString() : "",
                    StartDate = Convert.ToDateTime(row["StartDate"]),
                    EndDate = row["EndDate"] != DBNull.Value ? Convert.ToDateTime(row["EndDate"]) : (DateTime?)null,
                    MilkTypeId = Convert.ToInt32(row["MilkTypeId"]),
                    MilkTypeName = row["MilkTypeName"] != DBNull.Value ? row["MilkTypeName"].ToString() : "",
                    Quantity = Convert.ToInt32(row["Quantity"]),
                    UnitPrice = Convert.ToDecimal(row["UnitPrice"]),
                    DiscountRate = Convert.ToDecimal(row["DiscountRate"]),
                    GiftMilkTypeId = row["GiftMilkTypeId"] != DBNull.Value ? Convert.ToInt32(row["GiftMilkTypeId"]) : (int?)null,
                    GiftMilkTypeName = row["GiftMilkTypeName"] != DBNull.Value ? row["GiftMilkTypeName"].ToString() : "",
                    GiftQuantity = Convert.ToInt32(row["GiftQuantity"]),
                    GiftUnitPrice = row["GiftUnitPrice"] != DBNull.Value ? Convert.ToDecimal(row["GiftUnitPrice"]) : (decimal?)null,
                    DeductionAmount = Convert.ToDecimal(row["DeductionAmount"]),
                    DeductionReason = row["DeductionReason"] != DBNull.Value ? row["DeductionReason"].ToString() : "",
                    TotalAmount = Convert.ToDecimal(row["TotalAmount"]),
                    ActualAmount = Convert.ToDecimal(row["ActualAmount"]),
                    IsOriginal = Convert.ToBoolean(row["IsOriginal"]),
                    OriginalOrderId = row["OriginalOrderId"] != DBNull.Value ? Convert.ToInt32(row["OriginalOrderId"]) : (int?)null,
                    ModifiedBy = row["ModifiedBy"] != DBNull.Value ? Convert.ToInt32(row["ModifiedBy"]) : (int?)null,
                    ModifiedTime = row["ModifiedTime"] != DBNull.Value ? Convert.ToDateTime(row["ModifiedTime"]) : (DateTime?)null,
                    IsActive = Convert.ToBoolean(row["IsActive"])
                };
            }

            return null;
        }

        /// <summary>
        /// 获取订单列表
        /// </summary>
        public static DataTable GetOrders(int? customerId = null, DateTime? startDate = null, DateTime? endDate = null, 
                                         int? milkTypeId = null, int? staffId = null, int? channelId = null)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append(@"SELECT o.OrderId, c.CustomerName, c.Phone, c.Address, o.StartDate, o.EndDate, 
                                   m.TypeName AS MilkTypeName, o.Quantity, o.UnitPrice, o.DiscountRate, 
                                   gm.TypeName AS GiftMilkTypeName, o.GiftQuantity, o.DeductionAmount, o.TotalAmount, o.ActualAmount, 
                                   o.IsOriginal, o.ModifiedTime, c.OrderStatus, ch.ChannelName, ds.StaffName
                            FROM Orders o
                            LEFT JOIN Customers c ON o.CustomerId = c.CustomerId
                            LEFT JOIN MilkTypes m ON o.MilkTypeId = m.MilkTypeId
                            LEFT JOIN MilkTypes gm ON o.GiftMilkTypeId = gm.MilkTypeId
                            LEFT JOIN Channels ch ON c.ChannelId = ch.ChannelId
                            LEFT JOIN DeliveryStaff ds ON c.StaffId = ds.StaffId
                            WHERE o.IsActive = 1");

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (customerId.HasValue)
            {
                sqlBuilder.Append(" AND o.CustomerId = @CustomerId");
                parameters.Add(new SqlParameter("@CustomerId", customerId.Value));
            }

            if (startDate.HasValue)
            {
                sqlBuilder.Append(" AND o.StartDate >= @StartDate");
                parameters.Add(new SqlParameter("@StartDate", startDate.Value));
            }

            if (endDate.HasValue)
            {
                sqlBuilder.Append(" AND o.EndDate <= @EndDate");
                parameters.Add(new SqlParameter("@EndDate", endDate.Value));
            }

            if (milkTypeId.HasValue)
            {
                sqlBuilder.Append(" AND o.MilkTypeId = @MilkTypeId");
                parameters.Add(new SqlParameter("@MilkTypeId", milkTypeId.Value));
            }

            if (staffId.HasValue)
            {
                sqlBuilder.Append(" AND c.StaffId = @StaffId");
                parameters.Add(new SqlParameter("@StaffId", staffId.Value));
            }

            if (channelId.HasValue)
            {
                sqlBuilder.Append(" AND c.ChannelId = @ChannelId");
                parameters.Add(new SqlParameter("@ChannelId", channelId.Value));
            }

            sqlBuilder.Append(" ORDER BY o.StartDate DESC");

            return DBHelper.ExecuteQuery(sqlBuilder.ToString(), parameters.ToArray());
        }

        /// <summary>
        /// 添加停奶记录
        /// </summary>
        public static int AddSuspension(SuspensionRecord suspension)
        {
            string sql = @"INSERT INTO SuspensionRecords (CustomerId, SuspensionStartDate, SuspensionEndDate, 
                           Reason, CreatedBy, CreatedTime, IsActive) 
                           VALUES (@CustomerId, @SuspensionStartDate, @SuspensionEndDate, 
                           @Reason, @CreatedBy, GETDATE(), 1) 
                           SELECT @@IDENTITY";

            SqlParameter[] parameters = {
                new SqlParameter("@CustomerId", suspension.CustomerId),
                new SqlParameter("@SuspensionStartDate", suspension.SuspensionStartDate),
                new SqlParameter("@SuspensionEndDate", suspension.SuspensionEndDate),
                new SqlParameter("@Reason", suspension.Reason),
                new SqlParameter("@CreatedBy", Form1.CurrentUserId)
            };

            object result = DBHelper.ExecuteScalar(sql, parameters);
            int suspensionId = result != null ? Convert.ToInt32(result) : 0;

            // 记录系统操作日志
            string operationContent = string.Format("添加停奶记录：客户ID={0}，停奶时间段={1}至{2}", suspension.CustomerId, suspension.SuspensionStartDate.ToString("yyyy-MM-dd"), suspension.SuspensionEndDate.ToString("yyyy-MM-dd"));
            LogManager.AddOperationLog("停奶管理", operationContent);

            return suspensionId;
        }

        /// <summary>
        /// 取消停奶
        /// </summary>
        public static bool CancelSuspension(int suspensionId)
        {
            string sql = "UPDATE SuspensionRecords SET IsActive = 0 WHERE SuspensionId = @SuspensionId";
            SqlParameter[] parameters = { new SqlParameter("@SuspensionId", suspensionId) };

            bool result = DBHelper.ExecuteNonQuery(sql, parameters) > 0;

            if (result)
            {
                // 记录系统操作日志
                string operationContent = string.Format("取消停奶记录：停奶ID={0}", suspensionId);
                LogManager.AddOperationLog("停奶管理", operationContent);
            }

            return result;
        }

        /// <summary>
        /// 添加退订记录
        /// </summary>
        public static int AddCancellation(CancellationRecord cancellation)
        {
            try
            {
                // 使用事务处理退订操作
                using (SqlConnection connection = new SqlConnection(DBHelper.connectionString))
                {
                    connection.Open();
                    SqlTransaction transaction = connection.BeginTransaction();

                    try
                    {
                        // 添加退订记录
                        string insertSql = @"INSERT INTO CancellationRecords (CustomerId, CancellationDate, Reason, 
                                           CreatedBy, CreatedTime) 
                                           VALUES (@CustomerId, @CancellationDate, @Reason, @CreatedBy, GETDATE()) 
                                           SELECT @@IDENTITY";

                        SqlCommand insertCommand = new SqlCommand(insertSql, connection, transaction);
                        insertCommand.Parameters.AddWithValue("@CustomerId", cancellation.CustomerId);
                        insertCommand.Parameters.AddWithValue("@CancellationDate", cancellation.CancellationDate);
                        insertCommand.Parameters.AddWithValue("@Reason", cancellation.Reason);
                        insertCommand.Parameters.AddWithValue("@CreatedBy", Form1.CurrentUserId);

                        object result = insertCommand.ExecuteScalar();
                        int cancellationId = result != null ? Convert.ToInt32(result) : 0;

                        // 更新客户状态为已退订
                        string updateSql = "UPDATE Customers SET OrderStatus = '已退订', LastUpdatedTime = GETDATE() WHERE CustomerId = @CustomerId";
                        SqlCommand updateCommand = new SqlCommand(updateSql, connection, transaction);
                        updateCommand.Parameters.AddWithValue("@CustomerId", cancellation.CustomerId);
                        updateCommand.ExecuteNonQuery();

                        // 标记该客户的所有有效订单为无效
                        string updateOrderSql = "UPDATE Orders SET IsActive = 0 WHERE CustomerId = @CustomerId AND IsActive = 1";
                        SqlCommand updateOrderCommand = new SqlCommand(updateOrderSql, connection, transaction);
                        updateOrderCommand.Parameters.AddWithValue("@CustomerId", cancellation.CustomerId);
                        updateOrderCommand.ExecuteNonQuery();

                        transaction.Commit();

                        // 记录系统操作日志
                        string operationContent = string.Format("客户退订：客户ID={0}，退订日期={1}", cancellation.CustomerId, cancellation.CancellationDate.ToString("yyyy-MM-dd"));
                        LogManager.AddOperationLog("退订管理", operationContent);

                        return cancellationId;
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
        /// 获取停奶记录列表
        /// </summary>
        public static DataTable GetSuspensionRecords(int? customerId = null, DateTime? startDate = null, DateTime? endDate = null, bool? isActive = null)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append(@"SELECT sr.SuspensionId, c.CustomerName, c.Phone, sr.SuspensionStartDate, sr.SuspensionEndDate, 
                                   sr.Reason, u.Username AS CreatedByName, sr.CreatedTime, sr.IsActive
                            FROM SuspensionRecords sr
                            LEFT JOIN Customers c ON sr.CustomerId = c.CustomerId
                            LEFT JOIN Users u ON sr.CreatedBy = u.UserId
                            WHERE 1=1");

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (customerId.HasValue)
            {
                sqlBuilder.Append(" AND sr.CustomerId = @CustomerId");
                parameters.Add(new SqlParameter("@CustomerId", customerId.Value));
            }

            if (startDate.HasValue)
            {
                sqlBuilder.Append(" AND sr.SuspensionStartDate >= @StartDate");
                parameters.Add(new SqlParameter("@StartDate", startDate.Value));
            }

            if (endDate.HasValue)
            {
                sqlBuilder.Append(" AND sr.SuspensionEndDate <= @EndDate");
                parameters.Add(new SqlParameter("@EndDate", endDate.Value));
            }

            if (isActive.HasValue)
            {
                sqlBuilder.Append(" AND sr.IsActive = @IsActive");
                parameters.Add(new SqlParameter("@IsActive", isActive.Value));
            }

            sqlBuilder.Append(" ORDER BY sr.CreatedTime DESC");

            return DBHelper.ExecuteQuery(sqlBuilder.ToString(), parameters.ToArray());
        }

        /// <summary>
        /// 获取退订记录列表
        /// </summary>
        public static DataTable GetCancellationRecords(int? customerId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append(@"SELECT cr.CancellationId, c.CustomerName, c.Phone, cr.CancellationDate, cr.Reason, 
                                   u.Username AS CreatedByName, cr.CreatedTime
                            FROM CancellationRecords cr
                            LEFT JOIN Customers c ON cr.CustomerId = c.CustomerId
                            LEFT JOIN Users u ON cr.CreatedBy = u.UserId
                            WHERE 1=1");

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (customerId.HasValue)
            {
                sqlBuilder.Append(" AND cr.CustomerId = @CustomerId");
                parameters.Add(new SqlParameter("@CustomerId", customerId.Value));
            }

            if (startDate.HasValue)
            {
                sqlBuilder.Append(" AND cr.CancellationDate >= @StartDate");
                parameters.Add(new SqlParameter("@StartDate", startDate.Value));
            }

            if (endDate.HasValue)
            {
                sqlBuilder.Append(" AND cr.CancellationDate <= @EndDate");
                parameters.Add(new SqlParameter("@EndDate", endDate.Value));
            }

            sqlBuilder.Append(" ORDER BY cr.CreatedTime DESC");

            return DBHelper.ExecuteQuery(sqlBuilder.ToString(), parameters.ToArray());
        }
    }
}