using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Lucene.Net.Analysis.PanGu;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace KnifeZ.ClassLib.LuceneNP
{
    public class SearchEngine
    {
        /// <summary>
        /// 索引存放目录
        /// </summary>
        protected string INDEX_DIR => PanGu.Framework.Path.GetAssemblyPath() + @"IndexDic";
        protected string PANGU_PATH => PanGu.Framework.Path.GetAssemblyPath() + @"PanGu.xml";
        #region 创建单例
        // 定义一个静态变量来保存类的实例
        private static SearchEngine uniqueInstance;

        // 定义一个标识确保线程同步
        private static readonly object locker = new object();


        // 定义私有构造函数，使外界不能创建该类实例
        private SearchEngine()
        { }

        /// <summary>
        /// 定义公有方法提供一个全局访问点,同时你也可以定义公有属性来提供全局访问点
        /// </summary>
        /// <returns></returns>
        public static SearchEngine GetInstance()
        {
            // 当第一个线程运行到这里时，此时会对locker对象 "加锁"，
            // 当第二个线程运行该方法时，首先检测到locker对象为"加锁"状态，该线程就会挂起等待第一个线程解锁
            // lock语句运行完之后（即线程运行完之后）会对该对象"解锁"
            lock (locker)
            {
                // 如果类的实例不存在则创建，否则直接返回
                if (uniqueInstance == null)
                {
                    uniqueInstance = new SearchEngine();
                }
            }

            return uniqueInstance;
        }
        #endregion

        //任务队列,保存生产出来的任务和消费者使用,不使用list避免移除时数据混乱问题
        private Queue<IndexJob> jobs = new Queue<IndexJob>();      

        /// <summary>
        /// 任务类,包括任务的Id ,操作的类型
        /// </summary>
        class IndexJob
        {
            public string Id { get; set; }
            public JobType JobType { get; set; }

            public LiteNewsModel Model { get; set; }
        }
        /// <summary>
        /// 枚举,操作类型是增加还是删除
        /// </summary>
        enum JobType { Add, Remove }

        public void AddArticle(LiteNewsModel model)
        {
            IndexJob job = new IndexJob
            {
                Id = model.BillCode,
                JobType = JobType.Add,
                Model=model
            };
            jobs.Enqueue(job);//把任务加入列表
        }

        public void RemoveArticle(string billCode)
        {
            IndexJob job = new IndexJob
            {
                JobType = JobType.Remove,
                Id = billCode
            };
            jobs.Enqueue(job);//把任务加入列表
        }

        /// <summary>
        /// 启动消费者线程
        /// </summary>
        public void CustomerStart()
        {

            PanGu.Segment.Init(PANGU_PATH);

            Thread threadIndex = new Thread(IndexOn);
            threadIndex.IsBackground = true;
            threadIndex.Start();
        }
        /// <summary>
        /// 索引任务线程
        /// </summary>
        private void IndexOn()
        {
            //Console.WriteLine("索引任务线程启动");
            while (true)
            {
                if (jobs.Count <= 0)
                {
                    Thread.Sleep(5 * 1000);
                    continue;
                }
                //创建索引目录
                if (!System.IO.Directory.Exists(INDEX_DIR))
                {
                    System.IO.Directory.CreateDirectory(INDEX_DIR);
                }
                Lucene.Net.Store.Directory directory = FSDirectory.Open(new DirectoryInfo(INDEX_DIR), new NativeFSLockFactory());
                bool isUpdate = IndexReader.IndexExists(directory);
                //Console.WriteLine("索引库存在状态" + isUpdate);
                if (isUpdate)
                {
                    //如果索引目录被锁定（比如索引过程中程序异常退出），则首先解锁
                    if (IndexWriter.IsLocked(directory))
                    {
                        //Console.WriteLine("开始解锁索引库");
                        IndexWriter.Unlock(directory);
                        //Console.WriteLine("解锁索引库完成");
                    }
                }
                    IndexWriter writer = new IndexWriter(directory, new PanGuAnalyzer(), !isUpdate, IndexWriter.MaxFieldLength.UNLIMITED);
                    try
                    {
                        ProcessJobs(writer);
                        writer.Dispose();
                        directory.Dispose();
                    }

                    catch (ThreadAbortException ex)
                    {
                        writer.Dispose();
                        directory.Dispose();
                    }
                //Console.WriteLine("全部索引完毕");
            }
        }
        private void ProcessJobs(IndexWriter writer)
        {
            while (jobs.Count != 0)
            {
                IndexJob job = jobs.Dequeue();
                writer.DeleteDocuments(new Term("billCode", job.Id));
                if (job.JobType == JobType.Add)
                {
                    var model = job.Model;
                    if (model == null)//有可能刚添加就被删除了
                    {
                        continue;
                    }
                    //添加索引

                    Document doc = new Document();
                    //只有对需要全文检索的字段才ANALYZED
                    doc.Add(new Field("billCode", model.BillCode, Field.Store.YES, Field.Index.NOT_ANALYZED));
                    doc.Add(new Field("title", model.Title, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
                    doc.Add(new Field("abstract", model.Abstract, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
                    doc.Add(new Field("content", model.Content, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
                    doc.Add(new Field("url", model.Url, Field.Store.YES, Field.Index.NOT_ANALYZED));
                    doc.Add(new Field("time", model.Time.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
                    writer.AddDocument(doc);
                    //LNPIndex.AddIndex(writer, model);
                }
            }
        }

        /// <summary>
        /// 生成索引
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string CreatedIndex(List<LiteNewsModel> list)
        {
            LNPIndex index = new LNPIndex();
            var time=index.CreatedIndex(list);
            return "共计消耗 " + time + " 秒";
        }
        public static List<LiteNewsModel> QueryList(string qkey,int top)
        {
            LNPIndex index = new LNPIndex();
            var ret = index.SearchIndex(qkey,top);
            return ret;
        }
    }
}
