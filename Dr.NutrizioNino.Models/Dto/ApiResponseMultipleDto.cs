namespace Dr.NutrizioNino.Api.Dto;

[Obsolete("non serve più questa cacata", true)]
public class ApiResponseMultipleDto<T>
{
    public ApiResponseMultipleDto()
    {
        Data = [];
        Errors = [];
    }
    public bool Success { get; set; }
    public IList<T>? Data { get; set; }
    public IList<string> Errors { get; set; }

}
