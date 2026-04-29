namespace NorthLog.Domain.Entities;

public class Field
{
    public Field(string name, string block, Guid operatorId)
        => (Name, Block, OperatorId) = (name, block, operatorId);

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = default!;
    public string Block { get; private set; } = default!;
    public Guid OperatorId { get; private set; }
    public Operator Operator { get; private set; } = default!;
}