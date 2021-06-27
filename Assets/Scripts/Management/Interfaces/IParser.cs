using OneYearLater.Management.ViewModels;
using System.Collections.Generic;

namespace OneYearLater.Management.Interfaces
{
    interface IParser
    {
        IEnumerable<BaseRecordViewModel> Parse(string filepath);
    }
}
