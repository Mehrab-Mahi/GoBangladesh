using System;

namespace GoBangladesh.Application.ViewModels;

public class NewsVm
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Url { get; set; }
    public DateTime PublishDate { get; set; }
}