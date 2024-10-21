using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using CodeChecker;
using Microsoft.CodeAnalysis;
using Xunit;
using Microsoft.VisualStudio.TestPlatform.Utilities;

public class CodeAnalyzerTests
{
    [Fact]
    public void AnalyzeProject_NoClasses_NoSimilaritiesDetected()
    {
        // Arrange
        var codeAnalyzer = new CodeAnalyzer();
        var projectPath = CreateEmptyProject();  // Creates an empty project with no C# files
        int similarityThreshold = 5;
        int dryViolationThreshold = 10;

        // Act
        var ex = Record.Exception(() => codeAnalyzer.AnalyzeProject(projectPath, similarityThreshold, dryViolationThreshold));

        // Assert
        Assert.Null(ex); // No exceptions should be thrown during analysis
        // Assert that no output was printed regarding class similarities
    }

    [Fact]
    public void AnalyzeProject_SimilarClasses_SimilarityDetected()
    {
        // Arrange
        var codeAnalyzer = new CodeAnalyzer();
        var projectPath = CreateProjectWithSimilarClasses(); // Creates a project with two very similar classes
        int similarityThreshold = 2;
        int dryViolationThreshold = 0;

        // Act
        using (var consoleOutput = new ConsoleOutput())
        {
            codeAnalyzer.AnalyzeProject(projectPath, similarityThreshold, dryViolationThreshold);

            // Assert
            Assert.Contains("Classes", consoleOutput.GetOuput());
            Assert.Contains("are similar", consoleOutput.GetOuput());
        }
    }

    [Fact]
    public void AnalyzeProject_DryViolationsDetected()
    {
        // Arrange
        var codeAnalyzer = new CodeAnalyzer();
        var projectPath = CreateProjectWithDryViolations(); // Creates a project with potential DRY violations
        int similarityThreshold = 5;
        int dryViolationThreshold = 0;

        // Act
        using (var consoleOutput = new ConsoleOutput())
        {
            codeAnalyzer.AnalyzeProject(projectPath, similarityThreshold, dryViolationThreshold);

            // Assert
            Assert.Contains("DRY violation", consoleOutput.GetOuput());
        }
    }

    // Utility methods for setting up test projects
    private string CreateEmptyProject()
    {
        // Mock or create an empty project directory for testing
        var path = Path.Combine(Path.GetTempPath(), "EmptyProject");
        Directory.CreateDirectory(path);
        return path;
    }

    private string CreateProjectWithSimilarClasses()
    {
        // Create a temp project directory with two similar C# class files
        var path = Path.Combine(Path.GetTempPath(), "SimilarClassesProject");
        Directory.CreateDirectory(path);

        var class1 = @"
            namespace TestProject
            {
                public class Class1
                {
                    public void MethodA() { }
                }
            }";
        var class2 = @"
            namespace TestProject
            {
                public class Class2
                {
                    public void MethodA() { }
                }
            }";

        File.WriteAllText(Path.Combine(path, "Class1.cs"), class1);
        File.WriteAllText(Path.Combine(path, "Class2.cs"), class2);

        return path;
    }

    private string CreateProjectWithDryViolations()
    {
        // Create a temp project directory with methods that violate DRY principle
        var path = Path.Combine(Path.GetTempPath(), "DryViolationsProject");
        Directory.CreateDirectory(path);

        var class1 = @"
            namespace TestProject
            {
                public class Class1
                {
                    public void MethodA() 
                    {
                        for(int i=0;i <= 10;i++)
                        {
                        }
                        // Some repetitive logic here
                    }
                }
            }";
        var class2 = @"
            namespace TestProject
            {
                public class Class2
                {
                    public void MethodA() 
                    {
                        // Same repetitive logic as Class1.MethodA
                        for(int i=0;i <= 10;i++)
                        {
                        }
                    }
                }
            }";

        File.WriteAllText(Path.Combine(path, "Class1.cs"), class1);
        File.WriteAllText(Path.Combine(path, "Class2.cs"), class2);

        return path;
    }

    // Helper class to capture console output for assertions
    public class ConsoleOutput : IDisposable
    {
        private StringWriter _stringWriter;
        private TextWriter _originalOutput;

        public ConsoleOutput()
        {
            _stringWriter = new StringWriter();
            _originalOutput = Console.Out;
            Console.SetOut(_stringWriter);
        }

        public string GetOuput()
        {
            return _stringWriter.ToString();
        }

        public void Dispose()
        {
            Console.SetOut(_originalOutput);
            _stringWriter.Dispose();
        }
    }
}
