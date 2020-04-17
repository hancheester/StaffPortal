using System;

namespace StaffPortal.Web.Models
{
    public class ClocktimeViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int MyProperty { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsClockIn { get; set; }

        public ClocktimeViewModel()
        {

        }

        public ClocktimeViewModel(string firstName, string lastName, DateTime timestamp, bool isClockIn)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Timestamp = timestamp;
            this.IsClockIn = isClockIn;
        }
    }
}
