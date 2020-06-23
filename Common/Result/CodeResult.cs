using ShipWeb.Extend;

namespace ShipWeb.Common
{
    /// <summary>
    /// Ajax请求结果状态码
    /// @author  Ahri 2020.6.23
    /// </summary>
    public enum CodeResult
    {
        /// <summary>
        /// 0-成功
        /// </summary>
        [Remark("请求成功")]
        SUCCESS = 0,
        /// <summary>
        /// 400-失败
        /// </summary>
        [Remark("请求失败")]
        FAILED = 400
    }

}
