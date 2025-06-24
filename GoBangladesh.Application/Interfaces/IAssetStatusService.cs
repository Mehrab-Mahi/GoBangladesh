using GoBangladesh.Application.ViewModels;
using System.Collections.Generic;

namespace GoBangladesh.Application.Interfaces
{
    public interface IAssetStatusService
    {
        IEnumerable<AssetStatusVm> GetAll();
        AssetStatusVm GetById(string id);
        PayloadResponse Create(AssetStatusVm model);
        PayloadResponse Update(string id, AssetStatusVm model);
    }
}