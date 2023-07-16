namespace StreamApi.Models.General
{
    public class GeneralResponseError
    {
        public string ErrorCode { get; set; } = "Success";//(Success|Fail|Fatal|Warning)
        public string ErrorMessage { get; set; } = "The operation done successfully.";

        public GeneralResponseError(string ErrorCode, string ErrorMessage)
        {
            this.ErrorCode = ErrorCode;
            this.ErrorMessage = ErrorMessage;
        }//GeneralResponseError

        public GeneralResponseError() { }//GeneralResponseError

    }//class
}//namespace