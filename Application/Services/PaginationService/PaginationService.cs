using EduShpere.Application.DTOs.CommonDto;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace EduShpere.Application.Services
{
    public class PaginationService : IPaginationService
    {
        public async Task<PaginationResponseDto<T>> GetPagedResultAsync<T>(
            IQueryable<T> query,
            PaginationRequestDto paginationRequest) where T : class
        {
            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(paginationRequest.Search))
            {
                query = ApplySearchFilter(query, paginationRequest.Search);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting if provided
            if (!string.IsNullOrWhiteSpace(paginationRequest.SortBy))
            {
                query = ApplySorting(query, paginationRequest.SortBy, paginationRequest.SortDescending);
            }

            // Apply pagination
            var pagedData = await query
                .Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
                .Take(paginationRequest.PageSize)
                .ToListAsync();

            return new PaginationResponseDto<T>
            {
                Data = pagedData,
                TotalCount = totalCount,
                PageNumber = paginationRequest.PageNumber,
                PageSize = paginationRequest.PageSize
            };
        }

        private IQueryable<T> ApplySearchFilter<T>(IQueryable<T> query, string searchTerm) where T : class
        {
            var properties = typeof(T).GetProperties()
                .Where(p => p.PropertyType == typeof(string) || 
                           (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) && p.PropertyType.GetGenericArguments()[0] == typeof(string)))
                .ToList();

            if (!properties.Any())
                return query;

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? searchExpression = null;

            foreach (var property in properties)
            {
                var propertyAccess = Expression.Property(parameter, property);
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                var searchValue = Expression.Constant(searchTerm.ToLower());

                if (containsMethod != null && toLowerMethod != null)
                {
                    var propertyToLower = Expression.Call(propertyAccess, toLowerMethod);
                    var containsCall = Expression.Call(propertyToLower, containsMethod, searchValue);

                    searchExpression = searchExpression == null 
                        ? containsCall 
                        : Expression.OrElse(searchExpression, containsCall);
                }
            }

            if (searchExpression != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(searchExpression, parameter);
                query = query.Where(lambda);
            }

            return query;
        }

        private IQueryable<T> ApplySorting<T>(IQueryable<T> query, string sortBy, bool sortDescending) where T : class
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return query;

            var property = typeof(T).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            
            if (property == null)
                return query;

            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyAccess = Expression.Property(parameter, property);
            var lambda = Expression.Lambda(propertyAccess, parameter);

            var methodName = sortDescending ? "OrderByDescending" : "OrderBy";
            var resultExpression = Expression.Call(
                typeof(Queryable),
                methodName,
                new Type[] { typeof(T), property.PropertyType },
                query.Expression,
                Expression.Quote(lambda));

            return query.Provider.CreateQuery<T>(resultExpression);
        }
    }
}
