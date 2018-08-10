using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TEO.General;

namespace TEO.Commanding
{
    public class BatchSequential : IBatch, IGui
    {
        int SSN = 0;
        IBatchable[] AITM;

        public string Text { get; set; }

        public BatchSequential Append(params IBatchable[] items)
        {
            if (items == null || items.Length == 0)
                AITM = new IBatchable[0];
            else if (AITM == null)
                AITM = items;
            else {
                AITM = AITM.Union(items).Where(x => x != null).ToArray(Tefarov.Auto);
            }
            SSN++;

            return this;
        }
        
        public override string ToString()
        {
            int len = 0;
            if (AITM != null || AITM.Length > 0) len = AITM.Length - 1;

            return string.Format("{0}({1})", this.GetType().Name, len);
        }

        #region IEnumerable
        public IEnumerator<IBatchable> GetEnumerator()
        {
            return new enumerator(this);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new enumerator(this);
        }
        class enumerator : IEnumerator<IBatchable>
        {
            readonly BatchSequential ITM;
            readonly int SSN;
            int CUR = -1;

            public enumerator(BatchSequential parent)
            {
                ITM = parent ?? throw new ArgumentNullException("Parent-batch unset");
                SSN = parent.SSN;
            }

            object IEnumerator.Current { get { return this.Current; } }
            public IBatchable Current
            {
                get
                {
                    if (ITM.AITM == null || ITM.AITM.Length <= 0) return null;
                    if (CUR >= ITM.AITM.Length) return null;

                    return ITM.AITM[CUR];
                }
            }

            public void Dispose() { }
            public bool MoveNext()
            {
                if (SSN != ITM.SSN) throw new InvalidOperationException("Sequence broken");
                var val = ++CUR < ITM.AITM.Length;
                return val;
            }
            public void Reset()
            {
                CUR = 0;
            }
        }
        #endregion
    }
}
