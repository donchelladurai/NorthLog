
namespace NorthLog.Application.Featrures.Wellbores
{
    public record WellboreListItem
    {
        public WellboreListItem(Guid Id,
            string DisplayName,
            string FieldName,
            string OperatorName,
            string Status)
        {
            this.Id = Id;
            this.DisplayName = DisplayName;
            this.FieldName = FieldName;
            this.OperatorName = OperatorName;
            this.Status = Status;
        }

        public Guid Id { get; init; }
        public string DisplayName { get; init; }
        public string FieldName { get; init; }
        public string OperatorName { get; init; }
        public string Status { get; init; }
    }
}
