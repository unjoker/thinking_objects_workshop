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
        var dept = new Budget(owner: "R&D", budget: 300_000m);
        var candidate = new Candidate(name: "Ava Thompson", email: "ava@acme.com", annualSalary: 100_000m);
        
        var employee = hr.Hire(candidate, dept);
        
        Assert.NotNull(employee);
        Assert.Equal("ava@acme.com", employee.Email);
        Assert.True(accounting.PayrollAdded);
        Assert.True(it.AccountCreated);
        Assert.Equal(200_000m, dept.Available);
    }

    [Fact]
    public void NotEnoughBudget()
    {
        var accounting = new AccountingDept();
        var it = new ITDept();
        var hr = new HRDept(accounting, it);
        var dept = new Budget(owner: "R&D", budget: 50_000m);
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

    [Fact]
    public void ForProjectFundedBy2Depts()
    {
        var accounting = new AccountingDept();
        var it = new ITDept();
        var hr = new HRDept(accounting, it);
        var budget1 = new Budget(owner: "R&D", budget: 300_000m);
        var budget2 = new Budget(owner: "Marketing", budget: 50_000m);
        var project = new SharedBudget(owner: "Organic 3D printer", [budget1, budget2]);
        
        var candidate = new Candidate(name: "Ava Thompson", email: "ava@acme.com", annualSalary: 50_000m);
        
        var employee = hr.Hire(candidate, project);
        
        Assert.NotNull(employee);
        Assert.Equal("ava@acme.com", employee.Email);
        Assert.True(accounting.PayrollAdded);
        Assert.True(it.AccountCreated);
        Assert.Equal(300_000m, project.Available);
        
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

    public Employee Hire(Candidate candidate, IBudget sponsor)
    {
        if (!sponsor.CanAfford(candidate.AnnualSalary))
            throw new InvalidOperationException("Not enough budget.");

        var employee = candidate.HireFor(sponsor);
        
        _it.Onboard(employee);
        _accounting.AddToPayroll(employee);
        sponsor.Spend(employee.Salary);
        
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

    public Employee HireFor(IBudget sponsor)
    {
        Validate();
        return new Employee(Name, AnnualSalary, Email, sponsor.Owner);
    }
}

public interface IBudget
{
    string Owner { get; }
    decimal Available { get; }
    bool CanAfford(decimal amount);
    void Spend(decimal amount);
}

public class Budget : IBudget
{
    public Budget(string owner, decimal budget)
    {
        Owner = owner;
        Available = budget;
    }

    public string Owner { get; }
    public decimal Available { get; private set; }

    public bool CanAfford(decimal amount)
    {
        return amount <= Available;
    }

    public void Spend(decimal amount)
    {
        Available -= amount;
    }
}

public class SharedBudget: IBudget
{
    Budget[] _budgets;

    public SharedBudget(string owner, Budget[] budgets)
    {
        Owner =  owner;
        _budgets = budgets;
    }

    public string Owner { get; }
    public decimal Available => _budgets.Sum(b => b.Available);
    public bool CanAfford(decimal amount)
    {
        var total = _budgets.Sum(b => b.Available);
        return total >= amount;
    }

    public void Spend(decimal amount)
    {
        var charge = amount / _budgets.Length;
        foreach (var budget in _budgets)
        {
            budget.Spend(charge);
        }
    }
}

public class Employee
{
    public Employee(string name, decimal salary, string email, string sponsor)
    {
        Name = name;
        Salary = salary;
        Email = email;
        Sponsor = sponsor;
    }
    
    public string Name { get;}
    public decimal Salary { get; }
    public string Email { get; }
    public string Sponsor { get; }
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