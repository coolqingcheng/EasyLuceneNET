基于https://github.com/SilentCC/JIEba-netcore封装了一个lucene.net的全文检索工具

# 使用

## 安装nuget包

```
Install-Package EasyLuceneNET -Version 1.0.0
```

## 创建模型

``` csharp
 public class Article
    {
        [Lucene(FieldStore = Field.Store.YES, IsUnique = true, type = LuceneFieldType.Int32)]
        public int Id { get; set; }
        [Lucene(FieldStore = Field.Store.YES, IsUnique = false, type = LuceneFieldType.Text)]
        public string Title { get; set; }


        [Lucene(FieldStore = Field.Store.YES, IsUnique = false, type = LuceneFieldType.Text)]
        public string Content { get; set; }
    }
```

## 依赖注入

``` csharp
var service = new ServiceCollection();
service.AddLogging();
service.AddEasyLuceneNet();
var serviceProvider = service.BuildServiceProvider();

var easy = serviceProvider.GetService<IEasyLuceneNet>();
```

## 创建索引

``` csharp


var list = new List<Article>();
for (int i = 0; i < 100; i++)
{
    list.Add(new Article()
    {
        Id = i,
        Title = i + "使用Xamarin开发移动应用示例——数独游戏（八）使用MVVM实现完成游戏列表页面",
        Content = @"前面我们已经完成了游戏的大部分功能，玩家可以玩预制的数独游戏，也可以自己添加新的游戏。现在我们实现展示已完成游戏列表页面，显示用户已经完成的游戏列表，从这个列表可以进入详细的复盘页面。

前面的页面我们采用的是传统的事件驱动模型，在XAML文件中定义页面，在后台的cs文件中编写事件响应代码。采用这种模型是因为很多页面需要动态生成控件，然后动态改变这些控件的属性，事件驱动模型在这种场景下比较好理解。现在我们采用MVVM方式编写完成游戏列表页面。

MVVM是将页面绑定到视图模型，所有的操作和事件响应通过视图模型完成。视图模型中没有页面控件的定义，因此和页面是解耦的，可以独立进行测试。在视图模型中我们只关心数据，而不关心展示数据的控件。

首先，我们定义一个视图模型的基类，下一步在改造其它页面时，会用到这个基类："
    });
}
easy!.AddIndex(list);

```

## 检索

``` csharp
var result = easy.Search<Article>("移动游戏开发", 1, 20, new string[] { "Title", "Content" });
Console.WriteLine("一共:" + result.Total);
foreach (var item in result.list)
{
    Console.WriteLine($"id:{item.Id} title:{item.Title}");
}
Console.WriteLine($"分词:{string.Join(" ",result.cutKeys)}");
Console.WriteLine("完成");
```
