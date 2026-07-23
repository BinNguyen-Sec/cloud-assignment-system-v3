using CloudAssignment.Application.Common.Exceptions;
using CloudAssignment.Application.Features.Auth;

namespace CloudAssignment.UnitTests.Features.Auth;

public sealed class PasswordPolicyTests
{
    [Fact]
    public void StrongPasswordPassesValidation()
    {
        PasswordPolicy.Validate("Arcana@Test2026!");
    }

    [Theory]
    [InlineData("short")]
    [InlineData("alllowercase2026!")]
    [InlineData("ALLUPPERCASE2026!")]
    [InlineData("NoNumbersHere!")]
    [InlineData("NoSpecial2026")]
    public void WeakPasswordThrowsValidationException(string password)
    {
        Assert.Throws<RequestValidationException>(() => PasswordPolicy.Validate(password));
    }
}
