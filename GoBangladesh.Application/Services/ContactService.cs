using System.Linq;
using GoBangladesh.Application.DTOs.Contact;
using GoBangladesh.Application.Interfaces;
using GoBangladesh.Application.ViewModels;
using GoBangladesh.Domain.Entities;
using GoBangladesh.Domain.Interfaces;

namespace GoBangladesh.Application.Services;

public class ContactService : IContactService
{
    private readonly IRepository<Contact> _contactRepository;

    public ContactService(IRepository<Contact> contactRepository)
    {
        _contactRepository = contactRepository;
    }

    public PayloadResponse Submit(ContactFormDto contactForm)
    {
        try
        {
            var contact = new Contact
            {
                Name = contactForm.Name,
                Email = contactForm.Email,
                Mobile = contactForm.Mobile,
                Subject = contactForm.Subject,
                Message = contactForm.Message
            };

            _contactRepository.Insert(contact);
            _contactRepository.SaveChanges();

            return new PayloadResponse
            {
                IsSuccess = true,
                Message = "Your message has been submitted successfully."
            };
        }
        catch
        {
            return new PayloadResponse
            {
                IsSuccess = false,
                Message = "An error occurred while submitting your message. Please try again later."
            };
        }
    }

    public PayloadResponse GetAllUnreadContacts(int pageNo, int pageSize)
    {
        try
        {
            var contacts = _contactRepository
                .GetAll()
                .Where(c => !c.IsRead);

            var rowCount = contacts.Count();

            var data = contacts
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PayloadResponse()
            {
                IsSuccess = true,
                Content = new {data, rowCount},
                Message = "Unread contacts retrieved successfully."
            };
        }
        catch
        {
            return new PayloadResponse
            {
                IsSuccess = false,
                Message = "An error occurred while retrieving unread contacts. Please try again later."
            };
        }
    }

    public PayloadResponse GetAllReadContacts(int pageNo, int pageSize)
    {
        try
        {
            var contacts = _contactRepository
                .GetAll()
                .Where(c => c.IsRead);

            var rowCount = contacts.Count();

            var data = contacts
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PayloadResponse()
            {
                IsSuccess = true,
                Content = new {data, rowCount},
                Message = "Read contacts retrieved successfully."
            };
        }
        catch
        {
            return new PayloadResponse
            {
                IsSuccess = false,
                Message = "An error occurred while retrieving read contacts. Please try again later."
            };
        }
    }

    public PayloadResponse MarkAsRead(MarkAsReadDto markAsReadData)
    {
        try
        {
            var contact = _contactRepository.GetConditional(c => c.Id == markAsReadData.Id);

            if (contact == null)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    Message = "Contact not found."
                };
            }

            if (contact.IsRead)
            {
                return new PayloadResponse
                {
                    IsSuccess = false,
                    Message = "Contact is already marked as read."
                };
            }

            contact.IsRead = true;

            _contactRepository.Update(contact);
            _contactRepository.SaveChanges();

            return new PayloadResponse
            {
                IsSuccess = true,
                Message = "Contact marked as read successfully."
            };
        }
        catch
        {
            return new PayloadResponse
            {
                IsSuccess = false,
                Message = "An error occurred while marking the contact as read. Please try again later."
            };
        }
    }

    public PayloadResponse GetAllContactCount()
    {
        var unreadContacts = _contactRepository
            .GetAll()
            .Count(c => !c.IsRead);

        var readContacts = _contactRepository
            .GetAll()
            .Count(c => c.IsRead);

        return new PayloadResponse
        {
            IsSuccess = true,
            Content = new
            {
                unreadContacts,
                readContacts
            },
            Message = "Contact counts retrieved successfully."
        };
    }
}