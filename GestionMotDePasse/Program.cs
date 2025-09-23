
using GestionMotDePasse.Data;
using GestionMotDePasse.Data.Models;
using Microsoft.EntityFrameworkCore;

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
string choix = "";

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
                Console.WriteLine("Entrez le nom du mot de passe :");
                string? name = Console.ReadLine();
                while (string.IsNullOrEmpty(name))
                {
                   name = Console.ReadLine();
                }

                Console.WriteLine("Entrez le mot de passe :");
                string? password = Console.ReadLine();
                while (string.IsNullOrEmpty(password))
                {
                    password = Console.ReadLine();
                }

                var passwordEntry = new PasswordEntry()
                {
                    Name = name,
                    Value = password,
                    InsertDate = DateTime.UtcNow
                };

                context.Passwords.Add(passwordEntry);
                await context.SaveChangesAsync();
               
                Console.WriteLine("Mot de passe ajouté avec succès !");
                Console.WriteLine("Appuyez sur une touche pour continuer...");
                Console.ReadLine();
                break;


            case 2:
                Environment.Exit(0);
                break;
        }
    }
}
while (key.Key != ConsoleKey.Escape);

Console.WriteLine(" Au revoir! ");