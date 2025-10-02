using GoBangladesh.Application.DTOs.Contact;
using GoBangladesh.Application.ViewModels;

namespace GoBangladesh.Application.Interfaces;

public interface IContactService
{
    PayloadResponse Submit(ContactFormDto contactForm);
    PayloadResponse GetAllUnreadContacts(int pageNo, int pageSize);
    PayloadResponse GetAllReadContacts(int pageNo, int pageSize);
    PayloadResponse MarkAsRead(MarkAsReadDto markAsReadData);
    PayloadResponse GetAllContactCount();
}