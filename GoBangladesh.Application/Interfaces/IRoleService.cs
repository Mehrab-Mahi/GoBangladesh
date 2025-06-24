using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using System.Collections.Generic;

namespace GoBangladesh.Application.Interfaces
{
    public interface IRoleService
    {
        List<Role> GetAll();

        List<MenuTree> GetMenuTreeData();

        bool CreateRole(RoleVm roleVm);

        bool UpdateRole(RoleVm roleVm);

        List<string> GetRoleMenuIds(string id);

        Role GetRole(string id);
    }
}