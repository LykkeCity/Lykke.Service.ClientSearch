﻿using Lucene.Net.Analysis.Standard;
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
    public class IndexInfo
    {
        public static IndexedData GetIndexedData(string clientId)
        {
            IndexedData result;

            Lucene.Net.Store.Directory dir = Indexer.IndexDirectory;

            IndexReader reader = DirectoryReader.Open(dir);
            IndexSearcher searcher = new IndexSearcher(reader);

            using (var rAnalyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48))
            {
                TermQuery q = new TermQuery(new Term("ClientId", clientId));

                var collector = TopScoreDocCollector.Create(1000000, true);
                searcher.Search(q, collector);
                searcher.Similarity = new DefaultSimilarity();

                List<string> matchingResults = new List<string>();

                TopDocs topDocs = collector.GetTopDocs(0, collector.TotalHits);
                foreach (ScoreDoc scoreDoc in topDocs.ScoreDocs)
                {
                    Document doc = searcher.Doc(scoreDoc.Doc);
                    matchingResults.Add(doc.GetField("ClientId").GetStringValue());

                    result = new IndexedData();
                    result.ClientId = doc.GetField("ClientId").GetStringValue();
                    string val = doc.GetField("ClientNameAndDayOfBirth").GetStringValue();
                    result.ClientNameAndDayOfBirth = FullTextSearchCommon.DecodeFromIndex(val);
                    return result;
                }
            }

            return null;
        }
    }
}
