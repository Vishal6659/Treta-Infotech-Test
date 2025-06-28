namespace InvoiceManagementSystem.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string VendorName { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public List<InvoiceItem> Items { get; set; } = new();
    }
}
