using BuildingBlocks.Domain.Models;
using BuildingBlocks.Domain.Specifications;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.EfCore
{
    public static class SpecificationEvaluator
    {
        public static IQueryable<T> GetQuery<T>(IQueryable<T> inputQuery, Specification<T>? specification, bool applyPaging = true)
            where T : Entity
        {
            if (specification is null)
            {
                return inputQuery;
            }

            IQueryable<T> query = inputQuery;

            // 1) AsNoTracking
            if (specification.IsAsNoTracking)
            {
                query = query.AsNoTracking();
            }

            // 2) Filter
            var criteria = specification.ToExpression();
            query = query.Where(criteria);

            // 3) Includes
            if (specification.IncludeExpressions is { Count: > 0 })
            {
                query = specification.IncludeExpressions.Aggregate(query, (current, include) => current.Include(include));
            }
            if (specification.IncludeStrings is { Count: > 0 })
            {
                query = specification.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));
            }

            // 4) Ordering
            if (specification.OrderBy != null)
            {
                query = specification.IsDescending
                    ? query.OrderByDescending(specification.OrderBy)
                    : query.OrderBy(specification.OrderBy);
            }

            // 5) Paging
            if (applyPaging && specification.PageSize.HasValue && specification.PageNumber.HasValue)
            {
                var page = Math.Max(1, specification.PageNumber.Value);
                var size = Math.Max(1, specification.PageSize.Value);
                query = query.Skip((page - 1) * size).Take(size);
            }

            return query;
        }
    }
}

