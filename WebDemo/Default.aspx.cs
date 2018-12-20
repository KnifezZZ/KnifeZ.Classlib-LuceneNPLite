using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using KnifeZ.ClassLib.LuceneNP;

namespace WebDemo
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }
        public List<LiteNewsModel> MakeList()
        {
            var list = new List<LiteNewsModel>();
            for (int i = 0; i < 10000; i++)
            {
                var guid = Guid.NewGuid();
                list.Add(new LiteNewsModel()
                {
                    BillCode = "T2018110222"+i,
                    Abstract = guid+"描述测试测试",
                    Content = guid+"测试测试测试测试信息内容",
                    Time = DateTime.Now,
                    Title = "测试数据第一条2018--"+ guid,
                    Url = "/news/"+i+ guid + ".html"
                });
            }
            return list;
        }
        public LiteNewsModel  GetModel(string billCode)
        {
            var model = new LiteNewsModel()
            {
                BillCode = "T20181102221",
                Abstract = "描述测试测试",
                Content = "测试测试测试测试信息111内容",
                Time = DateTime.Now,
                Title = "测试数据第一条20122228",
                Url = "https://www.test.com/test.html"
            };
            return model;
        }

        protected void btn_AddIndex_Click(object sender, EventArgs e)
        {
            SearchEngine.GetInstance().AddArticle(GetModel("T20181102221"));

        }

        protected void btn_CreateAllIndex_Click(object sender, EventArgs e)
        {
            var list = MakeList();
            var ret=SearchEngine.CreatedIndex(list);
            Label2.Text = ret;
        }

        protected void btn_Search_Click(object sender, EventArgs e)
        {
            var ret = SearchEngine.QueryList(qKey.Text, 100);
            var html = "";
            foreach (var item in ret)
            {
                html += "<div>";
                html += "<h3>" + item.Title + "</h3>";
                html += "<div class='content'>" + item.Abstract + "</div>";
                html += "<div class='f3'>" + item.Url + "</div>";
                html += "</div>";
            }
            retList.InnerHtml = html;
        }
    }
}