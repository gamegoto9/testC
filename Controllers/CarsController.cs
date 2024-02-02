using System.Data;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace DotnetAPI.Controllers;

    [ApiController]
    [Route("[controller]")]
    public class CarsController : ControllerBase
    {
        DataContextDapper _dapper;
        private readonly IConfiguration _config;
        ICarRepository _carRepository;
        ICondsRepository _condsRepository;
        
        public CarsController(IConfiguration config, ICarRepository carRepository, ICondsRepository condsRepository) {
            _config = config;
            _carRepository = carRepository;
            _condsRepository = condsRepository;
            _dapper = new DataContextDapper(config);
        }

        [HttpGet("TestConnection")]
        public DateTime TestConnection() {
            return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
        }

        [HttpGet("Conds")]
        public  CondItem Conds() {
           
            // string sql = @"EXEC w60.getMMT";
            // IEnumerable<DataSet> mmt = _dapper.LoadData<DataSet>(sql);
            // return mmt;
            // return _carRepository.CallYourStoredProcedure();
            var ret = _condsRepository.getCond();
            return ret;
        }

        [HttpGet("home")]
        public HomeItem Home() {
           
            // string sql = @"EXEC w60.getMMT";
            // IEnumerable<DataSet> mmt = _dapper.LoadData<DataSet>(sql);
            // return mmt;

            var ret = _condsRepository.getHomeProcude();
            return ret;
            
        }

        [HttpPost("search")]
        public object search(SearchDto search) {
           
            // string sql = @"EXEC w60.getMMT";
            // IEnumerable<DataSet> mmt = _dapper.LoadData<DataSet>(sql);
            // return mmt;

            var ret = _carRepository.search(search);

            return ret;
            
        }

        [HttpPost("CarDet")]
        public object CarDet(int Cid) {
           
            // string sql = @"EXEC w60.getMMT";
            // IEnumerable<DataSet> mmt = _dapper.LoadData<DataSet>(sql);
            // return mmt;
            var carDetailRepository = new CarDetailRepository(_config);
            var ret = carDetailRepository.carDet(Cid);

            return ret;
            
        }

       
    }   

