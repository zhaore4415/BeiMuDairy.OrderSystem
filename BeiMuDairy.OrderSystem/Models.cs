using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeiMuDairy.OrderSystem
{
    // 用户实体类
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string FullName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedTime { get; set; }
    }

    // 渠道实体类
    public class Channel
    {
        public int ChannelId { get; set; }
        public string ChannelName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }

    // 配送人员实体类
    public class DeliveryStaff
    {
        public int StaffId { get; set; }
        public string StaffName { get; set; }
        public string Phone { get; set; }
        public bool IsActive { get; set; }
    }

    // 奶品种类实体类
    public class MilkType
    {
        public int MilkTypeId { get; set; }
        public string TypeName { get; set; }
        public string Unit { get; set; }
        public decimal DefaultPrice { get; set; }
        public bool IsActive { get; set; }
    }

    // 客户实体类
    public class Customer
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public int ChannelId { get; set; }
        public string ChannelName { get; set; } // 关联查询使用
        public int StaffId { get; set; }
        public string StaffName { get; set; } // 关联查询使用
        public string OrderStatus { get; set; }
        public string Remark { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime LastUpdatedTime { get; set; }
    }

    // 订单实体类
    public class Order
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } // 关联查询使用
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int MilkTypeId { get; set; }
        public string MilkTypeName { get; set; } // 关联查询使用
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountRate { get; set; }
        public int? GiftMilkTypeId { get; set; }
        public string GiftMilkTypeName { get; set; } // 关联查询使用
        public int GiftQuantity { get; set; }
        public decimal? GiftUnitPrice { get; set; }
        public decimal DeductionAmount { get; set; }
        public string DeductionReason { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public bool IsOriginal { get; set; }
        public int? OriginalOrderId { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public bool IsActive { get; set; }
    }

    // 停奶记录实体类
    public class SuspensionRecord
    {
        public int SuspensionId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } // 关联查询使用
        public DateTime SuspensionStartDate { get; set; }
        public DateTime SuspensionEndDate { get; set; }
        public string Reason { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; } // 关联查询使用
        public DateTime CreatedTime { get; set; }
        public bool IsActive { get; set; }
    }

    // 退订记录实体类
    public class CancellationRecord
    {
        public int CancellationId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } // 关联查询使用
        public DateTime CancellationDate { get; set; }
        public string Reason { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; } // 关联查询使用
        public DateTime CreatedTime { get; set; }
    }

    // 操作日志实体类
    public class OperationLog
    {
        public int LogId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } // 关联查询使用
        public string OperationType { get; set; }
        public string OperationContent { get; set; }
        public DateTime OperationTime { get; set; }
        public string IpAddress { get; set; }
    }

    // 统计结果实体类
    public class StatisticsResult
    {
        public DateTime Date { get; set; }
        public int TotalQuantity { get; set; }
        public int GiftQuantity { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalDeductionAmount { get; set; }
        public decimal TotalActualAmount { get; set; }
        public string ChannelName { get; set; }
        public string StaffName { get; set; }
        public string MilkTypeName { get; set; }
    }

    // 导入验证结果实体类
    public class ImportValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> ErrorMessages { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }

        public ImportValidationResult()
        {
            ErrorMessages = new List<string>();
        }
    }
}