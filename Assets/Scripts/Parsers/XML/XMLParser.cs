using OneYearLater.Management;
using OneYearLater.Management.Interfaces;
using OneYearLater.Management.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using static OneYearLater.Management.Constants;

namespace OneYearLater.Parsers.XML
{
    class XMLParser : IParser
    {
        public IEnumerable<BaseRecordViewModel> Parse(string filepath)
        {
            List<BaseRecordViewModel> result = new List<BaseRecordViewModel>();

            var reader = XmlReader.Create(filepath);

            reader.ReadToFollowing("record");

            do
            {
                ERecord type;
                DateTime datetime;

                reader.ReadToFollowing("datetime");
                var datetimeStr = reader.ReadElementContentAsString();
                long ticks = Convert.ToInt64(datetimeStr);
                datetime = new DateTime(ticks);

                reader.ReadToFollowing("type");
                var typeStr = reader.ReadElementContentAsString();

                try
                {
                    type = RecordTypeToString.First(x => x.Value == typeStr.Trim().ToLower()).Key;
                }
                catch (InvalidOperationException)
                {
                    continue;
                }

                reader.ReadToFollowing("content");

                BaseRecordViewModel recordViewModel = null;

                switch (type)
                {
                    case ERecord.Diary:
                        reader.ReadToFollowing("text");
                        string text = reader.ReadElementContentAsString();
                        recordViewModel = new DiaryRecordViewModel(datetime, text);
                        break;
                }

                result.Add(recordViewModel);

            } while (reader.ReadToFollowing("record"));

            return result;
        }
    }
}
