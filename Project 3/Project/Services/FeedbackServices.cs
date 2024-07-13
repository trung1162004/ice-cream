using System;
using Project.Data;
using Project.Models;
using Project.Repository;

namespace Project.Services
{
	public class FeedbackServices: FeedbackRepository
	{
	 
            private AppDbContext _db;
        public FeedbackServices(AppDbContext db) { _db = db; }

        public void create(Feedback feedback)
        {
            _db.Feedbacks.Add(feedback);
            _db.SaveChanges();
        }

        public void delete(int id)
        {
            var feedback = _db.Feedbacks.SingleOrDefault(f => f.FeedbackId == id);
            if (feedback != null)
            {
                _db.Feedbacks.Remove(feedback);
                _db.SaveChanges();
            }
        }

        public Feedback DetailFeedback(int FeedbackID)
        {
            return _db.Feedbacks.FirstOrDefault(u => u.FeedbackId == FeedbackID)!;
        }

        public List<Feedback> findAll()
        {
            
            return _db.Feedbacks.ToList();
        }

        public bool update(Feedback updatedFeedback)
        {
            
                var model = _db.Feedbacks.FirstOrDefault(u => u.FeedbackId == updatedFeedback.FeedbackId);
                if (model != null)
                {
                model.Name = updatedFeedback.Name;
                model.Email = updatedFeedback.Email;
                model.FeedbackText = updatedFeedback.FeedbackText;
                model.FeedbackDate = updatedFeedback.FeedbackDate;

                _db.Feedbacks.Update(model);
                    _db.SaveChanges();
                return true;
                }
                return false;

        }

        public Feedback update(int Fbid)
        {
            var model = _db.Feedbacks.FirstOrDefault(u => u.FeedbackId == Fbid);
            return model!;
        }
    }
}


