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
    public class DepartmentService : IDepartmentService
    {
        private readonly IRepository<Department> _repository;
        private readonly IMapper _mapper;
        public DepartmentService(IRepository<Department> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public PayloadResponse Create(DepartmentVm model)
        {
            var isExists = IsExists(model);
            if (isExists)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    PayloadType = "Department Creation",
                    Content = null,
                    Message = "Error !!! department already registered."
                };
            }

            try
            {
                var entity = new Department();
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
                    PayloadType = "Department Creation",
                    Content = null,
                    Message = "Department Creation has been successfull"
                };
            }
            catch (Exception ex)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    PayloadType = "Department Creation",
                    Content = null,
                    Message = $"Department Creation become unsuccessfull! Error:-{ex.Message} "
                };
            }
        }

        private bool IsExists(DepartmentVm model)
        {
            var data = _repository.GetConditional(_ => _.Name == model.Name);
            return data != null;
        }

        public IEnumerable<DepartmentVm> GetAll()
        {
            var list = new List<DepartmentVm>();
            var data = _repository.GetAll();
            list = _mapper.Map(data, list);

            return list.AsEnumerable();
        }

        public PayloadResponse Update(string id, DepartmentVm model)
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
                    PayloadType = "Department Update",
                    Content = null,
                    Message = "Department Update successfull"
                };
            }
            catch (Exception ex)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    PayloadType = "Department Update",
                    Content = null,
                    Message = $"Department Update become unsuccessfull! Error:-{ex.Message}"
                };
            }
        }

        public DepartmentVm GetById(string id)
        {
            var model = new DepartmentVm();
            var enity = _repository.GetConditional(_ => _.Id == id);
            model = _mapper.Map(enity, model);

            return model;
        }
    }
}