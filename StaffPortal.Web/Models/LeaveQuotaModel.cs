namespace StaffPortal.Web.Models
{
    public class LeaveQuotaModel
    {
        public decimal Total { get; set; }
        public decimal Used { get; set; }
        public decimal Remaining { get; set; }
        public decimal AccruedAsDay { get; set; }
        public decimal AccruedAsPay { get; set; }
        public decimal Pending { get; set; }
        public decimal Unused { get; set; }
        public decimal NoImpact { get; set; }
    }
}