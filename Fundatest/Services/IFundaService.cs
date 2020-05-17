using Fundatest.Model;
using System.Collections.Generic;

namespace Fundatest.Services
{
    public interface IApiService
    {
        public List<MakelaarCount> GetTop10(string searchString);
    }
}
