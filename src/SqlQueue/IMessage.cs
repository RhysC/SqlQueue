namespace SqlQueue
{
    public interface IMessage
    {
        T GetPayload<T>() where T : class;
        void MarkAsFailed(string theReasonForFailure);
        void MarkAsRead();
    }

    public interface IFailedMessage
    {
        object GetPayload();
        string FailureMessage { get; }
    }
}