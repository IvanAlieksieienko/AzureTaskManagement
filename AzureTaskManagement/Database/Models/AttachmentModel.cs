namespace AzureTaskManagement.Database.Models;

public class AttachmentModel
{
    public string FileName { get; set; }
    public TaskModel Task { get; set; }
    public string FixedFileName { get; set; }
}