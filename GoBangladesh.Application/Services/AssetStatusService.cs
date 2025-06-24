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
    public class AssetStatusService : IAssetStatusService
    {
        private readonly IRepository<AssetStatus> _repository;
        private readonly IMapper _mapper;
        public AssetStatusService(IRepository<AssetStatus> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public PayloadResponse Create(AssetStatusVm model)
        {
            var isExists = IsExists(model);
            if (isExists)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    PayloadType = "Asset Status Creation",
                    Content = null,
                    Message = "Error !!! Asset Status already registered."
                };
            }

            try
            {
                var entity = new AssetStatus();
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
                    PayloadType = "Asset Status Creation",
                    Content = null,
                    Message = "Asset Status Creation has been successfull"
                };
            }
            catch (Exception ex)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    PayloadType = "Asset Status Creation",
                    Content = null,
                    Message = $"Asset Status Creation become unsuccessfull! Error:-{ex.Message} "
                };
            }
        }

        private bool IsExists(AssetStatusVm model)
        {
            var data = _repository.GetConditional(_ => _.Name == model.Name);
            return data != null;
        }

        public IEnumerable<AssetStatusVm> GetAll()
        {
            var list = new List<AssetStatusVm>();
            var data = _repository.GetAll();
            list = _mapper.Map(data, list);

            return list.AsEnumerable();
        }

        public AssetStatusVm GetById(string id)
        {
            var model = new AssetStatusVm();
            var enity = _repository.GetConditional(_ => _.Id == id);
            model = _mapper.Map(enity, model);

            return model;
        }

        public PayloadResponse Update(string id, AssetStatusVm model)
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
                    PayloadType = "Asset Status Update",
                    Content = null,
                    Message = "Asset Status Update successfull"
                };
            }
            catch (Exception ex)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    PayloadType = "Asset Status Update",
                    Content = null,
                    Message = $"Asset Status Update become unsuccessfull! Error:-{ex.Message}"
                };
            }
        }
    }
}