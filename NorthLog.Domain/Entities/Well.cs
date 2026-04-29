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

    // To the interviewers: private modifier here so wellbores can only be added through the AddWellbore() method, which enforces the domain rules around wellbore creation.
    // I've also configured EF Core to use the private backing field to read/write instead of the public property.
    public IReadOnlyCollection<Wellbore> Wellbores => _wellbores.AsReadOnly();
}