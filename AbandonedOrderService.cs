
using Abp.Application.Features;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using ShopifySharp;
using ShopifySharp.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using zopoxo.Authorization.Roles;
using zopoxo.EntityFrameworkCore.Repositories;
using zopoxo.Models;
using zopoxo.Models.Abandoned;
using zopoxo.Models.CustomerCare;
using zopoxo.Models.Shopify.Entities;
using zopoxo.Services.FullFilment.Dtos;
using zopoxo.MultiTenancy;
using System.ComponentModel;

namespace zopoxo.Services.AbandonedOrder
{
    [RequiresFeature(FeatureConst.Abandoned)]
    [AbpAuthorize]
    public class AbandonedOrderService : AsyncCrudAppService<AbandonedOrderInformationLog, AbandonedOrderDto, long>
    {
        private readonly IRepository<AbandonedOrderInformationLog, long> abandonedOrderRepository;
        private readonly IRepository<AbandonedCheckouts, long> abandonedCheckoutsRepository;
        private readonly IRepository<OrderStatus, int> orderStatusRepository;
        private readonly IRepository<Proplix.Services.Shopify.Checkout, long> checkoutRepo;
        private readonly IRepository<Store> storeRepo;
        private readonly IRepository<AbandonedSyncStatus, long> abandonedSyncStatusRepo;
        private readonly IRepository<Authorization.Users.User, long> userRepo;
        private readonly IRepository<Role> rolesRepo;
        private readonly IRepository<AbandonedOrderStatus, long> abandonedOrderStatus;
        private readonly IRepository<InboundCalls> inboundCalls;
        private readonly IRepository<CallDetails, long> callDetailsRepo;
        private readonly IRepository<Proplix.Services.Shopify.Address, long> addressRepo;
        private readonly IRepository<ZopoxoOrder, long> zopoxoOrderRepository;
        private readonly IRepository<UtmParameters> utmParametersRepo;
        private readonly IRepository<AbandonedReportHeaders> abandonedReportHeadersRepo;
        private readonly IRepository<AbandonedRemarks> abandonedRemarksRepo;
        private readonly IRepository<AbandonedRemarkStatusMapping> abandonedRemarkStatusMappingRepo;
        private readonly IRepository<AbandonedReportStatusMapping> abandonedReportStatusMappingRepo;
        private readonly ICommonRepository commonRepository;

        public AbandonedOrderService(IRepository<AbandonedOrderInformationLog, long> abandonedOrderRepository,
             IRepository<AbandonedCheckouts, long> _AbandonedCheckoutsRepository,
              IRepository<OrderStatus, int> _orderStatusRepository,
              IRepository<Proplix.Services.Shopify.Checkout, long> checkoutRepo,
              IRepository<Store> _StoreRepo,
              IRepository<AbandonedSyncStatus, long> _AbandonedSyncStatusRepo,
              IRepository<Authorization.Users.User, long> _userRepo,
              IRepository<Authorization.Roles.Role> _rolesRepo,
              IRepository<AbandonedOrderStatus, long> _abandonedOrderStatus,
               IRepository<InboundCalls> _inboundCalls,
              IRepository<CallDetails, long> _CallDetailsRepo,
               IRepository<Proplix.Services.Shopify.Address, long> _addressRepo,
               IRepository<ZopoxoOrder, long> zopoxoOrderRepository,
               IRepository<UtmParameters> utmParametersRepo,
               IRepository<AbandonedReportHeaders> abandonedReportHeadersRepo,
               IRepository<AbandonedRemarks> abandonedRemarksRepo,
               IRepository<AbandonedRemarkStatusMapping> abandonedRemarkStatusMappingRepo,
               IRepository<AbandonedReportStatusMapping> abandonedReportStatusMappingRepo,
              ICommonRepository commonRepository
            ) : base(abandonedOrderRepository)
        {
            this.abandonedOrderRepository = abandonedOrderRepository;
            abandonedCheckoutsRepository = _AbandonedCheckoutsRepository;
            orderStatusRepository = _orderStatusRepository;
            this.checkoutRepo = checkoutRepo;
            storeRepo = _StoreRepo;
            abandonedSyncStatusRepo = _AbandonedSyncStatusRepo;
            userRepo = _userRepo;
            rolesRepo = _rolesRepo;
            abandonedOrderStatus = _abandonedOrderStatus;
            inboundCalls = _inboundCalls;
            callDetailsRepo = _CallDetailsRepo;
            addressRepo = _addressRepo;
            this.zopoxoOrderRepository = zopoxoOrderRepository;
            this.utmParametersRepo = utmParametersRepo;
            this.abandonedReportHeadersRepo = abandonedReportHeadersRepo;
            this.abandonedRemarksRepo = abandonedRemarksRepo;
            this.abandonedRemarkStatusMappingRepo = abandonedRemarkStatusMappingRepo;
            this.abandonedReportStatusMappingRepo = abandonedReportStatusMappingRepo;
            this.commonRepository = commonRepository;
        }
        public async Task InsertAbandonedOrder(AbandonedOrderDto input)
        {
            var query = new AbandonedOrderInformationLog()
            {
                AbandonedOrderId = input.AbandonedOrderId,
                OrderStatusId = input.OrderStatusId,
                Remarks = input.Remarks,
                Information = input.Information,
                //CreatedAt = input.CreatedAt,
                //UpdatedAt = input.UpdatedAt,

            };
            await abandonedOrderRepository.InsertAsync(query);
        }
        public async Task<List<AbandonedOrderDto>> GetAllAbandonedOrder()
        {
            var query = await abandonedOrderRepository.GetAll().Select(x => new AbandonedOrderDto
            {
                AbandonedOrderId = x.AbandonedOrderId,
                OrderStatusId = x.OrderStatusId,
                Remarks = x.Remarks,
                Information = x.Information,
                //CreatedAt = x.CreatedAt,
                //UpdatedAt = x.UpdatedAt,

            }).ToListAsync();
            return query;
        }
        public async Task UpdateAbandonedOrder(AbandonedOrderDto abandonedOrder)
        {
            var query = await abandonedOrderRepository.GetAsync(abandonedOrder.Id);
            query.Id = abandonedOrder.Id;
            query.AbandonedOrderId = abandonedOrder.AbandonedOrderId;
            query.OrderStatusId = abandonedOrder.OrderStatusId;
            query.Remarks = abandonedOrder.Remarks;
            query.Information = abandonedOrder.Information;

            await abandonedOrderRepository.UpdateAsync(query);
        }
        public async Task DeleteAbandonedOrder(int id)
        {
            await abandonedOrderRepository.DeleteAsync(id);
        }

