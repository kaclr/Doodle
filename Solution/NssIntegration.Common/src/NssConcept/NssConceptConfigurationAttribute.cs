using System;
using System.Collections.Generic;
using System.Text;
using Doodle.CommandLineUtils;

namespace NssIntegration
{
    public class NssConceptConfigurationAttribute : ParameterConfigurationAttribute
    {
        public NssConceptConfigurationAttribute(NssConcept nssConcept) : base(NssConceptHelper.GetConceptInfo(nssConcept))
        {
            valueChecker = value => NssConceptHelper.CheckConcept(nssConcept, value);
        }
    }
}
