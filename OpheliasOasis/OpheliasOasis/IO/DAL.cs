﻿using Newtonsoft.Json;
using Oasis.Dev;
using Oasis.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Oasis.IO
{
    /// <summary>
    ///     Filters provide a way to act only on the objects which return true from the filter. 
    /// </summary>
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

        #region untested
        // todo add persistant logs, not sure how I want to do this yet. Here's a possbility, but not what I recommend. 
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
        #endregion
    }
}
