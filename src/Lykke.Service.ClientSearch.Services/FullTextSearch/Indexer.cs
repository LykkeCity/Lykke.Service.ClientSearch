using Common.Log;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search.Similarities;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Lykke.Service.ClientSearch.Core.Services;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PersonalData.Contract.Models;
using Polly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.ClientSearch.Services.FullTextSearch
{
    public partial class Indexer
    {
        public Lucene.Net.Store.Directory IndexDirectory { get; } = new RAMDirectory();

        private FieldType storeFieldType;
        private FieldType searchFieldType;
        private FieldType exactTextSearchFieldType;

        private readonly IPersonalDataService _personalDataService;
        private readonly ILog _log;

        public Indexer(
            IPersonalDataService personalDataService,
            ILog log
            )
        {
            _personalDataService = personalDataService;
            _log = log;

            InitializeIndexFieldTypes();
        }

        private void InitializeIndexFieldTypes()
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

            exactTextSearchFieldType = new FieldType();
            exactTextSearchFieldType.IndexOptions = IndexOptions.DOCS_ONLY;
            exactTextSearchFieldType.IsIndexed = true;
            exactTextSearchFieldType.IsStored = true;
            exactTextSearchFieldType.IsTokenized = false;
            exactTextSearchFieldType.OmitNorms = true;
            exactTextSearchFieldType.Freeze();
        }

        private int CreateIndex(IEnumerable<IPersonalData> docsToIndex, IEnumerable<string> docsToDelete)
        {
            using (var wAnalyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48))
            {
                IndexWriterConfig config = new IndexWriterConfig(LuceneVersion.LUCENE_48, wAnalyzer);
                config.OpenMode = OpenMode.CREATE_OR_APPEND;
                config.Similarity = new DefaultSimilarity();

                using (var writer = new IndexWriter(IndexDirectory, config))
                {
                    if (docsToIndex != null)
                    {
                        foreach (IPersonalData pd in docsToIndex)
                        {
                            var doc = new Document();
                            doc.Add(new Field("ClientId", pd.Id, storeFieldType));

                            string nameAndDoB = CreateNameAndDOBToIndex(pd);
                            if (nameAndDoB != null)
                            {
                                doc.Add(new Field("ClientNameAndDayOfBirth", nameAndDoB, exactTextSearchFieldType));
                            }

                            writer.UpdateDocument(new Term("ClientId", pd.Id), doc, wAnalyzer);
                        }
                    }

                    if (docsToDelete != null)
                    {
                        foreach (string clientId in docsToDelete)
                        {
                            writer.DeleteDocuments(new Term("ClientId", clientId));
                        }
                    }

                    writer.Commit();

                    return writer.NumDocs;
                }
            }
        }

        private string CreateNameAndDOBToIndex(IPersonalData pd)
        {
            if (!pd.DateOfBirth.HasValue)
            {
                return null;
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
            string utf8FullNameAndDoB = FullTextSearchCommon.EncodeForIndex(fullNameAndDoB);
            return utf8FullNameAndDoB;
        }

        public async Task IndexSingleDocumentAsync(string clientId, IPersonalData docToIndex)
        {
            if (docToIndex == null)
            {
                CreateIndex(null, new string[] { clientId });
                await _log.WriteInfoAsync(nameof(Indexer), nameof(IndexSingleDocumentAsync), $"client {clientId} removed from index");
            }
            else
            {
                CreateIndex(new IPersonalData[] { docToIndex }, null);
                await _log.WriteInfoAsync(nameof(Indexer), nameof(IndexSingleDocumentAsync), $"client {clientId} reindexed");
            }
        }


    }
}
