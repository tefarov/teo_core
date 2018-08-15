 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using io = System.IO;

namespace TEO
{
    public enum Behaviour : byte
    {
        MaskRead = 0b01_000000,
        MaskWrite = 0b10_000000,

        // <summary>This will not check for validity</summary>
        //NoCheck = 0b00_000000 | MaskWrite | MaskRead,
        /// <summary>This will return an empty string if conflict</summary>
        Empty = 0b00_000010 | MaskWrite | MaskRead,
        /// <summary>This will return the filepath even in case of conflict</summary>
        Ignore = 0b00_000001 | MaskWrite | MaskRead,
        /// <summary>Throw error if conflict</summary>
        Exception = 0b00_100000 | MaskWrite | MaskRead,

        /// <summary>Rename a current file if conflict</summary>
        Rename = 0b00_000001 | MaskWrite,
        /// <summary>Delete the other file if conflict</summary>
        Overwrite = 0b00_000010 | MaskWrite,

        /// <summary>Create a file if not exists</summary>
        Create = 0b00_000001 | MaskRead
    }
    internal static class Helpers
    {
        public static Behaviour ParseBehaviour(string name)
        {
            if (name == "overwrite") return Behaviour.Overwrite;
            else if (name == "rename") return Behaviour.Rename;
            else if (name == "ignore") return Behaviour.Ignore;
            else if (name == "exception") return Behaviour.Exception;
            else if (name == "create") return Behaviour.Create;

            return Behaviour.Exception;
        }
    }

    /// <summary>
    /// General helpers
    /// </summary>
    public static class Tefarov
    {
        public static string NotNull(this string item, string exceptionmessage = null)
        {
            if (string.IsNullOrEmpty(item)) throw new ArgumentNullException(string.IsNullOrEmpty(exceptionmessage) ? "Invalid argument" : exceptionmessage);
            return item;
        }
        /// <summary>
        /// This will return an item if it's not null, otherwise through an Exception
        /// </summary>
        /// <typeparam name="T">Type of items to be checked for null</typeparam>
        /// <param name="item"></param>
        /// <param name="exceptionmessage">Exception's message</param>
        /// <returns></returns>
        public static T NotNull<T>(this T item, string exceptionmessage = null) where T : class
        {
            if (item == null) throw new ArgumentNullException(string.IsNullOrEmpty(exceptionmessage) ? "Invalid argument" : exceptionmessage);
            return item;
        }
        /// <summary>
        /// This will return an item if it's not null, otherwise through an Exception
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="defaultitem"></param>
        /// <returns></returns>
        public static T NotNull<T>(this T item, T defaultitem) where T : class
        {
            if (item == null && defaultitem == null) throw new ArgumentNullException("Default item is null");
            else if (item == null) return defaultitem;
            return item;
        }

        /// <summary>
        /// This MCorelies immediatly some action to all the items in an enumeration (Enumeration will be enumerated right now)
        /// </summary>
        /// <param name="action">an action to MCorely</param>
        /// <param name="processnulls">if set to false will not process any null-items</param>
        public static IEnumerable<T> MCorely<T>(this IEnumerable<T> items, Action<T> action, bool processnulls = false)
        {
            var enm = items.GetEnumerator();
            while (enm.MoveNext())
                if (processnulls || enm.Current != null)
                    action(enm.Current);

            return items;
        }
        /// <summary>
        /// This MCorelies immediatly some action to all the items in an array. This doesn't create any enumerators, unlike IEnumearable.MCorelyNow()
        /// </summary>
        /// <param name="action">an action to MCorely</param>
        /// <param name="processnulls">if set to false will not process any null-items</param>
        public static T[] MCorely<T>(this T[] items, Action<T> action, bool processnulls = false)
        {
            T itm;
            for (int i = 0; i < items.Length; i++) {
                if ((itm = items[i]) != null || processnulls)
                    action(itm);
            }

            return items;
        }
        /// <summary>
        /// This Applies immediatly some action to all the items in an array. This doesn't create any enumerators, unlike IEnumearable.MCorelyNow()
        /// </summary>
        /// <param name="action">an action to MCorely</param>
        /// <param name="processnulls">if set to false will not process any null-items</param>
        public static T[] Apply<T>(this T[] items, Action<int, T> action, bool processnulls = false)
        {
            T itm;
            for (int i = 0; i < items.Length; i++) {
                if ((itm = items[i]) != null || processnulls)
                    action(i, itm);
            }

            return items;
        }
        /// <summary>
        /// MCorelies some action to all the items in the enumeration at the time of enumeration (Enumeration will not be enumerated now)
        /// </summary>
        /// <param name="action">an action to MCorely</param>
        public static IEnumerable<T> Each<T>(this IEnumerable<T> items, Action<T> action)
        {
            var enm = items.GetEnumerator();
            while (enm.MoveNext()) {
                action(enm.Current);
                yield return enm.Current;
            }
        }
        public static GetterEnumerable<T> IGetter<T>(this IEnumerable<T> sequence)
        {
            return new GetterEnumerable<T>(sequence);
        }

