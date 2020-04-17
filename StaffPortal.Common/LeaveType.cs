namespace StaffPortal.Common
{
    public class LeaveType : BaseEntity
    {
        public string Name { get; set; }
        public bool Requestable { get; set; }
        public bool Payable { get; set; }
        public bool Accruable { get; set; }
        public bool ImpactOnAllowance { get; set; }
        public bool IsLocked { get; set; }
    }
}
