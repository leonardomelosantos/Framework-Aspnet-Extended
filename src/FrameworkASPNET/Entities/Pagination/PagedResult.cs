using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FrameworkAspNetExtended.Entities.Pagination
{
    [DataContract]
    public class PagedResult<TEntity>
    {
        public PagedResult()
        {
        }

        public PagedResult(IList<TEntity> list, int quantity, int currentPage, int pageSize)
        {
            this.List = list;
            this.TotalCount = quantity;
            this.CurrentPage = currentPage;
            this.PageSize = pageSize;
        }

        [DataMember]
        public int PageSize { get; set; }

        [DataMember]
        public IList<TEntity> List { get; set; }

        [DataMember]
        public int TotalCount { get; set; }

        [DataMember]
        public int CurrentPage { get; set; }
    }
}
