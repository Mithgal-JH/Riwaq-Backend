using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Team3.Backend.Data;
using Team3.Backend.Features.Authentication;
using Team3.Backend.Models;

namespace Team3.Backend.Tests;

internal sealed class ProvisioningTestContext : IDisposable
{
    private readonly AppDbContext _dbContext;

    public ProvisioningTestContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new AppDbContext(options);

        var userStore = new UserStore<User, IdentityRole<Guid>, AppDbContext, Guid>(
            _dbContext);
        UserManager = new UserManager<User>(
            userStore,
            Options.Create(new IdentityOptions()),
            new PasswordHasher<User>(),
            [new UserValidator<User>()],
            [new PasswordValidator<User>()],
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            new ServiceCollection().BuildServiceProvider(),
            NullLogger<UserManager<User>>.Instance);

        var roleStore = new RoleStore<IdentityRole<Guid>, AppDbContext, Guid>(
            _dbContext);
        RoleManager = new RoleManager<IdentityRole<Guid>>(
            roleStore,
            [new RoleValidator<IdentityRole<Guid>>()],
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            NullLogger<RoleManager<IdentityRole<Guid>>>.Instance);

        Service = new UserProvisioningService(
            _dbContext,
            UserManager,
            RoleManager);
    }

    public UserManager<User> UserManager { get; }

    public RoleManager<IdentityRole<Guid>> RoleManager { get; }

    public UserProvisioningService Service { get; }

    public AppDbContext DbContext => _dbContext;

    public void Dispose() => _dbContext.Dispose();
}
