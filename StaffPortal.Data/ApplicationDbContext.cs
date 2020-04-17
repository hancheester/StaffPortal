using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StaffPortal.Common;
using StaffPortal.Common.Settings;

namespace StaffPortal.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);         
        }

        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Permission_MenuItem> Permission_MenuItems { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<BusinessRole> BusinessRoles { get; set; }
        public DbSet<BusinessRole_Permission> BusinessRole_Permissions { get; set; }
        public DbSet<Employee_BusinessRole> Employee_BusinessRoles { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<OpeningHour> OpeningHours { get; set; }
        public DbSet<BusinessRole_Department> BusinessRole_Departments { get; set; }
        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<WorkingDay> WorkingDays { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<RequestedDate> RequestedDates { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }
        public DbSet<RejectionReason> RejectionReasons { get; set; }
        public DbSet<AuditTrail> AuditTrail { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<TrainingModule> TrainingModules { get; set; }
        public DbSet<Invitation> Invitations { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Recipient> Recipients { get; set; }
        public DbSet<TimeclockTimestamp> TimeClockTimestamps { get; set; }
        public DbSet<LastTimestamp_Employee> LastTimestamp_Employee_Registry { get; set; }
        public DbSet<Setting> Setting { get; set; }
    }
}