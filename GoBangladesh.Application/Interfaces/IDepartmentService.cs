using GoBangladesh.Application.ViewModels;
using System.Collections.Generic;

namespace GoBangladesh.Application.Interfaces
{
    public interface IDepartmentService
    {
        IEnumerable<DepartmentVm> GetAll();
        PayloadResponse Create(DepartmentVm model);
        PayloadResponse Update(string id, DepartmentVm model);
        DepartmentVm GetById(string id);
    }
}