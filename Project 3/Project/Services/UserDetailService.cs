using System;
using Project.Data;
using Project.Models;
using Project.Models.Identity;
using Project.Repository;

namespace Project.Services
{
	public class UserDetailService: UserDetailRepo
	{
        private AppDbContext _db;
        public UserDetailService(AppDbContext db) { _db = db; }

        public List<AppUser> findAll()
        {
            return _db.Users.ToList();
        }
    }
}