        public async Task<List<AbandonedOrderDto>> GetAbandonedOrder(int id)
        {
            var query = await abandonedOrderRepository.GetAll().Where(c => c.Id == id).Select(x => new AbandonedOrderDto
            {
                Id = x.Id,
                AbandonedOrderId = x.AbandonedOrderId,
                OrderStatusId = x.OrderStatusId,
                Remarks = x.Remarks,
                Information = x.Information,
                //CreatedAt = x.CreatedAt,
                //UpdatedAt = x.UpdatedAt,

            }).ToListAsync();
            return query;
        }
        [UnitOfWork(IsolationLevel.ReadUncommitted)]
        public async Task<ServerSidePaginationOutput<AllAbandonedOrdersDto>> GetAllAbandonedOrders(AbandonedFiltersDTO filter, ServerSidePaginationInput input)
        {
            var searchValue = Convert.ToString(input.Search.Value as Object).Trim().ToLower();
            var _startDate = new DateTime(filter.startDate.Year, filter.startDate.Month, filter.startDate.Day, 0, 0, 0, 0).ToUniversalTime();
            var _endDate = new DateTime(filter.endDate.Year, filter.endDate.Month, filter.endDate.Day, 23, 59, 59, 999).ToUniversalTime();
            var query = (from checkout in checkoutRepo.GetAll()
                                      .WhereIf(filter.remarkId != -1, x => x.RemarkId == filter.remarkId)
                                      .WhereIf(filter.statusId != 3 && filter.statusId != 2 && filter.statusId != 1, x => x.StatusId == filter.statusId)
                                      .WhereIf(filter.storeId != -1, x => x.StoreId == filter.storeId)
                                      .WhereIf(filter.statusId == 3, x => x.StatusId == 0 && x.AssigneeId != 0)
                                      .WhereIf(filter.statusId == 2, x => x.AssigneeId == 0)
                                      .WhereIf(filter.assignee != -1, x => x.AssigneeId == filter.assignee)
                                      .WhereIf(filter.statusId != 2 && filter.statusId != 1, x => x.assigningtDate >= _startDate
                                                                        && x.assigningtDate <= _endDate)
                                      .WhereIf(filter.statusId == 2 || (filter.statusId == 1 && input.Search.Value == null),
                                      x => x.CreatedAt >= _startDate.AddHours(-5).AddMinutes(-30) && x.CreatedAt <= _endDate.AddHours(-5).AddMinutes(-30))

                         join user in userRepo.GetAll()
                         on checkout.AssigneeId equals user.Id into uGroup
                         from u in uGroup.DefaultIfEmpty()

                         select new AllAbandonedOrdersDto
                         {
                             AbandonedCheckoutUrl = checkout.AbandonedCheckoutUrl,
                             Remarks = checkout.Remark,
                             ShippingAddress = checkout.FullShippingAddress,
                             Phone = checkout.Phone,
                             CreatedAt = (DateTimeOffset)checkout.CreatedAt,
                             RefNo = checkout.AbandonedCheckoutId,
                             Email = checkout.Email,
                             Customer = checkout.CustomerName,
                             LandingSite = checkout.LandingSite,
                             StoreId = checkout.StoreId,
                             StatusId = checkout.StatusId,
                             Id = checkout.Id,
                             AssigneeId = checkout.AssigneeId,
                             Product = (checkout.Products ?? "").Split(",", StringSplitOptions.None).ToList(),
                             TotalPrice = checkout.TotalPrice,
                             CallsAttempted = checkout.CallTimes,
                             OrderId = checkout.AbandonedOrderId,
                             AssignmentDate = checkout.assigningtDate.HasValue ? ((DateTime)(checkout.assigningtDate)) : null,
                             BillingAddress = checkout.FullBillingAddress,
                             AdditionalInfo = checkout.AdditionalInfo
                         })
                         .WhereIf(!string.IsNullOrEmpty(input.Search.Value), x => x.Email.Trim().ToLower().Contains(searchValue) ||
                          x.Phone.Trim().ToLower().Contains(searchValue) || x.Customer.Trim().ToLower().Contains(searchValue));
            query = SortingAllProducts(input, query);
            var totalCount = await query.CountAsync();
            var skipOrder = query.Skip(input.SkipCount);
            var abandoned = new List<AllAbandonedOrdersDto>();
            if (input.Take > 0)
            {
                abandoned = await skipOrder.Take(input.Take).ToListAsync();
            }
            else
            {
                abandoned = await skipOrder.ToListAsync();
            }
            abandoned = await GetJoinedAbandonedOrders(abandoned, filter);

            var res = new ServerSidePaginationOutput<AllAbandonedOrdersDto>()
            {
                Data = abandoned,
                TotalCount = totalCount,
                FilterCount = totalCount
            };
            return res;
        }
        public async Task<List<AllAbandonedOrdersDto>> GetJoinedAbandonedOrders(List<AllAbandonedOrdersDto> abandoned, AbandonedFiltersDTO filter)
        {

            if (filter.statusId != 2 && filter.statusId != 3)
            {
                var allStatus = await abandonedOrderStatus.GetAll().Select(x => new { x.Id, x.Status }).ToListAsync();
                abandoned = JoinEntities(abandoned, allStatus,
                    (abandonedOrder, entity) => abandonedOrder.StatusId == entity.Id,
                    (dto, joined) => dto.Status = joined.Status);
            }
            if (filter.statusId != 2)
            {
                var allUsers = await userRepo.GetAll().Select(x => new { x.Id, x.FullName }).ToListAsync();
                abandoned = JoinEntities(abandoned, allUsers,
                    (abandonedOrder, entity) => abandonedOrder.AssigneeId == entity.Id,
                    (dto, joined) => dto.Assignee = joined.FullName);
            }

            var allStores = await storeRepo.GetAll().Select(x => new { x.Id, x.Name }).ToListAsync();
            abandoned = JoinEntities(abandoned, allStores,
                (abandonedOrder, entity) => abandonedOrder.StoreId == entity.Id,
                (dto, joined) => dto.Store = joined.Name);

            return abandoned;
        }

