using System.Linq.Expressions;
using BuildingBlocks.Domain.Models;

namespace BuildingBlocks.Domain.Specifications
{
    public abstract class Specification<T>
        where T : Entity
    {
        public List<string> IncludeStrings { get; private set; } = [];
        public List<Expression<Func<T, object>>> IncludeExpressions { get; private set; } = [];
        public Expression<Func<T, object>>? OrderBy { get; private set; }
        public bool IsDescending { get; private set; }
        public int? PageNumber { get; private set; }
        public int? PageSize { get; private set; }
        public bool IsAsNoTracking { get; private set; } = false;

        public abstract Expression<Func<T, bool>> ToExpression();

        public Specification<T> And(Specification<T> other)
        {
            return new AndSpecification<T>(this, other);
        }

        public Specification<T> Or(Specification<T> other)
        {
            return new OrSpecification<T>(this, other);
        }
        public void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
            IsDescending = false;
        }
        public void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
        {
            OrderBy = orderByDescendingExpression;
            IsDescending = true;
        }
        public void AddIncludeStrings(params string[] includeStrings)
        {
            IncludeStrings.AddRange(includeStrings);
        }
        public void AddIncludeExpressions(params Expression<Func<T, object>>[] includeExpressions)
        {
            IncludeExpressions.AddRange(includeExpressions);
        }
        public void ApplyPaging(int pageNumber, int pageSize)
        {
            PageNumber = Math.Max(1, pageNumber);
            PageSize = Math.Max(1, Math.Min(100, pageSize));
        }
        public void AsNoTracking()
        {
            IsAsNoTracking = true;
        }
    }
}
