using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;

namespace FrameworkAspNetExtended.Context
{
    public class RequestContext
    {
        private readonly static ILog log = LogManager.GetLogger(typeof(RequestContext));

        public List<DbTransaction> Transactions { get; set; }

        public int ControleQtdServicosExecutados { get; set; }

        public void AddTransaction(DbTransaction trans)
        {
            if (Transactions == null)
            {
                this.Transactions = new List<DbTransaction>();
            }
            this.Transactions.Add(trans);
        }

        public void Clear()
        {
            ControleQtdServicosExecutados = 0;
            Transactions.ForEach(transaction => transaction.Dispose());
            Transactions.Clear();
        }

        /// <summary>
        /// Chama o SaveChanges de cada DbContext encontrado no DatabaseContext.
        /// </summary>
        public void SaveChanges()
        {
            IList<DbContext> dbContexts = ApplicationContext.Resolve<DatabaseContext>().DbContexts;
            foreach (var dbContext in dbContexts)
            {
                dbContext.SaveChanges();
            }
        }

        /// <summary>
        /// Realiza o Commit() e Dispose() de todas as transações abertas no DatabaseContext e limpa as transações do RequestContext.
        /// </summary>
        public void CommitTransactions()
        {
            if (Transactions == null)
            {
                return;
            }

            var validTransactions =
                        Transactions.Where(transaction => transaction != null && transaction.Connection != null);

            foreach (DbTransaction transaction in validTransactions)
            {
                transaction.Commit();
                transaction.Dispose();
            }

            Transactions.Clear();
        }

        /// <summary>
        /// Chama o Rollback() e o Dispose() de todas as transações do contexto e limpa a lista de transações.
        /// </summary>
        public void RollbackTransactions()
        {
            if (Transactions == null)
            {
                return;
            }

            var validTransactions =
                        Transactions.Where(transaction => transaction != null && transaction.Connection != null);
            foreach (DbTransaction transaction in validTransactions)
            {
                transaction.Rollback();
                transaction.Dispose();
            }
            Transactions.Clear();
        }

        internal void CloseConections()
        {
            try
            {
                IList<DbContext> dbContexts = ApplicationContext.Resolve<DatabaseContext>().DbContexts;
                if (dbContexts == null)
                    return;

                foreach (var dbContext in dbContexts)
                {
                    try
                    {
                        dbContext.Database.Connection.Close();
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        internal bool HasTransactionByDbConnection(DbConnection connection)
        {
            if (Transactions != null)
            {
                foreach (var trans in Transactions)
                {
                    if (trans.Connection.ConnectionString == connection.ConnectionString)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
