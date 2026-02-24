using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using slender_server.Domain.Interfaces;
using slender_server.Domain.Models;
using slender_server.Infra.Database;

namespace slender_server.Infra.Repositories;

public class Repository<T>(ApplicationDbContext dbContext) : IRepository<T>
    where T : class
{
    protected readonly DbSet<T> DbSet = dbContext.Set<T>();

    public virtual async Task<T?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return await DbSet.FindAsync([id], ct);
    }

    public virtual async Task<List<T>> GetAllAsync(CancellationToken ct = default)
    {
        return await DbSet.ToListAsync(ct);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await DbSet.AddAsync(entity, ct);
        await dbContext.SaveChangesAsync(ct);
        return entity;
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        DbSet.Update(entity);
        await dbContext.SaveChangesAsync(ct);
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        DbSet.Remove(entity);
        await dbContext.SaveChangesAsync(ct);
    }

    public virtual async Task<PagedResult<T>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        CancellationToken ct = default)
    {
        IQueryable<T> query = DbSet;

        // Apply filter
        if (filter is not null)
        {
            query = query.Where(filter);
        }

        // Get total count before pagination
        int totalCount = await query.CountAsync(ct);

        // Apply ordering
        if (orderBy is not null)
        {
            query = orderBy(query);
        }

        // Apply pagination
        List<T> items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
    }
}