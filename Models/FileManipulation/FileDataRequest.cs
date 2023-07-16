namespace StreamApi.Models.FileManipulation
{
    public class FileDataRequest
    {
        public string? InputFilePath { get; set; }
        public string? OutputFilePath { get; set; }
        public string InvoiceNo { get; set; } = "";
        public string BillingAccountId { get; set; } = "";
        public string SequenceNo { get; set; } = "";
        public string MwNotificationType { get; set; } = "None";
    }//class
}//namespace
