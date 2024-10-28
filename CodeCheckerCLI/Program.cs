using CodeChecker; // Reference your library



if (args.Length < 3)
{
    Console.WriteLine("Usage: CodeCheckerCLI <projectPath> <similarityThreshold> <dryThreshold>");
    return;
}


string projectPath = "C:\\Users\\boksand-adm\\source\\repos\\PDM\\PDM";

if (!Directory.Exists(projectPath))
{
    Console.WriteLine($"Error: The specified path '{projectPath}' does not exist.");
    return;
}
if (!int.TryParse(args[1], out int similarityThreshold) || !int.TryParse(args[2], out int dryThreshold))
{
    Console.WriteLine("Error: similarityThreshold and dryThreshold should be integers.");
    return;
}

var analyzer = new CodeAnalyzer();
analyzer.AnalyzeProject(projectPath, similarityThreshold, dryThreshold);

Console.WriteLine("Code analysis complete.");
Console.ReadKey();