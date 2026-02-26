namespace Dr.NutrizioNino.Api.Dto;

[Obsolete("non serve più questa cacata", true)]

public class ApiResponseSingleDto<T>
{
    public ApiResponseSingleDto()
    {
        Errors = [];
    }
    public bool Success { get; set; }
    public T? Data { get; set; }
    public IList<string> Errors { get; set; }

}
