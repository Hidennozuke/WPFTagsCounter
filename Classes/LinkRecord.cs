using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebTagsСounterWPF.Classes
{
    public class LinkRecord
    {
        public string link { get; set; }
        public int count { get; set; }

        public LinkRecord(string link, int count)
        {
            this.link = link;
            this.count = count;
        }
    }
}
