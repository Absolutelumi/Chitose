using PixivLib.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixivLib
{
    public interface IPaginator<T>
    {
        Task SetPage(int page);

        T[] Items();
        Pagination GetPagination();
    }

    public class Pager<T>
    {
        private IPaginator<T> paginator;

        public Pager(IPaginator<T> p)
        {
            paginator = p;
        }

        public int Count()
        {
            return paginator.GetPagination().Total;
        }

        public int PageCount()
        {
            return paginator.GetPagination().Pages;
        }

        async public Task<T[]> Page(int i)
        {
            if (paginator.GetPagination().Current != i)
                await paginator.SetPage(i).ConfigureAwait(false);
            return paginator.Items();
        }

        public IEnumerator GetEnumerator()
        {
            for (int i = 1; i <= paginator.GetPagination().Pages; ++i)
            {
                var items = Page(i);
                for (int j = 0; j < items.Result.Length; ++j)
                    yield return items.Result[j];
            }
        }
    }
}
