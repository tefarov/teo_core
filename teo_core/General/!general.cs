using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEO.General
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
}
