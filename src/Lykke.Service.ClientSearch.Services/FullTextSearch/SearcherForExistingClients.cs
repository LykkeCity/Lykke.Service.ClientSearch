using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Search.Similarities;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;

namespace Lykke.Service.ClientSearch.Services.FullTextSearch
{
    public class SearcherForExistingClients
    {
        public static IList<string> Search(string name, DateTime dateOfBirth)
        {
            if (String.IsNullOrWhiteSpace(name) || dateOfBirth == DateTime.MinValue)
            {
                return null;
            }

            Lucene.Net.Store.Directory dir = Indexer.IndexDirectory;

            IndexReader reader = DirectoryReader.Open(dir);
            IndexSearcher searcher = new IndexSearcher(reader);

            name = name.Trim();
            string beginningToRemove = FullTextSearchCommon.JUMIO_NA + " ";
            if (name.StartsWith(beginningToRemove))
            {
                name = name.Substring(beginningToRemove.Length);
            }
            string endingToRemove = " " + FullTextSearchCommon.JUMIO_NA;
            if (name.EndsWith(endingToRemove))
            {
                name = name.Substring(0, name.Length - endingToRemove.Length);
            }

            string namePart = name.ToLower();
            namePart = HtmlEncoder.Default.Encode(namePart); // encode special symbols
            namePart = namePart.Replace("&#x", "#");
            foreach (char chToReplace in FullTextSearchCommon.ReservedChars)
            {
                namePart = namePart.Replace(chToReplace + "", String.Format("#{0:X}", Convert.ToInt32(chToReplace)));
            }

            StringBuilder sb = new StringBuilder();
            if (!String.IsNullOrWhiteSpace(name))
            {
                string phrase = $"{namePart} {dateOfBirth.ToString(FullTextSearchCommon.DateTimeFormat)}";
                sb.Append($"ClientNameAndDayOfBirth: \"{phrase}\"");
            }

            if (sb.Length == 0)
            {
                return null;
            }

            string queryStr = sb.ToString();


            using (var rAnalyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48))
            {
                QueryParser parser = new QueryParser(LuceneVersion.LUCENE_48, "ClientNameAndDayOfBirth", rAnalyzer);
                Query q = parser.Parse(queryStr);

                var collector = TopScoreDocCollector.Create(1000000, true);
                searcher.Search(q, collector);
                searcher.Similarity = new DefaultSimilarity();

                List<string> matchingResults = new List<string>();

                TopDocs topDocs = collector.GetTopDocs(0, collector.TotalHits);
                foreach (ScoreDoc scoreDoc in topDocs.ScoreDocs)
                {
                    Document doc = searcher.Doc(scoreDoc.Doc);
                    matchingResults.Add(doc.GetField("ClientId").GetStringValue());
                }

                return matchingResults;
            }

        }

    }

}

