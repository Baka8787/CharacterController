using Project.Core.Blackboard;

namespace Project.Core.Pipeline
{
    public interface IInputSource
    {
        /// <summary>
        /// 負責採樣輸入裝置數據並封裝回傳
        /// </summary>
        InputData Sample();
    }
}
