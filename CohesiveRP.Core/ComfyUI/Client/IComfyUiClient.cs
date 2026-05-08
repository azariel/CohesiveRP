namespace CohesiveRP.Core.ComfyUI.Client
{
    public interface IComfyUiClient : IDisposable
    {
        Task ConnectAsync(CancellationToken cancellationToken);
        Task<string> SubmitAsync(string workflowApiJson, CancellationToken cancellationToken);
        Task<ComfyUiOutputFile> WaitAsync(string promptId, CancellationToken cancellationToken);
        Task<byte[]> DownloadAsync(ComfyUiOutputFile file, CancellationToken cancellationToken);
    }
}
