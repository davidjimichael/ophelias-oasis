using Oasis.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Oasis.IO
{
    public class IOBoundary
    {
        private static readonly string DEFAULT_FOLDER = "..\\..\\IO\\";
        private readonly string Location = "";

        private static string GetModelName<T>()
        {
            var modelFileNames = new Dictionary<Type, string>()
            {
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
        
        public IOBoundary(string modelName)
        {
            string FormatFileName(string fileName)
            {
                fileName = fileName.ToLower();

                if (!fileName.EndsWith(".txt"))
                {
                    fileName += ".txt";
                }
                return fileName;
            }

            Location = DEFAULT_FOLDER + FormatFileName(modelName);
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