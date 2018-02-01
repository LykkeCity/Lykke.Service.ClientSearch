/*
using Lucene.Net.Search.Similarities;
using Lucene.Net.Util;
using Lucene.Net.Index;

namespace Lykke.Service.ClientSearch.Services.FullTextSearch
{
    public class CustomSimilarity : DefaultSimilarity
    {
        public override float ScorePayload(int doc, int start, int end, BytesRef payload)
        {
            if (payload.Bytes[payload.Offset] == 0)
            {
                return 0.0f;
            }
            return 1.0f;
        }

        public override float Idf(long docFreq, long numDocs)
        {
            return 1.0f;
        }

        public override float Tf(float freq)
        {
            return 1.0f;
        }

        public override float SloppyFreq(int distance)
        {
            return 1.0f;
        }

        public override float LengthNorm(FieldInvertState state)
        {
            return 1.0f;
        }

        public override float QueryNorm(float sumOfSquaredWeights)
        {
            return 1.0f;
        }

    }
}
*/