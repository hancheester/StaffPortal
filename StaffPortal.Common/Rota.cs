using System;

namespace StaffPortal.Common
{
    public class Rota
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; }
        public string Day { get; set; }
        // Format: Start time, End time, Is on leave?
        public Tuple<string, string, bool> Shift { get; set; }
    }
}
