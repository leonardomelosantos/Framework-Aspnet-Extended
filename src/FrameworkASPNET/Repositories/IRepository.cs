using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkAspNetExtended.Repositories
{
    public interface IRepository<TEntity> : IRepositoryGeneric where TEntity : class
    {
        int Count();

        TEntity FindByKey(object key, bool throwConcurrencyExceptionIfNotFound = false);

        TEntity FindByKeyConcurrencyValidate(object key, object version, bool checkDeletedEntity = true);

        void ValidateConcurrency(TEntity entity, object version, bool checkDeletedEntity = true);

        IList<TEntity> All();

        IList<TEntity> All(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy);

        void Add(TEntity entity, bool saveChanges = false);

        void Add(params TEntity[] entities);

        void Add(IEnumerable<TEntity> entities, bool saveChanges = false);

        void Delete(TEntity entity);

        void Delete(IEnumerable<TEntity> entities);

        void LoadLazyPropertyFiltered<TElementNavProperty>(
            TEntity entity,
            Expression<Func<TEntity, ICollection<TElementNavProperty>>> navigationProperty,
            Expression<Func<TElementNavProperty, bool>> filter) where TElementNavProperty : class;

        void LoadLazyPropertyFiltered<TElementNavProperty>(
            TEntity entity, String navigationProperty, Expression<Func<TElementNavProperty, bool>> filter)
            where TElementNavProperty : class;

        void DeleteByKey(object key);

        int SaveChanges();

        // TODO Delete vom filtro
        void Delete(Expression<Func<TEntity, bool>> filterExpression);

        //bool HasConcurrencyControl();

        Func<TEntity, object> GetVersionFieldSelectorForConcurrencyControl();

        int GetResultsByPage();
    }
}
