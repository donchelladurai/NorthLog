namespace NorthLog.Domain.Entities;

public class Operator
{
    public Operator(string name) => Name = name;
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = default!;
}