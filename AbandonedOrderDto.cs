using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using zopoxo.Models;

namespace zopoxo.Services.AbandonedOrder
{
    [AutoMapFrom(typeof(AbandonedOrderInformationLog))]
    public class AbandonedOrderDto : FullAuditedEntityDto<long>
    {
        public int AbandonedOrderId { get; set; }
        public int OrderStatusId { get; set; }
        public string Remarks { get; set; }
        public string Information { get; set; }
        //public DateTimeOffset CreatedAt { get; set; }
        //public DateTimeOffset UpdatedAt { get; set; }

    }
    public class AllAbandonedOrdersDto
    {
        [Column(TypeName = "nvarchar(550)")]
        public string AbandonedCheckoutUrl { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string? BillingAddress { get; set; }
        public DateTimeOffset? ClosedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string Customer { get; set; }
        [Column(TypeName = "nvarchar(10)")]
        public string Email { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string LandingSite { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string Phone { get; set; }
        public decimal? TotalPrice { get; set; }
        public long? RefNo { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public String Store { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string? Remarks { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string? Status { get; set; }
        public int StatusId { get; set; }
        public int StoreId { get; set; }
        public int AssigneeId { get; set; }
        public string? Assignee { get; set; }
        public List<string>? Product { get; set; }
        public long? Id { get; set; }
        public long CallsAttempted { get; set; }
        public string? OrderId { get; set; }
        public bool selected { get; set; } = false;
        public DateTime? AssignmentDate { get; set; }
        public string? ShippingAddress { get; set; }
        public string? AdditionalInfo { get; set; }
    }
    public class AbandonedLogsDto
    {
        public int AbandonedId { get; set; }
        public string? Status { get; set; }
        public int StatusId { get; set; }
        public int? AssigneeId { get; set; }
        public string? Remarks { get; set; }
        public string? info { get; set; }
        public string? orderId { get; set; }
        public int RemarksId { get; set; }
    }
    public class AbandonedLogOrderDto : AuditedEntityDto
    {
        public int AbandonedOrderId { get; set; }
        public string OrderStatus { get; set; }
        public string Remarks { get; set; }
        public string Information { get; set; }
        public string User { get; set; }
        public string recordingURL { get; set; }


    }
    public class AssignOrders
    {
        public int UserId { get; set; }
        public List<int> OrderId { get; set; }



    }
    public class CreateAbandonedStatusDto
    {
        public string Status { get; set; }
        public List<int> ReportStatusIds { get; set; }
    }
    public class EditAbandonedStatusDto
    {
        public int StatusId { get; set; }
        public string NewStatus { get; set; }
        public List<int> ReportStatusIds { get; set; }
    }
    public class GetAbandonedRemarksDto
    {
        public string Remark { get; set; }
        public List<int> CallingStatusId { get; set; }
        public int id { get; set; }
    }
    public class GetAllAbandonedStatusDto
    {
        public List<string> ReportHeader { get; set; }
        public List<int> ReportHeaderId { get; set; }
        public string CallStatus { get; set; }
        public long CallStatusId { get; set; }
    }
    public class CreateAbandonedReportDto
    {
        public string Header { get; set; }
    }
    public class CreateAbandonedRemarkDto
    {
        public string Remark { get; set; }
        public List<int> StatusIds { get; set; }
    }
    public class EditRemarkStatusDto
    {
        public int RemarkId { get; set; }
        public string NewRemark { get; set; }
        public List<int> StatusIds { get; set; }
    }
    public class GetAllAbandonedRemarkDto
    {
        public string Remarks { get; set; }
        public int RemarksId { get; set; }
        public List<string> CallStatus { get; set; }
        public List<int> CallStatusId { get; set; }
    }
    public class GetAbandonedStatusDto
    {
        public string Status { get; set; }
        public long id { get; set; }
    }
    public class UnuseableOrders
    {
        public int UserId { get; set; }
        public List<int> OrderIds { get; set; }
        public int statusId { get; set; }
        public string remark { get; set; }
        public string info { get; set; }

    }

    public class AbandonedFiltersDTO
    {
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        //public string query { get; set; }
        public int statusId { get; set; }
        public int remarkId { get; set; }
        public long storeId { get; set; }
        public int assignee { get; set; }
    }
    public class AbandonedUsers
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public long Id { get; set; }  
    }
    public class UtmReport
    {
        public string OrderName { get; set; }
        public string Source { get; set; }
        public string Medium { get; set; }
        public string Content { get; set; }
        public string Campaign { get; set; }
        public string Store { get; set; }
    }
    public class mappedHeadersDto
    {
        public int HeaderId { get; set; }
        public string Headers { get; set; }
        public List<int> StatusIds { get; set; }
    }
}
