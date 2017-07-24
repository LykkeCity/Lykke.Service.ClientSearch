using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Lykke.Service.ClientSearch.Core.FullTextSearch;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.ClientSearch.FullTextSearch.FullTextSearch
{
    public class Searcher
    {
        public static IEnumerable<ClientFulltextSearchResultItem> Search(IList<ClientFulltextSearchRequestItem> requestItems, int top = 3)
        {
            ClientFulltextSearchResultItem[] result = new ClientFulltextSearchResultItem[requestItems.Count];

            int n = 0;
            foreach (ClientFulltextSearchRequestItem requestItem in requestItems)
            {
                requestItem.OrderNumber = n++;

                ClientFulltextSearchResultItem resultItem = new ClientFulltextSearchResultItem();
                resultItem.Name = requestItem.Name;
                resultItem.Address = requestItem.Address;
                resultItem.BackOfficeResultItems = Search(requestItem.AssetId, requestItem.Name, requestItem.Address, top);
                result[requestItem.OrderNumber] = resultItem;
            }


            /*
            Parallel.ForEach<ClientFulltextSearchRequestItem>(requestItems, _ => 
            {
                ClientFulltextSearchResultItem resultItem = new ClientFulltextSearchResultItem();
                resultItem.Name = _.Name;
                resultItem.Address = _.Address;
                resultItem.BackOfficeResultItems = Search(_.Name, _.Address, top);
                result[_.OrderNumber] = resultItem;
            });
            */


            return result;
        }

        public static IList<ClientFulltextSearchResultBackOfficeItem> Search(string assetId, string name, string addr, int top)
        {
            Lucene.Net.Store.Directory dir = Indexer.IndexDirectory;

            IndexReader reader = DirectoryReader.Open(dir);
            IndexSearcher searcher = new IndexSearcher(reader);

            char[] reservedChars = new char[] { '+', '-', '&', '|', '!', '(', ')', '{', '}', '[', ']', '^', '"', '~', '*', '?', ':', '\\', '/' };

            StringBuilder sb = new StringBuilder();
            if (!String.IsNullOrWhiteSpace(name))
            {
                string queryName = name.ToLower();
                foreach(char chToReplace in reservedChars)
                {
                    queryName = queryName.Replace(chToReplace, ' ');
                }
                sb.Append($"Name:({queryName}) ");
            }
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

            if (sb.Length == 0)
            {
                return new List<ClientFulltextSearchResultBackOfficeItem>();
            }

            string queryStr = sb.ToString();

            using (var rAnalyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48))
            {
                QueryParser parser = new QueryParser(LuceneVersion.LUCENE_48, "Name", rAnalyzer);
                Query q = parser.Parse(queryStr);

                /*
                Term term = new Term("Name", queryStr);
                Query q = new FuzzyQuery(term);
                */

                var collector = TopScoreDocCollector.Create(100, true);
                searcher.Search(q, collector);

                List<ClientFulltextSearchResultBackOfficeItem> matchingResults = new List<ClientFulltextSearchResultBackOfficeItem>();
                //List<string> explains = new List<string>();

                TopDocs topDocs = collector.GetTopDocs(0, collector.TotalHits);
                foreach (ScoreDoc scoreDoc in topDocs.ScoreDocs)
                {
                    Document doc = searcher.Doc(scoreDoc.Doc);

                    ClientFulltextSearchResultBackOfficeItem resultItem = new ClientFulltextSearchResultBackOfficeItem();

                    resultItem.AssetId = assetId;
                    resultItem.ClientId = doc.GetField("ClientId").GetStringValue();
                    resultItem.BackOfficeName = doc.GetField("Name").GetStringValue();
                    resultItem.BackOfficeAddress = doc.GetField("Address")?.GetStringValue();

                    matchingResults.Add(resultItem);

                    //explains.Add(searcher.Explain(q, scoreDoc.Doc).ToHtml());
                }
                return matchingResults;
            }

        }

    }
}

