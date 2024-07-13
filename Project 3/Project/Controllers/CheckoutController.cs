using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PayPal.Api;
using Project.Helpers;
using Project.Models;
using Project.Models.Identity;
using Project.Repository;
using System.Data;
using Payment = PayPal.Api.Payment;
namespace ProjectSem3.Controllers
{

    [Authorize(Roles = "Users")]
    public class CheckoutController : Controller
    {
        private IOrderRepository _orderRepository;
        private IBookRepository _bookRepository;
        private IHttpContextAccessor httpContextAccessor;
        IConfiguration _configuration;
        private readonly UserManager<AppUser> _userManager;
        public CheckoutController(IHttpContextAccessor context, IConfiguration iconfiguration, IOrderRepository orderRepository, IBookRepository bookRepository, UserManager<AppUser> userManager)
        {
            _orderRepository = orderRepository;
            httpContextAccessor = context;
            _configuration = iconfiguration;
            _userManager = userManager;
            _bookRepository = bookRepository;
        }


        [HttpGet]
        public async Task<IActionResult> Checkout()
        {

            var cart = SessionHelper.GetObjectJson<List<OrderItem>>(HttpContext.Session, "cart");
            ViewBag.cart = cart;
            return View();
        }

        [HttpPost]
        public IActionResult Checkout(Project.Models.Orders order)
        {
            try
            {
                var cart = SessionHelper.GetObjectJson<List<OrderItem>>(HttpContext.Session, "cart");
                if (cart == null || cart.Count == 0)
                {
                    TempData["error"] = "Your cart is empty.";
                    return RedirectToAction("ViewCart", "Cart");
                }

                order.OrderDate = DateTime.Now;
                order.Status = "Pending";
                //order.UserId = 1;
                order.OrderItems = cart.Select(item => new OrderItem
                {
                    BookId = item.Book.BookId,
                    OrderId = item.OrderId,
                    Quantity = item.Quantity,
                    Amount = item.Book.Discount != null ? item.Book.Discount : item.Book.Price,
                    Total = item.Quantity * (item.Book.Discount != null ? item.Book.Discount : item.Book.Price)
                }).ToList();

                var orderJson = JsonConvert.SerializeObject(order);
                HttpContext.Session.SetString("OrderData", orderJson);

                if (order.PaymentMethod == "pay_later")
                {
                    foreach (var item in cart)
                    {
                        _bookRepository.DecreaseQuantity(item.Book.BookId, item.Quantity);
                    }
                    _orderRepository.SaveOrder(order);
                    HttpContext.Session.Remove("cart");
                    TempData["notification"] = "Congratulations! Your order is confirmed, and payment can be settled upon receiving your delivery.";
                    return View("Result");
                }
                if (order.PaymentMethod == "online_payment")
                {
                    return RedirectToAction("PaymentWithPaypal");
                }
            }
            catch (Exception)
            {
                throw;
            }
            return View();
        }

        public ActionResult PaymentWithPaypal(string Cancel = null!, string blogId = "", string PayerID = "", string guid = "")
        {
            //getting the apiContext  
            var ClientID = _configuration.GetValue<string>("PayPal:Key");
            var ClientSecret = _configuration.GetValue<string>("PayPal:Secret");
            var mode = _configuration.GetValue<string>("PayPal:mode");
            APIContext apiContext = PaypalConfiguration.GetAPIContext(ClientID, ClientSecret, mode);
            // apiContext.AccessToken="Bearer access_token$production$j27yms5fthzx9vzm$c123e8e154c510d70ad20e396dd28287";
            try
            {
                //A resource representing a Payer that funds a payment Payment Method as paypal  
                //Payer Id will be returned when payment proceeds or click to pay  
                string payerId = PayerID;
                if (string.IsNullOrEmpty(payerId))
                {

                    string baseURI = this.Request.Scheme + "://" + this.Request.Host + "/Checkout/PaymentWithPayPal?";
                    //here we are generating guid for storing the paymentID received in session  
                    //which will be used in the payment execution  
                    var guidd = Convert.ToString((new Random()).Next(100000));
                    guid = guidd;
                    //CreatePayment function gives us the payment approval url  
                    //on which payer is redirected for paypal account payment  
                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid, blogId);
                    //get links returned from paypal in response to Create function call  
                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = null;
                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;
                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            //saving the payapalredirect URL to which user will be redirected for payment  
                            paypalRedirectUrl = lnk.href;
                        }
                    }
                    // saving the paymentID in the key guid  
                    httpContextAccessor.HttpContext.Session.SetString("payment", createdPayment.id);
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    // This function exectues after receving all parameters for the payment  

                    var paymentId = httpContextAccessor.HttpContext.Session.GetString("payment");
                    var executedPayment = ExecutePayment(apiContext, payerId, paymentId as string);
                    //If executed payment failed then we will show payment failure message to user  
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        TempData["notification"] = "Payment Failure";
                        return View("Result");
                    }
                    var blogIds = executedPayment.transactions[0].item_list.items[0].sku;
                    // If payment is successful, retrieve order data from session
                    var orderJson = HttpContext.Session.GetString("OrderData");
                    var order = JsonConvert.DeserializeObject<Project.Models.Orders>(orderJson);

                    // Decrease quantity for each item in the order
                    foreach (var item in order.OrderItems)
                    {
                        _bookRepository.DecreaseQuantity(item.BookId, item.Quantity);
                    }
                    // Save order data to the database
                    _orderRepository.SaveOrder(order);

                    // Clear session data
                    HttpContext.Session.Remove("cart");
                    HttpContext.Session.Remove("OrderData");
                    // Redirect to success page
                    TempData["notification"] = "Transaction Successful! Your payment has been processed successfully.";
                    return View("Result");
                }
            }
            catch (Exception)
            {
                return View();
            }
            //on successful payment, show success page to user.  
        }
        private PayPal.Api.Payment payment;
        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new Payment()
            {
                id = paymentId
            };
            return this.payment.Execute(apiContext, paymentExecution);
        }
        private Payment CreatePayment(APIContext apiContext, string redirectUrl, string blogId)
        {
            // Tính tổng số tiền dựa trên giỏ hàng
            var cart = SessionHelper.GetObjectJson<List<OrderItem>>(HttpContext.Session, "cart");
            decimal total = 0;
            foreach (var item in cart)
            {
                var bookPrice = item.Book.Discount ?? item.Book.Price;
                total += Convert.ToDecimal(bookPrice * item.Quantity);
            }

            //create itemlist and add item objects to it  

            var itemList = new ItemList()
            {
                items = new List<Item>()
            };
            //Adding Item Details like name, currency, price etc  
            itemList.items.Add(new Item()
            {
                name = "Item Detail",
                currency = "USD",
                price = total.ToString("0.00"),
                quantity = "1",
                sku = "asd"
            });
            var payer = new Payer()
            {
                payment_method = "paypal"
            };
            // Configure Redirect Urls here with RedirectUrls object  
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };

            var amount = new Amount()
            {
                currency = "USD",
                total = total.ToString("0.00"), // Total must be equal to sum of tax, shipping and subtotal.  
                //details = details
            };
            var transactionList = new List<Transaction>();
            // Adding description about the transaction  
            transactionList.Add(new Transaction()
            {
                description = "Transaction description",
                invoice_number = Guid.NewGuid().ToString(), //Generate an Invoice No  
                amount = amount,
                item_list = itemList
            });
            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };
            // Create a payment using a APIContext  
            return this.payment.Create(apiContext);
        }
    }
}
