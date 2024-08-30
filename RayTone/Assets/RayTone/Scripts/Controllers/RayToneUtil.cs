using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace RayTone
{
    public class RayToneUtil
    {
        /// <summary>
        /// Find string between leftKey and rightKey
        /// </summary>
        /// <param name="text"></param>
        /// <param name="leftKey"></param>
        /// <param name="rightKey"></param>
        /// <returns></returns>
        public static string FindStringArg(string text, string leftKey, string rightKey)
        {
            if(!text.Contains(leftKey)) return "";

            string right = text.Substring(text.IndexOf(leftKey) + leftKey.Length);
            string left = "";
            if(right.IndexOf(rightKey) >= 0)
            {
                left = right.Substring(0, right.IndexOf(rightKey));
            }

            return left;
        }

        /// <summary>
        /// Copy directories method described in https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="destinationDir"></param>
        /// <param name="recursive"></param>
        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Get information about the source directory
            DirectoryInfo dir = new(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists) return;

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }
    }
}
