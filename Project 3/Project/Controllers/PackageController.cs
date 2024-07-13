using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PayPal.Api;
using Project.Data;
using Project.Helpers;
using Project.Models;
using Project.Models.Identity;
using Project.Repository;
using System.Data;
using System.Net.Mail;
using System.Security.Claims;

namespace Project.Controllers
{
    [Authorize(Roles = "Users")]
    public class PackageController : Controller
    {

        
        private IHttpContextAccessor httpContextAccessor;
        private readonly AppDbContext _dbContext;
        private readonly EmailHelper _emailHelper;
        IConfiguration _configuration;
        private readonly UserManager<AppUser> _userManager;
        public PackageController(IHttpContextAccessor context, IConfiguration iconfiguration, AppDbContext dbContext, EmailHelper emailHelper, UserManager<AppUser> userManager)
        {
         
            httpContextAccessor = context;
            _configuration = iconfiguration;
            _dbContext = dbContext;
            _emailHelper = emailHelper;
            _userManager = userManager;
           
        }

       
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Policy = "CheckPaymentExpiry")]
        public IActionResult Test()
        {
            return View();
        }
        public ActionResult PaymentWithPaypal(string Cancel = null!, string blogId = "", string PayerID = "", string guid = "", string packageType = "")
        {
            var ClientID = _configuration.GetValue<string>("PayPal:Key");
            var ClientSecret = _configuration.GetValue<string>("PayPal:Secret");
            var mode = _configuration.GetValue<string>("PayPal:mode");
            APIContext apiContext = PaypalConfiguration.GetAPIContext(ClientID, ClientSecret, mode);

            try
            {
                string payerId = PayerID;
                if (string.IsNullOrEmpty(payerId))
                {
                    string baseURI = this.Request.Scheme + "://" + this.Request.Host + "/Package/PaymentWithPayPal?";
                    var guidd = Convert.ToString((new Random()).Next(100000));
                    guid = guidd;

                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid, blogId, packageType);

                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = null!;
                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;
                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            paypalRedirectUrl = lnk.href;
                        }
                    }

                    httpContextAccessor.HttpContext.Session.SetString("payment", createdPayment.id);
                    httpContextAccessor.HttpContext.Session.SetString("packageType", packageType);
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    var paymentId = httpContextAccessor.HttpContext.Session.GetString("payment");
                    var executedPayment = ExecutePayment(apiContext, payerId, paymentId as string);
                    var packageType1 = httpContextAccessor.HttpContext.Session.GetString("packageType");

                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("PaymentFailed");
                    }
                    var blogIds = executedPayment.transactions[0].item_list.items[0].sku;




                    //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    ////// Save payment information to the database

                    //    var paymentInfo = new PaymentInfo()
                    //    {
                    //        PaymentId = executedPayment.id,
                    //        PackageType =packageType,
                    //        CreatedDate = DateTime.UtcNow,
                    //        ExpiryDate = DateTime.UtcNow.AddDays(packageType == "monthly" ? 30 : 365),
                    //        UserId = userId
                    //    };

                    //    _dbContext.PaymentInfos.Add(paymentInfo);
                    //    _dbContext.SaveChanges();
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    // Save payment information to the database
                    SavePaymentInfo(executedPayment.id, packageType1, userId);

                    return View("PaymentSuccess");
                }
            }
            catch (Exception ex)
            {
                // Log the error or perform specific error handling
                Console.WriteLine(ex.Message);
                return View("PaymentFailed");
            }

            return View("SuccessView");
        }
        private void SavePaymentInfo(string paymentId, string packageType, string userId)
        {
            var user = _userManager.FindByIdAsync(userId).Result;
            if (user != null)
            {
                double price = 0;
                string packageDuration = "";

                if (packageType == "monthly")
                {
                    price = 15;
                    packageDuration = "1 month";
                }
                else if (packageType == "yearly")
                {
                    price = 150;
                    packageDuration = "1 year";
                }

                var paymentInfo = new PaymentInfo()
                {
                    PaymentId = paymentId,
                    PackageType = packageType,
                    CreatedDate = DateTime.UtcNow,
                    ExpiryDate = DateTime.UtcNow.AddDays(packageType == "monthly" ? 30 : 365),
                    UserId = userId
                };

                _dbContext.PaymentInfos.Add(paymentInfo);

                // Gửi email thông báo thành công
                string emailBody = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <title>Payment Success</title>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        background-color: #f4f4f4;
                        padding: 20px;
                    }}
                    .email-container {{
                        background-color: #fff;
                        border-radius: 8px;
                        padding: 30px;
                        box-shadow: 0px 0px 10px 0px rgba(0,0,0,0.1);
                    }}
                    h1 {{
                        color: #333;
                    }}
                    p {{
                        color: #555;
                    }}
                </style>
            </head>
            <body>
                <div class='email-container'>
                    <h1>Payment Success</h1>
                    <p>Thank you for purchasing the premium package at ICECREAM!</p>
                    <p>Payment ID: <strong>{paymentId}</strong></p>
                    <p>Package Type: <strong>{packageType}</strong></p>
                    <p>Price: <strong>${price}</strong></p>
                    <p>Package Duration: <strong>{packageDuration}</strong></p>
                    <p>Created Date: <strong>{paymentInfo.CreatedDate}</strong></p>
                    <p>Expiry Date: <strong>{paymentInfo.ExpiryDate}</strong></p>
                </div>
            </body>
            </html>
        ";

                _emailHelper.SendAsync(new()
                {
                    Subject = "Payment Success",
                    Body = emailBody,
                    To = user.Email
                });

                _dbContext.SaveChanges();
            }
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
        private Payment CreatePayment(APIContext apiContext, string redirectUrl, string blogId, string packageType)
        {
            var itemList = new ItemList()
            {
                items = new List<Item>()
            };

            if (packageType == "monthly")
            {
                itemList.items.Add(new Item()
                {
                    name = "Monthly Package",
                    currency = "USD",
                    price = "15.00", // Giá hàng tháng
                    quantity = "1",
                    sku = "SKU_Monthly" // SKU của gói hàng tháng
                });
            }
            else if (packageType == "yearly")
            {
                itemList.items.Add(new Item()
                {
                    name = "Yearly Package",
                    currency = "USD",
                    price = "150.00", // Giá hàng năm
                    quantity = "1",
                    sku = "SKU_Yearly" // SKU của gói hàng năm
                });
            }

            var payer = new Payer()
            {
                payment_method = "paypal"
            };

            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };

            var amount = new Amount()
            {
                currency = "USD",
                total = itemList.items[0].price // Giá trị total dựa trên gói hàng tháng hoặc hàng năm
            };

            var transactionList = new List<Transaction>();
            transactionList.Add(new Transaction()
            {
                description = "Transaction description",
                invoice_number = Guid.NewGuid().ToString(),
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


            return this.payment.Create(apiContext);




        }
     

        public async Task<IActionResult> ViewPackage()
        {
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Lấy ID của người dùng đã đăng nhập

            // Sử dụng loggedInUserId để truy vấn thông tin người dùng từ cơ sở dữ liệu

            var paymentInfo = await _dbContext.PaymentInfos.FirstOrDefaultAsync(p => p.UserId == loggedInUserId);

            return View(paymentInfo);
        }


        public IActionResult Delete()
        {
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Lấy ID của người dùng đã đăng nhập

            var paymentInfo = _dbContext.PaymentInfos.FirstOrDefault(p => p.UserId == loggedInUserId);
            if (paymentInfo == null)
            {
                return NotFound();
            }

            _dbContext.PaymentInfos.Remove(paymentInfo);
            _dbContext.SaveChanges();

            return RedirectToAction("Index", "Home");
        }





    }
}
