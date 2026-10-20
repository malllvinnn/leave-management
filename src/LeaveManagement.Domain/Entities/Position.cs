using LeaveManagement.Domain.Common;
using LeaveManagement.Domain.ValueObjects;

namespace LeaveManagement.Domain.Entities;

public class Position
{
    public PositionId Id { get; private set; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; }

    private Position(
        PositionId id,
        string name
    )
    {
        Id = id;
        Name = name;
        IsActive = true;
    }

    public static Result<Position> Create(string? name)
    {
        throw new NotImplementedException();
    }
}