using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Lykke.Service.ClientSearch.AzureRepositories.PersonalData;
using System;
using System.Collections.Generic;


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
                //config.OpenMode = OpenMode.CREATE;

                FieldType idFieldType = new FieldType();
                idFieldType.IndexOptions = IndexOptions.DOCS_ONLY;
                idFieldType.IsIndexed = true;
                idFieldType.IsStored = true;
                idFieldType.IsTokenized = false;
                idFieldType.OmitNorms = false;
                idFieldType.Freeze();

                FieldType searchFieldType = new FieldType();
                searchFieldType.IndexOptions = IndexOptions.DOCS_ONLY;
                searchFieldType.IsIndexed = true;
                searchFieldType.IsStored = true;
                searchFieldType.IsTokenized = true;
                searchFieldType.OmitNorms = false;
                searchFieldType.Freeze();

                using (var writer = new IndexWriter(IndexDirectory, config))
                {
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

                            var doc = new Document();
                            doc.Add(new Field("ClientId", d.Id, idFieldType));
                            if (d.FullName != null)
                            {
                                doc.Add(new Field("Name", nameToIndex, searchFieldType));
                            }
                            if (d.Address != null)
                            {
                                doc.Add(new Field("Address", addressToIndex, searchFieldType));
                            }
                            writer.UpdateDocument(new Term("ClientId", d.Id), doc);
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    writer.Commit();
                }
            }
        }
    }
}
