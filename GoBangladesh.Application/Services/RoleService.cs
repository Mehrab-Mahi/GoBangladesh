using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GoBangladesh.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRepository<Role> _roleRepo;
        private readonly IRepository<AccessControl> _accessRepo;
        private readonly IRepository<MenuCrud> _menuRepo;

        public RoleService(IRepository<Role> roleRepo, IRepository<AccessControl> accessRepo, IRepository<MenuCrud> menuRepo)
        {
            _roleRepo = roleRepo;
            _accessRepo = accessRepo;
            _menuRepo = menuRepo;
        }

        public bool CreateRole(RoleVm roleVm)
        {
            try
            {
                var role = new Role()
                {
                    Name = roleVm.Name
                };
                _roleRepo.Insert(role);
                _roleRepo.SaveChanges();

                foreach (var accessId in roleVm.AccessControlIds)
                {
                    var menu = new MenuCrud()
                    {
                        RoleId = role.Id,
                        AccessControlId = accessId
                    };
                    _menuRepo.Insert(menu);
                }
                _menuRepo.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<Role> GetAll()
        {
            return _roleRepo.GetAll().ToList();
        }

        public List<MenuTree> GetMenuTreeData()
        {
            return _accessRepo.GetAll().OrderBy(_ => _.SortOrder).Select(x => new MenuTree()
            {
                Id = x.Id,
                Parent = x.ParentId,
                Text = x.Name
            }).ToList();
        }

        public Role GetRole(string id)
        {
            return _roleRepo.Find(id);
        }

        public List<string> GetRoleMenuIds(string id)
        {
            return _menuRepo.GetConditionalList(_ => _.RoleId == id).Select(s => s.AccessControlId).ToList();
        }

        public bool UpdateRole(RoleVm roleVm)
        {
            try
            {
                var role = new Role { Name = roleVm.Name, Id = roleVm.Id };
                _roleRepo.Update(role);
                _roleRepo.SaveChanges();

                var menuCruds = _menuRepo.GetConditionalList(_ => _.RoleId == roleVm.Id).ToList();
                _menuRepo.Delete(menuCruds);
                _menuRepo.SaveChanges();

                foreach (var r in roleVm.AccessControlIds)
                {
                    var menu = new MenuCrud()
                    {
                        RoleId = roleVm.Id,
                        AccessControlId = r
                    };
                    _menuRepo.Insert(menu);
                }
                _menuRepo.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}