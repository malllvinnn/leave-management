using LeaveManagement.Domain.Common;

namespace LeaveManagement.Domain.ValueObjects;

public readonly record struct PositionId
{
    private const string EmptyValueError = "Position ID cannot be empty";
    public Guid Value { get; }

    private PositionId(Guid value)
    {
        Value = value;
    }

    public static PositionId New()
    {
        var value = Guid.CreateVersion7();
        var positionId = new PositionId(value);

        return positionId;
    }

    public static Result<PositionId> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            var failureResult = Result<PositionId>.Fail(EmptyValueError);

            return failureResult;
        }

        var positionId = new PositionId(value);
        var successResult = Result<PositionId>.Ok(positionId);

        return successResult;
    }

    public override string ToString()
    {
        var result = Value.ToString();

        return result;
    }
}