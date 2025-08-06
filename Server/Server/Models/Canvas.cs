using Server.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Canvas
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    public List<DrawingInCanvas> Drawings { get; set; } = new();
}
