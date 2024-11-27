namespace UserAuthService.Models.ResponseModel
{
    public interface IResponseModel<T>
    {
        T? Data { get; set; }
        string? Message { get; set; }
        bool Error { get; set; }
    }
}
