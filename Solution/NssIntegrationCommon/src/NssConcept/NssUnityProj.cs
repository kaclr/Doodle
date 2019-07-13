using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NssIntegration
{
    public class NssUnityProj : INssConcept2<string>
    {
        public static implicit operator NssUnityProj(string src) => new NssUnityProj(src);
        public static implicit operator string(NssUnityProj src) => src.value;

        public string value { get; set; }

        public NssUnityProj()
        {
        }

        public NssUnityProj(string value)
        {
            this.value = value;
        }

        public bool Check(string value)
        {
            return !Directory.Exists(Path.Combine(value, "Assets")) || !Directory.Exists(Path.Combine(value, "Version"));
        }
    }
}
