using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using OneYearLater.Storage.Models;
using SQLite;
using UnityEngine;

namespace OneYearLater.Storage
{
	class SQLiteStorage : MonoBehaviour, IStorage
	{
		private string _bigDummyParagraph = "Lorem ipsum dolor sit amet, consectetur adipiscing elit."
			+ "Ut aliquam risus vitae turpis blandit lobortis. Nam dictum"
			+ "ornare mi, id varius dolor venenatis in. Vestibulum eu "
			+ "suscipit velit. Proin euismod est lorem, id posuere diam "
			+ "consectetur sodales. Pellentesque sit amet metus ante. "
			+ "Maecenas in urna tincidunt, pharetra felis sed, eleifend "
			+ "quam. Integer nec lorem eget massa euismod suscipit sed varius "
			+ "tellus. Suspendisse potenti. Sed sit amet quam ut erat vehicula ornare. "
			+ "Duis tempor neque pulvinar commodo semper. Suspendisse imperdiet "
			+ "tristique pulvinar. Quisque feugiat ipsum tincidunt porta maximus. "
			+ "Maecenas quam lectus, eleifend a fermentum at, congue nec magna. "
			+ "Morbi sed interdum dui. Nam vulputate vulputate eros in pulvinar.";

		private SQLiteAsyncConnection _connection;

		public SQLiteStorage(string dbFile)
		{
			_connection = new SQLiteAsyncConnection(dbFile);
			_connection.CreateTableAsync<DiaryRecordModel>();

			_connection.InsertAsync(new DiaryRecordModel() { RecordDateTime = DateTime.Now, Text = _bigDummyParagraph });
		}

		public async Task<IEnumerable<BaseRecordViewModel>> GetAllDayRecordsAsync(DateTime date)
		{
			DateTime dayStartInc = date.Date;
			DateTime dayEndExc = date.Date.AddDays(1);

			var query = _connection.Table<DiaryRecordModel>().Where(r => (r.RecordDateTime >= dayStartInc) && (r.RecordDateTime < dayEndExc));
			return (await query.ToListAsync())
				.Select(rm => new DiaryRecordViewModel(rm.RecordDateTime, rm.Text));
		}

		public Task InsertRecordsAsync(IEnumerable<BaseRecordViewModel> records)
		{
			return Task.CompletedTask;
		}
	}
}
