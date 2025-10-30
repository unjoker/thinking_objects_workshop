using Xunit;

namespace ERP.ER;

public class DepartmentServiceTests
{
    [Fact]
    public void Success_DebitsBudget_And_AssignsDepartment()
    {
        var dept = new Department(1, "HR", 200_000m);
        var svc = new DepartmentService();
        var emp = svc.HireEmployee(dept, "Ava", "ava@corp.com", 100_000m);
        Assert.Equal("HR", emp.DepartmentName);
        Assert.Equal(100_000m, dept.Budget); // debited up-front
    }


    [Fact]
    public void Fails_When_InsufficientBudget()
    {
        var dept = new Department(1, "HR", 80_000m);
        var svc = new DepartmentService();
        Assert.Throws<InvalidOperationException>(() => svc.HireEmployee(dept, "Ben", "ben@corp.com", 100_000m));
    }
}