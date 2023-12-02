using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AzureTaskManagement.Database.Models;

public class TaskModel
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
}