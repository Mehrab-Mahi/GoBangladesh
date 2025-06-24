using System;
using System.Collections.Generic;
using System.Linq;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;

namespace GoBangladesh.Application.Services
{
    public class ContactService : IContactService
    {
        public readonly IRepository<Contact> _contactRepository;
        public readonly IRepository<User> _userRepository;
        private readonly IRepository<Location> _locationRepository;

        public ContactService(IRepository<Contact> contactRepository,
            IRepository<User> userRepository,
            IRepository<Location> locationRepository)
        {
            _contactRepository = contactRepository;
            _userRepository = userRepository;
            _locationRepository = locationRepository;
        }

        public PayloadResponse Create(Contact contactData)
        {
            try
            {
                _contactRepository.Insert(contactData);
                _contactRepository.SaveChanges();

                return new PayloadResponse()
                {
                    IsSuccess = true,
                    PayloadType = "Contact",
                    Content = null,
                    Message = "Contact placed successfully"
                };
            }
            catch (Exception ex)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Contact",
                    Content = null,
                    Message = $"Contact can't be placed because {ex.Message}"
                };
            }
        }

        public object GetAll(string contactType, int pageNo, int pageSize)
        {
            var allContact = _contactRepository
                .GetAll()
                .Where(c => c.ContactType == contactType);

            var totalRowCount = allContact
                .Count();

            var contactList = allContact
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var contactWithUserData = (from contact in contactList
                join user in _userRepository.GetAll()
                    on contact.CreatedBy equals user.Id into contactUserData
                from cu in contactUserData
                join district in _locationRepository.GetAll() on cu.District equals district.Id
                join upazila in _locationRepository.GetAll() on cu.Upazila equals upazila.Id
                join union in _locationRepository.GetAll() on cu.Union equals union.Id
                select new ContactVm()
                {
                    Id = contact.Id,
                    ContactType = contact.ContactType,
                    CreateTime = contact.CreateTime,
                    Subject = contact.Subject,
                    Message = contact.Message,
                    IsRead = contact.IsRead,
                    CreatedBy = cu.FullName,
                    UserData = new UserCreationVm()
                    {
                        Id = cu.Id,
                        FullName = cu.FullName,
                        BloodGroup = cu.BloodGroup,
                        DateOfBirth = cu.DateOfBirth,
                        MobileNumber = cu.MobileNumber,
                        District = cu.District,
                        DistrictName = district.Name,
                        Upazila = cu.Upazila,
                        UpazilaName = upazila.Name,
                        Union = cu.Union,
                        UnionName = union.Name,
                        Address = cu.Address,
                        FatherName = cu.FatherName,
                        MotherName = cu.MotherName,
                        BloodDonationStatus = cu.BloodDonationStatus,
                        Gender = cu.Gender,
                        UserType = cu.UserType,
                        LastDonationTime = cu.LastDonationTime,
                        ImageUrl = cu.ImageUrl,
                        BloodDonationCount = cu.BloodDonationCount,
                        IsApproved = cu.IsApproved
                    }
                }).ToList();

            return new
            {
                data = contactWithUserData,
                rowCount = totalRowCount
            };
        }

        public PayloadResponse ReadContact(string id)
        {
            try
            {
                var contact = _contactRepository.GetConditional(c => c.Id == id);
                contact.IsRead = true;
                _contactRepository.Update(contact);
                _contactRepository.SaveChanges();

                return new PayloadResponse()
                {
                    IsSuccess = true,
                    PayloadType = "Contact",
                    Content = null,
                    Message = "Contact updated successfully"
                };
            }
            catch (Exception ex)
            {
                return new PayloadResponse()
                {
                    IsSuccess = false,
                    PayloadType = "Contact",
                    Content = null,
                    Message = $"Contact can't be updated because {ex.Message}"
                };
            }
        }

        public ContactVm Get(string id)
        {
            var contact = _contactRepository
                .GetAll()
                .Where(c => c.Id == id)
                .Select(contact => new ContactVm()
                {
                    Id = contact.Id,
                    ContactType = contact.ContactType,
                    CreateTime = contact.CreateTime,
                    Subject = contact.Subject,
                    Message = contact.Message,
                    IsRead = contact.IsRead,
                    CreatedBy = contact.CreatedBy,
                })
                .FirstOrDefault();

            if (contact is null)
            {
                return new ContactVm();
            }

            var user = _userRepository.GetConditional(u => u.Id == contact.CreatedBy);

            if (user is null) return contact;

            contact.UserData.Id = user.Id;
            contact.UserData.FullName = user.FullName;
            contact.UserData.UserType = user.UserType;
            contact.UserData.MobileNumber = user.MobileNumber;
            contact.UserData.BloodGroup = user.BloodGroup;
            contact.UserData.Address = user.Address;
            contact.UserData.Gender = user.Gender;
            contact.UserData.DateOfBirth = user.DateOfBirth;
            contact.UserData.ImageUrl = user.ImageUrl;

            return contact;
        }
    }
}
