using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Text;
using Mana.Graphics.Shaders;
using Mana.Utilities;

namespace Mana.Asset.Loaders
{
    public static class ShaderPreprocessor
    {
        private static Logger _log = Logger.Create();
        private static ReadOnlyMemory<char> _includeCompare = "#include ".AsMemory();

        /// <summary>
        /// Loads and pre-processes a shader source file.
        /// </summary>
        /// <param name="assetManager">The AssetManager to be used to resolve the asset paths.</param>
        /// <param name="shaderPath">The path of the shader source file.</param>
        /// <param name="shaderIncludePaths">The resulting list of included file paths referenced by the shader.</param>
        public static string Process(AssetManager assetManager,
                                     string shaderPath,
                                     out List<string> shaderIncludePaths)
        {
            // Uses ZString as a StringBuilder alternative to reduce heap allocations.
            // Uses ReadOnlyMemory<char> instead of string where possible for slightly better performance.

            // TODO: This needs more testing with larger asset libraries down the road.
            // System.Text.StringBuilder is slightly faster in my testing, but uses a lot more heap allocations.
            // If the time difference is non-negligible for larger asset files, it may be beneficial to switch back to
            // System.Text.StringBuilder since shaders are only pre-processed when they're loaded, so the heap
            // allocations aren't really an issue.

            Utf16ValueStringBuilder stringBuilder = ZString.CreateStringBuilder();

            shaderIncludePaths = new List<string>();

            PreprocessShaderSource(assetManager,
                                    ref stringBuilder,
                                    shaderPath,
                                    shaderIncludePaths = new List<string>(),
                                    new Stack<string>());

            string result = stringBuilder.ToString();

            return result;
        }

        private static bool TryParseIncludeLine(ReadOnlyMemory<char> line, out ReadOnlyMemory<char> includePath)
        {
            int index = 9;
            int length = -1;
            bool completed = false;

            for (int i = index; i < line.Length; i++)
            {
                char c = line.Span[i];

                if (i == 9 && c != '\"')
                {
                    includePath = ReadOnlyMemory<char>.Empty;
                    return false;
                }

                if (c == '\"')
                {
                    if (completed)
                    {
                        includePath = ReadOnlyMemory<char>.Empty;
                        return false;
                    }

                    if (index == 9)
                    {
                        index = i + 1;
                    }
                    else
                    {
                        completed = true;
                        length = i - index;
                    }
                }
            }

            if (length == -1)
            {
                includePath = ReadOnlyMemory<char>.Empty;
                return false;
            }

            includePath = line.Slice(index, length);
            return true;
        }

        private static void PreprocessShaderSource(AssetManager assetManager,
                                                   ref Utf16ValueStringBuilder stringBuilder,
                                                   string shaderPath,
                                                   List<string> shaderIncludePaths,
                                                   Stack<string> includeStack)
        {
            string shaderSource = GetShaderSource(assetManager, shaderPath);

            // Skip pre-processing if the string doesn't have any include directives.
            if (!shaderSource.Contains("#include "))
            {
                stringBuilder.Append(shaderSource);
                return;
            }

            // Add current include path to the stack so we can book-keep to prevent circular dependencies.
            includeStack.Push(shaderPath);

            int lineNumber = 1;

            using var lineReader = new LineReader(shaderSource);

            for (ReadOnlyMemory<char> line = lineReader.ReadLine(); !lineReader.Finished; line = lineReader.ReadLine())
            {
                // Check if the line starts with "#include "
                if (line.Span.StartsWith(_includeCompare.Span))
                {
                    if (TryParseIncludeLine(line, out var includeFile))
                    {
                        string includePath = Path.Combine(Path.GetDirectoryName(shaderPath) ?? throw new Exception(), includeFile.ToString());

                        if (includeStack.Contains(includePath))
                            throw new ShaderPreprocessException($"Include statement would introduce cyclic dependency: {includeFile}\n" +
                                                                $"Parent: \"{shaderPath}\"");

                        shaderIncludePaths.Add(includePath);

                        stringBuilder.Append("#line 1 ");
                        stringBuilder.AppendLine((shaderIncludePaths.IndexOf(includePath) + 1).ToString());

                        PreprocessShaderSource(assetManager,
                                               ref stringBuilder,
                                               includePath,
                                               shaderIncludePaths,
                                               includeStack);

                        stringBuilder.Append("\n#line ");
                        stringBuilder.Append(lineNumber);
                        stringBuilder.AppendLine((shaderIncludePaths.IndexOf(includePath) + 1).ToString());
                    }
                    else
                    {
                        throw new ShaderPreprocessException($"Error when parsing include statement: {shaderPath}:{lineNumber}");
                    }
                }
                else
                {
                    stringBuilder.Append(line);
                    lineNumber++;
                }

                stringBuilder.Append('\n');
            }

            includeStack.Pop();
        }

        private static string GetShaderSource(AssetManager assetManager, string shaderPath)
        {
            using var stream = assetManager.GetStreamFromPath(shaderPath);
            using var streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }
    }
}
