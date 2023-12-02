namespace AzureTaskManagement.Database.Models;

public class TaskModel
{
    public Guid Id { get; set; }
    public TenantModel Tenant { get; set; }
    public List<AttachmentModel> Attachments { get; set; }
    public string Name { get; set; }
}