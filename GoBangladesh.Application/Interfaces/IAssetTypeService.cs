using GoBangladesh.Application.ViewModels;
using System.Collections.Generic;

namespace GoBangladesh.Application.Interfaces
{
    public interface IAssetTypeService
    {
        IEnumerable<AssetTypeVm> GetAll();

        AssetTypeVm GetById(string id);

        PayloadResponse Create(AssetTypeVm model);

        PayloadResponse Update(string id, AssetTypeVm model);
    }
}