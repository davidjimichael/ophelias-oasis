using Oasis.BusinessLogic;
using Oasis.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

// todo clean up there's like 3 different ways you do everything
namespace Oasis.IO
{
    /// <summary>
    /// Just a beefy IO class to handle everything right now, yes there are better ways
    /// no there are not any better ways that I have working right now
    /// 
    /// one thing i would like to do is add generic get and sets for use with each model
    /// </summary>
    public class IOBoundary
    {
        #region ioboundary
        //private static readonly string DEFAULT_FOLDER = @"F:\OpheliasOasis\OpheliasOasis\IO\";
        private static readonly string DEFAULT_FOLDER = @"C:\Users\david\Desktop\ophelias-oasis\OpheliasOasis\OpheliasOasis\IO\";
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
        #endregion

        //#region iogenerics
        //// just to reduce templating later on
        //public static IEnumerable<T> Get<T>(Func<T, bool> filter = null)
        //{
        //    return Get<T, T>(filter);
        //}

        //public static IEnumerable<R> Get<T, R>(Func<T, bool> filter = null, Func<T, R> map = null)
        //{
        //    try
        //    {
        //        var iob = new IOBoundary(GetModelName<T>());
        //        var lines = iob.Read();

        //        IEnumerable<T> objs = lines.Select(l => JsonConvert.DeserializeObject<T>(l));

        //        // optional filter like a range
        //        if (filter != null)
        //        {
        //            objs = objs.Where(filter);
        //        }

        //        if (map != null)
        //        {
        //            return objs.Select(map);
        //        }

        //        // look i trust you you decide to let this happen
        //        // if you screw up it will error so you'll know
        //        return (IEnumerable<R>)objs;
        //    }
        //    catch (Exception e)
        //    {
        //        if (Program.Employee == 2)
        //        {
        //            Console.WriteLine(e.Message);
        //        }
        //        return new R[] { };
        //    }
        //}

        //public static bool Set<T>(IEnumerable<T> items)
        //{
        //    try
        //    {
        //        var iob = new IOBoundary(GetModelName<T>());

        //        iob.Clear();

        //        IEnumerable<string> lines = items.Select(i => JsonConvert.SerializeObject(i));

        //        iob.Write(lines);

        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        if (Program.Employee == 2)
        //        {
        //            Console.WriteLine(e.Message);
        //        }
        //        return false;
        //    }       
        //}
        
        //// this is more of an update command
        //public static bool Set<T, TOrderBy>(Func<T, bool> filter = null, Func<T, T> update = null, Func<T, TOrderBy> order = null)
        //{
        //    try
        //    {
        //        IEnumerable<T> items = Enumerable.Empty<T>();
                
        //        Func<T, bool> noFilter = x => true;
        //        Func<T, T> noUpdate = x => x;

        //        var _filter = filter ?? noFilter;
        //        var _update = update ?? noUpdate;

        //        items = items.Concat(Get<T, T>(_filter, _update));

        //        var iob = new IOBoundary(GetModelName<T>());

        //        items = items.Union(Get<T>(t => !_filter(t))).OrderBy(order);

        //        iob.Write(items.Select(i => JsonConvert.SerializeObject(i)));

        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        if (Program.Employee == 2)
        //        {
        //            Console.WriteLine(e);
        //        }
        //        return false;
        //    }
        //}

        //public static bool Add<T, TOrderBy>(IEnumerable<T> items, Func<T, TOrderBy> order = null)
        //{
        //    try 
        //    {
        //        // filter out any items that should be removed during this
        //        // IEnumerable<T> currentItems = Get<T>();

        //        // currentItems.Union(items);
        //        // add currently stored files to items
        //        items = items.Concat(Get<T>());

        //        if (order != null)
        //        {
        //            items.OrderBy(order);
        //        }

        //        var iob = new IOBoundary(GetModelName<T>());
                
        //        iob.Write(items.Select(i => JsonConvert.SerializeObject(i)));
                
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        if (Program.Employee == 2)
        //        {
        //            Console.WriteLine(e);
        //        }
        //        return false;
        //    }
        //}
        //protected virtual void Dispose(bool disposing)
        //{

        //}
        //public void Dispose()
        //{
        //    // Dispose of unmanaged resources.
        //    Dispose(true);
        //    // Suppress finalization.
        //    GC.SuppressFinalize(this);
        //}
        //#endregion

    }

