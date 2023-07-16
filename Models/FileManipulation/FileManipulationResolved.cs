using StreamApi.Models.General;

namespace StreamApi.Models.FileManipulation
{
    public class FileManipulationResolved : GeneralResponseError
    {
        //public string? FileBase64String { get; set; }

        public byte[]? FileInByte { get; set; }
    }//class
}//namespace