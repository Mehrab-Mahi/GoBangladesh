using GoBangladesh.Application.DTOs.Bus;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace GoBangladesh.Application.Services;

public class BusService : IBusService
{
    private readonly IRepository<Bus> _busRepository;

    public BusService(IRepository<Bus> busRepository)
    {
        _busRepository = busRepository;
    }

    public PayloadResponse BusInsert(BusCreateRequest model)
    {
        try
        {
            if (IfDuplicateBusNumber(model.BusNumber))
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Bus",
                    Message = "Duplicate number found for bus"
                };
            }

            var bus = new Bus()
            {
                BusNumber = model.BusNumber,
                BusName = model.BusName,
                TripStartPlace = model.TripStartPlace,
                TripEndPlace = model.TripStartPlace,
                OrganizationId = model.OrganizationId,
            };

            _busRepository.Insert(bus);
            _busRepository.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Bus",
                Message = "Bus data has been inserted successfully!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Bus",
                Message = $"Bus creation is failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse BusUpdate(BusUpdateRequest model)
    {
        try
        {
            var bus = _busRepository
                .GetConditional(b => b.Id == model.Id);

            if (bus == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Bus",
                    Message = "Bus not found!"
                };
            }

            if (bus.BusNumber != model.BusNumber)
            {
                if (IfDuplicateBusNumber(model.BusNumber))
                {
                    return new PayloadResponse()
                    {
                        IsSuccess = false,
                        PayloadType = "Bus",
                        Message = "Duplicate bus number found"
                    };
                }
            }

            bus.BusName = model.BusName;
            bus.BusNumber = model.BusNumber;
            bus.TripStartPlace = model.TripStartPlace;
            bus.TripEndPlace = model.TripEndPlace;
            bus.OrganizationId = model.OrganizationId;

            _busRepository.Update(bus);
            _busRepository.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Bus",
                Message = "Bus has been updated successfully!"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Bus",
                Message = $"Bus update is failed because {ex.Message}!"
            };
        }
    }

    public PayloadResponse GetById(string id)
    {
        try
        {
            var bus = _busRepository.GetAll()
                .Where(o => o.Id == id)
                .Include(o => o.Organization)
                .FirstOrDefault();

            if (bus == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Bus",
                    Message = "Bus not found"
                };
            }

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Bus",
                Content = bus,
                Message = "Bus has been found"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Bus",
                Message = $"Bus fetching has been failed because {ex.Message}"
            };
        }
    }

    public PayloadResponse GetAll()
    {
        try
        {
            var busList = _busRepository
                .GetAll()
                .Include(b => b.Organization)
                .ToList();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Bus",
                Content = busList,
                Message = "Bus has been sent successfully"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Bus",
                Message = $"Bus fetching has been failed because {ex.Message}"
            };
        }
    }

    public PayloadResponse Delete(string id)
    {
        try
        {
            var bus = _busRepository
                .GetConditional(o => o.Id == id);

            if (bus == null)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Bus",
                    Message = "Bus not found"
                };
            }

            _busRepository.Delete(bus);
            _busRepository.SaveChanges();

            return new PayloadResponse()
            {
                IsSuccess = true,
                PayloadType = "Bus",
                Message = "Bus has been deleted successfully"
            };
        }
        catch (Exception ex)
        {
            return new PayloadResponse()
            {
                IsSuccess = false,
                PayloadType = "Bus",
                Message = $"Bus deletion is failed! because {ex.Message}"
            };
        }
    }

    private bool IfDuplicateBusNumber(string busNumber)
    {
        var bus = _busRepository.GetConditional(b => b.BusName == busNumber);

        return bus is not null;
    }
}