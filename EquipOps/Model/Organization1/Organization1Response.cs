namespace EquipOps.Model.Organization
{
    public class Organization1Response
    {
        public int TotalNumbers { get; set; }
        public List<Organization1ResponseViewModel> organizationResponseViewModel { get; set; } = new List<Organization1ResponseViewModel>();
    }
}
