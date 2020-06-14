using Light.PhoneNumber.Application.Contracts; 
using Microsoft.Extensions.Logging;
using Sugar.BaseTemplate.Dtos;
using Sugar.BaseTemplate.Models;
using Sugar.Utils.Extension;
using Sugar.Utils.Result;
using System; 
using System.Linq;
using System.Linq.Expressions;
#if NET47
using System.Data.Entity;
#elif NET48
using System.Data.Entity;
#else
using Microsoft.EntityFrameworkCore;
#endif

namespace Sugar.BaseTemplate.Application.Contracts
{
    public abstract class BasicsServer<TEntity, TQueryDto, TListQueryDto, TCreateDto, TUpdateDto> : IBasicsServer<TEntity, TQueryDto, TListQueryDto, TCreateDto, TUpdateDto>
        where TEntity : BaseModel, new()
        where TQueryDto : QueryDto, new()
        where TListQueryDto : QueryDto
        where TCreateDto : CreateDto
        where TUpdateDto : UpdateDto
    {
        protected BasicsServer()
        {
        }

        protected DbContext Context;
        protected ILogger Logger;
        public BasicsServer(DbContext context, ILogger logger)
        {
            this.Logger = logger;
            Context = context;
        }
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual SingleObjectResult<TQueryDto> Get(string id)
        {
            try
            {
                Guid uid;
                if (!Guid.TryParse(id, out uid))
                    return SingleObjectResult<TQueryDto>.Failed(MessageType.ParameterError);
                var entity = Find(uid);
                if (entity == null)
                    return SingleObjectResult<TQueryDto>.Failed(MessageType.NotFind);
                return SingleObjectResult<TQueryDto>.Success(entity.To<TQueryDto>());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                return SingleObjectResult<TQueryDto>.Failed(MessageType.UnKonwServerError, ex.Message);
            }
        }
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="expression">规则</param>
        /// <returns></returns>
        public virtual SingleObjectResult<TQueryDto> Get(Expression<Func<TEntity, bool>> expression)
        {
            try
            {
                var entity = GetDbSet(typeof(TEntity)).Where(t => !t.IsDeleted).FirstOrDefaultAsync(expression);
                if (entity == null)
                    return SingleObjectResult<TQueryDto>.Failed(MessageType.NotFind);
                return SingleObjectResult<TQueryDto>.Success(entity.To<TQueryDto>());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                return SingleObjectResult<TQueryDto>.Failed(MessageType.UnKonwServerError, ex.Message);
            }

        }
        /// <summary>
        /// 无规则 分页查询
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual DataResult<TQueryDto> Gets(int pageIndex, int pageSize)
        {
            try
            {
                var ds = GetDbSet(typeof(TEntity)).Where(t => !t.IsDeleted);
                var total = ds.Count();
                var data = ds.Skip(pageIndex * pageSize).Take(pageSize);
                return DataResult<TQueryDto>.Success(pageIndex, total, data.Select(t => t.To<TQueryDto>()));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                return DataResult<TQueryDto>.Failed(MessageType.UnKonwServerError, ex.Message);
            }
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual DataResult<TQueryDto> Gets(int pageIndex, int pageSize, Expression<Func<TEntity, bool>> expression)
        {
            try
            {
                var ds = GetDbSet(typeof(TEntity)).Where(t => !t.IsDeleted).Where(expression);
                var total = ds.Count();
                var data = ds.Skip(pageIndex * pageSize).Take(pageSize);
                return DataResult<TQueryDto>.Success(pageIndex, total, data.Select(t => t.To<TQueryDto>()));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                return DataResult<TQueryDto>.Failed(MessageType.UnKonwServerError, ex.Message);
            }
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="createDto"></param>
        /// <returns></returns>
        public virtual MessageResult Add(TCreateDto createDto)
        {
            try
            {
                var entity = createDto.To<TEntity>();
                GetDbSet(typeof(TEntity)).Add(entity);
                if (Context.SaveChanges() > 0)
                    return MessageResult.Success();
                return MessageResult.UnKown();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                return DataResult<TQueryDto>.Failed(MessageType.UnKonwServerError, ex.Message);
            }
        }

        /// <summary>
        /// 删除 更新标识 不是真正的delete Data
        /// </summary>
        /// <param name="id"></param>
        /// <param name="deleterId"></param>
        /// <returns></returns>
        public virtual MessageResult Delete(string id, Guid deleterId)
        {
            try
            {
                Guid uid;
                if (!Guid.TryParse(id, out uid))
                    return MessageResult.Failed(MessageType.ParameterError);
                var entity = Find(uid);
                if (entity == null)
                    return MessageResult.Failed(MessageType.NotFind);
                entity.IsDeleted = true;
                entity.DeletedTime = DateTime.Now;
                entity.DeleterId = deleterId;
                if (Context.SaveChanges() > 0)
                    return MessageResult.Success();
                return MessageResult.Failed(MessageType.UnKonwServerError);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                return MessageResult.Failed(MessageType.UnKonwServerError, ex.Message);
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="updateDto"></param>
        /// <param name="modifierId"></param>
        /// <returns></returns>
        public virtual MessageResult Modify(TUpdateDto updateDto, Guid modifierId)
        {
            try
            {
                var entity = Find(updateDto.Id);
                if (entity == null)
                    return MessageResult.Failed(MessageType.NotFind);
                entity = updateDto.AssignTo(entity);
                entity.LaseModifyTime = DateTime.Now;
                entity.ModifierId = modifierId;
                if (Context.SaveChanges() > 0)
                    return MessageResult.Success();
                return MessageResult.Failed(MessageType.UnKonwServerError);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                return MessageResult.Failed(MessageType.UnKonwServerError, ex.Message);
            }

        }

        protected string GetSalt(TEntity entity)
        {
            return entity.CreateTime.ToString("yyyyMMddHHmm");
        }

        protected DbSet<TEntity> GetDbSet(TEntity entity)
        {
            return GetDbSet(entity.GetType());
        }

        protected DbSet<TEntity> GetDbSet(Type entityType)
        {
            var contextType = Context.GetType();
            var dsProperty = contextType.GetProperties().ToList().FirstOrDefault(t => t.PropertyType.IsGenericType && t.PropertyType.GenericTypeArguments[0].Name == entityType.Name);
            return dsProperty.GetValue(Context, null) as DbSet<TEntity>;
        }

        protected DbSet<T> GetDbSet<T>() where T : BaseModel, new()
        {
            var entityType = typeof(T);
            var contextType = Context.GetType();
            var dsProperty = contextType.GetProperties().ToList().FirstOrDefault(t => t.PropertyType.IsGenericType && t.PropertyType.GenericTypeArguments[0].Name == entityType.Name);
            return dsProperty.GetValue(Context, null) as DbSet<T>;
        }

        protected TEntity Find(Guid id)
        {
            return Find<TEntity>(id);
        }

        protected T Find<T>(Guid id) where T : BaseModel, new()
        {
            return GetDbSet<T>().FirstOrDefault(t => !t.IsDeleted && t.Id == id);
        }

        protected TEntity Find(Expression<Func<TEntity, bool>> expression)
        {
            return Find<TEntity>(expression);
        }

        protected T Find<T>(Expression<Func<T, bool>> expression) where T : BaseModel, new()
        {
            return GetDbSet<T>().Where(expression).FirstOrDefault(t => !t.IsDeleted);
        }
    }
}
