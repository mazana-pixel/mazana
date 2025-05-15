using System.Collections.Generic;

namespace SignInSignUpApp
{
    public static class AuthenticationService
    {
        private static Dictionary<string, string> users = new Dictionary<string, string>();

        public static bool Authenticate(string email, string password)
        {
            return users.ContainsKey(email) && users[email] == password;
        }

        public static bool Register(string name, string email, string password)
        {
            if (users.ContainsKey(email)) return false;
            users[email] = password;
            return true;
        }

        public static bool ResetPassword(string email)
        {
            if (!users.ContainsKey(email)) return false;
            // Simulate password reset logic
            users[email] = "newpassword"; // A new password or token logic can be implemented here
            return true;
        }
    }
}
