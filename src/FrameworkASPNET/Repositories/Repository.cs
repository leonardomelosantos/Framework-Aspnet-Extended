using FrameworkAspNetExtended.Context;
using FrameworkAspNetExtended.Entities.Exceptions;
using FrameworkAspNetExtended.Entities.Pagination;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

namespace FrameworkAspNetExtended.Repositories
{
    public class Repository<TEntity, TDbContext> : IRepository<TEntity> where TEntity : class where TDbContext : DbContext
    {
        public Func<TEntity, object> GetVersionFieldSelectorForConcurrencyControl()
        {
            return null;
        }

        public DbContext Context
        {
            get
            {
                return ApplicationContext.Resolve<DatabaseContext>().GetDbContextInstance<TDbContext>();
            }
        }

        protected DbSet<TEntity> EntitySet
        {
            get
            {
                return Context.Set<TEntity>();
            }
        }

        public virtual int Count()
        {
            return this.EntitySet.Count();
        }

        public virtual TEntity FindByKey(object key, bool throwConcurrencyExceptionIfNotFound = false)
        {
            TEntity entityFound = this.EntitySet.Find(key);
            if (entityFound == null && throwConcurrencyExceptionIfNotFound)
            {
                throw new DbUpdateConcurrencyException("", new DeletedEntityConcurrencyException());
            }
            return entityFound;
        }

        public virtual TEntity FindByKeyConcurrencyValidate(object key, object version, bool checkDeletedEntity = true)
        {
            TEntity entityFinded = this.EntitySet.Find(key);

            ValidateConcurrency(entityFinded, version, checkDeletedEntity);

            return entityFinded;
        }

        public virtual void ValidateConcurrency(TEntity entity, object version, bool checkDeletedEntity = true)
        {
            if (version == null) throw new ArgumentNullException("version");

            if (checkDeletedEntity && entity == null)
            {
                throw new DbUpdateConcurrencyException();
            }

            var funcSelector = GetVersionFieldSelectorForConcurrencyControl();
            if (funcSelector != null)
            {
                bool isDifferentVersion = false;

                object entityVersionObject = funcSelector(entity);

                if (entityVersionObject is byte[] && version is byte[])
                {
                    byte[] entityVersion = entityVersionObject as byte[];
                    byte[] paramVersion = version as byte[];

                    isDifferentVersion = (!entityVersion.SequenceEqual(paramVersion));
                }
                else
                {
                    isDifferentVersion = (!entityVersionObject.Equals(version));
                }
                if (isDifferentVersion)
                {
                    throw new DbUpdateConcurrencyException();
                }
            }
        }

        public virtual IList<TEntity> All()
        {
            return this.EntitySet.ToList();
        }

        public virtual IList<TEntity> All(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy)
        {
            var query = this.EntitySet;

            return orderBy != null ? orderBy(query).ToList() : query.ToList();
        }

        public virtual void Add(TEntity entity, bool saveChanges = false)
        {
            this.EntitySet.Add(entity);
            if (saveChanges)
            {
                this.Context.SaveChanges();
            }
        }

        public virtual void Add(IEnumerable<TEntity> entities, bool saveChanges = false)
        {
            this.Context.Configuration.ValidateOnSaveEnabled = false;
            this.Context.Configuration.AutoDetectChangesEnabled = false;

            foreach (TEntity entity in entities)
            {
                Add(entity, false);
            }

            if (saveChanges)
            {
                this.Context.SaveChanges();
            }
        }

        public virtual void Add(params TEntity[] entities)
        {
            this.Add(entities.ToList());
        }

        public int SaveChanges()
        {
            return this.Context.SaveChanges();
        }

        public virtual void Delete(TEntity entity)
        {
            EntitySet.Remove(entity);
        }

        public virtual void Delete(IEnumerable<TEntity> entities)
        {
            TEntity[] entitiesArray = entities.ToArray();
            for (int i = 0; i < entitiesArray.Length; i++)
            {
                Delete(entitiesArray[i]);
            }
        }

        public virtual void DeleteByKey(object key)
        {
            var entity = FindByKey(key);
            EntitySet.Remove(entity);
        }