        private List<AllAbandonedOrdersDto> JoinEntities<T>(List<AllAbandonedOrdersDto> abandoned, List<T> entities,
            Func<AllAbandonedOrdersDto, T, bool> joinCondition, Action<AllAbandonedOrdersDto, T> updateDto)
        {
            return (from abandonedOrder in abandoned
                    from entity in entities.Where(entity => joinCondition(abandonedOrder, entity)).DefaultIfEmpty()
                    select new { AbandonedOrder = abandonedOrder, Entity = entity })
                   .ToList()
                   .Select(joined =>
                   {
                       var dto = joined.AbandonedOrder;
                       if (joined.Entity != null)
                       {
                           updateDto(dto, joined.Entity);
                       }
                       return dto;
                   })
                   .ToList();
        }

        private static IQueryable<AllAbandonedOrdersDto> SortingAllProducts(ServerSidePaginationInput input, IQueryable<AllAbandonedOrdersDto> productQuery)
        {
            switch (input.Sorting.PropertyName)
            {
                case "date":
                    if (input.Sorting.Format == "desc")
                    {
                        productQuery = productQuery.OrderByDescending(x => x.CreatedAt);
                    }
                    else
                    {
                        productQuery = productQuery.OrderBy(x => x.CreatedAt);
                    }
                    break;

                case "assignmentDate":
                    if (input.Sorting.Format == "desc")
                    {
                        productQuery = productQuery.OrderByDescending(x => x.AssignmentDate);
                    }
                    else
                    {
                        productQuery = productQuery.OrderBy(x => x.AssignmentDate);
                    }
                    break;

                case "storeName":
                    if (input.Sorting.Format == "desc")
                    {
                        productQuery = productQuery.OrderByDescending(x => x.Store);
                    }
                    else
                    {
                        productQuery = productQuery.OrderBy(x => x.Store);
                    }
                    break;

                case "price":
                    if (input.Sorting.Format == "desc")
                    {
                        productQuery = productQuery.OrderByDescending(x => x.TotalPrice);
                    }
                    else
                    {
                        productQuery = productQuery.OrderBy(x => x.TotalPrice);
                    }
                    break;
                case "customer":
                    if (input.Sorting.Format == "desc")
                    {
                        productQuery = productQuery.OrderByDescending(x => x.Customer);
                    }
                    else
                    {
                        productQuery = productQuery.OrderBy(x => x.Customer);
                    }
                    break;

                case "assignee":
                    if (input.Sorting.Format == "desc")
                    {
                        productQuery = productQuery.OrderByDescending(x => x.Assignee);
                    }
                    else
                    {
                        productQuery = productQuery.OrderBy(x => x.Assignee);
                    }
                    break;

                case "status":
                    if (input.Sorting.Format == "desc")
                    {
                        productQuery = productQuery.OrderByDescending(x => x.Status);
                    }
                    else
                    {
                        productQuery = productQuery.OrderBy(x => x.Status);
                    }
                    break;

                case "remarks":
                    if (input.Sorting.Format == "desc")
                    {
                        productQuery = productQuery.OrderByDescending(x => x.Remarks);
                    }
                    else
                    {
                        productQuery = productQuery.OrderBy(x => x.Remarks);
                    }
                    break;
            }
            return productQuery;
        }
        public async Task<List<AbandonedLogOrderDto>> GetAbandonedOrdersById(int Id)
        {


            var abandonedOrders = await (from log in abandonedOrderRepository.GetAll()
                                         join user in userRepo.GetAll()
                                         on log.CreatorUserId equals user.Id

                                         join status in abandonedOrderStatus.GetAll()
                                         on log.OrderStatusId equals status.Id into bGroup
                                         from b in bGroup.DefaultIfEmpty()

                                         join details in callDetailsRepo.GetAll()
                                         on log.CallId equals details.Id into detailsGroup
                                         from d in detailsGroup.DefaultIfEmpty()

                                         where log.AbandonedOrderId == Id
                                         orderby (log.CreationTime) descending
                                         select new AbandonedLogOrderDto
                                         {
                                             Information = log.Information,
                                             CreationTime = log.CreationTime,
                                             User = user.UserName,
                                             Remarks = log.Remarks,
                                             Id = log.AbandonedOrderId,
                                             OrderStatus = b.Status,
                                             recordingURL = d.RecordingUrl
                                         }).ToListAsync();
            return abandonedOrders;

        }
        public async Task<bool> SaveAbandonedLogs(AbandonedLogsDto input, string type)
        {
            var newOrder = true;
            var query = new AbandonedOrderInformationLog()
            {
                AbandonedOrderId = input.AbandonedId,
                OrderStatusId = input.StatusId,
                Remarks = input.Remarks,
                Information = input.info
            };
            var order = await checkoutRepo.GetAll().AsNoTracking().Where(x => x.Id == input.AbandonedId).FirstOrDefaultAsync();
            order.Remark = input.Remarks;
            order.Status = input.Status;
            order.StatusId = input.StatusId;
            order.AdditionalInfo = input.info;
            order.CallTimes += 1;
            order.AbandonedOrderId = input.orderId;
            order.RemarkId = input.RemarksId;
            if (input.orderId != null)
            {
                var placedOrder = await zopoxoOrderRepository.GetAll().Where(x => x.OrderName.ToLower().Trim() == input.orderId.ToLower().Trim()).FirstOrDefaultAsync();
                if (placedOrder != null)
                {
                    placedOrder.isAbandoned = true;
                    await zopoxoOrderRepository.UpdateAsync(placedOrder);
                }

                var existingOrder = await checkoutRepo.GetAll().Where(x => x.AbandonedOrderId.Trim().ToLower() == input.orderId.Trim().ToLower()).FirstOrDefaultAsync();
                if (existingOrder != null)
                {
                    newOrder = false;
                    return false;
                }
            }
            if (type == "invalid-order")
            {
                order.AssigneeId = (int)input.AssigneeId;
                order.CallTimes = 0;
                order.assigningtDate = DateTime.Now.ToUniversalTime();
            }
            if (newOrder)
            {
                await checkoutRepo.UpdateAsync(order);
                await abandonedOrderRepository.InsertAsync(query);
            }
            return true;
        }
        public async Task<bool> InsertAbandonedOrders()
        {
            var stores = await storeRepo.GetAll().ToListAsync();
            foreach (var store in stores)
            {
                try
                {
                    var lastAbandonedOrder = checkoutRepo.GetAll().OrderByDescending(x => x.CreatedAt).FirstOrDefault(x => x.StoreId == store.Id);
                    DateTimeOffset modificationTime = new DateTimeOffset();
                    if (lastAbandonedOrder != null)
                    {
                        modificationTime = (DateTimeOffset)lastAbandonedOrder.CreatedAt;
                        modificationTime = modificationTime.AddSeconds(1);
                    }
                    else
                    {
                        modificationTime = DateTime.Now;
                        modificationTime = modificationTime.AddHours(-24);
                    }

                    //var service = new CheckoutService(store.ShopifyUrl, store.AccessToken);
                    //var allAbandonedOrders = await service.ListAsync(new CheckoutListFilter { CreatedAtMin = modificationTime, Limit = 250 });
                    //var order = allAbandonedOrders.Items.ToList();
                    if (store.ShopifyUrl == null)
                    {
                        continue;
                    }
                    var order = await FetchAbandonedCheckouts(store.ShopifyUrl, store.AccessToken, modificationTime);
                    foreach (var item in order)
                    {
                        var items = (Proplix.Services.Shopify.Checkout)item;
                        items.CreatedAt = ((DateTimeOffset)items.CreatedAt).ToUniversalTime();
                        try
                        {
                            items.ShippingAddress.Phone = TrimPhoneNumber(items.ShippingAddress?.Phone);
                        }
                        catch (Exception ex)
                        {

                        }

                        try
                        {
                            items.BillingAddress.Phone = TrimPhoneNumber(items.BillingAddress?.Phone);
                        }
                        catch (Exception ex)
                        {

                        }
                        items.Products = string.Join(",", items.LineItems.Select(x => x.Title).ToList());
                        items.FullShippingAddress = items.ShippingAddress?.City + " " + items.ShippingAddress?.Address1 + " " + items.ShippingAddress?.Province + " " + items.ShippingAddress?.Zip;
                        items.CustomerName = items.BillingAddress?.Name ?? items.ShippingAddress?.Name ?? items?.Customer?.FirstName + " " + items?.Customer?.LastName;
                        items.Phone = items.BillingAddress?.Phone ?? items.ShippingAddress?.Phone ?? TrimPhoneNumber((items.Customer?.Phone));
                        items.FullBillingAddress = items.BillingAddress?.City + " " + items.BillingAddress?.Address1 + " " + items.BillingAddress?.Province + " " + items.BillingAddress?.Zip;
                        var exist = await checkoutRepo.GetAll().FirstOrDefaultAsync(x => x.AbandonedCheckoutId == items.AbandonedCheckoutId);
                        if (exist == null)
                        {
                            items.StoreId = store.Id;
                            await checkoutRepo.InsertAsync(items);
                        }
                    }
                }
                catch (Exception ex)
                {

                }

            }
            return true;

        }
        public async Task assigneOrders(AssignOrders input)
        {
            var order = await checkoutRepo.GetAll().Where(x => input.OrderId.Contains((int)x.Id)).ToListAsync();
            order = order.Select(q =>
            {
                q.AssigneeId = input.UserId;
                q.assigningtDate = DateTime.Now;
                return q;
            }).ToList();
            foreach (var item in order)
            {
                await checkoutRepo.UpdateAsync(item);
            }
        }
        [AbpAllowAnonymous]
        [ApiExplorerSettings(IgnoreApi = true)]
        public static string TrimPhoneNumber(string number)
        {
            if (!string.IsNullOrEmpty(number))
            {
                var result = number.Replace("-", "").Replace(" ", "").Replace(",", "").Trim();
                result = result.Length > 10 ? result.Remove(0, result.Length - 10) : result;
                return result;
            }
            return null;
        }

