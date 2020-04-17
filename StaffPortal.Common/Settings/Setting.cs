namespace StaffPortal.Common.Settings
{
    public class Setting : BaseEntity
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public int OrganizationId { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
}
