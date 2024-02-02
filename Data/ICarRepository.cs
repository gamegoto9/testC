using System.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;

namespace DotnetAPI.Data
{
    public interface ICarRepository
    {
      public object test();
       public object search(SearchDto iParam);
    }
}
