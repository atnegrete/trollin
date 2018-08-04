
using System.Text.RegularExpressions;

public class SimpleAuthValidator 
{

    private readonly int UsernameMinLength = 6;
    private readonly int UsernameMaxLength = 20;

    private readonly int PasswordMinLength = 6;
    private readonly int PasswordMaxLength = 20;


    public string ValidateUsername(string username)
    {
        if(username.Length < UsernameMinLength)
        {
            return string.Format("Username length must be at least {0} characters!", UsernameMinLength);
        }

        if(username.Length > UsernameMaxLength)
        {
            return string.Format("Username length must be at most {0} characters!", UsernameMaxLength);
        }

        Regex regex = new Regex(@"\d+");
        Match match = regex.Match("^[a-zA-Z0-9]+$");
        if (!match.Success)
        {
            return string.Format("Username must only contain alphabetical letters and numbers!");
        }

        return null;
    }

    public string ValidatePassword(string password)
    {
        if (password.Length < PasswordMinLength)
        {
            return string.Format("Password length must be at least {0} characters!", PasswordMinLength);
        }

        if (password.Length > PasswordMaxLength)
        {
            return string.Format("Password length must be at most {0} characters!", PasswordMaxLength);
        }

        if(password.Contains(" "))
        {
            return string.Format("Password must not contain empty spaces!");
        }

        var hasNumber = new Regex(@"[0-9]+");

        if (!hasNumber.IsMatch(password))
        {
            return string.Format("Password must contain at least one number!");
        }

        return null;
    }
}