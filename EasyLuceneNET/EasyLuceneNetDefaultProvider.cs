using jieba.NET;
using JiebaNet.Segmenter;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EasyLuceneNET
{
    public class EasyLuceneNetDefaultProvider : IEasyLuceneNet, IDisposable
    {
        const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
        readonly IndexWriter writer;

        private ILogger _logger;

        private FSDirectory dir;

        public EasyLuceneNetDefaultProvider(ILogger<EasyLuceneNetDefaultProvider> logger)
        {
            _logger = logger;
            var indexPath = Path.Combine(AppContext.BaseDirectory, "indexs");

            dir = FSDirectory.Open(indexPath);

            // Create an analyzer to process the text
            var analyzer = new JieBaAnalyzer(TokenizerMode.Search);
            // Create an index writer
            var indexConfig = new IndexWriterConfig(AppLuceneVersion, analyzer);
            writer = new IndexWriter(dir, indexConfig);
        }

        public void AddIndex<T>(List<T> list)
        {
            if (list != null)
            {
                list.ForEach(item =>
                {
                    var doc = new Document();
                    var properties = item.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    _logger.LogDebug("添加到文档:" + DateTime.Now);
                    Term term = null;
                    foreach (var property in properties)
                    {
                        string name = property.Name;
                        var value = property.GetValue(item);
                        var att = property.GetCustomAttribute<LuceneAttribute>();
                        if (att == null || att.type == LuceneFieldType.String)
                        {
                            //默认用StringField  
                            doc.Add(new StringField(name, value.ToString(), Field.Store.YES));
                        }
                        else
                        {

                            if (att.type == LuceneFieldType.Text)
                            {
                                doc.Add(new TextField(name, value.ToString(), att.FieldStore));
                            }
                            if (att.type == LuceneFieldType.Int32)
                            {
                                doc.Add(new Int32Field(name, Convert.ToInt32(value), att.FieldStore));
                            }
                            if (att.IsUnique)
                            {
                                term = new Term(name, value.ToString());

                            }
                        }
                    }
                    if (term == null)
                    {
                        writer.AddDocument(doc);
                    }
                    else
                    {
                        writer.UpdateDocument(term, doc);
                    }

                });
                var begin = DateTime.Now;
                _logger.LogDebug("正在提交索引:" + begin);
                //writer.Flush(triggerMerge: false, applyAllDeletes: false);
                writer.Commit();
                var end = DateTime.Now;
                _logger.LogDebug("索引提交完成:" + end);
                writer.Flush(false, false);
                writer.Commit();
            }
        }

        public void Dispose()
        {
            writer.Dispose();
        }

        public SearchResult<T> Search<T>(string keyword, int index, int size, string[] fields) where T : class, new()
        {

            if (keyword.Length > 75)
            {
                keyword = keyword.Substring(0, 75);
            }
            if (index <= 1)
            {
                index = 1;
            }
            if (size < 15)
            {
                index = 15;
            }
            var result = new SearchResult<T>();
            var segmenter = new JiebaSegmenter();
            var keywords = segmenter.Cut(keyword);
            result.cutKeys.AddRange(keywords);
            var biaodian = "[’!\"#$%&\'()*+,-./:;<=>?@[\\]^_`{|}~]+（）【】，。： ".ToCharArray();
            keywords = keywords.Where(a => !biaodian.Where(b => b.ToString() == a).Any()).ToList();
            BooleanQuery query = new BooleanQuery();
            foreach (var item in keywords)
            {
                foreach (var field in fields)
                {
                    if (biaodian.Any(a => a.ToString() == item) == false)
                    {
                        query.Add(new TermQuery(new Term(field, item)), Occur.SHOULD);
                    }
                }
            }

            var i = index * size;

            using var reader = writer.GetReader(applyAllDeletes: true);
            var searcher = new IndexSearcher(reader);
            var doc = searcher.Search(query, 20 /* top 20 */);
            result.Total = doc.TotalHits;
            foreach (var hit in doc.ScoreDocs)
            {
                var foundDoc = searcher.Doc(hit.Doc);
                var t = new T();
                var type = t.GetType();
                var propertity = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

                foreach (var item in propertity)
                {
                    var sValue = foundDoc.Get(item.Name);
                    if (sValue != null)
                    {

                        try
                        {
                            var v = Convert.ChangeType(sValue, item.PropertyType);

                            item.SetValue(t, v, null);
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
                result.list.Add(t);

            }
            return result;
        }
    }
}

public class LuceneAttribute : System.Attribute
{
    public LuceneFieldType type { get; set; } = LuceneFieldType.Text;

    public Field.Store FieldStore { get; set; }

    public bool IsUnique { get; set; } = false;

}

public enum LuceneFieldType
{
    Text,
    /// <summary>
    /// 用于不需要检索的，如果需要检索，选择Text
    /// </summary>
    String,
    Int32
}