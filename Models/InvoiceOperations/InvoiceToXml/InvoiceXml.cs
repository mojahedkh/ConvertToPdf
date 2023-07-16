using Microsoft.VisualBasic;

namespace StreamApi.Models.InvoiceOperations.InvoiceToXml
{
    public class InvoiceXml
    {
        public string? InvoiceType { get; set; }
        public DateTime InvoiceCycle { get; set; }
        public string? BillingRequestId { get; set; }
        public string? AccountNo { get; set; }
        public string? InvoiceNo { get; set; }
        public string? MobileNumber { get; set; }
        public string? CustomerName { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? Address3 { get; set; }
        public string? Address4 { get; set; }
        public string? ADNumber { get; set; }
        public string? InvoicePeriod { get; set; }
        public string? SubscriberClassification { get; set; }
        public string? Email { get; set; }
        public string? DueDate { get; set; }
        public double MonthlyPackageFee { get; set; }
        public double ValueAddedServicesFees { get; set; }
        public double CallsToJawwalAndPaltel { get; set; }
        public double CallsToOtherLocalNetworks { get; set; }
        public double CallsToIsraeliNetworks { get; set; }
        public double MobileInternet { get; set; }
        public double InternationalCalls { get; set; }
        public double NationalRoaming { get; set; }
        public double InternationalRoaming { get; set; }
        public double OneNetwork { get; set; }
        public double Messages { get; set; }
        public double CallsToShortCodes { get; set; }
        public double OtherCharges { get; set; }
        public double HandsetInstallmentsReceivables { get; set; }
        public double Discount { get; set; }
        public double Total { get; set; }
        public double ValueAddedTax { get; set; }
        public double GrandTotal { get; set; }
        public double DueInvoices { get; set; }
        public double TotalDueAmount { get; set; }
    }//class
}//namespace