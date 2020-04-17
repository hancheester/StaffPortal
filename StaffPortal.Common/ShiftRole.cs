namespace StaffPortal.Common
{
    public class ShiftRole
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsRequired { get; set; }
        public int MinimumRequired { get; set; }
    }
}
