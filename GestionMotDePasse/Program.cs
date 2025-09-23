
using System.Security.Cryptography;
using System.Text;
using GestionMotDePasse.Data;
using GestionMotDePasse.Data.Models;
using GestionMotDePasse.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

Console.WriteLine("Vueillez saisir le mot de passe maitre :");
string masterPassword = "";

while (string.IsNullOrEmpty(masterPassword))
{
    masterPassword = GetUserEntryPassword();
}

var hashMasterPassword = SHA256.HashData(Encoding.UTF8.GetBytes(masterPassword));

if (!File.Exists("pwd.bin"))
{
    File.WriteAllBytes("pwd.bin", hashMasterPassword);
}
else
{
    var password = File.ReadAllBytes("pwd.bin");
    if (!hashMasterPassword.SequenceEqual(password))
    {
        Console.WriteLine("Mot de passe maitre incorrect. Au revoir!");
        Environment.Exit(1);
    }

}

// gestion base sql lite
await using var context = new PassManagerDbContext();

// création de la base de données si elle n'existe pas
await context.Database.MigrateAsync();

// grise le menu si pas de mot de passes
bool isEmpty = !await context.Passwords.AnyAsync();

string[] menuItems = ["Ajouter un mot de passe", "Consulter un mot de passe", "Quitter"];

// index courrant du choix de l'utilisateur
int index = 0;

// lecture de la touche appuyée par l'utilisateur
ConsoleKeyInfo key;

do
{
    // vérifie si la base de données est vide
    if (isEmpty)
    {
        isEmpty = !await context.Passwords.AnyAsync();
    }

    Console.Clear();

    foreach ((int idx, string menuItem) in menuItems.Index())
    {
        if (isEmpty && idx == 1)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
        }
        else if (idx == index)
        {
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.ForegroundColor = ConsoleColor.Black;
        }
        Console.WriteLine(menuItem);
        Console.ResetColor();
    }

    key = Console.ReadKey();

    if (key.Key == ConsoleKey.UpArrow)
    {
        do
        {
            index--;
            if (index < 0) index = menuItems.Length - 1;
        } while (isEmpty && index == 1);
    }
    else if (key.Key == ConsoleKey.DownArrow)
    {
        do
        {
            index++;
            if (index >= menuItems.Length) index = 0;
        } while (isEmpty && index == 1);

    }
    else if (key.Key == ConsoleKey.Enter)
    {
        switch (index)
        {
            case 0:

                await AddOrUpdatePassword(hashMasterPassword, context);

                break;

            case 1:
                await SearchAndDisplayPassword(hashMasterPassword, context);

                break;
            case 2:
                Environment.Exit(0);
                break;
        }
    }
}
while (key.Key != ConsoleKey.Escape);

Console.WriteLine(" Au revoir! ");

static async Task AddOrUpdatePassword(byte[] hashMasterPassword, PassManagerDbContext context)
{
    Console.WriteLine("Entrez le nom du mot de passe :");
    string? name = Console.ReadLine();
    while (string.IsNullOrEmpty(name))
    {
        name = Console.ReadLine();
    }

    Console.WriteLine("Entrez le mot de passe :");
    string? password = GetUserEntryPassword();
    while (string.IsNullOrEmpty(password))
    {
        password = GetUserEntryPassword();
    }

    // test si le mot de passe existe déjà
    var exist = await context.Passwords.AnyAsync(p => p.Name == name);

    if (exist)
    {
        Console.WriteLine("Un mot de passe avec ce nom existe déjà ! Voulez vous le modifier ? (O/N)");

        var answer = Console.ReadLine() ?? "N";

        if (answer.Equals("O", StringComparison.OrdinalIgnoreCase))
        {
            var pass = await context.Passwords.FirstAsync(p => p.Name == name);
            pass.Value = CryptoService.Encrypt(password, hashMasterPassword);
            pass.UpdateDate = DateTime.UtcNow;
            context.Passwords.Update(pass);
            await context.SaveChangesAsync();
            Console.WriteLine("Mot de passe modifié avec succès !");
        }
        else
        {
            var passwordEntry = new PasswordEntry()
            {
                Name = name,
                Value = CryptoService.Encrypt(password, hashMasterPassword),
                InsertDate = DateTime.UtcNow
            };

            context.Passwords.Add(passwordEntry);
            await context.SaveChangesAsync();

            Console.WriteLine("Mot de passe ajouté avec succès !");
        }

    }
    else
    {
        var passwordEntry = new PasswordEntry()
        {
            Name = name,
            Value = CryptoService.Encrypt(password, hashMasterPassword),
            InsertDate = DateTime.UtcNow
        };

        context.Passwords.Add(passwordEntry);
        await context.SaveChangesAsync();

        Console.WriteLine("Mot de passe ajouté avec succès !");

    }


    Console.WriteLine("Appuyez sur une touche pour continuer...");
    Console.ReadLine();

}

