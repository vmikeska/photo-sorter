using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace photo_sorter.ints
{
    public class ConfigInt
    {
        public ButtonsInt[] actions { get; set; }
    }

    public class ButtonsInt
    {
        public string name { get; set; }

        public string directory { get; set; }

        public string direction { get; set; }

    }

}
