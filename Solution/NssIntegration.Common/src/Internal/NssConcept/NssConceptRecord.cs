using System;
using System.Collections.Generic;
using System.Text;

namespace NssIntegration
{
    internal abstract class NssConceptRecord
    {
        public Type type { get; private set; }
        public string info;

        protected NssConceptRecord(Type t)
        {
            type = t;
        }
    }

    internal class NssConceptRecord<T> : NssConceptRecord
    {
        public Func<T, string> checker;

        public NssConceptRecord() : base(typeof(T))
        {
            checker = value => null;
        }
    }
}
