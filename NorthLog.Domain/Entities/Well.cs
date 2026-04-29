namespace NorthLog.Domain.Entities;

public class Well
{
    public Well(string name, Guid fieldId) => (Name, FieldId) = (name, fieldId);

    public Wellbore AddWellbore(string name, decimal kickoffDepthMeters)
    {
        var bore = new Wellbore(this.Id, name, kickoffDepthMeters);
        _wellbores.Add(bore);
        return bore;
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = default!;
    public Guid FieldId { get; private set; }
    public Field Field { get; private set; } = default!;

    private readonly List<Wellbore> _wellbores = [];
    public IReadOnlyCollection<Wellbore> Wellbores => _wellbores.AsReadOnly();
}