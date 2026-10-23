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
        if (string.IsNullOrWhiteSpace(name))
        {
            var failureResult = Result<Position>.Fail("Position name cannot be empty");

            return failureResult;
        }

        var normalizedName = name.Trim();

        if (normalizedName.Length > 150)
        {
            var failureResult = Result<Position>.Fail("Position name cannot exceed 150 characters");

            return failureResult;
        }

        var position = new Position(
            id: PositionId.New(),
            name: normalizedName
        );

        var successResult = Result<Position>.Ok(position);

        return successResult;
    }
}