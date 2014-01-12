using NHibernate;
using Orchard.Data;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace RaisingStudio.SessionFactory.Services
{
    public class CustomSessionLocator : ISessionLocator, ITransactionManager, IDisposable {
        private readonly ISessionFactoryHolder _sessionFactoryHolder;
        private ISession _session;
        private ITransaction _transaction;
        private bool _cancelled;
        private bool _enableTransaction;

        public CustomSessionLocator(
            ISessionFactoryHolder sessionFactoryHolder,
            bool enableTransaction) {
            _sessionFactoryHolder = sessionFactoryHolder;
            _enableTransaction = enableTransaction;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public ISession For(Type entityType) {
            Logger.Debug("Acquiring session for {0}", entityType);

            ((ITransactionManager)this).Demand();

            return _session;
        }

        public void Demand() {
            EnsureSession();

            if (_enableTransaction)
            {
                if (_transaction == null)
                {
                    Logger.Debug("Creating transaction on Demand");
                    _transaction = _session.BeginTransaction(IsolationLevel.ReadCommitted);
                }
            }
        }

        public void RequireNew() {
            RequireNew(IsolationLevel.ReadCommitted);
        }

        public void RequireNew(IsolationLevel level) {
            EnsureSession();

            if (_enableTransaction)
            {
                if (_cancelled)
                {
                    if (_transaction != null)
                    {
                        _transaction.Rollback();
                        _transaction.Dispose();
                        _transaction = null;
                    }

                    _cancelled = false;
                }
                else
                {
                    if (_transaction != null)
                    {
                        _transaction.Commit();
                    }
                }

                Logger.Debug("Creating new transaction with isolation level {0}", level);
                _transaction = _session.BeginTransaction(level);
            }
        }

        public void Cancel() {
            Logger.Debug("Transaction cancelled flag set");
            _cancelled = true;
        }

        public void Dispose() {
            if (_enableTransaction)
            {
                if (_transaction != null)
                {
                    try
                    {
                        if (!_cancelled)
                        {
                            Logger.Debug("Marking transaction as complete");
                            _transaction.Commit();
                        }
                        else
                        {
                            Logger.Debug("Reverting operations from transaction");
                            _transaction.Rollback();
                        }

                        _transaction.Dispose();
                        Logger.Debug("Transaction disposed");
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, "Error while disposing the transaction.");
                    }
                    finally
                    {
                        _transaction = null;
                        _cancelled = false;
                    }
                }
            }
        }

        private void EnsureSession() {
            if (_session != null) {
                return;
            }

            var sessionFactory = _sessionFactoryHolder.GetSessionFactory();
            Logger.Information("Opening database session");
            _session = sessionFactory.OpenSession();
        }
    }
}