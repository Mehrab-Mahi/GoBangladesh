using System;

namespace GoBangladesh.Application.ViewModels
{
    public class DesignationVm
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Responsibilities { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime LastModifiedTime { get; set; }
        public string CreatedBy { get; set; }
        public string LastModifiedBy { get; set; }
    }
}