    #region newstuff
    interface IDAL
    {
        bool Create<T>(IEnumerable<T> items);
        bool Create<T, R>(IEnumerable<T> items, Func<T, R> orderBy = null);
        IEnumerable<T> Read<T>(Func<T, bool> filter = null);
        bool Update<T>(Func<T, T> update, Func<T, bool> filter = null);
        IEnumerable<T> Delete<T>(Func<T, bool> filter = null);
    }

    public class DAL : IDAL
    {
        // todo see theres a way to key on a serialized object so then we dont have to convert each 
        // object to check it against the filter
        public DAL() { }

        public bool Create<T>(IEnumerable<T> items)
        {
            return Create<T, T>(items: items, orderBy: null);
        }

        public bool Create<T, R>(IEnumerable<T> items, Func<T, R> orderBy = null)
        {
            try
            {
                IOBoundary iob = IOBoundary.Create<T>();
                IEnumerable<T> prev = iob.Read().Select(line => JsonConvert.DeserializeObject<T>(line));
                items = prev.Concat(items);

                if (orderBy != null)
                {
                    items.OrderBy(orderBy);
                }

                IEnumerable<string> lines = items.Select(i => JsonConvert.SerializeObject(i));

                iob.Write(lines);

                return true;
            }
            catch (Exception e)
            {
                if (Program.Employee == 2)
                {
                    Console.WriteLine(e.Message);
                }
                return false;
            }
        }

        public IEnumerable<T> _Delete<T>(Func<T, bool> filter = null)
        {
            try
            {
                IOBoundary iob = IOBoundary.Create<T>();
                IEnumerable<T> prev = iob.Read().Select(line => JsonConvert.DeserializeObject<T>(line));

                if (filter != null)
                {
                    prev = prev.Where(filter);
                }

                prev.Select(p => DeleteRecord(p));

                return prev;
            }
            catch (Exception e)
            {
                if (Program.Employee == 2)
                {
                    Console.WriteLine(e.Message);
                }
                return Enumerable.Empty<T>();
            }
        }

        private T DeleteRecord<T>(T p)
        {
            if (p is Reservation res)
            {
                res.Status = ReservationStatus.Cancelled;
            }
            throw new NotImplementedException();
        }

        public IEnumerable<T> Delete<T>(Func<T, bool> filter = null)
        {
            try
            {
                IOBoundary iob = IOBoundary.Create<T>();
                IEnumerable<T> prev, toSave, toDelete;

                prev = iob.Read().Select(line => JsonConvert.DeserializeObject<T>(line));

                if (filter != null)
                {
                    // get all excluding the ones we will delete
                    // todo add persistant logs
                    toSave = prev.Where(p => !filter(p));
                    toDelete = prev.Where(p => filter(p));
                }
                else
                {
                    toDelete = prev;
                    toSave = Enumerable.Empty<T>();
                }

                IEnumerable<string> lines = toSave.Select(t => JsonConvert.SerializeObject(t));

                iob.Write(lines);

                return toDelete;
            }
            catch (Exception e)
            {
                if (Program.Employee == 2)
                {
                    Console.WriteLine(e.Message);
                }
                return Enumerable.Empty<T>();
            }
        }

        public IEnumerable<T> Read<T>(Func<T, bool> filter = null)
        {
            try
            {
                IOBoundary iob = IOBoundary.Create<T>();
                IEnumerable<T> prev = iob.Read().Select(line => JsonConvert.DeserializeObject<T>(line));
                
                if (filter != null)
                {
                    prev = prev.Where(filter);
                }

                return prev;
            }
            catch (Exception e)
            {
                if (Program.Employee == 2)
                {
                    Console.WriteLine(e.Message);
                }
                return Enumerable.Empty<T>();
            }
        }

        // todo make so that update takes a return void update method which is calls from the Func<T, T>
        public bool Update<T>(Func<T, T> update, Func<T, bool> filter = null)
        {
            try
            {
                IOBoundary iob = IOBoundary.Create<T>();
                IEnumerable<T> prev, toUpdate;
                
                prev = iob.Read().Select(line => JsonConvert.DeserializeObject<T>(line));

                if (filter != null)
                {
                    toUpdate = prev.Where(p => filter(p));
                    prev = prev.Where(p => !filter(p));
                }
                else
                {
                    toUpdate = prev;
                    prev = Enumerable.Empty<T>();
                }

                IEnumerable<T> updated = toUpdate.Select(update).ToArray();

                IEnumerable<string> lines = prev.Concat(updated).Select(x => JsonConvert.SerializeObject(x));

                iob.Write(lines);

                return true;
            }
            catch (Exception e)
            {
                if (Program.Employee == 2)
                {
                    Console.WriteLine(e.Message);
                }
                return false;
            }
        }
    }
    #endregion
}