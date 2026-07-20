namespace CohesiveRP.Core.ComfyUI.Client
{
    public interface IComfyUiClient : IDisposable
    {
        Task ConnectAsync(CancellationToken cancellationToken);
        Task<string> SubmitAsync(string workflowApiJson, CancellationToken cancellationToken);
        Task<ComfyUiOutputFile> WaitAsync(string promptId, string nodeSaveImage, CancellationToken cancellationToken);
        Task<byte[]> DownloadAsync(ComfyUiOutputFile file, CancellationToken cancellationToken);
        Task<bool> IsAvailableAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Uploads an image to ComfyUI's input/ directory so it can be referenced by name in workflows.
        /// Returns the filename as confirmed by ComfyUI (may differ from the requested name).
        /// </summary>
        Task<string> UploadImageAsync(byte[] imageBytes, string fileName, CancellationToken cancellationToken);
    }
}
