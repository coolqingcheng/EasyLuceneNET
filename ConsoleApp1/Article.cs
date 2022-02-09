using Lucene.Net.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Article
    {
        [Lucene(FieldStore = Field.Store.YES, IsUnique = true, type = LuceneFieldType.Int32)]
        public int Id { get; set; }
        [Lucene(FieldStore = Field.Store.YES, IsUnique = false, type = LuceneFieldType.Text)]
        public string Title { get; set; }


        [Lucene(FieldStore = Field.Store.YES, IsUnique = false, type = LuceneFieldType.Text)]
        public string Content { get; set; }
    }
}
