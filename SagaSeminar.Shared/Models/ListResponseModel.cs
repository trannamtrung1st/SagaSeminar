namespace SagaSeminar.Shared.Models
{
    public class ListResponseModel<T>
    {
        public IEnumerable<T> List { get; set; }
        public int Total { get; set; }
    }
}
