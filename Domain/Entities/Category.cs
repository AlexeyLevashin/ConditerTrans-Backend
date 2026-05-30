using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("categories")]
public class Category
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("name")]
    public string Name { get; set; }
}