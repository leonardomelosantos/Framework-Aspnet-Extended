using FrameworkAspNetExtended.Entities;
using FrameworkAspNetExtended.Entities.Events;
using FrameworkAspNetExtended.Entities.Exceptions;
using System;

namespace FrameworkAspNetExtended.Core
{
    public interface IApplicationManagerEvents
    {
        void Exception(Exception ex);

        void ConcurrencyException(Exception ex);

        void BusinessException(BusinessException exception);

        void PermissionException(PermissaoException exception);

        void BeforeServiceMethodExecute(ServiceMethodEventInfo info);

        void AfterServiceMethodExecuted(ServiceMethodEventInfo info);

        void BeforeRepositoryMethodExecute(RepositoryMethodEventInfo info);

        void AfterRepositoryMethodExecuted(RepositoryMethodEventInfo info);
    }
}
