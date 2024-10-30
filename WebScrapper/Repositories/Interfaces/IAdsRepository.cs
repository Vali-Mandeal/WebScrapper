using WebScrapper.Entities;

namespace WebScrapper.Repositories.Interfaces;
public interface IAdsRepository
{
    /// <summary>
    /// <para>Queries all ads from the database matching the specified <paramref name="filter"/></para>
    /// <para>If no <paramref name="filter"/> is specified then everything is matched and returned</para>
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    Task<List<Ad>> GetByFilterAsync(string propertyName, string? filter = null);

    Task CreateAsync(List<Ad> ads);
}
