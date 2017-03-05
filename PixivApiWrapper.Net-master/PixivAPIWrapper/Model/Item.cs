using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixivAPIWrapper.Model
{
    /// <summary>
    /// A base abstract used for Illustrations, Images and Novels
    /// </summary>
    public abstract class Item
    {
        public int Id { get; protected set; }
        public int AuthorId { get; protected set; }
        public string Title { get; protected set; }
        public string AuthorName { get; protected set; }
        public string Date { get; protected set; }
        public Tag[] Tags { get; protected set; }
        public int Feedback { get; protected set; }
        public int Point { get; protected set; }
        public int Views { get; protected set; }
        public string Caption { get; protected set; }

        public Item(int id, int authorId, string title, string authorName, string date, Tag[] tags, int point, int feedback, int views, string caption)
        {
            Id = id;
            AuthorId = authorId;
            Title = title;
            AuthorName = authorName;
            Date = date;
            Tags = tags;
            Point = point;
            Feedback = feedback;
            Views = views;
            Caption = caption;
        }

        internal Item(PixivObjectFactory.ItemTransferObject obj)
        {
            Id = obj.Id;
            AuthorId = obj.AuthorId;
            Title = obj.Title;
            AuthorName = obj.AuthorName;
            Date = obj.Date;
            Tags = obj.Tags;
            Feedback = obj.Feedback;
            Point = obj.Point;
            Views = obj.Views;
            Caption = obj.Caption;
        }
    }
}
