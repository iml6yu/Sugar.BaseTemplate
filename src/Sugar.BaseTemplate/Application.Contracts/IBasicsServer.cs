using Sugar.BaseTemplate.Models;

namespace Light.PhoneNumber.Application.Contracts
{
    public interface IBasicsServer<TEntity, TQueryDto,TListQueryDto, TCreateDto,TUpdateDto>
        where TEntity:BaseModel 
    {
    }
}
