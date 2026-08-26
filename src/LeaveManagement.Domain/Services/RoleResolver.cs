using LeaveManagement.Domain.Entities;
using LeaveManagement.Domain.Enums;

namespace LeaveManagement.Domain.Services;

public sealed class RoleResolver
{
    public SystemRole Resolve(Employee employee, int activeSubordinateCount)
    {
        ArgumentNullException.ThrowIfNull(employee);
        ArgumentOutOfRangeException.ThrowIfNegative(activeSubordinateCount);

        SystemRole resolveResult;

        if (activeSubordinateCount >= 1)
        {
            resolveResult = SystemRole.SuperEmployee;
        }
        else
        {
            resolveResult = SystemRole.Employee;
        }

        employee.ApplyResolvedRole(resolveResult);

        return resolveResult;
    }
}