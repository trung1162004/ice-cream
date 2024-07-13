using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Helpers;
using Project.Models;
using Project.Repository;
using System.Security.Claims;
using System;


namespace Project.Controllers
{
    public class CartController : Controller
    {
        private IBookRepository _bookRepository;
        public CartController(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }


        public IActionResult ViewCart()
        {
            var cart = SessionHelper.GetObjectJson<List<OrderItem>>(HttpContext.Session, "cart");
            ViewBag.cart = cart;
            
            //ViewBag.subtotal = cart!.Sum(item => item.Book!.Price * item.Quantity);
            return View();
        }

        public int checkProduct(int id)
        {
            List<OrderItem> cart = SessionHelper.GetObjectJson<List<OrderItem>>(HttpContext.Session, "cart");
            for (int i = 0; i < cart!.Count; i++)
            {
                if (cart[i].Book!.BookId.Equals(id))
                {
                    return i;
                }
            }
            return -1;
        }

        public IActionResult Remove(int id)
        {
            List<OrderItem> cart = SessionHelper.GetObjectJson<List<OrderItem>>(HttpContext.Session, "cart");
            int index = checkProduct(id);
            cart!.RemoveAt(index);
            SessionHelper.SetObjectJson(HttpContext.Session, "cart", cart);
            return RedirectToAction("ViewCart", "Cart");
        }

        public IActionResult AddToCart(int id)
        {
            var model = _bookRepository.GetBooks();
            List<OrderItem> cart;
            if (SessionHelper.GetObjectJson<List<OrderItem>>(HttpContext.Session, "cart") == null)
            {
                cart = new List<OrderItem>();
                cart.Add(new OrderItem
                {
                    Book = model.Single(p => p.BookId.Equals(id)),
                    Quantity = 1
                });
                SessionHelper.SetObjectJson(HttpContext.Session, "cart", cart);
            }
            else
            {
                cart = SessionHelper.GetObjectJson<List<OrderItem>>(HttpContext.Session, "cart");
                int index = checkProduct(id);
                if (index != -1)
                {
                    cart[index].Quantity++;
                    SessionHelper.SetObjectJson(HttpContext.Session, "cart", cart);
                }
                else
                {
                    cart.Add(new OrderItem
                    {
                        Book = model.Single(p => p.BookId.Equals(id)),
                        Quantity = 1
                    });
                    SessionHelper.SetObjectJson(HttpContext.Session, "cart", cart);
                }
            }
            return RedirectToAction("Book", "Book");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int id, int newQuantity)
        {
            List<OrderItem> cart = SessionHelper.GetObjectJson<List<OrderItem>>(HttpContext.Session, "cart");
            int index = checkProduct(id);

            if (index != -1)
            {
                // Validate newQuantity against available stock
                if (newQuantity >= 1 && newQuantity <= cart[index].Book.Quantity)
                {
                    cart[index].Quantity = newQuantity;
                    SessionHelper.SetObjectJson(HttpContext.Session, "cart", cart);

                    // Trả về dữ liệu JSON (nếu cần)
                    return Json(new { success = true });
                }
                else
                {
                    // Return an error message or handle the invalid quantity
                    return Json(new { success = false, message = "Invalid quantity." });
                }
            }

            // Return an error message or handle the invalid product
            return Json(new { success = false, message = "Invalid product." });
        }

    }
}
