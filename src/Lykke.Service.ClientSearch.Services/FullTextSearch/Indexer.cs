﻿using Common.Log;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search.Similarities;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Lykke.Service.PersonalData.Contract.Models;
using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;

namespace Lykke.Service.ClientSearch.Services.FullTextSearch
{
    public class Indexer
    {
        //public static Lucene.Net.Store.Directory IndexDirectory = FSDirectory.Open(new DirectoryInfo("/Projects.Lykke/index.dir"));
        public static Lucene.Net.Store.Directory IndexDirectory = new RAMDirectory();

        private static FieldType storeFieldType;
        private static FieldType searchFieldType;
        private static FieldType phraseSearchFieldType;

        static Indexer()
        {
            storeFieldType = new FieldType();
            storeFieldType.IndexOptions = IndexOptions.DOCS_ONLY;
            storeFieldType.IsIndexed = true;
            storeFieldType.IsStored = true;
            storeFieldType.IsTokenized = false;
            storeFieldType.OmitNorms = true;
            storeFieldType.Freeze();

            searchFieldType = new FieldType();
            searchFieldType.IndexOptions = IndexOptions.DOCS_ONLY;
            searchFieldType.IsIndexed = true;
            searchFieldType.IsStored = true;
            searchFieldType.IsTokenized = true;
            searchFieldType.OmitNorms = false;
            searchFieldType.Freeze();

            phraseSearchFieldType = new FieldType();
            phraseSearchFieldType.IndexOptions = IndexOptions.DOCS_AND_FREQS_AND_POSITIONS;
            phraseSearchFieldType.IsIndexed = true;
            phraseSearchFieldType.IsStored = true;
            phraseSearchFieldType.IsTokenized = true;
            phraseSearchFieldType.OmitNorms = true;
            phraseSearchFieldType.Freeze();
        }

        public static void CreateIndex(IEnumerable<IPersonalData> docsToIndex)
        {

            using (var wAnalyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48))
            {
                IndexWriterConfig config = new IndexWriterConfig(LuceneVersion.LUCENE_48, wAnalyzer);
                config.OpenMode = OpenMode.CREATE_OR_APPEND;
                config.Similarity = new DefaultSimilarity();

                using (var writer = new IndexWriter(IndexDirectory, config))
                {
                    //List<string> indexedValues = new List<string>();

                    foreach (IPersonalData pd in docsToIndex)
                    {
                        if (pd == null)
                        {
                            writer.DeleteDocuments(new Term("ClientId", id));
                        }
                        else
                        {
                            bool somethingToIndex = false;

                            string id = pd.Id;

                            var doc = new Document();
                            doc.Add(new Field("ClientId", id, storeFieldType));

                            /*
                            string nameToIndex = pd.FullName ?? "";
                            if (!String.IsNullOrWhiteSpace(pd.FirstName) && !nameToIndex.Contains(pd.FirstName))
                            {
                                nameToIndex += " " + pd.FirstName;
                            }
                            if (!String.IsNullOrWhiteSpace(pd.LastName) && !nameToIndex.Contains(pd.LastName))
                            {
                                nameToIndex += " " + pd.LastName;
                            }
                            nameToIndex = nameToIndex.Trim();
                            if (nameToIndex.Length > 0)
                            {
                                doc.Add(new Field("Name", nameToIndex, searchFieldType));
                                somethingToIndex = true;
                            }
                            */

                            string nameAndDoB = CreateNameAndDOBToIndex(pd);
                            if (nameAndDoB != null)
                            {
                                doc.Add(new Field("ClientNameAndDayOfBirth", nameAndDoB, phraseSearchFieldType));
                                somethingToIndex = true;
                            }


                            if (somethingToIndex)
                            {
                                writer.UpdateDocument(new Term("ClientId", id), doc, wAnalyzer);
                            }
                            else
                            {
                                writer.DeleteDocuments(new Term("ClientId", id));
                            }
                        }

                        writer.Commit();
                    }
                }
            }
        }

        public static string CreateNameAndDOBToIndex(IPersonalData pd)
        {
            if (!pd.DateOfBirth.HasValue)
            {
                return null;
            }

            if (pd.FirstName!= null && pd.FirstName.StartsWith("NATALI"))
            {

            }

            List<string> parts = new List<string>();
            if (pd.FirstName != FullTextSearchCommon.JUMIO_NA && !String.IsNullOrWhiteSpace(pd.FirstName))
            {
                parts.Add(pd.FirstName.Trim());
            }
            if (pd.LastName != FullTextSearchCommon.JUMIO_NA && !String.IsNullOrWhiteSpace(pd.LastName))
            {
                parts.Add(pd.LastName.Trim());
            }
            if (parts.Count == 0)
            {
                return null;
            }

            string firstAndLastName = String.Join(" ", parts);

            string fullNameAndDoB = $"{firstAndLastName} {pd.DateOfBirth.Value.ToString(FullTextSearchCommon.DateTimeFormat)}";

            string utf8FullNameAndDoB = HtmlEncoder.Default.Encode(fullNameAndDoB); // encode special symbols
            utf8FullNameAndDoB = utf8FullNameAndDoB.Replace("&#x", "#");
            foreach (char chToReplace in FullTextSearchCommon.ReservedChars)
            {
                utf8FullNameAndDoB = utf8FullNameAndDoB.Replace(chToReplace + "", String.Format("#{0:X}", Convert.ToInt32(chToReplace)));
            }

            return utf8FullNameAndDoB;
        }

        public static void IndexSingleDocument(string clientId, IPersonalData docToIndex, ILog log)
        {
            CreateIndex(new IPersonalData [] { docToIndex });
            if (docToIndex == null)
            {
                log.WriteInfoAsync(nameof(Indexer), nameof(IndexSingleDocument), $"client {clientId} removed from index");
            }
            else
            {
                log.WriteInfoAsync(nameof(Indexer), nameof(IndexSingleDocument), $"client {clientId} reindexed");
            }
        }


    }
}
