using Project.Core.Blackboard;

namespace Project.Core.Pipeline
{
    public interface IInputSource
    {
        /// <summary>
        /// 負責採樣輸入裝置數據並封裝回傳（目前維持 v0.1 簽名）
        /// </summary>
        InputData Sample();
    }
}