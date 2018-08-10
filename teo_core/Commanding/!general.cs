using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEO.Commanding
{
    public struct ExecuteResult
    {
        public bool IsSuccessfull;
        public bool IsBreak;

        public string Description;

        public ExecuteResult(bool issuccess)
        {
            this.IsSuccessfull = issuccess;
            this.IsBreak = false;
            this.Description = string.Empty;
        }
    }
    public enum TCommand : byte
    {
        Sequential = 0b0001_0000,
        Async = 0b0010_0000,
        Ui = 0b0100_0000
    }
}
