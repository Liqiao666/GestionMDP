
using GestionMotDePasse.Data;
using Microsoft.EntityFrameworkCore;

// gestion base sql lite
await using var context = new PassManagerDbContext();

// création de la base de données si elle n'existe pas
await context.Database.MigrateAsync();


string[] menuItems = ["Ajouter un mot de passe", "Consulter un mot de passe", "Quitter"];

// index courrant du choix de l'utilisateur
int index = 0;

// lecture de la touche appuyée par l'utilisateur
ConsoleKeyInfo key;
string choix = "";

do
{
    Console.Clear();

    foreach ((int idx, string menuItem) in menuItems.Index())
    {
        if(idx == index)
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
        index--;
        if (index < 0) index = menuItems.Length - 1;
    }
    else if (key.Key == ConsoleKey.DownArrow)
    {
        index++;
        if (index >= menuItems.Length) index = 0;
    }
    else if (key.Key == ConsoleKey.Enter)
    {
        choix = menuItems[index];
        Console.WriteLine(choix);
        Console.WriteLine("Appuyez sur une touche pour continuer...");
        Console.ReadLine();
    }
}
while (key.Key != ConsoleKey.Escape);

Console.WriteLine(" Au revoir! ");