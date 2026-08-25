using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;

namespace LeaveManagement.Domain.Services;

public sealed class RoleResolver
{
    public SystemRole Resolve(Employee employee, int activeSubordinateCount)
    {
        ArgumentNullException.ThrowIfNull(employee);
        ArgumentOutOfRangeException.ThrowIfNegative(activeSubordinateCount);

        throw new NotImplementedException();
    }
}