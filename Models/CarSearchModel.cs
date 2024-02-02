
namespace DotnetAPI.Models
{

    public partial class CarSearchModel
    {
        public List<Car> Cars { get; set; }
        public string Title { get; set; }
        public int? CarCnt { get; set; }
        public string Sort { get; set; }
        public List<object> Makes { get; set; }
        public List<Model> Models { get; set; }
        public List<object> Bodies { get; set; }
        public List<object> Trims { get; set; }
        public List<object> Years { get; set; }
    }

    public partial class Car
    {
        public int? CId { get; set; }
        public string Name { get; set; }
        public string ModelName { get; set; }
        public string? Price { get; set; }
        public bool? IsPricePerMonth { get; set; }
        public int? Year { get; set; }
        public string DispTitle { get; set; }
        public string LastUpdate { get; set; }
        public int? PageView { get; set; }
        public string? Img300Url { get; set; }
        public string? Img600Url { get; set; }
        public string Status { get; set; }
    }

    public partial class Model
    {
        public int? Id { get; set; }
        public string Disp { get; set; }
    }
}
