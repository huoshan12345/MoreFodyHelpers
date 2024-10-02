namespace MoreFodyHelpers.Support;

public interface IWeaverLogger
{
    void Debug(string message);
    void Info(string message);
    void Warning(string message, SequencePoint? sequencePoint);
    void Error(string message, SequencePoint? sequencePoint);
}
