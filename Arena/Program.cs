using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

Random rng = new Random();

List<int> wins = new List<int>(); // turns of each won game
int game = 0;

while (true)
{
    game++;
      int turns = PlayGame();
    if (turns > 0)
    {
        wins.Add(turns);
        SaveResult($"Game {game}: victory in {turns} turns");
    }
    else
    {
        SaveResult($"Game {game}: defeat");
    }

    Console.Write("Play again? (y/n) > ");
    if (Console.ReadLine() != "y")
    {
        break;
    }
}

Console.WriteLine($"Victories: {wins.Count} of {game}");
foreach (int t in wins.OrderBy(t => t).Take(3))
{
    Console.WriteLine($"- {t} turns");
}

int PlayGame()
{
    Player player = new Player("Aya", 100, 2);
    player.Died += name => Console.WriteLine($"Game over for {name}.");

    List<Enemy> enemies = new List<Enemy>();
    enemies.Add(new Goblin());
    enemies.Add(new Orc());
    enemies.Add(new Goblin());
    enemies.Add(new Boss());

    int turn = 0;
    Console.WriteLine("=== Arena ===");

    while (player.IsAlive && enemies.Count > 0)
    {
        Enemy enemy = enemies[0];
        Console.WriteLine();
        Console.WriteLine($"Turn {turn + 1}: {player.Name} {player.Health} HP, {player.Potions} potions | {enemy.Name} {enemy.Health} HP");
        Console.Write("1 attack, 2 drink a potion > ");

        string? text = Console.ReadLine();
        if (!int.TryParse(text, out int choice))
        {
            Console.WriteLine("Type 1 or 2.");
            continue;
        }

        switch (choice)
        {
            case 1:
                enemy.TakeDamage(rng.Next(12, 25));
                break;
            case 2:
                player.DrinkPotion();
                break;
            default:
                Console.WriteLine("Type 1 or 2.");
                continue;
        }

        turn++;

        if (!enemy.IsAlive)
        {
            Console.WriteLine($"{enemy.Name} is defeated!");
            enemies.RemoveAt(0);
            continue;
        }

        enemy.Attack(player, rng);
    }

    Console.WriteLine();
    if (player.IsAlive)
    {
        Console.WriteLine($"Victory in {turn} turns!");
        return turn;
    }

    Console.WriteLine("Defeat...");
    return 0;
}

void SaveResult(string line)
{
    try
    {
        File.AppendAllText("results.txt", line + Environment.NewLine);
        Console.WriteLine($"Saved: {line}");
    }
    catch (IOException e) // file locked by another program, disk full...
    {
        Console.WriteLine($"Could not save the result: {e.Message}");
    }
    catch (UnauthorizedAccessException e) // read-only file, protected folder
    {
        Console.WriteLine($"Could not save the result: {e.Message}");
    }
}
public class Player
{
    private int health;

    public string Name { get; }
    public int MaxHealth { get; }
    public int Potions { get; private set; }

    public event Action<string>? Died;

    public int Health
    {
        get { return health; }
        private set { health = Math.Clamp(value, 0, MaxHealth); }
    }

    public bool IsAlive => health > 0;

    public Player(string name, int maxHealth, int potions)
    {
        Name = name;
        MaxHealth = maxHealth;
        Potions = potions;
        Health = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        bool wasAlive = IsAlive;
        Health -= amount;
        Console.WriteLine($"{Name} takes {amount} damage ({Health} HP)");

        if (wasAlive && !IsAlive)
        {
            Died?.Invoke(Name);
        }
    }

    public void DrinkPotion()
    {
        if (Potions == 0)
        {
            Console.WriteLine("No potion left!");
            return;
        }

        Potions--;
        Health += 30;
        Console.WriteLine($"{Name} drinks a potion ({Health} HP)");
    }
}

public abstract class Enemy
{
    public string Name { get; }
    public int Health { get; private set; }
    public bool IsAlive => Health > 0;

    protected Enemy(string name, int health)
    {
        Name = name;
        Health = health;
    }

    public virtual void TakeDamage(int amount)
    {
        Health = Math.Max(Health - amount, 0);
        Console.WriteLine($"{Name} takes {amount} damage ({Health} HP)");
    }

    public abstract void Attack(Player target, Random rng);
}

public class Goblin : Enemy
{
    public Goblin() : base("Goblin", 30)
    {
    }

    public override void Attack(Player target, Random rng)
    {
        Console.WriteLine("Goblin stabs!");
        target.TakeDamage(rng.Next(4, 9));
    }
}

public class Orc : Enemy
{
    public Orc() : base("Orc", 60)
    {
    }

    public override void Attack(Player target, Random rng)
    {
        Console.WriteLine("Orc swings its club!");
        target.TakeDamage(rng.Next(8, 15));
    }
}

public class Boss : Enemy
{
    public int Armour { get; } = 6;

    public Boss() : base("Boss", 60)
    {
    }

    public override void TakeDamage(int amount)
    {
        int reduced = Math.Max(amount - Armour, 1);
        Console.WriteLine($"The armour absorbs {amount - reduced} damage.");
        base.TakeDamage(reduced);
    }

    public override void Attack(Player target, Random rng)
    {
        Console.WriteLine("Boss slams the ground!");
        target.TakeDamage(rng.Next(9, 17));
    }
}