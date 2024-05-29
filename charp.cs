    public class GetManifestOrderDto : AuditedEntityDto<long>
    {
        public string OrderName { get; set; }
        public string CustomerName { get; set; }
        public string TrackingNumber { get; set; }
        public string Status { get; set; }
        public int StatusId { get; set; }
        public string ProductName { get; set; }

    }
        public class GetManifestOrder : AuditedEntityDto<long>
    {
        public string OrderName { get; set; }
        public string CustomerName { get; set; }
        public string TrackingNumber { get; set; }
        public string Status { get; set; }
        public int StatusId { get; set; }
        public string ProductName { get; set; }

    }
