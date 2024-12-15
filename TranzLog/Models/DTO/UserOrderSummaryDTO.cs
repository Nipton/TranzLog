namespace TranzLog.Models.DTO
{
    public class UserOrderSummaryDTO
    {
        public int OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public double DeliveryCost { get; set; }
    }
}