static async Task SearchAndDisplayPassword(byte[] hashMasterPassword, PassManagerDbContext context)
{
    Console.WriteLine("Entrez le mot de passe a rechercher :");
    string? name = Console.ReadLine();
    while (string.IsNullOrEmpty(name))
    {
        name = Console.ReadLine();
    }

    // liste des mots de passe correspondant
    var matchingPasswords = await context.Passwords
        .Where(p => p.Name == name)
        .OrderByDescending(p => p.InsertDate)
        .ToListAsync();

    if (matchingPasswords is [])
    {
        Console.WriteLine("Aucun mot de passe trouvé pour ce nom. Voulez vous essayer un autre nom ? (O/N)");
        var answer = Console.ReadLine() ?? "N";

        if (answer.Equals("O", StringComparison.OrdinalIgnoreCase))
        {
            await SearchAndDisplayPassword(hashMasterPassword, context);
        }

        return;
    }

    if (matchingPasswords is [_])
    {
        var passwordEntry = matchingPasswords[0];
        var decryptedPassword = CryptoService.Decrypt(passwordEntry.Value, hashMasterPassword);
        Console.WriteLine($"Mot de passe pour '{name}' : {decryptedPassword}");
    }
    else 
    {
        SelectAndShowPassword(matchingPasswords, hashMasterPassword);
    }

    Console.WriteLine("Appuyez sur une touche pour continuer...");
    Console.ReadLine();
}

static string GetUserEntryPassword()
{

    StringBuilder password = new();

    var key = Console.ReadKey(true);
    while (key.Key != ConsoleKey.Enter)
    {
        if (key.Key == ConsoleKey.Backspace && password.Length > 0)
        {
            password.Remove(password.Length - 1, 1);

            // efface le dernier *
            var pos = Console.CursorLeft;
            Console.SetCursorPosition(pos - 1, Console.CursorTop);
            Console.Write(" ");
            Console.SetCursorPosition(pos - 1, Console.CursorTop);

        }
        else if (!char.IsControl(key.KeyChar))
        {
            password.Append(key.KeyChar);
            Console.Write("*");
        }
        key = Console.ReadKey(true);

    }
    Console.WriteLine();

    return password.ToString();
}

static void SelectAndShowPassword(IReadOnlyList<PasswordEntry> passwordEntries, byte[] hashedMasterPassword) 
{
    var index = 0;
    ConsoleKeyInfo key;
    do
    {
        Console.Clear();
        Console.WriteLine("Selectionner un mot de passe: ");
        for(int i=0; i < passwordEntries.Count; i++) 
        {
            if (i == index)
            {
                Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.ForegroundColor = ConsoleColor.Black;               
            }

            Console.WriteLine($"{i + 1} - {passwordEntries[i].Name} ({passwordEntries[i].InsertDate:dd/MM/yyyyy HH:mm:ss})");
            Console.ResetColor();
        }

        key = Console.ReadKey();

        if (key.Key == ConsoleKey.UpArrow)
        {
            index = Math.Max(0, index - 1);
        }
        else if (key.Key == ConsoleKey.DownArrow)
        {
            index = Math.Min(passwordEntries.Count - 1, index + 1);
        }

    } while (key.Key != ConsoleKey.Enter);

    var selectPassword = passwordEntries[index];
    var clearPassword = CryptoService.Decrypt(selectPassword.Value, hashedMasterPassword);

    Console.WriteLine($"Mot de passe pour '{selectPassword.Name}' : {clearPassword}");
}