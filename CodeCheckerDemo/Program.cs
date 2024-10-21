// See https://aka.ms/new-console-template for more information

using CodeChecker;
using CodeCheckerDemo;


string currentDirectory = Directory.GetCurrentDirectory();

// Navigate up the directory tree
string sourcePath = Path.GetFullPath(Path.Combine(currentDirectory, @"..\..\..\"));

var codeAnalyzer = new CodeAnalyzer();
codeAnalyzer.AnalyzeProject(sourcePath, 2,0);
Console.ReadKey();



