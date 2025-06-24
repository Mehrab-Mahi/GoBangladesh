using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GoBangladesh.Application.Services
{
    public class AssetTypeService : IAssetTypeService
    {
        private readonly IRepository<AssetType> _repository;
        private readonly IMapper _mapper;

        public AssetTypeService(IRepository<AssetType> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public PayloadResponse Create(AssetTypeVm model)
        {
            var isExists = IsExists(model);
            if (isExists)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    PayloadType = "Asset Type Creation",
                    Content = null,
                    Message = "Error !!! Asset Type already registered."
                };
            }

            try
            {
                var entity = new AssetType();
                entity = _mapper.Map(model, entity);
                if (string.IsNullOrEmpty(entity.Id))
                {
                    entity.Id = Guid.NewGuid().ToString("N");
                }

                _repository.Insert(entity);
                _repository.SaveChanges();

                return new PayloadResponse
                {
                    IsSuccess = true,
                    PayloadType = "Asset Type Creation",
                    Content = null,
                    Message = "Asset Type Creation has been successfull"
                };
            }
            catch (Exception ex)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    PayloadType = "Asset Type Creation",
                    Content = null,
                    Message = $"Asset Type Creation become unsuccessfull! Error:-{ex.Message} "
                };
            }
        }

        private bool IsExists(AssetTypeVm model)
        {
            var data = _repository.GetConditional(_ => _.Name == model.Name);
            return data != null;
        }

        public IEnumerable<AssetTypeVm> GetAll()
        {
            var list = new List<AssetTypeVm>();
            var data = _repository.GetAll();
            list = _mapper.Map(data, list);

            return list.AsEnumerable();
        }

        public AssetTypeVm GetById(string id)
        {
            var model = new AssetTypeVm();
            var enity = _repository.GetConditional(_ => _.Id == id);
            model = _mapper.Map(enity, model);

            return model;
        }

        public PayloadResponse Update(string id, AssetTypeVm model)
        {
            var entity = _repository.GetConditional(_ => _.Id == id);
            try
            {
                entity = _mapper.Map(model, entity);

                _repository.Update(entity);
                _repository.SaveChanges();

                return new PayloadResponse
                {
                    IsSuccess = true,
                    PayloadType = "Asset Type Update",
                    Content = null,
                    Message = "Asset Type Update successfull"
                };
            }
            catch (Exception ex)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    PayloadType = "Asset Type Update",
                    Content = null,
                    Message = $"Asset Type Update become unsuccessfull! Error:-{ex.Message}"
                };
            }
        }
    }
}