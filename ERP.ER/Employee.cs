namespace ERP.ER;

public class Employee
{
    public int Id { get; }
    public string Name { get; }
    public string Email { get; }
    public decimal AnnualSalary { get; }
    public string DepartmentName { get; }
    public Employee(int id, string name, string email, decimal annualSalary, string departmentName)
    { Id = id; Name = name; Email = email; AnnualSalary = annualSalary; DepartmentName = departmentName; }
}