namespace StreamApi.Models.General
{
    public class GeneralErrorMessage
    {
        public int ResponseStatus { get; set; } = StatusCodes.Status200OK;
        public bool TerminateProccess { get; set; } = false;
        public GeneralResponseError? GeneralResponseError { get; set; }

        public object? ResponseObject { get; set; }

        public GeneralErrorMessage(int ResponseStatus, object? ResponseObject)
        {
            this.ResponseStatus = ResponseStatus;
            this.ResponseObject = ResponseObject;
        }//constructor

        public GeneralErrorMessage(bool TerminateProccess, int ResponseStatus, string ErrorCode, string ErrorMessage, object? ResponseObject)
        {
            this.TerminateProccess = TerminateProccess;
            this.ResponseStatus = ResponseStatus;
            this.ResponseObject = ResponseObject;
            GeneralResponseError = new GeneralResponseError(ErrorCode, ErrorMessage);

        }//constructor

        public GeneralErrorMessage() { }//constructor
    }//class
}//namespace
