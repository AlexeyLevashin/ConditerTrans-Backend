using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs;

[Authorize(Roles = "Dispatcher")]
public sealed class ReportsHub : Hub
{
    public static string CompanyGroup(Guid companyId) => $"reports:{companyId:N}";

    public override async Task OnConnectedAsync()
    {
        var companyIdClaim = Context.User?.FindFirstValue("CompanyId");
        if (!Guid.TryParse(companyIdClaim, out var companyId))
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, CompanyGroup(companyId));
        await base.OnConnectedAsync();
    }
}
