using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Orchard.Logging;
using Orchard.Utility.Extensions;
using Orchard.Environment.Configuration;
using Orchard.Environment.ShellBuilders.Models;
using Orchard.FileSystems.AppData;
using System.IO;
using Orchard.Data;


namespace RaisingStudio.Data.RepositoryFactory.Services
{
    public class DataRepository<T> : IDataRepository<T> where T : class, new()
    {
        private readonly ISessionLocator _sessionLocator;
        private readonly ShellSettings _shellSettings;
        private readonly ShellBlueprint _shellBlueprint;
        private readonly IAppDataFolder _appDataFolder;
        private readonly IJsonDataRepositoryFactoryHolder _jsonDataRepositoryFactoryHolder;

        public DataRepository(ISessionLocator sessionLocator,
            ShellSettings shellSettings,
            ShellBlueprint shellBlueprint,
            IAppDataFolder appDataFolder,
            IJsonDataRepositoryFactoryHolder jsonDataRepositoryFactoryHolder)
        {
            _sessionLocator = sessionLocator;
            _shellSettings = shellSettings;
            _shellBlueprint = shellBlueprint;
            _appDataFolder = appDataFolder;
            _jsonDataRepositoryFactoryHolder = jsonDataRepositoryFactoryHolder;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        protected virtual ISessionLocator SessionLocator
        {
            get { return _sessionLocator; }
        }

        private DataContext _dataContext;
        protected virtual DataContext DataContext
        {
            get
            {
                //return SessionLocator.For(typeof(T));
                if (this._dataContext == null)
                {
                    lock (this)
                    {
                        if (this._dataContext == null)
                        {

                            var connection = SessionLocator.For(typeof(T)).Connection;
                            this._dataContext = new DataContext(connection);
                        }
                    }
                }
                return this._dataContext;
            }
        }

        public virtual IQueryable<T> Table
        {
            get
            {
                //return Session.Query<T>().Cacheable();
                return DataContext.GetQuery<T>();
            }
        }


        private IEnumerable<Action<T, T>> _propertyTransfers;
        public IEnumerable<Action<T, T>> GetPropertyTransfers()
        {
            if (_propertyTransfers == null)
            {
                lock (this)
                {
                    if (_propertyTransfers == null)
                    {
                        _propertyTransfers = _jsonDataRepositoryFactoryHolder.GetRepositoryFactory().GetPropertyTransfers<T>();
                    }
                }
            }
            return _propertyTransfers;
        }

        #region IRepository<T> Members

        void IRepository<T>.Create(T entity)
        {
            Create(entity);
        }

        void IRepository<T>.Update(T entity)
        {
            Update(entity);
        }

        void IRepository<T>.Delete(T entity)
        {
            Delete(entity);
        }

        void IRepository<T>.Copy(T source, T target)
        {
            Copy(source, target);
        }

        void IRepository<T>.Flush()
        {
            Flush();
        }

        T IRepository<T>.Get(int id)
        {
            return Get(id);
        }

        T IRepository<T>.Get(Expression<Func<T, bool>> predicate)
        {
            return Get(predicate);
        }

        IQueryable<T> IRepository<T>.Table
        {
            get { return Table; }
        }

        int IRepository<T>.Count(Expression<Func<T, bool>> predicate)
        {
            return Count(predicate);
        }

        IEnumerable<T> IRepository<T>.Fetch(Expression<Func<T, bool>> predicate)
        {
            return Fetch(predicate).ToReadOnlyCollection();
        }

        IEnumerable<T> IRepository<T>.Fetch(Expression<Func<T, bool>> predicate, Action<Orderable<T>> order)
        {
            return Fetch(predicate, order).ToReadOnlyCollection();
        }

        IEnumerable<T> IRepository<T>.Fetch(Expression<Func<T, bool>> predicate, Action<Orderable<T>> order, int skip,
                                            int count)
        {
            return Fetch(predicate, order, skip, count).ToReadOnlyCollection();
        }

        #endregion

        public virtual T Get(int id)
        {
            //return Session.Get<T>(id);
            // TODO:
            return DataContext.GetEntity<T>(id);
        }

        public virtual T Get(Expression<Func<T, bool>> predicate)
        {
            return Fetch(predicate).SingleOrDefault();
        }

        public virtual void Create(T entity)
        {
            Logger.Debug("Create {0}", entity);
            //Session.Save(entity);
            // TODO:
            DataContext.Insert<T>(entity);
        }

        public virtual void Update(T entity)
        {
            Logger.Debug("Update {0}", entity);
            //Session.Evict(entity);
            //Session.Merge(entity);
            // TODO:
            DataContext.Update<T>(entity);
        }

        public virtual void Delete(T entity)
        {
            Logger.Debug("Delete {0}", entity);
            //Session.Delete(entity);
            // TODO:
            DataContext.Delete<T>(entity);
        }

        public virtual void Copy(T source, T target)
        {
            Logger.Debug("Copy {0} {1}", source, target);
            //var metadata = Session.SessionFactory.GetClassMetadata(typeof(T));
            //var values = metadata.GetPropertyValues(source, EntityMode.Poco);
            //metadata.SetPropertyValues(target, values, EntityMode.Poco);
            // TODO:
            IEnumerable<Action<T, T>> propertyTransferList = GetPropertyTransfers();
            foreach (var propertyTransfer in propertyTransferList)
            {
                propertyTransfer(source, target);
            }
        }

        public virtual void Flush()
        {
            //Session.Flush();
            // TODO:
        }

        public virtual int Count(Expression<Func<T, bool>> predicate)
        {
            return Fetch(predicate).Count();
        }

        public virtual IQueryable<T> Fetch(Expression<Func<T, bool>> predicate)
        {
            return Table.Where(predicate);
        }

        public virtual IQueryable<T> Fetch(Expression<Func<T, bool>> predicate, Action<Orderable<T>> order)
        {
            var orderable = new Orderable<T>(Fetch(predicate));
            order(orderable);
            return orderable.Queryable;
        }

        public virtual IQueryable<T> Fetch(Expression<Func<T, bool>> predicate, Action<Orderable<T>> order, int skip,
                                           int count)
        {
            return Fetch(predicate, order).Skip(skip).Take(count);
        }
    }
}