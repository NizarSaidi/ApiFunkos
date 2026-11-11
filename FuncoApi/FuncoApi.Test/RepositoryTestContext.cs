using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FuncoApi.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using Moq;

namespace FuncoApi.Test;

internal static class RepositoryTestContext
{
    public static Mock<FuncoDbContext> CreateContext()
    {
        var context = new Mock<FuncoDbContext>(new DbContextOptions<FuncoDbContext>());
        context.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        return context;
    }

    public static Mock<DbSet<T>> CreateDbSet<T>(List<T> data, Action<T>? onAdd = null)
        where T : class
    {
        var queryable = data.AsQueryable();
        var dbSet = new Mock<DbSet<T>>();

        dbSet.As<IAsyncEnumerable<T>>()
            .Setup(set => set.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns((CancellationToken cancellationToken) =>
                new TestAsyncEnumerator<T>(queryable.GetEnumerator()));
        dbSet.As<IQueryable<T>>().Setup(set => set.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        dbSet.As<IQueryable<T>>().Setup(set => set.Expression).Returns(queryable.Expression);
        dbSet.As<IQueryable<T>>().Setup(set => set.ElementType).Returns(queryable.ElementType);
        dbSet.As<IQueryable<T>>().Setup(set => set.GetEnumerator())
            .Returns(() => queryable.GetEnumerator());

        dbSet.Setup(set => set.AddAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .Callback<T, CancellationToken>((entity, _) =>
            {
                data.Add(entity);
                onAdd?.Invoke(entity);
            })
            .Returns((T _, CancellationToken _) =>
                ValueTask.FromResult<EntityEntry<T>>(null!));
        dbSet.Setup(set => set.Remove(It.IsAny<T>()))
            .Callback<T>(entity => data.Remove(entity))
            .Returns((T _) => null!);

        return dbSet;
    }

    private sealed class TestAsyncQueryProvider<TEntity>(IQueryProvider innerProvider) : IAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = expression.Type.GetGenericArguments().First();
            var query = (IQueryable)Activator.CreateInstance(
                typeof(TestAsyncEnumerable<>).MakeGenericType(elementType),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                args: [expression],
                culture: null)!;
            return query;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object? Execute(Expression expression)
        {
            return innerProvider.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return innerProvider.Execute<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var resultType = typeof(TResult).GetGenericArguments()[0];
            var executeMethod = typeof(IQueryProvider).GetMethods()
                .Single(method => method.Name == nameof(IQueryProvider.Execute) && method.IsGenericMethod)
                .MakeGenericMethod(resultType);
            var result = executeMethod.Invoke(innerProvider, [expression]);
            var task = typeof(Task).GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(resultType)
                .Invoke(null, [result]);
            return (TResult)task!;
        }
    }

    private sealed class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable)
        {
        }

        public TestAsyncEnumerable(Expression expression) : base(expression)
        {
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }
    }

    private sealed class TestAsyncEnumerator<T>(IEnumerator<T> innerEnumerator) : IAsyncEnumerator<T>
    {
        public T Current => innerEnumerator.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return ValueTask.FromResult(innerEnumerator.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            innerEnumerator.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