        public async Task<List<Dictionary<string, object>>> AbandonedStoreReportData(DateTime startDate, int userId, int storeId, DateTime endDate)
        {
            var mappedHeaders = await GetMappedHeadersAsync();

            var storeOrders = await commonRepository.GetAbandonedStoreReport(startDate, userId, storeId, endDate);

            return GetAbandonedReportData(storeOrders, mappedHeaders, "StoreName");
        }
        public async Task<List<Dictionary<string, object>>> AbandonedUserReportData(DateTime startDate, int userId, int storeId, DateTime endDate)
        {
            var mappedHeaders = await GetMappedHeadersAsync();

            var userOrders = await commonRepository.GetAbandonedUserReport(startDate, userId, storeId, endDate);

            return GetAbandonedReportData(userOrders, mappedHeaders, "User");
        }
        public async Task<List<Dictionary<string, object>>> AbandonedMonthlyReportData(DateTime startDate, int userId, int storeId)
        {
            var mappedHeaders = await GetMappedHeadersAsync();

            var dateOrders = await commonRepository.GetAbandonedMonthlyReport(startDate, userId, storeId);

            return GetAbandonedReportData(dateOrders, mappedHeaders, "Date");
        }
        public async Task<List<AbandonedStoreReportDTO>> AbandonedPrevRecallingReportData(DateTime startDate, int userId, int storeId, DateTime endDate)
        {
            var res = await commonRepository.GetAbandonedPrevReacallingReport(startDate, userId, storeId, endDate);
            return res;
        }
        public async Task unAssigneOrders(List<int> allId)
        {
            var order = await checkoutRepo.GetAll().Where(x => allId.Contains((int)x.Id)).ToListAsync();
            order = order.Select(q =>
            {
                q.AssigneeId = 0;
                q.assigningtDate = null;
                return q;
            }).ToList();
            foreach (var item in order)
            {
                await checkoutRepo.UpdateAsync(item);
            };
        }
        public async Task CreateAbandonedStatus(CreateAbandonedStatusDto input)
        {
            var query = new AbandonedOrderStatus()
            {
                Status = input.Status,
            };
            var id = await abandonedOrderStatus.InsertAndGetIdAsync(query);
            foreach (var mappingId in input.ReportStatusIds)
            {
                AbandonedReportStatusMapping abandonedReportStatusMapping = new AbandonedReportStatusMapping();
                abandonedReportStatusMapping.ReportHeaderId = mappingId;
                abandonedReportStatusMapping.CallingStatusId = (int)id;
                await abandonedReportStatusMappingRepo.InsertAsync(abandonedReportStatusMapping);
            }
        }
        public async Task EditAbandonedStatus(EditAbandonedStatusDto input)
        {
            var callStatus = await abandonedOrderStatus.GetAll().Where(x => x.Id == input.StatusId).FirstOrDefaultAsync();
            var oldAllHeaders = await abandonedReportStatusMappingRepo.GetAll().Where(x => x.CallingStatusId == input.StatusId).ToListAsync();
            var oldHeaders = oldAllHeaders.Select(x => x.ReportHeaderId).ToList();

            var deletedStatusId = oldHeaders.Where(oldHeader => !input.ReportStatusIds.Contains(oldHeader)).ToList();
            var newStatusId = input.ReportStatusIds.Where(newHeader => !oldHeaders.Contains(newHeader)).ToList();


            foreach (var deletionId in deletedStatusId)
            {
                var toDelete = oldAllHeaders.Where(x => x.ReportHeaderId == deletionId).FirstOrDefault();
                if (toDelete != null)
                {
                    toDelete.IsDeleted = true;
                }
                await abandonedReportStatusMappingRepo.UpdateAsync(toDelete);
            }
            foreach (var mappingId in newStatusId)
            {
                AbandonedReportStatusMapping abandonedReportStatusMapping = new AbandonedReportStatusMapping();
                abandonedReportStatusMapping.ReportHeaderId = mappingId;
                abandonedReportStatusMapping.CallingStatusId = input.StatusId;
                await abandonedReportStatusMappingRepo.InsertAsync(abandonedReportStatusMapping);
            }
            callStatus.Status = input.NewStatus;
            await abandonedOrderStatus.UpdateAsync(callStatus);
        }
        public async Task<List<GetAbandonedStatusDto>> GetGroupedAbandonedStatus()
        {
            var status = await (abandonedOrderStatus.GetAll()
               .Select(x => new GetAbandonedStatusDto()
               {
                   Status = x.Status,
                   id = x.Id
               })).ToListAsync();
            return status;
        }
        public async Task<List<GetAbandonedRemarksDto>> GetAllRemark()
        {
            var status = await (from remark in abandonedRemarksRepo.GetAll()
                                join mappings in abandonedRemarkStatusMappingRepo.GetAll()
                                on remark.Id equals mappings.RemarkId into mappingRemarkData
                                from mapped in mappingRemarkData.DefaultIfEmpty()
                                select new
                                {
                                    Remark = remark.Remark,
                                    CallingStatusId = mapped != null ? mapped.CallingStatusId : 0,
                                    id = remark.Id
                                }).GroupBy(x => x.id).Select(y => new GetAbandonedRemarksDto
                                {
                                    id = y.Key,
                                    Remark = y.Select(x => x.Remark).First(),
                                    CallingStatusId = y.Select(x => x.CallingStatusId).Where(x => x != 0).ToList()
                                }).ToListAsync();
            return status;
        }

