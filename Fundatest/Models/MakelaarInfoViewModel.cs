using Fundatest.Model;
using System.Collections.Generic;

namespace Fundatest.Models
{
    public class MakelaarInfoViewModel
    {
        public List<MakelaarCount> MakelaarsZonderTuin { get; set; }
        public List<MakelaarCount> MakelaarsMetTuin { get; set; }
        public string Message { get; set; }
    }
}
