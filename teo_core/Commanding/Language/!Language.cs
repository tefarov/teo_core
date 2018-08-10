using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TEO.General;

namespace TEO.Commanding.Language
{
    public partial class Language
    {
        public const string
            // Keywords
            KwUse = "use"
            , KwRun = "run"
            , KwAsk = "ask"
            , KwBreak = "break"

            // Types
            , TpString = "string"
            , TpInt = "int"
            , TpDecimal = "decimal"
            , TpDate = "date"
            , TpPeriod = "period"

            // Text
            , TxTrue = "true", TxTrue2 = "да"
            , TxFalse = "false", TxFalse2 = "нет"

            // Signs
            , SnAssign = ":="
            ;

        public const char
            ChLinebrk = ';'
            , ChEscapeC = '\\'
            , ChComment = '#'
            , ChLineend = '\r'
            , ChVariable = '$'

            , ChQuotSim = '\''
            , ChQuotCpx = '"'
            ;

        static char[] __ASPC = { ' ', '\t' };
    }
}
