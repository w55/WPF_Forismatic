using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MyWpfForismatic
{
    #region  class Forismatic

    public class Forismatic
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Author { get; set; }
        public string Link { get; set; }

        public Forismatic() { }

        public Forismatic(string text, string author = "", string link = "")
        {
            Text = text.Trim();
            Author = author.Trim();
            Link = link.Trim();
        }
        public Forismatic(string text, string link = "")
        {
            Text = text.Trim();
            Link = link.Trim();
        }

        #region  static Forismatic GetNextThought()
        /// <summary>
        /// Return next smart thought from Forismatic.com
        /// </summary>
        /// <returns></returns>
        public static Forismatic GetNextThought()
        {
            Forismatic thought = new Forismatic();
            try
            {
                XmlDocument RSSXml = new XmlDocument();
                RSSXml.Load("http://api.forismatic.com/api/1.0/?method=getQuote&format=xml&lang=ru");
                XmlNodeList RSSNodeList = RSSXml.SelectNodes("forismatic/quote");

                foreach (XmlNode node in RSSNodeList[0])
                {
                    ////thought.Text =
                    //// <quoteText>Хорошо сказанное слово человека, который ему не следует, столь же бесплодно, как и прекрасный цветок с приятной окраской, но лишенный аромата. </quoteText>

                    if (node.Name.Equals("quoteText", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // <quoteText>Хорошо сказанное слово человека, который ему не следует, ... </quoteText>
                        //
                        string text = node.InnerText;
                        if (!string.IsNullOrEmpty(text))
                        {
                            thought.Text = text;
                            Trace.WriteLine("thought.Text = " + text);
                        }
                    }
                    else if (node.Name.Equals("quoteAuthor", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // <quoteAuthor>Будда Гаутама</quoteAuthor>
                        //
                        string author = node.InnerText;
                        if (!string.IsNullOrEmpty(author))
                        {
                            thought.Author = author;
                            Trace.WriteLine("thought.Author = " + author);
                        }
                    }
                    else if (node.Name.Equals("quoteLink", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // <quoteLink>http://forismatic.com/ru/8aa0fa8d56/</quoteLink>
                        //
                        string link = node.InnerText;
                        if (!string.IsNullOrEmpty(link))
                        {
                            thought.Link = link;
                            Trace.WriteLine("thought.Link = " + link);
                        }
                    }
                }       //         foreach (XmlNode RSSNode in RSSNodeList)
            }
            catch (Exception x)
            {
                string msg = x.Message;
                Trace.Write(msg);

                thought = null;
            }
            return thought;
        }
        #endregion  static Forismatic GetNextThought()
    }
    #endregion  class Forismatic

    #region  class Thought
    public class Thought
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int AuthorId { get; set; }
        public string Link { get; set; }

        public Thought() { }

        public Thought(int id, string text, int author_id = 1, string link = "")
        {
            Id = id;
            Text = text.Trim();
            AuthorId = author_id;
            Link = link.Trim();
        }

        public Thought(string text, int author_id = 1, string link = "")
        {
            Text = text.Trim();
            AuthorId = author_id;
            Link = link.Trim();
        }
        public Thought(string text, string link = "")
        {
            Text = text.Trim();
            Link = link.Trim();
        }
    }
    #endregion  class Thought

    #region  class Authors
    public class Authors
    {
        public int Id { get; set; }
        public string AuthorName { get; set; }

        /// <summary>
        /// Число хранимых в БД мыслей данного автора
        /// </summary>
        public int ThoughtsCount { get; set; }

        public Authors()
        {
            ThoughtsCount = 0;
        }

        public Authors(string author_name)
        {
            ThoughtsCount = 0;

            if (author_name.Trim().Length == 0)
                AuthorName = "- no author -";
            else
                AuthorName = author_name.Trim();
        }

        public Authors(int id, string author_name)
        {
            Id = id;
            ThoughtsCount = 0;
            if (author_name.Trim().Length == 0)
                AuthorName = "- no author -";
            else
                AuthorName = author_name.Trim();
        }

        public Authors(int id, int thoughts_count, string author_name)
        {
            Id = id;
            ThoughtsCount = thoughts_count;
            if (author_name.Trim().Length == 0)
                AuthorName = "- no author -";
            else
                AuthorName = author_name.Trim();
        }
    }
    #endregion  class Thought
}
