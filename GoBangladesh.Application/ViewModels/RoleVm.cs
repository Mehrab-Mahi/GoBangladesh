using GoBangladesh.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoBangladesh.Application.ViewModels
{
    public class RoleVm : Role
    {
        public List<string> AccessControlIds { get; set; }

        public RoleVm()
        {
            AccessControlIds = new List<string>();
        }

    }

    public class CreateUpdateDelete
    {
        public bool Create { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }

    }

    public class RoleCrudAction
    {
        public string Name { get; set; }
        public string YesNo { get; set; }
        public string AccessControlId { get; set; }
    }
}
