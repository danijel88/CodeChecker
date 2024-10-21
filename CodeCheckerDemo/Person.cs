namespace CodeCheckerDemo;

public class Person
{
    public int Id { get; set; }
    public string? Name { get; set; }

    public void AddPerson(int id , string name)
    {
        Id = id;
        Name = name;
    }
}