        public async Task<List<GetAllAbandonedStatusDto>> GetAllAbandonedStatus()
        {
            return await ((from callStatus in abandonedOrderStatus.GetAll()
                           join mappings in abandonedReportStatusMappingRepo.GetAll()
                           on callStatus.Id equals mappings.CallingStatusId into mappingData
                           from mapped in mappingData.DefaultIfEmpty()

                           join headers in abandonedReportHeadersRepo.GetAll()
                           on mapped.ReportHeaderId equals headers.Id into mappingHeaderData
                           from mappedHeader in mappingHeaderData.DefaultIfEmpty()

                           select new
                           {
                               CallStatus = callStatus.Status,
                               CallStatusId = callStatus.Id,
                               HeaderId = mapped != null ? mapped.ReportHeaderId : 0,
                               Header = mappedHeader != null ? mappedHeader.Header : ""
                           }
                            ).GroupBy(x => x.CallStatusId).Select(y => new GetAllAbandonedStatusDto()
                            {
                                CallStatus = y.Select(x => x.CallStatus).FirstOrDefault(),
                                CallStatusId = y.Key,
                                ReportHeader = y.Select(x => x.Header).Where(x => !string.IsNullOrEmpty(x)).ToList(),
                                ReportHeaderId = y.Select(x => x.HeaderId).Where(x => x != 0).ToList(),
                            })).ToListAsync();
        }
        public async Task<List<GetAbandonedStatusDto>> GetAbandonedReportHeaders()
        {
            return await abandonedReportHeadersRepo.GetAll().Select(x => new GetAbandonedStatusDto
            {
                id = x.Id,
                Status = x.Header
            }).ToListAsync();
        }

        public async Task UnuseableOrders(UnuseableOrders input)
        {
            var order = await checkoutRepo.GetAll().Where(x => input.OrderIds.Contains((int)x.Id)).ToListAsync();
            order = order.Select(q =>
            {
                q.StatusId = input.statusId;
                q.AssigneeId = input.UserId;
                q.assigningtDate = DateTime.Now.ToUniversalTime();
                return q;

            }).ToList();
            foreach (var item in order)
            {
                await checkoutRepo.UpdateAsync(item);
            }
            foreach (var item in input.OrderIds)
            {
                var query = new AbandonedOrderInformationLog()
                {
                    AbandonedOrderId = item,
                    OrderStatusId = input.statusId,
                    Remarks = input.remark,
                    Information = input.info
                };
                await abandonedOrderRepository.InsertAsync(query);
            }

        }
        public async Task<List<UserInfoDTO>> GetAllAbandonedUsers()
        {
            return await commonRepository.GetUsersByPermission("Module.Abandoned.CallingAgent");

        }
        public async Task<List<AbandonedDailyReportDTO>> AbandonedDailyReportData(DateTime startDate, int userId, int storeId, DateTime endDate, int skip, int take)
        {
            var res = await commonRepository.GetAbandonedDailyReport(startDate, userId, storeId, endDate, skip, take);
            return res;
        }

