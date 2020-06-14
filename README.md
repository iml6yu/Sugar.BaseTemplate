# Sugar.BaseTemplate
 curd 基础操作
e.g.

使用样例
```csharp
public class BaseCustomerServer : BasicsServer<BaseCustomer, QueryBaseCustomerDto, QueryBaseCustomerDto, CreateBaseCustomerDto, UpdateBaseCustomerDto>, IBaseCustomerServer
    {
        public BaseCustomerServer(PhoneNumberDbContext context, ILogger logger) : base(context, logger)
        {
        }

        public DataResult<QueryBaseCustomerDto> Gets(int pageIndex, int pageSize, string key)
        {
            return null;
        } 
    }
```
