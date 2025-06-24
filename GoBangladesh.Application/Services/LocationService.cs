using System;
using System.Collections.Generic;
using System.Linq;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;

namespace GoBangladesh.Application.Services
{
    public class LocationService : ILocationService
    {
        private readonly IRepository<Location> _locationRepository;

        public LocationService(IRepository<Location> locationRepository)
        {
            _locationRepository = locationRepository;
        }

        public List<Location> GetLocationByParentId(string parentId)
        {
            return _locationRepository
                .GetAll()
                .Where(l => l.ParentId == parentId)
                .OrderBy(l => l.Name)
                .ToList();
        }

        public PayloadResponse Create(LocationVm locationData)
        {
            try
            {
                var location = new Location()
                {
                    Name = locationData.Name,
                    ParentId = locationData.ParentId,
                    Code = locationData.Code,
                    Type = locationData.Type,
                    Level = locationData.Level
                };

                _locationRepository.Insert(location);
                _locationRepository.SaveChanges();

                return new PayloadResponse()
                {
                    IsSuccess = true,
                    Message = "Location created successfully"
                };
            }
            catch (Exception ex)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Message = $"Location created failed because {ex.Message}"
                };
            }
        }

        public PayloadResponse Update(LocationVm locationData)
        {
            try
            {
                var location = _locationRepository.GetConditional(l => l.Id == locationData.Id);

                location.Name = locationData.Name;
                location.ParentId = locationData.ParentId;
                location.Code = locationData.Code;
                location.Type = locationData.Type;
                location.Level = locationData.Level;

                _locationRepository.Update(location);
                _locationRepository.SaveChanges();

                return new PayloadResponse()
                {
                    IsSuccess = true,
                    Message = "Location updated successfully"
                };
            }
            catch (Exception ex)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Message = $"Location updated failed because {ex.Message}"
                };
            }
        }

        public PayloadResponse Delete(string id)
        {
            try
            {
                var location = _locationRepository.GetConditional(l => l.Id == id);

                if (location is null)
                {
                    return new PayloadResponse()
                    {
                        IsSuccess = false,
                        Message = "Location not found"
                    };
                }

                _locationRepository.Delete(location);
                _locationRepository.SaveChanges();

                return new PayloadResponse()
                {
                    IsSuccess = true,
                    Message = "Location deleted successfully"
                };
            }
            catch (Exception ex)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    Message = $"Location deleted failed because {ex.Message}"
                };
            }
        }

        public Location Get(string id)
        {
            var location = _locationRepository.GetConditional(l => l.Id == id);

            return location;
        }
    }
}
