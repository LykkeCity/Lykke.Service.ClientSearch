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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.ClientSearch.FullTextSearch.FullTextSearch
{
    public class Searcher
    {
        private static Object thisLock = new Object();

        public static IEnumerable<ClientFulltextSearchResultItem> Search(IList<ClientFulltextSearchRequestItem> requestItems, int top = 3)
        {
            ClientFulltextSearchResultItem[] result = new ClientFulltextSearchResultItem[requestItems.Count];

            int n = 0;
            foreach (ClientFulltextSearchRequestItem requestItem in requestItems)
            {
                requestItem.OrderNumber = n++;
            }

            /*
            foreach (ClientFulltextSearchRequestItem requestItem in requestItems)
            {
                ClientFulltextSearchResultItem resultItem = new ClientFulltextSearchResultItem();
                resultItem.Name = requestItem.Name;
                resultItem.Address = requestItem.Address;
                resultItem.BackOfficeResultItems = Search(requestItem.AssetId, requestItem.Name, requestItem.Address, top);
                result[requestItem.OrderNumber] = resultItem;
            }
            */

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

            char[] reservedChars = new char[] { '+', '-', '&', '|', '!', '(', ')', '{', '}', '[', ']', '^', '"', '~', '*', '?', ':', '\\', '/', ',' };

            StringBuilder sb = new StringBuilder();
            if (!String.IsNullOrWhiteSpace(name))
            {
                string queryName = name.ToLower();
                foreach (char chToReplace in reservedChars)
                {
                    queryName = queryName.Replace(chToReplace, ' ');
                }
                string[] words = queryName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string[] uniqueWords = words.Distinct().ToArray();
                sb.Append("Name:(");
                foreach (string word in uniqueWords)
                {
                    if (word.Length > 5)
                    {
                        sb.Append(word).Append("~0.65 ");
                    }
                    else if (word.Length == 5)
                    {
                        sb.Append(word).Append("~0.65 ");
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
                sb.Append(") ");
            }


            /*
            if (!String.IsNullOrWhiteSpace(addr))
            {
                string queryAddr = addr.ToLower();
                foreach (char chToReplace in reservedChars)
                {
                    queryAddr = queryAddr.Replace(chToReplace, ' ');
                }
                if (sb.Length > 0)
                {
                    sb.Append(" ");
                }
                sb.Append($"Address:({queryAddr})");
            }
            */

            if (sb.Length == 0)
            {
                return new List<ClientFulltextSearchResultBackOfficeItem>();
            }

            string queryStr = sb.ToString();

            using (var rAnalyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48))
            {
                QueryParser parser = new QueryParser(LuceneVersion.LUCENE_48, "Name", rAnalyzer);
                Query q = parser.Parse(queryStr);

                //var q = new FuzzyQuery(new Term("Name", name));

                /*
                if (!String.IsNullOrWhiteSpace(name))
                {
                    string queryName = name.ToLower();
                    string[] words = queryName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string word in words)
                    {
                        q.Add(new Term("Name", word));
                    }
                }
                */

                var collector = TopScoreDocCollector.Create(10, true);
                searcher.Search(q, collector);
                searcher.Similarity = new DefaultSimilarity();

                List<ClientFulltextSearchResultBackOfficeItem> matchingResults = new List<ClientFulltextSearchResultBackOfficeItem>();
                List<string> explains = new List<string>();

                TopDocs topDocs = collector.GetTopDocs(0, collector.TotalHits);
                foreach (ScoreDoc scoreDoc in topDocs.ScoreDocs)
                {
                    Document doc = searcher.Doc(scoreDoc.Doc);

                    Explanation expl = searcher.Explain(q, scoreDoc.Doc);
                    Explanation[] explDetails = expl.GetDetails();

                    float coord = 1f;
                    int wordsMatched = 0;
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

                    bool hit = false;
                    if (coord > 0.51 || wordsMatched > 1)
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

                        //resultItem.Score = coord.ToString();

                        //string indexedName = doc.GetField("IndexedName").GetStringValue();
                        

                        //resultItem.Score = explDetails[explDetails.Length - 1].ToString();

                        matchingResults.Add(resultItem);
                    }


                    explains.Add(expl.ToHtml());
                }

                /*
                lock(thisLock)
                {
                    File.AppendAllLines("D:/Projects.Lykke/tmp/1234.htm", explains);
                }
                */

                return matchingResults;
            }

        }

    }

}

