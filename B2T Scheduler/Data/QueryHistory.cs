using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace B2T_Scheduler
{
    public class QueryHistory
    {
        public ConcurrentDictionary<string, QueryRecord> QueryRecords = new ConcurrentDictionary<string, QueryRecord>();
        public class QueryRecord
        {
            public string Name { get; set; }
            public string Step { get; set; }
            public System.Diagnostics.Stopwatch StopWatch { get; set; }
            public int? QueryCount;
            public int? LoadCount;
            public DateTime? QueryStartTime;
            public TimeSpan? QueryDuration;
            public TimeSpan? LoadDuration;

            public string GetQueryStartedMessage()
            {
                return Name + " " + Step;
            }

            public string GetQueryCompletedMessage()
            {
                return (QueryDuration.HasValue
                    ? $"{ QueryDuration.Value.TotalSeconds:0.000} "
                    : "") +
                Name +
                (QueryCount.HasValue
                    ? " retrieved " + pluralize(QueryCount, "record", "records")
                    : " complete");
            }

            public string GetLoadCompletedMessage()
            {
                return (LoadDuration.HasValue
                    ? $"{ LoadDuration.Value.TotalSeconds:0.000} "
                    : "") +
                Name +
                (LoadCount.HasValue
                    ? " loaded " + pluralize(LoadCount, "record", "records")
                    : " complete");
            }

            public string GetProgressMessage()
            {
                if (LoadDuration.HasValue)
                    return GetLoadCompletedMessage();
                if (QueryDuration.HasValue)
                    return GetQueryCompletedMessage();
                return GetQueryStartedMessage();
            }

            public override string ToString()
            {
                return base.ToString();
            }

            private string pluralize(int? n, string singular, string plural)
            {
                int v = n.HasValue ? n.Value : 0;
                return v.ToString() + " " + (v == 1 ? singular : plural);
            }

        };

        public bool IsTimingEnabled { get; set; } = true;
        public bool IsVerbose { get; set; } = true;
        public Action<QueryRecord> ProgressCallback { get; set; } = null;

        public void MarkStart(string name)
        {
            MarkQueryStart(name);
        }

        public void MarkStop(string name)
        {
            MarkLoadComplete(name, null, "Finished");
        }

        public void MarkQueryStart(string name, string step = "")
        {
            QueryRecord queryRecord;
            if (QueryRecords.ContainsKey(name))
                QueryRecords.TryRemove(name,out queryRecord);

            queryRecord = new QueryRecord()
            {
                QueryStartTime = DateTime.Now,
                Name = name,
                Step = step
            };

            if (IsTimingEnabled) queryRecord.StopWatch = new System.Diagnostics.Stopwatch();
            QueryRecords.TryAdd(name, queryRecord);

            if (ProgressCallback != null)
                ProgressCallback(queryRecord);

            if (IsVerbose)
                Console.WriteLine(queryRecord.GetQueryStartedMessage());
        }

        public void MarkQueryComplete(string name, int? count = null)
        {
            var queryRecord = QueryRecords[name];

            if (IsTimingEnabled)
            {
                queryRecord.StopWatch.Stop();
                queryRecord.QueryDuration = queryRecord.StopWatch.Elapsed;
                queryRecord.StopWatch.Restart();
            }
            queryRecord.QueryCount = count;
            queryRecord.Step = "QueryComplete";

            if (ProgressCallback != null)
                ProgressCallback(queryRecord);


            if (IsVerbose)
                Console.WriteLine(queryRecord.GetQueryCompletedMessage());
        }

        public void MarkLoadComplete(string name, int? count = null, string step = "LoadComplete")
        {
            var queryRecord = QueryRecords[name];

            if (IsTimingEnabled)
            {
                queryRecord.StopWatch.Stop();
                queryRecord.LoadDuration = queryRecord.StopWatch.Elapsed;
            }
            queryRecord.LoadCount = count;
            queryRecord.Step = step;

            if (ProgressCallback != null)
                ProgressCallback(queryRecord);

            if (IsVerbose)
                Console.WriteLine(queryRecord.GetLoadCompletedMessage());
        }
        
        public DateTime? GetLastQueryDate(string name)
        {
            if (!QueryRecords.ContainsKey(name)) return null;
            return QueryRecords[name].QueryStartTime;
        }

        public void Write()
        {
            if (!IsVerbose) return;
            var c1 = Math.Max(QueryRecords.Max(x => x.Key.Length), 4) + 1;
            var c2 = Math.Max(QueryRecords.Max(x => x.Value.QueryCount.ToString().Length), 4) + 1;
            var c3 = 20; // Math.Max(QueryRecords.Where(x => x.Value.QueryDuration.HasValue).Max(x => x.Value.QueryDuration.Value.TotalSeconds.ToString("#0.000").Length), 4) + 1;
            var c4 = Math.Max(QueryRecords.Max(x => x.Value.LoadCount.ToString().Length), 4) + 1;
            var c5 = Math.Max(QueryRecords.Where(x => x.Value.LoadDuration.HasValue).Max(x => x.Value.LoadDuration.Value.TotalSeconds.ToString("0.000").Length), 4) + 1;

            Console.Write("Item".PadRight(c1, ' '));
            Console.Write("Fech".PadLeft(c2, ' '));
            Console.Write("Secs".PadLeft(c3, ' '));
            Console.Write("Load".PadLeft(c4, ' '));
            Console.Write("Secs".PadLeft(c5, ' '));
            Console.WriteLine();
            Console.Write("".PadLeft(c1, '-') + " ");
            Console.Write("".PadLeft(c2 - 1, '-') + " ");
            Console.Write("".PadLeft(c3 - 1, '-') + " ");
            Console.Write("".PadLeft(c4 - 1, '-') + " ");
            Console.Write("".PadLeft(c5 - 1, '-') + " ");
            Console.WriteLine();

            foreach (var item in QueryRecords)
            {
                Console.Write(item.Key.PadRight(c1, ' '));
                Console.Write(item.Value.QueryCount.ToString().PadLeft(c2, ' '));
                if (item.Value.QueryDuration.HasValue)
                    Console.Write(item.Value.QueryDuration.Value.TotalSeconds.ToString("#0.000").PadLeft(c5, ' '));
                else
                    Console.Write("".PadLeft(c5, ' '));
                Console.Write(item.Value.LoadCount.ToString().PadLeft(c4, ' '));
                if (item.Value.LoadDuration.HasValue)
                    Console.Write(item.Value.LoadDuration.Value.TotalSeconds.ToString("#0.000").PadLeft(c5, ' '));
                else
                    Console.Write("".PadLeft(c5, ' '));

                Console.WriteLine();
            }
        }
    }
}

