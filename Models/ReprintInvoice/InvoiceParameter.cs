namespace StreamApi.Models.ReprintInvoice
{
    public class InvoiceParameter
    {
        public string? type { get; set; }
        public DateTime cycle { get; set; }
        public string? invoiceDocCode { get; set; }
        public string? billingRequestId { get; set; }
    }//class
}//namespace
