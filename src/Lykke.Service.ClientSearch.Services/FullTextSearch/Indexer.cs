using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Search.Similarities;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Lykke.Service.ClientSearch.AzureRepositories.PersonalData;
using Lykke.Service.ClientSearch.Services.FullTextSearch;
using System;
using System.Collections.Generic;
using System.IO;

namespace Lykke.Service.ClientSearch.FullTextSearch
{
    public class Indexer
    {
        //public static Lucene.Net.Store.Directory IndexDirectory = FSDirectory.Open(new DirectoryInfo("/Projects.Lykke/index.dir"));
        public static Lucene.Net.Store.Directory IndexDirectory = new RAMDirectory();

        public static void CreateIndex(IEnumerable<PersonalDataEntity> docsToIndex)
        {

            using (var wAnalyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48))
            {
                IndexWriterConfig config = new IndexWriterConfig(LuceneVersion.LUCENE_48, wAnalyzer);
                config.OpenMode = OpenMode.CREATE_OR_APPEND;
                //config.Similarity = new BM25Similarity();
                config.Similarity = new DefaultSimilarity();
                //config.OpenMode = OpenMode.CREATE;

                FieldType storeFieldType = new FieldType();
                storeFieldType.IndexOptions = IndexOptions.DOCS_ONLY;
                storeFieldType.IsIndexed = true;
                storeFieldType.IsStored = true;
                storeFieldType.IsTokenized = false;
                storeFieldType.OmitNorms = true;
                storeFieldType.Freeze();

                FieldType searchFieldType = new FieldType();
                searchFieldType.IndexOptions = IndexOptions.DOCS_ONLY;
                searchFieldType.IsIndexed = true;
                searchFieldType.IsStored = true;
                searchFieldType.IsTokenized = true;
                searchFieldType.OmitNorms = false;
                //searchFieldType.StoreTermVectorOffsets = true;
                //searchFieldType.StoreTermVectorPayloads = true;
                //searchFieldType.StoreTermVectorPositions = true;
                //searchFieldType.StoreTermVectors = true;
                searchFieldType.Freeze();

                try
                {
                }
                catch
                {

                }


                using (var writer = new IndexWriter(IndexDirectory, config))
                {

                    List<string> ccc = new List<string>();
                    foreach (PersonalDataEntity d in docsToIndex)
                    {
                        try
                        {
                            string nameToIndex = d.FullName ?? "";
                            if (!String.IsNullOrWhiteSpace(d.FirstName) && !nameToIndex.Contains(d.FirstName))
                            {
                                nameToIndex += " " + d.FirstName;
                            }
                            if (!String.IsNullOrWhiteSpace(d.LastName) && !nameToIndex.Contains(d.LastName))
                            {
                                nameToIndex += " " + d.LastName;
                            }

                            string addressToIndex = d.Address ?? "";

                            if (String.IsNullOrWhiteSpace(nameToIndex) && String.IsNullOrWhiteSpace(addressToIndex)) // nothing to index
                            {
                                continue;
                            }

                            string id = d.Id;

                            var doc = new Document();
                            doc.Add(new Field("ClientId", id, storeFieldType));
                            nameToIndex = nameToIndex.Trim();
                            if (nameToIndex.Length > 0)
                            {
                                doc.Add(new Field("Name", nameToIndex, searchFieldType));
                            }

                            if (d.Address != null)
                            {
                                doc.Add(new Field("Address", addressToIndex, searchFieldType));
                            }


                            writer.UpdateDocument(new Term("ClientId", id), doc, wAnalyzer);


                            writer.Commit();


                            try
                            {
                                if (!String.IsNullOrWhiteSpace(nameToIndex))
                                {
                                    ccc.Add(nameToIndex);
                                }
                            }
                            catch
                            {

                            }

                        }
                        catch (Exception ex)
                        {

                        }
                    }


                    File.AppendAllLines("D:/Projects.Lykke/tmp/iiiii.htm", ccc);

                    //writer.Commit();
                }
            }
        }
    }
}
