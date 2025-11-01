using Xunit;

namespace ERP.Domain.HR;

public class UseCase_Hire
{
    [Fact]
    public void HasEnoughBudget()
    {
        var accounting = new AccountingDept();
        var it = new ITDept();
        var hr = new HRDept(accounting, it);
        var dept = new Dept(name: "R&D", budget: 300_000m);
        var candidate = new Candidate(name: "Ava Thompson", email: "ava@acme.com", annualSalary: 100_000m);
        
        var employee = hr.Hire(candidate, dept);
        
        Assert.NotNull(employee);
        Assert.Equal("ava@acme.com", employee.Email);
        Assert.True(accounting.PayrollAdded);
        Assert.True(it.AccountCreated);
        Assert.Equal(200_000m, dept.Budget);
    }

    [Fact]
    public void NotEnoughBudget()
    {
        var accounting = new AccountingDept();
        var it = new ITDept();
        var hr = new HRDept(accounting, it);
        var dept = new Dept(name: "R&D", budget: 50_000m);
        var candidate = new Candidate(name: "Ava Thompson", email: "ava@acme.com", annualSalary: 100_000m);
        
        Assert.Throws<InvalidOperationException>(()=> hr.Hire(candidate, dept));
    }

    [Fact]
    public void InvalidEmail()
    {
        var candidate = new Candidate(name: "Ava Thompson", email: "", annualSalary: 100_000m);
        
        var error = Assert.Throws<ApplicationException>(()=> candidate.Validate());
        Assert.Equal("Invalid Email", error.Message);
    }
    
    [Fact]
    public void InvalidName()
    {
        var candidate = new Candidate(name: "", email: "ava@acme.com", annualSalary: 100_000m);
        
        var error = Assert.Throws<ApplicationException>(()=> candidate.Validate());
        Assert.Equal("Name must be set", error.Message);
    }
    
    [Fact]
    public void InvalidSalary()
    {
        var candidate = new Candidate(name: "Ava Thompson", email: "ava@acme.com", annualSalary: 0m);
        
        var error = Assert.Throws<ApplicationException>(()=> candidate.Validate());
        Assert.Equal("AnnualSalary must be greater than 0", error.Message);
    }

   
}

public class HRDept
{
    private readonly AccountingDept _accounting;
    private readonly ITDept _it;

    public HRDept(AccountingDept accounting, ITDept it)
    {
        _accounting = accounting;
        _it = it;
    }

    public Employee Hire(Candidate candidate, Dept dept)
    {
        candidate.Validate();
        
        if (!dept.CanAfford(candidate.AnnualSalary))
            throw new InvalidOperationException("Not enough budget.");

        var employee = candidate.HireFor(dept);
        
        _it.Onboard(employee);
        _accounting.AddToPayroll(employee);
        dept.Spend(candidate.AnnualSalary);
        return employee;
    }
}

public class Candidate
{
    public Candidate(string name, string email, decimal annualSalary)
    {
        Name = name;
        Email = email;
        AnnualSalary = annualSalary;
    }
    
    public string Name { get; }
    public string Email { get; }
    public decimal AnnualSalary { get; }

    public void Validate()
    {
        if (AnnualSalary <= 0)
            throw new ApplicationException("AnnualSalary must be greater than 0");
        
        if (string.IsNullOrWhiteSpace(Name))
            throw new ApplicationException("Name must be set");
        
        if (string.IsNullOrWhiteSpace(Email))
            throw new ApplicationException("Invalid Email");
            
    }

    public Employee HireFor(Dept dept)
    {
        Validate();
        return new Employee(Name, AnnualSalary, Email, dept.Name);
    }
}

public class Dept
{
    public Dept(string name, decimal budget)
    {
        Name = name;
        Budget = budget;
    }

    public string Name { get; }
    public decimal Budget { get; private set; }

    public bool CanAfford(decimal amount)
    {
        return amount <= Budget;
    }

    public void Spend(decimal amount)
    {
        Budget -= amount;
    }
}

public class Employee
{
    public Employee(string name, decimal salary, string email, string dept)
    {
        Name = name;
        Salary = salary;
        Email = email;
        Dept = dept;
    }
    
    public string Name { get;}
    public decimal Salary { get; }
    public string Email { get; }
    public string Dept { get; }
}

public class ITDept
{
    public void Onboard(Employee employee)
    {
        AccountCreated = true;
    }

    public bool AccountCreated { get; private set; }
}

public class AccountingDept
{
    public void AddToPayroll(Employee employee)
    {
        PayrollAdded = true;
    }

    public bool PayrollAdded { get; private set; }
}