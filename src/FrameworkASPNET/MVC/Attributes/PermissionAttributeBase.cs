using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FrameworkAspNetExtended.MVC.Attributes
{
    public class PermissionAttributeBase : Attribute
    {
        #region Properties

        public bool IgnoreControllerPermissions { get; set; }

        private string[] _requiredStringsPermissions;

        public string[] RequiredStringsPermissions
        {
            get { return _requiredStringsPermissions; }
            set
            {
                if (value == null)
                {
                    _requiredStringsPermissions = new string[0];
                }
                else
                {
                    _requiredStringsPermissions = value;
                }
            }
        }

        #endregion

        protected static List<String> GetRequiredPermissionsFor<T>(MethodInfo methodInfo)
             where T : PermissionAttributeBase
        {
            return GetStringListRequiredPermissionsFor(GetAllRequiredPermissionsFor<T>(methodInfo));
        }

        private static List<String> GetStringListRequiredPermissionsFor<T>(
            IEnumerable<T> permissionsRequiredAttributes)
             where T : PermissionAttributeBase
        {
            return permissionsRequiredAttributes.SelectMany(perm => perm.RequiredStringsPermissions).Distinct().ToList();
        }

        /// <summary>
        /// Junta as operações requeridas do método e da classe.
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        private static List<T> GetAllRequiredPermissionsFor<T>(MethodInfo methodInfo) where T : PermissionAttributeBase
        {
            List<object> permissionsInMethod = new List<object>();
            permissionsInMethod.AddRange(methodInfo.GetCustomAttributes(typeof(T), true));
            List<T> permissionAttributesInMethod = new List<T>();
            if (permissionsInMethod.Any())
            {
                permissionAttributesInMethod.AddRange(permissionsInMethod.Cast<T>());
            }

            List<object> permissionsInType = new List<object>();
            if (methodInfo.DeclaringType != null)
            {
                permissionsInType.AddRange(methodInfo.DeclaringType.GetCustomAttributes(typeof(T), true));
            }
            List<T> permissionAttributesInType = new List<T>();
            if (permissionsInType.Any())
            {
                permissionAttributesInType.AddRange(permissionsInType.Cast<T>());
            }

            List<T> permissionAttributes = new List<T>();
            if (permissionAttributesInMethod.Any(perm => perm.IgnoreControllerPermissions))
            {
                permissionAttributes.AddRange(permissionAttributesInMethod);
            }
            else
            {
                if (permissionAttributesInMethod.Any())
                {
                    permissionAttributes.AddRange(permissionAttributesInMethod);
                }

                if (permissionAttributesInType.Any())
                {
                    permissionAttributes.AddRange(permissionAttributesInType);
                }
            }
            return permissionAttributes.Distinct().ToList();

        }

        public bool Equals(PermissionAttributeBase other)
        {
            bool equals = false;
            if (other != null)
            {
                if (this.RequiredStringsPermissions == null && other.RequiredStringsPermissions == null)
                {
                    equals = true;
                }
                else if (this.RequiredStringsPermissions != null && other.RequiredStringsPermissions != null)
                {
                    equals = !this.RequiredStringsPermissions.Except(other.RequiredStringsPermissions).Any();
                }
            }
            return equals && IgnoreControllerPermissions == other.IgnoreControllerPermissions;
        }
    }
}
