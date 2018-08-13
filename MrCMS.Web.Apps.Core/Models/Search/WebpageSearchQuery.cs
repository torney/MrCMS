using System;
using System.ComponentModel;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Util;
using MrCMS.Entities.Documents.Web;
using MrCMS.Indexing.Definitions;
using MrCMS.Indexing.Management;

namespace MrCMS.Web.Apps.Core.Models.Search
{
    public class WebpageSearchQuery
    {
        public WebpageSearchQuery()
        {
            Page = 1;
        }

        public string Term { get; set; }
        public int Page { get; set; }
        public Webpage Parent { get; set; }
        public string Type { get; set; }

        [DisplayName("Created On From")]
        public DateTime? CreatedOnFrom { get; set; }

        [DisplayName("Created On To")]
        public DateTime? CreatedOnTo { get; set; }

        public Query GetQuery()
        {
            // TODO: refactor to outside of model
            var booleanQuery = new BooleanQuery
            {
                {
                    new TermRangeQuery(
                        FieldDefinition.GetFieldName<PublishedOnFieldDefinition>(), null,
                        // TODO: replace utcnow with site now
                        new BytesRef( DateTools.DateToString(DateTime.UtcNow, DateTools.Resolution.SECOND)), false, true),
                    Occur.MUST
                }
            };
            if (!string.IsNullOrWhiteSpace(Term))
            {
                //var indexDefinition = IndexingHelper.Get<WebpageSearchIndexDefinition>();
                //var analyser = indexDefinition.GetAnalyser();
                //var parser = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30,
                //    indexDefinition.SearchableFieldNames, analyser);
                //Query query = Term.SafeGetSearchQuery(parser, analyser);

                //booleanQuery.Add(query, Occur.MUST);
            }

            if (CreatedOnFrom.HasValue || CreatedOnTo.HasValue)
                booleanQuery.Add(GetDateQuery(), Occur.MUST);
            if (!string.IsNullOrEmpty(Type))
                booleanQuery.Add(new TermQuery(new Term(FieldDefinition.GetFieldName<TypeFieldDefinition>(), Type)),
                    Occur.MUST);
            if (Parent != null)
                booleanQuery.Add(
                    new TermQuery(new Term(FieldDefinition.GetFieldName<ParentIdFieldDefinition>(),
                        Parent.Id.ToString())), Occur.MUST);

            return booleanQuery;
        }

        private Query GetDateQuery()
        {
            return new TermRangeQuery(FieldDefinition.GetFieldName<CreatedOnFieldDefinition>(),
                CreatedOnFrom.HasValue
                    ? new BytesRef(DateTools.DateToString(CreatedOnFrom.Value, DateTools.Resolution.SECOND))
                    : null,
                CreatedOnTo.HasValue
                    ? new BytesRef(DateTools.DateToString(CreatedOnTo.Value, DateTools.Resolution.SECOND))
                    : null, CreatedOnFrom.HasValue, CreatedOnTo.HasValue);
        }
    }
}