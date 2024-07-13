using System;
using  Project.Models;

namespace Project.Repository
{
    public interface ContactRepository
    {
        List<ContactU> GetContact();
        bool PostContact(ContactU newContact);
        bool DeleteContact(int contact);
        bool UpdateContact(ContactU updatedContact);
        ContactU DetailContact(int ContactID);
        ContactU UpdateContact(int Conid);
    }
}

