using System;
using System.IO;
using System.Text.RegularExpressions;

public class NamespaceFixer
{
    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: NamespaceFixer <directory>");
            return;
        }

        string directory = args[0];
        if (!Directory.Exists(directory))
        {
            Console.WriteLine($"Directory not found: {directory}");
            return;
        }

        ProcessDirectory(directory);
    }

    private static void ProcessDirectory(string targetDirectory)
    {
        string[] csFiles = Directory.GetFiles(targetDirectory, "*.cs", SearchOption.AllDirectories);

        foreach (string filePath in csFiles)
        {
            ProcessFile(filePath);
        }
    }

    private static void ProcessFile(string filePath)
    {
        string content = File.ReadAllText(filePath);
        string originalContent = content;

        // Add "Solo" to the namespace
        content = Regex.Replace(content, @"namespace ExtremeRoles\.Roles\.Crewmate", "namespace ExtremeRoles.Roles.Solo.Crewmate");
        content = Regex.Replace(content, @"namespace ExtremeRoles\.Roles\.Impostor", "namespace ExtremeRoles.Roles.Solo.Impostor");
        content = Regex.Replace(content, @"namespace ExtremeRoles\.Roles\.Neutral", "namespace ExtremeRoles.Roles.Solo.Neutral");
        content = Regex.Replace(content, @"namespace ExtremeRoles\.Roles\.Host", "namespace ExtremeRoles.Roles.Solo.Host");

        // Fix using statements
        content = Regex.Replace(content, @"using ExtremeRoles\.Roles\.Crewmate", "using ExtremeRoles.Roles.Solo.Crewmate");
        content = Regex.Replace(content, @"using ExtremeRoles\.Roles\.Impostor", "using ExtremeRoles.Roles.Solo.Impostor");
        content = Regex.Replace(content, @"using ExtremeRoles\.Roles\.Neutral", "using ExtremeRoles.Roles.Solo.Neutral");
        content = Regex.Replace(content, @"using ExtremeRoles\.Roles\.Host", "using ExtremeRoles.Roles.Solo.Host");

        // Fix fully qualified names
        content = Regex.Replace(content, @"ExtremeRoles\.Roles\.Solo\.Solo\.", "ExtremeRoles.Roles.Solo.");


        if (content != originalContent)
        {
            File.WriteAllText(filePath, content);
            Console.WriteLine($"Updated: {filePath}");
        }
    }
}
