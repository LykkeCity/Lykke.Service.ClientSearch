using Lucene.Net.Analysis.Core;
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
using System.Text;
using System.Text.Encodings.Web;

namespace Lykke.Service.ClientSearch.FullTextSearch
{
    public class Indexer
    {
        //public static Lucene.Net.Store.Directory IndexDirectory = FSDirectory.Open(new DirectoryInfo("/Projects.Lykke/index.dir"));
        public static Lucene.Net.Store.Directory IndexDirectory = new RAMDirectory();

        private static FieldType storeFieldType;
        private static FieldType searchFieldType;
        private static FieldType phraseSearchFieldType;

        private static char[] reservedChars = new char[] { '+', '-', '&', '|', '!', '(', ')', '{', '}', '[', ']', '^', '"', '~', '*', '?', ':', '\\', '/', ',', '.', ';' };


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
            //searchFieldType.StoreTermVectorOffsets = true;
            //searchFieldType.StoreTermVectorPayloads = true;
            //searchFieldType.StoreTermVectorPositions = true;
            //searchFieldType.StoreTermVectors = true;
            searchFieldType.Freeze();

            phraseSearchFieldType = new FieldType();
            phraseSearchFieldType.IndexOptions = IndexOptions.DOCS_AND_FREQS_AND_POSITIONS;
            phraseSearchFieldType.IsIndexed = true;
            phraseSearchFieldType.IsStored = true;
            phraseSearchFieldType.IsTokenized = true;
            phraseSearchFieldType.OmitNorms = true;
            //phraseSearchFieldType.StoreTermVectorOffsets = true;
            //phraseSearchFieldType.StoreTermVectorPayloads = true;
            //phraseSearchFieldType.StoreTermVectorPositions = true;
            //phraseSearchFieldType.StoreTermVectors = true;
            phraseSearchFieldType.Freeze();

        }


        public static void CreateIndex(IEnumerable<PersonalDataEntity> docsToIndex)
        {

            using (var wAnalyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48))
            //using (var wAnalyzer = new WhitespaceAnalyzer(LuceneVersion.LUCENE_48))
            {
                IndexWriterConfig config = new IndexWriterConfig(LuceneVersion.LUCENE_48, wAnalyzer);
                config.OpenMode = OpenMode.CREATE_OR_APPEND;
                //config.Similarity = new BM25Similarity();
                config.Similarity = new DefaultSimilarity();
                //config.OpenMode = OpenMode.CREATE;

                using (var writer = new IndexWriter(IndexDirectory, config))
                {
                    //List<string> indexedValues = new List<string>();

                    foreach (PersonalDataEntity pd in docsToIndex)
                    {
                        //string indexedValue = "";

                        try
                        {
                            bool somethingToIndex = false;

                            string nameToIndex = pd.FullName ?? "";
                            if (!String.IsNullOrWhiteSpace(pd.FirstName) && !nameToIndex.Contains(pd.FirstName))
                            {
                                nameToIndex += " " + pd.FirstName;
                            }
                            if (!String.IsNullOrWhiteSpace(pd.LastName) && !nameToIndex.Contains(pd.LastName))
                            {
                                nameToIndex += " " + pd.LastName;
                            }

                            string id = pd.Id;

                            var doc = new Document();
                            doc.Add(new Field("ClientId", id, storeFieldType));
                            nameToIndex = nameToIndex.Trim();
                            if (nameToIndex.Length > 0)
                            {
                                doc.Add(new Field("Name", nameToIndex, searchFieldType));
                                somethingToIndex = true;
                            }

                            /*
                            if (!String.IsNullOrWhiteSpace(pd.Address))
                            {
                                doc.Add(new Field("Address", addressToIndex, searchFieldType));
                                somethingToIndex = true;
                            }
                            */

                            string fullName = pd.FullName;
                            string firstAndLastName = $"{pd.FirstName} {pd.LastName}";
                            if (string.IsNullOrWhiteSpace(fullName))
                            {
                                fullName = firstAndLastName;
                            }

                            if (!String.IsNullOrWhiteSpace(fullName))
                            {
                                doc.Add(new Field("FullName", fullName, searchFieldType));
                                somethingToIndex = true;
                            }

                            if (!String.IsNullOrWhiteSpace(firstAndLastName))
                            {
                                doc.Add(new Field("FirstAndLastName", firstAndLastName, searchFieldType));
                                somethingToIndex = true;
                            }

                            if (!String.IsNullOrWhiteSpace(fullName))
                            {
                                // commented because search switched from FullName to FirstName + LastName on Alexander Rumyantsev request
                                //string fullNameAndDoB = $"{fullName} {pd.DateOfBirth.ToString(FullTextSearchCommon.DateTimeFormat)}";

                                // search switched from FullName to FirstName + LastName on Alexander Rumyantsev request
                                string fullNameAndDoB = $"{firstAndLastName} {pd.DateOfBirth.ToString(FullTextSearchCommon.DateTimeFormat)}";

                                //doc.Add(new Field("ClientNameAndDayOfBirth", fullNameAndDoB, phraseSearchFieldType));

                                string utf8FullNameAndDoB = HtmlEncoder.Default.Encode(fullNameAndDoB); // encode special symbols
                                utf8FullNameAndDoB = utf8FullNameAndDoB.Replace("&#x", "#");
                                foreach (char chToReplace in reservedChars)
                                {
                                    utf8FullNameAndDoB = utf8FullNameAndDoB.Replace(chToReplace + "", String.Format("#{0:X}", Convert.ToInt32(chToReplace)));
                                }

                                doc.Add(new Field("ClientNameAndDayOfBirth", utf8FullNameAndDoB, phraseSearchFieldType));

                                //indexedValue = $"{fullNameAndDoB} || {utf8Name}";
                                somethingToIndex = true;
                            }

                            if (somethingToIndex)
                            {
                                /*
                                if (!String.IsNullOrWhiteSpace(indexedValue))
                                {
                                    indexedValue += " " + id;
                                }
                                */
                                writer.UpdateDocument(new Term("ClientId", id), doc, wAnalyzer);
                            }
                            else
                            {
                                writer.DeleteDocuments(new Term("ClientId", id));
                            }
                            /*
                            if (!String.IsNullOrWhiteSpace(indexedValue))
                            {
                                indexedValues.Add(indexedValue);
                            }
                            */

                            writer.Commit();
                        }
                        catch (Exception ex)
                        {

                        }
                    }



                    //File.AppendAllLines("D:/Projects.Lykke/tmp/iiiii.htm", indexedValues);

                    //writer.Commit();
                }
            }
        }

        public static void IndexSingleDocument(PersonalDataEntity docToIndex)
        {
            CreateIndex(new PersonalDataEntity[] { docToIndex });
        }


    }
}
