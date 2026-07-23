using CloudAssignment.Application.Common.Exceptions;

namespace CloudAssignment.Application.Features.Auth;

public static class PasswordPolicy
{
    public static void Validate(string password)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(password) || password.Length < 10)
        {
            errors.Add("Mật khẩu phải có ít nhất 10 ký tự.");
        }
        else
        {
            if (!password.Any(char.IsUpper))
            {
                errors.Add("Mật khẩu phải có ít nhất một chữ hoa.");
            }

            if (!password.Any(char.IsLower))
            {
                errors.Add("Mật khẩu phải có ít nhất một chữ thường.");
            }

            if (!password.Any(char.IsDigit))
            {
                errors.Add("Mật khẩu phải có ít nhất một chữ số.");
            }

            if (!password.Any(character => !char.IsLetterOrDigit(character)))
            {
                errors.Add("Mật khẩu phải có ít nhất một ký tự đặc biệt.");
            }
        }

        if (errors.Count > 0)
        {
            throw new RequestValidationException(
                new Dictionary<string, string[]> { ["newPassword"] = errors.ToArray() });
        }
    }
}
