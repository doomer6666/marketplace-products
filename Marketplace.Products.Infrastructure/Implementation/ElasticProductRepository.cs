using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Marketplace.Products.Application;
using Marketplace.Products.Application.DTOs;
using Marketplace.Products.Domain;

namespace Marketplace.Products.Infrastructure.Implementation;

public class ElasticProductRepository : IProductSearchReader, IProductSearchWriter
{
    private const string IndexName = "products";
    private readonly ElasticsearchClient _elasticClient;

    public ElasticProductRepository(string connectionString)
    {
        var settings = new ElasticsearchClientSettings(new Uri(connectionString));
        _elasticClient = new ElasticsearchClient(settings);
    }

    public async Task<List<Product>> SearchAsync(ProductFilterDto filter)
    {
        var response = await _elasticClient.SearchAsync<Product>(s => s
                                                                      .Indices(IndexName)
                                                                      .From((filter.PageNumber - 1) * filter.PageSize)
                                                                      .Size(filter.PageSize)
                                                                      .Sort(sort =>
                                                                      {
                                                                          switch (filter.SortBy)
                                                                          {
                                                                              case ProductSortBy.PriceAsc:
                                                                                  sort.Field(f => f.Price, new FieldSort { Order = SortOrder.Asc });
                                                                                  break;

                                                                              case ProductSortBy.PriceDesc:
                                                                                  sort.Field(f => f.Price, new FieldSort { Order = SortOrder.Desc });
                                                                                  break;

                                                                              case ProductSortBy.Newest:
                                                                                  sort.Field(f => f.CreatedAt, new FieldSort { Order = SortOrder.Desc });
                                                                                  break;

                                                                              case ProductSortBy.Relevance:
                                                                              default:
                                                                                  sort.Score(s => s.Order(SortOrder.Desc));
                                                                                  break;
                                                                          }
                                                                      })
                                                                      .Query(q => q.Bool(b =>
                                                                      {
                                                                          var mustClauses =
                                                                              new List<Action<
                                                                                  QueryDescriptor<Product>>>();
                                                                          var filterClauses =
                                                                              new List<Action<
                                                                                  QueryDescriptor<Product>>>();

                                                                          if (!string.IsNullOrWhiteSpace(
                                                                                  filter.SearchTerm))
                                                                          {
                                                                              mustClauses.Add(mq => mq
                                                                                  .Match(m => m
                                                                                      .Field(f => f.Name)
                                                                                      .Query(filter.SearchTerm)
                                                                                      .Fuzziness(new Fuzziness("AUTO"))
                                                                                  ));
                                                                          }

                                                                          if (filter.Category.HasValue)
                                                                          {
                                                                              filterClauses.Add(fq =>
                                                                                  fq.Term(t => t
                                                                                    .Field(new Field("category.keyword"))
                                                                                    .Value(filter.Category.Value.ToString()))
                                                                                  );
                                                                          }

                                                                          if (filter.MinPrice.HasValue
                                                                              || filter.MaxPrice.HasValue)
                                                                          {
                                                                              filterClauses.Add(fq => fq.Range(r => r
                                                                                  .NumberRange(nr => nr
                                                                                      .Field(f => f.Price)
                                                                                      .Gte(filter.MinPrice.HasValue
                                                                                              ? (double)filter.MinPrice
                                                                                                  .Value
                                                                                              : null)
                                                                                      .Lte(filter.MaxPrice.HasValue
                                                                                              ? (double)filter.MaxPrice
                                                                                                  .Value
                                                                                              : null)
                                                                                  )));
                                                                          }

                                                                          if (filter.CreatedAt.HasValue)
                                                                          {
                                                                              var dateStr =
                                                                                  filter.CreatedAt.Value.ToString(
                                                                                      "yyyy-MM-dd");
                                                                              filterClauses.Add(fq => fq.Range(r => r
                                                                                  .DateRange(dr => dr
                                                                                      .Field(f => f.CreatedAt)
                                                                                      .Gte(dateStr)
                                                                                      .Lte(dateStr)
                                                                                  )));
                                                                          }

                                                                          if (mustClauses.Count != 0)
                                                                          {
                                                                              b.Must(mustClauses.ToArray());
                                                                          }

                                                                          if (filterClauses.Count != 0)
                                                                          {
                                                                              b.Filter(filterClauses.ToArray());
                                                                          }
                                                                      }))
                       );
        if (!response.IsValidResponse)
        {
            if (response.ElasticsearchServerError?.Error?.Type == "index_not_found_exception")
            {
                return [];
            }

            throw new Exception($"Elasticsearch error: {response.DebugInformation}");
        }

        return [.. response.Documents];
    }

    public async Task IndexProductAsync(Product product)
    {
        await _elasticClient.IndexAsync(product,
            idx =>
                idx.Index(IndexName).Id(product.Id.ToString()));
    }

    public async Task DeleteProductAsync(Guid id) =>
        await _elasticClient.DeleteAsync<Product>(id.ToString(),
            d => d.Index(IndexName));
}