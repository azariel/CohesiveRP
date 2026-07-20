using System.Text;

namespace CohesiveRP.Common.Utils
{
    public static class FileUtils
    {
        public static string SanitizeNameForWindowsPath(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Guid.NewGuid().ToString();

            var invalidChars = Path.GetInvalidFileNameChars();

            var sb = new StringBuilder(name.Length);
            foreach (char c in name)
            {
                // Windows also rejects control characters.
                if (c < 32 || invalidChars.Contains(c))
                {
                    sb.Append('_');
                } else
                {
                    sb.Append(c);
                }
            }

            string result = sb.ToString();

            // Windows does not allow trailing dots or spaces in file/folder names.
            result = result.TrimEnd(' ', '.');

            // Avoid reserved device names, even with extensions.
            string baseName = result;
            int dotIndex = baseName.IndexOf('.');
            if (dotIndex >= 0)
                baseName = baseName.Substring(0, dotIndex);

            string upperBaseName = baseName.ToUpperInvariant();
            string[] reservedNames =
            {
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };

            if (reservedNames.Contains(upperBaseName))
                result = "_" + result;

            if (string.IsNullOrWhiteSpace(result))
                return "_";

            return result;
        }
    }
}
