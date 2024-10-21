namespace CodeChecker
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class CodeAnalyzer
    {
        private List<SyntaxTree> ParseProjectFiles(string projectPath)
        {
            var syntaxTrees = new List<SyntaxTree>();

            if (!Directory.Exists(projectPath))
            {
                Console.WriteLine($"Error: The specified path '{projectPath}' does not exist");
                return syntaxTrees;
            }

            var files = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                try
                {
                    Console.WriteLine($"Parsing file: {file}");
                    var code = File.ReadAllText(file);
                    var syntaxTree = CSharpSyntaxTree.ParseText(code);
                    syntaxTrees.Add(syntaxTree);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading or parsing file '{file}': {ex.Message}");
                }

            }

            return syntaxTrees;

        }
        // <summary>
        // Tokenizes the members of a class, including methods, fields, and properties.
        // </summary>
        // <param name="classDeclaration">The class declaration to tokenize.</param>
        // <returns>A collection of normalized tokens representing the members of the class.</returns>
        private IEnumerable<string> TokenizeClassMembers(ClassDeclarationSyntax classDeclaration)
        {
            var tokens = new List<string>();

            try
            {
                // Get method declarations and add their identifiers to the token list
                var methodDeclarations = classDeclaration.Members.OfType<MethodDeclarationSyntax>();
                foreach (var methodDeclaration in methodDeclarations)
                {
                    tokens.Add(methodDeclaration.Identifier.Text);
                }

                // Get field declarations and add their identifiers to the token list
                var fieldDeclarations = classDeclaration.Members.OfType<FieldDeclarationSyntax>();
                foreach (var field in fieldDeclarations)
                {
                    var variableDeclarators = field.Declaration.Variables;
                    foreach (var variable in variableDeclarators)
                    {
                        tokens.Add(variable.Identifier.Text);
                    }
                }

                // Get property declarations and add their identifiers to the token list
                var propertyDeclarations = classDeclaration.Members.OfType<PropertyDeclarationSyntax>();
                foreach (var property in propertyDeclarations)
                {
                    tokens.Add(property.Identifier.Text);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions such as null reference
                Console.WriteLine($"Error while tokenizing class members: {ex.Message}");
            }

            return tokens;
        }
        private string NormalizeToken(SyntaxToken token)
        {
     
            // Normalize numeric literals to a common label
             if (token.IsKind(SyntaxKind.NumericLiteralToken))
            {
                return "NUMBER";
            }
            // Normalize loop keywords (for, while, do)
            else if (token.IsKind(SyntaxKind.ForKeyword) || token.IsKind(SyntaxKind.WhileKeyword) || token.IsKind(SyntaxKind.DoKeyword))
            {
                return "LOOP"; // Normalize all loop types
            }
            // Handle other literals, if necessary
            else if (token.IsKind(SyntaxKind.CharacterLiteralToken))
            {
                return "CHARACTER"; // Generalize character literals to "CHARACTER"
            }
            // Handle string literals specifically to remove surrounding quotes
            else if (token.IsKind(SyntaxKind.StringLiteralToken))
            {
                return token.Text.Trim('"'); // Remove quotes from the string literal
            }
            else if (token.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                     token.IsKind(SyntaxKind.MultiLineCommentTrivia) ||
                     token.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                     token.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
            {
                return "COMMENTS";
            }

            return token.Text;
        }
        private int ComputeLevenshteinDistance(string[] sourceTokens, string[] targetTokens)
        {
            if (sourceTokens.Length == 0) return targetTokens.Length;
            if (targetTokens.Length == 0) return sourceTokens.Length;

            var matrix = new int[sourceTokens.Length + 1, targetTokens.Length + 1];

            // Initialize the matrix
            for (int i = 0; i <= sourceTokens.Length; i++)
                matrix[i, 0] = i;

            for (int j = 0; j <= targetTokens.Length; j++)
                matrix[0, j] = j;

            // Compute Levenshtein distance
            for (int i = 1; i <= sourceTokens.Length; i++)
            {
                for (int j = 1; j <= targetTokens.Length; j++)
                {
                    int cost = (sourceTokens[i - 1] == targetTokens[j - 1]) ? 0 : 1;

                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1), // Deletion and insertion
                        matrix[i - 1, j - 1] + cost // Substitution
                    );
                }
            }

            return matrix[sourceTokens.Length, targetTokens.Length];
        }
        private int ComputeLevenshteinDistance(string source, string target)
        {
            // Split the input strings into tokens using whitespace as a delimiter
            var sourceTokens = source.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var targetTokens = target.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            if (sourceTokens.Length == 0) return targetTokens.Length;
            if (targetTokens.Length == 0) return sourceTokens.Length;

            var matrix = new int[sourceTokens.Length + 1, targetTokens.Length + 1];

            // Initialize the matrix
            for (int i = 0; i <= sourceTokens.Length; i++)
                matrix[i, 0] = i;

            for (int j = 0; j <= targetTokens.Length; j++)
                matrix[0, j] = j;

            // Compute Levenshtein distance
            for (int i = 1; i <= sourceTokens.Length; i++)
            {
                for (int j = 1; j <= targetTokens.Length; j++)
                {
                    int cost = (sourceTokens[i - 1] == targetTokens[j - 1]) ? 0 : 1;

                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1), // Deletion and insertion
                        matrix[i - 1, j - 1] + cost // Substitution
                    );
                }
            }

            return matrix[sourceTokens.Length, targetTokens.Length];
        }
        private bool AreClassesSimilar(ClassDeclarationSyntax class1, ClassDeclarationSyntax class2, int similarityThreshold)
        {
            var tokens1 = TokenizeClassMembers(class1).ToArray();
            var tokens2 = TokenizeClassMembers(class2).ToArray();

            // If either class has no tokens, they are not similar
            if (!tokens1.Any() || !tokens2.Any())
            {
                return false;
            }

            int distance = ComputeLevenshteinDistance(tokens1, tokens2);

            return distance <= similarityThreshold;
        }
        /// <summary>
        /// Analyzes the specified C# project by parsing its files, extracting class declarations, and comparing them for similarity.
        /// It also checks for potential DRY (Don't Repeat Yourself) violations across the project.
        /// </summary>
        /// <param name="projectPath">The file path of the C# project to be analyzed.</param>
        /// <param name="similarityThreshold">The threshold level for identifying similar classes, allowing different levels of leniency for code duplication.</param>
        /// <param name="dyiSimilarityThreshold">The threshold for identifying potential DRY (Don't Repeat Yourself) principle violations, with varying degrees of similarity tolerance.</param>
        public void AnalyzeProject(string projectPath, int similarityThreshold,int dyiSimilarityThreshold)
        {
            // Get the syntax trees from the project files
            var projectFiles = ParseProjectFiles(projectPath);
            var classes = new List<ClassDeclarationSyntax>();

            // Iterate over the syntax trees and extract class declarations
            foreach (var syntaxTree in projectFiles)
            {
                var root = syntaxTree.GetRoot(); // Get the root of the syntax tree

                // Find all class declarations in the syntax tree
                var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                classes.AddRange(classDeclarations);
            }

            // Compare each class with every other class
            for (int i = 0; i < classes.Count; i++)
            {
                for (int j = i + 1; j < classes.Count; j++)
                {
                    var class1 = classes[i];
                    var class2 = classes[j];

                    if (AreClassesSimilar(class1, class2, similarityThreshold))
                    {
                        Console.WriteLine($"Classes '{class1.Identifier}' and '{class2.Identifier}' are similar.");
                    }
                }
            }
            CheckForDryViolations(projectFiles, dyiSimilarityThreshold);
        }
        private void CheckForDryViolations(List<SyntaxTree> projectFiles, int similarityThreshold)
        {
            var methodBodies = new Dictionary<string, (string ClassName, string MethodBody)>();

            foreach (var syntaxTree in projectFiles)
            {
                var root = syntaxTree.GetRoot();
                var methodDeclarations = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

                foreach (var method in methodDeclarations)
                {
                    // Tokenize the method body
                    var methodBody = method.Body?.ToString() ?? string.Empty;
                    var methodName = method.Identifier.Text;

                    // Get the class name containing this method
                    var classDeclaration = method.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
                    var className = classDeclaration?.Identifier.Text ?? "UnknownClass";

                    // Normalize the method body for comparison
                    var normalizedBody = NormalizeMethodBody(methodBody);

                    // Check for duplicates
                    foreach (var entry in methodBodies)
                    {
                        var (otherClassName, otherMethodBody) = entry.Value;
                        if (AreBodiesSimilar(normalizedBody, otherMethodBody, similarityThreshold))
                        {
                            Console.WriteLine($"DRY violation: Method '{methodName}'in class '{className}' is the same or similar like '{entry.Key}' in class '{otherClassName}'");
                        }
                    }

                    // Store the method body in the dictionary, using the method name and class name as the key
                    var fullMethodName = $"{className}.{methodName}";
                    methodBodies[fullMethodName] = (className, normalizedBody);
                }
            }
        }
        private string NormalizeMethodBody(string methodBody)
        {
            var withoutComments = RemoveComments(methodBody);
            var withoutWhiteSpace = RemoveExtraWhitespace(withoutComments);
            var identifier = SyntaxFactory.Identifier(withoutWhiteSpace);
            var normalized = NormalizeToken(identifier);
            // Implement normalization logic here (e.g., trim, remove comments, etc.)
            // Simulating fetching a user from a database
            // This is a simple example; you may want to use a more sophisticated approach.
            return normalized
                .Replace(" ", "")
                .Replace("\n", "")
                .Replace("\r", "")
                .Replace("COMMENTS","")
                .Replace("LOOP","")
                .Replace("NUMBER", "")
                .Replace("CHARACTER", "")
                .Trim();
        }
        private bool AreBodiesSimilar(string body1, string body2, int similarityThreshold)
        {
            // Compute Levenshtein distance or any other similarity measure
            int distance = ComputeLevenshteinDistance(body1, body2);
            return distance <= similarityThreshold;
        }
        private string RemoveComments(string code)
        {
            // Regex to match single-line comments and multi-line comments
            string singleLineCommentPattern = @"//.*?$";
            string multiLineCommentPattern = @"/\*.*?\*/";

            // Remove single-line comments
            string withoutSingleLineComments = System.Text.RegularExpressions.Regex.Replace(code, singleLineCommentPattern, string.Empty, System.Text.RegularExpressions.RegexOptions.Multiline);

            // Remove multi-line comments
            string withoutComments = System.Text.RegularExpressions.Regex.Replace(withoutSingleLineComments, multiLineCommentPattern, string.Empty, System.Text.RegularExpressions.RegexOptions.Singleline);

            return withoutComments;
        }
        private string RemoveExtraWhitespace(string code)
        {
            // Remove leading/trailing whitespace, reduce multiple spaces/tabs to a single space, and remove newlines
            string withoutWhitespace = System.Text.RegularExpressions.Regex.Replace(code, @"\s+", " ");

            // Trim any leading/trailing spaces
            return withoutWhitespace.Trim();
        }

    }


}
