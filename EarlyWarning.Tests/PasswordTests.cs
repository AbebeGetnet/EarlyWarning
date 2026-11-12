using EarlyWarning.Controllers;
using EarlyWarning.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace EarlyWarning.Tests;

/// <summary>Administrator reset to the default password, and the Amharic messages of the self-service change.</summary>
public class PasswordTests
{
    [Fact]
    public async Task DefaultPassword_PassesTheIdentityPasswordRules()
    {
        // Program.cs keeps Identity's default password options; a reset must never be rejected by them.
        var options = Options.Create(new IdentityOptions());
        var manager = new UserManager<ApplicationUser>(new NullStore(), options, new PasswordHasher<ApplicationUser>(),
            [], [new PasswordValidator<ApplicationUser>()], new UpperInvariantLookupNormalizer(), new IdentityErrorDescriber(), null!, null!);
        var result = await new PasswordValidator<ApplicationUser>().ValidateAsync(manager, new ApplicationUser(), UserRolesController.DefaultPassword);
        Assert.True(result.Succeeded, string.Join(" ", result.Errors.Select(x => x.Code)));
        Assert.Equal("SaleGod@1", UserRolesController.DefaultPassword);
    }

    [Theory]
    [InlineData("PasswordMismatch", "የአሁኑ የይለፍ ቃል ትክክል አይደለም።")]
    [InlineData("PasswordRequiresUpper", "የይለፍ ቃሉ ቢያንስ አንድ ትልቅ የእንግሊዝኛ ፊደል (A-Z) መያዝ አለበት።")]
    public void IdentityErrors_AreShownInAmharic(string code, string expected) =>
        Assert.Equal(expected, UserRolesController.PasswordError(new IdentityError { Code = code, Description = "english" }));

    [Fact]
    public void UnknownErrors_KeepIdentitysDescription() =>
        Assert.Equal("english", UserRolesController.PasswordError(new IdentityError { Code = "Other", Description = "english" }));

    private sealed class NullStore : IUserStore<ApplicationUser>
    {
        public void Dispose() { }
        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(user.Id);
        public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(user.UserName);
        public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken ct) => Task.CompletedTask;
        public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(user.NormalizedUserName);
        public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken ct) => Task.CompletedTask;
        public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(IdentityResult.Success);
        public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(IdentityResult.Success);
        public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(IdentityResult.Success);
        public Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken ct) => Task.FromResult<ApplicationUser?>(null);
        public Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken ct) => Task.FromResult<ApplicationUser?>(null);
    }
}