        protected IOrderedQueryable<TEntity> GenerateOrderBySelector<TKey>(IQueryable<TEntity> query, string sortOrder, Expression<Func<TEntity, TKey>> keySelector)
        {
            return sortOrder != null && sortOrder.ToUpper() == "DESC" ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        #region LazyLoad

        public void LoadLazyPropertyFiltered<TElementNavProperty>(TEntity entity, Expression<Func<TEntity, System.Collections.Generic.ICollection<TElementNavProperty>>> navigationProperty, Expression<Func<TElementNavProperty, bool>> filter) where TElementNavProperty : class
        {
            this.Context.Entry(entity)
                .Collection(navigationProperty)
                .Query()
                .Where(filter)
                .Load();
        }

        /// <summary>
        /// Carrega (realiza query) de uma propriedade "LazyLoad" de uma entidade, aplicando um filtro de carregamento.
        /// </summary>
        /// <typeparam name="TElementNavProperty">Tipo do elemento da coleção de navegação.</typeparam>
        /// <param name="entity">Entidade na qual queremos carregar a coleção em lazy.</param>
        /// <param name="navigationProperty">Propriedade de navegação. (String)</param>
        /// <param name="filter">Filtro a ser aplicado no carregamento da coleção de navegação.</param>
        public void LoadLazyPropertyFiltered<TElementNavProperty>(TEntity entity, String navigationProperty, Expression<Func<TElementNavProperty, bool>> filter) where TElementNavProperty : class
        {
            this.Context.Entry(entity)
            .Collection<TElementNavProperty>(navigationProperty)
            .Query()
            .Where(filter)
            .Load();
        }

        #endregion

        #region Paging

        /// <summary>
        /// Realiza uma busca paginada, usando uma restrição de busca (where), com quantidade de resultados por página seguindo padrão da configuração.
        /// </summary>
        /// <param name="criteria">Critério de busca. (where)</param>
        /// <param name="pagina">Página a ser consultada (iniciando em 1). Se não for passado nenhum valor (null) assume a pagina 1</param>
        /// <param name="ordenacao">Função usada na ordenação. Baseada nas propriedades da entidade.</param>
        /// <returns>Resultados na página atual</returns>
        protected PagedResult<TEntity> FindPaged(Expression<Func<TEntity, bool>> criteria = null, int? pagina = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> ordenacao = null)
        {
            return FindPaged(criteria, pagina, null, ordenacao);
        }

        /// <summary>
        /// Realiza uma busca paginada, usando uma restrição de busca (where).
        /// </summary>
        /// <param name="criteria">Critério de busca. (where)</param>
        /// <param name="pagina">Página a ser consultada (iniciando em 1). Se não for passado nenhum valor (null) assume a pagina 1</param>
        /// <param name="qtdResultsInPage">Quantidade de resultados por página. Se não for passado nenhum valor (null) assume o valor padrão em configuração.</param>
        /// <param name="ordenacao">Função usada na ordenação. Baseada nas propriedades da entidade.</param>
        /// <returns>Resultados na página atual</returns>
        protected PagedResult<TEntity> FindPaged(Expression<Func<TEntity, bool>> criteria = null, int? pagina = null, int? qtdResultsInPage = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> ordenacao = null)
        {
            int resultsByPage = qtdResultsInPage ?? GetResultsByPage();

            int paginaAtual = pagina ?? 1;
            int pular = pagina.HasValue ? (pagina.Value - 1) * resultsByPage : 0;

            var criteriaConsulta = criteria ?? (e => true);

            IEnumerable<TEntity> lista;
            if (ordenacao != null)
            {
                lista = ordenacao(EntitySet.Where(criteriaConsulta))
                    .Skip(pular)
                    .Take(resultsByPage);
            }
            else
            {
                lista = EntitySet.Where(criteriaConsulta)
                    .Skip(pular)
                    .Take(resultsByPage);
            }

            int quantidadeTotal = EntitySet.Where(criteriaConsulta).Count();


            return new PagedResult<TEntity>(lista.ToList(), quantidadeTotal, paginaAtual, resultsByPage);
        }

        /// <summary>
        /// Realiza uma busca paginada com quantidade de resultados por página seguindo padrão da configuração.
        /// </summary>
        /// <param name="query">Query de busca base.</param>
        /// <param name="pagina">Página a ser consultada (iniciando em 1). Se não for passado nenhum valor (null) assume a pagina 1</param>
        /// <param name="ordenacao">Função usada na ordenação. Baseada nas propriedades da entidade.</param>
        /// <returns>Resultados na página atual</returns>
        protected PagedResult<TEntity> FindPaged(IQueryable<TEntity> query, int? pagina = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> ordenacao = null)
        {
            return FindPaged(query, pagina, null, ordenacao);
        }

        /// <summary>
        /// Realiza uma busca paginada, usando uma restrição de busca (where).
        /// </summary>
        /// <param name="query">Critério de busca. (where)</param>
        /// <param name="pagina">Página a ser consultada (iniciando em 1). Se não for passado nenhum valor (null) assume a pagina 1</param>
        /// <param name="qtdResultsInPage">Quantidade de resultados por página. Se não for passado nenhum valor (null) assume o valor padrão em configuração.</param>
        /// <param name="ordenacao">Função usada na ordenação. Baseada nas propriedades da entidade.</param>
        /// <returns>Resultados na página atual</returns>
        protected PagedResult<TEntity> FindPaged(IQueryable<TEntity> query, int? pagina = null, int? qtdResultsInPage = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> ordenacao = null)
        {
            int resultsByPage;
            int paginaAtual;
            IEnumerable<TEntity> lista;
            int quantidadeTotal;

            GetDetailFindPaged(query, pagina, qtdResultsInPage, ordenacao, out resultsByPage, out paginaAtual, out lista, out quantidadeTotal);

            return new PagedResult<TEntity>(lista.ToList(), quantidadeTotal, paginaAtual, resultsByPage);
        }

        private void GetDetailFindPaged(IQueryable<TEntity> query, int? pagina, int? qtdResultsInPage, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> ordenacao, out int resultsByPage, out int paginaAtual, out IEnumerable<TEntity> lista, out int quantidadeTotal)
        {
            resultsByPage = qtdResultsInPage ?? GetResultsByPage();

            paginaAtual = pagina ?? 1;
            int pular = pagina.HasValue ? (pagina.Value - 1) * resultsByPage : 0;


            if (ordenacao != null)
            {
                lista = ordenacao(query)
                    .Skip(pular)
                    .Take(resultsByPage);
            }
            else
            {
                lista = query
                    .Skip(pular)
                    .Take(resultsByPage);
            }

            quantidadeTotal = query.Count();
        }

        /// <summary>
        /// Consulta as entidades projetadas beasedas na funcão <paramref name="projection"/> de <typeparamref name="TEntity"/> para <typeparamref name="TResult"/>.
        /// </summary>
        /// <typeparam name="TResult">Tipo de resultado da projeção</typeparam>
        /// <param name="query">Query na qual se baseia a busca paginada</param>
        /// <param name="projection">Funcão de projeção, ex: 
        /// <example>
        ///     <code>
        /// return FindPagedWithProjection(query,
        ///                            rn => new RegistroNotificacaoDTO
        ///                                {
        ///                                   DataRegistro = rn.DataRegistroNotificacao,
        ///                                    IdRegistroNotificacao = rn.Id,
        ///                                    Lido = rn.NotificacoesLidasUsuarios.Any(nl => nl.LoginUsuarioLeitura == loginUsuario),
        ///                                 NomeEvento = rn.EventoNotificacao.Nome,
        ///                                 CodigoDocumento = rn.DocumentoNormativo != null ? rn.DocumentoNormativo.CodigoIdentificador : null,
        ///                                 NomeLongoEquipamento = rn.Equipamento != null ? rn.Equipamento.NomeLongo : null,
        ///                                 NomeLongoEstacao = rn.Estacao != null ? rn.Estacao.NomeLongo : null
        ///                             },
        ///                         searchSetting.PageIndex,
        ///                         null,
        ///                         registro => BuildSort(registro, searchSetting));
        ///     </code>
        /// </example>
        /// </param>
        /// <param name="pagina">Página a ser consultada (iniciando em 1). Se não for passado nenhum valor (null) assume a pagina 1</param>
        /// <param name="qtdResultsInPage">Quantidade de resultados por página. Se não for passado nenhum valor (null) assume o valor padrão em configuração.</param>
        /// <param name="ordenacao">Função usada na ordenação. Baseada nas propriedades da entidade.</param>
        /// <returns>Resultados na página atual</returns>
        protected PagedResult<TResult> FindPagedWithProjection<TResult, TEntity>(IQueryable<TEntity> query, Func<TEntity, TResult> projection, int? pagina = null, int? qtdResultsInPage = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> ordenacao = null)
        {
            int resultsByPage;
            int paginaAtual;
            IEnumerable<TEntity> lista;
            int quantidadeTotal;

            GetPaginationInfo(query, pagina, qtdResultsInPage, ordenacao, out resultsByPage, out paginaAtual, out lista, out quantidadeTotal);

            return new PagedResult<TResult>(lista.Select(projection).ToList(), quantidadeTotal, paginaAtual, resultsByPage);
        }

        /// <summary>
        /// Retorna (em parametros de output) informações de paginação dada <paramref name="query"/>.
        /// </summary>
        /// <typeparam name="TDetail"></typeparam>
        /// <param name="query">Query da qual as informações de paginação se baseará</param>
        /// <param name="pagina">Página a ser consultada (iniciando em 1). Se não for passado nenhum valor (null) assume a pagina 1</param>
        /// <param name="qtdResultsInPage">Quantidade de resultados por página. Se não for passado nenhum valor (null) assume o valor padrão em configuração.</param>
        /// <param name="ordenacao">Função usada na ordenação. Baseada nas propriedades da entidade.</param>
        /// <param name="resultsByPage">Quantidade de registros por página</param>
        /// <param name="paginaAtual">Pagina Atual</param>
        /// <param name="lista">Resultado paginado</param>
        /// <param name="quantidadeTotal">Quantidade total de registros</param>
        protected void GetPaginationInfo<TDetail>(IQueryable<TDetail> query, int? pagina, int? qtdResultsInPage, Func<IQueryable<TDetail>, IOrderedQueryable<TDetail>> ordenacao, out int resultsByPage, out int paginaAtual, out IEnumerable<TDetail> lista, out int quantidadeTotal)
        {
            resultsByPage = qtdResultsInPage ?? GetResultsByPage();
            paginaAtual = pagina ?? 1;
            int pular = pagina.HasValue ? (pagina.Value - 1) * resultsByPage : 0;

            lista = (ordenacao != null ? ordenacao(query) : query).Skip(pular).Take(resultsByPage);

            quantidadeTotal = query.Count();
        }

        public virtual int GetResultsByPage()
        {
            return 15;
        }

        public void Delete(Expression<Func<TEntity, bool>> filterExpression)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}