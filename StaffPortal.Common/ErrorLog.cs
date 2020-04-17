namespace StaffPortal.Common
{
    public class ErrorLog : BaseEntity
    {
        public string Message { get; set; }
        public string InnerException { get; set; }

        public ErrorLog()
        { }

        public ErrorLog(string message, string innerException)
        {
            this.Message = message;
            this.InnerException = innerException;
        }        
    }
}
