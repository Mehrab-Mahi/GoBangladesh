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
    public class DesignationService : IDesignationService
    {
        private readonly IRepository<Designation> _repository;
        private readonly IMapper _mapper;

        public DesignationService(IRepository<Designation> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public IEnumerable<DesignationVm> GetAll()
        {
            var list = new List<DesignationVm>();
            var data = _repository.GetAll();
            list = _mapper.Map(data, list);

            return list.AsEnumerable();
        }

        public DesignationVm GetById(string id)
        {
            var model = new DesignationVm();
            var enity = _repository.GetConditional(_ => _.Id == id);
            model = _mapper.Map(enity, model);

            return model;
        }

        public PayloadResponse Create(DesignationVm model)
        {
            var isExists = IsExists(model);
            if (isExists)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    PayloadType = "Designation Creation",
                    Content = null,
                    Message = "Error !!! Designation already registered."
                };
            }

            try
            {
                var entity = new Designation();
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
                    PayloadType = "Designation Creation",
                    Content = null,
                    Message = "Designation Creation has been successfull"
                };
            }
            catch (Exception ex)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    PayloadType = "Designation Creation",
                    Content = null,
                    Message = $"Designation Creation become unsuccessfull! Error:-{ex.Message} "
                };
            }
        }

        private bool IsExists(DesignationVm model)
        {
            var data = _repository.GetConditional(_ => _.Name == model.Name);
            return data != null;
        }

        public PayloadResponse Update(string id, DesignationVm model)
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
                    PayloadType = "Designation Update",
                    Content = null,
                    Message = "Designation Update successfull"
                };
            }
            catch (Exception ex)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    PayloadType = "Designation Update",
                    Content = null,
                    Message = $"Designation Update become unsuccessfull! Error:-{ex.Message}"
                };
            }
        }
    }
}