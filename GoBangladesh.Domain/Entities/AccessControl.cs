using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoBangladesh.Domain.Entities
{
    public class AccessControl : Entity
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string ParentId { get; set; }
        public string Url { get; set; }
        public string MenuId { get; set; }
        public string Icon { get; set; }
        public int SortOrder { get; set; }
    }
}
