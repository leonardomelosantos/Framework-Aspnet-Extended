using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Util;

namespace FrameworkAspNetExtended.Context
{
    public class RequestContext
    {
        private readonly static ILog log = LogManager.GetLogger(typeof(RequestContext));

        public List<DbTransaction> Transactions { get; set; }

        public int ControleQtdServicosExecutados { get; set; }

        /*public bool TransactionStart
        {
            get
            {
                return Transaction != null;
            }
        }

        public void Clean()
        {
            Transaction = null;
        }*/

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
            //base.Clear();
            ControleQtdServicosExecutados = 0;
            //NomeServicoWcf = null;
            //Exception = null;
            //userTicket = null;
            //ipAddressContext = null;
            //cultureInfoContext = null;
            Transactions.ForEach(transaction => transaction.Dispose());
            Transactions.Clear();
            //ConnectionsNamesWithTransaction.Clear();
            //ConnectionNames.Clear();
            //IsolationLevel = null;
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
            //IsolationLevel = null;
            //ConnectionsNamesWithTransaction.Clear();
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
            //IsolationLevel = null;
            //ConnectionsNamesWithTransaction.Clear();
        }

        internal void CloseConections()
        {
			try
			{
				IList<DbContext> dbContexts = ApplicationContext.Resolve<DatabaseContext>().DbContexts;
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
