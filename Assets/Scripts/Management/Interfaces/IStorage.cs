using OneYearLater.Management.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneYearLater.Management.Interfaces
{
    public interface IStorage
    {
        Task InsertRecordsAsync(IEnumerable<BaseRecordViewModel> records);
        Task<IEnumerable<BaseRecordViewModel>> GetAllDayRecordsAsync(DateTime date);
    }
}
