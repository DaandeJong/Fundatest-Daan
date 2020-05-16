using Fundatest.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fundatest.Services
{
    public interface IApiService
    {
        public List<MakelaarCount> GetTop10(string searchString);
    }
}
