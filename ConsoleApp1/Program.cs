using jieba.NET;
using JiebaNet.Segmenter;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;

const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;

var indexPath = Path.Combine(AppContext.BaseDirectory, "indexs");

using var dir = FSDirectory.Open(indexPath);

// Create an analyzer to process the text
var analyzer = new JieBaAnalyzer(TokenizerMode.Search);

// Create an index writer
var indexConfig = new IndexWriterConfig(AppLuceneVersion, analyzer);
using var writer = new IndexWriter(dir, indexConfig);



#region 创建索引

for (int i = 0; i < 100; i++)
{
    var doc = new Document
{
    // StringField indexes but doesn't tokenize
    new StringField("name",
        "张三",
        Field.Store.YES),
    new TextField("desc",
        "小明硕士毕业于中国科学院计算所，后在日本京都大学深造",
        Field.Store.YES)
};

    writer.AddDocument(doc);
    writer.Flush(triggerMerge: false, applyAllDeletes: false);
}
writer.Flush(triggerMerge: true, applyAllDeletes: false);
writer.Commit();
#endregion

#region 查询

// Search with a phrase
var query = new BooleanQuery();
query.Add(new TermQuery(new Term("desc", "小明")), Occur.SHOULD);

using var reader = writer.GetReader(applyAllDeletes: true);
var searcher = new IndexSearcher(reader);
var hits = searcher.Search(query, 20 /* top 20 */).ScoreDocs;
// Display the output in a table
Console.WriteLine($"{"Score",10}" +
    $" {"Name",-15}" +
    $" {"Favorite Phrase",-40}");
foreach (var hit in hits)
{
    var foundDoc = searcher.Doc(hit.Doc);
    Console.WriteLine($"{hit.Score:f8}" +
        $" {foundDoc.Get("name"),-15}" +
        $" {foundDoc.Get("desc"),-40}");
}



#endregion




var segmenter = new JiebaSegmenter();
var s = "小明硕士毕业于中国科学院计算所，后在日本京都大学深造";
var tokens = segmenter.Tokenize(s, TokenizerMode.Search);

Console.WriteLine("");

