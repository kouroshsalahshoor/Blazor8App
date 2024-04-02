namespace Shared.Dtos
{
    public class ResponseDto
    {
        public bool IsSuccessful { get; set; } = false;
        public List<string> Errors { get; set; } = new();
    }
}
