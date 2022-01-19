using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace FrameworkAspNetExtended.Context
{
    public class DatabaseContext
    {
        public IList<DbContext> DbContexts { get; set; }

        public void InicializarDbContexts()
        {
            if (DbContexts == null)
            {
                DbContexts = new List<DbContext>();
            }
            
            // Obtendo todos os possíveis tipos de DbContext informados no ApplicationContext e instanciando um a um.
            if (ApplicationContext.AllPossibleDbContextTypes != null)
            {
                foreach (var dbContextType in ApplicationContext.AllPossibleDbContextTypes)
                {
                    bool jaExisteDbContext = false;
                    foreach (DbContext dbContext in this.DbContexts)
                    {
                        if (dbContext.GetType() == dbContextType)
                        {
                            jaExisteDbContext = true;
                            break;
                        }
                    }

                    if (!jaExisteDbContext)
                    {
                        DbContexts.Add(Activator.CreateInstance(dbContextType) as DbContext);
                    }
                }
            }
        }

        /// <summary>
        /// Obtem um DbContext expecifico do atual contexto.
        /// </summary>
        /// <typeparam name="TDbContextRequested"></typeparam>
        /// <returns></returns>
        public DbContext GetDbContextInstance<TDbContextRequested>() where TDbContextRequested : DbContext
        {
            if (this.DbContexts == null)
            {
                this.DbContexts = new List<DbContext>();
            }

            // Tenta encontrar o DbContext em questão na atual lista de DbContext existente na instância de DatabaseContext.
            foreach (var dbContext in this.DbContexts)
            {
                if (dbContext is TDbContextRequested)
                {
                    return dbContext;
                }
            }

            // Caso não encontre o DbContext, adiciona neste momento.
            var novoDbContext = Activator.CreateInstance<TDbContextRequested>();
            DbContexts.Add(novoDbContext);
            return novoDbContext;
        }
    }
}
