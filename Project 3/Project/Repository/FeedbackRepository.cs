using System;
using Project.Models;

namespace Project.Repository
{
	public interface FeedbackRepository
	{
        void create(Feedback feedback);
        bool update(Feedback updatedFeedback);
        List<Feedback> findAll();
        Feedback DetailFeedback(int FeedbackID);
        Feedback update(int Fbid);
        void delete(int id);
    }
}

