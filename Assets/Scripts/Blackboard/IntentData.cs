namespace Project.Core.Blackboard
{
    /// <summary>
    /// 意圖區：記錄這一瞬間「想」做什麼。
    /// 使用結構體 (struct) 配合整體覆寫，達到復位時零 GC。
    /// </summary>
    public struct IntentData
    {
        public bool JumpRequested;
        public bool RollRequested;
        public bool FireRequested;

        /// <summary>
        /// 每幀結尾呼叫，將所有單幀意圖復位
        /// </summary>
        public void Reset()
        {
            JumpRequested = false;
            RollRequested = false;
            FireRequested = false;
        }
    }
}
