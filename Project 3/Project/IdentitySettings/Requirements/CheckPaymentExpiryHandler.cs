using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Project.Data;


public class CheckPaymentExpiryRequirement : IAuthorizationRequirement { }
public class CheckPaymentExpiryHandler : AuthorizationHandler<CheckPaymentExpiryRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AppDbContext _dbContext;
    public CheckPaymentExpiryHandler(IHttpContextAccessor httpContextAccessor,AppDbContext dbContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CheckPaymentExpiryRequirement requirement)
    {
        var hasValidExpiryDate = CheckExpiryDate();

        if (hasValidExpiryDate) 
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    private bool CheckExpiryDate()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        // TODO: Thực hiện truy vấn để kiểm tra ExpiryDate của người dùng

        var expiryDate = _dbContext.PaymentInfos
            .Where(p => p.UserId == userId)
            .Select(p => p.ExpiryDate)
            .FirstOrDefault();

        // Kiểm tra xem ExpiryDate có hợp lệ không
        if (expiryDate > DateTime.UtcNow)
        {
            return true;
        }

        // Return false nếu ExpiryDate đã hết hạn hoặc không hợp lệ
        return false;
    }

}