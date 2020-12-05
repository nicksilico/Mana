using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Mana.IO
{
    /// <summary>
    /// A static helper class containing methods for retrieving game directory paths across all three major operating
    /// systems.
    /// </summary>
    public static class PathsHelper
    {
        /// <summary>
        /// Gets the recommended directory for storing save data for the current operating system.
        /// </summary>
        /// <param name="applicationName">The application name.</param>
        /// <param name="companyName">The company name, or null to not use a company subdirectory.</param>
        /// <returns>the recommended directory for storing game-specific save data for the current operating system.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the method is called on a platform that isn't
        /// Windows, Linux, or OSX.
        /// </exception>
        public static string GetSaveDataDirectory(string applicationName, string companyName = null)
        {
            string path = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                path = GetWindowsPath("Saves", applicationName, companyName);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                path = GetLinuxSaveDirectory(applicationName, companyName);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                path = GetOSXApplicationSupportPath("Saves", applicationName, companyName);
            }

            if (path == null)
            {
                throw new InvalidOperationException("This method is not supported on the current platform.");
            }

            Directory.CreateDirectory(path);

            return path;
        }

        /// <summary>
        /// Gets the recommended directory for storing config data for the current operating system.
        /// </summary>
        /// <param name="applicationName">The application name.</param>
        /// <param name="companyName">The company name, or null to not use a company subdirectory.</param>
        /// <returns>the recommended directory for storing game-specific save data for the current operating system.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the method is called on a platform that isn't
        /// Windows, Linux, or OSX.
        /// </exception>
        public static string GetConfigDirectory(string applicationName, string companyName = null)
        {
            string path = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                path = GetWindowsPath("Config", applicationName, companyName);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                path = GetLinuxConfigDirectory(applicationName, companyName);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                path = GetOSXApplicationSupportPath("Config", applicationName, companyName);
            }

            if (path == null)
            {
                throw new InvalidOperationException("This method is not supported on the current platform.");
            }

            Directory.CreateDirectory(path);

            return path;
        }

        /// <summary>
        /// Gets the recommended directory for storing logs for the current operating system.
        /// </summary>
        /// <param name="applicationName">The application name.</param>
        /// <param name="companyName">The company name, or null to not use a company subdirectory.</param>
        /// <returns>the recommended directory for storing game-specific save data for the current operating system.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the method is called on a platform that isn't
        /// Windows, Linux, or OSX.
        /// </exception>
        public static string GetLogDirectory(string applicationName, string companyName = null)
        {
            string path = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                path = GetWindowsPath("Logs", applicationName, companyName);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                path = GetLinuxLogDirectory(applicationName, companyName);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                path = GetOSXLogsPath(applicationName, companyName);
            }

            if (path == null)
            {
                throw new InvalidOperationException("This method is not supported on the current platform.");
            }

            Directory.CreateDirectory(path);

            return path;
        }

        private static string GetWindowsPath(string subDirectory, string applicationName, string companyName = null)
        {
            string myGamesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                              "My Games");

            return AppendDirectory(AppendApplicationPath(myGamesPath, applicationName, companyName), subDirectory);
        }

        private static string GetOSXApplicationSupportPath(string subDirectory,
                                                           string applicationName,
                                                           string companyName = null)
        {
            string supportPath =
                Path.Combine(Environment.GetEnvironmentVariable("HOME"), "Library/Application Support");

            return AppendDirectory(AppendApplicationPath(supportPath, applicationName, companyName), subDirectory);
        }

        private static string GetOSXLogsPath(string applicationName, string companyName = null)
        {
            string logsPath = Path.Combine(Environment.GetEnvironmentVariable("HOME"), "Library/Logs");

            return AppendApplicationPath(logsPath, applicationName, companyName);
        }

        private static string GetLinuxSaveDirectory(string applicationName, string companyName = null)
        {
            string path = Environment.GetEnvironmentVariable("XDG_DATA_HOME");

            if (string.IsNullOrEmpty(path))
            {
                string home = Environment.GetEnvironmentVariable("HOME");
                path = Path.Combine(home, ".local/share");
            }

            return AppendApplicationPath(path, applicationName, companyName);
        }

        private static string GetLinuxConfigDirectory(string applicationName, string companyName = null)
        {
            string path = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");

            if (string.IsNullOrEmpty(path))
            {
                string home = Environment.GetEnvironmentVariable("HOME");
                path = Path.Combine(home, ".config");
            }

            return AppendApplicationPath(path, applicationName, companyName);
        }

        public static string GetLinuxLogDirectory(string applicationName, string companyName)
        {
            return AppendApplicationPath("/var/log", applicationName, companyName);
        }

        private static string AppendApplicationPath(string path, string applicationName, string companyName = null)
        {
            string ret = path;

            if (companyName != null)
            {
                ret = AppendDirectory(ret, companyName);
            }

            return AppendDirectory(ret, applicationName);
        }

        private static string AppendDirectory(string path, string dir)
        {
            if (string.IsNullOrEmpty(dir))
            {
                return path;
            }

            dir = CleanForPath(dir);

            return Path.Combine(path, dir);
        }

        private static string CleanForPath(string str)
        {
            foreach (char character in Path.GetInvalidFileNameChars())
            {
                str = str.Replace(character, '_');
            }

            return str;
        }
    }
}
