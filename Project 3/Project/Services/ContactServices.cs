using System;
using Project.Data;
using Project.Models;
using Project.Repository;

namespace Project.Services
{

    public class ContactServices : ContactRepository
    {
        private AppDbContext _db;
        public ContactServices(AppDbContext db)
        {
            _db = db;

        }

        public bool DeleteContact(int contact)
        {
            var model = _db.ContactUs.FirstOrDefault(u => u.ContactId == contact);
            _db.ContactUs.Remove(model);
            _db.SaveChanges();
            return true;
        }

        public List<ContactU> GetContact()
        {
            return _db.ContactUs.ToList();
        }

        public ContactU UpdateContact(int Conid)
        {
            var model = _db.ContactUs.FirstOrDefault(u => u.ContactId == Conid);
            return model!;
        }

        public bool PostContact(ContactU newContact)
        {
            _db.ContactUs.Add(newContact);
            _db.SaveChanges();
            return true;
        }

        public bool UpdateContact(ContactU updatedContact)
        {
            var model = _db.ContactUs.FirstOrDefault(u => u.ContactId == updatedContact.ContactId);
            if (model != null)
            {
                model.Name = updatedContact.Name;
                model.Email = updatedContact.Email;
                model.Phone = updatedContact.Phone;
                model.Content = updatedContact.Content;

                _db.ContactUs.Update(model);
                _db.SaveChanges();
                return true;
            }
            return false;
        }

        public ContactU DetailContact(int ContactID)
        {
            return _db.ContactUs.FirstOrDefault(u => u.ContactId == ContactID)!;
        }
    }

}
    

