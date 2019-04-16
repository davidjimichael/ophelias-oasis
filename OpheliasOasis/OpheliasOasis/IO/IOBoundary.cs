using Oasis.Dev;
using Oasis.Models;
using System;
using System.Collections.Generic;
using System.IO;

// todo clean up there's like 3 different ways you do everything
namespace Oasis.IO
{
    /// <summary>
    ///     I'll be honest. Don't touch this class. Except to change the "connection string" or add model types. 
    ///     It's not that I think I'm right and you can't do better, it's that I'm probably wrong and this is britle 
    ///     difference is this works and that's what's important right now. 
    /// </summary>
    public class IOBoundary
    {
        // OKAY TO EDIT the DEFAULT_FOLDER to match whatever yours it, yes relative pathing is a thing, no I'm not using it right now (or probably ever)
        private static readonly string DEFAULT_FOLDER = @"C:\Users\david\Desktop\ophelias-oasis\OpheliasOasis\OpheliasOasis\IO\";
        private static string GetModelName<T>()
        {
            var modelFileNames = new Dictionary<Type, string>()
            {
                // OKAY TO EDIT if you need to add a model just put the type and the filename in the same format
                { typeof(Day), "days" },
                { typeof(Reservation), "reservations" },
                { typeof(CreditCard), "creditcards" },
            };

            return modelFileNames.TryGetValue(typeof(T), out string name) ? name : throw new Exception("Unknown model");
        }

        public static IOBoundary Create<T>()
        {
            return new IOBoundary(GetModelName<T>());
        }
        // current file
        private readonly string Location = "";
        
        public IOBoundary(string modelName, string folder = null)
        {
            // if you declare a folder it gives you that folder otherwise defaults
            // to the text file matching the model name

            string FormatFileName(string fileName)
            {
                fileName = fileName.ToLower();

                if (!fileName.EndsWith(".txt"))
                {
                    fileName += ".txt";
                }
                return fileName;
            }

            Location = folder ?? DEFAULT_FOLDER + FormatFileName(modelName);
        }

        public IEnumerable<string> Read()
        {
            return File.ReadAllLines(Location);
        }

        // this is essentially a DROP TABLE 
        void Clear()
        {
            File.WriteAllText(Location, string.Empty);
        }

        public void Write(IEnumerable<string> lines)
        {
            using (var file = new StreamWriter(Location))
            {
                foreach (string line in lines)
                {
                    file.WriteLine(line);
                }
            }
        }
    }
}