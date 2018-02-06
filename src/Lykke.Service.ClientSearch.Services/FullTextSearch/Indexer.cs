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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.ClientSearch.Services.FullTextSearch
{
    public partial class Indexer
    {
        //public Lucene.Net.Store.Directory IndexDirectory = FSDirectory.Open(new DirectoryInfo("/Projects.Lykke/index.dir"));
        public Lucene.Net.Store.Directory IndexDirectory { get; } = new RAMDirectory();

        private FieldType storeFieldType;
        private FieldType searchFieldType;
        private FieldType phraseSearchFieldType;

        private readonly ITriggerManager _triggerManager;
        private readonly IPersonalDataService _personalDataService;
        private readonly ILog _log;

        public Indexer(
            ITriggerManager triggerManager,
            IPersonalDataService personalDataService,
            ILog log
            )
        {
            _triggerManager = triggerManager;
            _personalDataService = personalDataService;
            _log = log;

            InitializeIndexFieldTypes();
        }

        public void Initialize()
        {
            Task task = Task.Factory.StartNew(async () =>
            {
                await LoadAllPersonalDataForIndexing();
            });
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

            phraseSearchFieldType = new FieldType();
            phraseSearchFieldType.IndexOptions = IndexOptions.DOCS_AND_FREQS_AND_POSITIONS;
            phraseSearchFieldType.IsIndexed = true;
            phraseSearchFieldType.IsStored = true;
            phraseSearchFieldType.IsTokenized = true;
            phraseSearchFieldType.OmitNorms = true;
            phraseSearchFieldType.Freeze();
        }

        private void CreateIndex(IEnumerable<IPersonalData> docsToIndex, IEnumerable<string> docsToDelete)
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
                            bool somethingToIndex = false;

                            var doc = new Document();
                            doc.Add(new Field("ClientId", pd.Id, storeFieldType));

                            string nameAndDoB = CreateNameAndDOBToIndex(pd);
                            if (nameAndDoB != null)
                            {
                                doc.Add(new Field("ClientNameAndDayOfBirth", nameAndDoB, phraseSearchFieldType));
                                somethingToIndex = true;
                            }

                            if (somethingToIndex)
                            {
                                writer.UpdateDocument(new Term("ClientId", pd.Id), doc, wAnalyzer);
                            }
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

        public async Task IndexSingleDocument(string clientId, IPersonalData docToIndex)
        {
            if (docToIndex == null)
            {
                CreateIndex(null, new string[] { clientId });
                await _log.WriteInfoAsync(nameof(Indexer), nameof(IndexSingleDocument), $"client {clientId} removed from index");
            }
            else
            {
                CreateIndex(new IPersonalData[] { docToIndex }, null);
                await _log.WriteInfoAsync(nameof(Indexer), nameof(IndexSingleDocument), $"client {clientId} reindexed");
            }
        }


    }
}
