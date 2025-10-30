namespace ERP.ER;

public class Department
{
    public int Id { get; }
    public string Name { get; }
    public decimal Budget { get; set; }
    public Department(int id, string name, decimal budget)
    { Id = id; Name = name; Budget = budget; }
    
}