using System;

namespace StaffPortal.Common.Models
{
    // This class is meant to represent either a DaysWorking instance or an Assignment instance
    public class Allocation
    {
        public int Id { get; set; }
        public bool IsAssignment { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public bool IsAssigned { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string Day { get; set; }
        public int EmployeeId { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public DateTime Date { get; set; }
        public bool IsOnHoliday { get; set; }

        public Allocation()
        {

        }

        public Allocation(Assignment assignment, DateTime date)
        {
            this.Id = assignment.Id;
            this.IsAssignment = true;
            this.CreatedDate = assignment.CreatedOnDate;
            this.StartDate = assignment.StartDate;
            this.EndDate = assignment.EndDate;
            this.DepartmentId = assignment.DepartmentId;
            this.IsAssigned = true;
            this.StartTime = assignment.StartTime;
            this.EndTime = assignment.EndTime;
            this.Day = assignment.Day;
            this.EmployeeId = assignment.EmployeeId;
            this.RoleId = assignment.BusinessRoleId;
            this.Date = date;
        }

        public Allocation(WorkingDay daysWorking, DateTime date)
        {
            this.Id = daysWorking.Id;
            this.DepartmentId = daysWorking.DepartmentId;
            this.IsAssigned = daysWorking.IsAssigned;
            this.StartTime = daysWorking.StartTime;
            this.EndTime = daysWorking.EndTime;
            this.Day = daysWorking.Day;
            this.EmployeeId = daysWorking.EmployeeId;
            this.Date = date;
        }
    }
}
