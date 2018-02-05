using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Search.Similarities;
using Lucene.Net.Util;
using Lykke.Service.ClientSearch.Core.FullTextSearch;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.ClientSearch.Services.FullTextSearch
{
    public class Searcher
    {
        private const double _minCoordToHit = 0.51;
        private const int _minWordsToHit = 1;

        public static IEnumerable<ClientFulltextSearchResultItem> Search(IList<ClientFulltextSearchRequestItem> requestItems, int top = 3)
        {
            ClientFulltextSearchResultItem[] result = new ClientFulltextSearchResultItem[requestItems.Count];

            int n = 0;
            foreach (ClientFulltextSearchRequestItem requestItem in requestItems)
            {
                requestItem.OrderNumber = n++;
            }

            Parallel.ForEach<ClientFulltextSearchRequestItem>(requestItems, _ => 
            {
                ClientFulltextSearchResultItem resultItem = new ClientFulltextSearchResultItem();
                resultItem.Name = _.Name;
                resultItem.Address = _.Address;
                resultItem.BackOfficeResultItems = Search(_.AssetId, _.Name, _.Address, top);
                result[_.OrderNumber] = resultItem;
            });

            return result;
        }

        public static IList<ClientFulltextSearchResultBackOfficeItem> Search(string assetId, string name, string addr, int top)
        {
            Lucene.Net.Store.Directory dir = Indexer.IndexDirectory;

            IndexReader reader = DirectoryReader.Open(dir);
            IndexSearcher searcher = new IndexSearcher(reader);

            string queryStr = BuildSearchQueryString(name);
            if (String.IsNullOrWhiteSpace(queryStr))
            {
                return new List<ClientFulltextSearchResultBackOfficeItem>();
            }

            List<ClientFulltextSearchResultBackOfficeItem> matchingResults = PerformSearch(assetId, searcher, queryStr);
            return matchingResults;
        }

        private static List<ClientFulltextSearchResultBackOfficeItem> PerformSearch(string assetId, IndexSearcher searcher, string queryStr)
        {
            List<ClientFulltextSearchResultBackOfficeItem> matchingResults = new List<ClientFulltextSearchResultBackOfficeItem>();

            using (var rAnalyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48))
            {
                QueryParser parser = new QueryParser(LuceneVersion.LUCENE_48, "Name", rAnalyzer);
                Query q = parser.Parse(queryStr);


                var collector = TopScoreDocCollector.Create(10, true);
                searcher.Search(q, collector);
                searcher.Similarity = new DefaultSimilarity();

                List<string> explains = new List<string>();

                TopDocs topDocs = collector.GetTopDocs(0, collector.TotalHits);
                foreach (ScoreDoc scoreDoc in topDocs.ScoreDocs)
                {
                    Document doc = searcher.Doc(scoreDoc.Doc);

                    ProcessFoundDoc(assetId, searcher, q, matchingResults, explains, scoreDoc, doc);
                }

            }

            return matchingResults;
        }

        private static void ProcessFoundDoc(string assetId, IndexSearcher searcher, Query q, List<ClientFulltextSearchResultBackOfficeItem> matchingResults, List<string> explains, ScoreDoc scoreDoc, Document doc)
        {
            Explanation expl = searcher.Explain(q, scoreDoc.Doc);
            Explanation[] explDetails = expl.GetDetails();

            float coord = 1f;
            int wordsMatched = 0;
            GetHitParams(explDetails, ref coord, ref wordsMatched);

            bool hit = false;
            if (coord > _minCoordToHit || wordsMatched > _minWordsToHit)
            {
                hit = true;
            }

            if (hit)
            {
                ClientFulltextSearchResultBackOfficeItem resultItem = new ClientFulltextSearchResultBackOfficeItem();

                resultItem.AssetId = assetId;
                resultItem.ClientId = doc.GetField("ClientId").GetStringValue();
                resultItem.BackOfficeName = doc.GetField("Name").GetStringValue();
                resultItem.BackOfficeAddress = doc.GetField("Address")?.GetStringValue();

                matchingResults.Add(resultItem);
            }


            explains.Add(expl.ToHtml());
        }

        private static void GetHitParams(Explanation[] explDetails, ref float coord, ref int wordsMatched)
        {
            if (explDetails[explDetails.Length - 1].ToString().Contains("coord"))
            {
                string s = explDetails[explDetails.Length - 1].ToString();
                string num = s.Substring(0, s.IndexOf(' ')).Trim().Replace(',', '.');
                float.TryParse(num, NumberStyles.Float, CultureInfo.InvariantCulture, out coord);

                if (s.IndexOf("/") > 0)
                {
                    string wordsMatchedStr = s.Substring(s.IndexOf("(") + 1, s.IndexOf("/") - 1 - s.IndexOf("("));
                    int.TryParse(wordsMatchedStr, out wordsMatched);
                }
            }
        }

        private static string BuildSearchQueryString(string name)
        {
            StringBuilder sb = new StringBuilder();
            if (!String.IsNullOrWhiteSpace(name))
            {
                string queryName = name.ToLower();
                foreach (char chToReplace in FullTextSearchCommon.ReservedChars)
                {
                    queryName = queryName.Replace(chToReplace, ' ');
                }
                string[] words = queryName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string[] uniqueWords = words.Distinct().ToArray();
                sb.Append("Name:(");
                foreach (string word in uniqueWords)
                {
                    AddQueryWordAndSimilarityForSearch(sb, word);
                }
                sb.Append(") ");
            }

            string queryStr = sb.ToString();
            return queryStr;
        }

        // add word similarity depending on word length
        private static void AddQueryWordAndSimilarityForSearch(StringBuilder sb, string word)
        {
            if (word.Length > 5)
            {
                sb.Append(word).Append("~0.79 ");
            }
            else if (word.Length == 5)
            {
                sb.Append(word).Append("~0.79 ");
            }
            else if (word.Length == 4)
            {
                sb.Append(word).Append("~0.74 ");
            }
            else if (word.Length == 3)
            {
                sb.Append(word).Append("~0.65 ");
            }
            else
            {
                sb.Append(word).Append(" ");
            }
        }
    }

}

