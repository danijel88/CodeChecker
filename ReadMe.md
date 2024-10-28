
# CodeChecker

CodeChecker is a C# code analysis tool designed to detect code duplication and DRY (Don't Repeat Yourself) violations in a C# project. It parses C# files, tokenizes class members, and computes the similarity between classes and methods using Levenshtein distance. This tool can help maintain code quality by identifying redundant or duplicated code segments.

## Features
- Parses all C# files in a given project directory.
- Identifies similar classes and methods based on a customizable similarity threshold.
- Detects potential violations of the DRY principle.
- Supports comparison of code using Levenshtein distance for accurate detection of similarities.

## Getting Started

### Prerequisites
- .NET Core SDK (version 6 or higher)

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/CodeChecker.git
   ```

2. Navigate to the project directory:
   ```bash
   cd CodeChecker
   ```

3. Build the project:
   ```bash
   dotnet build
   ```

### Usage

On exist project add .dll write code as example
```c# 
string currentDirectory = Directory.GetCurrentDirectory();

// Navigate up the directory tree
string sourcePath = Path.GetFullPath(Path.Combine(currentDirectory, @"..\..\..\"));

var codeAnalyzer = new CodeAnalyzer();
codeAnalyzer.AnalyzeProject(sourcePath, 2,0);
```
Another option is to download from nuget:
```dotnet add package CodeCheckerCLI --version 1.0.0```
and use like:
```dotnet run -- "C:\\Path With Spaces\\To\\Project" 2 3
```

Where:
- `projectPath` is the path to the C# project directory.
- `similarityThreshold` is the threshold for identifying similar classes(2).
- `dryThreshold` is the threshold for detecting potential DRY violations in methods(3).

### Explanation of Similarity Thresholds
The `similarityThreshold` allows you to control how strict the comparison should be:

- **0**: Only exact duplicates.
- **1-2**: Allows for slight modifications such as minor renaming or formatting changes.
- **3 or more**: Detects even more lenient similarities, including structural changes.

The `dryThreshold` works similarly for methods and helps detect potential DRY violations.

### Example Output
When two similar classes or methods are found, you will see output like this:

```
Classes 'Class1' and 'Class2' are similar.
DRY violation: Method 'MethodA' in class 'Class1' is similar to 'MethodB' in class 'Class2'.
```

## License
This project is licensed under the MIT License - see the [[LICENSE](https://github.com/danijel88/CodeChecker/tree/master?tab=MIT-1-ov-file)](LICENSE) file for details.

## Contributions
Contributions, issues, and feature requests are welcome! Feel free to check out the [issues page](https://github.com/danijel88/CodeChecker/issues).
