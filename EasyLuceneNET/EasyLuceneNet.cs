using jieba.NET;
using JiebaNet.Segmenter;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System.Reflection;

namespace EasyLuceneNET
{
    public class EasyLuceneNet : IEasyLuceneNet, IDisposable
    {
        const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
        readonly static IndexWriter writer;
        static EasyLuceneNet()
        {
            var indexPath = Path.Combine(AppContext.BaseDirectory, "indexs");

            using var dir = FSDirectory.Open(indexPath);

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
                    Console.WriteLine("添加到文档:" + DateTime.Now);
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
                Console.WriteLine("正在提交索引:" + begin);
                //writer.Flush(triggerMerge: false, applyAllDeletes: false);
                writer.Commit();
                var end = DateTime.Now;
                Console.WriteLine("索引提交完成:" + end);
                writer.Flush(false, false);
                writer.Commit();
            }
        }

        public void Dispose()
        {
            writer.Dispose();
        }

        public SearchResult<T> Search<T>(Query query) where T : class, new()
        {
            using var reader = writer.GetReader(applyAllDeletes: true);
            var searcher = new IndexSearcher(reader);
            var doc = searcher.Search(query, 20 /* top 20 */);
            var result = new SearchResult<T>
            {
                Total = doc.TotalHits
            };
            foreach (var item in doc.ScoreDocs)
            {
                var t = new T();
                foreach (var property in t.GetType().GetProperties())
                {
                    property.SetValue(t, searcher.Doc(item.Doc), null);
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