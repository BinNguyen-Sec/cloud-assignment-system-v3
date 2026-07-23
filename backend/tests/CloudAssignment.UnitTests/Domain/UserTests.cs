using CloudAssignment.Domain.Users;

namespace CloudAssignment.UnitTests.Domain;

public sealed class UserTests
{
    [Fact]
    public void ChangePasswordClearsMandatoryChangeFlag()
    {
        var createdAt = new DateTimeOffset(2026, 7, 23, 0, 0, 0, TimeSpan.Zero);
        var user = User.Create(
            Guid.NewGuid(),
            null,
            "Giảng viên Arcana",
            "teacher@arcana.local",
            "TEACHER@ARCANA.LOCAL",
            "initial-hash",
            UserRole.Teacher,
            createdAt,
            mustChangePassword: true);

        user.ChangePassword("replacement-hash", createdAt.AddMinutes(1));

        Assert.False(user.MustChangePassword);
        Assert.Equal("replacement-hash", user.PasswordHash);
    }
}
