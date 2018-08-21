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

            //Commands


            // Types
            , TpString = "string"
            , TpInt = "int"
            , TpDecimal = "decimal"
            , TpDate = "date"
            , TpPeriod = "period"
            , TpFile = "file.text"

            // Text
            , TxTrue = "true", TxTrue2 = "да"
            , TxFalse = "false", TxFalse2 = "нет"

            // Predefined texts
            , Tx_AskText = "Введите текст"
            , Tx_AskNumber = "Введите число"
            , Tx_AskPath = "Введите путь"
            ;

        public const char
            ChLinebrk = ';'
            , ChEscapeC = '\\'
            , ChComment = '#'
            , ChLineend = '\r'
            , ChVariable = '$'
            ,ChAssign = ':'

            , ChQuotSim = '\''
            , ChQuotCpx = '"'
            ;

        /// <summary>
        /// These chars are spaces
        /// </summary>
        public static char[] Spaces = { ' ', '\t' };
        /// <summary>
        /// These chars are operators
        /// </summary>
        public static char[] Operators = { '=' };

        /// <summary>
        /// These symbols are allowed in keywords besides digits and letters
        /// </summary>
        public static char[] SymbolsKeyword = { '_' };
        /// <summary>
        /// These symbols are allowed in variable's names besides digits and letters
        /// </summary>
        public static char[] SymbolsVariables = { '_' };

        public static string
            StNewLine = System.Environment.NewLine
            ;
    }
}
