namespace ERP.ER;

public class DepartmentService
{
    public Employee HireEmployee(Department department, string name, string email, decimal annualSalary)
    {
        if (department.Budget < annualSalary)
            throw new InvalidOperationException("Insufficient budget");

        department.Budget -= annualSalary;
        onBoard(name, email);
        addToPayroll(name, annualSalary);
        
        return new Employee(id: 1, name, email, annualSalary, department.Name);
    }

    private void addToPayroll(string name, decimal annualSalary)
    {
    }

    private void onBoard(string name, string email)
    {
    }
}