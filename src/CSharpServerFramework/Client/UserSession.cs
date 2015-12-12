using CSharpServerFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpServerFramework.Client
{
    /// <summary>
    /// 服务器用户信息基础类，根据业务不同继承此类
    /// </summary>
    public class UserSession : ICSharpServerSession
    {
        /// <summary>
        ///CSServer管理使用的Client
        /// </summary>
        internal CSServerClientBase Client { get; set; }

        /// <summary>
        /// UserSession构造，ID号生成由业务决定,-1表示未登录用户
        /// </summary>
        /// <param name="UserId">用户在业务中唯一的ID号，ClientManager会根据该UserId使用简单容器存储UserSession实例，并
        /// 提供接口GetUserByUserId通过该唯一ID号返回对应的UserSession实例或子类的实例，-1规定表示未进行用户验证的用户</param>
        public UserSession()
        {

        }

        public ICSharpServerUser User { get; private set; }

        /// <summary>
        /// 工作线程ID
        /// </summary>
        internal int WorkThreadId { get; set; }

        /// <summary>
        /// 该用户是未认证用户
        /// </summary>
        internal bool IsSessionNotValidate { get { return User == null || User.IsUserValidate == false; } }

        public bool RegistUser(ICSharpServerUser User)
        {
            this.User = User;
            this.User.Session = this;
            return true;
        }
    }
}
