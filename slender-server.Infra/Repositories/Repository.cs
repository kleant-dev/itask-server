using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using slender_server.Domain.Interfaces;
using slender_server.Domain.Models;
using slender_server.Infra.Database;

namespace slender_server.Infra.Repositories;

/// <summary>
/// Generic repository base. Intentionally does NOT call SaveChangesAsync internally —
/// the Unit of Work (ApplicationDbContext / IUnitOfWork) is responsible for committing.
/// This ensures multiple repository operations compose into a single atomic transaction.
/// </summary>
public class Repository<T>(ApplicationDbContext dbContext) : IRepository<T>
    where T : class
{
    protected readonly ApplicationDbContext _dbContext = dbContext;
    protected readonly DbSet<T> DbSet = dbContext.Set<T>();

    public virtual async Task<T?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return await DbSet.FindAsync([id], ct);
    }

    public virtual async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null,CancellationToken ct = default)
    {
        IQueryable<T> query = DbSet;
        if (predicate is not null)
        {
            query = query.Where(predicate);
        }
        return await query.ToListAsync(ct);
    }

    /// <summary>
    /// Stages the entity for insertion. Does NOT commit — call IUnitOfWork.SaveChangesAsync.
    /// </summary>
    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await DbSet.AddAsync(entity, ct);
        return entity;
    }

    /// <summary>
    /// Marks the entity as modified. Does NOT commit — call IUnitOfWork.SaveChangesAsync.
    /// </summary>
    public virtual Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        DbSet.Update(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Marks the entity for deletion. Does NOT commit — call IUnitOfWork.SaveChangesAsync.
    /// </summary>
    public virtual Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public virtual async Task<PagedResult<T>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        CancellationToken ct = default)
    {
        IQueryable<T> query = DbSet;

        if (filter is not null)
            query = query.Where(filter);

        int totalCount = await query.CountAsync(ct);

        if (orderBy is not null)
            query = orderBy(query);

        List<T> items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
    }
}