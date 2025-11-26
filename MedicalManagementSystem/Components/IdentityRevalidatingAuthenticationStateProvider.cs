using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using MedicalManagementSystem.Models;

namespace MedicalManagementSystem.Components;

public class IdentityRevalidatingAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IdentityOptions _options;
    private readonly ILogger<IdentityRevalidatingAuthenticationStateProvider> _logger;

    public IdentityRevalidatingAuthenticationStateProvider(
        IServiceScopeFactory scopeFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<IdentityRevalidatingAuthenticationStateProvider> logger)
    {
        _scopeFactory = scopeFactory;
        _options = optionsAccessor.Value;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var principal = signInManager.Context.User;
        
        if (principal?.Identity?.IsAuthenticated == true)
        {
            var user = await userManager.GetUserAsync(principal);
            if (user != null && user.IsActive)
            {
                var claimsPrincipal = await signInManager.CreateUserPrincipalAsync(user);
                return new AuthenticationState(claimsPrincipal);
            }
        }

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }
}
