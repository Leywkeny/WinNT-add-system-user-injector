# UserAdd – Local User Creation for Windows

UserAdd is a C# utility designed to create a new **local machine account** on Windows systems using the `System.DirectoryServices` API.
It automatically:

* Generates a **secure random password**
* Appends a **trailing '$'** to the username (useful for hidden-like machine accounts)
* Adds the user to **Administrators** and **Remote Desktop Users** groups (if available)
* Outputs the generated credentials to the console

## Features

### **1. Automatic Secure Password Creation**

The program generates a **10-character cryptographically secure password** using `RandomNumberGenerator`.

### **2. Automatic Group Assignment**

If the groups exist on the local machine, the user is added to:

* **Administrators**
* **Remote Desktop Users**

### **3. Clean, Maintained Architecture**

The codebase is rewritten for:

* Better readability
* Clear structure
* Proper error handling
* Separation of responsibilities

### **4. Fully Self-Contained**

No external dependencies.
Works out-of-the-box on Windows machines with .NET Framework or .NET installed.


# How It Works

Below is a high-level explanation of each major component.

---

## 1. **Argument Handling**

```csharp
if (args.Length != 1)
{
    Console.WriteLine("[!] By default, a 10-character random password will be generated.");
    Console.WriteLine("Usage: UserAdd.exe <username>");
    return;
}
```

* Ensures the user provides exactly one argument.
* The argument becomes the base username.
* A trailing **‘$’** is added:
  Example: `john` → `john$`.

---

## 2. **Password Generation**

```csharp
private static string GenerateRandomPassword(int length)
```

* Uses **RandomNumberGenerator**, a cryptographically strong algorithm.
* Splits randomness evenly across a predefined character set:

  ```
  !@#$%0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz
  ```
* Produces **high-entropy passwords**, suitable for administrative accounts.

---

## 3. **User Creation via Directory Services**

```csharp
DirectoryEntry computerEntry = new DirectoryEntry($"WinNT://{Environment.MachineName},computer");
DirectoryEntry newUser = computerEntry.Children.Add(username, "user");
```

* Connects to the local SAM (Security Accounts Manager).
* Creates a *new local user object*.
* Sets the generated password using:

  ```csharp
  newUser.Invoke("SetPassword", new object[] { password });
  ```

---

## 4. **Group Assignment**

```csharp
AddUserToLocalGroup(computerEntry, "Administrators", newUser);
AddUserToLocalGroup(computerEntry, "Remote Desktop Users", newUser);
```

The helper method:

```csharp
DirectoryEntry group = computerEntry.Children.Find(groupName, "group");
group?.Invoke("Add", new object[] { newUser.Path });
```

* Safely looks up the group.
* Adds the user to the group if the group exists.
* Uses a try–catch to avoid stopping execution if a group is missing.


## 5. **Console Output**

The result is printed cleanly:

```
[*] Account Created Successfully
[+] Username: john$
[+] Password: A9%kT1#ZqL
```

# Usage

### **Command**

```
UserAdd.exe <username>
```

### **Example**

```
UserAdd.exe backupUser
```

**Output example:**

```
[*] Account Created Successfully
[+] Username: backupUser$
[+] Password: H@8ds92LfQ
```


# Building the Project

1. Open the solution in Visual Studio
2. Select **Build Solution** from the **Build** menu.
3. The executable will be located in:

   ```
   /bin/Debug/
   ```


# Troubleshooting
- Group Not Found Warnings

  If groups like *Remote Desktop Users* are missing on the system, the tool simply ignores them.
  Windows editions differ (Home vs. Pro), so this is expected behavior.

- Username Already Exists

  If `username$` exists, you will receive an LDAP/SAM exception.
  Choose a different base username.

# Security Notes

This tool:

* Creates privileged administrative accounts
* Generates passwords and prints them to console
* Adds users to remote access groups


# Disclaimer

This project is intended **solely for educational, administrative, and authorized system maintenance purposes**.
The author assumes no liability for misuse.

## License

This project is licensed under the MIT License. For more information, see the [LICENSE file](LICENSE).