        public static IEnumerable<T> Single<T>(T item)
        {
            yield return item;
        }
        /// <summary>
        /// This returns a single item from the sequence if a sequence consists of a single item. Otherwise empty value
        /// </summary>
        /// <typeparam name="T">Type of items in a sequence</typeparam>
        /// <param name="items">The sequence</param>
        public static T SingleOrNone<T>(this IEnumerable<T> items)
        {
            if (items == null) return default(T);

            int c = 0;
            T val = default(T);

            var enm = items.GetEnumerator();
            while (enm.MoveNext()) {
                if (++c > 1) return default(T);
                val = enm.Current;
            }

            return val;
        }

        /// <summary>
        /// This makes ToArray a bit lighter for large collections with a known size
        /// </summary>
        public static T[] ToArray<T>(this IEnumerable<T> items, int count)
        {
            if (count == Tefarov.Auto) count = items.Count();

            T[] aitm = new T[count];

            int i = 0;
            var enm = items.GetEnumerator();
            while (enm.MoveNext()) {
                if (i >= aitm.Length) return aitm;
                aitm[i++] = enm.Current;
            }
            return aitm;
        }
        /// <summary>
        /// ToArrayReal : returns an array of non-null items. This makes ToArray a bit lighter for large collections with a known size
        /// </summary>
        /// <typeparam name="T">Type of items being processed</typeparam>
        /// <param name="count">The size of the result array</param>
        public static T[] ToArrayR<T>(this IEnumerable<T> source, int count) where T : class
        {
            if (count == Tefarov.Auto) count = source.Count();

            T[] aitm = new T[count];

            int i = 0;
            var enm = source.GetEnumerator();
            while (enm.MoveNext()) {
                if (enm.Current == null) continue;
                if (i >= aitm.Length) return aitm;
                aitm[i++] = enm.Current;
            }

            // This means that all the elements of the array are filled with real values
            if (i == aitm.Length) return aitm;

            T[] anew = new T[i];
            for (i = 0; i < anew.Length; i++)
                anew[i] = aitm[i];

            return anew;
        }

        /*public static string ToFilepath(this string path, bool onlyexistsing = false)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            if (path[0] == '\\' && !path.StartsWith(@"\\")) {
                var dir = io.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                path = io.Path.Combine(dir, path.Substring(1));
            }

            if (onlyexistsing && !io.File.Exists(path))
                throw new ArgumentException("File " + path + " doesn't exist");

            return path;
        }
        /// <summary>
        /// Creates a filename-getter to an existing or non-existing file
        /// </summary>
        /// <param name="filepath">path tot the desrired file wheather it exists or needs to be creaed</param>
        /// <param name="isnew">Wheather we are dealing with an exitsing file or it will be created by some outer code</param>
        /// <param name="overwritenew">If we are dealing with a path to the file that will be created and found out that this file exists, should we overwrite it or something else</param>
        public static IGetter<string> IGetterFile(this string filepath, bool isnew = true, bool overwritenew = false)
        {
            if (isnew && overwritenew)
                return new GetterFilenameWrite(filepath, GetterFilenameWrite.ModeOverwrite);
            else if (isnew)
                return new GetterFilenameWrite(filepath, GetterFilenameWrite.ModeRename);

            throw new NotImplementedException("Getters of existing files not yet implemented");
        }//*/

        public static string Concat(this IEnumerable<string> sequence, string separator)
        {
            if (sequence == null) return string.Empty;
            if (separator == null) separator = string.Empty;

            if (__SBD == null) __SBD = new StringBuilder(); else __SBD.Clear();
            var sbd = __SBD;

            var enm = sequence.GetEnumerator();
            while (enm.MoveNext()) {
                sbd.Append(separator);
                sbd.Append(enm.Current);
            }

            if (sbd.Length > 0) sbd.Remove(0, separator.Length);
            return sbd.ToString();
        }

        static StringBuilder __SBD;

        public const int Auto = -21245478;
    }
}
