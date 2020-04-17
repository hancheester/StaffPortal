using System.Collections.Generic;
using System.Text;

namespace StaffPortal.Common.Models
{
    public class OperationResult<T> : OperationResult
    {
        public T Object { get; set; }
       
        public OperationResult()
        {
            this.Succeeded = true;
        }

        public OperationResult(bool succeeded)
        {
            this.Succeeded = succeeded;
        }

        public OperationResult(T obj)
        {
            if (obj == null)
                this.Succeeded = false;
            else
                this.Succeeded = true;

            this.Object = obj;
            this.ErrorMessages = new Dictionary<string, string>();
        }

        public OperationResult(T el, bool succeeded)
        {
            this.Succeeded = succeeded;
            this.Object = el;
            this.ErrorMessages = new Dictionary<string, string>();
        }
    }

    public class OperationResult
    {
        public bool Succeeded { get; set; } = true;
        public Dictionary<string, string> ErrorMessages { get; set; } = new Dictionary<string, string>();
        public string ErrorSummary
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (var error in ErrorMessages)
                {
                    sb.AppendLine(error.Value);
                }

                return sb.ToString();
            }
        }

        public OperationResult()
        {

        }

        public OperationResult(bool succeeded)
        {
            this.Succeeded = succeeded;
        }

        public void AddOperationError(string errorCode, string message)
        {
            this.ErrorMessages.Add(errorCode, message);
            this.Succeeded = false;
        }        
    }
}