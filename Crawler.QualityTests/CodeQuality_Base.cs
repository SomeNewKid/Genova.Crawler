// This file is part of the Genova project licensed under the GNU General Public License v3.0.
// See the LICENSE file in the project root for more information.

using System.Reflection;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Genova.Crawler.QualityTests;

[TestClass]
public abstract class CodeQuality_Base
{
    private readonly Assembly _assembly;
    private readonly string _expectedAssemblyName;

    protected CodeQuality_Base(Assembly assembly, string expectedAssemblyName)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(expectedAssemblyName);
        _assembly = assembly;
        _expectedAssemblyName = expectedAssemblyName;
    }

    [TestMethod]
    public void All_classes_and_records_should_be_non_public_unless_marked_as_Public()
    {
        Assert.AreEqual(_expectedAssemblyName, _assembly.GetName().Name);

        Type[] types = _assembly.GetTypes();

        foreach (Type type in types)
        {
            if (!type.IsClass && !type.IsValueType)
            {
                continue;
            }

            if (IsGeneratedType(type))
            {
                continue;
            }

            CodeQualityAttribute? attribute = CodeQualityAttribute.Find(type);

            bool isNonPublic = type.IsNotPublic || type.IsNestedPrivate;
            if (attribute == null || !attribute.Public)
            {
                string error = $"{type.FullName} is public but not marked [CodeQuality(Public = true)].";
                isNonPublic.Should().BeTrue(error);
            }
        }
    }

    [TestMethod]
    public void All_classes_should_be_sealed_unless_marked_as_Unsealed()
    {
        Assert.AreEqual(_expectedAssemblyName, _assembly.GetName().Name);

        Type[] types = _assembly.GetTypes();

        foreach (Type type in types)
        {
            if (!type.IsClass)
            {
                continue;
            }

            if (type.IsAbstract)
            {
                continue;
            }

            if (IsGeneratedType(type))
            {
                continue;
            }

            CodeQualityAttribute? attribute = CodeQualityAttribute.Find(type);

            if (attribute == null || !attribute.Unsealed)
            {
                string error = $"{type.FullName} is not sealed yet not marked [CodeQuality(Unsealed=true)].";
                type.IsSealed.Should().BeTrue(error);
            }
        }
    }

    [TestMethod]
    public void Process_all_folders()
    {
        string machineName = Environment.MachineName;
        if (!machineName.Equals("ALISTERSSPECTRE", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        string solutionFolder = FindSolutionFolder();

        string folderPrefix = _expectedAssemblyName.Replace("Genova.", "");
        foreach (string directory in Directory.GetDirectories(solutionFolder))
        {
            string dirPath = directory.Replace('/', '\\');
            if (!ShouldProcessDirectory(dirPath))
            {
                continue;
            }
            if (directory.StartsWith(solutionFolder + '\\' + folderPrefix))
            {
                Process_all_files_in_folder(directory);
            }
        }
    }

    private string FindSolutionFolder()
    {
        string solutionFileName = $"{_expectedAssemblyName}.sln";
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, solutionFileName)))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        Assert.Fail($"Solution folder containing '{solutionFileName}' could not be found.");
        throw new InvalidOperationException();
    }

    private static bool ShouldProcessDirectory(string dirPath)
    {
        string[] segments = dirPath.Split('\\', StringSplitOptions.RemoveEmptyEntries);
        foreach (string segment in segments)
        {
            if (segment.Equals("bin", StringComparison.OrdinalIgnoreCase) ||
                segment.Equals("obj", StringComparison.OrdinalIgnoreCase) ||
                segment.StartsWith('.') ||
                segment.StartsWith('~'))
            {
                return false;
            }
        }
        return true;
    }

    private static void File_contains_expected_header(string file, string[] lines)
    {
        string header =
            "// This file is part of the Genova project licensed under the GNU General Public License v3.0.";
        bool found = false;
        foreach (string line in lines)
        {
            if (line.StartsWith(header, StringComparison.Ordinal))
            {
                found = true;
                break;
            }
        }

        Assert.IsTrue(found, $"File did not have expected header: {file}");
    }

    private static void File_has_system_usings_first_and_alphabetical(string file, string[] lines)
    {
        List<string> systemUsings = [];
        bool foundNonSystemUsing = false;

        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (trimmed.StartsWith("using ", StringComparison.Ordinal) && trimmed.EndsWith(';'))
            {
                if (trimmed.StartsWith("using System", StringComparison.Ordinal))
                {
                    if (foundNonSystemUsing)
                    {
                        Assert.Fail(
                            $"File '{file}' has a 'using System' statement after a non-System using statement.");
                    }
                    string withoutSemicolon = trimmed.EndsWith(';') ? trimmed[..^1].TrimEnd() : trimmed;
                    systemUsings.Add(withoutSemicolon);
                }
                else
                {
                    foundNonSystemUsing = true;
                }
            }

            if (trimmed.StartsWith("namespace ", StringComparison.Ordinal) ||
                trimmed.StartsWith("public ", StringComparison.Ordinal) ||
                trimmed.StartsWith("internal ", StringComparison.Ordinal) ||
                trimmed.StartsWith("class ", StringComparison.Ordinal) ||
                trimmed.StartsWith("record ", StringComparison.Ordinal))
            {
                break;
            }
        }

        List<string> sortedSystemUsings = [.. systemUsings];
        sortedSystemUsings.Sort(StringComparer.Ordinal);
        for (int i = 0; i < systemUsings.Count; i++)
        {
            if (systemUsings[i] != sortedSystemUsings[i])
            {
                Assert.Fail(
                    $"File '{file}' has 'using System' statements that are not in alphabetical order.");
            }
        }
    }

    private static void File_has_non_system_usings_alphabetical(string file, string[] lines)
    {
        List<string> nonSystemUsings = [];

        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (trimmed.StartsWith("using ", StringComparison.Ordinal) && trimmed.EndsWith(';'))
            {
                if (!trimmed.StartsWith("using System", StringComparison.Ordinal))
                {
                    string withoutSemicolon = trimmed.EndsWith(';') ? trimmed[..^1].TrimEnd() : trimmed;
                    nonSystemUsings.Add(withoutSemicolon);
                }
            }

            if (trimmed.StartsWith("namespace ", StringComparison.Ordinal) ||
                trimmed.StartsWith("public ", StringComparison.Ordinal) ||
                trimmed.StartsWith("internal ", StringComparison.Ordinal) ||
                trimmed.StartsWith("class ", StringComparison.Ordinal) ||
                trimmed.StartsWith("record ", StringComparison.Ordinal))
            {
                break;
            }
        }

        List<string> sortedNonSystemUsings = [.. nonSystemUsings];
        sortedNonSystemUsings.Sort(StringComparer.Ordinal);
        for (int i = 0; i < nonSystemUsings.Count; i++)
        {
            if (nonSystemUsings[i] != sortedNonSystemUsings[i])
            {
                Assert.Fail(
                    $"File '{file}' has non-System using statements that are not in alphabetical order.");
            }
        }
    }

    private static void File_has_no_usings_after_namespace(string file, string[] lines)
    {
        bool namespaceFound = false;
        for (int i = 0; i < lines.Length; i++)
        {
            string trimmed = lines[i].Trim();
            if (trimmed.StartsWith("namespace ", StringComparison.Ordinal))
            {
                namespaceFound = true;
            }
            else if (trimmed.Contains('{'))
            {
                break;
            }
            else if (namespaceFound && trimmed.StartsWith("using ", StringComparison.Ordinal) && trimmed.EndsWith(';'))
            {
                Assert.Fail(
                    $"File '{file}' has a using directive after the namespace declaration at line {i + 1}.");
            }
        }
    }

    private void File_contains_expected_namespace(string file, string[] lines)
    {
        bool found = false;
        foreach (string line in lines)
        {
            if (line.StartsWith($"namespace {_expectedAssemblyName}", StringComparison.Ordinal))
            {
                found = true;
                break;
            }
        }

        Assert.IsTrue(found, $"File did not have expected namespace: {file}");
    }

    private void Process_all_files_in_folder(string directory)
    {
        string[] files = Directory.GetFiles(directory, "*.cs", SearchOption.TopDirectoryOnly);
        foreach (string file in files)
        {
            if (file.EndsWith(".feature.cs", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string fileContent = File.ReadAllText(file);
            string[] lines = fileContent.Split(["\r\n", "\n", "\r"], StringSplitOptions.None);

            if (!string.Equals(Path.GetFileName(file), "AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(Path.GetFileName(file), "MSTestSettings.cs", StringComparison.OrdinalIgnoreCase))
            {
                File_contains_expected_namespace(file, lines);
                File_contains_expected_header(file, lines);
                File_has_system_usings_first_and_alphabetical(file, lines);
                File_has_non_system_usings_alphabetical(file, lines);
                File_has_no_usings_after_namespace(file, lines);
            }
        }

        string[] subdirectories = Directory.GetDirectories(directory);
        foreach (string subdirectory in subdirectories)
        {
            if (ShouldProcessDirectory(subdirectory))
            {
                Process_all_files_in_folder(subdirectory);
            }
        }
    }

    private static bool IsGeneratedType(Type type)
    {
        return type.ToString().Contains("<>") ||
            type.ToString().Contains("<PrivateImplementationDetails>") ||
            type.ToString().Contains("AspNetCoreGeneratedDocument") ||
            type.GetCustomAttribute<CompilerGeneratedAttribute>() != null;
    }

    private class CodeQualityAttribute
    {
        public bool Public { get; }
        public bool Unsealed { get; }

        private CodeQualityAttribute(bool isPublic, bool isUnsealed)
        {
            Public = isPublic;
            Unsealed = isUnsealed;
        }

        internal static CodeQualityAttribute? Find(Type type)
        {
            const string fqName = "Genova.Common.Attributes.CodeQualityAttribute";
            object? attr = type.GetCustomAttributes(false)
                .FirstOrDefault(a => a.GetType().FullName == fqName);

            if (attr == null)
            {
                return null;
            }

            Type attrType = attr.GetType();
            PropertyInfo? publicProp = attrType.GetProperty("Public");
            PropertyInfo? unsealedProp = attrType.GetProperty("Unsealed");

            bool isPublic = publicProp != null && publicProp.GetValue(attr) is bool b1 && b1;
            bool isUnsealed = unsealedProp != null && unsealedProp.GetValue(attr) is bool b2 && b2;

            return new CodeQualityAttribute(isPublic, isUnsealed);
        }
    }
}
