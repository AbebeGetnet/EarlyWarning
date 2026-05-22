namespace EarlyWarning.ViewModels.GrainViewModel
{
    public class MigrationDetailViewModel
    {
        public Guid OriginLocationId { get; set; }
        public string? MigrationReason { get; set; }
        public int MaleHouseholdHeads { get; set; }
        public int FemaleHouseholdHeads { get; set; }
        public int MaleFamilyMembers { get; set; }
        public int FemaleFamilyMembers { get; set; }
        public int MaleChildren { get; set; }
        public int FemaleChildren { get; set; }
        public int MaleYouth { get; set; }
        public int FemaleYouth { get; set; }
        public int MaleElderly { get; set; }
        public int FemaleElderly { get; set; }
        public int MaleDisabled { get; set; }
        public int FemaleDisabled { get; set; }
        public string? Notes { get; set; }
    }
}
