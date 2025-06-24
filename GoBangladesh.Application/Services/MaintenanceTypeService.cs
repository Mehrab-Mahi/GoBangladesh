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
    public class MaintenanceTypeService : IMaintenanceTypeService
    {
        private readonly IRepository<MaintenanceType> _repository;
        private readonly IMapper _mapper;

        public MaintenanceTypeService(IRepository<MaintenanceType> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public PayloadResponse Create(MaintenanceTypeVm model)
        {
            var isExists = IsExists(model);
            if (isExists)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    PayloadType = "Maintenance Type Creation",
                    Content = null,
                    Message = "Error !!! Maintenance Type already registered."
                };
            }

            try
            {
                var entity = new MaintenanceType();
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
                    PayloadType = "Maintenance Type Creation",
                    Content = null,
                    Message = "Maintenance Type Creation has been successfull"
                };
            }
            catch (Exception ex)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    PayloadType = "Maintenance Type Creation",
                    Content = null,
                    Message = $"Maintenance Type Creation become unsuccessfull! Error:-{ex.Message} "
                };
            }
        }

        private bool IsExists(MaintenanceTypeVm model)
        {
            var data = _repository.GetConditional(_ => _.Name == model.Name);
            return data != null;
        }

        public IEnumerable<MaintenanceTypeVm> GetAll()
        {
            var list = new List<MaintenanceTypeVm>();
            var data = _repository.GetAll();
            list = _mapper.Map(data, list);

            return list.AsEnumerable();
        }

        public MaintenanceTypeVm GetById(string id)
        {
            var model = new MaintenanceTypeVm();
            var enity = _repository.GetConditional(_ => _.Id == id);
            model = _mapper.Map(enity, model);

            return model;
        }

        public PayloadResponse Update(string id, MaintenanceTypeVm model)
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
                    PayloadType = "Maintenance Type Update",
                    Content = null,
                    Message = "Maintenance Type Update successfull"
                };
            }
            catch (Exception ex)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    PayloadType = "Maintenance Type Update",
                    Content = null,
                    Message = $"Maintenance Type Update become unsuccessfull! Error:-{ex.Message}"
                };
            }
        }
    }
}