        public async Task<List<AbandonedDailyReportDTO>> AbandonedDailyReportDataExport(DateTime startDate, int userId, int storeId, DateTime endDate)
        {
            var res = await commonRepository.GetAbandonedDailyReport(startDate, userId, storeId, endDate, -1, -1);
            return res;
        }


        public async Task<List<AllAbandonedOrdersDto>> GetAbandonedCallOrderDetails(int id)
        {
            var ids = callDetailsRepo.GetAll().FirstOrDefault(x => x.Id == id);
            List<long> primaryIds = new List<long>();
            var abandonedOrders = new List<AllAbandonedOrdersDto>();
            try
            {
                if (ids != null)
                {
                    primaryIds = ids.CustomId.Any() ? ids.CustomId.Split(",").Select(long.Parse).ToList() : null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            if (primaryIds == null)
            {
                return abandonedOrders;
            }
            abandonedOrders = await (from checkout in checkoutRepo.GetAll().Include(x => x.BillingAddress).Include(y => y.LineItems).Include(x => x.Customer)
                                     .Where(x => primaryIds.Contains(x.Id))
                                     join abandoneStatus in abandonedOrderStatus.GetAll()
                                     on checkout.StatusId equals abandoneStatus.Id into sGroup
                                     from s in sGroup.DefaultIfEmpty()

                                     join store in storeRepo.GetAll()
                                     on checkout.StoreId equals store.Id into bGroup
                                     from b in bGroup.DefaultIfEmpty()

                                     orderby checkout.CreatedAt descending
                                     select new AllAbandonedOrdersDto
                                     {
                                         AbandonedCheckoutUrl = checkout.AbandonedCheckoutUrl,
                                         Remarks = checkout.Remark,
                                         ShippingAddress = checkout.ShippingAddress.City + "," + checkout.ShippingAddress.Address1 + " " + checkout.ShippingAddress.Province + " " + checkout.ShippingAddress.Zip,
                                         Phone = checkout.BillingAddress.Phone != null ? checkout.BillingAddress.Phone : checkout.Customer.Phone != null ? checkout.Customer.Phone : checkout.ShippingAddress.Phone,
                                         CreatedAt = ((DateTimeOffset)checkout.CreatedAt).DateTime,
                                         RefNo = checkout.AbandonedCheckoutId,
                                         Email = checkout.Email,
                                         Customer = checkout.BillingAddress.Name != null ? checkout.BillingAddress.Name : checkout.Customer.FirstName + " " + checkout.Customer.LastName,
                                         LandingSite = checkout.LandingSite,
                                         Store = b.Name,
                                         Status = s.Status,
                                         Id = checkout.Id,
                                         Product = checkout.LineItems.Select(x => x.Title).ToList(),
                                         TotalPrice = checkout.TotalPrice,
                                         CallsAttempted = checkout.CallTimes,
                                         OrderId = checkout.AbandonedOrderId,
                                         AssignmentDate = checkout.assigningtDate.HasValue ? ((DateTime)(checkout.assigningtDate)): null,
                                         BillingAddress = checkout.BillingAddress.City + "," + checkout.BillingAddress.Address1 + " " + checkout.BillingAddress.Province + " " + checkout.BillingAddress.Zip,
                                     }).ToListAsync();

            return abandonedOrders;

        }
        public async Task<List<AllAbandonedOrdersDto>> GetAbandonedOrderById(int Id)
        {
            var abandonedOrders = new List<AllAbandonedOrdersDto>();
            abandonedOrders = await (from checkout in checkoutRepo.GetAll().Include(x => x.BillingAddress).Include(y => y.LineItems).Include(x => x.Customer)
                                     .Where(x => x.Id == Id)
                                     join abandoneStatus in abandonedOrderStatus.GetAll()
                                     on checkout.StatusId equals abandoneStatus.Id into sGroup
                                     from s in sGroup.DefaultIfEmpty()

                                     join store in storeRepo.GetAll()
                                     on checkout.StoreId equals store.Id into bGroup
                                     from b in bGroup.DefaultIfEmpty()

                                     orderby checkout.CreatedAt descending
                                     select new AllAbandonedOrdersDto
                                     {
                                         AbandonedCheckoutUrl = checkout.AbandonedCheckoutUrl,
                                         Remarks = checkout.Remark,
                                         ShippingAddress = checkout.ShippingAddress.City + "," + checkout.ShippingAddress.Address1 + " " + checkout.ShippingAddress.Province + " " + checkout.ShippingAddress.Zip,
                                         Phone = checkout.BillingAddress.Phone != null ? checkout.BillingAddress.Phone : checkout.Customer.Phone != null ? checkout.Customer.Phone : checkout.ShippingAddress.Phone,
                                         CreatedAt = ((DateTimeOffset)checkout.CreatedAt).DateTime,
                                         RefNo = checkout.AbandonedCheckoutId,
                                         Email = checkout.Email,
                                         Customer = checkout.BillingAddress.Name != null ? checkout.BillingAddress.Name : checkout.Customer.FirstName + " " + checkout.Customer.LastName,
                                         LandingSite = checkout.LandingSite,
                                         Store = b.Name,
                                         Status = s.Status,
                                         Id = checkout.Id,
                                         Product = checkout.LineItems.Select(x => x.Title).ToList(),
                                         TotalPrice = checkout.TotalPrice,
                                         CallsAttempted = checkout.CallTimes,
                                         OrderId = checkout.AbandonedOrderId,
                                         AssignmentDate = checkout.assigningtDate.HasValue ? ((DateTime)(checkout.assigningtDate)) : null,
                                         BillingAddress = checkout.BillingAddress.City + "," + checkout.BillingAddress.Address1 + " " + checkout.BillingAddress.Province + " " + checkout.BillingAddress.Zip,
                                     }).ToListAsync();

            return abandonedOrders;

        }

        public async Task AlreadyPlacedOrders(List<int> allId, int placedId)
        {
            var loggedUserId = AbpSession.UserId;
            var order = await checkoutRepo.GetAll().Where(x => allId.Contains((int)x.Id)).ToListAsync();
            order = order.Select(q =>
            {
                q.StatusId = placedId;
                q.AssigneeId = (int)loggedUserId;
                q.assigningtDate = DateTime.Now.ToUniversalTime();
                return q;
            }).ToList();

            foreach (var item in order)
            {
                var log = new AbandonedOrderInformationLog
                {
                    Remarks = "Assigning",
                    OrderStatusId = placedId,
                    AbandonedOrderId = (int)item.Id
                };
                await abandonedOrderRepository.InsertAsync(log);
                await checkoutRepo.UpdateAsync(item);
            };
        }
        public async Task<bool> RemoveDuplicateOrders(DateTime startDate, DateTime endDate)
        {
            var uid = AbpSession.UserId.Value;
            var res = await commonRepository.RemoveDuplicateOrders(startDate, endDate, uid);
            return res;
        }
        public async Task<List<AbandonedFUllReportDTO>> AbandonedFullReportData(DateTime startDate, int userId, int storeId, DateTime endDate)
        {
            var res = await commonRepository.GetAbandonedFullReport(startDate, userId, storeId, endDate);
            return res;
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<List<UtmReport>> UtmData(GetUtmDto utmInput)
        {
            CurrentUnitOfWork.SetTenantId(utmInput.tenantId);
            //utmInput.startDate = new DateTime(utmInput.startDate.Year, utmInput.startDate.Month, utmInput.startDate.Day, 0, 0, 0, 0).ToUniversalTime();
            //utmInput.endDate = new DateTime(utmInput.endDate.Year, utmInput.endDate.Month, utmInput.endDate.Day, 23, 59, 59, 999).ToUniversalTime();
            return await (from utm in utmParametersRepo.GetAll().WhereIf(utmInput.storeIds.Count()>0, x => utmInput.storeIds.Contains(x.StoreId))
                          join store in storeRepo.GetAll()
                          on utm.StoreId equals store.Id
                          where utm.CreationTime >= utmInput.startDate && utm.CreationTime <= utmInput.endDate
                          select new UtmReport
                          {
                              Campaign = utm.Campaign,
                              Content = utm.Content,
                              Medium = utm.Medium,
                              OrderName = utm.OrderName,
                              Source = utm.Source,
                              Store = store.Name

                          }).ToListAsync();

        }
        public async Task CreateReportStatus(CreateAbandonedReportDto input)
        {
            var query = new AbandonedReportHeaders()
            {
                Header = input.Header,
            };
            await abandonedReportHeadersRepo.InsertAsync(query);
        }
        public async Task CreateAbandonedRemark(CreateAbandonedRemarkDto input)
        {
            var query = new AbandonedRemarks()
            {
                Remark = input.Remark,
            };
            var id = await abandonedRemarksRepo.InsertAndGetIdAsync(query);
            foreach (var mappingId in input.StatusIds)
            {
                AbandonedRemarkStatusMapping abandonedRemarkStatusMapping = new AbandonedRemarkStatusMapping();
                abandonedRemarkStatusMapping.RemarkId = id;
                abandonedRemarkStatusMapping.CallingStatusId = mappingId;
                await abandonedRemarkStatusMappingRepo.InsertAsync(abandonedRemarkStatusMapping);
            }
        }
        public async Task EditAbandonedRemark(EditRemarkStatusDto input)
        {
            var remark = await abandonedRemarksRepo.GetAll().Where(x => x.Id == input.RemarkId).FirstOrDefaultAsync();
            var oldAllRemarks = await abandonedRemarkStatusMappingRepo.GetAll().Where(x => x.RemarkId == input.RemarkId).ToListAsync();
            var oldRemarks = oldAllRemarks.Select(x => x.CallingStatusId).ToList();

            var deletedStatusId = oldRemarks.Where(oldHeader => !input.StatusIds.Contains(oldHeader)).ToList();
            var newStatusId = input.StatusIds.Where(newHeader => !oldRemarks.Contains(newHeader)).ToList();


            foreach (var deletionId in deletedStatusId)
            {
                var toDelete = oldAllRemarks.Where(x => x.CallingStatusId == deletionId).FirstOrDefault();
                if (toDelete != null)
                {
                    toDelete.IsDeleted = true;
                }
                await abandonedRemarkStatusMappingRepo.UpdateAsync(toDelete);
            }
            foreach (var mappingId in newStatusId)
            {
                AbandonedRemarkStatusMapping abandonedRemarkStatusMapping = new AbandonedRemarkStatusMapping();
                abandonedRemarkStatusMapping.RemarkId = input.RemarkId;
                abandonedRemarkStatusMapping.CallingStatusId = mappingId;
                await abandonedRemarkStatusMappingRepo.InsertAsync(abandonedRemarkStatusMapping);
            }
            remark.Remark = input.NewRemark;
            await abandonedRemarksRepo.UpdateAsync(remark);

        }
        public async Task<List<GetAllAbandonedRemarkDto>> GetAllAbandonedRemarks()
        {
            return await ((from remark in abandonedRemarksRepo.GetAll()
                           join mappings in abandonedRemarkStatusMappingRepo.GetAll()
                           on remark.Id equals mappings.RemarkId into mappingRemarkData
                           from mapped in mappingRemarkData.DefaultIfEmpty()


                           join callStatus in abandonedOrderStatus.GetAll()
                           on mapped.CallingStatusId equals callStatus.Id into mappingCallingStatusData
                           from mappedHeader in mappingCallingStatusData.DefaultIfEmpty()
                           select new
                           {
                               Remark = remark.Remark,
                               RemarkId = remark.Id,
                               CallStatusId = mapped != null ? mapped.CallingStatusId : 0,
                               CallStatus = mappedHeader != null ? mappedHeader.Status : ""
                           }
             ).GroupBy(x => x.RemarkId).Select(y => new GetAllAbandonedRemarkDto()
             {
                 Remarks = y.Select(x => x.Remark).FirstOrDefault(),
                 RemarksId = y.Key,
                 CallStatus = y.Select(x => x.CallStatus).Where(x => !string.IsNullOrEmpty(x)).ToList(),
                 CallStatusId = y.Select(x => x.CallStatusId).Where(x => x != 0).ToList(),
             })).ToListAsync();
        }
        private async Task<List<mappedHeadersDto>> GetMappedHeadersAsync()
        {
            var mappedHeaders = await (
                from header in abandonedReportHeadersRepo.GetAll()
                join mapping in abandonedReportStatusMappingRepo.GetAll()
                on header.Id equals mapping.ReportHeaderId into allHeaders
                from mapped in allHeaders.DefaultIfEmpty()
                select new
                {
                    header.Header,
                    HeaderId = header.Id,
                    Statuses = (mapped != null) ? mapped.CallingStatusId : -999
                })
                .GroupBy(x => x.HeaderId)
                .Select(y => new mappedHeadersDto
                {
                    HeaderId = y.Key,
                    Headers = y.Select(o => o.Header).FirstOrDefault(),
                    StatusIds = y.Select(o => o.Statuses).ToList()
                })
                .ToListAsync();

            var header1 = mappedHeaders.Find(x => x.HeaderId == 1);
            if (header1 != null)
            {
                header1.StatusIds.Add(0);
            }
            
            var header3 = mappedHeaders.Find(x => x.HeaderId == 3);
            if (header3 != null)
            {
                header3.StatusIds.Add(0);
            }

            return mappedHeaders;
        }
        private List<Dictionary<string, object>> GetAbandonedReportData(List<AbandonedStoreReportDTO> orders, List<mappedHeadersDto> mappedHeaders, string type)
        {
            return orders.GroupBy(dataItem => dataItem.Name)
                                .Select(group =>
                                {
                                    var headerCounts = mappedHeaders.ToDictionary(
                                        mappingItem => Regex.Replace(mappingItem.Headers, @"\s+", ""),
                                        mappingItem => group.Sum(dataItem => mappingItem.StatusIds.Contains(dataItem.Status) ? dataItem.Value : 0)
                                        );

                                    var placed = headerCounts.GetValueOrDefault("Placed", 0);
                                    var attempted = headerCounts.GetValueOrDefault("Attempted", 1); // Avoid division by zero
                                    var connected = headerCounts.GetValueOrDefault("Connected", 1);

                                    return new Dictionary<string, object>
                                    {
                                                                        { type, group.Key },
                                                                        { "PlacedWithAttempted", attempted != 0 ?(placed * 100.0) / attempted : 0 },
                                                                        { "PlacedWithConnected", connected != 0 ? (placed * 100.0) / connected : 0 },
                                                                        { "ConnectedWithAttempted", attempted != 0 ? (connected * 100.0) / attempted : 0 }
                                    }
                                    .Concat(headerCounts.Select(kv => new KeyValuePair<string, object>(kv.Key, kv.Value)))
                                    .ToDictionary(kv => kv.Key, kv => kv.Value);
                                }).ToList();
        }
        [AllowAnonymous]
        [ApiExplorerSettings(IgnoreApi = true)]
        public static async Task<List<ShopifySharp.Checkout>> FetchAbandonedCheckouts(string shopUrl, string accessToken, DateTimeOffset createdAtMin)
        {
            shopUrl= CommonStaticService.GetStoreUrl(shopUrl, false);

            var checkouts = new List<ShopifySharp.Checkout>();
            string endpoint = $"{shopUrl}/admin/api/2023-10/checkouts.json";
            string nextPageUrl = $"{endpoint}?created_at_min={createdAtMin.ToString("yyyy-MM-ddTHH:mm:ssZ")}&limit=250";

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("X-Shopify-Access-Token", accessToken);

                while (!string.IsNullOrEmpty(nextPageUrl))
                {
                    var response = await httpClient.GetAsync(nextPageUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        JObject jsonObject = JObject.Parse(content);
                        JArray checkoutArray = (JArray)jsonObject["checkouts"];

                        foreach (JObject obj in checkoutArray)
                        {
                            obj.Remove("shipping_lines");
                            if (obj["shipping_lines"] != null)
                            {
                                obj["shipping_lines"] = new JArray();
                            }
                            else
                            {
                                obj.Add("shipping_lines", new JArray());
                            }
                        }

                        var checkoutsPage = JsonConvert.DeserializeObject<List<ShopifySharp.Checkout>>(checkoutArray.ToString());
                        checkouts.AddRange(checkoutsPage);

                        nextPageUrl = GetNextPageUrlFromLinkHeader(response.Headers);
                    }
                    else
                    {
                        Console.WriteLine($"Error fetching abandoned checkouts: {response.ReasonPhrase}");
                        return new List<ShopifySharp.Checkout>();
                    }
                }
            }

            return checkouts;
        }
        [AllowAnonymous]
        [ApiExplorerSettings(IgnoreApi = true)]
        public static string GetNextPageUrlFromLinkHeader(HttpResponseHeaders headers)
        {
            const string linkHeaderKey = "Link";

            if (headers.TryGetValues(linkHeaderKey, out IEnumerable<string> headerValues))
            {
                var linkHeader = string.Join(",", headerValues);

                var links = linkHeader.Split(',');
                foreach (var link in links)
                {
                    var trimmedLink = link.Trim();
                    var parts = trimmedLink.Split(';');
                    if (parts.Length == 2 && parts[1].Trim() == "rel=\"next\"")
                    {
                        var urlPart = parts[0].Trim();
                        if (urlPart.StartsWith("<") && urlPart.EndsWith(">"))
                        {
                            return urlPart.Substring(1, urlPart.Length - 2);
                        }
                    }
                }
            }

            return null;
        }


    }
}

