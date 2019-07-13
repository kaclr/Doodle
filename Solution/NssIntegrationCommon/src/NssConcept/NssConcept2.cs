using System;
using System.Collections.Generic;
using System.Text;

namespace NssIntegration
{
    public interface INssConcept2<T>
    {
        T value { get; set; }

        bool Check(T value);
    }
}
