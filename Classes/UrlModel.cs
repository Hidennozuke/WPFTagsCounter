using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTagsСounterWPF.Classes
{
    internal class UrlModel
    {
        [Newtonsoft.Json.JsonProperty("links")]
        public string[] links { get; set; }
    }
    
}
