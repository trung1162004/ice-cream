using System;
using Project.Models.Identity;
using Project.Models.ViewModels;
namespace Project.Repository
{
	public interface UserDetailRepo
	{
        List<AppUser> findAll();
    }
}

