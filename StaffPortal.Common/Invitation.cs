using System;

namespace StaffPortal.Common
{
    public class Invitation : BaseEntity
    {
        public int TrainingModuleId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime DateCreated { get; set; }

        // Request Status Enum
        public int StatusCode { get; set; }

        public Invitation()
        { }

        public Invitation(int employeeId, int moduleId)
        {
            this.TrainingModuleId = moduleId;
            this.EmployeeId = employeeId;
            this.DateCreated = DateTime.Now;
            this.StatusCode = 0;
        }
    }
}