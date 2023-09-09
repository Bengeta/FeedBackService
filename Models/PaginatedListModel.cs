using AutoMapper;

namespace Models;

public class PaginatedListModel<T>
{
    public List<T>? data { get; set; }
    public int currentPage { get; set; }
    public int countPage { get; set; }
    public bool isNext { get; set; }
    public bool isPrev { get; set; }
}

public class PagedListTypeConverter<T> : ITypeConverter<PagedList<T>, PaginatedListModel<T>>
{
    public PaginatedListModel<T> Convert(PagedList<T> source, PaginatedListModel<T> destination, ResolutionContext context)
    {
        return new PaginatedListModel<T>()
        {
            data = source,
            currentPage = source.CurrentPage,
            isNext = source.HasNext,
            isPrev = source.HasPrevious,
            countPage = source.TotalPages
        };
    }
}
