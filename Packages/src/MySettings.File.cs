using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static partial class MySettings
{
    private static class File
    {
        private static readonly UTF8Encoding _encoding = new(false, true);

        private static void Commit(string tempPath, string path)
        {
            if (System.IO.File.Exists(path))
                System.IO.File.Replace(tempPath, path, null);
            else
                System.IO.File.Move(tempPath, path);
        }

        private static string CreateTempPath(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            return path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        }

        private static string Escape(string value, bool escapeSeparator)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var text = value.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n");
            return escapeSeparator ? text.Replace("=", "\\=") : text;
        }

        private static int FindSeparator(string value)
        {
            for (var index = 0; index < value.Length; ++index)
            {
                if (value[index] == '\\')
                    ++index;
                else if (value[index] == '=')
                    return index;
            }

            return -1;
        }

        public static Dictionary<string, string> Read(string path)
        {
            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            string[] lines;
            try
            {
                lines = System.IO.File.ReadAllLines(path, _encoding);
            }
            catch (FileNotFoundException)
            {
                return values;
            }
            catch (DirectoryNotFoundException)
            {
                return values;
            }

            for (var index = 0; index < lines.Length; ++index)
            {
                var line = lines[index];
                if (line.Length == 0)
                    continue;

                var separator = FindSeparator(line);
                if (separator < 0)
                    throw new InvalidDataException("Settings entry is missing '=' on line " + (index + 1) + ".");

                var key = Unescape(line[..separator]);
                if (values.ContainsKey(key))
                    throw new InvalidDataException("Duplicate settings key on line " + (index + 1) + ".");

                values.Add(key, Unescape(line[(separator + 1)..]));
            }

            return values;
        }

        private static string Serialize(Dictionary<string, string> values)
        {
            var text = new StringBuilder();
            foreach (var value in values.OrderBy(value => value.Key, StringComparer.Ordinal))
            {
                text.Append(Escape(value.Key, true));
                text.Append('=');
                text.Append(Escape(value.Value, false));
                text.AppendLine();
            }

            return text.ToString();
        }

        private static string Unescape(string value)
        {
            var text = new StringBuilder(value.Length);
            for (var index = 0; index < value.Length; ++index)
            {
                var character = value[index];
                if (character == '\\')
                {
                    ++index;
                    if (index == value.Length)
                        throw new InvalidDataException("Settings entry ends with an incomplete escape sequence.");

                    character = value[index] switch
                    {
                        '\\' => '\\',
                        '=' => '=',
                        'n' => '\n',
                        'r' => '\r',
                        _ => throw new InvalidDataException("Settings entry contains an unknown escape sequence."),
                    };
                }

                text.Append(character);
            }

            return text.ToString();
        }

        public static void Write(string path, Dictionary<string, string> values)
        {
            path = Path.GetFullPath(path);
            var text = Serialize(values);
            var tempPath = CreateTempPath(path);
            try
            {
                System.IO.File.WriteAllText(tempPath, text, _encoding);
                Commit(tempPath, path);
            }
            finally
            {
                if (System.IO.File.Exists(tempPath))
                    System.IO.File.Delete(tempPath);
            }
        }

        public static async Task WriteAsync(string path, Dictionary<string, string> values)
        {
            path = Path.GetFullPath(path);
            var text = Serialize(values);
            var tempPath = CreateTempPath(path);
            try
            {
                await System.IO.File.WriteAllTextAsync(tempPath, text, _encoding).ConfigureAwait(false);
                Commit(tempPath, path);
            }
            finally
            {
                if (System.IO.File.Exists(tempPath))
                    System.IO.File.Delete(tempPath);
            }
        }
    }
}
