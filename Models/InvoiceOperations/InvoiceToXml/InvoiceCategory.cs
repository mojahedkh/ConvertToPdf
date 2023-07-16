namespace StreamApi.Models.InvoiceOperations.InvoiceToXml
{
    public class InvoiceCategory
    {
        public string? InvoiceType { get; set; }
        public DateTime InvoiceCycle { get; set; }
        public string? InvoiceCycleAsString { get; set; }
        public string? InvoiceNo { get; set; }
        public string? BillingRequestId { get; set; }
    }//class
}//namespace
